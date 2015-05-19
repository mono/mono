//
// TableLayoutControlCollection.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace System.Windows.Forms
{
	[DesignerSerializer ("System.Windows.Forms.Design.TableLayoutControlCollectionCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
	[ListBindable (false)]
	public class TableLayoutControlCollection : Control.ControlCollection
	{
		private TableLayoutPanel panel;

		#region Public Constructor
		public TableLayoutControlCollection (TableLayoutPanel container) : base (container)
		{
			this.panel = container;
		}
		#endregion

		#region Public Property
		public TableLayoutPanel Container { get { return this.panel; } }
		#endregion

		#region Public Method
		public virtual void Add (Control control, int column, int row)
		{
			if (column < -1)
				throw new ArgumentException ("column");
			if (row < -1)
				throw new ArgumentException ("row");
			
			base.Add (control);
			
			panel.SetCellPosition (control, new TableLayoutPanelCellPosition (column, row));
		}
		#endregion	
	}
}
