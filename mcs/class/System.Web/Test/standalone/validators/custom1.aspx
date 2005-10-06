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

      <h3>CustomValidator Example</h3>

      Enter something

      <br>

      <asp:TextBox id="TextBox1"
           runat="server"/>

      <br>

      <asp:CustomValidator id="Custom1"
           ControlToValidate="TextBox1"
           ClientValidationFunction="OhBabyValidateMe"
           Text="Dude, watch what you're entering!"
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
