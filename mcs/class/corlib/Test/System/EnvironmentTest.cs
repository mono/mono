//
// EnvironmentTest.cs - NUnit Test Cases for Environment
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2004-2005 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Collections;

namespace MonoTests.System {

	[TestFixture]
	public class EnvironmentTest {

		private void ExpandEquals (string toExpand, string toMatch) 
		{
			string expanded = Environment.ExpandEnvironmentVariables (toExpand);
			Assert.AreEqual (toMatch, expanded, "ExpandEnvironmentVariables(" + toExpand + ").Match");
		}

		private void ExpandStartsEnds (string toExpand, string start, string end) 
		{
			string expanded = Environment.ExpandEnvironmentVariables (toExpand);
			Assert.IsTrue (expanded.StartsWith (start), "ExpandEnvironmentVariables(" + toExpand + ").Start");
			Assert.IsTrue (expanded.EndsWith (end), "ExpandEnvironmentVariables(" + toExpand + ").End");
		}

		private void ExpandDifferent (string toExpand)
		{
			string expanded = Environment.ExpandEnvironmentVariables (toExpand);
			Assert.IsFalse ((toExpand == expanded), "ExpandEnvironmentVariables(" + toExpand + ").Different");
		}

		[Test]
		public void ExpandEnvironmentVariables_UnknownVariable () 
		{
			ExpandEquals ("Hello %UNKNOWN_ENV_VAR% :-)", "Hello %UNKNOWN_ENV_VAR% :-)");
		}

		[Test]
		public void ExpandEnvironmentVariables_KnownVariable () 
		{
			ExpandStartsEnds ("Path %PATH% :-)", "Path ", " :-)");
		}

		[Test]
		public void ExpandEnvironmentVariables_NotVariable () 
		{
			ExpandEquals ("100% :-)", "100% :-)");
		}
		
		[Test]
		public void ExpandEnvironmentVariables_Alone () 
		{
			ExpandDifferent ("%PATH%");
		}

		[Test]
		public void ExpandEnvironmentVariables_End () 
		{
			ExpandStartsEnds ("Hello %PATH%", "Hello ", "");
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
			ExpandDifferent ("%PATH%%PATH%");
			string path = Environment.GetEnvironmentVariable ("PATH");
			if (path != null) {
				string expanded = Environment.ExpandEnvironmentVariables ("%PATH%%PATH%");
				Assert.AreEqual (path + path, expanded, "#01");
			}
		}
		
		[Test]
		public void ExpandEnvironmentVariables_ComplexExpandable () 
		{
			ExpandStartsEnds ("Hello %%%PATH%%%", "Hello %%", "%%");
		}

		[Test]
		public void ExpandEnvironmentVariables_ExpandableAndNonExpandable () 
		{
			string path = Environment.GetEnvironmentVariable ("PATH");
			string expanded=Environment.ExpandEnvironmentVariables("%PATH% PATH%");
			Assert.AreEqual (path + " PATH%", expanded);
		}


		[Test]
		public void ExpandEnvironmentVariables_ExpandableWithTrailingPercent () 
		{
			string path = Environment.GetEnvironmentVariable ("PATH");
			string expanded=Environment.ExpandEnvironmentVariables("%PATH% %");
			Assert.AreEqual (path+" %",expanded);
		}
		
		[Test]
		public void ExpandEnvironmentVariables_ComplexExpandable2 () 
		{
			ExpandStartsEnds ("Hello %%PATH%%%", "Hello %", "%%");
		}

		[Test]
		public void GetEnvironmentVariables ()
		{
			IDictionary d = Environment.GetEnvironmentVariables ();
			Assert.IsTrue ((d is Hashtable), "Hashtable");
			Assert.IsFalse (d.IsFixedSize, "IsFixedSize");
			Assert.IsFalse (d.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (d.IsSynchronized, "IsSynchronized");
		}

		[Test]
		public void GetCommandLineArgs ()
		{
			string[] args = Environment.GetCommandLineArgs ();
			Assert.IsNotNull (args, "not null");
			Assert.IsTrue (((args.Length > 0) && (args.Length < 256)), "reasonable");
			Assert.IsNotNull (args [0], "application");
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnvironmentVariable_Target ()
		{
			Environment.GetEnvironmentVariable ("MONO", (EnvironmentVariableTarget)Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnvironmentVariables_Target ()
		{
			Environment.GetEnvironmentVariables ((EnvironmentVariableTarget)Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetEnvironmentVariable_Target ()
		{
			Environment.SetEnvironmentVariable ("MONO", "GO", (EnvironmentVariableTarget)Int32.MinValue);
		}
#endif
	}
}
