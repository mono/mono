
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
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
	[MonoTODO]
#if NET_2_0
	[Serializable]
#endif
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

#if NET_2_0
		protected WebControlToolboxItem (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
#endif

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