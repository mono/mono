//
// System.ComponentModel.DesignerCategoryAttribute.cs
//
// Authors:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
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

namespace System.ComponentModel {

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DesignerCategoryAttribute : Attribute
	{
		private string category;

		public static readonly DesignerCategoryAttribute Component = new DesignerCategoryAttribute ("Component");
		public static readonly DesignerCategoryAttribute Form = new DesignerCategoryAttribute ("Form");
		public static readonly DesignerCategoryAttribute Generic = new DesignerCategoryAttribute ("Designer");
		public static readonly DesignerCategoryAttribute Default = new DesignerCategoryAttribute (string.Empty);
		
		public DesignerCategoryAttribute ()
		{
			this.category = string.Empty;
		}
		
		public DesignerCategoryAttribute (string category)
		{
			this.category = category;
		}
		
		public override object TypeId {
			get {
				return GetType ();
			}
		}
		
		public string Category {
			get {
				return category;
			}
		}
		
		public override bool Equals (object obj)
		{
			if (!(obj is DesignerCategoryAttribute))
				return false;
			if (obj == this)
				return true;
			return ((DesignerCategoryAttribute) obj).Category == category;
		}
		
		public override int GetHashCode ()
		{
			return category.GetHashCode ();
		}
		
		public override bool IsDefaultAttribute ()
		{
			return category == DesignerCategoryAttribute.Default.Category;
		}
	}
}
