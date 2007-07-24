
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
     "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        UpdatePanel up1 = new UpdatePanel();
        up1.ID = "UpdatePanel1";
        up1.UpdateMode = UpdatePanelUpdateMode.Conditional;
        Button button1 = new Button();
        button1.ID = "Button1";
        button1.Text = "Submit";
        button1.Click += new EventHandler(Button_Click);
        Label label1 = new Label();
        label1.ID = "Label1";
        label1.Text = "A full page postback occurred.";
        up1.ContentTemplateContainer.Controls.Add(button1);
        up1.ContentTemplateContainer.Controls.Add(label1);
        Page.Form.Controls.Add(up1);
    }
    protected void Button_Click(object sender, EventArgs e)
    {
        ((Label)Page.FindControl("Label1")).Text = "Panel refreshed at " +
            DateTime.Now.ToString();
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanel Constructor Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1"
                               runat="server" />
        </div>
    </form>
</body>
</html>
