//
// System.Web.UI.Design.XslUrlEditor
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


using System;

namespace System.Web.UI.Design {

	public class XslUrlEditor : UrlEditor {

		public XslUrlEditor ()
		{
		}

		protected override string Caption {
			get {
				return "Select XSL Transform File";
			}
		}

		protected override string Filter {
			get {
				return "XSL Files(*.xsl;*.xslt)|*.xsl;*.xslt|All Files(*.*)|*.*|";
			}
		}

		protected override UrlBuilderOptions Options {
			get {
				return UrlBuilderOptions.NoAbsolute;
			}
		}
	}
}

