<%@ Page Language = "C#" %>

<html><head><title>Bug 517656</title><head><body>
<p>ASP.NET repeater control to follow...</p>
<asp:Repeater id="censusRepeater" runat="server">
	<HeaderTemplate />
	<ItemTemplate>
    	<tr>
    		<td><asp:Label id="idLabel" runat="server" /></td>
    		<td>
    			<asp:TextBox id="birthBox" runat="server" />
    		</td>
    		<!-- We do not ask if you are married
    		<td>
    			<asp:radiobuttonlist id="spouseRadioButtonList" runat="server">
    				<asp:listitem id="Married" runat="server" value="Yes" />
    				<asp:listitem id="Single" runat="server" value="No" />
    			</asp:radiobuttonlist>
    			<asp:Checkbox id="marriedCheckbox" runat="server" />
  			</td>
  			-->
  			<td>
  				<asp:TextBox id="spouseBirthBox" runat="server" />
   			</td>
  			<td>
  				<asp:DropDownList id="childrenDropDownList" runat="server">
  					<asp:listitem>0</asp:listitem>
  					<asp:listitem>1</asp:listitem>
  					<asp:listitem>2+</asp:listitem>
  				</asp:DropDownList>
  			</td>
    	</tr>
	</ItemTemplate>
	<FooterTemplate />
</asp:Repeater>
</body></html>
