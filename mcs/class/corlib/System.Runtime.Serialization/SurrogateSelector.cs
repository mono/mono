//
// System.Runtime.Serialization.SurrogateSelector.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;

namespace System.Runtime.Serialization
{
	public class SurrogateSelector : ISurrogateSelector
	{
		// Fields
		Hashtable Surrogates = new Hashtable ();
		string currentKey = null; // current key of Surrogates

		internal struct Bundle
		{
			public ISerializationSurrogate surrogate;
			public ArrayList selectors;

			public Bundle (ISerializationSurrogate surrogate)
			{
				this.surrogate = surrogate;
				selectors = new ArrayList ();
			}
		}
		
		// Constructor
		public SurrogateSelector()
			: base ()
		{
		}

		// Methods
		public virtual void AddSurrogate (Type type,
			  StreamingContext context, ISerializationSurrogate surrogate)
		{
			if (type == null || surrogate == null)
				throw new ArgumentNullException ("Null reference.");

			currentKey = type.FullName + "#" + context.ToString ();

			if (Surrogates.ContainsKey (currentKey))
				throw new ArgumentException ("A surrogate for " + type.FullName + " already exists.");

			Bundle values = new Bundle (surrogate);
			
			Surrogates.Add (currentKey, values);
		}

		public virtual void ChainSelector (ISurrogateSelector selector)
		{
			if (selector == null)
				throw new ArgumentNullException ("Selector is null.");
			
			Bundle current = (Bundle) Surrogates [currentKey];
			current.selectors.Add (selector);
		}

		public virtual ISurrogateSelector GetNextSelector ()
		{
			Bundle current = (Bundle) Surrogates [currentKey];
			return (ISurrogateSelector) current.selectors [current.selectors.Count];
		}

		public virtual ISerializationSurrogate GetSurrogate (Type type,
			     StreamingContext context, out ISurrogateSelector selector)
		{
			if (type == null)
				throw new ArgumentNullException ("type is null.");
			
			string key = type.FullName + "#" + context.ToString ();			
			Bundle current = (Bundle) Surrogates [key];
			selector = (ISurrogateSelector) current.selectors [current.selectors.Count - 1];
			
			return (ISerializationSurrogate) current.surrogate;
		}

		public virtual void RemoveSurrogate (Type type, StreamingContext context)
		{
			if (type == null)
				throw new ArgumentNullException ("type is null.");

			string key = type.FullName + "#" + context.ToString ();
			Surrogates.Remove (key);
		}
	}
}
