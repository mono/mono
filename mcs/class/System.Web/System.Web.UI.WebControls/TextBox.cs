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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
#if NET_2_0
	[ControlValuePropertyAttribute ("Text")]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.PreviewControlDesigner, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.IDesigner")]
#endif
	[ControlBuilder (typeof (TextBoxControlBuilder))]
	[DefaultEvent("TextChanged")]
	[DefaultProperty("Text")]
	[ParseChildren(false)]
	[ValidationProperty("Text")]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	public class TextBox : WebControl, IPostBackDataHandler
	{
		static readonly object TextChangedEvent = new object ();

		public TextBox() : base (HtmlTextWriterTag.Input)
		{
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (false), WebCategory ("Behavior")]
		[WebSysDescription ("The control automatically posts back after changing the text.")]
		public virtual bool AutoPostBack {
			get {
				object o = ViewState ["AutoPostBack"];
				return (o == null) ? false : (bool) o;
			}

			set { ViewState ["AutoPostBack"] = value; }
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (0), WebCategory ("Appearance")]
		[WebSysDescription ("The width of this control specified in characters.")]
		public virtual int Columns {
			get {
				object o = ViewState ["Columns"];
				return (o == null) ? 0 : (int) o;
			}

			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value",
						"Columns value has to be 0 for 'not set' or bigger than 0.");

				ViewState ["Columns"] = value; 
			}
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (0), WebCategory ("Behavior")]
		[WebSysDescription ("The maximum number of characters you can enter in this control.")]
		public virtual int MaxLength {
			get {
				object o = ViewState ["MaxLength"];
				return (o == null) ? 0 : (int) o;
			}

			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value",
						"MaxLength value has to be 0 for 'not set' or bigger than 0.");

				ViewState ["MaxLength"] = value;
			}
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (false), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("If the control is ReadOnly you cannot enter new text.")]
		public virtual bool ReadOnly {
			get {
				object o = ViewState ["ReadOnly"];
				return (o == null) ? false : (bool) o;
			}
			set { ViewState ["ReadOnly"] = value; }
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (0), WebCategory ("Behavior")]
		[WebSysDescription ("The number of lines that this multiline contol spans.")]
		public virtual int Rows {
			get {
				object o = ViewState ["Rows"];
				return (o == null) ? 0 : (int) o;
			}

			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value",
						"Rows value has to be 0 for 'not set' or bigger than 0.");
				ViewState ["Rows"] = value;
			}
		}

#if NET_2_0
	    [LocalizableAttribute (true)]
	    [EditorAttribute ("System.ComponentModel.Design.MultilineStringEditor,System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[PersistenceMode (PersistenceMode.EncodedInnerDefaultProperty)]
		[WebSysDescription ("The text that this control initially displays.")]
		public virtual string Text {
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (typeof (TextBoxMode), "SingleLine"), WebCategory ("Behavior")]
		[WebSysDescription ("A mode of how the control operates.")]
		public virtual TextBoxMode TextMode {
			get {
				object o = ViewState ["TextMode"];
				return (o == null) ? TextBoxMode.SingleLine : (TextBoxMode) o;
			}

			set {
				if(!Enum.IsDefined (typeof(TextBoxMode), value))
					throw new ArgumentOutOfRangeException ("value",
								"Only existing modes are allowed");

				ViewState ["TextMode"] = value;
			}
		}

		[DefaultValue (true), WebCategory ("Layout")]
		[WebSysDescription ("Determines if a line wraps at line-end.")]
		public virtual bool Wrap {
			get {
				object o = ViewState ["Wrap"];
				return (o == null) ? true : (bool) o;
			}

			set { ViewState ["Wrap"] = value; }
		}


		[WebCategory ("Action")]
		[WebSysDescription ("Raised when the text is changed.")]
		public event EventHandler TextChanged {
			add { Events.AddHandler (TextChangedEvent, value); }
			remove { Events.RemoveHandler (TextChangedEvent, value); }
		}

		protected override HtmlTextWriterTag TagKey {
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

			NumberFormatInfo invar = NumberFormatInfo.InvariantInfo;

			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
			if (TextMode == TextBoxMode.MultiLine) {
				if (Rows > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Rows,
							     Rows.ToString (invar));

				if (Columns > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Cols,
							     Columns.ToString (invar));

				if (!Wrap)
					writer.AddAttribute(HtmlTextWriterAttribute.Wrap, "off");
			} else {
				string mode;
				if (TextMode == TextBoxMode.Password) {
					mode = "password";
				} else {
					mode = "text";
					if (Text.Length > 0)
						writer.AddAttribute (HtmlTextWriterAttribute.Value, Text);
				}
					
				writer.AddAttribute (HtmlTextWriterAttribute.Type, mode);
				if (MaxLength > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Maxlength,
							     MaxLength.ToString (invar));

				if (Columns > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Size,
							     Columns.ToString (invar));
			}

			if (ReadOnly)
				writer.AddAttribute (HtmlTextWriterAttribute.ReadOnly, "readonly");

			base.AddAttributesToRender (writer);

			if (AutoPostBack && Page != null){
				writer.AddAttribute (HtmlTextWriterAttribute.Onchange,
						     Page.ClientScript.GetPostBackClientEvent (this, ""));
				writer.AddAttribute ("language", "javascript");
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(!(obj is LiteralControl))
				throw new HttpException ("Cannot have children of type" + obj.GetType ());

			Text = ((LiteralControl) obj).Text;
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			bool enabled = Enabled;
			if (Page != null) {
				if (AutoPostBack && enabled)
					Page.RequiresPostBackScript ();
			}

			/* Don't save passwords in ViewState */
			if (TextMode == TextBoxMode.Password ||
			    (enabled && Visible && Events [TextChangedEvent] == null))
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

#if NET_2_0
		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}
		
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
#else
		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
#endif
		{
			if (postCollection [postDataKey] != Text){
				Text = postCollection [postDataKey];
				return true;
			}
			return false;
		}

#if NET_2_0
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}
		
		protected virtual void RaisePostDataChangedEvent ()
		{
			OnTextChanged (EventArgs.Empty);
		}
#else
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnTextChanged (EventArgs.Empty);
		}
#endif
	}
}

