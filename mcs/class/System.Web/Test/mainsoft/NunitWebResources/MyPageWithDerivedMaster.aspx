<%@ Page Language="C#" MasterPageFile="MyDerived.master" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<asp:content ID="LiteralContent" ContentPlaceHolderID="Main" runat="server">
     Page main text <br />
</asp:content>

<asp:content ID="FormContent" ContentPlaceHolderID="Dynamic" runat="server">
    <form id="form1" runat="server" >
    </form>
</asp:content>
