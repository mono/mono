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

		[MonoTODO]
		protected IMenuEditorService menuEditorService;
	}
}
