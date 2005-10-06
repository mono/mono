<!-- bug on 45493. Right now, I am getting an error other than the one on the bug. the original issue was:

To reproduce the bug: 
 
1. in xsp, start the following aspx page. 
 
2. Select an exam, e.g. "calculus". 
 
3. Click "view subscriptions". The list of subscription correctly 
appears.  
 
4. Now click "view subscriptions" again, _without_ changing the 
selection (i.e. leave "calculus" selected). An error message appears, 
complaining that there was no exam selected. But it was! 

-->


<%@ language="C#" %> 
 
<script runat="server" > 
 
void Page_Load(Object Source, EventArgs E) {  
        if (!IsPostBack){ 
                #region fill the listbox exams 
                exams.Items.Clear(); 
                exams.Items.Add(new ListItem( "math")); 
                exams.Items.Add(new ListItem( "calculus")); 
                exams.Items.Add(new ListItem( "english"));       
                message_subscriptions.Visible=false; 
                subscriptions.Visible=false; 
                #endregion 
        } 
        else{ 
                if (exams.SelectedIndex >=0){    
                         
                        message_subscriptions.Visible=true; 
                        subscriptions.Visible=true; 
                        message.Text=""; 
                        message_subscriptions.Text = "Subscriptions to \"" + exams.SelectedItem.Text + "\" are:"; 
                        subscriptions.Items.Clear(); 
                        subscriptions.Items.Add(new ListItem( "John")); 
                        subscriptions.Items.Add(new ListItem( "Jack")); 
                } 
                else{ 
                        message.Text = "<h2>Error: no item selected. SelectedIndex = " +  
                                exams.SelectedIndex + " </h2>"; 
                } 
        } 
} 
</script> 
 
<html> 
 
<body> 
<asp:label id=message runat=server ForeColor=red/> 
<h1>Mono test page</h1> 
Select an exam, then click "view subscriptions". 
<form runat=server> 
<p><asp:listbox id="exams" runat=server  /> 
 
<p> <input type=submit value="View subscriptions"> 
<p><asp:label id="message_subscriptions" runat=server /> 
<p><asp:listbox id="subscriptions" runat=server  /> 
 
</form> 
 
</body> 
 
</html>


