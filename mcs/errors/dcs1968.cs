// CS1966: A constraint cannot be the dynamic type `I<object>''
// Line: 8

interface I<T>
{
}

class C<T> where T : I<dynamic>
{
}
