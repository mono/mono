//
// EnvironmentTest.cs - NUnit Test Cases for Environment
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;

namespace MonoTests.System {

	[TestFixture]
	public class EnvironmentTest : Assertion {

		private void ExpandEquals (string toExpand, string toMatch) 
		{
			string expanded = Environment.ExpandEnvironmentVariables (toExpand);
			AssertEquals ("ExpandEnvironmentVariables(" + toExpand + ").Match", toMatch, expanded);
		}

		private void ExpandStartsEnds (string toExpand, string start, string end) 
		{
			string expanded = Environment.ExpandEnvironmentVariables (toExpand);
			Assert ("ExpandEnvironmentVariables(" + toExpand + ").Start", expanded.StartsWith (start));
			Assert ("ExpandEnvironmentVariables(" + toExpand + ").End", expanded.EndsWith (end));
		}

		private void ExpandDifferent (string toExpand)
		{
			string expanded = Environment.ExpandEnvironmentVariables (toExpand);
			Assert ("ExpandEnvironmentVariables(" + toExpand + ").Different", (toExpand != expanded));
		}

		[Test]
		public void ExpandEnvironmentVariables_UnknownVariable () 
		{
			ExpandEquals ("Hello %UNKNOWN_ENV_VAR% :-)", "Hello %UNKNOWN_ENV_VAR% :-)");
		}

		[Test]
		public void ExpandEnvironmentVariables_KnownVariable () 
		{
			ExpandStartsEnds ("Path %Path% :-)", "Path ", " :-)");
		}

		[Test]
		public void ExpandEnvironmentVariables_NotVariable () 
		{
			ExpandEquals ("100% :-)", "100% :-)");
		}
		
		[Test]
		public void ExpandEnvironmentVariables_Alone () 
		{
			ExpandDifferent ("%Path%");
		}

		[Test]
		public void ExpandEnvironmentVariables_End () 
		{
			ExpandStartsEnds ("Hello %Path%", "Hello ", "");
		}

		[Test]
		public void ExpandEnvironmentVariables_None () 
		{
			ExpandEquals ("Hello Mono", "Hello Mono");
		}

		[Test]
		public void ExpandEnvironmentVariables_EmptyVariable () 
		{
			ExpandEquals ("Hello %% Mono", "Hello %% Mono");
		}

		[Test]
		public void ExpandEnvironmentVariables_Double () 
		{
			ExpandDifferent ("%Path%%Path%");
		}
		
		[Test]
		public void ExpandEnvironmentVariables_ComplexExpandable () 
		{
			ExpandStartsEnds ("Hello %%%Path%%%", "Hello %%", "%%");
		}

		[Test]
		public void ExpandEnvironmentVariables_ComplexExpandable2 () 
		{
			ExpandStartsEnds ("Hello %%Path%%%", "Hello %", "%%");
		}
	}
}
