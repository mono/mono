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
using System.Diagnostics;
using System.Drawing.Design;

namespace System.Windows.Forms.Design
{
	[ToolboxItemFilter("System.Windows.Forms")]
	public class DocumentDesigner : ScrollableControlDesigner, IRootDesigner, IDesigner, IDisposable, IToolboxUser, IOleDragClient
	{
		#region Public Instance Constructors

		[MonoTODO]
		public DocumentDesigner ()
		{
		}

		#endregion Public Instance Constructors

		#region Static Constructor

		[MonoTODO]
		static DocumentDesigner ()
		{
		}

		#endregion Static Constructor

		#region Override implementation of ScrollableControlDesigner

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
		protected override void OnContextMenu (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnCreateHandle ()
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
		public override SelectionRules SelectionRules
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion Override implementation of ScrollableControlDesigner

		#region Internal Instance Methods

		[MonoTODO]
		internal virtual bool CanDropComponents (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal virtual void DoProperMenuSelection (ICollection selComponents)
		{
			throw new NotImplementedException ();
		}

		#endregion Internal Instance Methods

		#region Protected Instance Methods

		[MonoTODO]
		protected virtual void EnsureMenuEditorService (IComponent c)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool GetToolSupported (ToolboxItem tool)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void ToolPicked (ToolboxItem tool)
		{
			throw new NotImplementedException ();
		}


		#endregion Protected Instance Methods

		#region Implementation of IRootDesigner

		[MonoTODO]
		ViewTechnology[] IRootDesigner.SupportedTechnologies
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		object IRootDesigner.GetView (ViewTechnology technology)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of IRootDesigner

		#region Implementation of IToolboxUser

		[MonoTODO]
		bool IToolboxUser.GetToolSupported (ToolboxItem tool)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IToolboxUser.ToolPicked (ToolboxItem tool)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of IToolboxUser

		#region Implementation of IOleDragClient

		[MonoTODO]
		Control IOleDragClient.GetControlForComponent (object component)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of IOleDragClient
	}
}
