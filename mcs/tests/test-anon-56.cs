
delegate void QueueHandler (Observable sender);

class Observable {
	static QueueHandler Queue;
	  
	public static void Main (string[] args) {
		Queue += (QueueHandler) delegate { System.Console.WriteLine ("OK"); };
	}
}