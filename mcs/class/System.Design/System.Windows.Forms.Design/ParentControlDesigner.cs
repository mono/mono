//
// System.Windows.Forms.Design.ComponentEditorForm.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
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

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;

namespace System.Windows.Forms.Design
{
	public class ParentControlDesigner : ControlDesigner, ISelectionUIHandler, IOleDragClient
	{
		#region Public Instance Constructors

		[MonoTODO]
		public ParentControlDesigner ()
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Constructors

		#region Static Constructor

		static ParentControlDesigner ()
		{
			ParentControlDesigner.StepControls = new BooleanSwitch ("StepControls", "ParentControlDesigner: step added controls");
		}

		#endregion Static Constructor

		#region Internal Instance Properties

		[MonoTODO]
		internal Size ParentGridSize
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion Internal Instance Properties

		#region Protected Instance Properties

		[MonoTODO]
		protected virtual Point DefaultControlLocation
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected virtual bool DrawGrid
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected Size GridSize
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		#endregion Protected Instance Properties

		#region Override implementation of ControlDesigner

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnDragDrop (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnDragEnter (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnDragLeave (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnDragOver (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnGiveFeedback (GiveFeedbackEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnMouseDragBegin (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnMouseDragEnd (bool cancel)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnMouseDragMove (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnMouseEnter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnMouseHover ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnMouseLeave ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnPaintAdornments (PaintEventArgs pe)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnSetCursor ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void WndProc (ref Message m)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool EnableDragRect
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion Override implementation of ControlDesigner

		#region Private Static Methods

		[MonoTODO]
		protected static void InvokeCreateTool (ParentControlDesigner toInvoke, ToolboxItem tool)
		{
			throw new NotImplementedException ();
		}

		#endregion Private Static Methods

		#region Implementation of IOleDragClient

		[MonoTODO]
		bool IOleDragClient.AddComponent (IComponent component, string name, bool firstAdd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IOleDragClient.CanModifyComponents
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		IComponent IOleDragClient.Component
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		Control IOleDragClient.GetControlForComponent (object component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		Control IOleDragClient.GetDesignerControl ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IOleDragClient.IsDropOk (IComponent component)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of IOleDragClient

		#region Implementation of ISelectionUIHandler

		[MonoTODO]
		bool ISelectionUIHandler.BeginDrag (object[] components, SelectionRules rules, int initialX, int initialY)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISelectionUIHandler.DragMoved (object[] components, Rectangle offset)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISelectionUIHandler.EndDrag (object[] components, bool cancel)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		Rectangle ISelectionUIHandler.GetComponentBounds (object component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		SelectionRules ISelectionUIHandler.GetComponentRules (object component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		Rectangle ISelectionUIHandler.GetSelectionClipRect (object component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISelectionUIHandler.OleDragDrop (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISelectionUIHandler.OleDragEnter (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISelectionUIHandler.OleDragLeave ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISelectionUIHandler.OleDragOver (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISelectionUIHandler.OnSelectionDoubleClick (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool ISelectionUIHandler.QueryBeginDrag (object[] components, SelectionRules rules, int initialX, int initialY)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISelectionUIHandler.ShowContextMenu (IComponent component)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of ISelectionUIHandler

		#region Public Instance Methods

		[MonoTODO]
		public virtual bool CanParent (Control control)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool CanParent (ControlDesigner controlDesigner)
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Methods

		#region Internal Instance Methods

		[MonoTODO]
		internal Point GetSnappedPoint (Point pt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal void SetCursor ()
		{
			throw new NotImplementedException ();
		}

		#endregion Internal Instance Methods

		#region Protected Instance Methods

		[MonoTODO]
		protected void CreateTool (ToolboxItem tool)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void CreateTool (ToolboxItem tool, Point location)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void CreateTool (ToolboxItem tool, Rectangle bounds)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual IComponent[] CreateToolCore (ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected Control GetControl (object component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected Rectangle GetUpdatedRect (Rectangle originalRect, Rectangle dragRect, bool updateSize)
		{
			throw new NotImplementedException ();
		}

		#endregion Protected Instance Methods

		#region Private Static Fields

		private static BooleanSwitch StepControls;

		#endregion Private Static Fields
	}
}
