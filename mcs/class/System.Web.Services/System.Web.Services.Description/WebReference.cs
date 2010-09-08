// 
// System.Web.Services.Description.WebReference.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Web.Services.Discovery;
using System.Collections.Specialized;
using System.CodeDom;

namespace System.Web.Services.Description 
{
#if NET_2_0
	public
#else
	internal
#endif
	sealed class WebReference
	{
		DiscoveryClientDocumentCollection _documents;
#if !TARGET_J2EE && !MOBILE
		CodeNamespace _proxyCode;
		ServiceDescriptionImportWarnings _warnings;
#endif
		string _protocolName;
		string _appSettingUrlKey;
		string _appSettingBaseUrl;
		StringCollection _validationWarnings;
		
#if !TARGET_J2EE && !MOBILE
		public WebReference (DiscoveryClientDocumentCollection documents, CodeNamespace proxyCode)
		{
			if (documents == null) throw new ArgumentNullException ("documents");
			if (proxyCode == null) throw new ArgumentNullException ("proxyCode");
			
			_documents = documents;
			_proxyCode = proxyCode;
		}
		
		public WebReference (DiscoveryClientDocumentCollection documents, CodeNamespace proxyCode, string appSettingUrlKey, string appSettingBaseUrl)
			: this (documents, proxyCode, String.Empty, appSettingUrlKey, appSettingBaseUrl)
		{
		}
		
		public WebReference (DiscoveryClientDocumentCollection documents, CodeNamespace proxyCode, string protocolName, string appSettingUrlKey, string appSettingBaseUrl)
		{
			if (documents == null) throw new ArgumentNullException ("documents");
			if (proxyCode == null) throw new ArgumentNullException ("proxyCode");
			
			_documents = documents;
			_proxyCode = proxyCode;
			_protocolName = protocolName;
			_appSettingUrlKey = appSettingUrlKey;
			_appSettingBaseUrl = appSettingBaseUrl;
		}
#endif		
		public string AppSettingBaseUrl {
			get { return _appSettingBaseUrl; }
		}
		
		public string AppSettingUrlKey {
			get { return _appSettingUrlKey; }
		}
		
		public DiscoveryClientDocumentCollection Documents {
			get { return _documents; }
		}

		public string ProtocolName {
			get { return _protocolName; }
			set { _protocolName = value; }
		}
#if !TARGET_J2EE && !MOBILE
		public CodeNamespace ProxyCode {
			get { return _proxyCode; }
		}

		public ServiceDescriptionImportWarnings Warnings {
			get { return _warnings; }
			set { _warnings = value; }
		}
#endif
		public StringCollection ValidationWarnings {
			get { 
				if (_validationWarnings == null) _validationWarnings = new StringCollection ();
				return _validationWarnings; 
			}
		}

		internal void SetValidationWarnings (StringCollection col)
		{
			_validationWarnings = col;
		}
	}
}

