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
		IEnumerator enumerator;

		// Constructor
		internal SerializationInfoEnumerator (ArrayList list)
		{
			this.enumerator = list.GetEnumerator ();
		}
		
		// Properties
		public SerializationEntry Current
		{
			get { return (SerializationEntry) enumerator.Current; }
		}

		object IEnumerator.Current
		{			
			get { return enumerator.Current; }
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
			return enumerator.MoveNext ();
		}

		public void Reset ()
		{
			enumerator.Reset ();
		}
	}	
}
