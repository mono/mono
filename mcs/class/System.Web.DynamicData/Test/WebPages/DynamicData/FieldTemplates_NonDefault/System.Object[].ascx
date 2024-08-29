<%@ Control Language="C#" CodeFile="System.Object[].ascx.cs" Inherits="SystemObjectArray_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="systemObjectArrayTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>