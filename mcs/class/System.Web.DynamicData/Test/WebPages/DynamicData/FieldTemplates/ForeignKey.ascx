<%@ Control Language="C#" CodeFile="ForeignKey.ascx.cs" Inherits="ForeignKeyField" %>

<asp:HyperLink ID="HyperLink1" runat="server"
    Text="<%# GetDisplayString() %>"
    NavigateUrl="<%# GetNavigateUrl() %>"  />