<html>
<head>
<!--
 Author: Gaurav Vaish
 Original Source: http://msdn.microsoft.com/library/en-us/cpref/html/frlrfSystemWebUIWebControlsCalendarClassSelectedDateTopic.asp
 Copyright: (C) Gaurav Vaish, 2001
-->
   <script language="C#" runat="server">

      void Selection_Change(Object sender, EventArgs e) 
      {
         Label1.Text = "The selected date is " + Calendar1.SelectedDate.ToShortDateString();
      }

      void Selection_Change_Month(Object sender, EventArgs e)
      {
         Label2.Text = "The selected date is " + Calendar2.SelectedDate.ToShortDateString();
      }

      void Selection_Change_DWM(Object sender, EventArgs e)
      {
         Label3.Text = "The selected date is " + Calendar3.SelectedDate.ToShortDateString();
      }

   </script>

</head>
<body>

   <form runat="server">

      <h3><font face="Verdana">Calendar Example</font></h3>

      Select a date on the Calendar control.<br><br>

      <asp:Calendar ID="Calendar1" runat="server"  
           SelectionMode="Day" 
           ShowGridLines="True"
           OnSelectionChanged="Selection_Change">
 
         <SelectedDayStyle BackColor="Yellow"
                           ForeColor="Red">
         </SelectedDayStyle>
      
      </asp:Calendar>     

      <asp:Label id="Label1" runat=server />

      <hr><br>

     <asp:Calendar ID="Calendar2" runat="server"  
           SelectionMode="DayWeek"
           ShowGridLines="True"
           OnSelectionChanged="Selection_Change_Month">
 
         <SelectedDayStyle BackColor="Yellow"
                           ForeColor="Red">
         </SelectedDayStyle>
      
      </asp:Calendar>     

      <asp:Label id="Label2" runat=server />

      <hr><br>

     <asp:Calendar ID="Calendar3" runat="server"  
           SelectionMode="DayWeekMonth"
           ShowGridLines="True"
           OnSelectionChanged="Selection_Change_DWM">
 
         <SelectedDayStyle BackColor="Yellow"
                           ForeColor="Red">
         </SelectedDayStyle>
      
      </asp:Calendar>     

      <asp:Label id="Label3" runat=server />

      <hr><br>

   </form>
</body>
</html>