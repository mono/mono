<%@ Control Language="C#" CodeFile="System.Byte.ascx.cs" Inherits="SystemByte_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="systemByteTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>