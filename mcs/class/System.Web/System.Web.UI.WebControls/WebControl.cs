/**
 * Namespace: System.Web.UI.WebControls
 * Class:     WebControl
 * 
 * Author:  Gaurav Vaish
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  15%??
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls
{
	public class WebControl : Control, IAttributeAccessor
	{
		//TODO: A list of private members may be incomplete

		private HtmlTextWriterTag    writerTag;
		private string               stringTag;
		private AttributesCollection attributes;
		private StateBag             attributeState;
		private Style                controlStyle;

		// TODO: The constructors definitions
		protected WebControl()
		{
			//todo: what now?
			controlStyle = null;
		}
		
		public WebControl(HtmlTextWriterTag tag)
		{
			//TODO: am i right?
			writerTag = tag;
			stringTag = null;
			controlStyle = null;
		}

		protected WebControl(string tag)
		{
			//TODO: am i right?
			stringTag = tag;
			writerTag = null;
			controlStyle = null;
		}
		
		public virtual string AccessKey
		{
			get
			{
				object o = ViewState["AccessKey"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["AccessKey"] = value;
			}
		}
		
		public AttributeCollection Attributes
		{
			if(attributes==null)
			{
				//TODO: From where to get StateBag and how? I think this method is OK!
				if(attributeState == null)
				{
					attributeState = new StateBag(true);
					if(attributeState.IsTrackingViewState)
					{
						attributeState.TrackViewState();
					}
				}
				attributes = new AttributeCollection(attributes);
			}
			return attributes;
		}
		
		public Style ControlStyle		
		{
			get
			{
				if(controlStyle == null)
				{
					controlStyle = CreateControlStyle();
					if(IsTrackingViewState)
					{
						controlStyle.TrackViewState();
					}
					controlStyle.LoadViewState(null);
				}
				return controlStyle;
			}
		}
		
		public bool ControlStyleCreated
		{
			get
			{
				return (controlStyle!=null);
			}
		}
		
		public virtual string CssClass
		{
			get
			{
				if(ControlStyleCreated)
					return controlStyle.CssClass;
				return String.Empty;
			}
		}
		
		public 
		
		protected virtual Style CreateControlStyle()
		{
			return new Style(ViewState); // from parent class Control
		}
		
		// Implemented procedures
		public string GetAttribute(string key)
		{
			return "";
		}
		
		public void SetAttribute(string key, string val)
		{			
		}

	}
}
