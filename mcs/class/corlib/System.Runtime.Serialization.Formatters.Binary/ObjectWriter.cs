// ObjectWriter.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003 Lluis Sanchez Gual

// FIXME: Implement the missing binary elements

using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace System.Runtime.Serialization.Formatters.Binary
{
	abstract class TypeMetadata
	{
		public TypeMetadata (Type instanceType)
		{
			InstanceType = instanceType;
			TypeAssembly = instanceType.Assembly;
		}
		
		public Assembly TypeAssembly;
		public Type InstanceType;
		
		public abstract void WriteAssemblies (ObjectWriter ow, BinaryWriter writer);
		public abstract void WriteTypeData (ObjectWriter ow, BinaryWriter writer);
		public abstract void WriteObjectData (ObjectWriter ow, BinaryWriter writer, object data);
		
		public virtual bool IsCompatible (TypeMetadata other)
		{
			return true;
		}
	}
	
	class SerializableTypeMetadata: TypeMetadata
	{
		Type[] types;
		string[] names;
		
		public SerializableTypeMetadata (Type itype, SerializationInfo info): base (itype)
		{
			types = new Type [info.MemberCount];
			names = new string [info.MemberCount];

			SerializationInfoEnumerator e = info.GetEnumerator ();

			int n = 0;
			while (e.MoveNext ())
			{
				types[n] = e.ObjectType;
				names[n] = e.Name;
				n++;
			}

			if (info.FullTypeName != InstanceType.FullName || info.AssemblyName != TypeAssembly.FullName) 
			{
				TypeAssembly = Assembly.Load (info.AssemblyName);
				InstanceType = TypeAssembly.GetType (info.FullTypeName);
			}
		}
		
		public override bool IsCompatible (TypeMetadata other)
		{
			if (!(other is SerializableTypeMetadata)) return false;
			
			SerializableTypeMetadata tm = (SerializableTypeMetadata)other;
			if (types.Length != tm.types.Length) return false;
			if (TypeAssembly != tm.TypeAssembly) return false;
			if (InstanceType != tm.InstanceType) return false;
			for (int n=0; n<types.Length; n++)
			{
				if (types[n] != tm.types[n]) return false;
				if (names[n] != tm.names[n]) return false;
			}
			return true;
		}
		
		public override void WriteAssemblies (ObjectWriter ow, BinaryWriter writer)
		{
			foreach (Type mtype in types)
			{
				Type type = mtype;
				while (type.IsArray) 
					type = type.GetElementType();
					
				ow.WriteAssembly (writer, type.Assembly);
			}
		}
		
		public override void WriteTypeData (ObjectWriter ow, BinaryWriter writer)
		{
			writer.Write (types.Length);

			// Names of fields
			foreach (string name in names)
				writer.Write (name);

			// Types of fields
			foreach (Type type in types)
				ObjectWriter.WriteTypeCode (writer, type);

			// Type specs of fields
			foreach (Type type in types)
				ow.WriteTypeSpec (writer, type);
		}
		
		public override void WriteObjectData (ObjectWriter ow, BinaryWriter writer, object data)
		{
			SerializationInfo info = (SerializationInfo) data;
			SerializationInfoEnumerator e = info.GetEnumerator ();

			while (e.MoveNext ())
				ow.WriteValue (writer, e.ObjectType, e.Value);
		}
	}
	
	class MemberTypeMetadata: TypeMetadata
	{
		MemberInfo[] members;
		
		public MemberTypeMetadata (Type type, StreamingContext context): base (type)
		{
			members = FormatterServices.GetSerializableMembers (type, context);
		}

		public override void WriteAssemblies (ObjectWriter ow, BinaryWriter writer)
		{
			foreach (FieldInfo field in members)
			{
				Type type = field.FieldType;
				while (type.IsArray) 
					type = type.GetElementType();
					
				ow.WriteAssembly (writer, type.Assembly);
			}
		}
		
		public override void WriteTypeData (ObjectWriter ow, BinaryWriter writer)
		{
			writer.Write (members.Length);

			// Names of fields
			foreach (FieldInfo field in members)
				writer.Write (field.Name);

			// Types of fields
			foreach (FieldInfo field in members)
				ObjectWriter.WriteTypeCode (writer, field.FieldType);

			// Type specs of fields
			foreach (FieldInfo field in members)
				ow.WriteTypeSpec (writer, field.FieldType);
		}
		
		public override void WriteObjectData (ObjectWriter ow, BinaryWriter writer, object data)
		{
			object[] values = FormatterServices.GetObjectData (data, members);
			for (int n=0; n<values.Length; n++)
				ow.WriteValue (writer, ((FieldInfo)members[n]).FieldType, values[n]);
		}
	}
	
	internal class ObjectWriter
	{
		ObjectIDGenerator _idGenerator = new ObjectIDGenerator();
		Hashtable _cachedMetadata = new Hashtable();
		Queue _pendingObjects = new Queue();
		Hashtable _assemblyCache = new Hashtable();
		
		// Type metadata that can be shared with all serializers
		static Hashtable _cachedTypes = new Hashtable();

		internal static Assembly CorlibAssembly = typeof(string).Assembly;

		ISurrogateSelector _surrogateSelector;
		StreamingContext _context;
		FormatterAssemblyStyle _assemblyFormat;
		
		class MetadataReference
		{
			public TypeMetadata Metadata;
			public long ObjectID;
			
			public MetadataReference (TypeMetadata metadata, long id)
			{
				Metadata = metadata;
				ObjectID = id;
			}
		}
		
		public ObjectWriter (ISurrogateSelector surrogateSelector, StreamingContext context, FormatterAssemblyStyle assemblyFormat)
		{
			_surrogateSelector = surrogateSelector;
			_context = context;
			_assemblyFormat = assemblyFormat;
		}

		public void WriteObjectGraph (BinaryWriter writer, object obj, Header[] headers)
		{
			_pendingObjects.Clear();
			if (headers != null) QueueObject (headers);
			QueueObject (obj);
			WriteQueuedObjects (writer);
			WriteSerializationEnd (writer);
		}

		public void QueueObject (object obj)
		{
			_pendingObjects.Enqueue (obj);
		}

		public void WriteQueuedObjects (BinaryWriter writer)
		{

			while (_pendingObjects.Count > 0)
				WriteObjectInstance (writer, _pendingObjects.Dequeue(), false);
		}

		public void WriteObjectInstance (BinaryWriter writer, object obj, bool isValueObject)
		{
			bool firstTime;
			long id;

			// If the object is a value type (not boxed) then there is no need
			// to register it in the id generator, because it won't have other
			// references to it

			if (isValueObject) id = _idGenerator.NextId;
			else id = _idGenerator.GetId (obj, out firstTime);

			if (obj is string) {
				WriteString (writer, id, (string)obj);
			}
			else if (obj is Array) {
				WriteArray (writer, id, (Array)obj);
			}
			else
				WriteObject (writer, id, obj);
		}

		public static void WriteSerializationEnd (BinaryWriter writer)
		{
			writer.Write ((byte) BinaryElement.End);
		}

		private void WriteObject (BinaryWriter writer, long id, object obj)
		{
			object data;
			TypeMetadata metadata;

			GetObjectData (obj, out metadata, out data);
			MetadataReference metadataReference = (MetadataReference)_cachedMetadata [metadata.InstanceType];

			if (metadataReference != null && metadata.IsCompatible (metadataReference.Metadata))
			{
				// An object of the same type has already been serialized
				// It is not necessary to write again type metadata

				writer.Write ((byte) BinaryElement.RefTypeObject);
				writer.Write ((int)id);

				writer.Write ((int)metadataReference.ObjectID);
				metadata.WriteObjectData (this, writer, data);
				return;
			}

			if (metadataReference == null)
			{
				metadataReference = new MetadataReference (metadata, id);
				_cachedMetadata [metadata.InstanceType] = metadataReference;
			}

			BinaryElement objectTag;

			int assemblyId;
			if (metadata.TypeAssembly == CorlibAssembly)
			{
				// A corlib type
				objectTag = BinaryElement.RuntimeObject;
				assemblyId = -1;
			}
			else
			{
				objectTag = BinaryElement.ExternalObject;
				assemblyId = WriteAssembly (writer, metadata.TypeAssembly);
			}

			// Registers the assemblies needed for each field
			// If there are assemblies that where not registered before this object,
			// write them now

			metadata.WriteAssemblies (this, writer);

			// Writes the object

			writer.Write ((byte) objectTag);
			writer.Write ((int)id);
			writer.Write (metadata.InstanceType.FullName);
			
			metadata.WriteTypeData (this, writer);
			if (assemblyId != -1) writer.Write (assemblyId);
			
			metadata.WriteObjectData (this, writer, data);
		}

		private void GetObjectData (object obj, out TypeMetadata metadata, out object data)
		{
			Type instanceType = obj.GetType();

			// Check if the formatter has a surrogate selector – if it does, 
			// check if the surrogate selector handles objects of the given type. 

			if (_surrogateSelector != null)
			{
				ISurrogateSelector selector;
				ISerializationSurrogate surrogate = _surrogateSelector.GetSurrogate (instanceType, _context, out selector);
				if (surrogate != null)
				{
					SerializationInfo info = new SerializationInfo (instanceType, new FormatterConverter ());
					surrogate.GetObjectData (obj, info, _context);
					metadata = new SerializableTypeMetadata (instanceType, info);
					data = info;
					return;
				}
			}

			// Check if the object is marked with the Serializable attribute

			if (!instanceType.IsSerializable)
				throw new SerializationException ("Type " + instanceType +
								  " is not marked as Serializable " + 
								  "and does not implement ISerializable.");

			ISerializable ser = obj as ISerializable;

			if (ser != null) 
			{
				SerializationInfo info = new SerializationInfo (instanceType, new FormatterConverter ());
				ser.GetObjectData (info, _context);
				metadata = new SerializableTypeMetadata (instanceType, info);
				data = info;
			} 
			else 
			{
				data = obj;
				if (_context.Context != null)
				{
					// Don't cache metadata info when the Context property is not null sice
					// we can't control the number of possible contexts in this case
					metadata = new MemberTypeMetadata (instanceType, _context);
					return;
				}
				
				metadata = null;
				lock (_cachedTypes)
				{
					Hashtable typesTable = (Hashtable) _cachedTypes [_context.State];
					if (typesTable != null)
					{
						metadata = (TypeMetadata) typesTable [instanceType];
						if (metadata == null) 
						{
							metadata = CreateMemberTypeMetadata (instanceType);
							typesTable [instanceType] = metadata;
						}
					}
					else
					{
						metadata = CreateMemberTypeMetadata (instanceType);
						typesTable = new Hashtable ();
						typesTable [instanceType] = metadata;
						_cachedTypes [_context.State] = typesTable;
					}
				}
			}
		}
		
		TypeMetadata CreateMemberTypeMetadata (Type type)
		{
			// This environment variable is only for test and benchmarking pourposes.
			// By default, mono will always use IL generated class serializers.
			if (Environment.GetEnvironmentVariable("MONO_REFLECTION_SERIALIZER") == null) {
				Type metaType = CodeGenerator.GenerateMetadataType (type, _context);
				return (TypeMetadata) Activator.CreateInstance (metaType);
			}
			else
				return new MemberTypeMetadata (type, _context);
		}

		private void WriteArray (BinaryWriter writer, long id, Array array)
		{
			// There are 4 ways of serializing arrays:
			// The element GenericArray (7) can be used for all arrays.
			// The element ArrayOfPrimitiveType (15) can be used for single-dimensional
			// arrays of primitive types
			// The element ArrayOfObject (16) can be used for single-dimensional Object arrays
			// The element ArrayOfString (17) can be used for single-dimensional string arrays

			Type elementType = array.GetType().GetElementType();

			if (elementType == typeof (object) && array.Rank == 1) {
				WriteObjectArray (writer, id, array);
			}
			else if (elementType == typeof (string) && array.Rank == 1) {
				WriteStringArray (writer, id, array);
			}
			else if (BinaryCommon.IsPrimitive(elementType) && array.Rank == 1) {
				WritePrimitiveTypeArray (writer, id, array);
			}
			else
				WriteGenericArray (writer, id, array);
		}

		private void WriteGenericArray (BinaryWriter writer, long id, Array array)
		{
			Type elementType = array.GetType().GetElementType();

			// Registers and writes the assembly of the array element type if needed

			if (!elementType.IsArray)
				WriteAssembly (writer, elementType.Assembly);

			// Writes the array

			writer.Write ((byte) BinaryElement.GenericArray);
			writer.Write ((int)id);
			
			// Write the structure of the array

			if (elementType.IsArray) 
				writer.Write ((byte) ArrayStructure.Jagged);
			else if (array.Rank == 1)
				writer.Write ((byte) ArrayStructure.SingleDimensional);
			else
				writer.Write ((byte) ArrayStructure.MultiDimensional);

			// Write the number of dimensions and the length
			// of each dimension

			writer.Write (array.Rank);
			for (int n=0; n<array.Rank; n++)
				writer.Write (array.GetUpperBound (n) + 1);

			// Writes the type
			WriteTypeCode (writer, elementType);
			WriteTypeSpec (writer, elementType);

			// Writes the values. For single-dimension array, a special tag is used
			// to represent multiple consecutive null values. I don't know why this
			// optimization is not used for multidimensional arrays.

			if (array.Rank == 1 && !elementType.IsValueType)
			{
				WriteSingleDimensionArrayElements (writer, array, elementType);
			}
			else
			{
				int[] indices = new int[array.Rank];

				// Initialize indexes
				for (int dim = array.Rank-1; dim >= 0; dim--)
					indices[dim] = array.GetLowerBound (dim);

				bool end = false;
				while (!end)
				{
					WriteValue (writer, elementType, array.GetValue (indices));

					for (int dim = array.Rank-1; dim >= 0; dim--)
					{
						indices[dim]++;
						if (indices[dim] > array.GetUpperBound (dim))
						{
							if (dim > 0) {
								indices[dim] = array.GetLowerBound (dim);
								continue;	// Increment the next dimension's index
							}
							end = true;	// That was the last dimension. Finished.
						}
						break;
					}
				}
			}
		}

		private void WriteObjectArray (BinaryWriter writer, long id, Array array)
		{
			writer.Write ((byte) BinaryElement.ArrayOfObject);
			writer.Write ((int)id);
			writer.Write (array.Length);	// Single dimension. Just write the length
			WriteSingleDimensionArrayElements (writer, array, typeof (object));
		}

		private void WriteStringArray (BinaryWriter writer, long id, Array array)
		{
			writer.Write ((byte) BinaryElement.ArrayOfString);
			writer.Write ((int)id);
			writer.Write (array.Length);	// Single dimension. Just write the length
			WriteSingleDimensionArrayElements (writer, array, typeof (string));
		}

		private void WritePrimitiveTypeArray (BinaryWriter writer, long id, Array array)
		{
			writer.Write ((byte) BinaryElement.ArrayOfPrimitiveType);
			writer.Write ((int)id);
			writer.Write (array.Length);	// Single dimension. Just write the length

			Type elementType = array.GetType().GetElementType();
			WriteTypeSpec (writer, elementType);

			switch (Type.GetTypeCode (elementType))
			{
				case TypeCode.Boolean:
					foreach (bool item in (bool[]) array)
						writer.Write (item);
					break;

				case TypeCode.Byte:
					writer.Write ((byte[]) array);
					break;

				case TypeCode.Char:
					foreach (char item in (char[]) array)
						writer.Write (item);
					break;

				case TypeCode.DateTime: 
					foreach (DateTime item in (DateTime[]) array)
						writer.Write (item.Ticks);
					break;

				case TypeCode.Decimal:
					foreach (decimal item in (decimal[]) array)
						writer.Write (item);
					break;

				case TypeCode.Double:
					foreach (double item in (double[]) array)
						writer.Write (item);
					break;

				case TypeCode.Int16:
					foreach (short item in (short[]) array)
						writer.Write (item);
					break;

				case TypeCode.Int32:
					foreach (int item in (int[]) array)
						writer.Write (item);
					break;

				case TypeCode.Int64:
					foreach (long item in (long[]) array)
						writer.Write (item);
					break;

				case TypeCode.SByte:
					foreach (sbyte item in (sbyte[]) array)
						writer.Write (item);
					break;

				case TypeCode.Single:
					foreach (float item in (float[]) array)
						writer.Write (item);
					break;

				case TypeCode.UInt16:
					foreach (ushort item in (ushort[]) array)
						writer.Write (item);
					break;

				case TypeCode.UInt32:
					foreach (uint item in (uint[]) array)
						writer.Write (item);
					break;

				case TypeCode.UInt64:
					foreach (ulong item in (ulong[]) array)
						writer.Write (item);
					break;

				case TypeCode.String:
					foreach (string item in (string[]) array)
						writer.Write (item);
					break;

				default:
					if (elementType == typeof (TimeSpan)) {
						foreach (TimeSpan item in (TimeSpan[]) array)
							writer.Write (item.Ticks);
					}
					else
						throw new NotSupportedException ("Unsupported primitive type: " + elementType.FullName);
					break;
			}			
		}

		private void WriteSingleDimensionArrayElements (BinaryWriter writer, Array array, Type elementType)
		{
			int numNulls = 0;
			for (int n = array.GetLowerBound (0); n<=array.GetUpperBound(0); n++)
			{
				object val = array.GetValue (n);
				if (val != null && numNulls > 0)
				{
					WriteNullFiller (writer, numNulls);
					WriteValue (writer, elementType, val);
					numNulls = 0;
				}
				else if (val == null)
					numNulls++;
				else
					WriteValue (writer, elementType, val);
			}
			if (numNulls > 0)
				WriteNullFiller (writer, numNulls);
		}

		private void WriteNullFiller (BinaryWriter writer, int numNulls)
		{
			if (numNulls == 1) {
				writer.Write ((byte) BinaryElement.NullValue);
			}
			else if (numNulls == 2) {
				writer.Write ((byte) BinaryElement.NullValue);
				writer.Write ((byte) BinaryElement.NullValue);
			}
			else if (numNulls <= byte.MaxValue) {
				writer.Write ((byte) BinaryElement.ArrayFiller8b);
				writer.Write ((byte) numNulls);
			}
			else {
				writer.Write ((byte) BinaryElement.ArrayFiller32b);
				writer.Write (numNulls);
			}
		}

		private void WriteObjectReference (BinaryWriter writer, long id)
		{

			writer.Write ((byte) BinaryElement.ObjectReference);
			writer.Write ((int)id);
		}

		public void WriteValue (BinaryWriter writer, Type valueType, object val)
		{
			if (val == null) 
			{
				writer.Write ((byte) BinaryElement.NullValue);
			}
			else if (BinaryCommon.IsPrimitive(val.GetType()))
			{
				if (!BinaryCommon.IsPrimitive(valueType))
				{
					// It is a boxed primitive type value
					writer.Write ((byte) BinaryElement.BoxedPrimitiveTypeValue);
					WriteTypeSpec (writer, val.GetType());
				}
				WritePrimitiveValue (writer, val);
			}
			else if (valueType.IsValueType)
			{
				// Value types are written embedded in the containing object
				WriteObjectInstance (writer, val, true);
			}
			else if (val is string)
			{
				// Strings are written embedded, unless already registered
				bool firstTime;
				long id = _idGenerator.GetId (val, out firstTime);

				if (firstTime) WriteObjectInstance (writer, val, false);
				else WriteObjectReference (writer, id);
			}			
			else
			{
				// It is a reference type. Write a forward reference and queue the
				// object to the pending object list (unless already written).

				bool firstTime;
				long id = _idGenerator.GetId (val, out firstTime);

				if (firstTime) _pendingObjects.Enqueue (val);
				WriteObjectReference (writer, id);
			}
		}
		
		private void WriteString (BinaryWriter writer, long id, string str)
		{
			writer.Write ((byte) BinaryElement.String);
			writer.Write ((int)id);
			writer.Write (str);
		}

		public int WriteAssembly (BinaryWriter writer, Assembly assembly)
		{
			if (assembly == ObjectWriter.CorlibAssembly) return -1;
			
			bool firstTime;
			int id = RegisterAssembly (assembly, out firstTime);
			if (!firstTime) return id;
					
			writer.Write ((byte) BinaryElement.Assembly);
			writer.Write (id);
			if (_assemblyFormat == FormatterAssemblyStyle.Full)
				writer.Write (assembly.GetName ().FullName);
			else
				writer.Write (assembly.GetName ().Name);
				
			return id;
		}

		public int GetAssemblyId (Assembly assembly)
		{
			return (int)_assemblyCache[assembly];
		}

		private int RegisterAssembly (Assembly assembly, out bool firstTime)
		{
			if (_assemblyCache.ContainsKey (assembly))
			{
				firstTime = false;
				return (int)_assemblyCache[assembly];
			}
			else
			{
				int id = (int)_idGenerator.GetId (0, out firstTime);
				_assemblyCache.Add (assembly, id);
				return id;
			}
		}

		public static void WritePrimitiveValue (BinaryWriter writer, object value)
		{
			Type type = value.GetType();

			switch (Type.GetTypeCode (type))
			{
				case TypeCode.Boolean:
					writer.Write ((bool)value);
					break;

				case TypeCode.Byte:
					writer.Write ((byte) value);
					break;

				case TypeCode.Char:
					writer.Write ((char) value);
					break;

				case TypeCode.DateTime: 
					writer.Write ( ((DateTime)value).Ticks);
					break;

				case TypeCode.Decimal:
					writer.Write ((decimal) value);
					break;

				case TypeCode.Double:
					writer.Write ((double) value);
					break;

				case TypeCode.Int16:
					writer.Write ((short) value);
					break;

				case TypeCode.Int32:
					writer.Write ((int) value);
					break;

				case TypeCode.Int64:
					writer.Write ((long) value);
					break;

				case TypeCode.SByte:
					writer.Write ((sbyte) value);
					break;

				case TypeCode.Single:
					writer.Write ((float) value);
					break;

				case TypeCode.UInt16:
					writer.Write ((ushort) value);
					break;

				case TypeCode.UInt32:
					writer.Write ((uint) value);
					break;

				case TypeCode.UInt64:
					writer.Write ((ulong) value);
					break;

				case TypeCode.String:
					writer.Write ((string) value);
					break;

				default:
					if (type == typeof (TimeSpan))
						writer.Write (((TimeSpan)value).Ticks);
					else
						throw new NotSupportedException ("Unsupported primitive type: " + value.GetType().FullName);
					break;
			}
		}

		public static void WriteTypeCode (BinaryWriter writer, Type type)
		{
			writer.Write ((byte) GetTypeTag (type));
		}

		public static TypeTag GetTypeTag (Type type)
		{
			if (type == typeof (string)) {
				return TypeTag.String;
			}
			else if (BinaryCommon.IsPrimitive (type)) {
				return TypeTag.PrimitiveType;
			}
			else if (type == typeof (object)) {
				return TypeTag.ObjectType;
			}
			else if (type.IsArray && type.GetArrayRank() == 1 && type.GetElementType() == typeof (object)) {
				return TypeTag.ArrayOfObject; 
			}
			else if (type.IsArray && type.GetArrayRank() == 1 && type.GetElementType() == typeof (string)){
				return TypeTag.ArrayOfString;
			}
			else if (type.IsArray && type.GetArrayRank() == 1 && BinaryCommon.IsPrimitive(type.GetElementType())) {
				return TypeTag.ArrayOfPrimitiveType;
			}
			else if (type.Assembly == CorlibAssembly) {
				return TypeTag.RuntimeType;
			}
			else
				return TypeTag.GenericType;
		}

		public void WriteTypeSpec (BinaryWriter writer, Type type)
		{
			// WARNING Keep in sync with EmitWriteTypeSpec
			
			switch (GetTypeTag (type))
			{
				case TypeTag.PrimitiveType:
					writer.Write (BinaryCommon.GetTypeCode (type));
					break;

				case TypeTag.RuntimeType:
					writer.Write (type.FullName);
					break;

				case TypeTag.GenericType:
					writer.Write (type.FullName);
					writer.Write ((int)GetAssemblyId (type.Assembly));
					break;

				case TypeTag.ArrayOfPrimitiveType:
					writer.Write (BinaryCommon.GetTypeCode (type.GetElementType()));
					break;

				default:
					// Type spec not needed
					break;
			}
		}
	}
}
