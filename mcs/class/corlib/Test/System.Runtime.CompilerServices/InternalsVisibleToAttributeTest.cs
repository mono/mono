//
// InternalsVisibleToAttributeTest.cs - Unit tests for
// System.Runtime.CompilerServices.InternalsVisibleToAttribute
//
// Author:
//	Gert Driesen <drieseng@users.sourceforge.net>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Runtime.CompilerServices;

using NUnit.Framework;

namespace MonoTests.System.Runtime.CompilerServices {
	[TestFixture]
	public class InternalsVisibleToAttributeTest {
		[Test] // .ctor (String)
		public void Constructor1 ()
		{
			InternalsVisibleToAttribute ivt;
			string aname;

			aname = "Mono";
			ivt = new InternalsVisibleToAttribute (aname);
			Assert.IsTrue (ivt.AllInternalsVisible, "#A:AllInternalsVisible");
			Assert.AreSame (aname, ivt.AssemblyName, "#A:AssemblyName");
			Assert.AreEqual (ivt.GetType (), ivt.TypeId, "#A:TypeId");

			aname = typeof (int).Assembly.FullName;
			ivt = new InternalsVisibleToAttribute (aname);
			Assert.IsTrue (ivt.AllInternalsVisible, "#B:AllInternalsVisible");
			Assert.AreSame (aname, ivt.AssemblyName, "#B:AssemblyName");
			Assert.AreEqual (ivt.GetType (), ivt.TypeId, "#B:TypeId");

			aname = string.Empty;
			ivt = new InternalsVisibleToAttribute (aname);
			Assert.IsTrue (ivt.AllInternalsVisible, "#C:AllInternalsVisible");
			Assert.AreSame (aname, ivt.AssemblyName, "#C:AssemblyName");
			Assert.AreEqual (ivt.GetType (), ivt.TypeId, "#C:TypeId");

			aname = null;
			ivt = new InternalsVisibleToAttribute (aname);
			Assert.IsTrue (ivt.AllInternalsVisible, "#D:AllInternalsVisible");
			Assert.IsNull (ivt.AssemblyName, "#D:AssemblyName");
			Assert.AreEqual (ivt.GetType (), ivt.TypeId, "#D:TypeId");
		}

		[Test]
		public void AllInternalsVisible ()
		{
			InternalsVisibleToAttribute ivt = new InternalsVisibleToAttribute ("Mono");
			ivt.AllInternalsVisible = false;
			Assert.IsFalse (ivt.AllInternalsVisible, "#1");
			ivt.AllInternalsVisible = true;
			Assert.IsTrue (ivt.AllInternalsVisible, "#2");
		}
	}
}

#endif
