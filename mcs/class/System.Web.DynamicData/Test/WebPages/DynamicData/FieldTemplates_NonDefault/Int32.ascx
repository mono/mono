<%@ Control Language="C#" CodeFile="Int32.ascx.cs" Inherits="Int32_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="int32Template"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>