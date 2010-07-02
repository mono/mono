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
// Copyright (c) 2005-2010 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ValidationProperty("SelectedItem")]
	[SupportsEventValidation]
	public class DropDownList : ListControl, IPostBackDataHandler
	{
		#region Public Constructors
		public DropDownList() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[Browsable(false)]
		public override Color BorderColor {
			get { return base.BorderColor; }
			set { base.BorderColor = value; }
		}

		[Browsable(false)]
		public override BorderStyle BorderStyle {
			get { return base.BorderStyle; }
			set { base.BorderStyle = value; }
		}

		[Browsable(false)]
		public override Unit BorderWidth {
			get { return base.BorderWidth; }
			set { base.BorderWidth = value; }
		}

		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public override int SelectedIndex {
			get {
				int selected;

				selected = base.SelectedIndex;
				if ((selected != -1) || (Items.Count == 0))
					return selected;

				Items[0].Selected = true;
				return 0;
			}

			set { base.SelectedIndex = value; }
		}
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			Page page = Page;
			if (page != null)
				page.VerifyRenderingInServerForm (this);

			if (writer == null)
				return;
			if (!String.IsNullOrEmpty (UniqueID))
				writer.AddAttribute (HtmlTextWriterAttribute.Name, this.UniqueID, true);

			if (!IsEnabled && SelectedIndex == -1)
				SelectedIndex = 1;

			if (AutoPostBack) {
				string onchange = page != null ? page.ClientScript.GetPostBackEventReference (GetPostBackOptions (), true) : String.Empty;
				onchange = String.Concat ("setTimeout('", onchange.Replace ("\\", "\\\\").Replace ("'", "\\'"), "', 0)");
				writer.AddAttribute (HtmlTextWriterAttribute.Onchange, BuildScriptAttribute ("onchange", onchange));
			}

			base.AddAttributesToRender(writer);
		}

		PostBackOptions GetPostBackOptions ()
		{
			PostBackOptions options = new PostBackOptions (this);
			options.ActionUrl = null;
			options.ValidationGroup = null;
			options.Argument = String.Empty;
			options.RequiresJavaScriptProtocol = false;
			options.ClientSubmit = true;

			Page page = Page;
			options.PerformValidation = CausesValidation && page != null && page.AreValidatorsUplevel (ValidationGroup);
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return base.CreateControlCollection();
		}

		protected internal override void VerifyMultiSelect ()
		{
			throw new HttpException ("DropDownList only may have a single selected item");
		}		
		#endregion	// Protected Instance Methods

		#region	Interface Methods
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			EnsureDataBound ();
			int index = Items.IndexOf(postCollection[postDataKey]);
			ValidateEvent (postDataKey, postCollection [postDataKey]);
			if (index != this.SelectedIndex) {
				SelectedIndex = index;
				return true;
			}
			
			return false;
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			if (CausesValidation) {
				Page page = Page;
				if (page != null)
					page.Validate (ValidationGroup);
			}
			
			OnSelectedIndexChanged(EventArgs.Empty);
		}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}
		#endregion	// Interface Methods
	}
}
