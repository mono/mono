<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <fieldset>
                <legend>UpdatePanel</legend>
                <asp:Label ID="Label1" runat="server" Text="Panel created."></asp:Label><br />
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>    
    </div>
    </form>
</body>
</html>
