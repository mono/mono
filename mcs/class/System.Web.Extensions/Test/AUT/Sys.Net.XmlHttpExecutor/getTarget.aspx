<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
	protected override void OnInit (EventArgs e) {
		base.OnInit (e);
		System.Threading.Thread.Sleep (30000);
	}
</script>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Hello Page</title>
</head>
<body>
    <p> 
   
        ...hello there. Congratulations, you got here via a GET Web Request!
    
     </p>
</body>
</html>
