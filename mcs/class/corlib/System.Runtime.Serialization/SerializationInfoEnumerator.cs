//
// System.Runtime.Serialization.SerializationInfoEnumerator.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization
{
	public sealed class SerializationInfoEnumerator : IEnumerator
	{
		IDictionaryEnumerator ide;

		// Constructor
		internal SerializationInfoEnumerator (Hashtable collection)
		{
			ide = collection.GetEnumerator ();
		}
		
		// Properties
		public SerializationEntry Current
		{
		     get { return (SerializationEntry) ide.Value; }
		}

		object IEnumerator.Current
		{			
		     get { return ide.Value; }
		}

		public string Name
		{
			get { return this.Current.Name; }
		}

		public Type ObjectType
		{
			get  { return this.Current.ObjectType; }
		}

		public object Value
		{			
			get { return this.Current.Value; }
		}

		// Methods
		public bool MoveNext ()
		{
			return ide.MoveNext ();
		}

		public void Reset ()
		{
			ide.Reset ();
		}
	}
	
}
