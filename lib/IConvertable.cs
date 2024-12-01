namespace ChadNedzlek.AdventOfCode.Library;

public interface IConvertable<in T1, out T2> where T2 : IConvertable<T1, T2>
{
    static abstract implicit operator T2(T1 p);
}