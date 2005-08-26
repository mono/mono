<html>
<body>

<form runat="server">
<table border="0" bgcolor="#b0c4de">
   <tr valign="top">
     <td colspan="4"><h4>Compare two values</h4></td>
   </tr>
   <tr valign="top">
     <td><asp:TextBox id="txt1" runat="server" /></td>
     <td> = </td>
     <td><asp:TextBox id="txt2" runat="server" /></td>
     <td><asp:Button Text="Validate" runat="server" /></td>
   </tr>
</table>
<br />
<asp:CompareValidator
id="compval"
Display="dynamic"
ControlToValidate="txt1"
ControlToCompare="txt2"
ForeColor="red"
BackColor="yellow"
Type="String"
EnableClientScript="false"
Text="Validation Failed!"
runat="server" />
</form>

</body>
</html>