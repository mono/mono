// DelegateSerializationHolder.cs
//
// Author:
//  Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2003 Lluis Sanchez Gual

using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class DelegateSerializationHolder: ISerializable, IObjectReference
	{
		Delegate _delegate;	// The deserialized delegate

		[Serializable]
		class DelegateEntry
		{
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

			public Delegate DeserializeDelegate(SerializationInfo info)
			{
				object realTarget = null;
				if (target != null)
					realTarget = info.GetValue (target.ToString(), typeof(object));

				Assembly dasm = Assembly.Load (assembly);
				Type dt = dasm.GetType (type);

				Delegate del;
				if (realTarget != null)
					del = Delegate.CreateDelegate (dt, realTarget, methodName);
				else
				{
					Assembly tasm = Assembly.Load (targetTypeAssembly);
					Type tt = tasm.GetType (targetTypeName);
					del = Delegate.CreateDelegate (dt, tt, methodName);
				}

				if (!del.Method.IsPublic)
					throw new SerializationException ("Serialization will not deserialize delegates to non-public methods.");

				return del;
			}

			string type;
			string assembly;
			public object target;
			string targetTypeAssembly;
			string targetTypeName;
			string methodName;
			public DelegateEntry delegateEntry;	// next delegate in the invocation list
		}

		DelegateSerializationHolder(SerializationInfo info, StreamingContext ctx)
		{
			DelegateEntry entryChain = (DelegateEntry)info.GetValue ("Delegate", typeof(DelegateEntry));

			// Count the number of delegates to combine

			int count = 0;
			DelegateEntry entry = entryChain;
			while (entry != null)
			{
				entry = entry.delegateEntry;
				count++;
			}

			// Deserializes and combines the delegates

			if (count == 1) 
				_delegate = entryChain.DeserializeDelegate (info);
			else
			{
				Delegate[] delegates = new Delegate[count];
				entry = entryChain;
				for (int n=0; n<count; n++)
				{
					delegates[n] = entry.DeserializeDelegate (info);
					entry = entry.delegateEntry;
				}

				_delegate = Delegate.Combine (delegates);
			}
		}

		public static void GetDelegateData(Delegate instance, SerializationInfo info, StreamingContext ctx)
		{
			// Fills a SerializationInfo object with the information of the delegate.

			Delegate[] delegates = instance.GetInvocationList();
			DelegateEntry lastEntry = null;
			for (int n=0; n<delegates.Length; n++)
			{
				Delegate del = delegates[n];
				string targetLabel = (del.Target != null) ? ("target" + n) : null;
				DelegateEntry entry = new DelegateEntry(del, targetLabel);

				if (lastEntry == null)
					info.AddValue ("Delegate", entry);
				else
					lastEntry.delegateEntry = entry;

				lastEntry = entry;
				if (del.Target != null)
					info.AddValue (targetLabel, del.Target);
			}
			info.SetType (typeof (DelegateSerializationHolder));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// Not needed.
			throw new NotSupportedException();
		}

		public object GetRealObject(StreamingContext context)
		{
			return _delegate;
		}
	}
}
