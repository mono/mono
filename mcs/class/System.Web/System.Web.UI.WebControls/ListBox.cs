/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ListBox
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  95%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class ListBox : ListControl, IPostBackDataHandler
	{
		public ListBox(): base()
		{
		}

		public override Color BorderColor
		{
			get
			{
				return BorderColor;
			}
			set
			{
				BorderColor = value;
			}
		}

		public override BorderStyle BorderStyle
		{
			get
			{
				return BorderStyle;
			}
			set
			{
				BorderStyle = value;
			}
		}

		public override Unit BorderWidth
		{
			get
			{
				return BorderWidth;
			}
			set
			{
				BorderWidth = value;
			}
		}

		public virtual int Rows
		{
			get
			{
				object o = ViewState["Rows"];
				if(o != null)
					return (int)o;
				return 4;
			}
			set
			{
				if(value < 1 || value > 2000)
				{
					throw new ArgumentOutOfRangeException();
				}
				ViewState["Rows"] = value;
			}
		}

		public virtual ListSelectionMode SelectionMode
		{
			get
			{
				object o = ViewState["SelectionMode"];
				if(o != null)
					return (ListSelectionMode)o;
				return ListSelectionMode.Single;
			}
			set
			{
				if(!Enum.IsDefined(typeof(ListSelectionMode), value))
				{
					throw new ArgumentException();
				}
				ViewState["SelectionMode"] = value;
			}
		}

		public override string ToolTip
		{
			get
			{
				return String.Empty;
			}
			set
			{
				// Don't do anything.
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			if(Page != null)
			{
				Page.VerifyRenderingInServerForm(this);
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
			writer.AddAttribute(HtmlTextWriterAttribute.Size, Rows.ToString(NumberFormatInfo.InvariantInfo);
			writer.AddAttribute(HtmlTextWriterAttribute.Multiple, "multiple");
			if(AutoPostBack && Page != null)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Onchange, Page.GetPostBackClientEvent(""));
				writer.AddAttribute("language", "javascript");
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if(Page != null && SelectionMode == ListSelectionMode.Multiple && Enabled)
			{
				Page.RegisterRequiresPostBack(this);
			}
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			bool isMultAllowed = (SelectionMode == ListSelectionMode.Multiple);
			bool selMade = false;
			foreach(ListItem current in Items)
			{
				writer.WriteBeginTag("option");
				if(current.Selected)
				{
					if(!isMultAllowed && selMade)
					{
						throw new HttpException("Cannnot_MutliSelect_In_Single_Mode");
					}
					selMade = true;
					writer.WriteAttribute("selected", "selected");
				}
				writer.WriteAttribute("value", current.Value, true);
				writer.Write('>');
				writer.Write(HttpUtility.Encode(Text));
				writer.WriteEndTag("option");
				writer.WriteLine();
			}
		}

		[MonoTODO]
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			string[] vals = postCollection.GetValues(postDataKey);
			bool flag = false;
			
			throw new NotImplementedException();
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			OnSelectedIndexChanged(EventArgs.Empty);
		}
	}
}
