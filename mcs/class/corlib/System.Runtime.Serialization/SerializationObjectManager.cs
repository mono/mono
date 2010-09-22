//
// System.Runtime.Serialization.SerializationObjectManager.cs
//
// Author:
//   Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace System.Runtime.Serialization {

	public sealed class SerializationObjectManager
	{
		readonly StreamingContext context;
		readonly Hashtable seen = new Hashtable (HashHelper.Instance, HashHelper.Instance);

		event SerializationCallbacks.CallbackHandler callbacks;

		public SerializationObjectManager (StreamingContext context)
		{
			this.context = context;
		}
		
		public void RegisterObject (object obj)
		{
			if (seen.Contains (obj))
				return;

			SerializationCallbacks sc = SerializationCallbacks
				.GetSerializationCallbacks (obj.GetType ());

			seen [obj] = HashHelper.NonNullObject;
			sc.RaiseOnSerializing (obj, context);

			if (sc.HasSerializedCallbacks) {
				// record for later invocation
				callbacks += delegate (StreamingContext ctx)
				{
					sc.RaiseOnSerialized (obj, ctx);
				};
			}
		}

		public void RaiseOnSerializedEvent ()
		{
			if (callbacks != null)
				callbacks (context);
		}

		class HashHelper : IHashCodeProvider, IComparer {
			public static object NonNullObject = new object ();
			public static HashHelper Instance = new HashHelper ();

			private HashHelper ()
			{
			}

			public int GetHashCode (object obj)
			{
				if (obj == null)
					return 0;
				return Object.InternalGetHashCode (obj);
			}

			public int Compare (object x, object y)
			{
				return Object.ReferenceEquals (x, y) ? 0 : 1;
			}
		}
	}
}

