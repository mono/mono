<%@ Control Language="C#" CodeFile="Text.ascx.cs" Inherits="TextField" %>

<span class="field"><%= Column.Name %></span>: <span class="defaultTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>