//
// System.Web.UI.WebControls.HiddenField.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc. (http://www.novell.com)
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

using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public class HiddenField : Control, IPostBackDataHandler
	{
		static readonly object ValueChangedEvent = new object ();

		public event EventHandler ValueChanged {
			add { Events.AddHandler (ValueChangedEvent, value); }
			remove { Events.RemoveHandler (ValueChangedEvent, value); }
		}

		[Bindable (true)]
		public virtual string Value {
			get { return ViewState.GetString ("Value", String.Empty); }
			set { ViewState ["Value"] = value; }
		}

		public override bool EnableTheming {
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		public override string SkinID {
			get { return String.Empty; }
			set { throw new NotSupportedException (); }
		}

		public override void Focus ()
		{
			throw new NotSupportedException ();
		}

		protected virtual void OnValueChanged (EventArgs e)
		{
			EventHandler h = (EventHandler) Events [ValueChangedEvent];
			if (h != null)
				h (this, e);
		}

		protected virtual bool LoadPostData (string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			if (Value != postCollection [postDataKey]) {
				Value = postCollection [postDataKey];
				return true;
			}
			return false;
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			ValidateEvent (UniqueID, String.Empty);
			OnValueChanged (EventArgs.Empty);
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new System.Web.UI.EmptyControlCollection (this);
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			Page page = Page;
			string uniqueid = UniqueID;
			if (page != null)
				page.ClientScript.RegisterForEventValidation (uniqueid);
			
			writer.AddAttribute (HtmlTextWriterAttribute.Type, "hidden", false);

			if (!String.IsNullOrEmpty (ClientID))
				writer.AddAttribute (HtmlTextWriterAttribute.Id, ClientID);

			if (!String.IsNullOrEmpty (uniqueid))
				writer.AddAttribute (HtmlTextWriterAttribute.Name, uniqueid);

			if (!String.IsNullOrEmpty (Value))
				writer.AddAttribute (HtmlTextWriterAttribute.Value, Value);

			writer.RenderBeginTag (HtmlTextWriterTag.Input);
			writer.RenderEndTag ();	
		}

		#region IPostBackDataHandler Members

		bool IPostBackDataHandler.LoadPostData (string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}

		#endregion
	}
}

