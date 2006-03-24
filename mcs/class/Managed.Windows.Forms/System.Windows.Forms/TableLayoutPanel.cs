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
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) 2004 Novell, Inc.
//
#if NET_2_0
using System;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class TableLayoutPanel : Panel, IExtenderProvider {
		TableLayoutSettings settings;
		
		public TableLayoutPanel ()
		{
			settings = new TableLayoutSettings (this);
		}

		internal void Relayout ()
		{
		}
		
#region Proxy Properties
		new public BorderStyle BorderStyle {
			get {
				return base.BorderStyle;
			}

			set {
				base.BorderStyle = value;
			}
		}

		public int ColumnCount {
			get {
				return settings.ColumnCount;
			}

			set {
				settings.ColumnCount = value;
			}
		}

		public int RowCount {
			get {
				return settings.RowCount;
			}

			set {
				settings.RowCount = value;
			}
		}
#endregion
		
#region IExtenderProvider
		bool IExtenderProvider.CanExtend (object extendee)
		{
			//
			// Read: `Implementing an Extender Provider'
			//
			throw new NotImplementedException ();
		}
#endregion
	}
}
#endif
