/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : MobileListItemCollection
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web.UI;

namespace System.Web.UI.MobileControls
{
	public class MobileListItemCollection : ArrayListCollectionBase,
	                                        IStateManager
	{
		public MobileListItemCollection()
		{
		}

		void IStateManager.LoadViewState(object state)
		{
			throw new NotImplementedException();
		}

		object IStateManager.SaveViewState()
		{
			throw new NotImplementedException();
		}

		void IStateManager.TrackViewState()
		{
			throw new NotImplementedException();
		}

		bool IStateManager.IsTrackingViewState
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public void Add(string item)
		{
			throw new NotImplementedException();
		}

		public void Add(MobileListItem item)
		{
			throw new NotImplementedException();
		}
	}
}
