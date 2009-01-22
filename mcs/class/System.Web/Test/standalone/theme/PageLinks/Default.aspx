<%@	Page Language="C#" CodeFile="Default.aspx.cs" Inherits="MyDefault"
 MasterPageFile="~/Default.master" Title="Your Name Here | Home" Theme ="Black" %>

<%@ Register Src="MyParent.ascx" TagName="MyParent" TagPrefix="uc1" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
    <uc1:MyParent id="Parent1" runat="server" skinid="login"/>
</asp:Content>
