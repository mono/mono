// CS0557: Duplicate user-defined conversion in type `C'
// Line: 9
class C {
		public static bool operator != (C a, C b) 
		{
			return true;
		}
		
		public static bool operator != (C a, C b) 
		{
			return true;
		}
	
		public static bool operator == (C a, C b)
		{	return false; }		
	}





class X {
	static void Main ()
	{
	}
}
