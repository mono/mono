using System;
using System.Reflection;

namespace Mono.Test
{
	class Program
	{
		static int Main ()
		{
			Type t = typeof (B);
			InterfaceMapping map = t.GetInterfaceMap (typeof (ITest));

			foreach (MethodInfo m in map.TargetMethods) {
				if (m.Name.Contains ("."))
					return 3;
			}

			if (map.TargetMethods.Length != 3)
				return 1;

			MethodInfo[] methods = t.GetMethods (BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			if (methods.Length != 0)
				return 2;

			return 0;
		}
	}

	public interface ITest
	{
		bool Success
		{
			get;
		}

		void Run ();
		void Gen<T> ();
	}

	public class A
	{
		public bool Success
		{
			get { return true; }
		}

		public void Run ()
		{
		}

		public void Gen<U> ()
		{
		}
	}

	public class B : A, ITest
	{
	}
}
