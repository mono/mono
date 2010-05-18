// It's actually C# specification and csc bug
// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=310363

class C1
{
	static void Foo (string val)
	{
		const object obj = null;
		switch (val) {
			case (string) obj:
				return;
		}
	}
}

class C2
{
	public static void Main ()
	{
		// BUG compatibility for now
		//const object o = null;
		//const string s = (string) o;
	}
}
