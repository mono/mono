/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TextBox
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  80%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class TextBox : WebControl, IPostBackDataHandler
	{
		private static readonly object TextChangedEvent = new object();

		public TextBox(): base(HtmlTextWriterTag.Input)
		{
		}

		public virtual bool AutoPostBack
		{
			get
			{
				object o = ViewState["AutoPostBack"];
				if(o!=null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AutoPostBack"] = value;
			}
		}

		public virtual int Columns
		{
			get
			{
				object o = ViewState["Columns"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				ViewState["Columns"] = value;
			}
		}

		public virtual int MaxLength
		{
			get
			{
				object o = ViewState["MaxLrngth"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				ViewState["MaxLrngth"] = value;
			}
		}

		public virtual bool ReadOnly
		{
			get
			{
				object o = ViewState["ReadOnly"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ReadOnly"] = value;
			}
		}

		public virtual int Rows
		{
			get
			{
				object o = ViewState["Rows"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				ViewState["Rows"] = value;
			}
		}

		public virtual string Text
		{
			get
			{
				object o = ViewState["Text"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}

		public virtual TextBoxMode TextMode
		{
			get
			{
				object o = ViewState["TextMode"];
				if(o != null)
					return (TextBoxMode)o;
				return TextBoxMode.SingleLine;
			}
			set
			{
				if(!Enum.IsDefined(typeof(TextBoxMode), value))
				{
					throw new ArgumentException();
				}
				ViewState["TextMode"] = value;
			}
		}

		public virtual bool Wrap
		{
			get
			{
				object o = ViewState["Wrap"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["Wrap"] = value;
			}
		}

		public event EventHandler TextChanged
		{
			add
			{
				Events.AddHandler(TextChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(TextChangedEvent, value);
			}
		}

		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				if(TextMode == TextBoxMode.MultiLine)
				{
					return HtmlTextWriterTag.Textarea;
				}
				return HtmlTextWriterTag.Input;
			}
		}

		[MonoTODO("Check_Value_of_Text_Potential_Bug_In_MS_Implementation")]
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			if(Page != null)
			{
				Page.VerifyRenderingInServerForm(this);
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
			if(TextMode == TextBoxMode.MultiLine)
			{
				if(Rows > 0)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Rows, Rows.ToString(NumberFormatInfo.InvariantInfo));
				}
				if(Columns > 0)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Cols, Columns.ToString(NumberFormatInfo.InvariantInfo));
				}
				if(!Wrap)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Wrap, "off");
				}
			} else
			{
				if(TextMode == TextBoxMode.Password)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Type, "password");
				} else
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
				}
				if(MaxLength > 0)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, MaxLength.ToString(NumberFormatInfo.InvariantInfo));
				}
				if(Columns > 0)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Size, Columns.ToString(NumberFormatInfo.InvariantInfo));
				}
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Value, Text);
			if(ReadOnly)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.ReadOnly, "readonly");
			}
			base.AddAttributesToRender(writer);

			if(AutoPostBack && Page != null)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Onchange, Page.GetPostBackClientEvent(this, ""));
				writer.AddAttribute("language", "javascript");
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(obj is LiteralControl)
			{
				Text = ((LiteralControl)obj).Text;
				return;
			}
			throw new HttpException(HttpRuntime.FormatResourceString("Cannot_Have_Children_Of_Type", "TextBox", GetType().Name.ToString()));
		}

		[MonoTODO("OnPreRender")]
		protected override void OnPreRender(EventArgs e)
		{
			OnPreRender(e);
			throw new NotImplementedException();
		}

		protected virtual void OnTextChanged(EventArgs e)
		{
			if(Events != null)
			{
				EventHandler eh = (EventHandler)(Events[TextChangedEvent]);
				if(eh != null)
					eh(this, e);
			}
		}

		[MonoTODO("Encode_Text")]
		protected override void Render(HtmlTextWriter writer)
		{
			RenderBeginTag(writer);
			//TODO: if(TextMode == MultiLine) { Encode(Text) and writeTo(writer) }
			RenderEndTag(writer);
			throw new NotImplementedException();
		}

		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			if(postCollection[postDataKey] != Text)
			{
				Text = postCollection[postDataKey];
				return true;
			}
			return false;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			OnTextChanged(EventArgs.Empty);
		}
	}
}
