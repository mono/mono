//
// System.Windows.Forms.Design.ComponentDocumentDesigner.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

namespace System.Windows.Forms.Design
{
	public class ComponentDocumentDesigner : ComponentDesigner, IRootDesigner, IToolboxUser, ITypeDescriptorFilterService, IOleDragClient
	{
		#region Public Instance Constructors

		[MonoTODO]
		public ComponentDocumentDesigner ()
		{
		}

		#endregion Public Instance Constructors

		#region Implementation of IRootDesigner

		ViewTechnology[] IRootDesigner.SupportedTechnologies
		{
			get
			{
				ViewTechnology[] array1 = new ViewTechnology[1];
				array1[0] = ViewTechnology.WindowsForms;
				return array1;
			}
		}

		[MonoTODO]
		object IRootDesigner.GetView (ViewTechnology technology)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of IRootDesigner

		#region Implementation of IToolboxUser

		bool IToolboxUser.GetToolSupported (ToolboxItem tool)
		{
			return true;
		}

		[MonoTODO]
		void IToolboxUser.ToolPicked (ToolboxItem tool)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of IToolboxUser

		#region Implementation of ITypeDescriptorFilterService

		[MonoTODO]
		bool ITypeDescriptorFilterService.FilterAttributes (IComponent component, IDictionary attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool ITypeDescriptorFilterService.FilterEvents (IComponent component, IDictionary events)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool ITypeDescriptorFilterService.FilterProperties (IComponent component, IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		#endregion Implementation of ITypeDescriptorFilterService

		#region Implementation of IOleDragClient

		[MonoTODO]
		bool IOleDragClient.AddComponent (IComponent component, string name, bool firstAdd)
		{
			throw new NotImplementedException ();
		}

		bool IOleDragClient.CanModifyComponents
		{
			get
			{
				return true;
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
			return true;
		}

		[MonoTODO]
		IComponent IOleDragClient.Component
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		#endregion Implementation of IOleDragClient

		#region Public Instance Properties

		[MonoTODO]
		public Control Control
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public bool TrayAutoArrange
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
		public bool TrayLargeIcon
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

		[MonoTODO]
		public override void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		protected virtual bool GetToolSupported (ToolboxItem tool)
		{
			return true;
		}

		#region Override implementation of ComponentDesigner

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		#endregion Override implementation of ComponentDesigner
	}
}
