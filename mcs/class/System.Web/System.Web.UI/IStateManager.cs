//
// System.Web.UI.IStateManager.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Web;

namespace System.Web.UI
{
        public interface IStateManager
        {
                void LoadViewState(object state);
                object SaveViewState();
                void TrackViewState();
        	    bool IsTrackingViewState { get; }
        }
}
