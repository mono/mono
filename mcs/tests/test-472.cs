using System;
using System.Reflection;

class Test
{
        public int Prop {
                get { return prop; }
                set { prop = value; }
        }

        int prop = 0;

        public static int Main()
        {
                MethodInfo mi = typeof (Test).GetMethod ("set_Prop");
                if (mi.GetParameters ().Length != 1)
					return 1;
				if ((mi.GetParameters ()[0].Name) != "value")
					return 2;
				
				Console.WriteLine ("OK");
				return 0;
        }
}
