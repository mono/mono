<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    Hello World of mono lovers
                <!--    <asp:Literal ID="form1" runat="server" />

                        <br>                            -->
    </div>
    <asp:Repeater runat="server" id="r1">
    <ItemTemplate>
    <asp:Literal ID="form2" runat="server" />
    </ItemTemplate>
    </asp:Repeater>
    </form>
</body>
</html>
