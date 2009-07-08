<%@ Control Language="C#" CodeFile="Int64.ascx.cs" Inherits="Int64_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="int64Template"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>