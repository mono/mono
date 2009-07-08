<%@ Control Language="C#" CodeFile="FooEmpty.ascx.cs" Inherits="FooEmpty_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="fooEmptyTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>