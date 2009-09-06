using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

class Class1 {
    delegate void Action ();

    static void Main ()
    {
        Dispatcher d = Dispatcher.CurrentDispatcher;
        Action a = delegate {
		object x = d;
            d.Invoke (DispatcherPriority.Normal, new Action (mine));
            Console.WriteLine ("Task");
        };

	d.BeginInvoke (DispatcherPriority.Normal, (Action) delegate {
		Console.WriteLine ("First");
	});
        d.BeginInvoke (DispatcherPriority.Normal, (Action) delegate {
		Console.WriteLine ("Second");
		d.InvokeShutdown ();
	});
	d.BeginInvoke (DispatcherPriority.Send, (Action) delegate {
		Console.WriteLine ("High Priority");
		d.BeginInvoke (DispatcherPriority.Send, (Action) delegate {
			Console.WriteLine ("INSERTED");
		});
	});
	d.BeginInvoke (DispatcherPriority.SystemIdle, (Action) delegate {
		Console.WriteLine ("Idle");
	});

	Dispatcher.Run ();
    }

    static void mine ()
    {
        Console.WriteLine ("Mine");
    }
}
