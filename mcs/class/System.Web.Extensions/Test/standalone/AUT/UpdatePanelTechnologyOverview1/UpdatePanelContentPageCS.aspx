<%@ Page Language="C#" MasterPageFile="MasterCS.master"
    Title="ScriptManager in Content Page" %>

<%@ MasterType VirtualPath="MasterCS.master" %>

<script runat="server">

    protected void Button3_Click(object sender, EventArgs e)
    {
        Master.LastUpdate = DateTime.Now;
    }

</script>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1"
    runat="Server">
    <asp:Panel ID="Panel2"
               GroupingText="ContentPage"
               runat="server" >
        <asp:UpdatePanel ID="UpdatePanel1" 
                         UpdateMode="Conditional" 
                         runat="server">
            <ContentTemplate>
                <p>
                    Last updated: <strong>
                        <%= Master.LastUpdate.ToString() %>
                    </strong>
                </p>
                <asp:Button ID="Button3"
                            Text="Refresh Panel"
                            OnClick="Button3_Click"
                            runat="server"  />
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>

