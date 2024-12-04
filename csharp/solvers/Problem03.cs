using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem03 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(string[] data)
        {
            int sum = 0;
            int switchSum = 0;
            bool run = true;
            foreach (var line in data)
            {
                foreach(Match m in Regex.Matches(line, @"mul\((\d{1,3}),(\d{1,3})\)|do\(\)|don't\(\)"))
                {
                    switch (m.Groups[0].Value)
                    {
                        case "do()":
                            run = true;
                            break;
                        case "don't()":
                            run = false;
                            break;
                        default:
                            int v = int.Parse(m.Groups[1].Value) * int.Parse(m.Groups[2].Value);
                            sum += v;
                            if (run) switchSum += v;
                            break;
                    }
                }
            }
            Console.WriteLine($"Prod Sum: {sum}");
            Console.WriteLine($"Conditional Prod Sum: {switchSum}");
        }
    }
}