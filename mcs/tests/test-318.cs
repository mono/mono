// This code must be compilable without any warning
// Compiler options: -warnaserror -warn:4

public class C
{
        public static void my_from_fixed(out int val)
        {
            val = 3;
        }
        
        public static void month_from_fixed(int date) {
		int year;
		my_from_fixed(out year);
	}
        
        public static void Main ()
        {
        }
}
