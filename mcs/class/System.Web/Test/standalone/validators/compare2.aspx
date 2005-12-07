<%@ Page Language="C#" AutoEventWireup="True" %>
 
<html>
<head>
   <script runat="server">
 
      void Button_Click(Object sender, EventArgs e) 
      {
 
         if (Page.IsValid) 
         {
            lblOutput.Text = "Result: Valid!";
         }
         else 
         {
            lblOutput.Text = "Result: Not valid!";
         }

      }
 
      void Operator_Index_Changed(Object sender, EventArgs e) 
      {

         Compare1.Operator = (ValidationCompareOperator) ListOperator.SelectedIndex;
         Compare1.Validate();

      }

      void Type_Index_Changed(Object sender, EventArgs e) 
      {

         Compare1.Type = (ValidationDataType) ListType.SelectedIndex;
         Compare1.Validate();

      }
 
   </script>
 
</head>
<body>
 
   <form runat=server>

      <h3>CompareValidator Example</h3>
      <p>
      Enter a value in each textbox. Select a comparison operator<br>
      and data type. Click "Validate" to compare values.
 
      <table bgcolor="#eeeeee" cellpadding=10>

         <tr valign="top">

            <td>

               <h5>String 1:</h5>
               <asp:TextBox id="TextBox1" 
                    runat="server"/>

            </td>

            <td>

               <h5>Comparison Operator:</h5>
 
               <asp:ListBox id="ListOperator" 
                    OnSelectedIndexChanged="Operator_Index_Changed" 
                    runat="server">

                  <asp:ListItem Selected Value="Equal">Equal</asp:ListItem>
                  <asp:ListItem Value="NotEqual">NotEqual</asp:ListItem>
                  <asp:ListItem Value="GreaterThan">GreaterThan</asp:ListItem>
                  <asp:ListItem Value="GreaterThanEqual">GreaterThanEqual</asp:ListItem>
                  <asp:ListItem Value="LessThan">LessThan</asp:ListItem>
                  <asp:ListItem Value="LessThanEqual">LessThanEqual</asp:ListItem>
                  <asp:ListItem Value="DataTypeCheck">DataTypeCheck</asp:ListItem>

               </asp:ListBox>

            </td>

            <td>

               <h5>String 2:</h5>
               <asp:TextBox id="TextBox2" 
                    runat="server"/>
               <p>
               <asp:Button id="Button1"
                    Text="Validate"  
                    OnClick="Button_Click" 
                    runat="server"/>

            </td>
         </tr>

         <tr>
            <td colspan="3" align="center">

               <h5>Data Type:</h5>

               <asp:ListBox id="ListType" 
                    OnSelectedIndexChanged="Type_Index_Changed" 
                    runat="server">

                  <asp:ListItem Selected Value="String" >String</asp:ListItem>
                  <asp:ListItem Value="Integer" >Integer</asp:ListItem>
                  <asp:ListItem Value="Double" >Double</asp:ListItem>
                  <asp:ListItem Value="Date" >Date</asp:ListItem>
                  <asp:ListItem Value="Currency" >Currency</asp:ListItem>

               </asp:ListBox>
            </td>
         </tr>
      </table>
 
      <asp:CompareValidator id="Compare1" 
           ControlToValidate="TextBox1" 
           ControlToCompare="TextBox2"
           EnableClientScript="False" 
           Type="String" 
           runat="server"/>
 
      <br>
       
      <asp:Label id="lblOutput" 
           Font-Name="verdana" 
           Font-Size="10pt" 
           runat="server"/>
 
   </form>
 
</body>
</html>
