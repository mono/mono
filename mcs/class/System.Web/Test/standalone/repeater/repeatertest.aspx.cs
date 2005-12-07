using System;
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;
using System.Xml.XPath;

namespace test
{
  public class SimplePage : System.Web.UI.Page
  {
    protected XmlDataSource XmlDataSource;

    public SimplePage()
    {
      RepeaterTest t = new RepeaterTest ();

      Controls.Add (t);

#if true
      t.DataSourceID = "XmlDataSource";
#else
      t.DataSource = XmlDataSource;
#endif
    }
  }

  public class RepeaterTest : Repeater {
  	public override void DataBind ()
  	{
		Page.Response.Write (String.Format ("<pre>In DataBind, from {0}</pre>", Environment.StackTrace));
		base.DataBind ();
	}

  	protected override void CreateControlHierarchy (bool useDataSource) {
		Page.Response.Write (String.Format ("<pre>In CreateControlHierarchy({0}), from {1}</pre>", useDataSource, Environment.StackTrace));
		base.CreateControlHierarchy (useDataSource);
	}

  	protected override void OnInit (EventArgs e) {
		Page.Response.Write (String.Format ("<pre>In OnInit, from {0}</pre>", Environment.StackTrace));
		base.OnInit (e);
	}

  	protected override IEnumerable GetData () {
		Page.Response.Write (String.Format ("<pre>In GetData, from {0}</pre>", Environment.StackTrace));

		IEnumerable data = base.GetData();

		IEnumerator e = data.GetEnumerator();

		while (e.MoveNext()) {
			Page.Response.Write (String.Format (" + {0}<br/>", e.Current));
			IXPathNavigable desc = (IXPathNavigable)e.Current;
			Page.Response.Write (String.Format ("+ + navigator = {0}<br/>", desc.CreateNavigator().GetType()));
		}

		return data;
	}
#if false
    /* can't do this one, as it is invoked in the setter for DataSourceID. */

  	protected override void OnDataPropertyChanged () {
		if (Page != null)
			Page.Response.Write (String.Format ("<pre>In OnDataPropertyChanged, from {0}</pre>", Environment.StackTrace));
		base.OnDataPropertyChanged ();
	}
#endif
  	protected override void OnDataSourceViewChanged (object sender, EventArgs e) {
		Page.Response.Write (String.Format ("<pre>In OnDataSourceViewChanged, from {0}</pre>", Environment.StackTrace));
		base.OnDataSourceViewChanged (sender, e);
	}

    protected override RepeaterItem CreateItem (int itemIndex, ListItemType itemType) {
      Page.Response.Write (String.Format ("<pre>In CreateItem, from {0}</pre>", Environment.StackTrace));
      return base.CreateItem (itemIndex, itemType);
    }

    protected override void InitializeItem (RepeaterItem item) {
      Page.Response.Write (String.Format ("<pre>In InitializeItem, from {0}</pre>", Environment.StackTrace));
      base.InitializeItem (item);
    }
  }
}
