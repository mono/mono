//
// System.Security.Policy.Evidence
//
// Authors:
//	Sean MacIsaac (macisaac@ximian.com)
//	Nick Drochak (ndrochak@gol.com)
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Ximian, Inc.
// Portions (C) 2003, 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Policy {

	[Serializable]
	[MonoTODO ("Serialization format not compatible with .NET")]
	[ComVisible (true)]
	public sealed class Evidence : ICollection, IEnumerable {
	
		private bool _locked;
		private ArrayList hostEvidenceList;	
		private ArrayList assemblyEvidenceList;

		public Evidence () 
		{
		}

		public Evidence (Evidence evidence)
		{
			if (evidence != null)
				Merge (evidence);	
		}

		public Evidence (EvidenceBase[] hostEvidence, EvidenceBase[] assemblyEvidence)
		{
			if (null != hostEvidence)
				HostEvidenceList.AddRange (hostEvidence);
			if (null != assemblyEvidence)
				AssemblyEvidenceList.AddRange (assemblyEvidence);
		}

		[Obsolete]
		public Evidence (object[] hostEvidence, object[] assemblyEvidence)
		{
			if (null != hostEvidence)
				HostEvidenceList.AddRange (hostEvidence);
			if (null != assemblyEvidence)
				AssemblyEvidenceList.AddRange (assemblyEvidence);
		}
		
		//
		// Public Properties
		//
	
		[Obsolete]
		public int Count {
			get {
				int count = 0;
				if (hostEvidenceList != null)
					count += hostEvidenceList.Count;
				if (assemblyEvidenceList!= null)
					count += assemblyEvidenceList.Count;
				return count;
			}
		}

		public bool IsReadOnly {
			get{ return false; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}

		public bool Locked {
			get { return _locked; }
			[SecurityPermission (SecurityAction.Demand, ControlEvidence = true)]
			set { 
				_locked = value; 
			}
		}	

		public object SyncRoot {
			get { return this; }
		}

		internal ArrayList HostEvidenceList {
			get {
				if (hostEvidenceList == null)
					hostEvidenceList = ArrayList.Synchronized (new ArrayList ());
				return hostEvidenceList;
			}
		}

		internal ArrayList AssemblyEvidenceList {
			get {
				if (assemblyEvidenceList == null)
					assemblyEvidenceList = ArrayList.Synchronized (new ArrayList ());
				return assemblyEvidenceList;
			}
		}

		//
		// Public Methods
		//

		[Obsolete]
		public void AddAssembly (object id) 
		{
			AssemblyEvidenceList.Add (id);
		}

		[Obsolete]
		public void AddHost (object id) 
		{
			if (_locked && SecurityManager.SecurityEnabled) {
				new SecurityPermission (SecurityPermissionFlag.ControlEvidence).Demand ();
			}
			HostEvidenceList.Add (id);
		}

		[ComVisible (false)]
		public void Clear ()
		{
			if (hostEvidenceList != null)
				hostEvidenceList.Clear ();
			if (assemblyEvidenceList != null)
				assemblyEvidenceList.Clear ();
		}

        [ComVisible(false)]
        public Evidence Clone ()
        {
            return new Evidence(this);
        }		

		[Obsolete]
		public void CopyTo (Array array, int index) 
		{
			int hc = 0;
			if (hostEvidenceList != null) {
				hc = hostEvidenceList.Count;
				if (hc > 0)
					hostEvidenceList.CopyTo (array, index);
			}
			if ((assemblyEvidenceList != null) && (assemblyEvidenceList.Count > 0))
				assemblyEvidenceList.CopyTo (array, index + hc);
		}


		[Obsolete]
		public IEnumerator GetEnumerator () 
		{
			IEnumerator he = null;
			if (hostEvidenceList != null)
				he = hostEvidenceList.GetEnumerator ();
			IEnumerator ae = null;
			if (assemblyEvidenceList != null)
				ae = assemblyEvidenceList.GetEnumerator ();
			return new EvidenceEnumerator (he, ae);
		}

		public IEnumerator GetAssemblyEnumerator () 
		{
			return AssemblyEvidenceList.GetEnumerator ();
		}

		public IEnumerator GetHostEnumerator () 
		{
			return HostEvidenceList.GetEnumerator ();
		}

		public void Merge (Evidence evidence) 
		{
			if ((evidence != null) && (evidence.Count > 0)) {
				if (evidence.hostEvidenceList != null) {
					foreach (object o in evidence.hostEvidenceList)
						AddHost (o);
				}
				if (evidence.assemblyEvidenceList != null) {
					foreach (object o in evidence.assemblyEvidenceList)
						AddAssembly (o);
				}
			}
		}

		[ComVisible (false)]
		public void RemoveType (Type t)
		{
			for (int i = hostEvidenceList.Count; i >= 0; i--) {
				if (hostEvidenceList.GetType () == t) {
					hostEvidenceList.RemoveAt (i);
				}
			}
			for (int i = assemblyEvidenceList.Count; i >= 0; i--) {
				if (assemblyEvidenceList.GetType () == t) {
					assemblyEvidenceList.RemoveAt (i);
				}
			}
		}

		// Use an icall to avoid multiple file i/o to detect the 
		// "possible" presence of an Authenticode signature
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern bool IsAuthenticodePresent (Assembly a);
#if MOBILE
		static internal Evidence GetDefaultHostEvidence (Assembly a)
		{
			return new Evidence ();
		}
#else
		// this avoid us to build all evidences from the runtime
		// (i.e. multiple unmanaged->managed calls) and also allows
		// to delay their creation until (if) needed
		[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
		static internal Evidence GetDefaultHostEvidence (Assembly a) 
		{
			Evidence e = new Evidence ();
			string aname = a.EscapedCodeBase;

			// by default all assembly have the Zone, Url and Hash evidences
			e.AddHost (Zone.CreateFromUrl (aname));
			e.AddHost (new Url (aname));
			e.AddHost (new Hash (a));

			// non local files (e.g. http://) also get a Site evidence
			if (String.Compare ("FILE://", 0, aname, 0, 7, true, CultureInfo.InvariantCulture) != 0) {
				e.AddHost (Site.CreateFromUrl (aname));
			}

			// strongnamed assemblies gets a StrongName evidence
			AssemblyName an = a.GetName ();
			byte[] pk = an.GetPublicKey ();
			if ((pk != null) && (pk.Length > 0)) {
				StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (pk);
				e.AddHost (new StrongName (blob, an.Name, an.Version));
			}

			// Authenticode(r) signed assemblies get a Publisher evidence
			if (IsAuthenticodePresent (a)) {
				try {
					X509Certificate x509 = X509Certificate.CreateFromSignedFile (a.Location);
					e.AddHost (new Publisher (x509));
				}
				catch (CryptographicException) {
				}
			}

			// assemblies loaded from the GAC also get a Gac evidence (new in Fx 2.0)
			if (a.GlobalAssemblyCache) {
				e.AddHost (new GacInstalled ());
			}

			// the current HostSecurityManager may add/remove some evidence
			AppDomainManager dommgr = AppDomain.CurrentDomain.DomainManager;
			if (dommgr != null) {
				if ((dommgr.HostSecurityManager.Flags & HostSecurityManagerOptions.HostAssemblyEvidence) ==
					HostSecurityManagerOptions.HostAssemblyEvidence) {
					e = dommgr.HostSecurityManager.ProvideAssemblyEvidence (a, e);
				}
			}

			return e;
		}

#endif // MOBILE

		private class EvidenceEnumerator : IEnumerator {
			
			private IEnumerator currentEnum, hostEnum, assemblyEnum;		
	
			public EvidenceEnumerator (IEnumerator hostenum, IEnumerator assemblyenum) 
			{
				this.hostEnum = hostenum;
				this.assemblyEnum = assemblyenum;
				currentEnum = hostEnum;		
			}

			public bool MoveNext () 
			{
				if (currentEnum == null)
					return false;

				bool ret = currentEnum.MoveNext ();
				
				if (!ret && (hostEnum == currentEnum) && (assemblyEnum != null)) {
					currentEnum = assemblyEnum;
					ret = assemblyEnum.MoveNext ();
				}

				return ret;
			}

			public void Reset () 
			{
				if (hostEnum != null) {
					hostEnum.Reset ();
					currentEnum = hostEnum;
				} else {
					currentEnum = assemblyEnum;
				}
				if (assemblyEnum != null)
					assemblyEnum.Reset ();
			}

			public object Current {
				get {
					return currentEnum.Current;
				}
			}
		}
	}
}

