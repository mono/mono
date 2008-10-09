<%@ Page Language="C#" %>
<%@ Register Assembly="System.Web_test" Namespace="MyTemplateControls" TagPrefix="mc" %>
<script runat="server">
  void TemplateControl2_ItemCreated (TestTemplateControl sender, TestTemplateItemEventArgs args)
  {
     PlaceHolder ph = args.Item.FindControl ("PlaceHolder1") as PlaceHolder;
     if (ph == null)
        throw new InvalidOperationException ("Missing PlaceHolder1 - template children processing is broken in TemplateCompiler.");
  }
</script>
<html><head><title>Templates test</title></head>
  <body>
    <form runat="server">
      <mc:TestTemplateControl runat="server" id="templateControl2" OnItemCreated="TemplateControl2_ItemCreated">
	<Container>
	  <ItemTemplate>
	    <asp:PlaceHolder runat="server" id="PlaceHolder1"/>
	  </ItemTemplate>
	</Container>
      </mc:TestTemplateControl>
    </form>
  </body>
</html>
