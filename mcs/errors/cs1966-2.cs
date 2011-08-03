// CS1966: `C': cannot implement a dynamic interface `I<I<dynamic>>'
// Line: 8

interface I<T>
{
}

class C : I<I<dynamic>>
{
}
