//
// System.Web.UI.Design.XmlUrlEditor
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace System.Web.UI.Design {

	public class XmlUrlEditor : UrlEditor {

		public XmlUrlEditor ()
		{
		}

		protected override string Caption {
			get {
				return "Select XML File";
			}
		}

		protected override string Filter {
			get {
				return "XML Files(*.xml)|*.xml|All Files(*.*)|*.*|";
			}
		}

		protected override UrlBuilderOptions Options {
			get {
				return UrlBuilderOptions.NoAbsolute;
			}
		}
	}
}

