using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: Test]

namespace N 
{
}

[AttributeUsage(AttributeTargets.All)]
public class TestAttribute: Attribute
{
}

public class Test_1
{
    [return: Test]
    public void Test (int a)
    {
    }
}

[return: Test]
public delegate Delegate test_delegate(int i);


public class Test_2
{
    public int Test
    {
        [return: Test]
        get {
            return 4;
        }

		[return: Test]
		set {
		}
    }

	public bool Test2
	{
		[param: Test]
		set {}
	}
}

public class Test_3
{
	[field: Test]
	public event test_delegate e_1;

	[method: Test]
	public event test_delegate e_2;
}

public class Test_4
{
	// TODO: Where to apply ?

	[event: Test]
	public event test_delegate e_1 {
		add {}
		remove {}
	}

	public event test_delegate e_2 {
		[return: Test]
		add {}
		[return: Test]
		remove {}
	}

	public event test_delegate e_3 {
		[param: Test]
		add {}
		[param: Test]
		remove {}
	}
}


public class ClassMain
{
        static bool failed = false;
    
	static void Assert (object[] attrs, bool expected_presence, int tc)
	{
		if (attrs.Length == 1 && expected_presence)
			return;

		if (!expected_presence && attrs.Length == 0)
			return;

		Console.WriteLine ("#" + tc.ToString () + " failed");
                failed = true;
	}

	public static int Main () {
		MethodInfo mi = typeof (Test_1).GetMethod ("Test");
		Assert (mi.GetParameters ()[0].GetCustomAttributes (true), false, 1);
		Assert (mi.GetCustomAttributes (true), false, 2);
		Assert (mi.ReturnTypeCustomAttributes.GetCustomAttributes (true), true, 3);
        
		mi = typeof (test_delegate).GetMethod ("Invoke");
		Assert (mi.GetParameters ()[0].GetCustomAttributes (true), false, 4);
		Assert (mi.GetCustomAttributes (true), false, 5);
		Assert (mi.ReturnTypeCustomAttributes.GetCustomAttributes (true), true, 6);

		/* Under net 2.0, SerializableAttribute is returned */
		if (typeof (test_delegate).GetCustomAttributes (false).Length != 1)
			Assert (typeof (test_delegate).GetCustomAttributes (false), false, 7);

		PropertyInfo pi = typeof (Test_2).GetProperty ("Test");
		Assert (pi.GetCustomAttributes (true), false, 31);
		Assert (pi.GetGetMethod ().GetCustomAttributes (true), false, 32);
		Assert (pi.GetGetMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), true, 33);
		Assert (pi.GetSetMethod ().GetCustomAttributes (true), false, 34);
		Assert (pi.GetSetMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), true, 35);
		pi = typeof (Test_2).GetProperty ("Test2");
		Assert (pi.GetCustomAttributes (true), false, 36);
		Assert (pi.GetSetMethod ().GetCustomAttributes (true), false, 37);
		Assert (pi.GetSetMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), false, 38);
		Assert (pi.GetSetMethod ().GetParameters ()[0].GetCustomAttributes (true), true, 39);

		EventInfo ei = typeof(Test_3).GetEvent ("e_1");
		Assert (ei.GetCustomAttributes (true), false, 41);
		Assert (ei.GetAddMethod ().GetCustomAttributes (true), false, 42);
		Assert (ei.GetAddMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), false, 43);
		Assert (ei.GetRemoveMethod ().GetCustomAttributes (true), false, 44);
		Assert (ei.GetRemoveMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), false, 45);
		FieldInfo fi = typeof(Test_3).GetField ("e_1", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		Assert (fi.GetCustomAttributes (typeof (CompilerGeneratedAttribute), true), true, 46);
		Assert (fi.GetCustomAttributes (typeof (TestAttribute), true), true, 47);

		ei = typeof(Test_3).GetEvent ("e_2");
		Assert (ei.GetCustomAttributes (true), false, 51);
		Assert (ei.GetAddMethod ().GetCustomAttributes (true), true, 52);
		Assert (ei.GetAddMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), false, 53);
		Assert (ei.GetRemoveMethod ().GetCustomAttributes (true), true, 54);
		Assert (ei.GetRemoveMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), false, 55);
		fi = typeof(Test_3).GetField ("e_2", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		Assert (fi.GetCustomAttributes (typeof (CompilerGeneratedAttribute), true), true, 56);
		Assert (fi.GetCustomAttributes (typeof (TestAttribute), true), false, 57);

		ei = typeof(Test_4).GetEvent ("e_2");
		Assert (ei.GetCustomAttributes (true), false, 71);
		Assert (ei.GetAddMethod ().GetCustomAttributes (true), false, 72);
		Assert (ei.GetAddMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), true, 73);
		Assert (ei.GetRemoveMethod ().GetCustomAttributes (true), false, 74);
		Assert (ei.GetRemoveMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), true, 75);
		fi = typeof(Test_3).GetField ("e_2", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		Assert (fi.GetCustomAttributes (typeof (CompilerGeneratedAttribute), true), true, 76);
		Assert (fi.GetCustomAttributes (typeof (TestAttribute), true), false, 77);

		ei = typeof(Test_4).GetEvent ("e_3");
		Assert (ei.GetCustomAttributes (true), false, 81);
		Assert (ei.GetAddMethod ().GetCustomAttributes (true), false, 82);
		Assert (ei.GetAddMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), false, 83);
		Assert (ei.GetAddMethod ().GetParameters ()[0].GetCustomAttributes (true), true, 84);
		Assert (ei.GetRemoveMethod ().GetCustomAttributes (true), false, 85);
		Assert (ei.GetRemoveMethod ().ReturnTypeCustomAttributes.GetCustomAttributes (true), false, 86);
		Assert (ei.GetRemoveMethod ().GetParameters ()[0].GetCustomAttributes (true), true, 87);
		fi = typeof(Test_3).GetField ("e_2", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		Assert (fi.GetCustomAttributes (typeof (CompilerGeneratedAttribute), true), true, 86);
		Assert (fi.GetCustomAttributes (typeof (TestAttribute), true), false, 87);

		return failed ? 1 : 0;
	}
}
