//
// This test ensures that we emit attributes for operators
// only once.
//

using System.ComponentModel;
using System.Reflection;

public class BrowsableClass
{
        [EditorBrowsable(EditorBrowsableState.Always)]
        public static BrowsableClass operator ++(BrowsableClass a) 
        { 
                return null; 
        }

        public static int Main ()
        {
                BrowsableClass c = new BrowsableClass ();
                MethodInfo mi = c.GetType().GetMethod ("op_Increment");
                
                object[] attributes = mi.GetCustomAttributes
                        (typeof(EditorBrowsableAttribute), false);

                if (attributes.Length != 1)
                        return 1;

                return 0;
        }
}

