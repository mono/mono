using System;

class C
{
	void Test ()
	{
		Action a = delegate {
			{
				object ca = 3;
				{
					int location = 0;

					Action a2 = delegate {
						{
							location = 1;

							object routing = ca;

							Action a4 = delegate {
								if (routing != null) {
									Console.WriteLine ("ok");
								}
							};
							
							a4 ();
						}
					};
					
					a2 ();
				}
			}
		};
		
		a ();
	}

	public static int Main ()
	{
		new C ().Test ();
		return 0;
	}
}