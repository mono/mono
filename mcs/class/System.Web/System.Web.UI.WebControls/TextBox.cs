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
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ControlBuilder (typeof (TextBoxControlBuilder))]
	[DefaultEvent("TextChanged")]
	[DefaultProperty("Text")]
	[ParseChildren(false)]
	[ValidationProperty("Text")]
	public class TextBox : WebControl, IPostBackDataHandler
	{
		private static readonly object TextChangedEvent = new object ();

		public TextBox() : base (HtmlTextWriterTag.Input)
		{
		}

		public virtual bool AutoPostBack
		{
			get {
				object o = ViewState ["AutoPostBack"];
				return (o == null) ? false : (bool) o;
			}

			set { ViewState ["AutoPostBack"] = value; }
		}

		public virtual int Columns
		{
			get {
				object o = ViewState ["Columns"];
				return (o == null) ? 0 : (int) o;
			}

			set { ViewState ["Columns"] = value; }
		}

		public virtual int MaxLength
		{
			get
			{
				object o = ViewState ["MaxLength"];
				return (o == null) ? 0 : (int) o;
			}

			set { ViewState ["MaxLength"] = value; }
		}

		public virtual bool ReadOnly
		{
			get
			{
				object o = ViewState ["ReadOnly"];
				return (o == null) ? false : (bool) o;
			}
			set { ViewState ["ReadOnly"] = value; }
		}

		public virtual int Rows
		{
			get
			{
				object o = ViewState ["Rows"];
				return (o == null) ? 0 : (int) o;
			}

			set { ViewState ["Rows"] = value; }
		}

		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}

		public virtual TextBoxMode TextMode
		{
			get {
				object o = ViewState ["TextMode"];
				return (o == null) ? TextBoxMode.SingleLine : (TextBoxMode) o;
			}

			set {
				if(!Enum.IsDefined (typeof(TextBoxMode), value))
					throw new ArgumentException ();
				ViewState ["TextMode"] = value;
			}
		}

		public virtual bool Wrap
		{
			get {
				object o = ViewState ["Wrap"];
				return (o == null) ? true : (bool) o;
			}

			set { ViewState ["Wrap"] = value; }
		}

		public event EventHandler TextChanged
		{
			add { Events.AddHandler (TextChangedEvent, value); }
			remove { Events.RemoveHandler (TextChangedEvent, value); }
		}

		protected override HtmlTextWriterTag TagKey
		{
			get {
				if(TextMode == TextBoxMode.MultiLine)
					return HtmlTextWriterTag.Textarea;
				return HtmlTextWriterTag.Input;
			}
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			if(Page != null)
				Page.VerifyRenderingInServerForm (this);

			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
			if (TextMode == TextBoxMode.MultiLine){
				if (Rows > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Rows,
							     Rows.ToString (
								NumberFormatInfo.InvariantInfo));

				if (Columns > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Cols,
							     Columns.ToString (
								NumberFormatInfo.InvariantInfo));

				if (!Wrap)
					writer.AddAttribute(HtmlTextWriterAttribute.Wrap, "off");
			} else {
				string mode;
				if (TextMode == TextBoxMode.Password)
					mode = "password";
				else {
					mode = "text";
					if (Text.Length > 0)
						writer.AddAttribute (HtmlTextWriterAttribute.Value, Text);
				}
					
				writer.AddAttribute (HtmlTextWriterAttribute.Type, mode);
				if (MaxLength > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Maxlength,
							     MaxLength.ToString (NumberFormatInfo.InvariantInfo));

				if (Columns > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Size,
							     Columns.ToString (NumberFormatInfo.InvariantInfo));
			}

			if (ReadOnly)
				writer.AddAttribute (HtmlTextWriterAttribute.ReadOnly, "readonly");

			base.AddAttributesToRender (writer);

			if (AutoPostBack && Page != null){
				writer.AddAttribute (HtmlTextWriterAttribute.Onchange,
						     Page.GetPostBackClientEvent (this, ""));
				writer.AddAttribute ("language", "javascript");
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(!(obj is LiteralControl))
				throw new HttpException (HttpRuntime.FormatResourceString (
							"Cannot_Have_Children_Of_Type", "TextBox",
							GetType ().Name.ToString ()));

			Text = ((LiteralControl) obj).Text;
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Events [TextChangedEvent] == null)
				ViewState.SetItemDirty ("Text", false);
		}

		protected virtual void OnTextChanged (EventArgs e)
		{
			if(Events != null){
				EventHandler eh = (EventHandler) (Events [TextChangedEvent]);
				if(eh != null)
					eh (this, e);
			}
		}

		protected override void Render (HtmlTextWriter writer)
		{
			RenderBeginTag(writer);
			if (TextMode == TextBoxMode.MultiLine)
				HttpUtility.HtmlEncode (Text, writer);
			RenderEndTag(writer);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey,
							NameValueCollection postCollection)
		{
			if (postCollection [postDataKey] != Text){
				Text = postCollection [postDataKey];
				return true;
			}
			return false;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnTextChanged (EventArgs.Empty);
		}
	}
}
