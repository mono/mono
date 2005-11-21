//
// RegexCas.cs - CAS unit tests for System.Text.RegularExpressions.Regex
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace MonoCasTests.System.Text.RegularExpressions {

	class TestRegex: Regex {

		public TestRegex ()
		{
			// note: test protected default ctor
		}

		public void TestProtectedStuff ()
		{
			pattern = String.Empty;
			roptions = RegexOptions.None;

			try {
				InitializeReferences ();
			}
			catch (NotImplementedException) {
				// mono
			}

			Assert.IsFalse (UseOptionC (), "UseOptionC");
			Assert.IsFalse (UseOptionR (), "UseOptionR");

			Assert.IsNull (capnames, "capnames");
			Assert.IsNull (caps, "caps");
			Assert.AreEqual (0, capsize, "capsize");
			Assert.IsNull (capslist, "capslist");
			Assert.IsNull (factory, "factory");
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class RegexCas {

		private AssemblyName aname;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			aname = new AssemblyName ();
			aname.Name = "Cas.Test.RegularExpressions";
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private string Evaluator (Match match)
		{
			return String.Empty;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Static_Deny_Unrestricted ()
		{
			Assert.AreEqual (String.Empty, Regex.Escape (String.Empty), "Escape");
			Assert.AreEqual (String.Empty, Regex.Unescape (String.Empty), "Unescape");
			
			Assert.IsTrue (Regex.IsMatch (String.Empty, String.Empty), "IsMatch");
			Assert.IsTrue (Regex.IsMatch (String.Empty, String.Empty, RegexOptions.Singleline), "IsMatch2");
			
			Assert.IsNotNull (Regex.Match (String.Empty, String.Empty), "Match");
			Assert.IsNotNull (Regex.Match (String.Empty, String.Empty, RegexOptions.Singleline), "Match2");

			Assert.AreEqual (1, Regex.Matches (String.Empty, String.Empty).Count, "Matches");
			Assert.AreEqual (1, Regex.Matches (String.Empty, String.Empty, RegexOptions.Singleline).Count, "Matches2");

			Assert.AreEqual (String.Empty, Regex.Replace (String.Empty, String.Empty, new MatchEvaluator (Evaluator)), "Replace");
			Assert.AreEqual (String.Empty, Regex.Replace (String.Empty, String.Empty, new MatchEvaluator (Evaluator), RegexOptions.Singleline), "Replace2");
			Assert.AreEqual (String.Empty, Regex.Replace (String.Empty, String.Empty, String.Empty), "Replace3");
			Assert.AreEqual (String.Empty, Regex.Replace (String.Empty, String.Empty, String.Empty, RegexOptions.Singleline), "Replace4");

			Assert.AreEqual (2, Regex.Split (String.Empty, String.Empty).Length, "Split");
			Assert.AreEqual (2, Regex.Split (String.Empty, String.Empty, RegexOptions.Singleline).Length, "Split2");
#if NET_2_0
			Assert.AreEqual (15, Regex.CacheSize, "CacheSize");
			Regex.CacheSize = 1;
#endif
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		[Category ("NotWorking")]
		public void CompileToAssembly_Deny_ControlEvidence ()
		{
			RegexCompilationInfo info = new RegexCompilationInfo (String.Empty, RegexOptions.None, "name", String.Empty, false);
			Regex.CompileToAssembly (new RegexCompilationInfo[1] { info }, aname, null, null);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		[Category ("NotWorking")]
		public void CompileToAssembly_Deny_FileIOPermission ()
		{
			RegexCompilationInfo info = new RegexCompilationInfo (String.Empty, RegexOptions.None, "name", String.Empty, false);
			Regex.CompileToAssembly (new RegexCompilationInfo[1] { info }, aname, null, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[Category ("NotWorking")]
		public void CompileToAssembly_PermitOnly_ControlEvidence ()
		{
			RegexCompilationInfo info = new RegexCompilationInfo (String.Empty, RegexOptions.None, "name", String.Empty, false);
			Regex.CompileToAssembly (new RegexCompilationInfo[1] { info }, aname, null, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Instance_Deny_Unrestricted ()
		{
			Regex r = new Regex (String.Empty);
			Assert.AreEqual (RegexOptions.None, r.Options, "Options");
			Assert.IsFalse (r.RightToLeft, "RightToLeft");

			Assert.AreEqual (1, r.GetGroupNames ().Length, "GetGroupNames");
			Assert.AreEqual (1, r.GetGroupNumbers ().Length, "GetGroupNumbers");
			Assert.AreEqual ("0", r.GroupNameFromNumber (0), "GroupNameFromNumber");
			Assert.AreEqual (0, r.GroupNumberFromName ("0"), "GroupNumberFromName");

			Assert.IsTrue (r.IsMatch (String.Empty), "IsMatch");
			Assert.IsTrue (r.IsMatch (String.Empty, 0), "IsMatch2");

			Assert.IsNotNull (r.Match (String.Empty), "Match");
			Assert.IsNotNull (r.Match (String.Empty, 0), "Match2");
			Assert.IsNotNull (r.Match (String.Empty, 0, 0), "Match3");

			Assert.AreEqual (1, r.Matches (String.Empty).Count, "Matches");
			Assert.AreEqual (1, r.Matches (String.Empty, 0).Count, "Matches2");

			Assert.AreEqual (String.Empty, r.Replace (String.Empty, new MatchEvaluator (Evaluator)), "Replace");
			Assert.AreEqual (String.Empty, r.Replace (String.Empty, new MatchEvaluator (Evaluator), 0), "Replace2");
			Assert.AreEqual (String.Empty, r.Replace (String.Empty, new MatchEvaluator (Evaluator), 0, 0), "Replace3");
			Assert.AreEqual (String.Empty, r.Replace (String.Empty, String.Empty), "Replace4");
			Assert.AreEqual (String.Empty, r.Replace (String.Empty, String.Empty, 0), "Replace5");
			Assert.AreEqual (String.Empty, r.Replace (String.Empty, String.Empty, 0, 0), "Replace6");

			Assert.AreEqual (2, r.Split (String.Empty).Length, "Split");
			Assert.AreEqual (2, r.Split (String.Empty, 0).Length, "Split2");
			Assert.AreEqual (2, r.Split (String.Empty, 0, 0).Length, "Split3");

			Assert.AreEqual (String.Empty, r.ToString (), "ToString");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Protected_Deny_Unrestricted ()
		{
			TestRegex r = new TestRegex ();
			r.TestProtectedStuff ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Serialization_Deny_SerializationFormatter ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();

			Regex r1 = new Regex (String.Empty, RegexOptions.Singleline);
			bf.Serialize (ms, r1);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		public void Serialization_PermitOnly_SerializationFormatter ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();

			Regex r1 = new Regex (String.Empty, RegexOptions.Singleline);
			bf.Serialize (ms, r1);

			ms.Position = 0;
			Regex r2 = (Regex) bf.Deserialize (ms);

			Assert.AreEqual (r1.Options, r2.Options, "Options");
			Assert.AreEqual (r1.ToString (), r2.ToString (), "ToString");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Type[] types = new Type[1] { typeof (string) };
			ConstructorInfo ci = typeof (Regex).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor (string)");
			Assert.IsNotNull (ci.Invoke (new object[1] { String.Empty }), "invoke");
		}
	}
}
