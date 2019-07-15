//
// System.DelegateSerializationHolder.cs
//
// Author:
//   Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2003 Lluis Sanchez Gual
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

using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Remoting;

namespace System
{
	[Serializable]
	internal class DelegateSerializationHolder: ISerializable, IObjectReference
	{
		Delegate _delegate; // The deserialized delegate

		[Serializable]
		class DelegateEntry
		{
			string type;
			string assembly;
			object target;
			string targetTypeAssembly;
			string targetTypeName;
			string methodName;
			public DelegateEntry delegateEntry; // next delegate in the invocation list

			// A DelegateEntry holds information about a delegate that is part
			// of an invocation list of a multicast delegate.
			public DelegateEntry (Delegate del, string targetLabel)
			{
				type = del.GetType().FullName;
				assembly = del.GetType().Assembly.FullName;
				target = targetLabel;
				targetTypeAssembly = del.Method.DeclaringType.Assembly.FullName;
				targetTypeName = del.Method.DeclaringType.FullName;
				methodName = del.Method.Name;
			}

			public Delegate DeserializeDelegate (SerializationInfo info, int index)
			{
				object realTarget = null;
				if (target != null)
					realTarget = info.GetValue (target.ToString(), typeof(object));

				var key = "method" + index;
				var method = (MethodInfo)info.GetValueNoThrow (key, typeof(MethodInfo));

				Assembly dasm = Assembly.Load (assembly);
				Type dt = dasm.GetType (type);

				if (realTarget != null) {
#if FEATURE_REMOTING
					if (RemotingServices.IsTransparentProxy (realTarget)) {
						// The call to IsInstanceOfType will force the proxy
						// to load the real type of the remote object. This is
						// needed to make sure that subsequent calls to
						// GetType() return the expected type.
						Assembly tasm = Assembly.Load (targetTypeAssembly);
						Type tt = tasm.GetType (targetTypeName);
						if (!tt.IsInstanceOfType (realTarget))
							throw new RemotingException ("Unexpected proxy type.");
					}
#endif
					return method == null ?
						Delegate.CreateDelegate (dt, realTarget, methodName) :
						Delegate.CreateDelegate (dt, realTarget, method);
				}

				if (method != null)
					return Delegate.CreateDelegate (dt, realTarget, method);

				Type tt2 = Assembly.Load (targetTypeAssembly).GetType (targetTypeName);
				return Delegate.CreateDelegate (dt, tt2, methodName);
			}
		}

		DelegateSerializationHolder(SerializationInfo info, StreamingContext ctx)
		{
			DelegateEntry entryChain = (DelegateEntry)info.GetValue ("Delegate", typeof(DelegateEntry));

			// Count the number of delegates to combine
			int count = 0;
			DelegateEntry entry = entryChain;
			while (entry != null) {
				entry = entry.delegateEntry;
				count++;
			}

			// Deserializes and combines the delegates
			if (count == 1) 
				_delegate = entryChain.DeserializeDelegate (info, 0);
			else
			{
				Delegate[] delegates = new Delegate[count];
				entry = entryChain;
				for (int n=0; n<count; n++)
				{
					delegates[n] = entry.DeserializeDelegate (info, n);
					entry = entry.delegateEntry;
				}
				_delegate = Delegate.Combine (delegates);
			}
		}

		public static void GetDelegateData (Delegate instance, SerializationInfo info, StreamingContext ctx)
		{
			// Fills a SerializationInfo object with the information of the delegate.

			Delegate[] delegates = instance.GetInvocationList ();
			DelegateEntry lastEntry = null;
			for (int n=0; n<delegates.Length; n++) {
				Delegate del = delegates[n];
				string targetLabel = (del.Target != null) ? ("target" + n) : null;
				DelegateEntry entry = new DelegateEntry (del, targetLabel);

				if (lastEntry == null)
					info.AddValue ("Delegate", entry);
				else
					lastEntry.delegateEntry = entry;

				lastEntry = entry;
				if (del.Target != null)
					info.AddValue (targetLabel, del.Target);

				info.AddValue ("method" + n, del.Method);
			}
			info.SetType (typeof (DelegateSerializationHolder));
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// Not needed.
			throw new NotSupportedException ();
		}

		public object GetRealObject (StreamingContext context)
		{
			return _delegate;
		}
	}
}
