// Bug #594238
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TestNamedHolders
{
	public class MyContainer : WebControl, INamingContainer
	{
		Control whereTheChildrenPlay;
		
		// can't do this if it is an INamingContainer
		public override ControlCollection Controls 
		{
			get { return whereTheChildrenPlay.Controls;	}
		}
		
		public MyContainer()
		{
			whereTheChildrenPlay = new Content();
			whereTheChildrenPlay.ID = "children";
		}

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			
			// would normally put other stuff here
			
			base.Controls.Add(whereTheChildrenPlay);

			// and possibly here
		}

	}
}
