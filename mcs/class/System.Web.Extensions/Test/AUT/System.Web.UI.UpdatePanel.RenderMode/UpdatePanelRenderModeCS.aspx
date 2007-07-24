
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected int PostBackCount
    {
        get
        {
            return (int)(ViewState["PostBackCount"] ?? 0);
        }
        set
        {
            ViewState["PostBackCount"] = value;
        }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            PostBackCount++;
        }
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanelRenderMode Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1"
                               runat="server" />
            The number of times you have clicked the button is
            <asp:UpdatePanel ID="UpdatePanel1"
                             UpdateMode="Conditional"
                             RenderMode="Inline"
                             runat="server">
                <ContentTemplate>
                    <%= PostBackCount.ToString() %>
                    times. Every time you click the count is incremented. The panel
                    containing the number of times you clicked is rendered in-line.
                    <br />
                    <asp:Button ID="Button1"
                                Text="Increment"
                                runat="server" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
