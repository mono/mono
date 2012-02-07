class C
{
	public static void Main ()
	{
		Prop = 3;
	}
	
	static int Prop
	{
		get {
			return 4;
		}
		
		set {
		}
	}
	
	static int PropAuto
	{
		get;
		set;
	}
}
