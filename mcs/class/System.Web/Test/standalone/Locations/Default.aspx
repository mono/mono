<%@ Page Language="C#" %>
<html>
  <head>
    <title>/Default.aspx</title>
  </head>
  <body>
<%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %>/Hello<%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
  </body>
</html>

