//
// System.Web.UI.WebControls.TextBox.cs
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
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	public class TextBox : WebControl, IPostBackDataHandler
	{
		private static readonly object TextChangedEvent = new object ();

		public TextBox() : base (HtmlTextWriterTag.Input)
		{
		}

		[DefaultValue (false), WebCategory ("Behavior")]
		[WebSysDescription ("The control automatically posts back after changing the text.")]
		public virtual bool AutoPostBack
		{
			get {
				object o = ViewState ["AutoPostBack"];
				return (o == null) ? false : (bool) o;
			}

			set { ViewState ["AutoPostBack"] = value; }
		}

		[DefaultValue (0), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The width of this control specified in characters.")]
		public virtual int Columns
		{
			get {
				object o = ViewState ["Columns"];
				return (o == null) ? 0 : (int) o;
			}

			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "Columns value has to be 0 for 'not set' or bigger than 0.");
				ViewState ["Columns"] = value; 
			}
		}

		[DefaultValue (0), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("The maximum number of characters you can enter in this control.")]
		public virtual int MaxLength
		{
			get
			{
				object o = ViewState ["MaxLength"];
				return (o == null) ? 0 : (int) o;
			}

			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "MaxLength value has to be 0 for 'not set' or bigger than 0.");
				ViewState ["MaxLength"] = value;
			}
		}

		[DefaultValue (false), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("If the control is ReadOnly you cannot enter new text.")]
		public virtual bool ReadOnly
		{
			get
			{
				object o = ViewState ["ReadOnly"];
				return (o == null) ? false : (bool) o;
			}
			set { ViewState ["ReadOnly"] = value; }
		}

		[DefaultValue (0), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("The number of lines that this multiline contol spans.")]
		public virtual int Rows
		{
			get
			{
				object o = ViewState ["Rows"];
				return (o == null) ? 0 : (int) o;
			}

			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "Rows value has to be 0 for 'not set' or bigger than 0.");
				ViewState ["Rows"] = value;
			}
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[PersistenceMode (PersistenceMode.EncodedInnerDefaultProperty)]
		[WebSysDescription ("The text that this control initially displays.")]
		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}

		[DefaultValue (typeof (TextBoxMode), "SingleLine"), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("A mode of how the control operates.")]
		public virtual TextBoxMode TextMode
		{
			get {
				object o = ViewState ["TextMode"];
				return (o == null) ? TextBoxMode.SingleLine : (TextBoxMode) o;
			}

			set {
				if(!Enum.IsDefined (typeof(TextBoxMode), value))
					throw new ArgumentOutOfRangeException ("value", "Only existing modes are allowed");
				ViewState ["TextMode"] = value;
			}
		}

		[DefaultValue (true), WebCategory ("Layout")]
		[WebSysDescription ("Determines if a line wraps at line-end.")]
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
