/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzmán Álvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls
{
	public sealed class TlsSessionSettings
	{
		#region FIELDS

		private string						serverName;
		private int							serverPort;
		private Encoding					encoding;
		private TlsProtocol					protocol;
		private TlsCompressionMethod		compressionMethod;
		private X509CertificateCollection	certificates;
	
		#endregion

		#region PROPERTIES

		public string ServerName
		{
			get { return serverName; }
			set { serverName = value; }
		}

		public int ServerPort
		{
			get { return serverPort; }
			set { serverPort = value; }
		}

		public Encoding Encoding
		{
			get { return encoding; }
			set { encoding = value; }
		}

		public TlsProtocol Protocol
		{
			get { return protocol; }
			set 
			{ 
				if (value != TlsProtocol.Tls1 &&
					value != TlsProtocol.Ssl3)
				{
					throw new NotSupportedException("Specified protocol is not supported");
				}
				protocol = value; 
			}
		}

		public TlsCompressionMethod CompressionMethod
		{
			get { return compressionMethod; }
			set 
			{ 
				if (value != TlsCompressionMethod.None)
				{
					throw new NotSupportedException("Specified compression method is not supported");
				}
				compressionMethod = value; 
			}
		}

		public X509CertificateCollection Certificates
		{
			get { return certificates; }
			set { certificates = value; }
		}

		#endregion

		#region CONSTRUCTORS

		public TlsSessionSettings()
		{
			this.protocol			= TlsProtocol.Tls1;
			this.compressionMethod	= TlsCompressionMethod.None;
			this.certificates		= new X509CertificateCollection();
			this.serverName			= "localhost";
			this.serverPort			= 443;
			this.encoding			= Encoding.Default;
		}

		public TlsSessionSettings(TlsProtocol protocol) : this()
		{
			this.Protocol	= protocol;
		}

		public TlsSessionSettings(TlsProtocol protocol, Encoding encoding) : this(protocol)
		{
			this.encoding	= encoding;
		}

		public TlsSessionSettings(string serverName) : this()
		{
			this.serverName	= serverName;
		}

		public TlsSessionSettings(string serverName, Encoding encoding) : this()
		{
			this.serverName	= serverName;
			this.encoding	= encoding;
		}

		public TlsSessionSettings(string serverName, int serverPort) : this()
		{
			this.serverName	= serverName;
			this.serverPort	= serverPort;
		}

		public TlsSessionSettings(string serverName, int serverPort, Encoding encoding) : this()
		{
			this.serverName	= serverName;
			this.serverPort	= serverPort;
			this.encoding	= encoding;
		}

		public TlsSessionSettings(TlsProtocol protocol, string serverName) : this(protocol)
		{
			this.serverName	= serverName;
		}

		public TlsSessionSettings(TlsProtocol protocol, string serverName, Encoding encoding) : this(protocol)
		{
			this.serverName	= serverName;
			this.encoding	= encoding;
		}


		public TlsSessionSettings(TlsProtocol protocol, string serverName, int serverPort) : this(protocol)
		{
			this.serverName	= serverName;
			this.serverPort	= serverPort;
		}

		public TlsSessionSettings(TlsProtocol protocol, string serverName, int serverPort, Encoding encoding) : this(protocol)
		{
			this.serverName	= serverName;
			this.serverPort	= serverPort;
			this.encoding	= encoding;
		}

		public TlsSessionSettings(TlsProtocol protocol, X509CertificateCollection certificates) : this(protocol)
		{
			this.certificates	= certificates;
		}

		public TlsSessionSettings(TlsProtocol protocol, X509CertificateCollection certificates, Encoding encoding) : this(protocol)
		{
			this.certificates	= certificates;
			this.encoding		= encoding;
		}

		public TlsSessionSettings(TlsProtocol protocol, X509CertificateCollection certificates, string serverName, int serverPort) : this(protocol)
		{
			this.certificates	= certificates;
			this.serverName		= serverName;
			this.serverPort		= serverPort;
		}

		public TlsSessionSettings(TlsProtocol protocol, X509CertificateCollection certificates, string serverName, int serverPort, Encoding encoding) : this(protocol)
		{
			this.certificates	= certificates;
			this.serverName		= serverName;
			this.serverPort		= serverPort;
			this.encoding		= encoding;
		}

		public TlsSessionSettings(TlsProtocol protocol, X509Certificate[] certificates) 
			: this(protocol, new X509CertificateCollection(certificates))
		{
		}

		public TlsSessionSettings(TlsProtocol protocol, X509Certificate[] certificates, Encoding encoding) 
			: this(protocol, new X509CertificateCollection(certificates), encoding)
		{
		}

		public TlsSessionSettings(TlsProtocol protocol, X509Certificate[] certificates, string serverName, int serverPort) : 
			this(protocol, new X509CertificateCollection(certificates), serverName, serverPort)
		{
		}

		public TlsSessionSettings(TlsProtocol protocol, X509Certificate[] certificates, string serverName, int serverPort, Encoding encoding) : 
			this(protocol, new X509CertificateCollection(certificates), serverName, serverPort, encoding)
		{
		}

		#endregion
	}
}
