<%@ Page Language="C#" Title="ASP.NET AJAX Script Walkthrough" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
 
<html xmlns="http://www.w3.org/1999/xhtml">
   
  <head id="Head1" runat="server">
    <title="ASP.NET AJAX Script Walkthrough" />
    <style type="text/css">
      body { font: 11pt Trebuchet MS;
        font-color: #000000;
        padding-top: 72px;
        text-align: center }
   
      .text { font: 8pt Trebuchet MS }
    </style>
   
   </head>
   <body>
   <form id="Form1" runat="server">
   <asp:ScriptManager runat="server" ID="scriptManager">
      <Services>
        <asp:ServiceReference path="HelloWorldService.asmx" />
      </Services>
   </asp:ScriptManager>

     <div>
       Search for
       <input id="SearchKey" type="text" />
       <input id="SearchButton" type="button" value="Search"
         onclick="DoSearch()" />
    </div>
  </form>
  <hr style="width: 300px" />
  <div>
  <span id="Results"></span>
  </div> 
   <script type="text/javascript">

     function DoSearch()
     {
       var SrchElem = document.getElementById("SearchKey");
       Samples.AspNet.HelloWorldService.HelloWorld(SrchElem.value, OnRequestComplete);
     }
     
     function OnRequestComplete(result)
     {
       var RsltElem = document.getElementById("Results");
       RsltElem.innerHTML = result;
     }
     
   </script>
  </body>
</html>

