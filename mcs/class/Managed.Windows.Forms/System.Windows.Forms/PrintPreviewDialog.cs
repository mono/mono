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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;

namespace System.Windows.Forms {

	public class PrintPreviewDialog : Form {
		#region Local variables
		PrintPreviewControl printPreview;
		#endregion // Local variables

		#region Public Constructors
		public PrintPreviewDialog() {
		}
		#endregion // Public Constructors

		#region Public Instance Properties
		public PrintDocument Document {
			get { return printPreview.Document; }
			set { printPreview.Document = value; }
		}
		public override ISite Site {
			get {
				return base.Site;
			}
			set {
				base.Site = value;
			}
		}
		public bool UseAntiAlias {
			get { return printPreview.UseAntiAlias; }
			set { printPreview.UseAntiAlias = value; }
		}

		#endregion // Public Instance Properties

		#region Protected Instance Properties
		protected override void CreateHandle() {
			base.CreateHandle ();
		}


		#endregion // Protected Instance Methods
	}
}