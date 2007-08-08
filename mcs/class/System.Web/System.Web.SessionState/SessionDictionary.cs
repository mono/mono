//
// System.Web.SessionState.SessionDictionary
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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

using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace System.Web.SessionState {

internal class SessionDictionary : NameObjectCollectionBase
{
	object this_lock = new object ();
	
	public SessionDictionary ()
	{
	}

	internal SessionDictionary Clone ()
	{
		SessionDictionary sess = new SessionDictionary ();
		int last = Count;
		for (int i = 0; i < last; i++) {
			string key = GetKey (i);
			sess [key] = this [key];
		}

		return sess;
	}
	
	internal void Clear ()
	{
		lock (this_lock)
			BaseClear ();
	}

	internal string GetKey (int index)
	{
		string value;
		lock (this_lock)
			value = BaseGetKey (index);
			
		return value;
	}

	internal void Remove (string s)
	{
		lock (this_lock)
			BaseRemove (s);
	}

	internal void RemoveAt (int index)
	{
		lock (this_lock)
			BaseRemoveAt (index);
	}

	internal void Serialize (BinaryWriter writer)
	{
		writer.Write (Count);
		foreach (string key in base.Keys) {
			writer.Write (key);
			System.Web.Util.AltSerialization.Serialize (writer, BaseGet (key));
		}
	}

	internal static SessionDictionary Deserialize (BinaryReader r)
	{
		SessionDictionary result = new SessionDictionary ();
		for (int i = r.ReadInt32(); i > 0; i--)
			result [r.ReadString ()] =
				System.Web.Util.AltSerialization.Deserialize (r);

		return result;
	}

	internal object this [string s]
	{
		get {
			object o;
			lock (this_lock)
				o = BaseGet (s);

			return o;
		}

		set {
			lock (this_lock)
			{				 
				object obj = BaseGet(s);
				if ((obj == null) && (value == null))
					return; 
				BaseSet (s, value);
			}
		}
	}

	public object this [int index]
	{
		get {
			object o;
			lock (this_lock)
				o = BaseGet (index);

			return o;
		}
		set {
			lock (this_lock)
			{
				object obj = BaseGet(index);
				if ((obj == null) && (value == null))
					return;
				BaseSet (index, value);
			}
		}
	}

	internal byte [] ToByteArray ()
	{
		MemoryStream stream = null;
		try {
			stream = new MemoryStream ();
			Serialize (new BinaryWriter (stream));
			return stream.GetBuffer ();
		} catch {
			throw;
		} finally {
			if (stream != null)
				stream.Close ();
		}
	}

	internal static SessionDictionary FromByteArray (byte [] data)
	{
		SessionDictionary result = null;
		MemoryStream stream = null;
		try {
			stream = new MemoryStream (data);
			result = Deserialize (new BinaryReader (stream));
		} catch {
			throw;
		} finally {
			if (stream != null)
				stream.Close ();
		}
		return result;
	}
}

}

