// CS0233: `MainClass.S' does not have a predefined size, therefore sizeof can only be used in an unsafe context (consider using System.Runtime.InteropServices.Marshal.SizeOf)
// Line: 10

public class MainClass {
	struct S
	{
	}
	
        static int Main () {
                return sizeof(S);
        }
}


