//
// X509Stores.cs: Handles X.509 certificates/CRLs stores group.
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

	public class X509Stores {

		private string _storePath;
		private X509Store _personal;
		private X509Store _other;
		private X509Store _intermediate;
		private X509Store _trusted;
		private X509Store _untrusted;

		internal X509Stores (string path) 
		{
			_storePath = path;
		}

		// properties

		public X509Store Personal {
			get { 
				if (_personal == null) {
					string path = Path.Combine (_storePath, Names.Personal);
					_personal = new X509Store (path, false);
				}
				return _personal; 
			}
		}

		public X509Store OtherPeople {
			get { 
				if (_other == null) {
					string path = Path.Combine (_storePath, Names.OtherPeople);
					_other = new X509Store (path, false);
				}
				return _other; 
			}
		}

		public X509Store IntermediateCA {
			get { 
				if (_intermediate == null) {
					string path = Path.Combine (_storePath, Names.IntermediateCA);
					_intermediate = new X509Store (path, true);
				}
				return _intermediate; 
			}
		}

		public X509Store TrustedRoot {
			get { 
				if (_trusted == null) {
					string path = Path.Combine (_storePath, Names.TrustedRoot);
					_trusted = new X509Store (path, true);
				}
				return _trusted; 
			}
		}

		public X509Store Untrusted {
			get { 
				if (_untrusted == null) {
					string path = Path.Combine (_storePath, Names.Untrusted);
					_untrusted = new X509Store (path, false);
				}
				return _untrusted; 
			}
		}

		// methods

		public void Clear () 
		{
			// this will force a reload of all stores
			if (_personal != null)
				_personal.Clear ();
			_personal = null;
			if (_other != null)
				_other.Clear ();
			_other = null;
			if (_intermediate != null)
				_intermediate.Clear ();
			_intermediate = null;
			if (_trusted != null)
				_trusted.Clear ();
			_trusted = null;
			if (_untrusted != null)
				_untrusted.Clear ();
			_untrusted = null;
		}

		// names

		public class Names {

			// do not translate
			public const string Personal = "My";
			public const string OtherPeople = "AddressBook";
			public const string IntermediateCA = "CA";
			public const string TrustedRoot = "Trust";
			public const string Untrusted = "Disallowed";
			
			public Names () {}
		}
	}
}
