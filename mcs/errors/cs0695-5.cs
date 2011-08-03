// CS0695: `C<T1,T2>' cannot implement both `IA<T1>' and `IB<IA<T2>>' because they may unify for some type parameter substitutions
// Line: 12

interface IA<T>
{
}

interface IB<T> : IA<T>
{
}

class C<T1, T2> : IA<T1>, IB<IA<T2>>
{
}
