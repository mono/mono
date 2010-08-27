//
// System.Runtime.Serialization.SerializationInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
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
	[System.Runtime.InteropServices.ComVisibleAttribute (true)]
	public sealed class SerializationInfo
	{
		Hashtable serialized = new Hashtable ();
		ArrayList values = new ArrayList ();

		string assemblyName; // the assembly being serialized
		string fullTypeName; // the type being serialized.
#if NET_4_0
		Type objectType;
		bool isAssemblyNameSetExplicit;
		bool isFullTypeNameSetExplicit;
#endif

		IFormatterConverter converter;
		
		/* used by the runtime */
		private SerializationInfo (Type type)
		{
			assemblyName = type.Assembly.FullName;
			fullTypeName = type.FullName;
			converter = new FormatterConverter ();
#if NET_4_0
			objectType = type;
#endif
		}
		
		/* used by the runtime */
		private SerializationInfo (Type type, SerializationEntry [] data)
		{
			int len = data.Length;

			assemblyName = type.Assembly.FullName;
			fullTypeName = type.FullName;
			converter = new FormatterConverter ();
#if NET_4_0
			objectType = type;
#endif

			for (int i = 0; i < len; i++) {
				serialized.Add (data [i].Name, data [i]);
				values.Add (data [i]);
			}
		}

		// Constructor
		[CLSCompliant (false)]
		public SerializationInfo (Type type, IFormatterConverter converter)
		{
			if (type == null)
				throw new ArgumentNullException ("type", "Null argument");

			if (converter == null)
				throw new ArgumentNullException ("converter", "Null argument");
			
			this.converter = converter;
			assemblyName = type.Assembly.FullName;
			fullTypeName = type.FullName;
#if NET_4_0
			objectType = type;
#endif
		}

		// Properties
		public string AssemblyName
		{
			get { return assemblyName; }
			
			set {
				if (value == null)
					throw new ArgumentNullException ("Argument is null.");
				assemblyName = value;
#if NET_4_0
				isAssemblyNameSetExplicit = true;
#endif
			}
		}
		
		public string FullTypeName
		{
			get { return fullTypeName; }
			
			set {
				if ( value == null)
					throw new ArgumentNullException ("Argument is null.");
				fullTypeName = value;
#if NET_4_0
				isFullTypeNameSetExplicit = true;
#endif
			}
		}
		
		public int MemberCount
		{
			get { return serialized.Count; }
		}

#if NET_4_0
		public bool IsAssemblyNameSetExplicit {
			get {
				return isAssemblyNameSetExplicit;
			}
		}

		public bool IsFullTypeNameSetExplicit {
			get {
				return isFullTypeNameSetExplicit;
			}
		}

		public Type ObjectType {
			get {
				return objectType;
			}
		}
#endif

		// Methods
		public void AddValue (string name, object value, Type type)
		{
			if (name == null)
				throw new ArgumentNullException ("name is null");
			if (type == null)
				throw new ArgumentNullException ("type is null");
			
			if (serialized.ContainsKey (name))
				throw new SerializationException ("Value has been serialized already.");
			
			SerializationEntry entry = new SerializationEntry (name, type, value);

			serialized.Add (name, entry);
			values.Add (entry);
		}

		public object GetValue (string name, Type type)
		{
			if (name == null)
				throw new ArgumentNullException ("name is null.");
			if (type == null)
				throw new ArgumentNullException ("type");
			if (!serialized.ContainsKey (name))
				throw new SerializationException ("No element named " + name + " could be found.");
						
			SerializationEntry entry = (SerializationEntry) serialized [name];

			if (entry.Value != null && !type.IsInstanceOfType (entry.Value))
				return converter.Convert (entry.Value, type);
			else
				return entry.Value;
		}

		internal bool HasKey (string name)
		{
			return serialized [name] != null;
		}
		
		public void SetType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type is null.");

			fullTypeName = type.FullName;
			assemblyName = type.Assembly.FullName;
#if NET_4_0
			objectType = type;
			isAssemblyNameSetExplicit = false;
			isFullTypeNameSetExplicit = false;
#endif
		}

		public SerializationInfoEnumerator GetEnumerator ()
		{
			return new SerializationInfoEnumerator (values);
		}
		
		public void AddValue (string name, short value)
		{
			AddValue (name, value, typeof (System.Int16));
		}

		[CLSCompliant(false)]
		public void AddValue (string name, UInt16 value)
		{
			AddValue (name, value, typeof (System.UInt16));
		}
		
		public void AddValue (string name, int value)
		{
			AddValue (name, value, typeof (System.Int32));
		}
		
		public void AddValue (string name, byte value)
		{
			AddValue (name, value, typeof (System.Byte));
		}
		
		public void AddValue (string name, bool value)
		{
			AddValue (name, value, typeof (System.Boolean));
		}
	       
		public void AddValue (string name, char value)
		{
			AddValue (name, value, typeof (System.Char));
		}

		[CLSCompliant(false)]
		public void AddValue (string name, SByte value)
		{
			AddValue (name, value, typeof (System.SByte));
		}
		
		public void AddValue (string name, double value)
		{
			AddValue (name, value, typeof (System.Double));
		}
		
		public void AddValue (string name, Decimal value)
		{
			AddValue (name, value, typeof (System.Decimal));
		}
		
		public void AddValue (string name, DateTime value)
		{
			AddValue (name, value, typeof (System.DateTime));
		}
		
		public void AddValue (string name, float value)
		{
			AddValue (name, value, typeof (System.Single));
		}

		[CLSCompliant(false)]
		public void AddValue (string name, UInt32 value)
		{
			AddValue (name, value, typeof (System.UInt32));
		}
	       
		public void AddValue (string name, long value)
		{
			AddValue (name, value, typeof (System.Int64));
		}

		[CLSCompliant(false)]
		public void AddValue (string name, UInt64 value)
		{
			AddValue (name, value, typeof (System.UInt64));
		}
		
		public void AddValue (string name, object value)
		{
			if (value == null)
				AddValue (name, value, typeof (System.Object));
			else
				AddValue (name, value, value.GetType ());
		}		
		
		public bool GetBoolean (string name)
		{
			object value = GetValue (name, typeof (System.Boolean));
			return converter.ToBoolean (value);
		}
		
		public byte GetByte (string name)
		{
			object value = GetValue (name, typeof (System.Byte));
			return converter.ToByte (value);
		}
		
		public char GetChar (string name)
		{
			object value = GetValue (name, typeof (System.Char));
			return converter.ToChar (value);
		}

		public DateTime GetDateTime (string name)
		{
			object value = GetValue (name, typeof (System.DateTime));
			return converter.ToDateTime (value);
		}
		
		public Decimal GetDecimal (string name)
		{
			object value = GetValue (name, typeof (System.Decimal));
			return converter.ToDecimal (value);
		}
		
		public double GetDouble (string name)
		{
			object value = GetValue (name, typeof (System.Double));
			return converter.ToDouble (value);
		}
						
		public short GetInt16 (string name)
		{
			object value = GetValue (name, typeof (System.Int16));
			return converter.ToInt16 (value);
		}
		
		public int GetInt32 (string name)
		{
			object value = GetValue (name, typeof (System.Int32));
			return converter.ToInt32 (value);
		}
	       
		public long GetInt64 (string name)
		{
			object value = GetValue (name, typeof (System.Int64));
			return converter.ToInt64 (value);
		}

		[CLSCompliant(false)]
		public SByte GetSByte (string name)
		{
			object value = GetValue (name, typeof (System.SByte));
			return converter.ToSByte (value);
		}
		
		public float GetSingle (string name)
		{
			object value = GetValue (name, typeof (System.Single));
			return converter.ToSingle (value);
		}
		
		public string GetString (string name)
		{
			object value = GetValue (name, typeof (System.String));
			if (value == null) return null;
			return converter.ToString (value);
		}

		[CLSCompliant(false)]
		public UInt16 GetUInt16 (string name)
		{
			object value = GetValue (name, typeof (System.UInt16));
			return converter.ToUInt16 (value);
		}
		
		[CLSCompliant(false)]
		public UInt32 GetUInt32 (string name)
		{
			object value = GetValue (name, typeof (System.UInt32));
			return converter.ToUInt32 (value);
		}
		[CLSCompliant(false)]
		public UInt64 GetUInt64 (string name)
		{
			object value = GetValue (name, typeof (System.UInt64));
			return converter.ToUInt64 (value);
		}

		/* used by the runtime */
#pragma warning disable 169		
		private SerializationEntry [] get_entries ()
		{
			SerializationEntry [] res = new SerializationEntry [this.MemberCount];
			int i = 0;
			
			foreach (SerializationEntry e in this)
				res [i++] = e;
			
			return res;
		}
#pragma warning restore 169		
	}
}
