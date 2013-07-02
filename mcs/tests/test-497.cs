using System;

public class PlotMenuItem
{
	private EventHandler callback_;
	static int set;

	public PlotMenuItem ()
	{
	}

	public PlotMenuItem (EventHandler callback)
	{
		callback_ = callback;

		PlotMenuItem child = new PlotMenuItem ();
		child.Callback += new EventHandler (callback);
	}

	public static int Main ()
	{
		PlotMenuItem pmi = new PlotMenuItem (new EventHandler (MenuItem_Click));
		pmi.Callback (null, null);
		
		if (set != 999)
			return 1;
			
		return 0;
	}

	static void MenuItem_Click (object sender, EventArgs e)
	{
		set = 999;
	}

	public EventHandler Callback {
		get { return callback_; }
		set { callback_ = value; }
	}
}
