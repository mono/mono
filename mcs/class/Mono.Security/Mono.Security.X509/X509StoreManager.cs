//
// X509StoreManager.cs: X.509 store manager.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;

using Mono.Security.X509.Extensions;

namespace Mono.Security.X509 {

	public class X509StoreManager {

		static private X509Stores _userStore;
		static private X509Stores _machineStore;

		protected X509StoreManager () {}

		static public X509Stores CurrentUser {
			get { 
				if (_userStore == null) {
					string _userPath = Path.Combine (
						Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
						".mono");
					_userPath = Path.Combine (_userPath, "certs");

					_userStore = new X509Stores (_userPath);
				}
				return _userStore;
			}
		}

		static public X509Stores LocalMachine {
			get {
				if (_machineStore == null) {
					// FIXME: where should it be ?
					string _machinePath = Path.Combine (
						Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
						".mono");
					_machinePath = Path.Combine (_machinePath, "machinecerts"); // FIXME

					_machineStore = new X509Stores (_machinePath);
				}
				return _machineStore;
			}
		}

		// Merged stores collections
		// we need to look at both the user and the machine (entreprise)
		// certificates/CRLs when building/validating a chain

		static public X509CertificateCollection IntermediateCACertificates {
			get { 
				X509CertificateCollection intermediateCerts = new X509CertificateCollection ();
				intermediateCerts.AddRange (CurrentUser.IntermediateCA.Certificates);
				intermediateCerts.AddRange (LocalMachine.IntermediateCA.Certificates);
				return intermediateCerts; 
			}
		}

		static public ArrayList IntermediateCACRLs {
			get { 
				ArrayList intermediateCRLs = new ArrayList ();
				intermediateCRLs.AddRange (CurrentUser.IntermediateCA.CRLs);
				intermediateCRLs.AddRange (LocalMachine.IntermediateCA.CRLs);
				return intermediateCRLs; 
			}
		}

		static public X509CertificateCollection TrustedRootCertificates {
			get { 
				X509CertificateCollection trustedCerts = new X509CertificateCollection ();
				trustedCerts.AddRange (CurrentUser.TrustedRoot.Certificates);
				trustedCerts.AddRange (LocalMachine.TrustedRoot.Certificates);
				return trustedCerts; 
			}
		}

		static public ArrayList TrustedRootCACRLs {
			get { 
				ArrayList trustedCRLs = new ArrayList ();
				trustedCRLs.AddRange (CurrentUser.TrustedRoot.CRLs);
				trustedCRLs.AddRange (LocalMachine.TrustedRoot.CRLs);
				return trustedCRLs; 
			}
		}

		static public X509CertificateCollection UntrustedCertificates {
			get { 
				X509CertificateCollection untrustedCerts = new X509CertificateCollection ();
				untrustedCerts.AddRange (CurrentUser.Untrusted.Certificates);
				untrustedCerts.AddRange (LocalMachine.Untrusted.Certificates);
				return untrustedCerts; 
			}
		}
	}
}
