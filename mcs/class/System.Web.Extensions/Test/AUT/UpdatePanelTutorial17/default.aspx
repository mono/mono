<%@ Page Language="C#" %>
<%@ Import Namespace="System.Collections.Generic" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void NewsClick_Handler(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(2000);
        HeadlineList.DataSource = GetHeadlines();
        HeadlineList.DataBind();
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            HeadlineList.DataSource = GetHeadlines();
            HeadlineList.DataBind();
        }
    }
    // Helper method to simulate news headline fetch.
    private SortedList GetHeadlines()
    {
        SortedList headlines = new SortedList();
        headlines.Add(1, "This is headline 1.");
        headlines.Add(2, "This is headline 2.");
        headlines.Add(3, "This is headline 3.");
        headlines.Add(4, "This is headline 4.");
        headlines.Add(5, "This is headline 5.");
        headlines.Add(6, "(Last updated on " + DateTime.Now.ToString() + ")");
        return headlines;
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Canceling Postback Example</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
    #UpdatePanel1{
       width: 400px;
       height: 200px;
       border: solid 1px gray;
    }
    div.AlertStyle {
      font-size: smaller;
      background-color: #FFC080;
      width: 400px;
      height: 20px;
      visibility: hidden;
    }
	</style>
</head>
<body>
    <form id="form1" runat="server">
        <div >
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        <Scripts>
        <asp:ScriptReference Path="CancelPostback.js" />
        </Scripts>
        </asp:ScriptManager>
        <asp:UpdatePanel  ID="UpdatePanel1" runat="Server" >
            <ContentTemplate>
                <asp:DataList ID="HeadlineList" runat="server">
                    <HeaderTemplate>
                    <strong>Headlines</strong>
                    </HeaderTemplate>
                    <ItemTemplate>
                         <%# Eval("Value") %>
                    </ItemTemplate>
                    <FooterTemplate>
                    </FooterTemplate>
                    <FooterStyle HorizontalAlign="right" />
                </asp:DataList>
                <p style="text-align:right">
                <asp:Button ID="RefreshButton" 
                            Text="Refresh" 
                            runat="server" 
                            OnClick="NewsClick_Handler" />
                </p>
                <div id="AlertDiv" class="AlertStyle">
                <span id="AlertMessage"></span> 
                &nbsp;&nbsp;&nbsp;&nbsp;
                <asp:LinkButton ID="CancelRefresh" runat="server">
                Cancel</asp:LinkButton>                      
            </ContentTemplate>
        </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
