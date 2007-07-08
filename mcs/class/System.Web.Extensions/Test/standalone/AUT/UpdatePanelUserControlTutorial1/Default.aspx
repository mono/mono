<%@ Page Language="C#" AutoEventWireup="true" Inherits="UpdatePanelUserControl.Default" Codebehind="Default.aspx.cs" %>

<%@ Register Src="EmployeeInfo.ascx" TagName="EmployeeInfo" TagPrefix="uc1" %>
<%@ Register Src="EmployeeList.ascx" TagName="EmployeeList" TagPrefix="uc2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>AdventureWorks Employees</title>
    <style type="text/css">
    body
    {
      font-family:verdana;
      font-size:smaller;
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"
                           EnablePartialRendering="true" /> 
        <table>
            <tr>
                <td valign="top">
                    <uc2:EmployeeList ID="EmployeeList1" runat="server" UpdateMode="Conditional"
                                      OnSelectedIndexChanged="EmployeeList1_OnSelectedIndexChanged" />
                </td>
                <td valign="top">
                    <uc1:EmployeeInfo ID="EmployeeInfo1" runat="server" UpdateMode="Conditional" />
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
