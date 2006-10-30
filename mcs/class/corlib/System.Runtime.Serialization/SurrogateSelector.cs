//
// System.Runtime.Serialization.SurrogateSelector.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez (lsg@ctv.es)
//
// (C) Ximian, Inc.
//

//
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

namespace System.Runtime.Serialization
{
#if NET_2_0
	[System.Runtime.InteropServices.ComVisibleAttribute (true)]
#endif
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
