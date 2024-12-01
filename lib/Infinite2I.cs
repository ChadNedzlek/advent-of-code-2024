using System;
using System.Collections;
using System.Collections.Generic;

namespace ChadNedzlek.AdventOfCode.Library;

public class Infinite2I<T> : IEnumerable<T>
{
    private readonly Dictionary<(int, int), T> _sparse = new();
    private readonly Func<int, int, T> _populate = (_,_) => default;
    private int _min0, _min1, _max0, _max1;

    public Infinite2I()
    {
    }

    public Infinite2I(int length0, int length1)
    {
        _max0 = length0 - 1;
        _max1 = length1 - 1;
    }

    public Infinite2I(Func<int, int, T> populate) : this(0, 0, populate)
    {
    }

    public Infinite2I(int length0, int length1, Func<int, int, T> populate)
    {
        _max0 = length0 - 1;
        _max1 = length1 - 1;
        _populate = populate;
    }

    public T this[int i0, int i1]
    {
        get
        {
            if (_sparse.TryGetValue((i0, i1), out var value))
                return value;
            return _populate(i0, i1);
        }
        set
        {

            _min0 = int.Min(_min0, i0);
            _min1 = int.Min(_min1, i1);
                
            _max0 = int.Max(_max0, i0);
            _max1 = int.Max(_max1, i1);
                
            _sparse[(i0, i1)] = value;
        }
    }

    public bool TrySet(int i0, int i1, T value)
    {
        if (i0 < _min0 || i0 > _max0 || i1 < _min1 || i1 > _max1)
        {
            return false;
        }

        _sparse[(i0, i1)] = value;
        return true;
    }
    
    public bool TryGet(int i0, int i1, out T value)
    {
        if (i0 < _min0 || i0 > _max0 || i1 < _min1 || i1 > _max1)
        {
            value = default;
            return false;
        }

        if (!_sparse.TryGetValue((i0, i1), out value))
        {
            value = _populate(i0, i1);
        }

        return true;
    }

    public int GetLowerBound(int i) => i switch
    {
        0 => _min0,
        1 => _min1,
        _ => throw new ArgumentException(),
    };
    public int GetUpperBound(int i) =>  i switch
    {
        0 => _max0,
        1 => _max1,
        _ => throw new ArgumentException(),
    };
    public int GetLength(int i) => i switch
    {
        0 => _max0 - _min0 + 1,
        1 => _max1 - _min1 + 1,
        _ => throw new ArgumentException(),
    };

    public IEnumerator<T> GetEnumerator()
    {
        for (int i0 = _min0; i0 <= _max0; i0++)
        {
            for (int i1 = _min1; i1 <= _max1; i1++)
            {
                if (_sparse.TryGetValue((i0, i1), out var value))
                {
                    yield return value;
                }
                else
                {
                    yield return _populate(i0, i1);
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear()
    {
        _sparse.Clear();
    }
}

public class TransformationMatrix2I
{
}