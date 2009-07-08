<%@ Control Language="C#" CodeFile="Int16.ascx.cs" Inherits="Int16_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="int16Template"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>