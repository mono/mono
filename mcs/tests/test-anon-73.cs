using System;
using System.Threading;

// Cloning mechanism manual tests

delegate void D (object o);

class T {
	public static void Main ()
	{
			D d = delegate (object state) {
			try {
			} catch {
				throw;
			} finally {
			}
			};
	}
		
	static void Test_1 ()
	{
		System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
		{
			using (System.Threading.Timer t = new System.Threading.Timer (null, null, 1, 1))
			{
			}
		});
	}		
}


