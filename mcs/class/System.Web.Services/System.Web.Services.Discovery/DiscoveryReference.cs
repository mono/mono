// 
// System.Web.Services.Discovery.DiscoveryReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.IO;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {
	public abstract class DiscoveryReference {
		
		#region Fields

		private string defaultFilename;
		private DiscoveryClientProtocol clientProtocol;

		#endregion // Fields

		#region Constructors

		protected DiscoveryReference () 
		{
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public DiscoveryClientProtocol ClientProtocol {
			get { return clientProtocol; }			
			set { clientProtocol = value; }
			
		}
		
		public virtual string DefaultFilename {
			get { return FilenameFromUrl (Url); }			
		}
		
		[XmlIgnore]
		public abstract string Url {
			get;		
			set;
		}
		
		#endregion // Properties

		#region Methods

		protected static string FilenameFromUrl (string url)
		{
			if (url.ToLower().EndsWith ("/wsdl"))
				url = url.Substring (0,url.Length-5);
			else if (url.ToLower().EndsWith ("/soap"))
				url = url.Substring (0,url.Length-5);
			else if (url.ToLower().EndsWith ("/wsdl.jsp"))
				url = url.Substring (0,url.Length-9);
			else if (url.ToLower().EndsWith ("/soap.wsdl"))
				url = url.Substring (0,url.Length-10);
			
			int i = url.LastIndexOf ("/");
			if (i != -1) url = url.Substring (i+1);
			
			i = url.IndexOfAny (new char[] {'.','?','\\'});
			if (i != -1) url = url.Substring (0,i);
			
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			for (int n=0; n<url.Length; n++)
				if (Char.IsLetterOrDigit (url[n]) || url[n] == '_') sb.Append (url[n]);
				
			return sb.ToString ();
		}
		
		public abstract object ReadDocument (Stream stream);
		
		public void Resolve () 
		{
			if (clientProtocol == null) 
				throw new InvalidOperationException ("The ClientProtocol property is a null reference.");
			
			if (clientProtocol.Documents.Contains (Url)) 	// Already resolved
				return;
			
			try
			{
				string contentType = null;
				string url = Url;
				Stream stream = clientProtocol.Download (ref url, ref contentType);
				Resolve (contentType, stream);
			}
			catch (Exception ex)
			{
				ReportError (Url, ex);
			}
		}
                
		protected internal abstract void Resolve (string contentType, Stream stream);
		
		public abstract void WriteDocument (object document, Stream stream);

		internal void ReportError (string url, Exception ex)
		{
			if (ex is DiscoveryException) throw ex;
			else throw new DiscoveryException (url, ex);
		}
		
		#endregion // Methods
	}
}
