//
// System.ComponentModel.ToolboxItemAttribute
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

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

		public override bool IsDefaultAttribute ()
		{
			return Equals (Default);
		}
	}
}

