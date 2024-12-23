using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;
using Microsoft.VisualBasic;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem23 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        var linkList = data.Select(d => d.Split('-')).Select(a => (a[0], a[1])).ToList();

        Dictionary<string, Node> nodes = [];

        foreach (var l in linkList)
        {
            var a = nodes.GetOrAdd(l.Item1, s => new Node(s));
            var b = nodes.GetOrAdd(l.Item2, s => new Node(s));
            a.LinkTo(b);
        }

        HashSet<ImmutableHashSet<Node>> triples = [];
        
        // "Bron–Kerbosch algorithm" (with pivots when above size 3 to increase speed)

        ImmutableHashSet<Node> best = [];
        Stack<(ImmutableHashSet<Node> r, ImmutableHashSet<Node> p, ImmutableHashSet<Node> x)> search = [];
        search.Push(([], [..nodes.Values], []));
        while (search.TryPop(out var s))
        {
            var (r, p, x) = s;
            if (r.Count == 3)
            {
                Helpers.VerboseLine($"Triple set: {string.Join(',', r.Select(r => r.Name).OrderBy())}");
                triples.Add(r);
            }

            if (x.IsEmpty && p.IsEmpty)
            {
                if (r.Count > best.Count)
                {
                    best = r;
                    Console.WriteLine($"Maximal set: {string.Join(',', r.Select(r => r.Name).OrderBy())}");
                }
                continue;
            }

            var d = p;
            if (r.Count >= 3)
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
        Console.WriteLine($"{triples.Count(t => t.Any(c => c.Name[0] == 't'))} sets with a t");
    }

    public static bool IsConnection(string computer, (string, string) link, out string other)
    {
        if (link.Item1 == computer)
        {
            other = link.Item2;
            return true;
        }

        if (link.Item2 == computer)
        {
            other = link.Item1;
            return true;
        }

        other = null;
        return false;
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