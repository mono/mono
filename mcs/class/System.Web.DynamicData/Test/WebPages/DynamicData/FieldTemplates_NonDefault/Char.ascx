<%@ Control Language="C#" CodeFile="Char.ascx.cs" Inherits="Char_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="charTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>