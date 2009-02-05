using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class _Bug471305_Default : System.Web.UI.Page 
{
    public class CustomControl : Control
    {
        protected override void OnInit(EventArgs e)
        {
            Label label = new Label();
            label.Text = "label";
            Controls.Add(label);

            base.OnInit(e);
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        CustomControl ctrl = new CustomControl();
        Form.Controls.Add(ctrl);
        Form.Controls.Remove(ctrl);
        Form.Controls.Add(ctrl);
    }
}
