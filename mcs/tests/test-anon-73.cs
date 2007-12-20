using System;
using System.Threading;

delegate void D (object o);

class T {
        static void Main () {
			D d = delegate (object state) {
			try {
			} catch {
				throw;
			} finally {
			}
			};
		}
}


