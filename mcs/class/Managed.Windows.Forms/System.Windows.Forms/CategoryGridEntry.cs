using System;
using System.Drawing;

namespace System.Windows.Forms.PropertyGridInternal
{
	/// <summary>
	/// Summary description for CategoryGridEntry.
	/// </summary>
	internal class CategoryGridEntry : GridEntry
	{
		private string label;
		public CategoryGridEntry(string category)
		{
			label = category;
		}

		public override GridItemType GridItemType {
			get {
				return GridItemType.Category;
			}
		}


		public override string Label {
			get {
				return label;
			}
		}
	}
}
