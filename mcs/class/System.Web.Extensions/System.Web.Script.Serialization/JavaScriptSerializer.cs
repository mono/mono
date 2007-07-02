//
// JavaScriptSerializer.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
//
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
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json.Utilities;
using System.ComponentModel;

namespace System.Web.Script.Serialization
{
	public class JavaScriptSerializer
	{
		internal abstract class LazyDictionary : IDictionary<string, object>
		{
			#region IDictionary<string,object> Members

			void IDictionary<string, object>.Add (string key, object value) {
				throw new NotSupportedException ();
			}

			bool IDictionary<string, object>.ContainsKey (string key) {
				throw new NotSupportedException ();
			}

			ICollection<string> IDictionary<string, object>.Keys {
				get { throw new NotSupportedException (); }
			}

			bool IDictionary<string, object>.Remove (string key) {
				throw new NotSupportedException ();
			}

			bool IDictionary<string, object>.TryGetValue (string key, out object value) {
				throw new NotSupportedException ();
			}

			ICollection<object> IDictionary<string, object>.Values {
				get { throw new NotSupportedException (); }
			}

			object IDictionary<string, object>.this [string key] {
				get {
					throw new NotSupportedException ();
				}
				set {
					throw new NotSupportedException ();
				}
			}

			#endregion

			#region ICollection<KeyValuePair<string,object>> Members

			void ICollection<KeyValuePair<string, object>>.Add (KeyValuePair<string, object> item) {
				throw new NotSupportedException ();
			}

			void ICollection<KeyValuePair<string, object>>.Clear () {
				throw new NotSupportedException ();
			}

			bool ICollection<KeyValuePair<string, object>>.Contains (KeyValuePair<string, object> item) {
				throw new NotSupportedException ();
			}

			void ICollection<KeyValuePair<string, object>>.CopyTo (KeyValuePair<string, object> [] array, int arrayIndex) {
				throw new NotSupportedException ();
			}

			int ICollection<KeyValuePair<string, object>>.Count {
				get { throw new NotSupportedException (); }
			}

			bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
				get { throw new NotSupportedException (); }
			}

			bool ICollection<KeyValuePair<string, object>>.Remove (KeyValuePair<string, object> item) {
				throw new NotSupportedException ();
			}

			#endregion

			#region IEnumerable<KeyValuePair<string,object>> Members

			IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator () {
				return GetEnumerator ();
			}

			protected abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator ();

			#endregion

			#region IEnumerable Members

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
				return ((IEnumerable<KeyValuePair<string, object>>) this).GetEnumerator ();
			}

			#endregion
		}

		List<IEnumerable<JavaScriptConverter>> _converterList;
		static JavaScriptSerializer _defaultSerializer = new JavaScriptSerializer ();

		public JavaScriptSerializer () {
		}

		public JavaScriptSerializer (JavaScriptTypeResolver resolver) {
			throw new NotImplementedException ();
		}

		internal static JavaScriptSerializer DefaultSerializer {
			get { return _defaultSerializer; }
		}

		public int MaxJsonLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public int RecursionLimit {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public T ConvertToType<T> (object obj) {
			if (obj == null)
				return default (T);

			return (T) ConvertToType (typeof (T), obj);
		}

		internal object ConvertToType (Type type, object obj) {
			if (obj == null)
				return null;

			if (obj is IDictionary<string, object>) {
				if (type == null)
					obj = Evaluate ((IDictionary<string, object>) obj);
				else {
					JavaScriptConverter converter = GetConverter (type);
					if (converter != null)
						return converter.Deserialize (
							Evaluate ((IDictionary<string, object>) obj),
							type, this);
				}

				return Deserialize ((IDictionary<string, object>) obj, type);
			}
			if (obj is IEnumerable<object>)
				return Deserialize ((IEnumerable<object>) obj, type);

			if (type == null)
				return obj;

			Type sourceType = obj.GetType ();
			if (type.IsAssignableFrom (sourceType))
				return obj;

			TypeConverter c = TypeDescriptor.GetConverter (type);
			if (c.CanConvertFrom(sourceType)) {
				if (obj is string)
					return c.ConvertFromInvariantString((string)obj);

				return c.ConvertFrom (obj);
			}
			
			return Convert.ChangeType (obj, type);
		}

		public T Deserialize<T> (string input) {
			return ConvertToType<T> (DeserializeObject(input));
		}

		static object Evaluate (object value) {
			if (value is IDictionary<string, object>)
				value = Evaluate ((IDictionary<string, object>) value);
			else
			if (value is IEnumerable<object>)
				value = Evaluate ((IEnumerable<object>) value);
			return value;
		}

		static object Evaluate (IEnumerable<object> e) {
			ArrayList list = new ArrayList ();
			foreach (object value in e)
				list.Add (Evaluate(value));

			return list;
		}

		static IDictionary<string, object> Evaluate (IDictionary<string, object> dict) {
			if (dict is Dictionary<string, object>)
				return dict;
			Dictionary<string, object> d = new Dictionary<string, object> (StringComparer.Ordinal);
			foreach (KeyValuePair<string, object> entry in dict)
				d.Add (entry.Key, Evaluate(entry.Value));

			return d;
		}

		static readonly Type typeofObject = typeof(object);
		static readonly Type typeofGenList = typeof (List<>);

		object Deserialize (IEnumerable<object> col, Type type) {
			Type elementType = null;
			if (type != null && type.HasElementType)
				elementType = type.GetElementType ();

			IList list;
			if (type == null || type.IsArray || typeofObject == type)
				list = new ArrayList ();
			else if (ReflectionUtils.IsInstantiatableType (type))
				// non-generic typed list
				list = (IList) Activator.CreateInstance (type, true);
			else if (ReflectionUtils.IsAssignable (type, typeofGenList)) {
				if (type.IsGenericType) {
					Type [] genArgs = type.GetGenericArguments ();
					elementType = genArgs [0];
					// generic list
					list = (IList) Activator.CreateInstance (typeofGenList.MakeGenericType (genArgs));
				}
				else
					list = new ArrayList ();
			}
			else
				throw new JsonSerializationException (string.Format ("Deserializing list type '{0}' not supported.", type.GetType ().Name));

			if (list.IsReadOnly) {
				Evaluate (col);
				return list;
			}

			foreach (object value in col)
				list.Add (ConvertToType (elementType, value));

			if (type != null && type.IsArray)
				list = ((ArrayList) list).ToArray (elementType);

			return list;
		}

		object Deserialize (IDictionary<string, object> dict, Type type) {
			if (type == null)
				type = Type.GetType ((string) dict ["__type"]);

			object target = Activator.CreateInstance (type, true);

			foreach (KeyValuePair<string, object> entry in dict) {
				object value = entry.Value;
				if (target is IDictionary) {
					((IDictionary) target).Add (entry.Key, ConvertToType (ReflectionUtils.GetTypedDictionaryValueType (type), value));
					continue;
				}
				MemberInfo [] memberCollection = type.GetMember (entry.Key);
				if (memberCollection == null || memberCollection.Length == 0) {
					//must evaluate value
					Evaluate (value);
					continue;
				}

				MemberInfo member = memberCollection [0];

				if (!ReflectionUtils.CanSetMemberValue (member)) {
					//must evaluate value
					Evaluate (value);
					continue;
				}
				
				ReflectionUtils.SetMemberValue (member, target, ConvertToType(ReflectionUtils.GetMemberUnderlyingType (member), value));
			}

			return target;
		}

		public object DeserializeObject (string input) {
			return DeserializeObject (new StringReader (input));
		}

		internal object DeserializeObject (TextReader input) {
			JsonSerializer ser = new JsonSerializer (this);
			return ser.Deserialize (input);
		}

		public void RegisterConverters (IEnumerable<JavaScriptConverter> converters) {
			if (converters == null)
				throw new ArgumentNullException ("converters");

			if (_converterList == null)
				_converterList = new List<IEnumerable<JavaScriptConverter>> ();
			_converterList.Add (converters);
		}

		internal JavaScriptConverter GetConverter (Type type) {
			if (_converterList != null)
				for (int i = 0; i < _converterList.Count; i++) {
					foreach (JavaScriptConverter converter in _converterList [i])
						foreach (Type supportedType in converter.SupportedTypes)
							if (supportedType == type)
								return converter;
				}

			return null;
		}

		public string Serialize (object obj) {
			StringBuilder b = new StringBuilder ();
			Serialize (obj, b);
			return b.ToString ();
		}

		public void Serialize (object obj, StringBuilder output) {
			Serialize (obj, new StringWriter (output));
		}

		internal void Serialize (object obj, TextWriter output) {
			JsonSerializer ser = new JsonSerializer (this);
			ser.Serialize (output, obj);
		}
	}
}
