<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    public void Page_Load(Object sender, EventArgs e)
    {
        if (!IsPostBack)
            Calendar1.SelectedDate = DateTime.Today;
        
    }
    protected void Page_PreRender(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptInclude(
            this,
            typeof(Page),
            "AlertScript",
            ResolveClientUrl("~/scripts/script_alertdiv.js"));
    }
    protected void IncrementButton_Click(object sender, EventArgs e)
    {
        Calendar1.SelectedDate = Calendar1.SelectedDate.AddDays(1.0);
    }
    protected void DecrementButton_Click(object sender, EventArgs e)
    {
        Calendar1.SelectedDate = Calendar1.SelectedDate.AddDays(-1.0);
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ScriptManager RegisterClientScriptInclude</title>
    <style type="text/css">
	div.MessageStyle
	{
      background-color: Green;
      top: 95%;
      left: 1%;
      position: absolute;
      visibility: hidden;
    }
	</style>
</head>
<body>
    <form id="Form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1"
                               runat="server"/>

            <script type="text/javascript">
            Sys.WebForms.PageRequestManager.instance.add_endRequest(Notify);
            </script>

            <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional"
                runat="server">
                <ContentTemplate>
                    <asp:Calendar ID="Calendar1" runat="server"/>
                    <br />
                    Change the selected date: 
                    <asp:Button runat="server" ID="DecrementButton" Text="-" OnClick="DecrementButton_Click" />
                    <asp:Button runat="server" ID="IncrementButton" Text="+" OnClick="IncrementButton_Click" />
                </ContentTemplate>
            </asp:UpdatePanel>

            <div id="NotifyDiv" class="MessageStyle">
                Updates are complete.
            </div>
        </div>
    </form>
</body>
</html>
