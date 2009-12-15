// CS1966: A constraint cannot be the dynamic type `I<dynamic>'
// Line: 8

interface I<T>
{
}

class C<T> where T : I<dynamic>
{
}
