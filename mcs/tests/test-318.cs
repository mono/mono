// This code must be compilable without any warning
// Compiler options: -warnaserror -warn:4 -unsafe
// Test of wrong CS0219 warning

public class C {
    
	public static void my_from_fixed(out int val)
	{
		val = 3;
	}
        
	public static void month_from_fixed(int date)
	{
		int year;
		my_from_fixed(out year);
	}
        
	internal static int CreateFromString (int arg)
	{
		int major = 0;
		int number = 5;

		major = number;
		number = -1;
                    
		return major;
	}   
        
        public unsafe double* GetValue (double value)
	{
		double d = value;
		return &d;
	}        
        
	public static void Main () {
	}
}
