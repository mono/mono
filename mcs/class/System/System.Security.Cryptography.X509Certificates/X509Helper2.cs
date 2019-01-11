//
// X509Helper2.cs
//
// Authors:
//	Martin Baulig  <martin.baulig@xamarin.com>
//
// Copyright (C) 2016 Xamarin, Inc. (http://www.xamarin.com)
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

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MX = MonoSecurity::Mono.Security.X509;
#else
using MX = Mono.Security.X509;
#endif

#if MONO_FEATURE_BTLS
using Mono.Btls;
#endif

using System.IO;
using System.Text;
using Mono;

namespace System.Security.Cryptography.X509Certificates
{
	internal static class X509Helper2
	{
		/*
		 * This is used by X509ChainImplMono
		 * 
		 * Some of the missing APIs such as X509v3 extensions can be added to the native
		 * BTLS implementation.
		 * 
		 * We should also consider replacing X509ChainImplMono with a new X509ChainImplBtls
		 * at some point.
		 */
		[MonoTODO ("Investigate replacement; see comments in source.")]
		internal static MX.X509Certificate GetMonoCertificate (X509Certificate2 certificate)
		{
			if (certificate.Impl is X509Certificate2ImplMono monoImpl)
				return monoImpl.MonoCertificate;
			return new MX.X509Certificate (certificate.RawData);
		}

		internal static X509ChainImpl CreateChainImpl (bool useMachineContext)
		{
			return new X509ChainImplMono (useMachineContext);
		}

		public static bool IsValid (X509ChainImpl impl)
		{
			return impl != null && impl.IsValid;
		}

		internal static void ThrowIfContextInvalid (X509ChainImpl impl)
		{
			if (!IsValid (impl))
				throw GetInvalidChainContextException ();
		}

		internal static Exception GetInvalidChainContextException ()
		{
			return new CryptographicException (Locale.GetText ("Chain instance is empty."));
		}

		[Obsolete ("This is only used by Mono.Security's X509Store and will be replaced shortly.")]
		internal static long GetSubjectNameHash (X509Certificate certificate)
		{
#if MONO_FEATURE_BTLS
			X509Helper.ThrowIfContextInvalid (certificate.Impl);
			using (var x509 = GetNativeInstance (certificate.Impl))
			using (var subject = x509.GetSubjectName ())
				return subject.GetHash ();
#else
			throw new PlatformNotSupportedException ();
#endif
		}

		[Obsolete ("This is only used by Mono.Security's X509Store and will be replaced shortly.")]
		internal static void ExportAsPEM (X509Certificate certificate, Stream stream, bool includeHumanReadableForm)
		{
#if MONO_FEATURE_BTLS
			X509Helper.ThrowIfContextInvalid (certificate.Impl);
			using (var x509 = GetNativeInstance (certificate.Impl))
			using (var bio = MonoBtlsBio.CreateMonoStream (stream))
				x509.ExportAsPEM (bio, includeHumanReadableForm);
#else
			throw new PlatformNotSupportedException ();
#endif
		}

#if MONO_FEATURE_BTLS
		static MonoBtlsX509 GetNativeInstance (X509CertificateImpl impl)
		{
			X509Helper.ThrowIfContextInvalid (impl);
			var btlsImpl = impl as X509CertificateImplBtls;
			if (btlsImpl != null)
				return btlsImpl.X509.Copy ();
			else
				return MonoBtlsX509.LoadFromData (impl.RawData, MonoBtlsX509Format.DER);
		}
#endif
	}
}
