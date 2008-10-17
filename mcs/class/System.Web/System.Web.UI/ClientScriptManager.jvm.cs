//
// (C) 2007 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections;

namespace System.Web.UI
{
	partial class ClientScriptManager : IStateManager
	{
		#region IStateManager Members

		void IStateManager.LoadViewState (object state) {
			throw new NotSupportedException ();
		}

		object IStateManager.SaveViewState () {
			if (hiddenFields != null) {
				Hashtable clone = (Hashtable) hiddenFields.Clone ();
				clone.Remove ("__VIEWSTATE");
				clone.Remove (Page.postEventArgumentID);
				clone.Remove (Page.postEventSourceID);
				clone.Remove (Page.CallbackArgumentID);
				clone.Remove (Page.CallbackSourceID);
				if (clone.Keys.Count > 0)
					return clone;
			}
			return null;
		}

		void IStateManager.TrackViewState () {
			throw new NotSupportedException ();
		}

		bool IStateManager.IsTrackingViewState {
			get { throw new NotSupportedException (); }
		}

		#endregion
	}
}
