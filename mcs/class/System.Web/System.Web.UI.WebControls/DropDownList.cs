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
using System.Drawing;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ValidationProperty("SelectedItem")]
	public class DropDownList : ListControl, IPostBackDataHandler
#if NET_2_0
		, ITextControl
#endif
	{
#if NET_2_0
		private static readonly object TextChangedEvent = new object();
#endif
		
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

#if !NET_2_0
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
#endif

#if NET_2_0

		[MonoTODO ("Make sure that the following attributes are correct")]
		[DefaultValue (null)]
		[ThemeableAttribute (false)]
		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Text {
			get {
				if (SelectedItem != null) return SelectedItem.Text;
				else return null;
			}
			set {
				for (int n=0; n < Items.Count; n++) {
					if (Items[n].Text == value) {
						SelectedIndex = n;
						return;
					}
				}
				SelectedIndex = -1;
			}
		}
		
		[WebCategory ("Action")]
		public event EventHandler TextChanged
		{
			add {
				Events.AddHandler (TextChangedEvent, value);
			}
			remove {
				Events.RemoveHandler (TextChangedEvent, value);
			}
		}
		
		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);
			OnTextChanged (e);
		}
		
		protected virtual void OnTextChanged (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler)(Events[TextChangedEvent]);
				if (eh != null)
					eh (this, e);
			}
		}
#endif

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

#if !NET_2_0
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
#else
		protected internal override void VerifyMultiSelect ()
		{
			throw new HttpException(HttpRuntime.FormatResourceString("Cannot_Multiselect_In_DropDownList"));
		}
#endif


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

#if NET_2_0
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}
		
		protected virtual void RaisePostDataChangedEvent ()
		{
			if (CausesValidation)
				Page.Validate (ValidationGroup);

			OnSelectedIndexChanged (EventArgs.Empty);
		}
#else
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnSelectedIndexChanged (EventArgs.Empty);
		}
#endif
	}
}
