// cs0111.cs: Duplicated method definition for the operator
// 
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
