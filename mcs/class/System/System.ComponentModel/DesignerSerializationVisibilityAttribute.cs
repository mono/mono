//
// System.ComponentModel.DesignerSerializationVisibilityAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class DesignerSerializationVisibilityAttribute : Attribute {
		DesignerSerializationVisibility visibility;

		static DesignerSerializationVisibilityAttribute ()
		{
			Content = new DesignerSerializationVisibilityAttribute (
				DesignerSerializationVisibility.Content);
			Hidden = new DesignerSerializationVisibilityAttribute (
				DesignerSerializationVisibility.Hidden);
			Visible = new DesignerSerializationVisibilityAttribute (
				DesignerSerializationVisibility.Visible);
		}
		
		DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility vis)
		{
			visibility = vis;
		}

		public static readonly DesignerSerializationVisibilityAttribute Content;
		public static readonly DesignerSerializationVisibilityAttribute Hidden;
		public static readonly DesignerSerializationVisibilityAttribute Visible;

		public DesignerSerializationVisibility Visibility {
			get {
				return visibility;
			}
		}
	}
}
