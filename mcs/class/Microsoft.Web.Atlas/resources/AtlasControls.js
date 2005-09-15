Type.registerNamespace ('Web.UI');

// Button
Web.UI.Button = function(ele) {
   Web.UI.Button.initializeBase(ele);
}
Type.registerClass ('Web.UI.Button', Web.UI.Control, null /* interface(s) */);

Web.UI.Button.prototype.initialize = function () {
    Web.UI.Button.callBaseMethod ()

    return Demo.Employee.callBaseMethod(this, 'toString') + '\r\n' + this.getTitle() + '\r\n' + this.getTeam();	
}


// Control
//
Web.UI.Control = function(ele) {
   Web.UI.Control.initializeBase(ele);
}
Type.registerClass ('Web.UI.Control', null, null /* interface(s) */);

// Select
//
Web.UI.Select = function(ele) {
    Web.UI.Button.initializeBase(ele);
}
Type.registerClass ('Web.UI.Select', Web.UI.Control, null /* interface(s) */);



// TextBox
//
Web.UI.TextBox = function(ele) {
    Web.UI.Control.initializeBase(ele);
}
Type.registerClass ('Web.UI.TextBox', Web.UI.Control, null /* interface(s) */);


