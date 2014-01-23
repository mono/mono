//
// System.ComponentModel.ToolboxItemAttribute test cases
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2005 Novell

using System;
using System.ComponentModel;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel 
{
	[TestFixture]
	public class ToolboxItemAttributeTests
	{
#if !MOBILE
		[Test]
#if TARGET_JVM
		[Ignore ("TD BUG ID: 7215, 7216")]
#endif
		public void DefaultType ()
		{
			ToolboxItemAttribute attr = new ToolboxItemAttribute (true);
			
			Type toolboxItemType = typeof(global::System.Drawing.Design.ToolboxItem);

			Assert.AreEqual (toolboxItemType.AssemblyQualifiedName, attr.ToolboxItemTypeName, "#1");
			Assert.AreEqual (toolboxItemType, attr.ToolboxItemType, "#2");
			Assert.AreEqual (true, attr.IsDefaultAttribute (), "#3");
			Assert.AreEqual (attr.ToolboxItemTypeName.GetHashCode (), attr.GetHashCode (), "#4");

			Assert.AreEqual (toolboxItemType.AssemblyQualifiedName, ToolboxItemAttribute.Default.ToolboxItemTypeName, "#5");
			Assert.AreEqual (toolboxItemType, ToolboxItemAttribute.Default.ToolboxItemType, "#2");
			Assert.AreEqual (true, ToolboxItemAttribute.Default.IsDefaultAttribute (), "#3");
			Assert.AreEqual (ToolboxItemAttribute.Default.ToolboxItemTypeName.GetHashCode (), attr.GetHashCode (), "#4");
		}
#endif
		[Test]
		public void NonDefaultType ()
		{
			ToolboxItemAttribute attr = new ToolboxItemAttribute (false);
			Assert.AreEqual (string.Empty, attr.ToolboxItemTypeName, "#1");
			Assert.IsNull (attr.ToolboxItemType, "#2");
			Assert.AreEqual (false, attr.IsDefaultAttribute (), "#3");

			Assert.AreEqual (string.Empty, ToolboxItemAttribute.None.ToolboxItemTypeName, "#4");
			Assert.IsNull (ToolboxItemAttribute.None.ToolboxItemType, "#5");
			Assert.AreEqual (false, ToolboxItemAttribute.None.IsDefaultAttribute (), "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidItemTypeName ()
		{
			ToolboxItemAttribute attr = new ToolboxItemAttribute ("typedoesnotexist");
			// this next statement should fail
			Type type = attr.ToolboxItemType;
		}
	}
}
