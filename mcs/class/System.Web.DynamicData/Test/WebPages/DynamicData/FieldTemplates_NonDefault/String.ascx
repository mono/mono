<%@ Control Language="C#" CodeFile="String.ascx.cs" Inherits="String_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="stringTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>