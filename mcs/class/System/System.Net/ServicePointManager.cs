//
// System.Net.ServicePointManager
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Security.Cryptography.X509Certificates;

//
// notes:
// A service point manager manages service points (duh!).
// A service point maintains a list of connections (per scheme + authority 
// seems logical).
// According to HttpWebRequest.ConnectionGroupName each connection group
// creates additional connections. therefor, a service point has a hashtable
// of connection groups where each value is a list of connections.
// 
// when we need to make an HttpWebRequest, we need to do the following:
// 1. find service point, given Uri and Proxy 
// 2. find connection group, given service point and group name
// 3. find free connection in connection group, or create one (if ok due to limits)
// 4. lease connection
// 5. execute request
// 6. when finished, return connection
//


namespace System.Net 
{
	public class ServicePointManager
	{
		private static ICertificatePolicy policy = null;
		private static int defaultConnectionLimit = DefaultPersistentConnectionLimit;
		private static int maxServicePointIdleTime = 900000; // 15 minutes
		private static int maxServicePoints = 0;
		
		// Fields
		
		public const int DefaultNonPersistentConnectionLimit = 4;
		public const int DefaultPersistentConnectionLimit = 2;
		
		// Constructors
		private ServicePointManager ()
		{
		}		
		
		// Properties
		
		public static ICertificatePolicy CertificatePolicy {
			get { return policy; }
			set { policy = value; }
		}
		
		public static int DefaultConnectionLimit {
			get { return defaultConnectionLimit; }
			set { 
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				defaultConnectionLimit = value; 
			}
		}
		
		public static int MaxServicePointIdleTime {
			get { 
				return maxServicePointIdleTime;
			}
			set { 
				if (value < -2 || value > Int32.MaxValue)
					throw new ArgumentOutOfRangeException ("value");
				maxServicePointIdleTime = value;
			}
		}
		
		public static int MaxServicePoints {
			get { 
				return maxServicePoints; 
			}
			set {  
				if (value < 0)
					throw new ArgumentException ("value");
				maxServicePoints = value;
			}
		}
		
		// Methods
		
		public static ServicePoint FindServicePoint (Uri address) 
		{
			return FindServicePoint (address, GlobalProxySelection.Select);
		}
		
		public static ServicePoint FindServicePoint (string uriString, IWebProxy proxy)
		{
			return FindServicePoint (new Uri(uriString), proxy);
		}
		
		public static ServicePoint FindServicePoint (Uri address, IWebProxy proxy)
		{
			if (address == null)
				throw new ArgumentNullException ("address");
			// if ()
			//	throw new InvalidOperationException ("maximum number of service points reached");
			
			throw new NotImplementedException ();
		}
	}
}