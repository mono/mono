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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
	[MonoTODO ("Fix serialization compatibility with MS.NET")]
	public sealed class Evidence : ICollection, IEnumerable {
	
		private bool _locked;
		private ArrayList hostEvidenceList;	
		private ArrayList assemblyEvidenceList;
		private int _hashCode;

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
			_hashCode = 0;
		}

		public void AddHost (object id) 
		{
			if (_locked) {
				new SecurityPermission (SecurityPermissionFlag.ControlEvidence).Demand ();
			}
			hostEvidenceList.Add (id);
			_hashCode = 0;
		}

#if NET_2_0
		public void Clear ()
		{
			hostEvidenceList.Clear ();
			assemblyEvidenceList.Clear ();
			_hashCode = 0;
		}
#endif

		public void CopyTo (Array array, int index) 
		{
			if (hostEvidenceList.Count > 0) 
				hostEvidenceList.CopyTo (array, index);
			if (assemblyEvidenceList.Count > 0) 
				assemblyEvidenceList.CopyTo (array, index + hostEvidenceList.Count);
		}

#if NET_2_0
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			Evidence e = (obj as Evidence);
			if (e == null)
				return false;

			if (hostEvidenceList.Count != e.hostEvidenceList.Count)
				return false;
			if (assemblyEvidenceList.Count != e.assemblyEvidenceList.Count)
				return false;

			for (int i = 0; i < hostEvidenceList.Count; i++) {
				bool found = false;
				for (int j = 0; j < e.hostEvidenceList.Count; i++) {
					if (hostEvidenceList [i].Equals (e.hostEvidenceList [j])) {
						found = true;
						break;
					}
				}
				if (!found)
					return false;
			}
			for (int i = 0; i < assemblyEvidenceList.Count; i++) {
				bool found = false;
				for (int j = 0; j < e.assemblyEvidenceList.Count; i++) {
					if (assemblyEvidenceList [i].Equals (e.assemblyEvidenceList [j])) {
						found = true;
						break;
					}
				}
				if (!found)
					return false;
			}
			
			return true;
		}
#endif

		public IEnumerator GetEnumerator () 
		{
			return new EvidenceEnumerator (hostEvidenceList.GetEnumerator (), 
				assemblyEvidenceList.GetEnumerator ());
		}

		public IEnumerator GetAssemblyEnumerator () 
		{
			return assemblyEvidenceList.GetEnumerator ();
		}

#if NET_2_0
		public override int GetHashCode ()
		{
			// kind of long so we cache it
			if (_hashCode == 0) {
				for (int i = 0; i < hostEvidenceList.Count; i++)
					_hashCode ^= hostEvidenceList [i].GetHashCode ();
				for (int i = 0; i < assemblyEvidenceList.Count; i++)
					_hashCode ^= assemblyEvidenceList [i].GetHashCode ();
			}
			return _hashCode;
		}
#endif

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

#if NET_2_0
		public void RemoveType (Type t)
		{
			for (int i = hostEvidenceList.Count; i >= 0; i--) {
				if (hostEvidenceList.GetType () == t) {
					hostEvidenceList.RemoveAt (i);
					_hashCode = 0;
				}
			}
			for (int i = assemblyEvidenceList.Count; i >= 0; i--) {
				if (assemblyEvidenceList.GetType () == t) {
					assemblyEvidenceList.RemoveAt (i);
					_hashCode = 0;
				}
			}
		}
#endif
	
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

