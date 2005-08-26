<%@ Page AutoEventWireup="True" %>

<html>
<head>
   <script language="C#" runat="server">
	public void Button_Click (object sender, EventArgs e)
	{
		Random rand_number = new Random();

		Compare1.ValueToCompare = rand_number.Next(1,10).ToString();
		Compare1.Validate();

		if (Page.IsValid)
			lblOutput.Text = "You guessed correctly!!";
		else
			lblOutput.Text = "You guessed poorly";

		lblOutput.Text += "<br><br>" + "The number is: " + Compare1.ValueToCompare;
	}
   </script>
 
</head>
<body>
 
   <form runat=server>

      <h3>Validator Example</h3>

      <h5>Pick a number between 1 and 10:</h5>
     
      <asp:TextBox id="TextBox1" 
           runat="server"/>

      <asp:CompareValidator id="Compare1" 
           ControlToValidate="TextBox1"
           ValueToCompare="0"
           EnableClientScript="False"  
           Type="Integer"
           Display="Dynamic" 
           ErrorMessage="Incorrect guess!!"
           Text="*"
           runat="server"/>

      <asp:RequiredFieldValidator id="Require1" 
           ControlToValidate="TextBox1"
           EnableClientScript="False"
           Display="Dynamic" 
           ErrorMessage="No number entered!!"
           Text="*"
           runat="server"/>

      <br><br>

      <asp:Button id="Button1"
           Text="Submit"
           OnClick="Button_Click"
           runat="server"/>
 
      <br><br>
       
      <asp:Label id="lblOutput" 
           Font-Name="verdana" 
           Font-Size="10pt" 
           runat="server"/>

      <br><br>

      <asp:ValidationSummary
           id="Summary1"
           runat="server"/>
 
   </form>
 
</body>
</html>
