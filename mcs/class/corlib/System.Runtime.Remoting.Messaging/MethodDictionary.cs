//
// System.Runtime.Remoting.Messaging.MethodDictionary.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// 2003 (C) Lluis Sanchez Gual
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

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	internal class MethodDictionary : IDictionary
	{
		IDictionary _internalProperties = null;
		protected IMethodMessage _message;
		string[] _methodKeys;
		bool _ownProperties = false;

		public MethodDictionary (IMethodMessage message)
		{
			_message = message;
		}

		internal bool HasInternalProperties 
		{
			get 
			{
				if (null != _internalProperties)
				{
					// MethodCallMessageWrapper uses a nested MethodDictionary
					if (_internalProperties is MethodDictionary)
						return ((MethodDictionary)_internalProperties).HasInternalProperties;
					else 
						return _internalProperties.Count > 0;
				}
				return false;
			}
		}

		internal IDictionary InternalProperties
		{
			get 
			{
				if (null != _internalProperties)
				{
					if (_internalProperties is MethodDictionary)
						return ((MethodDictionary)_internalProperties).InternalProperties;
				}
				return _internalProperties;
			}
		}

		public string[] MethodKeys
		{
			get { return _methodKeys; }
			set { _methodKeys = value; }
		}

		protected virtual IDictionary AllocInternalProperties()
		{
			_ownProperties = true;
			return new Hashtable();
		}

		public IDictionary GetInternalProperties()
		{
			if (_internalProperties == null) _internalProperties = AllocInternalProperties();
			return _internalProperties;
		}

		private bool IsOverridenKey (string key)
		{
			// Small optimization. If the internal properties have been
			// created by this dictionary, then it can be assured that it does
			// not contain values for overriden keys.
			if (_ownProperties) return false;

			foreach (string mkey in _methodKeys)
				if (key == mkey) return true;
			return false;
		}

		public MethodDictionary(string[] keys)
		{
			_methodKeys = keys;
		}

		public bool IsFixedSize 
		{ 
			get { return false; } 
		}
		
		public bool IsReadOnly 
		{ 
			get { return false; } 
		}

		public object this[object key] 
		{ 
			get
			{
				string keyStr = (string)key;
				for (int n=0; n<_methodKeys.Length; n++)
					if (_methodKeys[n] == keyStr) return GetMethodProperty (keyStr);

				if (_internalProperties != null) 
					return _internalProperties[key];
				else 
					return null;
			}

			set
			{
				Add (key, value);
			}
		}

		protected virtual object GetMethodProperty (string key)
		{
			switch (key)
			{
				case "__Uri" : return _message.Uri;
				case "__MethodName" : return _message.MethodName;
				case "__TypeName" : return _message.TypeName;
				case "__MethodSignature" : return _message.MethodSignature;
				case "__CallContext" : return _message.LogicalCallContext;
				case "__Args" : return _message.Args;
				case "__OutArgs": return ((IMethodReturnMessage)_message).OutArgs;
				case "__Return": return ((IMethodReturnMessage)_message).ReturnValue;
				default : return null;
			}
		}

		protected virtual void SetMethodProperty (string key, object value)
		{
			switch (key)
			{
				case "__CallContext": // Ignore?
				case "__OutArgs":
				case "__Return": return;

				case "__MethodName" : 
				case "__TypeName" : 
				case "__MethodSignature" : 
				case "__Args" : throw new ArgumentException ("key was invalid");
				case "__Uri": ((IInternalMessage)_message).Uri = (string) value; return;
			}
		}

		public ICollection Keys 
		{ 
			get 
			{ 
				ArrayList keys = new ArrayList();
				for (int n=0; n<_methodKeys.Length; n++)
					keys.Add (_methodKeys[n]);

				if (_internalProperties != null)
				{
					foreach (string key in _internalProperties.Keys)
						if (!IsOverridenKey (key)) keys.Add (key);
				}

				return keys; 
			}
		}

		public ICollection Values 
		{ 
			get 
			{ 
				ArrayList values = new ArrayList();
				for (int n=0; n<_methodKeys.Length; n++)
					values.Add (GetMethodProperty(_methodKeys[n]));

				if (_internalProperties != null)
				{
					foreach (DictionaryEntry entry in _internalProperties)
						if (!IsOverridenKey((string)entry.Key)) values.Add (entry.Value);
				}

				return values; 
			}
		}

		public void Add (object key, object value)
		{
			string keyStr = (string)key;
			for (int n=0; n<_methodKeys.Length; n++)
				if (_methodKeys[n] == keyStr) {
					SetMethodProperty (keyStr, value);
					return;
				}

			if (_internalProperties == null) _internalProperties = AllocInternalProperties();
			_internalProperties[key] = value;
		}

		public void Clear ()
		{
			if (_internalProperties != null) _internalProperties.Clear();
		}

		public bool Contains (object key)
		{
			string keyStr = (string)key;
			for (int n=0; n<_methodKeys.Length; n++)
				if (_methodKeys[n] == keyStr) return true;

			if (_internalProperties != null) return _internalProperties.Contains (key);
			else return false;
		}

		public void Remove (object key)
		{
			string keyStr = (string)key;
			for (int n=0; n<_methodKeys.Length; n++)
				if (_methodKeys[n] == keyStr) throw new ArgumentException ("key was invalid");

			if (_internalProperties != null) _internalProperties.Remove (key);
		}

		public int Count 
		{ 
			get 
			{
				if (_internalProperties != null) return _internalProperties.Count + _methodKeys.Length;
				else return _methodKeys.Length;
			}
		}

		public bool IsSynchronized 
		{ 
			get { return false; }
		}

		public object SyncRoot 
		{ 
			get { return this; }
		}

		public void CopyTo (Array array, int index)
		{
			Values.CopyTo (array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new DictionaryEnumerator (this);
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return new DictionaryEnumerator (this);
		}

		// Dictionary enumerator

		class DictionaryEnumerator : IDictionaryEnumerator
		{
			MethodDictionary _methodDictionary;
			IDictionaryEnumerator _hashtableEnum;
			int _posMethod;

			public DictionaryEnumerator (MethodDictionary methodDictionary)
			{
				_methodDictionary = methodDictionary;
				_hashtableEnum = (_methodDictionary._internalProperties != null) ? _methodDictionary._internalProperties.GetEnumerator() : null;
				_posMethod = -1;
			}

			public object Current 
			{
				get {return Entry.Value; }
			}

			public bool MoveNext()
			{
				if (_posMethod != -2)
				{
					_posMethod++;
					if (_posMethod < _methodDictionary._methodKeys.Length) return true;
					_posMethod = -2;
				}

				if (_hashtableEnum == null) return false;
				
				while (_hashtableEnum.MoveNext())
				{
					if (!_methodDictionary.IsOverridenKey((string)_hashtableEnum.Key)) 
						return true;
				}
				return false;
			}

			public void Reset()
			{
				_posMethod = -1;
				_hashtableEnum.Reset();
			}

			public DictionaryEntry Entry 
			{
				get
				{
					if (_posMethod >= 0) 
						return new DictionaryEntry (_methodDictionary._methodKeys[_posMethod], _methodDictionary.GetMethodProperty(_methodDictionary._methodKeys[_posMethod]));
					else if (_posMethod == -1 || _hashtableEnum == null) 
						throw new InvalidOperationException ("The enumerator is positioned before the first element of the collection or after the last element");
					else
						return _hashtableEnum.Entry;
				}
			}

			public object Key
			{
				get { return Entry.Key; }
			}

			public object Value
			{
				get { return Entry.Value; }
			}
		}

	}
}
