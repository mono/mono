//
// This tests only checks if we can compile the program.
//
// The trick is that we are accessing a protected property, with the way
// mcs deals with properties in two steps (property first, method next)
// this text excercises some eager optimizations in the TypeManager.FilterWithClosure.
//
//
// The second class excercises accessing private members from a container class
//
// The third class excercises accessing a private/protected value on an instance of
// a child

using System;
using System.Collections;
        
class ProtectedAccessToPropertyOnChild : Hashtable {

	ProtectedAccessToPropertyOnChild ()
	{
		comparer = null;
	}
	
	public static int Main ()
	{
		TestAccessToProtectedOnChildInstanceFromParent t = new TestAccessToProtectedOnChildInstanceFromParent ();

		if (t.Test () != 0)
			return 1;
		
		return 0;
		
	}
}

//
// Again, only for compiling reasons
//
public class TestAccessToPrivateMemberInParentClass
{
        double[][] data;
        int rows;
        int columns;

        public TestAccessToPrivateMemberInParentClass()
        {
        }

        double[][] Array
        {
                get { return data; }
        }

        class CholeskyDecomposition
        {
                TestAccessToPrivateMemberInParentClass L;
                bool isSymmetric;
                bool isPositiveDefinite;
        
                public CholeskyDecomposition(TestAccessToPrivateMemberInParentClass A)
                {
                        L = new TestAccessToPrivateMemberInParentClass();
                                        
                        double[][] a = A.Array;
                        double[][] l = L.Array;
                }
        }
}

public class TestAccessToProtectedOnChildInstanceFromParent {

	class Parent {
		protected int a;

		static int x;
		
		protected Parent ()
		{
			a = x++;
		}
		
		public int TestAccessToProtected (Child c)
		{
			if (c.a == 0)
				return 1;
			else
				return 2;
		}
	}

	class Child : Parent {
		
	}

	Child c, d;
	
	public TestAccessToProtectedOnChildInstanceFromParent ()
	{
		c = new Child ();
		d = new Child ();
	}

	public int Test ()
	{
		if (d.TestAccessToProtected (c) == 1)
			return 0;
		return 1;
	}
	
}
