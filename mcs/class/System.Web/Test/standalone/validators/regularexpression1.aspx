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
                     Text="Enter a 5 digit zip code" 
                     runat="server"/>
             </td>
          </tr>
 
          <tr>
             <td colspan="3">
                <b>Personal Information</b>
             </td>
          </tr>
          <tr>
             <td align="right">
                Zip Code:
             </td>
             <td>
                <asp:TextBox id="TextBox1" 
                     runat="server"/>
             </td>
             <td>
                <asp:RegularExpressionValidator id="RegularExpressionValidator1" 
                     ControlToValidate="TextBox1"
                     ValidationExpression="\d{5}"
                     Display="Static"
                     ErrorMessage="Zip code must be 5 numeric digits"
                     EnableClientScript="True" 
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
