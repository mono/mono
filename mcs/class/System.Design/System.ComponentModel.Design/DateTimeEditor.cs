//
// System.ComponentModel.Design.DateTimeEditor
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Windows.Forms;
using System.Drawing.Design;

namespace System.ComponentModel.Design
{
	public class DateTimeEditor : UITypeEditor
	{
		public DateTimeEditor ()
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
