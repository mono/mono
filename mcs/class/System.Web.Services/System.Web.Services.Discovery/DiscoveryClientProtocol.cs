// 
// System.Web.Services.Protocols.DiscoveryClientProtocol.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.Collections;
using System.IO;
using System.Web.Services.Protocols;

namespace System.Web.Services.Discovery {
	public class DiscoveryClientProtocol : HttpWebClientProtocol {

		#region Constructors

		[MonoTODO]
		public DiscoveryClientProtocol () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public IList AdditionalInformation {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		public DiscoveryClientDocumentCollection Documents {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		public DiscoveryExceptionDictionary Errors {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public DiscoveryClientReferenceCollection References {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}		
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public DiscoveryDocument Discover (string url)
		{
                        throw new NotImplementedException ();
		}

		[MonoTODO]
		public DiscoveryDocument DiscoverAny (string url)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Stream Download (ref string url)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Stream Download (ref string url, ref string contentType)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public DiscoveryClientResultCollection ReadAll (string topLevelFilename)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResolveOneLevel ()
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public DiscoveryClientResultCollection WriteAll (string directory, string topLevelFilename)
		{
                        throw new NotImplementedException ();
		}
		
		#endregion // Methods
		
		#region Classes
		
		public sealed class DiscoveryClientResultsFile {
			
			#region Contructors
			
			[MonoTODO]
			public DiscoveryClientResultsFile () 
			{
				throw new NotImplementedException ();
			}
		
			#endregion // Constructors
			
			#region Properties
		
			public DiscoveryClientResultCollection Results {
				[MonoTODO]
				get { throw new NotImplementedException (); }
			}
			
			#endregion // Properties
		}
		#endregion // Classes
	}
}
