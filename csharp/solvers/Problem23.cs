using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem23 : DualProblemBase
{
    protected override void ExecutePart1(string[] data)
    {
        var linkList = data.Select(d => d.Split('-')).Select(a => (a[0], a[1])).ToList();

        Dictionary<string, Node> nodes = [];

        foreach (var l in linkList)
        {
            Node a = nodes.GetOrAdd(l.Item1, s => new Node(s));
            Node b = nodes.GetOrAdd(l.Item2, s => new Node(s));
            a.LinkTo(b);
        }
        
        // "Bron–Kerbosch algorithm" (with pivots when above size 3 to increase speed)

        Stopwatch timer = Stopwatch.StartNew();
        (_, long tCount) = Iterative(nodes, false);
        
        Console.WriteLine($"{tCount} sets with a t");
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[SUB TIME] Iterative: {timer.Elapsed} ({timer.Elapsed.TotalMilliseconds}ms)");
        Console.ResetColor();
        
        timer.Restart();
        (_, tCount) = Recursive(nodes, false);
        
        Console.WriteLine($"{tCount} sets with a t");
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[SUB TIME] Recursive: {timer.Elapsed} ({timer.Elapsed.TotalMilliseconds}ms)");
        Console.ResetColor();
        
        timer.Restart();
        (_, tCount) = RecursiveWithDegeneracy(nodes, false);
        
        Console.WriteLine($"{tCount} sets with a t");
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[SUB TIME] Degeneracy: {timer.Elapsed} ({timer.Elapsed.TotalMilliseconds}ms)");
        Console.ResetColor();
    }

    protected override void ExecutePart2(string[] data)
    {
        var linkList = data.Select(d => d.Split('-')).Select(a => (a[0], a[1])).ToList();

        Dictionary<string, Node> nodes = [];

        foreach (var l in linkList)
        {
            Node a = nodes.GetOrAdd(l.Item1, s => new Node(s));
            Node b = nodes.GetOrAdd(l.Item2, s => new Node(s));
            a.LinkTo(b);
        }
        
        // "Bron–Kerbosch algorithm" (with pivots when above size 3 to increase speed)

        Stopwatch timer = Stopwatch.StartNew();
        (ImmutableHashSet<Node> best, _) = Iterative(nodes, true);
        
        Console.WriteLine($"Largest set: {string.Join(',', best.Select(r => r.Name).OrderBy())}");
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[SUB TIME] Iterative: {timer.Elapsed} ({timer.Elapsed.TotalMilliseconds}ms)");
        Console.ResetColor();
        
        timer.Restart();
        (best, _) = Recursive(nodes, true);
        
        Console.WriteLine($"Largest set: {string.Join(',', best.Select(r => r.Name).OrderBy())}");
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[SUB TIME] Recursive: {timer.Elapsed} ({timer.Elapsed.TotalMilliseconds}ms)");
        Console.ResetColor();
        
        timer.Restart();
        (best, _) = RecursiveWithDegeneracy(nodes, true);
        
        Console.WriteLine($"Largest set: {string.Join(',', best.Select(r => r.Name).OrderBy())}");
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[SUB TIME] Degeneracy: {timer.Elapsed} ({timer.Elapsed.TotalMilliseconds}ms)");
        Console.ResetColor();
    }

    private static (ImmutableHashSet<Node> best, long tCount) Iterative(Dictionary<string, Node> nodes, bool maximal)
    {
        ImmutableHashSet<Node> best = [];
        Stack<(ImmutableHashSet<Node> r, ImmutableHashSet<Node> p, ImmutableHashSet<Node> x)> search = new ();
        search.Push(([], [..nodes.Values], []));
        long tCount = 0;
        while (search.TryPop(out var s))
        {
            var (r, p, x) = s;
            if (r.Count == 3)
            {
                if (r.Any(n => n.Name[0] == 't'))
                {
                    tCount++;
                }
                continue;
            }

            if (x.IsEmpty && p.IsEmpty)
            {
                if (r.Count > best.Count)
                {
                    best = r;
                }
                continue;
            }

            var d = p;
            if (maximal)
            {
                var u = p.Union(x).First();
                d = p.Except(u.Links);
            }

            foreach (var v in d)
            {
                search.Push((r.Add(v), p.Intersect(v.Links), x.Intersect(v.Links)));
                p = p.Remove(v);
                x = x.Add(v);
            }
        }

        return (best, tCount);
    }

    private static (ImmutableHashSet<Node> maximal, long tCount) Recursive(Dictionary<string, Node> nodes, bool maximal)
    {
        var (cliques, ts) = Inner([], [..nodes.Values], [], maximal);
        return (cliques.MaxBy(c => c.Count), ts);

        static (ImmutableList<ImmutableHashSet<Node>> maximal, long tCount) Inner(
            ImmutableHashSet<Node> r,
            ImmutableHashSet<Node> p,
            ImmutableHashSet<Node> x,
            bool maximal
        )
        {
            if (!maximal && r.Count == 3)
            {
                Helpers.VerboseLine($"Triple set: {string.Join(',', r.Select(r => r.Name).OrderBy())}");
                if (r.Any(n => n.Name[0] == 't'))
                {
                    return ([], 1);
                }

                return ([], 0);
            }

            if (x.IsEmpty && p.IsEmpty)
            {
                Helpers.VerboseLine($"Maximal set: {string.Join(',', r.Select(r => r.Name).OrderBy())}");
                return ([r], 0);
            }

            var b = ImmutableList.CreateBuilder<ImmutableHashSet<Node>>();
            var d = p;
            if (maximal)
            {
                var u = p.Union(x).First();
                d = p.Except(u.Links);
            }

            long tCount = 0;
            foreach (var v in d)
            {
                var (childMaximal, childTCount) = Inner(r.Add(v), p.Intersect(v.Links), x.Intersect(v.Links), maximal);
                tCount += childTCount;
                b.AddRange(childMaximal);
                p = p.Remove(v);
                x = x.Add(v);
            }

            return (b.ToImmutable(), tCount);
        }
    }

    private static (ImmutableHashSet<Node> maximal, long tCount) RecursiveWithDegeneracy(Dictionary<string, Node> nodes, bool maximal)
    {
        var (cliques, ts) = DegeneracySort(nodes.Values.ToList(), maximal);
        return (cliques.MaxBy(c => c.Count), ts);

        static (ImmutableList<ImmutableHashSet<Node>> maximal, long tCount) DegeneracySort(IReadOnlyList<Node> nodes, bool maximal)
        {
            long tCount = 0;
            var b = ImmutableList.CreateBuilder<ImmutableHashSet<Node>>();
            var ordered = nodes.OrderBy(n => n.Links.Count);
            ImmutableHashSet<Node> p = [..nodes];
            ImmutableHashSet<Node> x = [];
            foreach (var v in ordered)
            {
                var (childMaximal, childTCount) = Inner([v], p.Intersect(v.Links), x.Intersect(v.Links), maximal);
                tCount += childTCount;
                b.AddRange(childMaximal);
                p = p.Remove(v);
                x = x.Add(v);
            }
            return (b.ToImmutable(), tCount);
        }

        static (ImmutableList<ImmutableHashSet<Node>> maximal, long tCount) Inner(
            ImmutableHashSet<Node> r,
            ImmutableHashSet<Node> p,
            ImmutableHashSet<Node> x,
            bool maximal
        )
        {
            long tCount = 0;
            if (!maximal && r.Count == 3)
            {
                Helpers.VerboseLine($"Triple set: {string.Join(',', r.Select(r => r.Name).OrderBy())}");
                if (r.Any(n => n.Name[0] == 't'))
                {
                    return ([], 1);
                }

                return ([], 0);
            }

            if (x.IsEmpty && p.IsEmpty)
            {
                Helpers.VerboseLine($"Maximal set: {string.Join(',', r.Select(r => r.Name).OrderBy())}");
                return ([r], tCount);
            }

            var b = ImmutableList.CreateBuilder<ImmutableHashSet<Node>>();
            var d = p;
            if (maximal)
            {
                var u = p.Union(x).First();
                d = p.Except(u.Links);
            }

            foreach (var v in d)
            {
                var (childMaximal, childTCount) = Inner(r.Add(v), p.Intersect(v.Links), x.Intersect(v.Links), maximal);
                tCount += childTCount;
                b.AddRange(childMaximal);
                p = p.Remove(v);
                x = x.Add(v);
            }

            return (b.ToImmutable(), tCount);
        }
    }

    public class Node
    {
        public Node(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public ImmutableHashSet<Node> Links { get; private set; } = [];
        
        public void LinkTo(Node other)
        {
            Links = Links.Add(other);
            other.Links = other.Links.Add(this);
        }

        protected bool Equals(Node other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Node)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(Node left, Node right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Node left, Node right)
        {
            return !Equals(left, right);
        }
    }

    public class Clique
    {
        public readonly ImmutableHashSet<Node> Members;

        public Clique(params ImmutableHashSet<Node> members)
        {
            Members = members;
        }

        public bool TryAdd(Node node, out Clique grown)
        {
            if (Members.Contains(node))
            {
                grown = null;
                return false;
            }

            if (Members.Intersect(node.Links).Count == Members.Count)
            {
                // We overlapped!
                grown = new Clique(Members.Add(node));

                return true;
            }

            grown = null;
            return false;
        }

        public override string ToString()
        {
            return string.Join(',', Members.Select(m => m.Name).OrderBy());
        }
    }
}