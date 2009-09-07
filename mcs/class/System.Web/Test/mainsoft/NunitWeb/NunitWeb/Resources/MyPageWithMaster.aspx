<%@ Page Language="C#" MasterPageFile="My.master" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<asp:Content ID="BlankContent" ContentPlaceHolderID="Header" runat="server"/>

<asp:content ID="LiteralContent" ContentPlaceHolderID="Main" runat="server">
     Page main text
</asp:content>

<asp:content ID="FormContent" ContentPlaceHolderID="Dynamic" runat="server">
    <form id="form1" runat="server" >
    </form>
</asp:content>
