// System.Security.Policy.Evidence
//
// Authors:
//  Sean MacIsaac (macisaac@ximian.com)
//  Nick Drochak (ndrochak@gol.com)
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2001 Ximian, Inc.

using System;
using System.Collections;

namespace System.Security.Policy {

	[MonoTODO]
	public sealed class Evidence : ICollection, IEnumerable {
	
		private ArrayList hostEvidenceList = new ArrayList ();	
		private ArrayList assemblyEvidenceList = new ArrayList ();
		
		public Evidence () 
		{
		}

		public Evidence (Evidence evidence) 
		{
			Merge (evidence);	
		}

		public Evidence (object[] hostEvidence, object[] assemblyEvidence ) 
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
		
		public bool IsSynchronized {
			get { return false; }
		}

		[MonoTODO]
		public bool Locked {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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

		[MonoTODO("If Locked is true and the code that calls this method does not have SecurityPermissionFlag.ControlEvidence a SecurityException should be thrown")]
		public void AddHost (object id) 
		{
			hostEvidenceList.Add (id);
		}

		public void CopyTo (Array array, int index) 
		{
			if (hostEvidenceList.Count > 0) 
				hostEvidenceList.CopyTo (array,index);
			if (assemblyEvidenceList.Count > 0) 
				assemblyEvidenceList.CopyTo (array,index + hostEvidenceList.Count);
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

