//
// System.Windows.Forms.Design.DockEditor.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.ComponentModel;
using System.Drawing.Design;

namespace System.Windows.Forms.Design
{
	public sealed class DockEditor : UITypeEditor
	{
		public DockEditor ()
		{
		}

		[MonoTODO]
		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			throw new NotImplementedException ();
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
	}
}
