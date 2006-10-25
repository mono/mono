//
// System.Web.UI.WebControls.MailDefinition.cs
//
// Authors:
//	Igor Zelmanovich (igorz@mainsoft.com)
//
// (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Web.UI;
using System.Net.Mail;
using System.Collections;

namespace System.Web.UI.WebControls
{
	[BindableAttribute (false)]
	public sealed class MailDefinition : IStateManager
	{
		StateBag _bag = new StateBag ();

		[DefaultValue ("")]
		[NotifyParentProperty (true)]
		public string BodyFileName {
			get { return _bag.GetString ("BodyFileName", String.Empty); }
			set { _bag ["BodyFileName"] = value; }
		}

		[DefaultValue ("")]
		[NotifyParentProperty (true)]
		public string CC {
			get { return _bag.GetString ("CC", String.Empty); }
			set { _bag ["CC"] = value; }
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[NotifyParentProperty (true)]
		public EmbeddedMailObjectsCollection EmbeddedObjects {
			get { throw new NotImplementedException (); }
		}

		[NotifyParentProperty (true)]
		[DefaultValue ("")]
		public string From {
			get { return _bag.GetString ("From", String.Empty); }
			set { _bag ["From"] = value; }
		}

		[DefaultValue (false)]
		[NotifyParentProperty (true)]
		public bool IsBodyHtml {
			get { return _bag.GetBool ("IsBodyHtml", false); }
			set { _bag ["IsBodyHtml"] = value; }
		}

		[NotifyParentProperty (true)]
		public MailPriority Priority {
			get { return _bag ["Priority"] == null ? MailPriority.Normal : (MailPriority) _bag ["Priority"]; }
			set { _bag ["Priority"] = value; }
		}

		[DefaultValue ("")]
		[NotifyParentProperty (true)]
		public string Subject {
			get { return _bag.GetString ("Subject", String.Empty); }
			set { _bag ["Subject"] = value; }
		}

		[MonoTODO]
		public MailMessage CreateMailMessage (string recipients, IDictionary replacements, Control owner) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MailMessage CreateMailMessage (string recipients, IDictionary replacements, string body, Control owner) {
			throw new NotImplementedException ();
		}

#region IStateManager Members

		void IStateManager.LoadViewState (object state) {
			_bag.LoadViewState (state);
		}

		object IStateManager.SaveViewState () {
			return _bag.SaveViewState ();
		}

		void IStateManager.TrackViewState () {
			_bag.TrackViewState ();
		}

		bool IStateManager.IsTrackingViewState {
			get { return _bag.IsTrackingViewState; }
		}

#endregion
	}
}

#endif