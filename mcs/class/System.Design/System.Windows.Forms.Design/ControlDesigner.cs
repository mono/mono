//
// System.Windows.Forms.Design.ComponentEditorForm.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Design {
	public class ControlDesigner : ComponentDesigner
	{
		#region Public Instance Constructors

		[MonoTODO]
		public ControlDesigner()
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Constructors

		#region Static Constructor

		static ControlDesigner()
		{
			ControlDesigner.InvalidPoint = new Point(int.MinValue, int.MinValue);
		}

		#endregion Static Constructor

		#region Public Instance Methods

		[MonoTODO]
		public virtual bool CanBeParentedTo(IDesigner parentDesigner)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void OnSetComponentDefaults()
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		[MonoTODO]
		protected void BaseWndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void DefWndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void DisplayError(Exception e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void EnableDragDrop(bool value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool GetHitTest (Point point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void HookChildControls (Control firstChild)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnContextMenu (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnCreateHandle ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDragDrop (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDragEnter (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDragLeave (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDragOver(DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnGiveFeedback (GiveFeedbackEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseDragBegin (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseDragEnd (bool cancel)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseDragMove (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseEnter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseHover ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseLeave ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnPaintAdornments (PaintEventArgs pe)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnSetCursor ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void UnhookChildControls (Control firstChild)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void WndProc (ref Message m)
		{
			throw new NotImplementedException ();
		}

		#endregion Protected Instance Methods

		#region Override implementation of ComponentDesigner

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
		public override void InitializeNonDefault ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override ICollection AssociatedComponents
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion Override implementation of ComponentDesigner

		#region Public Instance Properties

		[MonoTODO]
		public virtual AccessibleObject AccessibilityObject {
			get {
				if (accessibilityObj == null)
					accessibilityObj = new ControlDesignerAccessibleObject (this, Control);

				return accessibilityObj;
			}
		}

		[MonoTODO]
		public virtual SelectionRules SelectionRules 
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual Control Control
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion Public Instance Properties

		#region Protected Instance Properties

		[MonoTODO]
		protected virtual bool EnableDragRect
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion Protected Instance Properties

		#region Protected Static Fields

		protected static readonly Point InvalidPoint;
		protected AccessibleObject accessibilityObj;

		#endregion Protected Static Fields

		[ComVisibleAttribute(true)]
		public class ControlDesignerAccessibleObject : AccessibleObject
		{
			[MonoTODO]
			public ControlDesignerAccessibleObject (ControlDesigner designer, Control control)
			{
				throw new NotImplementedException ();
			}

			#region Override implementation of AccessibleObject

			[MonoTODO]
			public override AccessibleObject GetChild (int index)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override int GetChildCount ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject GetFocused ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject GetSelected ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject HitTest (int x, int y)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override Rectangle Bounds 
			{ 
				get
				{
					throw new NotImplementedException ();
				}
			}

			[MonoTODO]
			public override string DefaultAction
			{
				get
				{
					throw new NotImplementedException ();
				}
			}

			[MonoTODO]
			public override string Description
			{
				get
				{
					throw new NotImplementedException ();
				}
			}

			[MonoTODO]
			public override string Name
			{
				get
				{
					throw new NotImplementedException ();
				}
			}

			[MonoTODO]
			public override AccessibleObject Parent
			{
				get
				{
					throw new NotImplementedException ();
				}
			}

			[MonoTODO]
			public override AccessibleRole Role
			{
				get
				{
					throw new NotImplementedException ();
				}
			}

			[MonoTODO]
			public override AccessibleStates State
			{
				get
				{
					throw new NotImplementedException ();
				}
			}

			[MonoTODO]
			public override string Value
			{
				get
				{
					throw new NotImplementedException ();
				}
			}

			#endregion Override implementation of AccessibleObject
		}
	}
}
