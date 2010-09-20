// Test case for Bug #75934
// Checks for duplicate field names

using System;
using System.Collections;
using System.Reflection;

class test
{
        public IEnumerable testen (int x)
        {
                for (int i = 0;i < x; i++)
                        if (i % 2 == 0) {
                                int o = i;
                                yield return o;
                        } else {
                                int o = i*2;
                                yield return o;
                        }
        }
}

class reflect
{
	public static void Main (string [] args)
	{
		Hashtable ht = new Hashtable ();
		Assembly asm = Assembly.GetAssembly (typeof (test));
		foreach (Type t in asm.GetTypes ()) {
			ht.Clear ();
			foreach (FieldInfo fi in t.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				ht.Add (fi.Name, fi);
		}
	}
}
