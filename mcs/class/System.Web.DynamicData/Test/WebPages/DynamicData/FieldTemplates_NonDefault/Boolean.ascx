<%@ Control Language="C#" CodeFile="Boolean.ascx.cs" Inherits="Boolean_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="booleanTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>