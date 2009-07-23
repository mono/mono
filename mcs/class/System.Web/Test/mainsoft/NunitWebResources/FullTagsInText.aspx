<%@ Page Language="C#" %>
<script runat="server">
   void Page_Load (object sender, EventArgs e)
   {
       if (lit1.Text != "literal")
          throw new ApplicationException ("Invalid value of lit1");
       if (lit2.Text != String.Empty)
          throw new ApplicationException ("Invalid value of lit2");
   }
</script>
<html><head><title>Tags in text with content</title></head>
<script language="javascript" type="text/javascript">
   var one = 1;
   var two = 2;

   var lit = <asp:Literal id="lit1" runat="server">literal</asp:Literal>
</script>
<body>
<form runat="server">
<script language="javascript" type="text/javascript">
   var three = 3;
   var four = 4;
   var lit = <asp:Literal id="lit2" runat="server"></asp:Literal>

   alert ("something");
</script>
</form>
</body>
</html>