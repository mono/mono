//
// SSPIWrapper.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif
#if MONO_X509_ALIAS
extern alias PrebuiltSystem;
#endif

#if MONO_SECURITY_ALIAS
using MX = MonoSecurity::Mono.Security.X509;
using MonoSecurity::Mono.Security.Interface;
#else
using MX = Mono.Security.X509;
using Mono.Security.Interface;
#endif
#if MONO_X509_ALIAS
using XX509CertificateCollection = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509CertificateCollection;
#else
using XX509CertificateCollection = System.Security.Cryptography.X509Certificates.X509CertificateCollection;
#endif

using System.Runtime.InteropServices;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography.X509Certificates;
using MNS = Mono.Net.Security;

namespace System.Net.Security
{
	internal class SSPIInterface
	{
		public IMonoTlsContext Context {
			get;
			private set;
		}

		public IMonoTlsEventSink EventSink {
			get;
			private set;
		}

		public SSPIInterface (IMonoTlsContext context, IMonoTlsEventSink eventSink)
		{
			Context = context;
			EventSink = eventSink;
		}
	}

	internal static class GlobalSSPI
	{
		internal static SSPIInterface Create (string hostname, bool serverMode, SchProtocols protocolFlags, X509Certificate serverCertificate, XX509CertificateCollection clientCertificates,
		                                           bool remoteCertRequired, bool checkCertName, bool checkCertRevocationStatus, EncryptionPolicy encryptionPolicy,
		                                           LocalCertSelectionCallback certSelectionDelegate, RemoteCertValidationCallback remoteValidationCallback, SSPIConfiguration userConfig)
		{
			if (userConfig.Settings != null && remoteValidationCallback != null)
				throw new InvalidOperationException ();
			var context = userConfig.Provider.CreateTlsContext (
				hostname, serverMode, (TlsProtocols)protocolFlags, serverCertificate, clientCertificates,
				remoteCertRequired, checkCertName, checkCertRevocationStatus,
				(MonoEncryptionPolicy)encryptionPolicy, userConfig.Settings);
			return new SSPIInterface (context, userConfig.EventSink);
		}
	}

	/*
	 * SSPIWrapper _is a _class that provides a managed implementation of the equivalent
	 * _class _in Microsofts .NET Framework.   
	 * 
	 * The SSPIWrapper class is used by the TLS/SSL stack to implement both the 
	 * protocol handshake as well as the encryption and decryption of messages.
	 * 
	 * Microsoft's implementation of this class is merely a P/Invoke wrapper
	 * around the native SSPI APIs on Windows.   This implementation instead, 
	 * provides a managed implementation that uses the cross platform Mono.Security 
	 * to provide the equivalent functionality.
	 * 
	 * Ideally, this should be abstracted with a different name, and decouple
	 * the naming from the SSPIWrapper name, but this allows Mono to reuse
	 * the .NET code with minimal changes.
	 * 
	 * The "internal" methods here are the API that is consumed by the class
	 * libraries.
	 */
	internal static class SSPIWrapper
	{
		static void SetCredentials (SSPIInterface secModule, SafeFreeCredentials credentials)
		{
			if (credentials != null && !credentials.IsInvalid) {
				if (!secModule.Context.HasCredentials && credentials.Certificate != null) {
					var cert = new X509Certificate2 (credentials.Certificate.RawData);
					secModule.Context.SetCertificate (cert, credentials.Certificate.PrivateKey);
				}
				bool success = true;
				credentials.DangerousAddRef (ref success);
			}
		}

		/*
		 * @safecontext is null on the first use, but it will become non-null for invocations 
		 * where the connection is being re-negotiated.
		 * 
		*/
		internal static int AcceptSecurityContext (SSPIInterface secModule, ref SafeFreeCredentials credentials, ref SafeDeleteContext safeContext, ContextFlags inFlags, Endianness endianness, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
		{
			if (endianness != Endianness.Native)
				throw new NotSupportedException ();

			if (safeContext == null) {
				if (credentials == null || credentials.IsInvalid)
					return (int)SecurityStatus.CredentialsNeeded;

				secModule.Context.Initialize (secModule.EventSink);
				safeContext = new SafeDeleteContext (secModule.Context);
			}

			SetCredentials (secModule, credentials);

			var incoming = GetInputBuffer (inputBuffer);
			IBufferOffsetSize outgoing;

			var retval = (int)safeContext.Context.GenerateNextToken (incoming, out outgoing);
			UpdateOutput (outgoing, outputBuffer);
			return retval;
		}

		internal static int InitializeSecurityContext (SSPIInterface secModule, ref SafeFreeCredentials credentials, ref SafeDeleteContext safeContext, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
		{
			if (inputBuffer != null)
				throw new InvalidOperationException ();

			if (safeContext == null) {
				secModule.Context.Initialize (secModule.EventSink);
				safeContext = new SafeDeleteContext (secModule.Context);
			}

			return InitializeSecurityContext (secModule, credentials, ref safeContext, targetName, inFlags, endianness, null, outputBuffer, ref outFlags);
		}

		internal static int InitializeSecurityContext (SSPIInterface secModule, SafeFreeCredentials credentials, ref SafeDeleteContext safeContext, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
		{
			if (endianness != Endianness.Native)
				throw new NotSupportedException ();

			SetCredentials (secModule, credentials);

			SecurityBuffer inputBuffer = null;
			if (inputBuffers != null) {
				if (inputBuffers.Length != 2 || inputBuffers [1].type != BufferType.Empty)
					throw new NotSupportedException ();
				inputBuffer = inputBuffers [0];
			}

			var incoming = GetInputBuffer (inputBuffer);
			IBufferOffsetSize outgoing = null;

			var retval = (int)safeContext.Context.GenerateNextToken (incoming, out outgoing);
			UpdateOutput (outgoing, outputBuffer);
			return retval;
		}

		internal static int EncryptMessage (SSPIInterface secModule, SafeDeleteContext safeContext, SecurityBuffer securityBuffer, uint sequenceNumber)
		{
			var incoming = GetInputBuffer (securityBuffer);
			var retval = (int)safeContext.Context.EncryptMessage (ref incoming);
			UpdateOutput (incoming, securityBuffer);
			return retval;
		}

		internal static int DecryptMessage (SSPIInterface secModule, SafeDeleteContext safeContext, SecurityBuffer securityBuffer, uint sequenceNumber)
		{
			var incoming = GetInputBuffer (securityBuffer);
			var retval = (int)safeContext.Context.DecryptMessage (ref incoming);
			UpdateOutput (incoming, securityBuffer);
			return retval;
		}

		internal static byte[] CreateShutdownMessage (SSPIInterface secModule, SafeDeleteContext safeContext)
		{
			return safeContext.Context.CreateCloseNotify ();
		}

		internal static byte[] CreateHelloRequestMessage (SSPIInterface secModule, SafeDeleteContext safeContext)
		{
			return safeContext.Context.CreateHelloRequest ();
		}

		internal static bool IsClosed (SSPIInterface secModule, SafeDeleteContext safeContext)
		{
			return safeContext.Context.ReceivedCloseNotify;
		}

		internal static SafeFreeCredentials AcquireCredentialsHandle (SSPIInterface SecModule, string package, CredentialUse intent, SecureCredential scc)
		{
			return new SafeFreeCredentials (scc);
		}

		public static ChannelBinding QueryContextChannelBinding (SSPIInterface SecModule, SafeDeleteContext securityContext, ContextAttribute contextAttribute)
		{
			return null;
		}

		internal static X509Certificate2 GetRemoteCertificate (SafeDeleteContext safeContext, out X509Certificate2Collection remoteCertificateStore)
		{
			XX509CertificateCollection monoCollection;
			if (safeContext == null || safeContext.IsInvalid) {
				remoteCertificateStore = null;
				return null;
			}
			var monoCert = safeContext.Context.GetRemoteCertificate (out monoCollection);
			if (monoCert == null) {
				remoteCertificateStore = null;
				return null;
			}

			remoteCertificateStore = new X509Certificate2Collection ();
			foreach (var cert in monoCollection) {
				remoteCertificateStore.Add (cert);
			}
			return (X509Certificate2)monoCert;
		}

		internal static bool CheckRemoteCertificate (SafeDeleteContext safeContext)
		{
			return safeContext.Context.VerifyRemoteCertificate ();
		}

		internal static MonoTlsConnectionInfo GetMonoConnectionInfo (SSPIInterface SecModule, SafeDeleteContext securityContext)
		{
			return securityContext.Context.GetConnectionInfo ();
		}

		internal static SslConnectionInfo GetConnectionInfo (SSPIInterface SecModule, SafeDeleteContext securityContext)
		{
			var info = securityContext.Context.GetConnectionInfo ();
			if (info == null)
				return null;

			return new SslConnectionInfo ((int)info.ProtocolVersion);
		}

		class InputBuffer : IBufferOffsetSize
		{
			public byte[] Buffer {
				get;
				private set;
			}

			public int Offset {
				get;
				private set;
			}

			public int Size {
				get;
				private set;
			}

			public InputBuffer (byte[] buffer, int offset, int size)
			{
				Buffer = buffer;
				Offset = offset;
				Size = size;
			}
		}

		static IBufferOffsetSize GetInputBuffer (SecurityBuffer incoming)
		{
			return incoming != null ? new InputBuffer (incoming.token, incoming.offset, incoming.size) : null;
		}

		static void UpdateOutput (IBufferOffsetSize buffer, SecurityBuffer outputBuffer)
		{
			if (buffer != null) {
				outputBuffer.token = buffer.Buffer;
				outputBuffer.offset = buffer.Offset;
				outputBuffer.size = buffer.Size;
				outputBuffer.type = BufferType.Token;
			} else {
				outputBuffer.token = null;
				outputBuffer.size = outputBuffer.offset = 0;
				outputBuffer.type = BufferType.Empty;
			}
		}
	}
}
#endif
