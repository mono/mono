<%@ Control Language="C#" CodeFile="System.String[].ascx.cs" Inherits="SystemStringArray_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="systemStringArrayTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>