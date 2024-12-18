using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;
using SkiaSharp;
using Spectre.Console;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem14 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        bool isReal = Program.ExecutionMode == ExecutionMode.Normal;
        Point2<int> end = isReal ? (101, 103) : (11, 7);
        Rect2<int> bounds = new Rect2<int>((0, 0), end);
        ImmutableList<Bot> init = ImmutableList.CreateRange(
            data.As<int, int, int, int>(@"p=(-?\d+),(-?\d+) v=(-?\d+),(-?\d+)").Select(x => new Bot((x.Item1, x.Item2), (x.Item3, x.Item4)))
        );
        IEnumerable<Bot> bots = init;

        var bmp = new SKBitmap(new SKImageInfo(bounds.Width, bounds.Height, SKColorType.Gray8));
        Directory.CreateDirectory("images");
        for (int i = 0; i < 100; i++)
        {
            bots = bots.Select(b => b.Move(bounds));
            
            bmp.Erase(SKColor.Empty);
            foreach (var b in bots)
            {
                bmp.SetPixel(b.Position.X, b.Position.Y, new SKColor(255, 255, 255));
            }

            using var s = File.Create($"images\\img{i:D4}.png");
            SKImage.FromBitmap(bmp).Encode().SaveTo(s);
        }

        Span<int> quad = stackalloc int[4];
        foreach (var bot in bots)
        {
            if (bot.Position.X == end.X / 2) continue;
            if (bot.Position.Y == end.Y / 2) continue;
            int q = bot.Position.X / (end.X / 2 + 1) + 2 * (bot.Position.Y / (end.Y / 2 + 1));
            quad[q]++;
        }
        
        Console.WriteLine($"Quads {string.Join(',', quad.ToArray())}, {quad.ToArray().Product()}");
        
        AnsiConsole.WriteLine("Generating a images for you, give me a moment...");
        for (int i = 100; i < 500; i++)
        {
            bots = bots.Select(b => b.Move(bounds));
            bmp.Erase(SKColor.Empty);
            foreach (var b in bots)
            {
                bmp.SetPixel(b.Position.X, b.Position.Y, new SKColor(255, 255, 255));
            }

            using var s = File.Create($"images\\img{i:D4}.png");
            SKImage.FromBitmap(bmp).Encode().SaveTo(s);
        }

        AnsiConsole.WriteLine("I've opened a directory for you, go to it, for for things that look similar...");
        Process.Start(new ProcessStartInfo("images\\") { UseShellExecute = true, Verb = "open" });
        var p1 = new TextPrompt<int>("Give me index of the first one: ").Show(AnsiConsole.Console) + 1;
        var p2 = new TextPrompt<int>("And another: ").Show(AnsiConsole.Console) + 1;
        bots = init;
        for (int j = 0; j < p1; j++)
        {
            bots = bots.Select(b => b.Move(bounds));
        }
        
        Directory.Delete("images", true);
        Directory.CreateDirectory("images");
        for (int i = p1; i < bounds.Width * bounds.Height; i += p2 - p1)
        {
            bmp.Erase(SKColor.Empty);
            foreach (var b in bots)
            {
                bmp.SetPixel(b.Position.X, b.Position.Y, new SKColor(255, 255, 255));
            }

            using var s = File.Create($"images\\img{i:D6}.png");
            SKImage.FromBitmap(bmp).Encode().SaveTo(s);
            for (int j = 0; j < p2 - p1; j++)
            {
                bots = bots.Select(b => b.Move(bounds));
            }
        }
    }

    public readonly record struct Bot(Point2<int> Position, Point2<int> Velocity)
    {
        public Bot Move(Rect2<int> bounds) => this with { Position = (Position + Velocity) % bounds };
    }
}