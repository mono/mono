// The event access is illegal and it should bail out

using System;

delegate void MyEventHandler();

class MyEvent {
	public event MyEventHandler SomeEvent;
}

class EventDemo {

	public static void Main(){
		MyEvent evt = new MyEvent();
		// CS0070
                evt.SomeEvent();
        }
}
