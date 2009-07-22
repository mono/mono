<%@ Page Language="C#" %>
<script runat="server">
  protected string expressionTest;

  void Page_Load (object sender, EventArgs e)
  {
     if (SyncPath == null)
        throw new ApplicationException ("Missing SyncPath");
     if (newName == null)
        throw new ApplicationException ("Missing newName");
     if (newPublishStatus == null)
        throw new ApplicationException ("Missing newPublishStatus");
  }

  string SetAndGetExpressionTest (string text)
  {
     expressionTest = text;
     return text;
  }
</script>
<html><head><title>Tags, expressions and comments in text</title></head>
<body>
<form runat="server">
<script type="text/javascript">
            <!-- some 
                    comment
                            here
                        -->
            parent.top.syncTree('<asp:Literal id="SyncPath" runat="server"></asp:Literal>', '<%= SetAndGetExpressionTest ("test") %>', '<asp:Literal id="newName" runat="server"></asp:Literal>', '<asp:Literal id="newPublishStatus" runat="server"></asp:Literal>');
      </script>

<%
if (expressionTest != "test")
        throw new ApplicationException ("expressionTest has invalid value");
%>
</form>
</body>
</html>
