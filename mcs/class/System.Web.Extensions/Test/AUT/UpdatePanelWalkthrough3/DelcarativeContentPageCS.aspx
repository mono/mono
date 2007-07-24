<%@ Page Language="C#" MasterPageFile="DeclarativeMasterPageCS.master" Title="Content Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
            <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server" />
            <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" 
                             runat="server">
                <ContentTemplate>
                UpdatePanel on content page refreshed at
                <%=DateTime.Now.ToString() %>
                <asp:Button ID="Button1" Text="Refresh" runat="server" />
                </ContentTemplate>
            </asp:UpdatePanel>
</asp:Content>
