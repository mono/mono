//
// System.Net.ServicePoint
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace System.Net 
{
	public class ServicePoint
	{
		private Uri uri;
		private int connectionLimit;
		private int maxIdleTime;
		private int currentConnections;
		private DateTime idleSince;
		private Version protocolVersion;
		
		// Constructors

		internal ServicePoint (Uri uri, int connectionLimit, int maxIdleTime)
		{
			this.uri = uri;  
			this.connectionLimit = connectionLimit;
			this.maxIdleTime = maxIdleTime;			
			this.currentConnections = 0;
			this.idleSince = DateTime.Now;
		}
		
		// Properties
		
		public Uri Address {
			get { return this.uri; }
		}
		
		[MonoTODO]
		public X509Certificate Certificate {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public X509Certificate ClientCertificate {
			get { throw new NotImplementedException (); }
		}
		
		public int ConnectionLimit {
			get { return connectionLimit; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();
				connectionLimit = value;
			}
		}
		
		public string ConnectionName {
			get { return uri.Scheme; }
		}

		public int CurrentConnections {
			get { return currentConnections; }
		}

		public DateTime IdleSince {
			get { return idleSince; }
		}
		
		public int MaxIdleTime {
			get { return maxIdleTime; }
			set { 
				if (value < Timeout.Infinite || value > Int32.MaxValue)
					throw new ArgumentOutOfRangeException ();
				this.maxIdleTime = value; 
			}
		}
		
		public virtual Version ProtocolVersion {
			get { return protocolVersion; }
		}
		
		public bool SupportsPipelining {
			get { return HttpVersion.Version11.Equals (protocolVersion); }
		}
		
		// Methods
		
		public override int GetHashCode() 
		{
			return base.GetHashCode ();
		}
		
		// Internal Methods

		internal bool AvailableForRecycling {
			get { 
				return CurrentConnections == 0
				    && maxIdleTime != Timeout.Infinite
			            && DateTime.Now >= IdleSince.AddMilliseconds (maxIdleTime);
			}
		}
	}
}