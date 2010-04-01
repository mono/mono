<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
  <head>
    <title>Bug #578863</title>
  </head>
  <body>
    <form id="form1" runat="server">
      <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:FormView ID="FormView1" runat="server" AllowPaging="True" BackColor="#DEBA84"
		    BorderColor="#DEBA84" BorderStyle="None" BorderWidth="1px" CellPadding="3" CellSpacing="2"
		    DataSourceID="ObjectDataSource1" GridLines="Both">
	<FooterStyle BackColor="#F7DFB5" ForeColor="#8C4510" />
	<EditRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="White" />
	<RowStyle BackColor="#FFF7E7" ForeColor="#8C4510" />
	<PagerStyle ForeColor="#8C4510" HorizontalAlign="Center" />
	<ItemTemplate>
          <asp:Label ID="Label1" runat="server" Text="<%# FormView1.DataItem.ToString() %>"></asp:Label>
	</ItemTemplate>
	<HeaderStyle BackColor="#A55129" Font-Bold="True" ForeColor="White" />
	<PagerSettings Visible="false"/>
      </asp:FormView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
      <asp:ObjectDataSource ID="ObjectDataSource1" runat="server"
			    DeleteMethod="DeleteList" InsertMethod="InsertList" SelectMethod="GetMyList"
			    TypeName="MonoTests.System.Web.UI.WebControls.TestMyData" UpdateMethod="UpdateList">
	<DeleteParameters>
          <asp:Parameter Name="value" Type="Int32" />
	</DeleteParameters>
	<InsertParameters>
          <asp:Parameter Name="value" Type="Int32" />
	</InsertParameters>
	<UpdateParameters>
          <asp:Parameter Name="index" Type="Int32" />
          <asp:Parameter Name="value" Type="Int32" />
	</UpdateParameters>
      </asp:ObjectDataSource>
    </form>
  </body>
</html>
