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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;

namespace System.Windows.Forms.Design
{
	[DesignTimeVisible (false)]
	[ToolboxItem (false)]
	[ProvideProperty ("Location", typeof(IComponent))]
	public class ComponentTray : ScrollableControl, IExtenderProvider, ISelectionUIHandler, IOleDragClient
	{
		#region Public Instance Constructors

		[MonoTODO]
		public ComponentTray (IDesigner mainDesigner, IServiceProvider serviceProvider)
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Constructors

		#region Static Constructor

		[MonoTODO]
		static ComponentTray ()
		{
		}

		#endregion Static Constructor

		#region Public Instance Properties

		[MonoTODO]
		public bool AutoArrange
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
		public int ComponentCount
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool ShowLargeIcons
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

		#endregion Public Instance Properties

		#region Override implementation of ScrollableControl

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override object GetService (Type serviceType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void WndProc (ref Message m)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnDoubleClick (EventArgs e)
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
		protected override void OnGiveFeedback (GiveFeedbackEventArgs gfevent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnLayout (LayoutEventArgs levent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnMouseDown (MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnMouseMove (MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnMouseUp (MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnPaint (PaintEventArgs pe)
		{
			throw new NotImplementedException ();
		}

		#endregion Override implementation of ScrollableControl

		#region Implementation of IExtenderProvider

		[MonoTODO]
		bool IExtenderProvider.CanExtend (object component)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of IExtenderProvider

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
		public virtual void AddComponent (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveComponent (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CreateComponentFromTool (ToolboxItem tool)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[DesignOnly (true)]
		[Category ("Layout")]
		[Localizable (false)]
		[Browsable (false)]
		public Point GetLocation (IComponent receiver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetLocation (IComponent receiver, Point location)
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		[MonoTODO]
		protected virtual bool CanCreateComponentFromTool (ToolboxItem tool)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool CanDisplayComponent (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void DisplayError (Exception e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnSetCursor ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnLostCapture ()
		{
			throw new NotImplementedException ();
		}

		#endregion Protected Instance Methods
	}
}
