//
// System.Windows.Forms.Panel.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

//
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
using System.Drawing;
using System.Runtime.Remoting;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// Represents a Windows Panel control
	// </summary>

	public class Panel : ScrollableControl {

		BorderStyle borderStyle = BorderStyle.None;
		//
		//  --- Constructor
		//
		[MonoTODO]
		public Panel() {
			SetStyle ( ControlStyles.Selectable, false );
			SetStyle ( ControlStyles.UserPaint,  true  );
		}

		//
		//  --- Public Properties
		//
		public BorderStyle BorderStyle {
			get {   return borderStyle; }
			set {
				if ( !Enum.IsDefined ( typeof(BorderStyle), value ) )
					throw new InvalidEnumArgumentException( "BorderStyle",
						(int)value,
						typeof(BorderStyle));
				
				if ( borderStyle != value ) {
					int oldStyle = Win32.getBorderStyle ( borderStyle );
					int oldExStyle = Win32.getBorderExStyle ( borderStyle );
					borderStyle = value;

					if ( IsHandleCreated ) {
						Win32.UpdateWindowStyle ( Handle, oldStyle, Win32.getBorderStyle ( borderStyle ) );
						Win32.UpdateWindowExStyle ( Handle, oldExStyle, Win32.getBorderExStyle ( borderStyle ) );
					}
				}
			}
		}

		[MonoTODO]
		public override ISite Site {
			get {
				//FIXME:
				return base.Site;
			}
			set {
				//FIXME:
				base.Site = value;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value;}
		}

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;

				createParams.Style |= (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_CLIPCHILDREN |
					WindowStyles.WS_CLIPSIBLINGS);

				createParams.Style   |= Win32.getBorderStyle   ( BorderStyle );
				createParams.ExStyle |= Win32.getBorderExStyle ( BorderStyle );

				return createParams;
			}		
		}

		protected override Size DefaultSize {
			get { return new Size(219,109);	}
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e) {
			//FIXME:
			base.OnResize(e);
		}

		public override string ToString()
		{
			return GetType().FullName.ToString() + ", BorderStyle: " + BorderStyle.ToString();
		}
	}
}
