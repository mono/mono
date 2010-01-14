<%@ Page Language="C#" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="System.Web.Configuration" %>

<script runat="server">
  protected void Page_Load (object sender, EventArgs args)
  {
     NameValueCollection nvc = WebConfigurationManager.AppSettings;

     foreach (string k in nvc)
        settings.InnerHtml += String.Format ("{0,23}: '{1}'\n", k, nvc [k]);
  }
</script>
<html>
  <head>
    <title>locations tests</title>
  </head>
  <body>
    AppSettings:
    <%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><pre runat="server" id="settings"></pre><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
  </body>
</html>

