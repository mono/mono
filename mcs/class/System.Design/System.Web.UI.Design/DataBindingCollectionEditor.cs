//
// System.Web.UI.Design.DataBindingCollectionEditor
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.ComponentModel;
using System.Drawing.Design;

namespace System.Web.UI.Design
{
	public class DataBindingCollectionEditor : UITypeEditor
	{
		public DataBindingCollectionEditor ()
		{
		}

		[MonoTODO]
		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			throw new NotImplementedException ();
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}
	}
}
