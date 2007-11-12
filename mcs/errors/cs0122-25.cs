// CS0122: `AAttribute.AAttribute()' is inaccessible due to its protection level
// Line: 9

class AAttribute : System.Attribute
{
	protected AAttribute() { }
}

[A]
class C
{
}
