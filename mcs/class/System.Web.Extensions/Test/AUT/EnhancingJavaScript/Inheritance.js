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
    
    getEmailAddress: function() {
        return this._emailAddress;
    },
    setEmailAddress: function(emailAddress) {
        this._emailAddress = emailAddress;
    },
    
    getName: function() {
        return this._firstName + ' ' + this._lastName;
    },

    dispose: function() {
        alert('bye ' + this.getName());
    },
    
    sendMail: function() {
        var emailAddress = this.getEmailAddress();

        if (emailAddress.indexOf('@') < 0) {
            emailAddress = emailAddress + '@example.com';
        }
        alert('Sending mail to ' + emailAddress + ' ...');
    },
    
    toString: function() {
        return this.getName() + ' (' + this.getEmailAddress() + ')';
    }
}
Demo.Person.registerClass('Demo.Person', null, Sys.IDisposable);
Demo.Employee = function(firstName, lastName, emailAddress, team, title) {
    Demo.Employee.initializeBase(this, [firstName, lastName, emailAddress]);
    
    this._team = team;
    this._title = title;
}

Demo.Employee.prototype = {
    
    getTeam: function() {
        return this._team;
    },
    setTeam: function(team) {
        this._team = team;
    },
    
    getTitle: function() {
        return this._title;
    },
    setTitle: function(title) {
        this._title = title;
    },
    toString: function() {
        //return Demo.Employee.callBaseMethod(this, 'toString') + '\r\n' + this.getTitle() + '\r\n' + this.getTeam();
        return Demo.Employee.callBaseMethod(this, 'toString') + '\n' + this.getTitle() + '\n' + this.getTeam();
    }
}
Demo.Employee.registerClass('Demo.Employee', Demo.Person);
