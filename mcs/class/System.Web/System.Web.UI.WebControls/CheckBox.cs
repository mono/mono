//
// System.Web.UI.WebControls.CheckBox.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
// Thanks to Leen Toelen (toelen@hotmail.com)'s classes that helped me
// to write the contents of the function LoadPostData(...)
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
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls
{
#if NET_2_0
	[ControlValuePropertyAttribute ("Checked")]
#endif
	[DefaultEvent("CheckedChanged")]
	[DefaultProperty("Text")]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[Designer ("System.Web.UI.Design.WebControls.CheckBoxDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	public class CheckBox : WebControl, IPostBackDataHandler
	{
		private static readonly object CheckedChangedEvent = new object();
		AttributeCollection commonAttrs;
		
		public CheckBox(): base(HtmlTextWriterTag.Input)
		{
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
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


#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (false), Bindable (true)]
		[WebSysDescription ("Determines if the control is checked.")]
		public virtual bool Checked
		{
			get {
				object o = ViewState ["Checked"];
				return (o == null) ? false : (bool) o;
			}

			set { ViewState ["Checked"] = value; }
		}

#if NET_2_0
	    [Localizable (true)]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The text that this control displays.")]
		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}
		
		private bool SaveCheckedViewState
		{
			get {
				if (Events [CheckedChangedEvent] != null || !Enabled)
					return true;

				Type type = GetType ();
				return (type != typeof (CheckBox) && type != typeof (RadioButton));
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (TextAlign), "Right"), WebCategory ("Appearance")]
		[WebSysDescription ("The alignment of the text.")]
		public virtual TextAlign TextAlign
		{
			get {
				object o = ViewState ["TextAlign"];
				return (o == null) ? TextAlign.Right : (TextAlign) o;
			}

			set {
				if (!System.Enum.IsDefined (typeof (TextAlign), value))
					throw new ArgumentException ();
				ViewState ["TextAlign"] = value;
			}
		}

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when the control is checked or unchecked.")]
		public event EventHandler CheckedChanged
		{
			add { Events.AddHandler (CheckedChangedEvent, value); }
			remove { Events.RemoveHandler (CheckedChangedEvent, value); }
		}
		
		protected virtual void OnCheckedChanged(EventArgs e)
		{
			if(Events != null){
				EventHandler eh = (EventHandler) (Events [CheckedChangedEvent]);
				if(eh != null)
					eh (this, e);
			}
		}
		
		protected override void OnPreRender(EventArgs e)
		{
			if (Page != null && Enabled) {
				Page.RegisterRequiresPostBack (this);
				if (AutoPostBack)
					Page.RequiresPostBackScript ();
			}

			if (!SaveCheckedViewState)
				ViewState.SetItemDirty ("Checked", false);
		}

		static bool IsInputOrCommonAttr (string attname)
		{
			switch (attname) {
			case "VALUE":
			case "CHECKED":
			case "SIZE":
			case "MAXLENGTH":
			case "SRC":
			case "ALT":
			case "USEMAP":
			case "DISABLED":
			case "READONLY":
			case "ACCEPT":
			case "ACCESSKEY":
			case "TABINDEX":
			case "ONFOCUS":
			case "ONBLUR":
			case "ONSELECT":
			case "ONCHANGE":
			case "ONCLICK":
			case "ONDBLCLICK":
			case "ONMOUSEDOWN":
			case "ONMOUSEUP":
			case "ONMOUSEOVER":
			case "ONMOUSEMOVE":
			case "ONMOUSEOUT":
			case "ONKEYPRESS":
			case "ONKEYDOWN":
			case "ONKEYUP":
				return true;
			default:
				return false;
			}
		}

		void AddAttributesForSpan (HtmlTextWriter writer)
		{
			ICollection k = Attributes.Keys;
			string [] keys = new string [k.Count];
			k.CopyTo (keys, 0);
			foreach (string key in keys) {
				if (!IsInputOrCommonAttr (key.ToUpper ()))
					continue;

				if (commonAttrs == null)
					commonAttrs = new AttributeCollection (new StateBag ());

				commonAttrs [key] = Attributes [key];
				Attributes.Remove (key);
			}

			Attributes.AddAttributes (writer);
		}

		protected override void Render (HtmlTextWriter writer)
		{
			bool hasBeginRendering = false;
			if(ControlStyleCreated && !ControlStyle.IsEmpty){
				hasBeginRendering = true;
				ControlStyle.AddAttributesToRender (writer, this);
			}
			
			if (!Enabled)
			{
				hasBeginRendering = true;
 				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");				
			}

			if (ToolTip.Length > 0){
				hasBeginRendering = true;
				writer.AddAttribute (HtmlTextWriterAttribute.Title, ToolTip);
			}

			if (Attributes.Count > 0){
				hasBeginRendering = true;
				AddAttributesForSpan (writer);
			}

			if (hasBeginRendering)
				writer.RenderBeginTag (HtmlTextWriterTag.Span);

			if (Text.Length > 0){
				TextAlign ta = TextAlign;
				if(ta == TextAlign.Right) {
					if (commonAttrs != null)
						commonAttrs.AddAttributes (writer);
					RenderInputTag (writer, ClientID);
				}
				writer.AddAttribute (HtmlTextWriterAttribute.For, ClientID);
				writer.RenderBeginTag (HtmlTextWriterTag.Label);
				writer.Write (Text);
				writer.RenderEndTag ();
				if(ta == TextAlign.Left) {
					if (commonAttrs != null)
						commonAttrs.AddAttributes (writer);
					RenderInputTag (writer, ClientID);
				}
			} else {
				if (commonAttrs != null)
					commonAttrs.AddAttributes (writer);
				RenderInputTag (writer, ClientID);
			}

			if (hasBeginRendering)
				writer.RenderEndTag ();
		}
		
		internal virtual void RenderInputTag (HtmlTextWriter writer, string clientId)
		{
			if (!Enabled)
				writer.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled");

			writer.AddAttribute (HtmlTextWriterAttribute.Id, clientId);
			writer.AddAttribute( HtmlTextWriterAttribute.Type, "checkbox");
			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
			if (Checked)
				writer.AddAttribute (HtmlTextWriterAttribute.Checked, "checked");

			if (AutoPostBack){
				writer.AddAttribute (HtmlTextWriterAttribute.Onclick,
						     Page.GetPostBackClientEvent (this, String.Empty));
				writer.AddAttribute ("language", "javascript");
			}

			if (AccessKey.Length > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Accesskey, AccessKey);

			if (TabIndex != 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Tabindex,
						     TabIndex.ToString (NumberFormatInfo.InvariantInfo));

			writer.RenderBeginTag (HtmlTextWriterTag.Input);
			writer.RenderEndTag ();
		}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			string postedVal = postCollection [postDataKey];			
			bool haveData = ((postedVal != null)&& (postedVal.Length > 0));
			bool diff  = (haveData != Checked);
			Checked = haveData;
			return diff ;
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			OnCheckedChanged (EventArgs.Empty);
		}
	}
}
