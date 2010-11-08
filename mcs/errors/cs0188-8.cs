// CS0188: The `this' object cannot be used before all of its fields are assigned to
// Line: 8

struct Sample
{
	public Sample (int arg)
	{
		text = this.Prop;
	}
	
	int Prop { get { return 1; } }

	int text;
}