interface X {

	// Methods
	new int IntegerMethod (int a, int b);
	new int IntegerMethod (int a, string c);
	new int StringMethod ();
	int A (string b);

	// Properties
	new string TheString { get; set; }
	int TheInt { get; }
	int TheInt2 { set; }
	int TheInt3 { set; get; }

	// Events
	new event int MyEvent;
	event string MyEvent2;

	// Indexers
}
	
	
