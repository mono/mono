<%@ Page Language="C#" 
         AutoEventWireup="true" 
         Inherits="Tests.TagsNestedInClientTag" %>
<html>
<head runat="server"><title>Bug #323719</title></head>
<body>

<p>
This is a test to see if mono can handle a control embedded in a script tag; which MS is able to deal with.
</p>

<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><script <asp:Literal ID="languageLiteral" runat="server" EnableViewState="false" /> <asp:Literal ID="srcLiteral" runat="server" EnableViewState="false" /> <asp:Literal ID="typeLiteral" runat="server" EnableViewState="false" />></script>
<sometag <asp:Literal ID="languageLiteral1" runat="server" EnableViewState="false" /> <asp:Literal ID="srcLiteral1" runat="server" EnableViewState="false" /> <asp:Literal ID="typeLiteral1" runat="server" EnableViewState="false"/>></sometag></sometag><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
</body>
</html>
