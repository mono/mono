<!-- <Snippet3> -->
<%@ Control Language="C#" ClassName="WebUserControl" %>

<script runat="server">
    public event EventHandler InnerClick
    {
        add
        {
            UCButton1.Click += value;
        }
        remove
        {
            UCButton1.Click -= value;
        }
    }
    public String Name
    {
        get
        {
            return UCTextBox1.Text;
        }
    }
</script>

<asp:Panel ID="UCPanel1" runat="server" GroupingText="User Control">
    Enter your name:
    <asp:TextBox ID="UCTextBox1" runat="server"></asp:TextBox>
    <br />
    <asp:Button ID="UCButton1" runat="server" Text="Submit" />
</asp:Panel>
<!-- </Snippet3> -->
