using System;
using System.Reflection;

class Test {

	static protected internal void MyProtectedInternal () { }
	static internal void MyInternal() { }
	static public void MyPublic () { }
	static void MyPrivate () {}
	      
	static int Main ()
	{
		Type myself = typeof (Test);
		BindingFlags bf = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
		MethodAttributes mpia;
		MethodInfo mpi;

		//
		// protected internal
		//
		mpi = myself.GetMethod ("MyProtectedInternal", bf);
		mpia = mpi.Attributes & MethodAttributes.MemberAccessMask;
		if (mpia != MethodAttributes.FamORAssem)
			return 1;

		//
		// internal
		//
		mpi = myself.GetMethod ("MyInternal", bf);
		mpia = mpi.Attributes & MethodAttributes.MemberAccessMask;
		if (mpia != MethodAttributes.Assembly)
			return 2;

		//
		// public
		//
		mpi = myself.GetMethod ("MyPublic", bf);
		mpia = mpi.Attributes & MethodAttributes.MemberAccessMask;
		if (mpia != MethodAttributes.Public)
			return 3;

		//
		// private
		//
		mpi = myself.GetMethod ("MyPrivate", bf);
		mpia = mpi.Attributes & MethodAttributes.MemberAccessMask;
		if (mpia != MethodAttributes.Private)
			return 4;

		Console.WriteLine ("All tests pass");
		return 0;
	}
}
