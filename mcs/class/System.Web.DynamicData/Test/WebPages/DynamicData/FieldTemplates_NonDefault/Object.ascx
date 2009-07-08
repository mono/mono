<%@ Control Language="C#" CodeFile="Object.ascx.cs" Inherits="Object_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="objectTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>