<%@ Page Language="C#" AutoEventWireup="True" %>

<html>

<head>

   <script runat="server">

      void ButtonClick(Object sender, EventArgs e)
      {

         if (Page.IsValid)
         {
            Label1.Text="Page is valid.";
         }
         else
         {
            Label1.Text="Page is not valid!!";
         }

      }

   </script>

</head>

<body>

   <form runat="server">

      <h3>RangeValidator Example</h3>

      Enter a number from 1 to 10:

      <br>

      <asp:TextBox id="TextBox1"
           runat="server"/>

      <br>

      <asp:RangeValidator id="Range1"
           ControlToValidate="TextBox1"
           MinimumValue="1"
           MaximumValue="10"
           Type="Integer"
           EnableClientScript="false"
           Text="The value must be from 1 to 10!"
           runat="server"/>

      <br><br>

      <asp:Label id="Label1"
           runat="server"/>

      <br><br>

      <asp:Button id="Button1"
           Text="Submit"
           OnClick="ButtonClick"
           runat="server"/>
            

   </form>

</body>
</html>
