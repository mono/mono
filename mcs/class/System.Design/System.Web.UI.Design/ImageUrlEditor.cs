//
// System.Web.UI.Design.ImageUrlEditor
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


using System;

namespace System.Web.UI.Design {

	public class ImageUrlEditor : UrlEditor {

		private string caption;
		private string filter;
		
		public ImageUrlEditor ()
		{
		}

		protected override string Caption {
			get { return "Select Image"; }
		}

		protected override string Filter {
			get {
				return "Image Files(*.gif;*.jpg;*.jpeg;*.bmp;*.wmf;*.png)|" +
				"*.gif;*.jpg;*.jpeg;*.bmp;*.wmf;*.png|All Files(*.*)|*.*|";
			}
		}
	}
}

