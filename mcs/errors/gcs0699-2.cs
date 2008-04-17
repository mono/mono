// CS0699: `I<T>': A constraint references nonexistent type parameter `U'
// Line: 8

partial interface I<T>
{
}

partial interface I<T> where U : class
{
}
