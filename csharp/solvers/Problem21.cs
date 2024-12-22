using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem21 : SyncProblemBase
{
    private static readonly Dictionary<char, GPoint2<int>> s_moves = new (){
        ['A'] = (0, 0),
        ['v'] = (1, 0),
        ['^'] = (-1, 0),
        ['<'] = (0, -1),
        ['>'] = (0, 1),
    };

    private static readonly Dictionary<GPoint2<int>, char> s_rev = s_moves.ToDictionary(s => s.Value, s => s.Key);
    
    protected override void ExecuteCore(string[] data)
    {
        char[,] primaryKeypad =
        {
            {'7','8','9'},
            {'4','5','6'},
            {'1','2','3'},
            {'\0','0','A'},
        };
        
        char[,] secondaryKeypad =
        {
            {'\0','^','A'},
            {'<','v','>'},
        };

        Dictionary<(char from, char to), ImmutableList<string>> primaryPaths  = FillPaths(primaryKeypad);
        Dictionary<(char from, char to), ImmutableList<string>> secondaryPaths  = FillPaths(secondaryKeypad);
        Dictionary<long, Dictionary<(char from, char to), long>> levelCosts = [];

        long iterations = 25;
        for (int i = 0; i < iterations; i++)
        {
            Dictionary<(char from, char to), long> distances = [];
            if (i == 0)
            {
                foreach (((char from, char to) key, ImmutableList<string> paths) in secondaryPaths)
                {
                    distances.Add(key, paths.Min(p => p.Length));
                }
            }
            else
            {
                var prev = levelCosts[i - 1];
                foreach (((char from, char to) key, ImmutableList<string> paths) in secondaryPaths)
                {
                    long best = long.MaxValue;
                    foreach (var path in paths)
                    {
                        best = long.Min(best, ("A" + path).Zip(path).Sum(d => prev[(d.First, d.Second)]));
                    }
                    distances.Add(key, best);
                }
            }
            levelCosts.Add(i, distances);
        }

        Dictionary<(char from, char to), long> oneIndirectionCost = [];
        {
            var prev = levelCosts[iterations - 1];
            foreach (((char from, char to) key, ImmutableList<string> paths) in primaryPaths)
            {
                long best = long.MaxValue;
                foreach (var path in paths)
                {
                    best = long.Min(best, ("A" + path).Zip(path).Sum(d => prev[(d.First, d.Second)]));
                }

                oneIndirectionCost.Add(key, best);
            }
        }

        long total = 0;
        foreach (var line in data.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var cost = ("A" + line).Zip(line).Sum(d => oneIndirectionCost[(d.First, d.Second)]);
            var value = long.Parse(line[..^1]);
            var part = cost * value;
            total += part;
            Helpers.VerboseLine($"Line {line} has value {cost} * {value} = {part}");
        }
        Console.WriteLine($"Total: {total}");

        Dictionary<(char from, char to),ImmutableList<string>> FillPaths(char[,] keypad)
        {
            Dictionary<(char from, char to), ImmutableList<string>> paths = [];
            foreach (var a in keypad.AsEnumerableWithPoint())
            {
                if (a.value == '\0') continue;
                foreach (var b in keypad.AsEnumerableWithPoint())
                {
                    if (b.value == '\0') continue;
                    paths.Add((a.value, b.value), GetPaths(a.point, b.point, keypad));
                }
            }

            return paths;

            ImmutableList<string> GetPaths(GPoint2<int> a, GPoint2<int> b, char[,] map)
            {
                if (a == b) return ["A"];
                var p = ImmutableList.CreateBuilder<string>();
                var distance = (b - a).OrthogonalDistance;
                for (var i = 0; i < Helpers.OrthogonalDirections.Length; i++)
                {
                    GPoint2<int> dir = Helpers.OrthogonalDirections[i];
                    GPoint2<int> nextPoint = a + dir;
                    if ((nextPoint - b).OrthogonalDistance >= distance) continue;
                    if (map.Get(nextPoint) == '\0') continue;

                    char d = s_rev[dir];
                    p.AddRange(GetPaths(nextPoint, b, map).Select(c => d + c));
                }

                return p.ToImmutable();
            }
        }
    }

    private static List<string> Recur(IEnumerable<string> targets, char[,] secondaryKeypad)
    {
        List<string> best = null;
        foreach (var s1 in targets)
        {
            var newSolution = new BruteSearch(secondaryKeypad, new BruteSearch.State("", "", (0, 2)), new BruteSearch.State(s1, "", default))
                .SearchAll();
            if (newSolution is [var c1, ..])
            {
                if (best is not [var b1, ..] || c1.Cost < b1.Length)
                {
                    best = newSolution.Select(x => x.Current.Commands).ToList();
                }
                else if (c1.Cost == b1.Length)
                {
                    // best.AddRange(newSolution.Select(x => x.Current.Commands));
                }
            }
        }

        return best;
    }

    private class BruteSearch : Algorithms.BasicPriorityState<BruteSearch.State>
    {
        private readonly char[,] _map;

        internal readonly record struct State(string Typed, string Commands, GPoint2<int> Location);

        public BruteSearch(char[,] map, State start, State end) : base(start, end)
        {
            _map = map;
        }

        public BruteSearch(BruteSearch from, State current) : base(from, current)
        {
            _map = from._map;
        }

        public override IComparer<long> ScoreComparer => Comparer<long>.Default;
        
        protected override BruteSearch With(State current) => new(this, current);

        protected override IEnumerable<State> GetNextValues()
        {
            foreach (var (code, dir) in s_moves)
            {
                var candidate = Current with {Commands = Current.Commands + code, Location = Current.Location + dir};
                char target = _map.Get(candidate.Location);
                if (target == '\0') continue;

                if (code == 'A')
                {
                    candidate = candidate with { Typed = candidate.Typed + target };
                }
                
                if (!End.Typed.StartsWith(candidate.Typed)) continue;

                string sub = candidate.Commands[(candidate.Commands.LastIndexOf('A') + 1)..];
                if(sub.Contains('v') && sub.Contains('^')) continue;
                if(sub.Contains('<') && sub.Contains('>')) continue;
                yield return candidate;
            }
        }

        protected override long GetCostTo(State target) => target.Commands.Length - Current.Commands.Length;

        public override bool IsEndState() => Current.Typed == End.Typed;
    }
}

public class Problem21Whatever : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        char[,] primaryKeypad =
        {
            {'7','8','9'},
            {'4','5','6'},
            {'1','2','3'},
            {'\0','0','A'},
        };
        
        char[,] secondaryKeypad =
        {
            {'\0','^','A'},
            {'<','v','>'},
        };
        
        Dictionary<string, string> secondaryStrings = BuildString(secondaryKeypad, null);
        secondaryStrings = BuildString(secondaryKeypad, secondaryStrings);
        secondaryStrings = BuildString(secondaryKeypad, secondaryStrings);
        secondaryStrings = BuildString(secondaryKeypad, secondaryStrings);
        secondaryStrings = BuildString(secondaryKeypad, secondaryStrings);
        secondaryStrings = BuildString(secondaryKeypad, secondaryStrings);
        Dictionary<string, string> primaryStrings = BuildString(primaryKeypad, secondaryStrings);
        long total = 0;
        foreach (var line in data.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            Helpers.VerboseLine(line);
            string o1 = IndirectBot(line, primaryStrings);
            string o2 = IndirectBot(o1, secondaryStrings);
            string o3 = IndirectBot(o2, secondaryStrings);
            long codeValue = long.Parse(line[..^1]);
            long partTotal = o3.Length * codeValue;
            Console.WriteLine($"Line {line} costs {o3.Length} * {codeValue} = {partTotal}");
            total += partTotal;
        }
        Console.WriteLine($"Total value: {total}");
    }

    private static string IndirectBot(string line, Dictionary<string, string> primaryStrings)
    {
        var parts = ("A" + line).Zip(line);
        string count = "";
        foreach (var move in parts)
        {
            string p = primaryStrings[$"{move.First}{move.Second}"];
            Helpers.Verbose(p + " ");
            count += p;
        }
        Helpers.VerboseLine();

        return count;
    }

    private static readonly Dictionary<char, GPoint2<int>> moves = new (){
        ['A'] = (0, 0),
        ['v'] = (1, 0),
        ['^'] = (-1, 0),
        ['<'] = (0, -1),
        ['>'] = (0, 1),
    };

    private static Dictionary<string, string> BuildString(char[,] keyboard, Dictionary<string, string> previousStrings)
    {
        var tinyDriver = new TinyDriver(keyboard, previousStrings);
        Dictionary<string, string> topLevel = [];
        foreach ((char c1, GPoint2<int> p1) in keyboard.AsEnumerableWithPoint())
        foreach ((char c2, GPoint2<int> p2) in keyboard.AsEnumerableWithPoint())
        {
            if (c1 == '\0' || c2 == '\0') continue;
            var best = new Algorithms.BasicPriorityState<TinyDriver, TinySearch>(tinyDriver, new TinySearch("", p1), new TinySearch("", p2)).Search();
            topLevel[$"{c1}{c2}"] = best.Current.Command + "A";
        }

        return topLevel;
    }

    private class TinyDriver : Algorithms.IPrioritySearchable<TinySearch>
    {
        private readonly char[,] _map;
        private readonly Dictionary<string, string> _previousStrings;

        public TinyDriver(char[,] map, Dictionary<string, string> previousStrings)
        {
            _map = map;
            _previousStrings = previousStrings;
        }

        public long GetCost(TinySearch from, TinySearch to)
        {
            if (to.Command.Length == 0) return 0;
            string key = to.Command.Length == 1 ? "A" + to.Command : to.Command[^2..];
            return _previousStrings?[key].Length ?? 1;
        }

        public IEnumerable<TinySearch> GetNextValuesFrom(TinySearch from)
        {
            foreach ((char key, var direction) in moves)
            {
                string c = from.Command + key;
                if(c.Contains('v') && c.Contains('^')) continue;
                if(c.Contains('<') && c.Contains('>')) continue;

                var target = from.Location + direction;
                if (!_map.TryGet(target, out var l) || l == '\0') continue;

                yield return new TinySearch(c, target);
            }
        }

        public bool ReachedGoal(TinySearch test, TinySearch goal) => test.Location == goal.Location;
    }

    private class TinySearch : IEquatable<TinySearch>
    {
        public readonly string Command;
        public readonly GPoint2<int> Location;

        public TinySearch(string command, GPoint2<int> location)
        {
            Command = command;
            Location = location;
        }

        public bool Equals(TinySearch other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Location.Equals(other.Location);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TinySearch)obj);
        }

        public override int GetHashCode()
        {
            return Location.GetHashCode();
        }

        public static bool operator ==(TinySearch left, TinySearch right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TinySearch left, TinySearch right)
        {
            return !Equals(left, right);
        }
    }

    private static string S(string s, long c)
    {
        c = long.Abs(c);
        var b = "";
        for (int i = 0; i < c; i++)
        {
             b += s;
        }

        return b;
    }
}