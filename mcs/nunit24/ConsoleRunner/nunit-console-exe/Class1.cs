// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.ConsoleRunner
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static int Main(string[] args)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine ("Note: nunit-console shipped with Mono is deprecated, please use the NUnit NuGet package or some other form of acquiring NUnit.");
			Console.ResetColor ();
			return Runner.Main( args );
		}
	}
}
