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

		#region Fields

		private IList additionalInformation;
		private DiscoveryClientDocumentCollection documents;
		private DiscoveryExceptionDictionary errors;
		private DiscoveryClientReferenceCollection references;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public DiscoveryClientProtocol () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public IList AdditionalInformation {
			get { return additionalInformation; }
		}
		
		public DiscoveryClientDocumentCollection Documents {
			get { return documents; }
		}
		
		public DiscoveryExceptionDictionary Errors {
			get { return errors; }
		}

		public DiscoveryClientReferenceCollection References {
			get { return references; }
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
			
			#region Fields
			
			private DiscoveryClientResultCollection results;

			#endregion // Fields

			#region Contructors
			
			[MonoTODO]
			public DiscoveryClientResultsFile () 
			{
				throw new NotImplementedException ();
			}
		
			#endregion // Constructors
			
			#region Properties
		
			public DiscoveryClientResultCollection Results {				
				get { return results; }
			}
			
			#endregion // Properties
		}
		#endregion // Classes
	}
}
