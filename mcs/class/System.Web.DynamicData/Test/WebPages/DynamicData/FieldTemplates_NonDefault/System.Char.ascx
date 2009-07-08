<%@ Control Language="C#" CodeFile="System.Char.ascx.cs" Inherits="SystemChar_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="systemCharTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>