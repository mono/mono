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
	const object o2 = c2;
	const string c2 = null;
	
	public static void Main ()
	{
		const object o = null;
		const string s = (string) o;
	}
}
