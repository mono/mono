using System;
using System.Collections.Generic;
using System.IO;
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
		public string ExistingDataField {
			get;
			set;
		}

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

		public string RenderToString ()
		{
			var sb = new StringBuilder ();
			Render (new HtmlTextWriter (new StringWriter (sb)));
			return sb.ToString ();
		}

		protected override void OnInit (EventArgs e)
		{
			string existingField = ExistingDataField;
			if (!String.IsNullOrEmpty (existingField))
				Column = Table.GetColumn (existingField);

			base.OnInit (e);
		}
	}
}
