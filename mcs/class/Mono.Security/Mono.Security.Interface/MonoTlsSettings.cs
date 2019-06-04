//
// MonoTlsSettings.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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
using System;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Interface
{
	public sealed class MonoTlsSettings
	{
		public MonoRemoteCertificateValidationCallback RemoteCertificateValidationCallback {
			get; set;
		}

		public MonoLocalCertificateSelectionCallback ClientCertificateSelectionCallback {
			get; set;
		}

		public bool CheckCertificateName {
			get { return checkCertName; }
			set { checkCertName = value; }
		}

		public bool CheckCertificateRevocationStatus {
			get { return checkCertRevocationStatus; }
			set { checkCertRevocationStatus = value; }
		}

		public bool? UseServicePointManagerCallback {
			get { return useServicePointManagerCallback; }
			set { useServicePointManagerCallback = value; }
		}

		public bool SkipSystemValidators {
			get { return skipSystemValidators; }
			set { skipSystemValidators = value; }
		}

		public bool CallbackNeedsCertificateChain {
			get { return callbackNeedsChain; }
			set { callbackNeedsChain = value; }
		}

		/*
		 * Use custom time for certificate expiration checks
		 */
		public DateTime? CertificateValidationTime {
			get; set;
		}

		/*
		 * This is only supported if CertificateValidationHelper.SupportsTrustAnchors is true.
		 */
		public X509CertificateCollection TrustAnchors {
			get; set;
		}

		public object UserSettings {
			get; set;
		}

		internal string[] CertificateSearchPaths {
			get; set;
		}

		/*
		 * This is only supported if MonoTlsProvider.SupportsCleanShutdown is true.
		 */
		internal bool SendCloseNotify {
			get; set;
		}

		/*
		 * Client Certificate Support.
		 */
		public string[] ClientCertificateIssuers {
			get; set;
		}

		public bool DisallowUnauthenticatedCertificateRequest {
			get; set;
		}

		/*
		 * If you set this here, then it will override 'ServicePointManager.SecurityProtocol'.
		 */
		public TlsProtocols? EnabledProtocols {
			get; set;
		}

		[CLSCompliant (false)]
		public CipherSuiteCode[] EnabledCiphers {
			get; set;
		}

		bool cloned = false;
		bool checkCertName = true;
		bool checkCertRevocationStatus = false;
		bool? useServicePointManagerCallback = null;
		bool skipSystemValidators = false;
		bool callbackNeedsChain = true;
		ICertificateValidator certificateValidator;

		public MonoTlsSettings ()
		{
		}

		static MonoTlsSettings defaultSettings;

		public static MonoTlsSettings DefaultSettings {
			get {
				if (defaultSettings == null)
					Interlocked.CompareExchange (ref defaultSettings, new MonoTlsSettings (), null);
				return defaultSettings;
			}
			set {
				defaultSettings = value ?? new MonoTlsSettings ();
			}
		}

		public static MonoTlsSettings CopyDefaultSettings ()
		{
			return DefaultSettings.Clone ();
		}

		#region Private APIs

		/*
		 * Private APIs - do not use!
		 * 
		 * This is only public to avoid making our internals visible to System.dll.
		 * 
		 */

		[Obsolete ("Do not use outside System.dll!")]
		public ICertificateValidator CertificateValidator {
			get { return certificateValidator; }
		}

		[Obsolete ("Do not use outside System.dll!")]
		public MonoTlsSettings CloneWithValidator (ICertificateValidator validator)
		{
			if (cloned) {
				this.certificateValidator = validator;
				return this;
			}

			var copy = new MonoTlsSettings (this);
			copy.certificateValidator = validator;
			return copy;
		}

		public MonoTlsSettings Clone ()
		{
			return new MonoTlsSettings (this);
		}

		MonoTlsSettings (MonoTlsSettings other)
		{
			RemoteCertificateValidationCallback = other.RemoteCertificateValidationCallback;
			ClientCertificateSelectionCallback = other.ClientCertificateSelectionCallback;
			checkCertName = other.checkCertName;
			checkCertRevocationStatus = other.checkCertRevocationStatus;
			UseServicePointManagerCallback = other.useServicePointManagerCallback;
			skipSystemValidators = other.skipSystemValidators;
			callbackNeedsChain = other.callbackNeedsChain;
			UserSettings = other.UserSettings;
			EnabledProtocols = other.EnabledProtocols;
			EnabledCiphers = other.EnabledCiphers;
			CertificateValidationTime = other.CertificateValidationTime;
			SendCloseNotify = other.SendCloseNotify;
			ClientCertificateIssuers = other.ClientCertificateIssuers;
			DisallowUnauthenticatedCertificateRequest = other.DisallowUnauthenticatedCertificateRequest;
			if (other.TrustAnchors != null)
				TrustAnchors = new X509CertificateCollection (other.TrustAnchors);
			if (other.CertificateSearchPaths != null) {
				CertificateSearchPaths = new string [other.CertificateSearchPaths.Length];
				other.CertificateSearchPaths.CopyTo (CertificateSearchPaths, 0);
			}

			cloned = true;
		}

		#endregion
	}
}

