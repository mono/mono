// CS1609: Modifiers cannot be placed on event accessor declarations
// Line: 9

delegate int d();

class C
{
	public event d E {
		private  add {}
		remove {}
	}
}
