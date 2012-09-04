//
// EnvironmentTest.cs - NUnit Test Cases for Environment
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2004-2005 Novell (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
//

using System;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class EnvironmentTest
	{
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
		// Bug #5169
		public void ExpandEnvironmentVariables_ExpandMultiple ()
		{
			string path = Environment.GetEnvironmentVariable ("PATH");
			var expected = "%TEST123" + path + "TEST456%";
			ExpandEquals ("%TEST123%PATH%TEST456%", expected);
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

#if NET_2_0 && !TARGET_JVM && !NET_2_1
		[Test] // bug #333740
		public void GetEnvironmentVariables_NewlySet ()
		{
			Environment.SetEnvironmentVariable ("MonoTestVariable", "TestValue");
			IDictionary d = Environment.GetEnvironmentVariables ();
			Assert.AreEqual ("TestValue", d ["MonoTestVariable"], "#1");
			Environment.SetEnvironmentVariable ("MonoTestVariable", string.Empty);
			Assert.AreEqual ("TestValue", d ["MonoTestVariable"], "#2");
			d = Environment.GetEnvironmentVariables ();
			Assert.IsNull (d ["MonoTestVariable"], "#3");
		}
#endif

		[Test]
		public void GetCommandLineArgs ()
		{
			string[] args = Environment.GetCommandLineArgs ();
			Assert.IsNotNull (args, "not null");
			Assert.IsTrue (((args.Length > 0) && (args.Length < 256)), "reasonable");
			Assert.IsNotNull (args [0], "application");
		}

#if NET_2_0 && !NET_2_1
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnvironmentVariable_Target_Invalid ()
		{
			Environment.GetEnvironmentVariable ("MONO", (EnvironmentVariableTarget)Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnvironmentVariables_Target_Invalid ()
		{
			Environment.GetEnvironmentVariables ((EnvironmentVariableTarget)Int32.MinValue);
		}

#if !TARGET_JVM // Environment.SetEnvironmentVariable not supported under TARGET_JVM
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetEnvironmentVariable_Target_Invalid ()
		{
			Environment.SetEnvironmentVariable ("MONO", "GO", (EnvironmentVariableTarget)Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetEnvironmentVariable_Name_Null ()
		{
			Environment.SetEnvironmentVariable (null, "A");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetEnvironmentVariable_Name_Empty ()
		{
			Environment.SetEnvironmentVariable ("", "A");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetEnvironmentVariable_Name_ZeroChar ()
		{
			Environment.SetEnvironmentVariable ("\0", "A");
		}

		[Test]
		public void SetEnvironmentVariable ()
		{
			// Test set
			Environment.SetEnvironmentVariable ("A1", "B1");
			Environment.SetEnvironmentVariable ("A2", "B2");
			Environment.SetEnvironmentVariable ("A3", "B3");
			Assert.AreEqual (Environment.GetEnvironmentVariable ("A1"), "B1");
			Assert.AreEqual (Environment.GetEnvironmentVariable ("A2"), "B2");
			Assert.AreEqual (Environment.GetEnvironmentVariable ("A3"), "B3");

			// Test update
			Environment.SetEnvironmentVariable ("A3", "B4");
			Assert.AreEqual (Environment.GetEnvironmentVariable ("A3"), "B4");

			// Test delete
			Environment.SetEnvironmentVariable ("A1", null);
			Assert.IsNull (Environment.GetEnvironmentVariables ()["A1"]);
			Environment.SetEnvironmentVariable ("A2", "");
			Assert.IsNull (Environment.GetEnvironmentVariables ()["A2"]);
			Environment.SetEnvironmentVariable ("A3", "\0");
			Assert.IsNull (Environment.GetEnvironmentVariables ()["A3"]);
		}
#endif // TARGET_JVM
#endif
	}
}
