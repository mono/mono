// gcs0265.cs: Partial declarations of `Partial<T>' have inconsistent constraints for type parameter `T'
// Line: 4

partial class Partial<T> where T: class, new()
{
}

partial class Partial<T> where T : new ()
{
}
