<%@ Page Language="C#" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
    <head>
        <title>FormView class test</title>
    </head>
<body>
    <form id="form1" runat="server">
    <asp:FormView ID="formView1" runat="server" DataSourceID="DataSource1">
        <ItemTemplate>
        </ItemTemplate>
    </asp:FormView>
    <asp:FormView ID="formView2" runat="server" CssClass="test1" DataSourceID="DataSource1">
        <ItemTemplate>
        </ItemTemplate>
    </asp:FormView>
    <asp:ObjectDataSource ID="DataSource1" runat="server" TypeName="System.Guid" SelectMethod="ToByteArray" />
    
    </form>
</body>
</html>
﻿