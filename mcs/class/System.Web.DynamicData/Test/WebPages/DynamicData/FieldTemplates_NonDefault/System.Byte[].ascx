<%@ Control Language="C#" CodeFile="System.Byte[].ascx.cs" Inherits="SystemByteArray_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="systemByteArrayTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>