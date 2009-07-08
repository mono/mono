<%@ Control Language="C#" CodeFile="Byte[].ascx.cs" Inherits="ByteArray_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="byteArrayTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>