/**
 * Namespace: System.Web.UI.WebControls
 * Class:     RadioButton
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class RadioButton: CheckBox, IPostBackDataHandler
	{
		public RadioButton(): base()
		{
		}
		
		public virtual string GroupName
		{
			get
			{
				object o = ViewState["GroupName"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["GroupName"] = value;
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if(Page != null && Enabled && !Checked)
			{
				Page.RegisterRequiresPostBack(this);
			}
			if(GroupName.Length == 0)
			{
				GroupName = UniqueID;
			}
		}
		
		internal override void RenderInputTag(HtmlTextWriter writer, string id)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, id);
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueGroupNamePrivate);
			writer.AddAttribute(HtmlTextWriterAttribute.Value, ValueAttributePrivate);
			
			if(Checked)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
			}
			if(AutoPostBack)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.OnClick, Page.GetPostBackClientEvent(this, ""));
				writer.AddAttribute("language", "javascript");
			}
			
			if(AccessKey.Length > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, AccessKey);
			}
			
			if(TabIndex > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, TabIndex.ToString(NumberFormatInfo.InvariantInfo))
			}
			
			writer.RenderBeginTag(System.Web.UI.HtmlTextWriterTag.Input);
			writer.RenderEndTag();
		}
		
		private string UniqueGroupNamePrivate
		{
			get
			{
				string retVal = GroupName;
				if(UniqueID.LastIndexOf(":") >= 0)
				{
					retVal += UniqueID.Substring(UniqueID.LastIndexOf(":") + 1);
				}
				return retVal;
			}
		}
		
		private string ValueAttributePrivate
		{
			get
			{
				string retVal = Attributes["value"];
				if(retVal == null)
				{
					retVal = ID;
				}
				if(retVal == null)
				{
					retVal = UniqueID;
				}
				return retVal;
			}
		}
		
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			if(postCollection[UniqueGroupNamePrivate] != null && postCollection[UniqueGroupNamePrivate] == ValueAttributePrivate)
			{
				if(!Checked)
				{
					Checked = true;
				}
				return true;
			}
			if(Checked)
			{
				Checked = false;
			}
			return true;
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			OnCheckChanged(EventArgs.Empty);
		}
	}
}
