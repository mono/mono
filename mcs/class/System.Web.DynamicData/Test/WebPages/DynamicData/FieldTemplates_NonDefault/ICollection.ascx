<%@ Control Language="C#" CodeFile="ICollection.ascx.cs" Inherits="ICollection_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="iCollectionTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>