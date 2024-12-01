// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Cookie = System.Net.Cookie;

namespace ChadNedzlek.AdventOfCode.DataModule
{
    public class AocData
    {
        private readonly int _year;
        
        private static readonly string RootFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ChadNedzlek",
            "AocData");

        public AocData(int year)
        {
            _year = year;
        }

        public async Task<string[]> GetDataAsync(int day)
        {
            string dir = Path.Combine(RootFolder, _year.ToString());
            Directory.CreateDirectory(dir);
            string targetPath = Path.Combine(dir, $"day-{day:D2}-input.txt");
            if (!File.Exists(targetPath))
            {
                string token = await GetAccessTokenAsync();
                var cookies = new CookieContainer();
                using var http = new HttpClient(new HttpClientHandler { CookieContainer = cookies });
                if (token != null)
                {
                    cookies.Add(new Cookie("session", token, null, "adventofcode.com"){Secure = true});
                }

                await using Stream httpStream =
                    await http.GetStreamAsync($"https://adventofcode.com/{_year}/day/{day}/input");
                await using FileStream cacheStream = File.Create(targetPath);
                await httpStream.CopyToAsync(cacheStream);
            }

            return File.ReadAllLines(targetPath);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            string metadataPath = Path.Combine(RootFolder, _year.ToString(), "metadata.json");
            Metadata metadata = new Metadata();
            if (File.Exists(metadataPath))
            {
                try
                {
                    await using FileStream stream = File.OpenRead(metadataPath);
                    metadata = await JsonSerializer.DeserializeAsync<Metadata>(stream);
                }
                catch
                {
                    // Whatever, we'll make new data
                }
            }

            string returnValue;
            if (!string.IsNullOrEmpty(metadata.AccessToken))
            {
                if (OperatingSystem.IsWindows())
                {
                    returnValue = Encoding.UTF8.GetString(ProtectedData.Unprotect(
                        Convert.FromBase64String(metadata.AccessToken), null, DataProtectionScope.CurrentUser));
                }
                else
                {
                    returnValue = metadata.AccessToken;
                }
            }
            else
            {
                async Task<string> TryGetCookie(IPage p)
                {
                    IReadOnlyList<BrowserContextCookiesResult> all = await p.Context.CookiesAsync();
                    return all.FirstOrDefault(c => c.Name == "session")?.Value;
                }

                Program.Main(new[] { "install", "chromium" });
                IPlaywright pl = await Playwright.CreateAsync();
                IBrowser ch = await pl.Chromium.LaunchAsync(new BrowserTypeLaunchOptions{Headless = false});
                IPage page = await ch.NewPageAsync();
                TaskCompletionSource<bool> closed = new TaskCompletionSource<bool>();
                Task<bool> cTask = closed.Task;
                page.Close += (_, _) => closed.TrySetResult(true);
                await page.GotoAsync($"https://adventofcode.com/{_year}/auth/login");
                string cookie = await TryGetCookie(page);
                while (cookie == null)
                {
                    Task completed = await Task.WhenAny(
                        cTask,
                        page.WaitForURLAsync(u => u.StartsWith("https://adventofcode.com/"), new() { Timeout = 0 })
                    );
                    
                    if (completed == cTask)
                        return null;
                    
                    cookie = await TryGetCookie(page);
                }

                returnValue = cookie;
                if (OperatingSystem.IsWindows())
                {
                    metadata.AccessToken = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(cookie),
                        null, DataProtectionScope.CurrentUser));
                }
                else
                {
                    metadata.AccessToken = cookie;
                }
                
                try
                {
                    await using FileStream stream = File.Create(metadataPath);
                    await JsonSerializer.SerializeAsync(stream, metadata);
                }
                catch
                {
                    // Whatever, we'll make new data
                }
            }

            return returnValue;
        }

        private class Metadata
        {
            public string AccessToken { get; set; }
        }
    }
}