//
// System.ComponentModel.DesignerSerializationVisibilityAttribute.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//
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

	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Property
			 | AttributeTargets.Field | AttributeTargets.Event
			 )]
	public sealed class DesignerSerializationVisibilityAttribute : Attribute
	{

		private DesignerSerializationVisibility visibility;

		public static readonly DesignerSerializationVisibilityAttribute Default=
				new DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Visible);
		public static readonly DesignerSerializationVisibilityAttribute Content =
				new DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content);
		public static readonly DesignerSerializationVisibilityAttribute Hidden =
				new DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden);
		public static readonly DesignerSerializationVisibilityAttribute Visible=
				new DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Visible);


		public DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility vis)
		{
			visibility = vis;
		}


		public DesignerSerializationVisibility Visibility {
			get { return visibility; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is DesignerSerializationVisibilityAttribute))
				return false;
			if (obj == this)
				return true;
			return ((DesignerSerializationVisibilityAttribute) obj).Visibility == visibility;
		}

		public override int GetHashCode ()
		{
			return visibility.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return visibility == DesignerSerializationVisibilityAttribute.Default.Visibility;
		}
	}
}
