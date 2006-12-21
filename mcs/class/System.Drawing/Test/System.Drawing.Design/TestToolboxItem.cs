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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Drawing;
using System.Drawing.Design;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Design {

	[TestFixture]
	public class TestToolboxItem {

		private AssemblyName an;
		private Bitmap bitmap;
		private ToolboxItemFilterAttribute[] filter;
		private AssemblyName[] dependent;

		class OurToolboxItem: ToolboxItem {

			public OurToolboxItem ()
			{
			}

			public void _CheckUnlocked () 
			{ 
				CheckUnlocked (); 
			}

			public Type _GetType (IDesignerHost host, AssemblyName an, string tn, bool r)
			{
				return GetType (host, an, tn, r);
			}
#if NET_2_0
			public object Filter (string propertyName, object value)
			{
				return FilterPropertyValue (propertyName, value);
			}

			public void _ValidatePropertyType (string propertyName, object value, Type expectedType, bool allowNull)
			{
				ValidatePropertyType (propertyName, value, expectedType, allowNull);
			}

			public object _ValidatePropertyValue (string propertyName, object value)
			{
				return ValidatePropertyValue (propertyName, value);
			}
#endif
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			bitmap = new Bitmap (20, 20);
			an = new AssemblyName ();
			filter = new ToolboxItemFilterAttribute[0];
			dependent = new AssemblyName[0];
		}

		[Test]
		public void DefaultValues ()
		{
			ToolboxItem item = new ToolboxItem ();
			Assert.IsNull (item.AssemblyName, "AssemblyName");
			Assert.IsNull (item.Bitmap, "Bitmap");
			Assert.AreEqual (String.Empty, item.DisplayName, "DisplayName");
			Assert.AreEqual (typeof (ToolboxItemFilterAttribute[]), item.Filter.GetType (), "Filter/Type");
			Assert.AreEqual (0, item.Filter.Count, "Filter");
			Assert.AreEqual (String.Empty, item.TypeName, "TypeName");
#if NET_2_0
			Assert.IsNull (item.Company, "Company");
			Assert.AreEqual (".NET Component", item.ComponentType, "ComponentType");
			Assert.IsNull (item.DependentAssemblies, "DependentAssemblies");
			Assert.IsNull (item.Description, "Description");
			Assert.IsFalse (item.IsTransient, "IsTransient");
			Assert.AreEqual (0, item.Properties.Count, "Properties");
			Assert.AreEqual (String.Empty, item.Version, "Version");
#endif
		}

		[Test]
		public void NullValues ()
		{
			ToolboxItem item = new ToolboxItem ();
			item.AssemblyName = null;
			Assert.IsNull (item.AssemblyName, "AssemblyName");
			item.Bitmap = null;
			Assert.IsNull (item.Bitmap, "Bitmap");
			item.DisplayName = null;
			Assert.AreEqual (String.Empty, item.DisplayName, "DisplayName");
			item.Filter = null;
			Assert.AreEqual (0, item.Filter.Count, "Filter");
			item.TypeName = null;
			Assert.AreEqual (String.Empty, item.TypeName, "TypeName");
#if NET_2_0
			item.Company = null;
			Assert.AreEqual (String.Empty, item.Company, "Company");
			// can't assign null without a NRE
			item.DependentAssemblies = new AssemblyName [0];
			Assert.AreEqual (0, item.DependentAssemblies.Length, "DependentAssemblies");
			item.Description = null;
			Assert.AreEqual (String.Empty, item.Description, "Description");
			item.IsTransient = true;
			Assert.IsTrue (item.IsTransient, "IsTransient");
			Assert.AreEqual (9, item.Properties.Count, "Properties");
			item.Lock ();
			Assert.AreEqual (9, item.Properties.Count, "Properties/Lock");
#endif
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

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetType_Null_Null_Null_False ()
		{
			new OurToolboxItem ()._GetType (null, null, null, false);
		}

		[Test]
		public void GetType_Null_Null_String_False ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			Assert.IsNull (item._GetType (null, null, "string", false), "GetType");
		}

		[Test]
		public void Initialize_Null ()
		{
			ToolboxItem item = new ToolboxItem ();
			item.Initialize (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Initialize_Locked_Null ()
		{
			ToolboxItem item = new ToolboxItem ();
			item.Lock ();
			item.Initialize (null);
		}

		[Test]
		public void Locked_Twice ()
		{
			ToolboxItem item = new ToolboxItem ();
			item.Lock ();
			item.Lock ();
		}

#if NET_2_0
		[Test]
		public void TestNewProperties  ()
		{
			ToolboxItem item = new ToolboxItem ();
			item.Company = "OurCompany";
			Assert.AreEqual ("OurCompany", item.Company, "TNP#1");

			Assert.AreEqual (".NET Component", item.ComponentType, "TNP#2");

			item.Description = "Description";
			Assert.AreEqual ("Description", item.Description, "TNP#3");

			item.IsTransient = true;
			Assert.AreEqual (true, item.IsTransient, "TNP#4");
			
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void DependentAssemblies_Null ()
		{
			new ToolboxItem ().DependentAssemblies = null;
		}

		[Test]
		public void DependentAssemblies_Empty ()
		{
			ToolboxItem item = new ToolboxItem ();
			item.DependentAssemblies = new AssemblyName[0];
			Assert.AreEqual (0, item.DependentAssemblies.Length, "Length");
		}

		[Test]
		public void DependentAssemblies ()
		{
			AssemblyName[] names = new AssemblyName [1];
			names[0] = new AssemblyName ();
			ToolboxItem item = new ToolboxItem ();
			item.DependentAssemblies = names;
			Assert.AreEqual (1, item.DependentAssemblies.Length, "Length");
			Assert.IsTrue (Object.ReferenceEquals (names[0], item.DependentAssemblies[0]), "ReferenceEquals");
			names[0] = null;
			Assert.IsNotNull (item.DependentAssemblies[0], "0");
		}

		[Test]
		public void Filter ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			Assert.IsNull (item.Filter ("AssemblyName", null), "AssemblyName,null");
			// they are "equal" (publicly wise) but have a different hash code
			Assert.AreEqual (typeof (AssemblyName), item.Filter ("AssemblyName", an).GetType (), "AssemblyName,an");

			Assert.IsNull (item.Filter ("Bitmap", null), "Bitmap,null");
			Assert.AreSame (bitmap, item.Filter ("Bitmap", bitmap), "Bitmap,bitmap");

			Assert.AreEqual (String.Empty, item.Filter ("DisplayName", null), "DisplayName,null");
			Assert.AreSame (String.Empty, item.Filter ("DisplayName", String.Empty), "DisplayName,string");

			Assert.AreEqual (filter, item.Filter ("Filter", null), "Filter,null");
			Assert.AreSame (filter, item.Filter ("Filter", filter), "Filter,ToolboxItemFilterAttribute[]");

			Assert.AreEqual (String.Empty, item.Filter ("TypeName", null), "TypeName,null");
			Assert.AreSame (String.Empty, item.Filter ("TypeName", String.Empty), "TypeName,string");

			Assert.IsNull (item.Filter ("Company", null), "Company,null");
			Assert.AreSame (String.Empty, item.Filter ("Company", String.Empty), "Company,string");

			Assert.AreEqual (null, item.Filter ("DependentAssemblies", null), "DependentAssemblies,null");
			// note: not same
			Assert.AreEqual (dependent, item.Filter ("DependentAssemblies", filter), "DependentAssemblies,AssemblyName[]");
			Assert.IsFalse (Object.ReferenceEquals (dependent, item.Filter ("DependentAssemblies", filter)), "DependentAssemblies,AssemblyName[]/Reference");

			Assert.IsNull (item.Filter ("Description", null), "Description,null");
			Assert.AreSame (String.Empty, item.Filter ("Description", String.Empty), "Description,string");

			Assert.IsTrue ((bool) item.Filter ("IsTransient", true), "IsTransient,true");
			Assert.IsFalse ((bool) item.Filter ("IsTransient", false), "IsTransient,false");
		}

		[Test]
		public void GetType_Null ()
		{
			ToolboxItem item = new ToolboxItem ();
			Assert.IsNull (item.GetType (null), "GetType(null)");
		}

		[Test]
		public void ValidatePropertyType ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyType ("IsTransient", true, typeof (bool), false);
			item._ValidatePropertyType ("IsTransient", String.Empty, typeof (string), false);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ValidatePropertyType_Type_Null ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyType ("IsTransient", true, null, false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidatePropertyType_IsTransient_WrongType ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyType ("IsTransient", new object (), typeof (bool), false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ValidatePropertyType_DontAllowNull ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyType ("IsTransient", null, typeof (bool), false);
		}

		[Test]
		public void ValidatePropertyValue ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			object o = new object ();
			Assert.IsNull (item._ValidatePropertyValue (null, null), "null,null");
			Assert.AreSame (o, item._ValidatePropertyValue (null, o), "null,object");
			Assert.IsNull (item._ValidatePropertyValue ("string", null), "string,null");
			Assert.AreSame (o, item._ValidatePropertyValue ("string", o), "string,object");

			Assert.IsNull (item._ValidatePropertyValue ("AssemblyName", null), "AssemblyName,null");
			Assert.AreSame (an, item._ValidatePropertyValue ("AssemblyName", an), "AssemblyName,an");

			Assert.IsNull (item._ValidatePropertyValue ("Bitmap", null), "Bitmap,null");
			Assert.AreSame (bitmap, item._ValidatePropertyValue ("Bitmap", bitmap), "Bitmap,bitmap");

			Assert.AreEqual (String.Empty, item._ValidatePropertyValue ("DisplayName", null), "DisplayName,null");
			Assert.AreSame (String.Empty, item._ValidatePropertyValue ("DisplayName", String.Empty), "DisplayName,string");

			Assert.AreEqual (filter, item._ValidatePropertyValue ("Filter", null), "Filter,null");
			Assert.AreEqual (filter, item._ValidatePropertyValue ("Filter", filter), "Filter,ToolboxItemFilterAttribute[]");
			//Assert.IsFalse (Object.ReferenceEquals (filter, item._ValidatePropertyValue ("Filter", filter)), "Filter,ToolboxItemFilterAttribute[]/Reference");

			Assert.AreEqual (String.Empty, item._ValidatePropertyValue ("TypeName", null), "TypeName,null");
			Assert.AreSame (String.Empty, item._ValidatePropertyValue ("TypeName", String.Empty), "TypeName,string");

			Assert.AreEqual (String.Empty, item._ValidatePropertyValue ("Company", null), "Company,null");
			Assert.AreSame (String.Empty, item._ValidatePropertyValue ("Company", String.Empty), "Company,string");

			Assert.AreEqual (null, item._ValidatePropertyValue ("DependentAssemblies", null), "DependentAssemblies,null");
			Assert.AreEqual (dependent, item._ValidatePropertyValue ("DependentAssemblies", dependent), "DependentAssemblies,AssemblyName[]");
			Assert.IsTrue (Object.ReferenceEquals (dependent, item._ValidatePropertyValue ("DependentAssemblies", dependent)), "DependentAssemblies,AssemblyName[]/Reference");

			Assert.AreEqual (String.Empty, item._ValidatePropertyValue ("Description", null), "Description,null");
			Assert.AreSame (String.Empty, item._ValidatePropertyValue ("Description", String.Empty), "Description,string");

			Assert.IsTrue ((bool)item._ValidatePropertyValue ("IsTransient", true), "IsTransient,true");
			Assert.IsFalse ((bool)item._ValidatePropertyValue ("IsTransient", false), "IsTransient,false");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidatePropertyValue_AssemblyName_WrongType ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyValue ("AssemblyName", false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidatePropertyValue_Bitmap_WrongType ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyValue ("Bitmap", false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidatePropertyValue_DisplayName_WrongType ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyValue ("DisplayName", false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidatePropertyValue_Filter_WrongType ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyValue ("Filter", false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidatePropertyValue_TypeName_WrongType ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyValue ("TypeName", false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidatePropertyValue_Company_WrongType ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyValue ("Company", false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidatePropertyValue_Description_WrongType ()
		{
			OurToolboxItem item = new OurToolboxItem ();
			item._ValidatePropertyValue ("Description", false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ValidatePropertyValue_IsTransient_Null ()
		{
			// only documented case
			new OurToolboxItem ()._ValidatePropertyValue ("IsTransient", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidatePropertyValue_IsTransient_WrongType ()
		{
			// only documented case
			new OurToolboxItem ()._ValidatePropertyValue ("IsTransient", new object ());
		}
#endif
	}
}
