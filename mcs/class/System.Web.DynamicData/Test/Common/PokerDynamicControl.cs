using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.Common
{
	public class PokerDynamicControl : DynamicControl
	{
		public override string ID {
			get {
				string id = base.ID;
				if (String.IsNullOrEmpty (id))
					return DataField;

				return id;
			}

			set { base.ID = value; }
		}

		public object GetViewState ()
		{
			return SaveViewState ();
		}
	}
}
