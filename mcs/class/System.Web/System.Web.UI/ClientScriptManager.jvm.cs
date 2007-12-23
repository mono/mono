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
