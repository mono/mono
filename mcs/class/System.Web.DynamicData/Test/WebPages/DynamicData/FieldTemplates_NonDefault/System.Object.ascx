<%@ Control Language="C#" CodeFile="System.Object.ascx.cs" Inherits="SystemObject_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="systemObjectTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>