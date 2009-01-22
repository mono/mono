<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Untitled Page</title>
    <script language="C#" runat="server">
        protected void Page_PreRender (object sender, EventArgs e)
		{
			XmlDataSource1.Data = @"<?xml version=""1.0"" encoding=""iso-8859-1""?>
					     <orders>
					       <order>
						 <customer id=""12345"" />
						 <customername>
						     <firstn>Todd</firstn>
						     <lastn>Rowe</lastn>
						 </customername>
						 <transaction id=""12345"" />
						 <shipaddress>
						     <address1>1234 Tenth Avenue</address1>
						     <city>Bellevue</city>
						     <state>Washington</state>
						     <zip>98001</zip>
						 </shipaddress>
						 <summary>
						     <item dept=""tools"">screwdriver</item>
						     <item dept=""tools"">hammer</item>
						     <item dept=""plumbing"">fixture</item>
						 </summary>
					       </order>
					    </orders>";      
                 }
    </script>
</head>

<body>

    <form id="form1" runat="server">
        begint<div>
            <asp:XmlDataSource
            runat="server"
            id="XmlDataSource1" EnableCaching="false"
            TransformFile="~/XMLDataSourceTest.xsl">
            </asp:XmlDataSource>

          <asp:Repeater ID="Repeater1"
            runat="server"
            DataSourceID="XmlDataSource1">
            <ItemTemplate>
                <h2>Order</h2>
                <hr>
                <table>
                  <tr>
                    <td>Customer</td>
                    <td><font color="blue"><%# XPath ("customer/id") %></font></td>
                    <td><%# XPath ("customer/firstname")%></td>
                    <td><%# XPath ("customer/lastname")%></td>
                  </tr>
                </table>
                <hr>
            </ItemTemplate>
        </asp:Repeater>
      </div>endt
    </form>
</body>
</html>
