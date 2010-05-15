using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _default : System.Web.UI.Page
{
	protected void Page_Load (object sender, EventArgs e)
	{
		var list = new List<string> {
		    "One",
		    "Two",
		    "Three"
		};

		try {
			GetUniqueIDRelativeTo (null);
		} catch (ArgumentNullException ex) {
			log.InnerText += String.Format ("Page; Relative to: null; Result: exception {0} (expected)\n", ex.GetType ());
		} catch (Exception ex) {
			log.InnerText += String.Format ("Page; Relative to: null; Result: exception {0}\n", ex.GetType ());
		}

		var ctl = new Control ();
		try {
			ctl.GetUniqueIDRelativeTo (this);
		} catch (InvalidOperationException ex) {
			log.InnerText += String.Format ("A control; Relative to: {0}; Result: exception {1} (expected)\n", this.UniqueID, ex.GetType ());
		} catch (Exception ex) {
			log.InnerText += String.Format ("A control; Relative to: {0}; Result: exception {1}\n", this.UniqueID, ex.GetType ());
		}

		try {
			textBox1.GetUniqueIDRelativeTo (this);
		} catch (InvalidOperationException ex) {
			log.InnerText += String.Format ("TextBox; Relative to: {0}; Result: exception {1} (expected)\n", this.UniqueID, ex.GetType ());
		} catch (Exception ex) {
			log.InnerText += String.Format ("TextBox; Relative to: {0}; Result: exception {1}\n", this.UniqueID, ex.GetType ());
		}

		repeater1.DataSource = list;
		repeater1.DataBind ();
	}

	protected void OnItemDataBound_Repeater1 (object sender, RepeaterItemEventArgs args)
	{
		if (args.Item.ItemType == ListItemType.Separator)
			return;
		
		int index = args.Item.ItemIndex;
		var sb = new StringBuilder ();
		Label label = args.Item.FindControl ("label1") as Label;

		string id = label.GetUniqueIDRelativeTo (args.Item);
		LogItem (index, label, args.Item, sb);

		id = label.GetUniqueIDRelativeTo (repeater1);
		LogItem (index, label, repeater1, sb);

		try {
			id = label.GetUniqueIDRelativeTo (this);
		} catch (InvalidOperationException ex) {
			sb.AppendFormat ("Item: {0}; Relative to: {1}; Result: exception {2} (expected)\n", args.Item.ItemIndex, this.UniqueID, ex.GetType ());
		} catch (Exception ex) {
			sb.AppendFormat ("Item: {0}; Relative to: {1}; Result: exception {2}\n", args.Item.ItemIndex, this.UniqueID, ex.GetType ());
		}

		log.InnerText += sb.ToString ();

		int listStart = index * 3;
		var list = new List<int> {
			listStart,
			listStart + 1,
			listStart + 2
		};
		Repeater innerRepeater = args.Item.FindControl ("innerRepeater1") as Repeater;
		innerRepeater.DataSource = list;
		innerRepeater.DataBind ();
	}

	protected void OnItemDataBound_InnerRepeater1 (object sender, RepeaterItemEventArgs args)
	{
		if (args.Item.ItemType == ListItemType.Separator)
			return;

		int index = args.Item.ItemIndex;
		var sb = new StringBuilder ();
		Label label = args.Item.FindControl ("innerLabel1") as Label;

		string id = label.GetUniqueIDRelativeTo (args.Item);
		LogItem (index, label, args.Item, sb, "\t");

		id = label.GetUniqueIDRelativeTo (repeater1);
		LogItem (index, label, repeater1, sb, "\t");

		id = label.GetUniqueIDRelativeTo (args.Item.Parent);
		LogItem (index, label, args.Item.Parent, sb, "\t");

		try {
			id = label.GetUniqueIDRelativeTo (this);
		} catch (InvalidOperationException ex) {
			sb.AppendFormat ("\tItem: {0}; Relative to: {1}; Result: exception {2} (expected)\n", args.Item.ItemIndex, this.UniqueID, ex.GetType ());
		} catch (Exception ex) {
			sb.AppendFormat ("\tItem: {0}; Relative to: {1}; Result: exception {2}\n", args.Item.ItemIndex, this.UniqueID, ex.GetType ());
		}

		log.InnerText += sb.ToString ();
	}

	void LogItem (int index, Control ctl, Control relativeTo, StringBuilder sb, string indent = "")
	{
		string id = ctl.GetUniqueIDRelativeTo (relativeTo);
		sb.AppendFormat ("{0}Item: {1}; Relative to: {2}; Result: '{3}'\n", indent, index, relativeTo.UniqueID, id);
	}
}