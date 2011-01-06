// CS0703: Inconsistent accessibility: constraint type `I' is less accessible than `C<T>'
// Line: 8

interface I
{
}

public class C<T>  where T : I
{
}