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
	[DefaultEvent("CheckedChanged")]
	[DefaultProperty("Text")]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[Designer ("System.Web.UI.Design.WebControls.CheckBoxDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	public class CheckBox : WebControl, IPostBackDataHandler
	{
		private static readonly object CheckedChangedEvent = new object();
		
		public CheckBox(): base(HtmlTextWriterTag.Input)
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
				if (Events [CheckedChangedEvent] != null){
					if (!Enabled)
						return true;

					Type type = GetType ();
					if (type == typeof (CheckBox))
						return false;

					if (type == typeof (RadioButton))
						return false;
				}
				return true;
			}
		}


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
			if (Page != null && Enabled)
				Page.RegisterRequiresPostBack (this);

			if (SaveCheckedViewState)
				ViewState.SetItemDirty ("checked", false);
		}
		
		protected override void Render (HtmlTextWriter writer)
		{
			bool hasBeginRendering = false;
			if(ControlStyleCreated && !ControlStyle.IsEmpty){
				hasBeginRendering = true;
				ControlStyle.AddAttributesToRender (writer, this);
			}

			if (!Enabled){
				hasBeginRendering = true;
				writer.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled");
			}

			if (ToolTip.Length > 0){
				hasBeginRendering = true;
				writer.AddAttribute (HtmlTextWriterAttribute.Title, ToolTip);
			}

			if (Attributes.Count > 0){
				string val = Attributes ["value"];
				Attributes.Remove ("value");
				if (Attributes.Count > 0){
					hasBeginRendering = true;
					Attributes.AddAttributes (writer);
				}

				if (val != null)
					Attributes ["value"] = val;
			}

			if (hasBeginRendering)
				writer.RenderBeginTag (HtmlTextWriterTag.Span);

			if (Text.Length > 0){
				TextAlign ta = TextAlign;
				if(ta == TextAlign.Right)
					RenderInputTag (writer, ClientID);
				writer.AddAttribute (HtmlTextWriterAttribute.For, ClientID);
				writer.RenderBeginTag (HtmlTextWriterTag.Label);
				writer.Write (Text);
				writer.RenderEndTag ();
				if(ta == TextAlign.Left)
					RenderInputTag (writer, ClientID);
			}
			else
				RenderInputTag (writer, ClientID);

			if (hasBeginRendering)
				writer.RenderEndTag ();
		}
		
		internal virtual void RenderInputTag (HtmlTextWriter writer, string clientId)
		{
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
			bool   postChecked = false;
			if(postedVal != null)
				postChecked = postedVal.Length > 0;
			Checked = postChecked;
			return (postChecked == false);
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			OnCheckedChanged (EventArgs.Empty);
		}
	}
}
