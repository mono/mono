//
// System.Net.WebConnectionGroup
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;
using System.Net.Sockets;

namespace System.Net
{
	class WebConnectionGroup
	{
		ServicePoint sPoint;
		string name;
		ArrayList connections;

		public WebConnectionGroup (ServicePoint sPoint, string name)
		{
			this.sPoint = sPoint;
			this.name = name;
			connections = new ArrayList (1);
		}

		public WebConnection GetConnection (string name)
		{
			WebConnection cnc = null;
			lock (connections) {
				WeakReference cncRef = null;

				// Remove disposed connections
				int end = connections.Count;
				ArrayList removed = null;
				for (int i = 0; i < end; i++) {
					cncRef = (WeakReference) connections [i];
					cnc = cncRef.Target as WebConnection;
					if (cnc == null) {
						if (removed == null)
							removed = new ArrayList (1);

						removed.Add (i);
					}
				}

				if (removed != null) {
					for (int i = removed.Count - 1; i >= 0; i--)
						connections.RemoveAt ((int) removed [i]);
				}

				//TODO: Should use the limits in the config file.
				if (connections.Count == 0) {
					cnc = new WebConnection (this, sPoint);
					connections.Add (new WeakReference (cnc));
				} else {
					cncRef = (WeakReference) connections [connections.Count - 1];
					cnc = cncRef.Target as WebConnection;
				}
			}

			return cnc;
		}

		public string Name {
			get { return name; }
		}
		
	}
}

