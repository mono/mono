/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Label
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

namespace System.Web.UI.WebControls
{
	public class Label : WebControl
	{
		public Label(): base()
		{
		}

		internal Label(HtmlTextWriterTag tagKey): base(tagKey)
		{
		}

		public virtual string Text
		{
			get
			{
				object o = ViewState["Text"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(HasControls())
			{
				base.AddParsedSubObject(obj);
				return;
			}
			if(obj is LiteralControl)
			{
				Text = ((LiteralControl)obj).Text;
				return;
			}
			if(Text.Length > 0)
			{
				base.AddParsedSubObject(Text);
				Text = String.Empty;
			}
			base.AddParsedSubObject(obj);
		}

		protected override void LoadViewState(object savedState)
		{
			if(savedState != null)
			{
				base.LoadViewState(savedState);
				string savedText = (string)ViewState["Text"];
				if(savedText != null)
					Text = savedText;
			}
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			if(HasControls())
			{
				base.RenderContents(writer);
			} else
			{
				writer.Write(Text);
			}
		}
	}
}
