//
// System.Runtime.Serialization.SurrogateSelector.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez (lsg@ctv.es)
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
		ISurrogateSelector nextSelector = null;

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

			string currentKey = type.FullName + "#" + context.ToString ();

			if (Surrogates.ContainsKey (currentKey))
				throw new ArgumentException ("A surrogate for " + type.FullName + " already exists.");

			Surrogates.Add (currentKey, surrogate);
		}

		public virtual void ChainSelector (ISurrogateSelector selector)
		{
			if (selector == null)
				throw new ArgumentNullException ("Selector is null.");

			// Chain the selector at the beggining of the chain
			// since "The last selector added to the list will be the first one checked"
			// (from MS docs)

			if (nextSelector != null)
				selector.ChainSelector (nextSelector);

			nextSelector = selector;
		}

		public virtual ISurrogateSelector GetNextSelector ()
		{
			return nextSelector;
		}

		public virtual ISerializationSurrogate GetSurrogate (Type type,
			     StreamingContext context, out ISurrogateSelector selector)
		{
			if (type == null)
				throw new ArgumentNullException ("type is null.");

			// Check this selector, and if the surrogate is not found,
			// check the chained selectors
			
			string key = type.FullName + "#" + context.ToString ();			
			ISerializationSurrogate surrogate = (ISerializationSurrogate) Surrogates [key];

			if (surrogate != null) {
				selector = this;
				return surrogate;
			}
			
			if (nextSelector != null)
				return nextSelector.GetSurrogate (type, context, out selector);
			else {
				selector = null;
				return null;
			}
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
