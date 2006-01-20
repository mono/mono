//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
// Authors:
//
//	Jordi Mas i Hernandez, jordimash@gmail.com
//

using System;
using System.Collections;
using System.Reflection;
using System.Drawing;
using System.Drawing.Design;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Design
{
	[TestFixture]
	public class TestToolboxItem
	{
		class OurToolboxItem: ToolboxItem
		{
			public OurToolboxItem () {}
			public void _CheckUnlocked () 
			{ 
				CheckUnlocked (); 
			}
		}

		[TearDown]
		public void Clean() {}

		[SetUp]
		public void GetReady()
		{

		}

		[Test]
		public void TestProperties  ()
		{
			ToolboxItem item = new ToolboxItem ();
			AssemblyName name = new AssemblyName ();
			name.Name = "OurAssembly";
			item.AssemblyName = name;
			Assert.AreEqual (name.Name.ToString (), "OurAssembly", "TP#1");

			item.TypeName = "TypeName1";
			Assert.AreEqual ("TypeName1", item.TypeName, "TP#2");

			item.DisplayName = "ShowName";
			Assert.AreEqual (item.DisplayName, "ShowName", "TP#3");

			item.TypeName = "TypeNameSt";
			Assert.AreEqual (item.TypeName, "TypeNameSt", "TP#4");
			
			Bitmap bmp = new Bitmap (200, 200);
			item.Bitmap = bmp;
			Assert.AreEqual (bmp, item.Bitmap, "TP#5");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestCheckUnlocked1 ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item.Lock ();
			item._CheckUnlocked ();
		}

		[Test]		
		public void TestCheckUnlocked2 ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._CheckUnlocked ();
		}

#if NET_2_0
		[Test]
		public void TestNewProperties  ()
		{
			ToolboxItem item = new ToolboxItem ();
			item.Company = "OurCompany";
			Assert.AreEqual ("OurCompany", item.Company, "TNP#1");

			Assert.AreEqual ("DotNET_ComponentType", item.ComponentType, "TNP#2");

			item.Description = "Description";
			Assert.AreEqual ("Description", item.Description, "TNP#3");

			item.IsTransient = true;
			Assert.AreEqual (true, item.IsTransient, "TNP#4");
			
		}
#endif


	}
}


