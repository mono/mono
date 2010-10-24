//
// System.Web.UI.ObjectStateFormatter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2003 Ben Maurer
// (c) Copyright 2004-2010 Novell, Inc. (http://www.novell.com)
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

//#define TRACE

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.Util;
using System.Diagnostics;
using System.Web.Configuration;

namespace System.Web.UI
{
	public sealed class ObjectStateFormatter : IFormatter, IStateFormatter
	{
		const ushort SERIALIZED_STREAM_MAGIC = 0x01FF;

		Page page;
		MachineKeySection section;

		public ObjectStateFormatter ()
		{
		}
		
		internal ObjectStateFormatter (Page page)
		{
			this.page = page;
		}

		bool EnableMac {
			get {
				return (page == null) ? (section != null) : page.EnableViewStateMac;
			}
		}

		bool NeedViewStateEncryption {
			get {
				return (page == null) ? false : page.NeedViewStateEncryption;
			}
		}

		internal MachineKeySection Section {
			get {
				if (section == null)
					section = (MachineKeySection) WebConfigurationManager.GetWebApplicationSection ("system.web/machineKey");
				return section;
			}
			set {
				section = value;
			}
		}

		public object Deserialize (Stream inputStream)
		{
			if (inputStream == null)
				throw new ArgumentNullException ("inputStream");

			BinaryReader reader = new BinaryReader (inputStream);
			short magic = reader.ReadInt16 ();
			if (magic != SERIALIZED_STREAM_MAGIC)
				throw new ArgumentException ("The serialized data is invalid");

			return DeserializeObject (reader);
		}
		
		public object Deserialize (string inputString)
		{
			if (inputString == null)
				throw new ArgumentNullException ("inputString");
			if (inputString.Length == 0)
				throw new ArgumentNullException ("inputString");

			byte [] data = Convert.FromBase64String (inputString);
			if (data == null || (data.Length) == 0)
				throw new ArgumentNullException ("inputString");

			if (NeedViewStateEncryption) {
				if (EnableMac) {
					data = MachineKeySectionUtils.VerifyDecrypt (Section, data);
				} else {
					data = MachineKeySectionUtils.Decrypt (Section, data);
				}
			} else if (EnableMac) {
				data = MachineKeySectionUtils.Verify (Section, data);
			}

			if (data == null)
				throw new HttpException ("Unable to validate data.");

			using (MemoryStream ms = new MemoryStream (data)) {
				return Deserialize (ms);
			}
		}
		
		public string Serialize (object stateGraph)
		{
			if (stateGraph == null)
				return String.Empty;

			byte[] data = null;
			using (MemoryStream ms = new MemoryStream ()) {
				Serialize (ms, stateGraph);
				data = ms.GetBuffer ();
			}

			if (NeedViewStateEncryption) {
				if (EnableMac) {
					data = MachineKeySectionUtils.EncryptSign (Section, data);
				} else {
					data = MachineKeySectionUtils.Encrypt (Section, data);
				}
			} else if (EnableMac) {
				data = MachineKeySectionUtils.Sign (Section, data);
			}
			
			return Convert.ToBase64String (data, 0, data.Length);
		}
		
		public void Serialize (Stream outputStream, object stateGraph)
		{
			if (outputStream == null)
				throw new ArgumentNullException ("outputStream");

			if (stateGraph == null)
				throw new ArgumentNullException ("stateGraph");

			BinaryWriter writer = new BinaryWriter (outputStream);
			writer.Write (SERIALIZED_STREAM_MAGIC);

			SerializeValue (writer, stateGraph);
		}
		
		void SerializeValue (BinaryWriter w, object o)
		{
			ObjectFormatter.WriteObject (w, o, new WriterContext ());
		}
		
		object DeserializeObject (BinaryReader r)
		{
			return ObjectFormatter.ReadObject (r, new ReaderContext ());
		}
		
#region IFormatter
		
		object IFormatter.Deserialize (Stream serializationStream)
		{
			return Deserialize (serializationStream);
		}
		
		void IFormatter.Serialize (Stream serializationStream, object stateGraph)
		{
			Serialize (serializationStream, stateGraph);
		}
		
		SerializationBinder IFormatter.Binder {
			get { return null; }
			set { }
		}
		
		StreamingContext IFormatter.Context {
			get { return new StreamingContext (StreamingContextStates.All); }
			set { }
		}
		
		ISurrogateSelector IFormatter.SurrogateSelector {
			get { return null; }
			set { }
		}
		
#endregion

#region Object Readers/Writers
		
		sealed class WriterContext
		{
			Hashtable cache;
			short nextKey = 0;
			short key = 0;

			public short Key {
				get { return key; }
			}

			public bool RegisterCache (object o)
			{
				if (nextKey == short.MaxValue)
					return false;

				if (cache == null) {
					cache = new Hashtable ();
					cache.Add (o, key = nextKey++);
					return false;
				}
				
				object posKey = cache [o];
				if (posKey == null) {
					cache.Add (o, key = nextKey++);
					return false;
				}
				
				key = (short) posKey;
				return true;
			}
		}
		
		sealed class ReaderContext
		{
			ArrayList cache;
			
			public void CacheItem (object o)
			{
				if (cache == null)
					cache = new ArrayList ();
				
				cache.Add (o);
			}
			
			public object GetCache (short key)
			{
				return cache [key];
			}
		}
		
		abstract class ObjectFormatter
		{
			static readonly Hashtable writeMap = new Hashtable ();
			static ObjectFormatter [] readMap = new ObjectFormatter [256];
			static BinaryObjectFormatter binaryObjectFormatter;
			static TypeFormatter typeFormatter;
			static EnumFormatter enumFormatter;
			static SingleRankArrayFormatter singleRankArrayFormatter;
			static TypeConverterFormatter typeConverterFormatter;
			
			static ObjectFormatter ()
			{			
				new StringFormatter ().Register ();
				new Int64Formatter ().Register ();
				new Int32Formatter ().Register ();
				new Int16Formatter ().Register ();
				new ByteFormatter ().Register ();
				new BooleanFormatter ().Register ();
				new CharFormatter ().Register ();
				new DateTimeFormatter ().Register ();
				new PairFormatter ().Register ();
				new TripletFormatter ().Register ();
				new ArrayListFormatter ().Register ();
				new HashtableFormatter ().Register ();
				new ObjectArrayFormatter ().Register ();
				new UnitFormatter ().Register ();
				new FontUnitFormatter ().Register ();
				new IndexedStringFormatter ().Register ();
				new ColorFormatter ().Register ();

				enumFormatter = new EnumFormatter ();
				enumFormatter.Register ();

				typeFormatter = new TypeFormatter ();
				typeFormatter.Register ();

				singleRankArrayFormatter = new SingleRankArrayFormatter ();
				singleRankArrayFormatter.Register ();

				typeConverterFormatter = new TypeConverterFormatter ();
				typeConverterFormatter.Register ();

				binaryObjectFormatter = new BinaryObjectFormatter ();
				binaryObjectFormatter.Register ();
			}
		
			// 0 == null
			static byte nextId = 1;
			
			public ObjectFormatter ()
			{
				PrimaryId = nextId ++;
				if (NumberOfIds == 1)
					return;
				
				SecondaryId = nextId ++;
				if (NumberOfIds == 2)
					return;
				
				TertiaryId = nextId ++;
				if (NumberOfIds == 3)
					return;
				
				throw new Exception ();
			}
			
			protected readonly byte PrimaryId, SecondaryId = 255, TertiaryId = 255;
			
			protected abstract void Write (BinaryWriter w, object o, WriterContext ctx);
			protected abstract object Read (byte token, BinaryReader r, ReaderContext ctx);
			protected abstract Type Type { get; }
			protected virtual int NumberOfIds { get { return 1; } }
			
			public virtual void Register ()
			{
				writeMap [Type] = this;
				readMap [PrimaryId] = this;
				if (SecondaryId != 255) {
					readMap [SecondaryId] = this;
					if (TertiaryId != 255)
						readMap [TertiaryId] = this;
				}
			}
			
			public static void WriteObject (BinaryWriter w, object o, WriterContext ctx)
			{
#if TRACE && !TARGET_J2EE
				if (o != null) {
					Trace.WriteLine (String.Format ("Writing {0} (type: {1})", o, o.GetType ()));
					Trace.Indent ();
				} else {
					Trace.WriteLine ("Writing null");
				}
				long pos = w.BaseStream.Position;
#endif
				
				if (o == null) {
					w.Write ((byte) 0);
					return;
				}
				
				Type t = o.GetType ();
#if TRACE
				Trace.WriteLine (String.Format ("Looking up formatter for type {0}", t));
#endif

				ObjectFormatter fmt = writeMap [t] as ObjectFormatter;
#if TRACE
				Trace.WriteLine (String.Format ("Formatter from writeMap: '{0}'", fmt));
#endif
				if (fmt == null) {
					// Handle abstract types here
					
					if (o is Type)
						fmt = typeFormatter;
					else if (t.IsEnum)
						fmt = enumFormatter;
					else if (t.IsArray && ((Array) o).Rank == 1)
						fmt = singleRankArrayFormatter;
					else {
						TypeConverter converter;
						converter = TypeDescriptor.GetConverter (o);
#if TRACE
						Trace.WriteLine (String.Format ("Type converter: '{0}' (to string: {1}; from {2}: {3})",
										converter,
										converter != null ? converter.CanConvertTo (typeof (string)) : false,
										t,
										converter != null ? converter.CanConvertFrom (t) : false));
#endif
						// Do not use the converter if it's an instance of
						// TypeConverter itself - it reports it is able to
						// convert to string, but it's only a conversion
						// consisting of a call to ToString() with no
						// reverse conversion supported. This leads to
						// problems when deserializing the object.
						if (converter == null || converter.GetType () == typeof (TypeConverter) ||
						    !converter.CanConvertTo (typeof (string)) || !converter.CanConvertFrom (typeof (string)))
							fmt = binaryObjectFormatter;
						else {
							typeConverterFormatter.Converter = converter;
							fmt = typeConverterFormatter;
						}
					}
				}

#if TRACE
				Trace.WriteLine (String.Format ("Writing with formatter '{0}'", fmt.GetType ()));
#endif
				fmt.Write (w, o, ctx);
#if TRACE && !TARGET_J2EE
				Trace.Unindent ();
				Trace.WriteLine (String.Format ("Wrote {0} (type: {1}) {2} bytes", o, o.GetType (), w.BaseStream.Position - pos));
#endif
			}
			
			public static object ReadObject (BinaryReader r, ReaderContext ctx)
			{
				byte sig = r.ReadByte ();
				
				if (sig == 0)
					return null;
				
				return readMap [sig].Read (sig, r, ctx);
			}
			
			protected void Write7BitEncodedInt (BinaryWriter w, int value)
			{
				do {
					int high = (value >> 7) & 0x01ffffff;
					byte b = (byte)(value & 0x7f);
	
					if (high != 0)
						b = (byte)(b | 0x80);
	
					w.Write(b);
					value = high;
				} while(value != 0);
			}
			
			protected int Read7BitEncodedInt (BinaryReader r)
			{
				int ret = 0;
				int shift = 0;
				byte b;
	
				do {
					b = r.ReadByte();
					
					ret = ret | ((b & 0x7f) << shift);
					shift += 7;
				} while ((b & 0x80) == 0x80);
	
				return ret;
			}
		}
		
#region Primitive Formatters
		class StringFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				if (ctx.RegisterCache (o)) {
					w.Write (SecondaryId);
					w.Write (ctx.Key);
				} else {
					w.Write (PrimaryId);
					w.Write ((string)o);
				}
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				if (token == PrimaryId) {
					string s = r.ReadString ();
					ctx.CacheItem (s);
					return s;
				} else {
					return ctx.GetCache (r.ReadInt16 ());
				}
			}
			protected override Type Type {
				get { return typeof (string); }
			}
			
			protected override int NumberOfIds {
				get { return 2; }
			}
		}

		class IndexedStringFormatter : StringFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				IndexedString s = o as IndexedString;

				if (s == null)
					throw new InvalidOperationException ("object is not of the IndexedString type");
				
				base.Write (w, s.Value, ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				string s = base.Read (token, r, ctx) as string;
				if (String.IsNullOrEmpty (s))
					throw new InvalidOperationException ("string must not be null or empty.");
				
				return new IndexedString (s);
			}
			
			protected override Type Type {
				get { return typeof (IndexedString); }
			}
			
			protected override int NumberOfIds {
				get { return 2; }
			}
		}
		
		class Int64Formatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				w.Write (PrimaryId);
				w.Write ((long)o);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				return r.ReadInt64 ();
			}
			protected override Type Type {
				get { return typeof (long); }
			}
		}
		
		class Int32Formatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				int i = (int) o;
				if ((int)(byte) i == i) {
					w.Write (SecondaryId);
					w.Write ((byte) i);
				} else {
					w.Write (PrimaryId);
					w.Write (i);
				}
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				if (token == PrimaryId)
					return r.ReadInt32 ();
				else
					return (int) r.ReadByte ();
			}
			
			protected override Type Type {
				get { return typeof (int); }
			}
			
			protected override int NumberOfIds {
				get { return 2; }
			}
		}
		
		class Int16Formatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				w.Write (PrimaryId);
				w.Write ((short)o);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				return r.ReadInt16 ();
			}

			protected override Type Type {
				get { return typeof (short); }
			}
		}
		
		class ByteFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				w.Write (PrimaryId);
				w.Write ((byte)o);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				return r.ReadByte ();
			}

			protected override Type Type {
				get { return typeof (byte); }
			}
		}
		
		class BooleanFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				if ((bool)o == true)
					w.Write (PrimaryId);
				else
					w.Write (SecondaryId);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				return token == PrimaryId;
			}
			
			protected override Type Type {
				get { return typeof (bool); }
			}
			
			protected override int NumberOfIds {
				get { return 2; }
			}
		}
		
		class CharFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				w.Write (PrimaryId);
				w.Write ((char) o);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				return r.ReadChar ();
			}
			
			protected override Type Type {
				get { return typeof (char); }
			}
		}
		
		class DateTimeFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				w.Write (PrimaryId);
				w.Write (((DateTime) o).Ticks);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				return new DateTime (r.ReadInt64 ());
			}
			
			protected override Type Type {
				get { return typeof (DateTime); }
			}
		}
		
		class PairFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				Pair p = (Pair) o;
				w.Write (PrimaryId);
				WriteObject (w, p.First, ctx);
				WriteObject (w, p.Second, ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				Pair p = new Pair ();
				p.First = ReadObject (r, ctx);
				p.Second = ReadObject (r, ctx);
				return p;
			}
			
			protected override Type Type {
				get { return typeof (Pair); }
			}
		}
		
		class TripletFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				Triplet t = (Triplet) o;
				w.Write (PrimaryId);
				WriteObject (w, t.First, ctx);
				WriteObject (w, t.Second, ctx);
				WriteObject (w, t.Third, ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				Triplet t = new Triplet ();
				t.First = ReadObject (r, ctx);
				t.Second = ReadObject (r, ctx);
				t.Third = ReadObject (r, ctx);
				return t;
			}
			
			protected override Type Type {
				get { return typeof (Triplet); }
			}
		}
		
		class ArrayListFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				ArrayList l = (ArrayList) o;
				
				w.Write (PrimaryId);
				Write7BitEncodedInt (w, l.Count);
				for (int i = 0; i < l.Count; i++)
					WriteObject (w, l [i], ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				int len = Read7BitEncodedInt (r);
				ArrayList l = new ArrayList (len);
				
				for (int i = 0; i < len; i++)
					l.Add (ReadObject (r, ctx));
				
				return l;
			}
			
			protected override Type Type {
				get { return typeof (ArrayList); }
			}
		}
		
		class HashtableFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				Hashtable ht = (Hashtable) o;
				
				w.Write (PrimaryId);
				Write7BitEncodedInt (w, ht.Count);
				foreach (DictionaryEntry de in ht) {
					WriteObject (w, de.Key, ctx);
					WriteObject (w, de.Value, ctx);
				}
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				int len = Read7BitEncodedInt (r);
				Hashtable ht = new Hashtable (len);
				
				for (int i = 0; i < len; i++) {
					object key = ReadObject (r, ctx);
					object val = ReadObject (r, ctx);
					
					ht.Add (key, val);
				}
				
				return ht;
			}
			
			protected override Type Type {
				get { return typeof (Hashtable); }
			}
		}
		
		class ObjectArrayFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				object [] val = (object []) o;
				
				w.Write (PrimaryId);
				Write7BitEncodedInt (w, val.Length);
				for (int i = 0; i < val.Length; i++)
					WriteObject (w, val [i], ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				int len = Read7BitEncodedInt (r);
				object [] ret = new object [len];
				
				for (int i = 0; i < len; i++)
					ret [i] = ReadObject (r, ctx);
				
				return ret;
			}
			
			protected override Type Type {
				get { return typeof (object []); }
			}
		}
		
#endregion
		
#region System.Web Optimizations
		class ColorFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				Color c = (Color) o;
				
				if (c.IsEmpty || c.IsKnownColor) {
					w.Write (SecondaryId);
					if (c.IsEmpty)
						w.Write (-1); //isempty marker
					else
						w.Write ((int) c.ToKnownColor ());
				} else {
					w.Write (PrimaryId);
					w.Write (c.ToArgb ());
				}
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				int value = r.ReadInt32 ();
				if (token == PrimaryId)
					return Color.FromArgb (value);
				else {
					if (value == -1) //isempty marker
						return Color.Empty;
					return Color.FromKnownColor ((KnownColor)value);
				}
			}
			
			protected override Type Type {
				get { return typeof (Color); }
			}
			
			protected override int NumberOfIds {
				get { return 2; }
			}
		}
		
#endregion
		
#region Special Formatters
		class EnumFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				object value = Convert.ChangeType (o, ((Enum) o).GetTypeCode ());
				w.Write (PrimaryId);
				WriteObject (w, o.GetType (), ctx);
				WriteObject (w, value, ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				Type t = (Type) ReadObject (r, ctx);
				object value = ReadObject (r, ctx);
				
				return Enum.ToObject (t, value);
			}

			protected override Type Type {
				get { return typeof (Enum); }
			}
		}
		
		class TypeFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				if (ctx.RegisterCache (o)) {
					w.Write (SecondaryId);
					w.Write (ctx.Key);
				} else {
					w.Write (PrimaryId);
					w.Write (((Type) o).FullName);

					// We should cache the name of the assembly
					w.Write (((Type) o).Assembly.FullName);
				}
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				if (token == PrimaryId) {
					string type = r.ReadString ();
					string assembly = r.ReadString ();
					Type t = Assembly.Load (assembly).GetType (type);
					ctx.CacheItem (t);
					return t;
				} else {
					return ctx.GetCache (r.ReadInt16 ());
				}
			}
			
			protected override Type Type {
				get { return typeof (Type); }
			}
			
			protected override int NumberOfIds {
				get { return 2; }
			}
		}
		
		class SingleRankArrayFormatter : ObjectFormatter
		{
			readonly BinaryFormatter _binaryFormatter = new BinaryFormatter ();

			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				Array val = (Array) o;
				if (val.GetType ().GetElementType ().IsPrimitive) {
					w.Write (SecondaryId);
					_binaryFormatter.Serialize (w.BaseStream, o);
					return;
				}
				
				w.Write (PrimaryId);
				WriteObject (w, val.GetType ().GetElementType (), ctx);
				
				Write7BitEncodedInt (w, val.Length);
				for (int i = 0; i < val.Length; i++)
					WriteObject (w, val.GetValue (i), ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				if (token == SecondaryId)
					return _binaryFormatter.Deserialize (r.BaseStream);
				Type t = (Type) ReadObject (r, ctx);
				int len = Read7BitEncodedInt (r);
				Array val = Array.CreateInstance (t, len);
				
				for (int i = 0; i < len; i++)
					val.SetValue (ReadObject (r, ctx), i);
				
				return val;
			}
			
			protected override Type Type {
				get { return typeof (Array); }
			}

			protected override int NumberOfIds {
				get { return 2; }
			}
		}
		
		class FontUnitFormatter : StringFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				base.Write (w, o.ToString (), ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				return FontUnit.Parse ((string) base.Read (token, r, ctx));
			}
			
			protected override Type Type {
				get { return typeof (FontUnit); }
			}
		}

		class UnitFormatter : StringFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				base.Write (w, o.ToString (), ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				return Unit.Parse ((string) base.Read (token, r, ctx));
			}
			
			protected override Type Type {
				get { return typeof (Unit); }
			}
		}

		class TypeConverterFormatter : StringFormatter
		{
			TypeConverter converter;

			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				w.Write (PrimaryId);
				ObjectFormatter.WriteObject (w, o.GetType (), ctx);
				string v = (string) converter.ConvertTo (null, Helpers.InvariantCulture,
									 o, typeof (string));
				base.Write (w, v, ctx);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				Type t = (Type) ObjectFormatter.ReadObject (r, ctx);
				converter = TypeDescriptor.GetConverter (t);
				token = r.ReadByte ();
				string v = (string) base.Read (token, r, ctx);
				return converter.ConvertFrom (null, Helpers.InvariantCulture, v);
			}
			
			protected override Type Type {
				get { return typeof (TypeConverter); }
			}

			public TypeConverter Converter {
				set { converter = value; }
			}
		}

		class BinaryObjectFormatter : ObjectFormatter
		{
			protected override void Write (BinaryWriter w, object o, WriterContext ctx)
			{
				w.Write (PrimaryId);
				
				MemoryStream ms = new MemoryStream (128);
				new BinaryFormatter ().Serialize (ms, o);
				
				byte [] buf = ms.GetBuffer ();
				Write7BitEncodedInt (w, buf.Length);
				w.Write (buf, 0, buf.Length);
			}
			
			protected override object Read (byte token, BinaryReader r, ReaderContext ctx)
			{
				int len = Read7BitEncodedInt (r);
				byte [] buf = r.ReadBytes (len);
				if (buf.Length != len)
					throw new Exception ();
				
				return new BinaryFormatter ().Deserialize (new MemoryStream (buf));
			}
			
			protected override Type Type {
				get { return typeof (object); }
			}
		}
		
#endregion
		
#endregion
	}
}
