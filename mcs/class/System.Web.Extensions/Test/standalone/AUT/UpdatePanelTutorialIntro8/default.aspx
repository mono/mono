<%@ Page Language="C#" MasterPageFile="MasterPage.master" Title="UpdatePanel in Master Pages" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    Content Page<br />
    <asp:UpdatePanel id="UpdatePanel1" runat="server">
        <contenttemplate>
        <fieldset>
        <legend>UpdatePanel</legend>
           <asp:Calendar id="Calendar1" runat="server"></asp:Calendar>
        </fieldset>
        </contenttemplate>
    </asp:UpdatePanel>
</asp:Content>
