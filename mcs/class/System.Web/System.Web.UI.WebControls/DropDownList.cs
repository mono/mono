//
// System.Web.UI.WebControls.DropDownList.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ValidationProperty("SelectedItem")]
	public class DropDownList : ListControl, IPostBackDataHandler
	{
		public DropDownList(): base()
		{
		}

		[Browsable (false)]
		public override Color BorderColor
		{
			get
			{
				return base.BorderColor;
			}
			set
			{
				base.BorderColor = value;
			}
		}

		[Browsable (false)]
		public override BorderStyle BorderStyle
		{
			get
			{
				return base.BorderStyle;
			}
			set
			{
				base.BorderStyle = value;
			}
		}

		[Browsable (false)]
		public override Unit BorderWidth
		{
			get
			{
				return base.BorderWidth;
			}
			set
			{
				base.BorderWidth = value;
			}
		}

		[DefaultValue (0), WebCategory ("Misc")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The index number of the currently selected ListItem.")]
		public override int SelectedIndex
		{
			get
			{
				int index  = base.SelectedIndex;
				if (index < 0 && Items.Count > 0) {
					index = 0;
					Items [0].Selected = true;
				}
				return index;
			}
			set
			{
				base.SelectedIndex = value;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Bindable (false), EditorBrowsable (EditorBrowsableState.Never)]
		public override string ToolTip
		{
			// MS ignores the tooltip for this one
			get {
				return String.Empty;
			}
			set {
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			if(Page != null)
			{
				Page.VerifyRenderingInServerForm(this);
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
			base.AddAttributesToRender(writer);

			if(AutoPostBack && Page != null)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Onchange, Page.GetPostBackClientEvent(this,""));
				writer.AddAttribute("language", "javascript");
			}
		}

		protected override ControlCollection CreateControlCollection()
		{
			return new EmptyControlCollection(this);
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			if(Items != null)
			{
				bool selected = false;
				foreach(ListItem current in Items)
				{
					writer.WriteBeginTag("option");
					if(current.Selected)
					{
						if(selected)
						{
							throw new HttpException(HttpRuntime.FormatResourceString("Cannot_Multiselect_In_DropDownList"));
						}
						selected = true;
						writer.WriteAttribute("selected", "selected", false);
					}
					writer.WriteAttribute("value", current.Value, true);
					writer.Write('>');
					HttpUtility.HtmlEncode(current.Text, writer);
					writer.WriteEndTag("option");
					writer.WriteLine();
				}
			}
		}

		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			string[] vals = postCollection.GetValues(postDataKey);
			if(vals != null)
			{
				int index = Items.FindByValueInternal(vals[0]);
				if(index != SelectedIndex)
				{
					SelectedIndex = index;
					return true;
				}
			}
			return false;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			OnSelectedIndexChanged(EventArgs.Empty);
		}
	}
}
