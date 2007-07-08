<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void Button1_Click(object sender, EventArgs e)
    {
        try
        {
            int a = Int32.Parse(TextBox1.Text);
            int b = Int32.Parse(TextBox2.Text);
            int res = a / b;
            Label1.Text = res.ToString();
        }
        catch (Exception ex)
        {
            if (TextBox1.Text.Length > 0 && TextBox2.Text.Length > 0)
            {
                ex.Data["ExtraInfo"] = " You can't divide " +
                    TextBox1.Text + " by " + TextBox2.Text + ".";
            }
            throw ex;
        }        
    }
    protected void ScriptManager1_AsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
    {
        if (e.Exception.Data["ExtraInfo"] != null)
        {
            ScriptManager1.AsyncPostBackErrorMessage =
                e.Exception.Message +
                e.Exception.Data["ExtraInfo"].ToString();
        }
        else
        {
            ScriptManager1.AsyncPostBackErrorMessage =
                "An unspecified error occurred.";
        }
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Partial-Page Update Error Handling Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" OnAsyncPostBackError="ScriptManager1_AsyncPostBackError">
            </asp:ScriptManager>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:TextBox ID="TextBox1" runat="server" Width="39px"></asp:TextBox>
                    /
                    <asp:TextBox ID="TextBox2" runat="server" Width="39px"></asp:TextBox>
                    =
                    <asp:Label ID="Label1" runat="server"></asp:Label><br />
                    <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="calculate" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
