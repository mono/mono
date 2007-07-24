<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Namespace</title>
</head>

<body>
    <form id="Main" runat="server">
        <asp:ScriptManager runat="server" ID="scriptManager" />
    </form>

    <div>
        <p>This example creates an instance of the Person class 
            and puts it in the "Demo" namespace.</p>

        <input id="Button1" value="Create Demo.Person" 
            type="button" onclick="return OnButton1Click()" />
        
    </div>

    <script type="text/javascript" src="Namespace.js"></script>
    <script type="text/javascript" language="JavaScript">

    function OnButton1Click() 
    {
        var testPerson = new Demo.Person(  
            'John', 'Smith', 'john.smith@example.com');
        alert(testPerson.getFirstName() + " " +    
            testPerson.getLastName() );

        return false;
    }
    

    </script>

</body>
</html>
