<%@ Control Language="C#" CodeFile="System.Collections.Generic.List`1[System.String].ascx.cs" Inherits="SystemStringList_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="systemStringListTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>