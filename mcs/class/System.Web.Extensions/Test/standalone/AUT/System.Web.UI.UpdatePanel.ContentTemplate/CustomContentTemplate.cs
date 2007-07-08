using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public class CustomContentTemplate : System.Web.UI.ITemplate
{
	public CustomContentTemplate()
	{
		//contructor
	}

	public void InstantiateIn(System.Web.UI.Control container)
	{
		PlaceHolder ph = new PlaceHolder();
		Label label1 = new Label();
		label1.ID = "Label1";
		label1.Text = "A full page postback occurred.";
		Button button1 = new Button();
		button1.ID = "Button1";
		button1.Text = "Refresh Panel";
		button1.Click += new EventHandler(button1_Click);
		ph.Controls.Add(label1);
		ph.Controls.Add(new LiteralControl("<br/>"));
		ph.Controls.Add(button1);
		container.Controls.Add(ph);
	}

	void button1_Click(object sender, EventArgs e)
	{
		Button button1 = (Button)sender;
		Page page = button1.Page;
		Label label1 = (Label)page.FindControl("Label1");
		label1.Text = "Panel refreshed at " + DateTime.Now.ToString();

	}
}
