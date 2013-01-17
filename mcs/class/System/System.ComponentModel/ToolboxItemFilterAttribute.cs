//
// System.ComponentModel.ToolboxItemFilterAttribute.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
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
			return ToString ().GetHashCode ();
		}

		public override bool Match (object obj)
		{
			if (!(obj is ToolboxItemFilterAttribute))
				return false;
			return ((ToolboxItemFilterAttribute) obj).FilterString == Filter;
		}

		public override string ToString ()
		{
			return String.Format ("{0},{1}", Filter, ItemFilterType);
		}
	}
}
