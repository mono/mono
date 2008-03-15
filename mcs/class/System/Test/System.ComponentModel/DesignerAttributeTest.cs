//
// System.ComponentModel.DesignerAttribute test cases
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net
//
// (c) 2008 Gert Driesen
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel 
{
	[TestFixture]
	public class DesignerAttributeTest
	{
		[Test] // ctor (String)
		public void Constructor1 ()
		{
			DesignerAttribute da;

			da = new DesignerAttribute ("CategoryType");
			Assert.AreEqual (typeof (IDesigner).FullName, da.DesignerBaseTypeName, "#A1");
			Assert.AreEqual ("CategoryType", da.DesignerTypeName, "#A2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + da.DesignerBaseTypeName, da.TypeId, "#A3");

			da = new DesignerAttribute ("Mono.Components.CategoryType");
			Assert.AreEqual (typeof (IDesigner).FullName, da.DesignerBaseTypeName, "#B1");
			Assert.AreEqual ("Mono.Components.CategoryType", da.DesignerTypeName, "#B2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + da.DesignerBaseTypeName, da.TypeId, "#B3");

			da = new DesignerAttribute (string.Empty);
			Assert.AreEqual (typeof (IDesigner).FullName, da.DesignerBaseTypeName, "#C1");
			Assert.AreEqual (string.Empty, da.DesignerTypeName, "#C2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + da.DesignerBaseTypeName, da.TypeId, "#C3");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor1_DesignerTypeName_Null ()
		{
			new DesignerAttribute ((string) null);
		}

		[Test] // ctor (Type)
		public void Constructor2 ()
		{
			DesignerAttribute da = new DesignerAttribute (typeof (string));
			Assert.AreEqual (typeof (IDesigner).FullName, da.DesignerBaseTypeName, "#1");
			Assert.AreEqual (typeof (string).AssemblyQualifiedName, da.DesignerTypeName, "#2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + da.DesignerBaseTypeName, da.TypeId, "#3");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor2_DesignerType_Null ()
		{
			new DesignerAttribute ((Type) null);
		}

		[Test] // ctor (String, String)
		public void Constructor3 ()
		{
			DesignerAttribute da;

			da = new DesignerAttribute ("Mono.Components.CategoryType", "Mono.Design.CompositeAttribute");
			Assert.AreEqual ("Mono.Design.CompositeAttribute", da.DesignerBaseTypeName, "#A1");
			Assert.AreEqual ("Mono.Components.CategoryType", da.DesignerTypeName, "#A2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + da.DesignerBaseTypeName, da.TypeId, "#A3");

			da = new DesignerAttribute ("CategoryType", "CompositeAttribute");
			Assert.AreEqual ("CompositeAttribute", da.DesignerBaseTypeName, "#B1");
			Assert.AreEqual ("CategoryType", da.DesignerTypeName, "#B2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + da.DesignerBaseTypeName, da.TypeId, "#B3");

			da = new DesignerAttribute (string.Empty, string.Empty);
			Assert.AreEqual (string.Empty, da.DesignerBaseTypeName, "#C1");
			Assert.AreEqual (string.Empty, da.DesignerTypeName, "#C2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName, da.TypeId, "#C3");
		}

		[Test]
		public void Constructor3_DesignerBaseType_Null ()
		{
			DesignerAttribute da = new DesignerAttribute (
				"CategoryType", (string) null);
			Assert.IsNull (da.DesignerBaseTypeName, "#1");
			Assert.AreEqual ("CategoryType", da.DesignerTypeName, "#2");
			try {
				object typeId = da.TypeId;
				Assert.Fail ("#3: " + typeId);
			} catch (NullReferenceException) {
			}
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor3_DesignerTypeName_Null ()
		{
			new DesignerAttribute ((string) null, "Mono.Design.CompositeAttribute");
		}

		[Test] // ctor (String, Type)
		public void Constructor4 ()
		{
			DesignerAttribute da;

			da = new DesignerAttribute ("Mono.Components.CategoryType", typeof (string));
			Assert.AreEqual (typeof (string).AssemblyQualifiedName, da.DesignerBaseTypeName, "#A1");
			Assert.AreEqual ("Mono.Components.CategoryType", da.DesignerTypeName, "#A2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + typeof (string).FullName, da.TypeId, "#A3");

			da = new DesignerAttribute ("CategoryType", typeof (int));
			Assert.AreEqual (typeof (int).AssemblyQualifiedName, da.DesignerBaseTypeName, "#B1");
			Assert.AreEqual ("CategoryType", da.DesignerTypeName, "#B2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + typeof (int).FullName, da.TypeId, "#B3");

			da = new DesignerAttribute (string.Empty, typeof (string));
			Assert.AreEqual (typeof (string).AssemblyQualifiedName, da.DesignerBaseTypeName, "#C1");
			Assert.AreEqual (string.Empty, da.DesignerTypeName, "#C2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + typeof (string).FullName, da.TypeId, "#C3");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor4_DesignerBaseTypeName_Null ()
		{
			new DesignerAttribute ("CategoryType", (Type) null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor4_DesignerTypeName_Null ()
		{
			new DesignerAttribute ((string) null, typeof (string));
		}

		[Test] // ctor (Type, Type)
		public void Constructor5 ()
		{
			DesignerAttribute da;

			da = new DesignerAttribute (typeof (int), typeof (string));
			Assert.AreEqual (typeof (string).AssemblyQualifiedName, da.DesignerBaseTypeName, "#A1");
			Assert.AreEqual (typeof (int).AssemblyQualifiedName, da.DesignerTypeName, "#A2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + typeof (string).FullName, da.TypeId, "#A3");

			da = new DesignerAttribute (typeof (string), typeof (int));
			Assert.AreEqual (typeof (int).AssemblyQualifiedName, da.DesignerBaseTypeName, "#B1");
			Assert.AreEqual (typeof (string).AssemblyQualifiedName, da.DesignerTypeName, "#B2");
			Assert.AreEqual (typeof (DesignerAttribute).FullName + typeof (int).FullName, da.TypeId, "#B3");
		}

		[Test] // ctor (Type, Type)
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor5_DesignerBaseTypeName_Null ()
		{
			new DesignerAttribute (typeof (string), (Type) null);
		}

		[Test] // ctor (Type, Type)
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor5_DesignerTypeName_Null ()
		{
			new DesignerAttribute ((Type) null, typeof (string));
		}
	}
}
