<%@ Page Language="C#" AutoEventWireup="True" %>

 <html>
 <head>

    <script runat="server">
 
       void ValidateBtn_Click(Object sender, EventArgs e) 
       {
          if (Page.IsValid) 
          {
             lblOutput.Text = "Page is Valid!";
          }
          else 
          {
             lblOutput.Text = "Page is InValid! :-(";
          }
       }
 
    </script>
 
 </head>
 <body>
 
    <h3>RegularExpressionValidator Example</h3>
    <p>
 
    <form runat="server">
 
       <table bgcolor="#eeeeee" cellpadding="10">
          <tr valign="top">
             <td colspan="3">
                <asp:Label ID="lblOutput" 
                     Text="Enter a number greater than 50"
                     runat="server"/>
             </td>
          </tr>
 
          <tr>
             <td colspan="3">
                <b>Information</b>
             </td>
          </tr>
          <tr>
             <td align="right">
                Value:
             </td>
             <td>
                <asp:TextBox id="TextBox1" 
                     runat="server"/>
             </td>
             <td>
		<asp:CompareValidator id="CompareValidator1"
		     ControlToValidate="TextBox1"
		     Operator="greaterthan"
		     Type="Integer"
		     ValueToCompare="xx"
		     Display="Static"
		     ErrorMessage="Value must be greater than 50"
		     EnableClientScript="true"
		     runat="server"/>
             </td>
          </tr>
          <tr>
             <td></td>
             <td>
                <asp:Button text="Validate" 
                     OnClick="ValidateBtn_Click" 
                     runat=server />
             </td>
             <td></td>
          </tr>
       </table>
 
    </form>
 
 </body>
 </html>
