using System; 
using System.Reflection;
using System.Runtime.CompilerServices;

public class Test 
{ 
	public delegate void DelType(); 
 
	public event DelType MyEvent; 
 
	public static int Main()
        {
                EventInfo ei = typeof(Test).GetEvent ("MyEvent");
		MethodImplAttributes methodImplAttributes = ei.GetAddMethod ().GetMethodImplementationFlags();
            
                if ((methodImplAttributes & MethodImplAttributes.Synchronized) == 0) {
                    Console.WriteLine ("FAILED");
                    return 1;
                }

                methodImplAttributes = ei.GetRemoveMethod ().GetMethodImplementationFlags();
                if ((methodImplAttributes & MethodImplAttributes.Synchronized) == 0) {
                    Console.WriteLine ("FAILED");
                    return 2;
                }

                return 0;
        } 
}
