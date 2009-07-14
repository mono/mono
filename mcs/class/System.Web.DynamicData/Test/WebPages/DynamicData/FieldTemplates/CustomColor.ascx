<%@ Control Language="C#" CodeFile="CustomColor.ascx.cs" Inherits="CustomColorField" %>

<span class="field"><%= Column.Name %></span>: <span class="customColorTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>