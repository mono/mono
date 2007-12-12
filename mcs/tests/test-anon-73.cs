using System;
using System.Threading;

class T {
        static void Main () {
		new Thread (delegate (object state) {
			try {
			} catch {
				throw;
			} finally {
			}
		});
        }

}


