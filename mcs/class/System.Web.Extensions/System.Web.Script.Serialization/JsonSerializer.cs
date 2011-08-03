//
// JsonSerializer.cs
//
// Author:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2008 Novell, Inc.  http://novell.com/
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.Web.Script.Serialization
{
	internal sealed class JsonSerializer
	{
		internal static readonly long InitialJavaScriptDateTicks = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
                static readonly DateTime MinimumJavaScriptDate = new DateTime (100, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		static readonly MethodInfo serializeGenericDictionary = typeof (JsonSerializer).GetMethod ("SerializeGenericDictionary", BindingFlags.NonPublic | BindingFlags.Instance);

		Dictionary <object, bool> objectCache;
		JavaScriptSerializer serializer;
		JavaScriptTypeResolver typeResolver;
		int recursionLimit;
		int maxJsonLength;
		int recursionDepth;
		
		Dictionary <Type, MethodInfo> serializeGenericDictionaryMethods;
		
		public JsonSerializer (JavaScriptSerializer serializer)
		{
			if (serializer == null)
				throw new ArgumentNullException ("serializer");
			this.serializer = serializer;
			typeResolver = serializer.TypeResolver;
			recursionLimit = serializer.RecursionLimit;
			maxJsonLength = serializer.MaxJsonLength;
		}

		public void Serialize (object obj, StringBuilder output)
		{
			if (output == null)
				throw new ArgumentNullException ("output");
			
			DoSerialize (obj, output);
		}

		public void Serialize (object obj, TextWriter output)
		{
			if (output == null)
				throw new ArgumentNullException ("output");

			StringBuilder sb = new StringBuilder ();
			DoSerialize (obj, sb);
			output.Write (sb.ToString ());
		}

		void DoSerialize (object obj, StringBuilder output)
		{
			recursionDepth = 0;
			objectCache = new Dictionary <object, bool> ();
			SerializeValue (obj, output);
		}
		
		void SerializeValue (object obj, StringBuilder output)
		{
			recursionDepth++;
			SerializeValueImpl (obj, output);
			recursionDepth--;
		}
		
		void SerializeValueImpl (object obj, StringBuilder output)
		{
			if (recursionDepth > recursionLimit)
				throw new ArgumentException ("Recursion limit has been exceeded while serializing object of type '{0}'", obj != null ? obj.GetType ().ToString () : "[null]");

			if (obj == null || DBNull.Value.Equals (obj)) {
				StringBuilderExtensions.AppendCount (output, maxJsonLength, "null");
				return;
			}

			Type valueType = obj.GetType ();
			JavaScriptConverter jsc = serializer.GetConverter (valueType);
			if (jsc != null) {
				IDictionary <string, object> result = jsc.Serialize (obj, serializer);

				if (result == null) {
					StringBuilderExtensions.AppendCount (output, maxJsonLength, "null");
					return;
				}

				if (typeResolver != null) {
					string typeId = typeResolver.ResolveTypeId (valueType);
					if (!String.IsNullOrEmpty (typeId))
						result [JavaScriptSerializer.SerializedTypeNameKey] = typeId;
				}

				SerializeValue (result, output);
				return;
			}

			TypeCode typeCode = Type.GetTypeCode (valueType);
			switch (typeCode) {
				case TypeCode.String:
					WriteValue (output, (string)obj);
                                        return;
					
                                case TypeCode.Char:
					WriteValue (output, (char)obj);
                                        return;
					
                                case TypeCode.Boolean:
					WriteValue (output, (bool)obj);
                                        return;
					
                                case TypeCode.SByte:
                                case TypeCode.Int16:
                                case TypeCode.UInt16:
                                case TypeCode.Int32:
                                case TypeCode.Byte:
                                case TypeCode.UInt32:
                                case TypeCode.Int64:
                                case TypeCode.UInt64:
					if (valueType.IsEnum) {
						WriteEnumValue (output, obj, typeCode);
						return;
					}
					goto case TypeCode.Decimal;
					
                                case TypeCode.Single:
					WriteValue (output, (float)obj);
					return;
					
                                case TypeCode.Double:
					WriteValue (output, (double)obj);
					return;
					
				case TypeCode.Decimal:
					WriteValue (output, obj as IConvertible);
                                        return;
					
                                case TypeCode.DateTime:
					WriteValue (output, (DateTime)obj);
					return;
			}
			
			if (typeof (Uri).IsAssignableFrom (valueType)) {
				WriteValue (output, (Uri)obj);
				return;
			}

			if (typeof (Guid).IsAssignableFrom (valueType)) {
				WriteValue (output, (Guid)obj);
				return;
			}
			
			IConvertible convertible = obj as IConvertible;
			if (convertible != null) {
				WriteValue (output, convertible);
				return;
			}

			try {
				if (objectCache.ContainsKey (obj))
					throw new InvalidOperationException ("Circular reference detected.");
				objectCache.Add (obj, true);

				Type closedIDict = GetClosedIDictionaryBase(valueType);
				if (closedIDict != null) {
					if (serializeGenericDictionaryMethods == null)
						serializeGenericDictionaryMethods = new Dictionary <Type, MethodInfo> ();

					MethodInfo mi;
					if (!serializeGenericDictionaryMethods.TryGetValue (closedIDict, out mi)) {
						Type[] types = closedIDict.GetGenericArguments ();
						mi = serializeGenericDictionary.MakeGenericMethod (types [0], types [1]);
						serializeGenericDictionaryMethods.Add (closedIDict, mi);
					}

					mi.Invoke (this, new object[] {output, obj});
					return;
				}				

				IDictionary dict = obj as IDictionary;
				if (dict != null) {
					SerializeDictionary (output, dict);
					return;
				}

				IEnumerable enumerable = obj as IEnumerable;
				if (enumerable != null) {
					SerializeEnumerable (output, enumerable);
					return;
				}

				SerializeArbitraryObject (output, obj, valueType);
			} finally {
				objectCache.Remove (obj);
			}
		}
		
		Type GetClosedIDictionaryBase(Type t) {
			if(t.IsGenericType && typeof (IDictionary <,>).IsAssignableFrom (t.GetGenericTypeDefinition ()))
				return t;
				
			foreach(Type iface in t.GetInterfaces()) {
				if(iface.IsGenericType && typeof (IDictionary <,>).IsAssignableFrom (iface.GetGenericTypeDefinition ()))
					return iface;
			}

			return null;
		}

		bool ShouldIgnoreMember (MemberInfo mi, out MethodInfo getMethod)
		{
			getMethod = null;
			if (mi == null)
				return true;
			
			if (mi.IsDefined (typeof (ScriptIgnoreAttribute), true))
				return true;
			
			FieldInfo fi = mi as FieldInfo;
			if (fi != null)
				return false;
			
			PropertyInfo pi = mi as PropertyInfo;
			if (pi == null)
				return true;
			
			getMethod = pi.GetGetMethod ();
			if (getMethod == null || getMethod.GetParameters ().Length > 0) {
				getMethod = null;
				return true;
			}
			
			return false;
		}

		object GetMemberValue (object obj, MemberInfo mi)
		{
			FieldInfo fi = mi as FieldInfo;

			if (fi != null)
				return fi.GetValue (obj);

			MethodInfo method = mi as MethodInfo;
			if (method == null)
				throw new InvalidOperationException ("Member is not a method (internal error).");

			object ret;

			try {
				ret = method.Invoke (obj, null);
			} catch (TargetInvocationException niex) {
				if (niex.InnerException is NotImplementedException) {
					Console.WriteLine ("!!! COMPATIBILITY WARNING. FEATURE NOT IMPLEMENTED. !!!");
					Console.WriteLine (niex);
					Console.WriteLine ("!!! RETURNING NULL. PLEASE LET MONO DEVELOPERS KNOW ABOUT THIS EXCEPTION. !!!");
					return null;
				}

				throw;
			}

			return ret;
		}
		
		void SerializeArbitraryObject (StringBuilder output, object obj, Type type)
		{
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "{");

			bool first = true;
			if (typeResolver != null) {
				string typeId = typeResolver.ResolveTypeId (type);
				if (!String.IsNullOrEmpty (typeId)) {
					WriteDictionaryEntry (output, first, JavaScriptSerializer.SerializedTypeNameKey, typeId);
					first = false;
				}
			}

			SerializeMembers <FieldInfo> (type.GetFields (BindingFlags.Public | BindingFlags.Instance), obj, output, ref first);
			SerializeMembers <PropertyInfo> (type.GetProperties (BindingFlags.Public | BindingFlags.Instance), obj, output, ref first);

			StringBuilderExtensions.AppendCount (output, maxJsonLength, "}");
		}

		void SerializeMembers <T> (T[] members, object obj, StringBuilder output, ref bool first) where T: MemberInfo
		{
			MemberInfo member;
			MethodInfo getMethod;
			string name;
			
			foreach (T mi in members) {
				if (ShouldIgnoreMember (mi as MemberInfo, out getMethod))
					continue;

				name = mi.Name;
				if (getMethod != null)
					member = getMethod;
				else
					member = mi;

				WriteDictionaryEntry (output, first, name, GetMemberValue (obj, member));
				if (first)
					first = false;
			}
		}
		
		void SerializeEnumerable (StringBuilder output, IEnumerable enumerable)
		{
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "[");
			bool first = true;
			foreach (object value in enumerable) {
				if (!first)
					StringBuilderExtensions.AppendCount (output, maxJsonLength, ',');
				SerializeValue (value, output);
				if (first)
					first = false;
			}
			
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "]");
		}
		
		void SerializeDictionary (StringBuilder output, IDictionary dict)
		{
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "{");
			bool first = true;
			string key;
			
			foreach (DictionaryEntry entry in dict) {
				WriteDictionaryEntry (output, first, entry.Key as string, entry.Value);
				if (first)
					first = false;
			}
			
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "}");
		}

		void SerializeGenericDictionary <TKey, TValue> (StringBuilder output, IDictionary <TKey, TValue> dict)
		{
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "{");
			bool first = true;
			string key;
			
			foreach (KeyValuePair <TKey, TValue> kvp in dict) {
				WriteDictionaryEntry (output, first, kvp.Key as string, kvp.Value);
				if (first)
					first = false;
			}
			
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "}");
		}

		void WriteDictionaryEntry (StringBuilder output, bool skipComma, string key, object value)
		{
			if (key == null)
				throw new InvalidOperationException ("Only dictionaries with keys convertible to string are supported.");
			
			if (!skipComma)
				StringBuilderExtensions.AppendCount (output, maxJsonLength, ',');

			WriteValue (output, key);
			StringBuilderExtensions.AppendCount (output, maxJsonLength, ':');
			SerializeValue (value, output);
		}

		void WriteEnumValue (StringBuilder output, object value, TypeCode typeCode)
		{
			switch (typeCode) {
				case TypeCode.SByte:
					StringBuilderExtensions.AppendCount (output, maxJsonLength, (sbyte)value);
					return;
					
                                case TypeCode.Int16:
					StringBuilderExtensions.AppendCount (output, maxJsonLength, (short)value);
					return;
					
                                case TypeCode.UInt16:
					StringBuilderExtensions.AppendCount (output, maxJsonLength, (ushort)value);
					return;
					
                                case TypeCode.Int32:
					StringBuilderExtensions.AppendCount (output, maxJsonLength, (int)value);
					return;
					
                                case TypeCode.Byte:
					StringBuilderExtensions.AppendCount (output, maxJsonLength, (byte)value);
					return;
					
                                case TypeCode.UInt32:
					StringBuilderExtensions.AppendCount (output, maxJsonLength, (uint)value);
					return;
					
                                case TypeCode.Int64:
					StringBuilderExtensions.AppendCount (output, maxJsonLength, (long)value);
					return;
					
                                case TypeCode.UInt64:
					StringBuilderExtensions.AppendCount (output, maxJsonLength, (ulong)value);
					return;

				default:
					throw new InvalidOperationException (String.Format ("Invalid type code for enum: {0}", typeCode));
			}
		}

		void WriteValue (StringBuilder output, float value)
		{
			StringBuilderExtensions.AppendCount (output, maxJsonLength, value.ToString ("r"));
		}

		void WriteValue (StringBuilder output, double value)
		{
			StringBuilderExtensions.AppendCount (output, maxJsonLength, value.ToString ("r"));
		}
		
		void WriteValue (StringBuilder output, Guid value)
		{
			WriteValue (output, value.ToString ());
		}
		
		void WriteValue (StringBuilder output, Uri value)
		{
			WriteValue (output, value.OriginalString);
		}
		
		void WriteValue (StringBuilder output, DateTime value)
		{
			value = value.ToUniversalTime ();

			if (value < MinimumJavaScriptDate)
				value = MinimumJavaScriptDate;

			long ticks = (value.Ticks - InitialJavaScriptDateTicks) / (long)10000;
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "\"\\/Date(" + ticks + ")\\/\"");
		}
		
		void WriteValue (StringBuilder output, IConvertible value)
		{
			StringBuilderExtensions.AppendCount (output, maxJsonLength, value.ToString (CultureInfo.InvariantCulture));
		}
		
		void WriteValue (StringBuilder output, bool value)
		{
			StringBuilderExtensions.AppendCount (output, maxJsonLength, value ? "true" : "false");
		}
		
		void WriteValue (StringBuilder output, char value)
		{
			if (value == '\0') {
				StringBuilderExtensions.AppendCount (output, maxJsonLength, "null");
				return;
			}
			
			WriteValue (output, value.ToString ());
		}
		
		void WriteValue (StringBuilder output, string value)
		{
			if (String.IsNullOrEmpty (value)) {
				StringBuilderExtensions.AppendCount (output, maxJsonLength, "\"\"");
				return;
			}
			
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "\"");

			char c;
			for (int i = 0; i < value.Length; i++) {
				c = value [i];

				switch (c) {
					case '\t':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, @"\t");
						break;
					case '\n':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, @"\n");
						break;
					case '\r':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, @"\r");
						break;
					case '\f':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, @"\f");
						break;
					case '\b':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, @"\b");
						break;
					case '<':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, @"\u003c");
						break;
					case '>':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, @"\u003e");
						break;
					case '"':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, "\\\"");
						break;
					case '\'':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, @"\u0027");
						break;
					case '\\':
						StringBuilderExtensions.AppendCount (output, maxJsonLength, @"\\");
						break;
					default:
						if (c > '\u001f')
							StringBuilderExtensions.AppendCount (output, maxJsonLength, c);
						else {
							output.Append("\\u00");
							int intVal = (int) c;
							StringBuilderExtensions.AppendCount (output, maxJsonLength, (char) ('0' + (intVal >> 4)));
							intVal &= 0xf;
							StringBuilderExtensions.AppendCount (output, maxJsonLength, (char) (intVal < 10 ? '0' + intVal : 'a' + (intVal - 10)));
						}
						break;
				}
			}
			
			StringBuilderExtensions.AppendCount (output, maxJsonLength, "\"");
		}
	}
}
