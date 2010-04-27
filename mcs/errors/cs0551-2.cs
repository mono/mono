// CS0551: Explicit interface implementation `PropertyClass.PropertyInterface.this[bool]' is missing accessor `PropertyInterface.this[bool].get'
// Line: 11

interface PropertyInterface
{
	int this [bool b] { get; set; }
}

public class PropertyClass: PropertyInterface
{
	int PropertyInterface.this [bool b] { 
		set { 
			return 0;
		} 
	}
}

