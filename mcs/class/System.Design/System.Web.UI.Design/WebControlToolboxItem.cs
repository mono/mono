using System;
using System.Collections;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
	[MonoTODO]
	public class WebControlToolboxItem : ToolboxItem
	{
		#region Public Instance Constructors

		public WebControlToolboxItem ()
		{
			toolData = null;
			persistChildren = -1;
		}

		[MonoTODO]
		public WebControlToolboxItem (Type type)
		{
			toolData = null;
			persistChildren = -1;
		}

		#endregion Public Instance Constructors

		#region Override implementation of ToolboxItem

		[MonoTODO]
		protected override IComponent[] CreateComponentsCore (IDesignerHost host)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Serialize (SerializationInfo info, StreamingContext context)
		{
			base.Serialize (info, context);
			if (this.toolData != null)
			{
				info.AddValue ("ToolData", this.toolData);
			}
			if (this.persistChildren != -1)
			{
				info.AddValue ("PersistChildren", this.persistChildren);
			}
		}

		[MonoTODO]
		protected override void Deserialize (SerializationInfo info, StreamingContext context)
		{
			base.Deserialize (info, context);
			toolData = info.GetString ("ToolData");
			persistChildren = info.GetInt32 ("PersistChildren");
		}

		[MonoTODO]
		public override void Initialize (Type type)
		{
			throw new NotImplementedException ();
		}

		#endregion Override implementation of ToolboxItem

		#region Public Instance Methods

		[MonoTODO]
		public object GetToolAttributeValue (IDesignerHost host, Type attributeType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetToolHtml (IDesignerHost host)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Type GetToolType (IDesignerHost host)
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Methods

		#region Private Instance Fields

		private int persistChildren;
		private string toolData;

		#endregion Private Instance Fields
	}

}