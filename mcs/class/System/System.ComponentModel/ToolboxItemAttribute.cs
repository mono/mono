//
// System.ComponentModel.ToolboxItemAttribute
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public class ToolboxItemAttribute : Attribute
	{
		private static string defaultItemType = "System.Drawing.Design.ToolboxItem,System.Drawing";

		public static readonly ToolboxItemAttribute Default = new ToolboxItemAttribute (defaultItemType);
		public static readonly ToolboxItemAttribute None = new ToolboxItemAttribute (false);

		private Type itemType;
		private string itemTypeName;

		public ToolboxItemAttribute (bool defaultType)
		{
			if (defaultType)
				itemTypeName = defaultItemType;
		}

		public ToolboxItemAttribute (string toolboxItemName)
		{
			itemTypeName = toolboxItemName;
		}

		public ToolboxItemAttribute (Type toolboxItemType)
		{
			itemType = toolboxItemType;
		}

		public Type ToolboxItemType
		{
			get {
				if (itemType == null && itemTypeName != null)
					itemType = Type.GetType (itemTypeName);
				return itemType;
			}
		}
		
		public string ToolboxItemTypeName
		{
			get {
				if (itemTypeName == null) {
					if (itemType == null)
						return "";
					itemTypeName = itemType.AssemblyQualifiedName;
				}

				return itemTypeName;
			}
		}
		
		public override bool Equals (object o)
		{
			if (!(o is ToolboxItemAttribute))
				return false;

			return (((ToolboxItemAttribute) o).ToolboxItemTypeName == ToolboxItemTypeName);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public bool IsDefaultAttribute ()
		{
			return Equals (Default);
		}
	}
}

