<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PageHyperLink.aspx.cs" Inherits="GHTTests.System_Web_dll.PageDirectories.Pages.PageHyperLink" MasterPageFile="~/System_Web/PageDirectories/UserMaster.Master"%>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>

<asp:Content ID="testcontent" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
    <cc1:GHTSubTest id="GHTSubTest1" runat="server">
	    <asp:HyperLink ID="link2" runat="server" NavigateUrl="page.aspx" Text="link2" />
	</cc1:GHTSubTest>
</asp:Content>