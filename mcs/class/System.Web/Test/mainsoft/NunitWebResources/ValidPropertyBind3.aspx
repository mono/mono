<%@ Page Language="C#" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Register TagPrefix="uc1" TagName="ReadWritePropertyControl" Src="ReadWritePropertyControl.ascx" %>

<html xmlns="http://www.w3.org/1999/xhtml" >
    <head>
        <title>UrlProperty test</title>
    </head>
<body>
    <form id="form1" runat="server">
    <asp:FormView runat="server" ID="fv1" DataSourceID="DataSource1" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" ForeColor="Black" GridLines="Vertical" >
    <ItemTemplate>
    <uc1:ReadWritePropertyControl runat="server" ID="wuc1" ReadWriteProperty='<%# Bind(            "Data"           ) %>' />
    </ItemTemplate>
    </asp:FormView>
    <asp:ObjectDataSource ID="DataSource1" runat="server" TypeName="MonoTests.System.Web.Compilation.BindTestDataSource" SelectMethod="GetData" />
    </form>
</body>
</html>
