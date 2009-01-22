<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    test1
        &nbsp;<asp:FormView ID="FormView1" runat="server" AllowPaging="True" BackColor="#DEBA84"
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
        </asp:FormView>
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
        test2
        <asp:FormView ID="FormView2" runat="server" AllowPaging="True" CellPadding="4" DataSourceID="ObjectDataSource1"
            ForeColor="#333333">
            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <EditRowStyle BackColor="#999999" />
            <PagerTemplate>
                <asp:Button ID="Button1" runat="server" CommandArgument='<%# "Prev" %>' Text='<%# "Prev Item" %>' CommandName='<%# "Page" %>' />
                <asp:Button ID="Button2" runat="server" CommandArgument='<%# "Next" %>' Text='<%# "Next Item" %>' CommandName='<%# "Page" %>' />
                <asp:Button ID="Button3" runat="server" CommandArgument='<%# "First" %>' Text='<%# "First Item" %>' CommandName='<%# "Page" %>' />
                <asp:Button ID="Button4" runat="server" CommandArgument='<%# "Last" %>' Text='<%# "Last Item" %>' CommandName='<%# "Page" %>' />
            </PagerTemplate>
            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <FooterTemplate>
                <asp:Label ID="Label3" runat="server" Text='<%# "Footer Template Test" %>'></asp:Label>
            </FooterTemplate>
            <ItemTemplate>
                <asp:Label ID="Label2" runat="server" Text="<%# FormView2.DataItem.ToString() %>"></asp:Label>
            </ItemTemplate>
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        </asp:FormView>
    
    </div>
    test3
        <asp:FormView ID="FormView3" runat="server" AllowPaging="True" BackColor="LightGoldenrodYellow"
            BorderColor="Tan" BorderWidth="1px" CellPadding="2" DataSourceID="ObjectDataSource1"
            ForeColor="Black">
            <FooterStyle BackColor="Tan" ForeColor="#FFC0FF" HorizontalAlign="Right" />
            <EditRowStyle BackColor="DarkSlateBlue" ForeColor="GhostWhite" />
            <PagerStyle BackColor="PaleGoldenrod" ForeColor="DarkSlateBlue" HorizontalAlign="Center" />
            <FooterTemplate>
                <asp:Label ID="Label6" runat="server" Text='<%# "FormView Footer" %>'></asp:Label>
            </FooterTemplate>
            <ItemTemplate>
                <asp:Label ID="Label4" runat="server" Text="<%# FormView3.DataItem.ToString() %>"></asp:Label>
            </ItemTemplate>
            <HeaderStyle BackColor="Tan" Font-Bold="True" ForeColor="#C00000" HorizontalAlign="Center" VerticalAlign="Top" />
            <HeaderTemplate>
                <asp:Label ID="Label5" runat="server" Text='<%# "Header Template Test" %>'></asp:Label>
            </HeaderTemplate>
        </asp:FormView>
        test4
        <asp:FormView ID="FormView4" runat="server" AllowPaging="True" BackColor="White"
            BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" CellPadding="3" CellSpacing="10"
            DataSourceID="ObjectDataSource1" DefaultMode="Edit" FooterText="Using Footer Text property"
            GridLines="Both" HeaderText="Using Header Text property" HorizontalAlign="Right" CaptionAlign="Right">
            <FooterStyle BackColor="Maroon" ForeColor="#000066" HorizontalAlign="Center" />
            <EditRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
            <PagerTemplate>
                <asp:LinkButton ID="LinkButton1" runat="server" CommandArgument='<%# "Next" %>' Text='<%# "Next" %>' CommandName='<%# "Page" %>'></asp:LinkButton>
                <asp:LinkButton ID="LinkButton2" runat="server" CommandArgument='<%# "Prev" %>' Text='<%# "Prev" %>' CommandName='<%# "Page" %>'></asp:LinkButton>
                <asp:Label ID="Label7" runat="server" Text='<%# "Page Index: "+ FormView4.PageIndex %>'></asp:Label>
            </PagerTemplate>
            <RowStyle BackColor="#FF8080" ForeColor="#000066" HorizontalAlign="Center" />
            <PagerStyle BackColor="LightGray" ForeColor="#000066" HorizontalAlign="Left" />            
            <ItemTemplate>
                &nbsp;<asp:TextBox ID="TextBox1" runat="server" Text="<%# FormView4.DataItem.ToString() %>"></asp:TextBox>
            </ItemTemplate>
            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" HorizontalAlign="Left" />
        </asp:FormView>
        endtest
    </form>
</body>
</html>
endtest