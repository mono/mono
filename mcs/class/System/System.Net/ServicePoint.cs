//
// System.Net.ServicePoint
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Security.Cryptography.X509Certificates;

namespace System.Net 
{
	public class ServicePoint
	{
		private Uri uri;
		private int connectionLimit;
		private int maxIdleTime;
		
		// Constructors
		internal ServicePoint (Uri uri, int connectionLimit, int maxIdleTime)
		{
			this.uri = uri;
			this.connectionLimit = connectionLimit;
			this.maxIdleTime = maxIdleTime;
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
		
		[MonoTODO]
		public int ConnectionLimit {
			get { return connectionLimit; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();
				connectionLimit = value;
			}
		}
		
		[MonoTODO]
		public string ConnectionName {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int CurrentConnections {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]		
		public DateTime IdleSince {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int MaxIdleTime {
			get { return maxIdleTime; }
			set { this.maxIdleTime = value; }
		}
		
		[MonoTODO]
		public virtual Version ProtocolVersion {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public bool SupportsPipelining {
			get { throw new NotImplementedException (); }
		}
		
		// Methods
		
		[MonoTODO]
		public override int GetHashCode() 
		{
			throw new NotImplementedException ();
		}
	}
}