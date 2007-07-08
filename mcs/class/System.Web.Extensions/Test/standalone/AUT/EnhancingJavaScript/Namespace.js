Type.registerNamespace("Demo");

Demo.Person = function(firstName, lastName, emailAddress) {
    this._firstName = firstName;
    this._lastName = lastName;
    this._emailAddress = emailAddress;
}

Demo.Person.prototype = {
    
    getFirstName: function() {
        return this._firstName;
    },
    
    getLastName: function() {
        return this._lastName;
    },
    
    getName: function() {
        return this._firstName + ' ' + this._lastName;
    },

    dispose: function() {
        alert('bye ' + this.getName());
    }
}
Demo.Person.registerClass('Demo.Person', null, Sys.IDisposable);

// Notify ScriptManager that this is the end of the script.
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

