//
// This test probes that we treat events differently than fields
// This used to cause a compiler crash.
//
using System;

delegate void PersonArrivedHandler (object source, PersonArrivedArgs args);

class PersonArrivedArgs /*: EventArgs*/ {
    public string name;
    public PersonArrivedArgs (string name) {
	this.name = name;
    }
}

class Greeter {
    string greeting;

    public Greeter (string greeting) {
	this.greeting = greeting;
    }

    public void HandlePersonArrived (object source, PersonArrivedArgs args) {
	Console.WriteLine(greeting, args.name);
    }
}

class Room {
    public event PersonArrivedHandler PersonArrived;

    public Room () {
	    // Assign a value to it, this also used to crash the compiler.
	    PersonArrived = null;
    }

    public void AddPerson (string name) {
	PersonArrived(this, null); //(this, PersonArrivedArgs(name));
    }
}

class DelegateTest {
    public static int Main () {
	return 0;
    }
}


