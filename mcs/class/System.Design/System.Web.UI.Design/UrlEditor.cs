//
// System.Web.UI.Design.UrlEditor
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace System.Web.UI.Design {

	public class UrlEditor : UITypeEditor {

		public UrlEditor ()
		{
		}

		protected virtual string Caption {
			get {
				return "Select URL";
			}
		}

		protected virtual string Filter {
			get {
				return "All Files(*.*)|*.*|";
			}
		}

		protected virtual UrlBuilderOptions Options {
			get {
				return UrlBuilderOptions.None;
			}
		}

		[MonoTODO]
		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}
	}
}

