<%@ Page Language="C#" %>
<%@ Import Namespace="System.Web.Services" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

 
<script runat="server">

    [WebMethod]
    // Get session state value.
    public static string GetSessionValue(string key)
    {
        return (string)HttpContext.Current.Session[key];
    }

    [WebMethod]
    // Set session state value.
    public static string SetSessionValue(string key, string value)
    {
        HttpContext.Current.Session[key] = value;
        return (string)HttpContext.Current.Session[key];
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">

<head id="Head1" runat="server">

    <title>Using Page Methods with Session State</title>
    <style type="text/css">
            body {  font: 11pt Trebuchet MS;
                    font-color: #000000;
                    padding-top: 72px;
                    text-align: center }
            .text { font: 8pt Trebuchet MS }
    </style>
</head>

<body>

    <h2>Using Page Methods with Session State</h2>
    
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" 
            runat="server" EnablePageMethods="true">
            <Scripts>
                <asp:ScriptReference Path="PageMethod.js"/>
            </Scripts>
        </asp:ScriptManager>
    </form>

     <center>
         <table>
            <tr align="left">
                <td>Write current date and time in session state:</td>
                <td>
                    <input type="Button" 
                        onclick="SetSessionValue('SessionValue', 'test value')" 
                        value="Write" />
                </td>
            </tr>
            <tr align="left">
                <td>Read current date and time from session state:</td>
                <td>         
                    <input type="Button" 
                        onclick="GetSessionValue('SessionValue')" 
                        value="Read" />
                </td>
            </tr>
        </table>           
    </center>
     
    <hr/>
  
    <span style="background-color:Yellow" id="ValueId"></span>
    <br />
    <span style="background-color:Aqua" id="ResultId"></span>
       
</body>

</html>
