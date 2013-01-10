// CS0529: Inherited interface `IB' causes a cycle in the interface hierarchy of `IC'
// Line: 12

interface IC : IB
{
}

partial interface IB
{
}

partial interface IB : IC
{
}

