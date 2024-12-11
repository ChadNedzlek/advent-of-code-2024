using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem11 : SyncProblemBase
    {
        public Problem11(string executionMode) : base(executionMode)
        {
        }

        protected override void ExecuteCore(string[] data)
        {
            Dictionary<(ulong value, int fromEnd), ulong> steps = [];
            var stones = data[0].Split().Select(ulong.Parse).ToList();
            Console.WriteLine($"25 count {stones.Sum(s => GetStepsFromEnd(s, 25))}");
            Console.WriteLine($"75 count {stones.Sum(s => GetStepsFromEnd(s, 75))}");
            ulong GetStepsFromEnd(ulong v, int s)
            {
                if (s == 0)
                    return 1;

                if (steps.TryGetValue((v, s), out ulong value))
                    return value;

                steps.Add((v,s), value = Calculate());
                return value;
                
                ulong Calculate(){
                    if (v == 0)
                        return GetStepsFromEnd(1, s - 1);

                    string str = v.ToString();

                    if (str.Length % 2 == 0)
                    {
                        return GetStepsFromEnd(ulong.Parse(str[..(str.Length / 2)]), s - 1) +
                            GetStepsFromEnd(ulong.Parse(str[(str.Length / 2)..]), s - 1);
                    }

                    return GetStepsFromEnd(v * 2024, s - 1);
                }
            }
        }
    }

    public struct StoneState(int Value)
    {
    }
}