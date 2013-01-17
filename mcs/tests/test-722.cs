interface IA
{
}

interface IF : IA
{
	int Prop { set; }
}

struct S : IF
{
	int prop;
	
	public S (int a)
	{
		this.prop = 5;
	}
	
	public int Prop {
		set {
			prop = value;
		}
	}
	
	void M<T> (T ia) where T : struct, IA
	{
		((IF)ia).Prop = 3;
	}
	
	public static void Main ()
	{
		S s = new S ();
		object o = s;
		((IF)((S)o)).Prop = 3;
		
		IA ia = new S ();
		((IF)ia).Prop = 3;
	}
}
