<%@ Control Language="C#" CodeFile="System.Boolean.ascx.cs" Inherits="SystemBoolean_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="systemBooleanTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>