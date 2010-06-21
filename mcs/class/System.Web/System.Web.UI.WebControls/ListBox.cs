//
// System.Web.UI.WebControls.Literal.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Collections.Specialized;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ValidationProperty("SelectedItem")]
#if NET_2_0
	[SupportsEventValidation]
#endif
	public class ListBox : ListControl, IPostBackDataHandler {

		public ListBox ()
		{
		}

		[Browsable(false)]
#if NET_2_0 && HAVE_CONTROL_ADAPTERS
		public virtual new
#else		
		public override
#endif
		Color BorderColor {
			get { return base.BorderColor; }
			set { base.BorderColor = value; }
		}

		[Browsable(false)]
#if NET_2_0 && HAVE_CONTROL_ADAPTERS
		public virtual new
#else		
		public override
#endif
		BorderStyle BorderStyle {
			get { return base.BorderStyle; }
			set { base.BorderStyle = value; }
		}

		[Browsable(false)]
#if NET_2_0 && HAVE_CONTROL_ADAPTERS
		public virtual new
#else		
		public override
#endif
		Unit BorderWidth {
			get { return base.BorderWidth; }
			set { base.BorderWidth = value; }
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(4)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int Rows {
			get {
				return ViewState.GetInt ("Rows", 4);
			}
			set {
				if (value < 1 || value > 2000)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["Rows"] = value;
			}
		}

		[DefaultValue(ListSelectionMode.Single)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual ListSelectionMode SelectionMode {
			get {
				return (ListSelectionMode) ViewState.GetInt ("SelectionMode",
						(int) ListSelectionMode.Single);
			}
			set {
				if (!Enum.IsDefined (typeof (ListSelectionMode), value))
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["SelectionMode"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string ToolTip {
			get { return String.Empty; }
			set { /* Tooltip is always String.Empty */ }
		}
#endif		

#if NET_2_0
		public virtual int[] GetSelectedIndices ()
		{
			return (int []) GetSelectedIndicesInternal ().ToArray (typeof (int));
		}
#endif		
		
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);

#if NET_2_0
			if (ID != null)
				writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
#else
			writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
#endif

			if (AutoPostBack) {
#if NET_2_0
				string onchange = Page.ClientScript.GetPostBackEventReference (GetPostBackOptions (), true);
				onchange = String.Concat ("setTimeout('", onchange.Replace ("\\", "\\\\").Replace ("'", "\\'"), "', 0)");
				writer.AddAttribute (HtmlTextWriterAttribute.Onchange, BuildScriptAttribute ("onchange", onchange));
#else
				writer.AddAttribute (HtmlTextWriterAttribute.Onchange,
						     BuildScriptAttribute ("onchange",
									   Page.ClientScript.GetPostBackClientHyperlink (this, "")));
#endif
			}
			
			if (SelectionMode == ListSelectionMode.Multiple)
				writer.AddAttribute (HtmlTextWriterAttribute.Multiple,
						"multiple", false);
			writer.AddAttribute (HtmlTextWriterAttribute.Size,
                                        Rows.ToString (Helpers.InvariantCulture));
			
			base.AddAttributesToRender (writer);
		}

#if NET_2_0
		PostBackOptions GetPostBackOptions () {
			PostBackOptions options = new PostBackOptions (this);
			options.ActionUrl = null;
			options.ValidationGroup = null;
			options.Argument = String.Empty;
			options.RequiresJavaScriptProtocol = false;
			options.ClientSubmit = true;
			options.PerformValidation = CausesValidation && Page != null && Page.AreValidatorsUplevel (ValidationGroup);
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}
#endif		

#if ONLY_1_1
		protected override void RenderContents (HtmlTextWriter writer)
		{
			foreach (ListItem item in Items) {
				if (item.Selected) {
					writer.AddAttribute (HtmlTextWriterAttribute.Selected, "selected", false);
				}
				writer.AddAttribute (HtmlTextWriterAttribute.Value, item.Value, true);
				writer.RenderBeginTag (HtmlTextWriterTag.Option);
				
				string encoded = HttpUtility.HtmlEncode (item.Text);
				writer.Write (encoded);
				writer.RenderEndTag ();
				writer.WriteLine ();
			}
		}
#endif

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			Page page = Page;
			if (page != null && IsEnabled)
				page.RegisterRequiresPostBack (this);
		}

#if NET_2_0
		protected virtual
#endif
		bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
#if NET_2_0
			EnsureDataBound ();
#endif
			string [] values = postCollection.GetValues (postDataKey);
			if (values == null || values.Length == 0) {
				int prev_index = SelectedIndex;
				SelectedIndex = -1;
				return (prev_index != -1);
			}
#if NET_2_0
			ValidateEvent (UniqueID, values [0]);
#endif

			if (SelectionMode == ListSelectionMode.Single)
				return SelectSingle (values);
			return SelectMultiple (values);
		}

		bool SelectSingle (string [] values)
		{
			string val = values [0];
			int idx = Items.IndexOf (val);
			int prev_index = SelectedIndex;
			if (idx != prev_index) {
				// This will set both the index value and the item.Selected property
				SelectedIndex = idx;
				return true;
			}
			return false;
		}

		bool SelectMultiple (string [] values)
		{
			ArrayList prev_selected = GetSelectedIndicesInternal ();
			ClearSelection ();
			foreach (string val in values) {
				ListItem item = Items.FindByValue (val);
				if (item != null)
					item.Selected = true;
			}

			ArrayList new_selection = GetSelectedIndicesInternal ();
			int i = prev_selected.Count;
			if (new_selection.Count != i)
				return true;

			while (--i >= 0) {
				if ((int) prev_selected [i] != (int) new_selection [i])
					return true;
			}

			return false;
		}

#if NET_2_0
		protected virtual
#endif
		void RaisePostDataChangedEvent ()
		{
#if NET_2_0
			if (CausesValidation)
				Page.Validate (ValidationGroup);
#endif
			OnSelectedIndexChanged (EventArgs.Empty);
		}
			
		bool IPostBackDataHandler.LoadPostData (string postDataKey,
							NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}
#if NET_2_0
        protected internal override void VerifyMultiSelect()
        {
            //by default the ListControl will throw an exception in this method,
            //therefor we should override the method if the class is supporting
            //MultiSelect option.
            if (this.SelectionMode != ListSelectionMode.Multiple)
                throw new HttpException("Multiple select option is not supported");
        }
#endif
	}
}



