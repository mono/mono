/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Style
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  10%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class Style : Component, IStateManager
	{
		internal static MARKED    = 0x01;
		internal static BACKCOLOR = (0x01 < 1);
		
		private StateBag viewState;
		private bool     marked;
		private int      selectionBits;
		private bool     selfStateBag;
		
		public Style()
		{
			Initialize(null);
			selfStateBag = true;
			
		}
		
		public Style(StateBag bag): base()
		{
			Initialize(bag);
			selfStateBag = false;
		}
		
		private void Initialize(StateBag bag)
		{
			viewState     = bag;
			marked        = false;
			selectionBits = 0x00;
		}
		
		StateBag ViewState
		{
			get
			{
				if(stateBag == null)
				{
					stateBag = new stateBag(false);
					if(IsTrackingViewState)
						stateBag.TrackViewState();
				}
				return stateBag;
			}
		}
		
		
	}
}

