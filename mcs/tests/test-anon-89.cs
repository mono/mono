using System;

class C
{
	public delegate void D ();

	int[] chats;
	int total;

	public static int Main ()
	{
		new C ().Test ();
		return 0;
	}

	object GdkWindow
	{
		get { return null; }
		set { }
	}

	public static void Invoke (D d)
	{
	}

	void Test ()
	{
		try {
			if (total < 0)
				return;

			int x = 0;

			Invoke (delegate {
				try {
					Invoke (delegate {
						GdkWindow = null;
					});

					total = x;
					int[] chats = new int[] { 1, 2 };

					Invoke (delegate {
						foreach (int chat in chats) {
							total = chat;
						}
					});
				} finally {
					Invoke (delegate {
						if (GdkWindow != null) {
							GdkWindow = null;
						}
					});
				}
			});
		} catch {
			int x = 9;
		}
	}
}
