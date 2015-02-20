// This file can't be recompiled with current (2015-03-16) mcs/csc as it doesn't reuse locals in different scopes.
// Original was compiled with RemObjects C# (8.0.81.1681)

using System;

namespace DebugLocals
{
	static class Program
	{
		public static Int32 Main(string[] args)
		{
			{
				int testa = 15;
				testa++;
			} 
			{
				int testb = 15;
				testb++;
			}
			return 0;
		}
	}
}
