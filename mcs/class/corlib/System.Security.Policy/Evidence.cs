//
// System.Security.Policy.Evidence
//
// Authors:
//	Sean MacIsaac (macisaac@ximian.com)
//	Nick Drochak (ndrochak@gol.com)
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2001 Ximian, Inc.
// Portions (C) 2003, 2004 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
	public sealed class Evidence : ICollection, IEnumerable {
	
		private bool _locked;
		private ArrayList hostEvidenceList;	
		private ArrayList assemblyEvidenceList;
		
		public Evidence () 
		{
			hostEvidenceList = ArrayList.Synchronized (new ArrayList ());
			assemblyEvidenceList = ArrayList.Synchronized (new ArrayList ());
		}

		public Evidence (Evidence evidence) : this ()
		{
			if (evidence != null)
				Merge (evidence);	
		}

		public Evidence (object[] hostEvidence, object[] assemblyEvidence) : this ()
		{
			if (null != hostEvidence)
				hostEvidenceList.AddRange (hostEvidence);
			if (null != assemblyEvidence)
				assemblyEvidenceList.AddRange (assemblyEvidence);
		}
		
		//
		// Public Properties
		//
	
		public int Count {
			get {
				return (hostEvidenceList.Count + assemblyEvidenceList.Count);
			}
		}

		public bool IsReadOnly {
			get{ return false; }
		}
		
		// LAMESPEC: Always TRUE (not FALSE)
		public bool IsSynchronized {
			get { return true; }
		}

		public bool Locked {
			get { return _locked; }
			set { 
				new SecurityPermission (SecurityPermissionFlag.ControlEvidence).Demand ();
				_locked = value; 
			}
		}	

		public object SyncRoot {
			get { return this; }
		}

		//
		// Public Methods
		//

		public void AddAssembly (object id) 
		{
			assemblyEvidenceList.Add (id);
		}

		public void AddHost (object id) 
		{
			if (_locked) {
				new SecurityPermission (SecurityPermissionFlag.ControlEvidence).Demand ();
			}
			hostEvidenceList.Add (id);
		}

		public void CopyTo (Array array, int index) 
		{
			if (hostEvidenceList.Count > 0) 
				hostEvidenceList.CopyTo (array, index);
			if (assemblyEvidenceList.Count > 0) 
				assemblyEvidenceList.CopyTo (array, index + hostEvidenceList.Count);
		}

		public IEnumerator GetEnumerator () 
		{
			return new EvidenceEnumerator (hostEvidenceList.GetEnumerator (), 
				assemblyEvidenceList.GetEnumerator ());
		}

		public IEnumerator GetAssemblyEnumerator () 
		{
			return assemblyEvidenceList.GetEnumerator ();
		}

		public IEnumerator GetHostEnumerator () 
		{
			return hostEvidenceList.GetEnumerator ();
		}

		public void Merge (Evidence evidence) 
		{
			IEnumerator hostenum, assemblyenum;
			
			hostenum = evidence.GetHostEnumerator ();
			while( hostenum.MoveNext () ) {
				AddHost (hostenum.Current);
			}

			assemblyenum = evidence.GetAssemblyEnumerator ();
			while( assemblyenum.MoveNext () ) {
				AddAssembly (assemblyenum.Current);
			}
		}
	
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
				bool ret = currentEnum.MoveNext ();
				
				if ( !ret && hostEnum == currentEnum ) {
					currentEnum = assemblyEnum;
					ret = assemblyEnum.MoveNext ();
				}

				return ret;
			}

			public void Reset () 
			{
				hostEnum.Reset ();
				assemblyEnum.Reset ();
				currentEnum = hostEnum;
			}

			public object Current {
				get {
					return currentEnum.Current;
				}
			}
		}
	}
}

