<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Untitled Page</title>
    <script language="C#" runat="server">
        protected void Page_PreRender (object sender, EventArgs e)
        {
            System.Xml.XmlDocument myXml = new System.Xml.XmlDocument ();
            myXml = (System.Xml.XmlDocument) XmlDataSource1.GetXmlDocument ();
            System.Xml.XmlNode myNode = myXml.SelectSingleNode ("bookstore/book [title='Pride and Prejudice']/title");
            myNode.InnerText = "ThisIsATest";
            XmlDataSource1.Save ();
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        begint<div>
            <asp:XmlDataSource
            runat="server"
            id="XmlDataSource1" EnableCaching="false"
            DataFile="~/XMLDataSourceTest.xml" />
            
            <asp:Repeater ID="Repeater1"
            runat="server"
            DataSourceID="XmlDataSource1">
            <ItemTemplate>
                <h2>BookStore</h2>
                <hr>
                <table>
                  <tr>
                    <td>Book</td>
                    <td><font color="blue"><%# XPath ("title")%></font></td>
                    <td><%# XPath ("first_name")%></td>
                    <td><%# XPath ("last_name")%></td>
                    <td><%# XPath ("price") %></td>
                  </tr>
                </table>
                <hr>
            </ItemTemplate>
        </asp:Repeater>
      </div>endt
    </form>
</body>
</html>
