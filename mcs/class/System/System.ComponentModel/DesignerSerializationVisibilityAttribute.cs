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

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property)]
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
