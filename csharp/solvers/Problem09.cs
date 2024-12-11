using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem09 : SyncProblemBase
{
    private class ArrayImplProblem09 : DualProblemBase
    {
        protected override void ExecutePart1(string[] data)
        {
            var line = data[0];
            int len = line.Sum(c => c - '0');
            int[] disk = new int[len];
            int pos = 0;
            int count = 0;

            for (int i = 0; i < line.Length; i += 2)
            {
                int fileLength = line[i] - '0';
                int spaceLength = Helpers.Get(line, i + 1, '0') - '0';
                for (int j = 0; j < fileLength; j++)
                {
                    disk[pos + j] = count;
                }

                count++;

                pos += fileLength;
                for (int j = 0; j < spaceLength; j++)
                {
                    disk[pos + j] = -1;
                }

                pos += spaceLength;
            }

            int start = 0;
            int end = len - 1;
            while (start < end)
            {
                if (disk[start] != -1)
                {
                    start++;
                    continue;
                }

                if (disk[end] == -1)
                {
                    end--;
                    continue;
                }

                (disk[start], disk[end]) = (disk[end], disk[start]);
                start++;
                end--;
            }

            long checksum = 0;
            for (int i = 0; disk[i] != -1; i++)
            {
                checksum += i * disk[i];
            }

            Console.WriteLine($"Checksum thingy {checksum}");
        }

        protected override void ExecutePart2(string[] data)
        {
            var line = data[0];
            Dictionary<int, List<RangeL>> ranges = [];
            int len = line.Sum(c => c - '0');
            int[] disk = new int[len];
            int pos = 0;
            int count = 0;

            for (int i = 0; i < line.Length; i += 2)
            {
                int fileLength = line[i] - '0';
                int spaceLength = Helpers.Get(line, i + 1, '0') - '0';
                for (int j = 0; j < fileLength; j++)
                {
                    disk[pos + j] = count;
                }

                count++;

                pos += fileLength;
                for (int j = 0; j < spaceLength; j++)
                {
                    disk[pos + j] = -1;
                }

                pos += spaceLength;
            }

            Checksums = new List<long>();
            for (int end = len - 1; end > 0; end--)
            {
                int id = disk[end];
                if (id == -1) continue;
                if (TryMove(end, id))
                {
                    Checksums.Add(CalculateChecksum(disk));
                }

                while (Helpers.Get<int>(disk, end, -1) == id)
                {
                    end--;
                }

                end++;
            }

            long checksum = CalculateChecksum(disk);

            Console.WriteLine($"Checksum thingy {checksum}");
            return;

            bool TryMove(int end, int id)
            {
                for (int start = 0; start < end; start++)
                {
                    if (disk[start] != -1) continue;

                    int i;
                    for (i = 0; disk[start + i] == -1; i++)
                    {
                        if (disk[end - i] != id)
                        {
                            var fileSpan = disk.AsSpan(end - i + 1, i);
                            fileSpan.CopyTo(disk.AsSpan(start, i));
                            fileSpan.Fill(-1);
                            Helpers.VerboseLine(string.Join("", disk.Select(d => d == -1 ? "." : d.ToString())));
                            return true;
                        }
                    }

                    if (disk[end - i] != id)
                    {
                        var fileSpan = disk.AsSpan(end - i + 1, i);
                        fileSpan.CopyTo(disk.AsSpan(start, i));
                        fileSpan.Fill(-1);
                        Helpers.VerboseLine(string.Join("", disk.Select(d => d == -1 ? "." : d.ToString())));
                        return true;
                    }

                    start += i - 1;
                }

                return false;
            }
        }

        public List<long> Checksums { get; private set; }

        private static long CalculateChecksum(int[] disk)
        {
            long checksum = 0;
            for (int i = 0; i < disk.Length; i++)
            {
                if (disk[i] == -1) continue;
                checksum += i * disk[i];
            }

            return checksum;
        }
    }

    private class RangesImplProblem09 : DualProblemBase
    {
        public List<long> Checksums { get; set; }

        protected override void ExecutePart1(string[] data)
        {
            LinkedList<(int? Id, RangeL Location)> ranges = BuildRanges(data);

            var findEmpty = ranges.First;
            var toMove = ranges.Last;

            while (findEmpty is { Value.Id: not null })
            {
                findEmpty = findEmpty.Next;
            }

            while (toMove is { Value.Id: null })
            {
                toMove = toMove.Previous;
            }

            while (findEmpty is not null && toMove is not null && findEmpty != toMove)
            {
                var emptyRange = findEmpty.Value.Location;
                (int? Id, RangeL Location) fileRec = toMove.Value;
                RangeL fileRange = fileRec.Location;
                RangeL movingRange = fileRange.FromEnd(emptyRange.Length, out var remainingMaybe);
                RangeL emptyToMove = emptyRange.FromStart(movingRange.Length, out var remainingEmptyMaybe);
                RangeL movedEmpty = fileRange.FromEnd(emptyToMove.Length, out _);

                Helpers.VerboseLine(
                    $"Moving {movingRange.Length} blocks of {fileRec.Id} to {emptyRange.Start} (remaining: {remainingMaybe.GetValueOrDefault().Length})"
                );

                ranges.AddAfter(toMove, (null, movedEmpty));
                ranges.AddBefore(findEmpty, (fileRec.Id, movingRange with { Start = emptyRange.Start }));

                var prevMove = toMove;
                if (remainingMaybe is { } remaining)
                {
                    toMove = ranges.AddAfter(toMove, (fileRec.Id, remaining));
                    ranges.Remove(prevMove);
                }
                else
                {
                    toMove = toMove.Previous;
                    ranges.Remove(prevMove);
                    while (toMove is { Value.Id: null } && findEmpty != toMove)
                    {
                        toMove = toMove.Previous;
                    }
                }

                var prevEmpty = findEmpty;
                if (remainingEmptyMaybe is { } remainingEmpty)
                {
                    findEmpty = ranges.AddBefore(findEmpty, (null, remainingEmpty));
                    ranges.Remove(prevEmpty);
                }
                else
                {
                    findEmpty = findEmpty.Next;
                    ranges.Remove(prevEmpty);
                    while (findEmpty is { Value.Id: not null } && findEmpty != toMove)
                    {
                        findEmpty = findEmpty.Next;
                    }
                }

                Helpers.IfVerbose(
                    () =>
                    {
                        foreach (var range in ranges)
                        {
                            if (range.Id is { } id)
                            {
                                Helpers.Verbose(new string((char)('0' + id), (int)range.Location.Length));
                            }
                            else
                            {
                                Helpers.Verbose(new string('.', (int)range.Location.Length));
                            }
                        }

                        Helpers.VerboseLine();
                    }
                );
            }

            WriteChecksum(ranges);
        }

        protected override void ExecutePart2(string[] data)
        {
            LinkedList<(int? Id, RangeL Location)> ranges = BuildRanges(data);

            Queue<long> checksumChecks = new(Checksums);

            var toMove = ranges.Last;
            while (true)
            {
                LinkedListNode<(int? Id, RangeL Location)> orig = toMove;
                while (toMove is { Value.Id: null })
                {
                    toMove = toMove.Previous;
                }

                if (toMove is null)
                {
                    break;
                }

                if (toMove.Value.Id is { } id && id % 100 == 0)
                {
                }

                // We need to double previous, because we might be about to delete 
                LinkedListNode<(int? Id, RangeL Location)> nextMove = toMove.Next;
                if (TryMoveBlock())
                {
                }

                toMove = nextMove is null ? ranges.Last.Previous : nextMove.Previous?.Previous;
            }

            if (checksumChecks.TryDequeue(out var missedOne))
            {
            }

            WriteChecksum(ranges);
            return;

            bool TryMoveBlock()
            {
                for (var findEmpty = ranges.First; findEmpty != toMove; findEmpty = findEmpty.Next)
                {
                    if (findEmpty.Value.Id.HasValue)
                    {
                        continue;
                    }

                    RangeL emptyRange = findEmpty.Value.Location;
                    (int? Id, RangeL Location) fileRec = toMove.Value;
                    RangeL fileRange = fileRec.Location;
                    RangeL movingRange = fileRange.FromEnd(emptyRange.Length, out var remainingMaybe);
                    if (remainingMaybe.HasValue)
                    {
                        // Can't fit the whole file
                        continue;
                    }

                    RangeL emptyToMove = emptyRange.FromStart(movingRange.Length, out var remainingEmptyMaybe);
                    RangeL movedEmpty = fileRange.FromEnd(emptyToMove.Length, out _);

                    Helpers.VerboseLine(
                        $"Moving {movingRange.Length} blocks of {fileRec.Id} to {emptyRange.Start} (remaining: {remainingMaybe.GetValueOrDefault().Length})"
                    );

                    var prevMove = toMove;
                    toMove = ranges.AddAfter(toMove, (null, movedEmpty));
                    ranges.Remove(prevMove);
                    ranges.AddBefore(findEmpty, (fileRec.Id, movingRange with { Start = emptyRange.Start }));
                    if (remainingEmptyMaybe is { } remainingEmpty)
                    {
                        ranges.AddBefore(findEmpty, (null, remainingEmpty));
                    }

                    ranges.Remove(findEmpty);

                    Helpers.IfVerbose(
                        () =>
                        {
                            foreach (var range in ranges)
                            {
                                if (range.Id is { } id)
                                {
                                    Helpers.Verbose(new string((char)('0' + id), (int)range.Location.Length));
                                }
                                else
                                {
                                    Helpers.Verbose(new string('.', (int)range.Location.Length));
                                }
                            }

                            Helpers.VerboseLine();
                        }
                    );
                    return true;
                }

                return false;
            }
        }

        private static LinkedList<(int? Id, RangeL Location)> BuildRanges(string[] data)
        {
            LinkedList<(int? Id, RangeL Location)> ranges = [];
            int id = 0;
            bool empty = true;
            foreach (var c in data[0])
            {
                empty = !empty;
                int size = c - '0';
                if (size == 0) continue;
                var prevEnd = ranges.Last?.Value.Location.End ?? 0;
                ranges.AddLast((empty ? null : id++, new RangeL(prevEnd, size)));
            }

            return ranges;
        }

        private void WriteChecksum(LinkedList<(int? Id, RangeL Location)> ranges)
        {
            long sum = CalculateChecksum(ranges);

            Console.WriteLine($"Checksum {sum}");
        }

        private static long CalculateChecksum(LinkedList<(int? Id, RangeL Location)> ranges)
        {
            long sum = 0;
            foreach ((int? Id, RangeL Location) r in ranges)
            {
                if (r.Id is { } id)
                {
                    long idSum = (r.Location.End + r.Location.Start - 1) * r.Location.Length / 2;
                    long partialSum = id * idSum;
                    sum += partialSum;
                }
            }

            return sum;
        }
    }

    protected override void ExecuteCore(string[] data)
    {
        var a = new ArrayImplProblem09();
        var r = new RangesImplProblem09();
        Stopwatch timer = Stopwatch.StartNew();
        a.ExecuteAsync().GetAwaiter().GetResult();
        Console.WriteLine($"[TIME] ArrayImpl: {timer.Elapsed}");
        r.Checksums = a.Checksums;
        timer.Restart();
        r.ExecuteAsync().GetAwaiter().GetResult();
        Console.WriteLine($"[TIME] RangesImpl: {timer.Elapsed}");
    }
}