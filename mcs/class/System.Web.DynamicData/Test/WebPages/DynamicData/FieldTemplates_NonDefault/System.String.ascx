<%@ Control Language="C#" CodeFile="System.String.ascx.cs" Inherits="SystemString_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="systemStringTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>