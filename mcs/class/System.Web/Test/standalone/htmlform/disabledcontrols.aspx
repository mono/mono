<%@ page language="C#"%>

<script runat="server">

  void Page_Load(Object sender, EventArgs e)
  {
    
    if (IsPostBack)
    {
	// the second time through, print out the submitted form info
	foreach (string key in Page.Request.Form.AllKeys) {
		Response.Write (String.Format ("{0}: {1}<br/>", key, Page.Request.Form[key]));
	}
    }
    else
    {
    // The first time the page loads, set the values
    // of the HtmlInputText and HtmlInputCheckBox controls.

      InputText1.Value = "Test";
      InputCheckBox1.Checked = true;
    }
  }
  
</script>

<html>

<head id="Head1" 
      runat="server">

    <title>HtmlForm SubmitDisabledControls Property Example</title>

</head>

<body>

  <form id="form1" 
	submitdisabledcontrols="true"
        runat="server">
    
      <h3>HtmlForm SubmitDisabledControls Property Example</h3>
    
      <input id="InputText1" 
             name="InputText1" 
             type="text"
             runat="server" />

      <input id="InputCheckBox1" 
             name="InputCheckBox1" 
             type="Checkbox" 
             runat="server" />
    
      <asp:Button id="PostBackButton"
                  text="Post back"
                  runat="server" />

  </form>    
    
</body>

</html>

<script Language="javascript">

    // Disable the HTML controls on the form.
    document.getElementById('InputText1').disabled = true;
    document.getElementById('InputCheckBox1').disabled = true;
</script>
