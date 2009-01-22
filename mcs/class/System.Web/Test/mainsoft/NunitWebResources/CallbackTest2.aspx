<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Implements Interface="System.Web.UI.ICallbackEventHandler" %> 
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

  <script runat="server" >
      
      
      protected String returnValue;
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
          returnValue += "|Load";
      }

      protected override void OnPreInit (EventArgs e)
      {
          MonoTests.SystemWeb.Framework.WebTest t = MonoTests.SystemWeb.Framework.WebTest.CurrentTest;
          if (t != null)
              t.Invoke (this);
          returnValue += "|PreInit";
          base.OnPreInit (e);
      }

      protected override void OnInit (EventArgs e)
      {
          returnValue += "|Init";
          base.OnInit (e);
      }

      protected override void OnInitComplete (EventArgs e)
      {
          returnValue += "|InitComplete";
          base.OnInitComplete (e);
      }

      protected override void OnLoadComplete (EventArgs e)
      {
          returnValue += "|LoadComplete";
          base.OnLoadComplete (e);
      }

      protected override void OnPreLoad (EventArgs e)
      {
          returnValue += "|PreLoad";
          base.OnPreLoad (e);
      }

      protected override void OnPreRender (EventArgs e)
      {
          returnValue += "|PreRender";
          base.OnPreRender (e);
      }

      protected override void OnUnload (EventArgs e)
      {
          returnValue += "|Unload";
          base.OnUnload (e);
      }

      protected override void OnSaveStateComplete (EventArgs e)
      {
          returnValue += "|SaveStateComplete";
          base.OnSaveStateComplete (e);
      }

      void System.Web.UI.ICallbackEventHandler.RaiseCallbackEvent (String eventArgument)
      {
          returnValue += "|RaiseCallbackEvent";
      }

      String System.Web.UI.ICallbackEventHandler.GetCallbackResult ()
      {
          returnValue += "|GetCallbackResult";
          return returnValue;
      }
      
  </script>
   
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" >
<head id="Head1" runat="server">
  <title>Client Callback Example</title>
  
  <script type="text/ecmascript">
    function LookUpStock()
    {
        CallServer("" , "");
    }
    
    function ReceiveServerData(rValue)
    {   
        
    }
  </script>
</head>
<body>
  <form id="form1" runat="server">
    <div>
      
    </div>
  </form>
</body>
</html>