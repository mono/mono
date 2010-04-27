// gcs0409.cs: A constraint clause has already been specified for type parameter `U'
// Line: 7

class C<T, U> where U: class, new() where U: new()
{
}
