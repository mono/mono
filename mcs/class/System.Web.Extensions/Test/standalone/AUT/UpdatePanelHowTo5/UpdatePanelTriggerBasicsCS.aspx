
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void Search_Click(object sender, EventArgs e)
    {
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Declarative Trigger Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
            <asp:TextBox ID="SearchField" runat="server"></asp:TextBox>
            <asp:Button ID="SearchButton" Text="Submit" OnClick="Search_Click"
                runat="server" />
            <hr />
            <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" 
                             runat="server">
                <ContentTemplate>
                    <asp:Label ID="Label1" runat="server"/>
                    <br />
                </ContentTemplate>
                <Triggers>
                   <asp:AsyncPostBackTrigger ControlID="SearchButton" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
