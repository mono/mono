<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Inheritance</title>
</head>

<body>
    <form id="Main" runat="server">
        <asp:ScriptManager runat="server" ID="scriptManager" />
    <script type="text/javascript" src="Inheritance.js"></script>
    </form>
    
    <h2>Inheritance</h2>
    <p />

    <div>
        This file contains two classes defined in script: Person and Employee, where
        Employee derives from Person.
        <p />
        
        Each class has private fields, and public properties and methods. In addition,
        Employee overrides the toString implementation, and in doing so, it uses the
        base class functionality.
        <p />

        This example puts the Person class in the "Demo" namespace.
        <p />
    </div>


    <div>
        <ul>
            <li><a href="#" onclick="return OnTestNewClick()">Object Creation</a></li>
            <li><a href="#" onclick="return OnTestDisposeClick()">Object Dispose</a></li>
            <li><a href="#" onclick="return OnTestPrivatePropertyClick()">Public vs. Private Properties</a></li>
            <li><a href="#" onclick="return OnTestInstanceMethodClick()">Instance Methods</a></li>
            <li><a href="#" onclick="return OnTestOverrideMethodClick()">Overriden Methods</a></li>
            <li><a href="#" onclick="return OnTestInstanceOfClick()">Instance Of Check</a></li>
        </ul>
    </div>

    <script type="text/javascript" language="JavaScript">

    function GetTestPerson() 
    {
        return new Demo.Person('Jane', 'Doe', 'jane.doe@example.com');
    }

    function GetTestEmployee() 
    {
        return new Demo.Employee('John', 'Doe', 'john.doe@example.com', 'Platform', 'Programmer');
    }

    function OnTestNewClick() {
        var aPerson = GetTestPerson();

        alert(aPerson.getFirstName());
        alert(aPerson);
        alert(Object.getType(aPerson).getName());

        var testPerson = GetTestPerson();
        alert(testPerson.getFirstName());
        alert(testPerson);

        return false;
    }
    
    function OnTestDisposeClick() {
        var aPerson = GetTestEmployee();
        alert(aPerson.getFirstName());
        aPerson.dispose();
    }

    function OnTestPrivatePropertyClick() {
        var aPerson = GetTestEmployee();
        alert('aPerson._firstName = ' + aPerson._firstName);
        alert('aPersona.getFirstName() = ' + aPerson.getFirstName());

        return false;
    }

    function OnTestInstanceMethodClick() {
        var aPerson = GetTestEmployee();
        aPerson.sendMail('Hello', 'This is a test mail.');

        return false;
    }

    function OnTestOverrideMethodClick() {
        var testPerson = GetTestEmployee();
        alert(testPerson);

        return false;
    }

    function OnTestInstanceOfClick() {
        var aPerson = GetTestEmployee();
        if (Demo.Employee.isInstanceOfType(aPerson)) {
            alert(aPerson.getName() + ' is an Employee instance.\nTitle property: ' + aPerson.getTitle());
        }

        return false;
    }

    </script>
</body>
</html>
 

 
