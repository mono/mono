//
// System.ComponentModel.ToolboxItemFilterAttribute
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	[Serializable]
	public sealed class ToolboxItemFilterAttribute : Attribute
	{
		private string Filter;
		private ToolboxItemFilterType ItemFilterType;

		public ToolboxItemFilterAttribute (string filterString)
		{
			Filter = filterString;
			ItemFilterType = ToolboxItemFilterType.Allow;
		}

		public ToolboxItemFilterAttribute (string filterString, ToolboxItemFilterType filterType)
		{
			Filter = filterString;
			ItemFilterType = filterType;
		}

		public string FilterString {
			get { return Filter; }
		}

		public ToolboxItemFilterType FilterType {
			get { return ItemFilterType; }
		}

		public override object TypeId {
			get { return base.TypeId + Filter; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ToolboxItemFilterAttribute))
				return false;
			if (obj == this)
				return true;
			return (((ToolboxItemFilterAttribute) obj).FilterString == Filter) &&
				(((ToolboxItemFilterAttribute) obj).FilterType == ItemFilterType);
		}

		public override int GetHashCode()
		{
			return (Filter + ItemFilterType.ToString()).GetHashCode ();
		}

		public override bool Match (object obj)
		{
			if (!(obj is ToolboxItemFilterAttribute))
				return false;
			return ((ToolboxItemFilterAttribute) obj).FilterString == Filter;
		}
	}
}
