<%@ Page Language="C#" %>
<%@ Implements Interface=System.Web.UI.ICallbackEventHandler %>
<script runat=server>
string ICallbackEventHandler.RaiseCallbackEvent(string eventArgument) {
  return "Hello, " +
    eventArgument +
    ". Your name is " +
    eventArgument.Length.ToString() +
    " characters long.";
}
</script>
<html>
  <head runat=server/>
  <script>
  function OnCallback(result, context) {
    alert(result);
  }
  </script>
  <body>
    <form runat=server>
      Enter your name here:
      <input name="name" />
      <input type=button ID=CallbackBtn value="Send"
	monotype="<%= this.ID %>"
        onclick="<%= ClientScript.GetCallbackEventReference(
          this,
          "document.forms[0].name.value", 
          "OnCallback", 
          "this", 
          "OnCallback", 
          true) %>" />
    </form>
  </body>
</html>