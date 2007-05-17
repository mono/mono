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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	olivier Dufour	olivier.duff@free.fr
//
//

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace System.Windows.Forms
{
	public class DataGridViewLinkColumn : DataGridViewColumn
	{
		
		public DataGridViewLinkColumn ()
		{
			base.CellTemplate = new DataGridViewLinkCell ();
		}

		public override object Clone ()
		{
			DataGridViewLinkColumn clone = (DataGridViewLinkColumn)base.Clone ();
			clone.CellTemplate = (DataGridViewCell) this.CellTemplate.Clone ();
			return clone;
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		#region private fields

		private string text = string.Empty;

		#endregion

		#region Public Properties
		[MonoTODO]
		public Color ActiveLinkColor {
			get {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : default The default value is the user's Internet Explorer setting for the color of links in the hover state.
				return template.ActiveLinkColor; 
			}
			set {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : sets the ActiveLinkColor property of every cell in the column 
				//TODO : refreshes the column display
				template.ActiveLinkColor = value; 
			}
		}

		public override DataGridViewCell CellTemplate {
			get { return base.CellTemplate; }
			set { base.CellTemplate = value as DataGridViewLinkCell; }
		}

		[MonoTODO]
		public LinkBehavior LinkBehavior {
			get	{
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : default The default value is LinkBehavior.SystemDefault
				return template.LinkBehavior;
			}
			set	{
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : sets the LinkBehavior property of every cell in the column 
				//TODO : refreshes the column display
				template.LinkBehavior = value;
			}
		}
		[MonoTODO]
		public Color LinkColor {
			get	{
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : default The default value is the user's Internet Explorer setting for the link color.
				return template.LinkColor;
			}
			set	{
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : sets the LinkColor property of every cell in the column 
				//TODO : refreshes the column display
				template.LinkColor = value;
			}
		}
		[MonoTODO]
		public string Text {
			get {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				return text;
			}
			set {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : sets the Text property of every cell in the column 
				//TODO : refreshes the column display
				//TODO only if UseColumnTextForLinkValue is true
				text = value;
			}
		}
		//When TrackVisitedState is true, the VisitedLinkColor property value is used to display links that have already been visited.
		[MonoTODO]
		public bool TrackVisitedState {
			get {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : default The default value is true
				return template.TrackVisitedState;
			}
			set {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : sets the TrackVisitedState property of every cell in the column 
				//TODO : refreshes the column display
				template.TrackVisitedState = value;
			}
		}
		// true if the Text property value is displayed as the link text; false if the cell FormattedValue property value is displayed as the link text. The default is false.
		[MonoTODO]
		public bool UseColumnTextForLinkValue {
			get {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : default The default value is false
				return template.UseColumnTextForLinkValue;
			}
			set {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : sets the UseColumnTextForLinkValue property of every cell in the column 
				//TODO : refreshes the column display
				template.UseColumnTextForLinkValue = value;
			}
		}
		//If the TrackVisitedState property is set to false, the VisitedLinkColor property is ignored.
		[MonoTODO]
		public Color VisitedLinkColor {
			get {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : default The default value is the user's Internet Explorer setting for the visited link color.
				return template.VisitedLinkColor;
			}
			set {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				//TODO : sets the VisitedLinkColor property of every cell in the column 
				//TODO : refreshes the column display
				template.VisitedLinkColor = value;
			}
		}
		#endregion
	}
}

#endif