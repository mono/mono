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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls {
	[ValidationProperty("SelectedItem")]
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
#if NET_2_0
		public virtual
#else		
		public
#endif
		int Rows {
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
#if NET_2_0
		public virtual
#else		
		public
#endif		
		ListSelectionMode SelectionMode {
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
		[MonoTODO]
		public virtual int[] GetSelectedIndices ()
		{
			throw new NotImplementedException ();
		}
#endif		
		
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);

			writer.AddAttribute (HtmlTextWriterAttribute.Name, ClientID);

			if (SelectionMode == ListSelectionMode.Multiple)
				writer.AddAttribute (HtmlTextWriterAttribute.Multiple,
						"multiple");
			writer.AddAttribute (HtmlTextWriterAttribute.Size,
                                        Rows.ToString (CultureInfo.InvariantCulture));
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderContents (HtmlTextWriter writer)
		{
			base.RenderContents (writer);

			foreach (ListItem item in Items) {
				writer.WriteBeginTag ("option");
				if (item.Selected) {
					writer.WriteAttribute ("selected", "selected", false);
				}
				writer.WriteAttribute ("value", item.Value, true);

				writer.Write (">");
				writer.Write (item.Text);
				writer.WriteEndTag ("option");
				writer.WriteLine ();
			}
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Page != null)
				Page.RegisterRequiresPostBack (this);
		}

#if NET_2_0
		[MonoTODO]
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void RaisePostDataChangedEvent ()
		{
			throw new NotImplementedException ();
		}
#endif		
			
		bool IPostBackDataHandler.LoadPostData (string postDataKey,
				NameValueCollection postCollection)
		{
			string [] items = postCollection.GetValues (postDataKey) as string [];
			bool res = false;

			if (items == null)
				return false;

			foreach (string value in items) {
				ListItem item = Items.FindByValue (value);
				if (!item.Selected) {
					item.Selected = true;
					res = true;
				}
			}

			// So we can tell when they have been changed
			Items.TrackViewState ();
			return res;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnSelectedIndexChanged (EventArgs.Empty);
		}
	}
}

