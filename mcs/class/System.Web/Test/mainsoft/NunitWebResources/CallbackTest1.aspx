<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Implements Interface="System.Web.UI.ICallbackEventHandler" %> 
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

  <script runat="server" >
      protected System.Collections.Specialized.ListDictionary catalog;
      protected String returnValue;
      public bool raiseCallbackEvent;
      public bool getCallbackResult;
      protected void Page_Load (object sender, EventArgs e)
      {
          String cbReference =
	      Page.ClientScript.GetCallbackEventReference (this,
	      "arg", "ReceiveServerData", "context", null, false);
          String callbackScript;
          callbackScript = "function CallServer(arg, context)" +
	      "{ " + cbReference + ";}";
          Page.ClientScript.RegisterClientScriptBlock (this.GetType (),
	      "CallServer", callbackScript, true);
          catalog = new System.Collections.Specialized.ListDictionary ();
          catalog.Add ("monitor", 12);
          catalog.Add ("laptop", 10);
          catalog.Add ("keyboard", 23);
          catalog.Add ("mouse", 17);

          ListBox1.DataSource = catalog;
          ListBox1.DataTextField = "key";
          ListBox1.DataBind ();
      }

      protected override void OnPreInit (EventArgs e)
      {
          MonoTests.SystemWeb.Framework.WebTest t = MonoTests.SystemWeb.Framework.WebTest.CurrentTest;
          if (t != null)
              t.Invoke (this);
          
          base.OnPreInit (e);
      }

      

      void System.Web.UI.ICallbackEventHandler.RaiseCallbackEvent (String eventArgument)
      {
          raiseCallbackEvent = true;
          if (catalog[eventArgument] == null) {
              returnValue = "-1";
          }
          else {
              returnValue = catalog[eventArgument].ToString ();
          }
      }

      String System.Web.UI.ICallbackEventHandler.GetCallbackResult ()
      {
          getCallbackResult = true;
          
          if (getCallbackResult == true)
              returnValue += "|true";
          else
              returnValue += "|false";
          
          if (raiseCallbackEvent == true)
              returnValue += "|true";
          else
              returnValue += "|false";  
          
          return returnValue;
      }
      
  </script>
   
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" >
<head id="Head1" runat="server">
  <title>Client Callback Example</title>
  
  <script type="text/ecmascript">
    function LookUpStock()
    {
        var lb = document.getElementById("ListBox1");
        var product = lb.options[lb.selectedIndex].text;
        CallServer(product, "");
    }
    
    function ReceiveServerData(rValue)
    {   
        document.getElementById("ResultsSpan").innerHTML = rValue;
    }
  </script>
</head>
<body>
  <form id="form1" runat="server">
    <div>
      <asp:ListBox ID="ListBox1" Runat="server"></asp:ListBox>
      <br />
      <br />
      <button type="Button" onclick="LookUpStock()">Look Up Stock</button>
      <br />
      <br />
      Items in stock: <span id="ResultsSpan" runat="server"></span>
      <br />
    </div>
  </form>
</body>
</html>