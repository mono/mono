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
	internal class ObjectWriter
	{
		ObjectIDGenerator _idGenerator = new ObjectIDGenerator();
		Hashtable _cachedTypes = new Hashtable();
		Queue _pendingObjects = new Queue();
		Hashtable _assemblyCache = new Hashtable();

		static Assembly _corlibAssembly = typeof(string).Assembly;

		ISurrogateSelector _surrogateSelector;
		StreamingContext _context;

		class TypeMetadata
		{
			public Type[] Types;
			public string[] Names;
			public Assembly TypeAssembly;
			public Type InstanceType;
			public long ObjectID;
			public bool CustomSerialization;

			public bool Equals (TypeMetadata other)
			{
				if (!CustomSerialization) return true;

				TypeMetadata tm = (TypeMetadata)other;
				if (Types.Length != tm.Types.Length) return false;
				if (TypeAssembly != other.TypeAssembly) return false;
				if (InstanceType != other.InstanceType) return false;
				for (int n=0; n<Types.Length; n++)
				{
					if (Types[n] != tm.Types[n]) return false;
					if (Names[n] != tm.Names[n]) return false;
				}
				return true;
			}
		}

		public ObjectWriter(ISurrogateSelector surrogateSelector, StreamingContext context)
		{
			_surrogateSelector = surrogateSelector;
			_context = context;
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

			if (obj.GetType() == typeof(string)) {
				WriteString (writer, id, (string)obj);
			}
			else if (obj.GetType().IsArray) {
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
			object[] values;
			TypeMetadata metadata;

			GetObjectData (obj, out metadata, out values);

			TypeMetadata chachedMetadata = (TypeMetadata)_cachedTypes[metadata.InstanceType];

			if (chachedMetadata != null && metadata.Equals(chachedMetadata))
			{
				// An object of the same type has already been serialized
				// It is not necessary to write again type metadata

				writer.Write ((byte) BinaryElement.RefTypeObject);
				writer.Write ((int)id);

				// Get the id of the object that has the same type as this
				long refId = chachedMetadata.ObjectID;

				writer.Write ((int)refId);
				WriteObjectContent (writer, metadata.Types, values);
				return;
			}

			if (chachedMetadata == null)
			{
				metadata.ObjectID = id;
				_cachedTypes [metadata.InstanceType] = metadata;
			}

			BinaryElement objectTag;

			int assemblyId;
			if (metadata.TypeAssembly == _corlibAssembly)
			{
				// A corlib type
				objectTag = BinaryElement.RuntimeObject;
				assemblyId = -1;
			}
			else
			{
				objectTag = BinaryElement.ExternalObject;

				bool firstTime;
				assemblyId = RegisterAssembly (metadata.TypeAssembly, out firstTime);
				if (firstTime) WriteAssembly (writer, assemblyId, metadata.TypeAssembly);
			}

			// Registers the assemblies needed for each field
			// If there are assemblies that where not registered before this object,
			// write them now

			foreach (object value in values)
			{
				if (value == null) continue;

				Type memberType = value.GetType();
				while (memberType.IsArray) 
					memberType = memberType.GetElementType();

				if (memberType.Assembly != _corlibAssembly)
				{
					bool firstTime;
					int aid = RegisterAssembly (memberType.Assembly, out firstTime);
					if (firstTime) WriteAssembly (writer, aid, memberType.Assembly);
				}
			}

			// Writes the object

			writer.Write ((byte) objectTag);
			writer.Write ((int)id);
			writer.Write (metadata.InstanceType.FullName);
			WriteObjectMetadata (writer, metadata, assemblyId);
			WriteObjectContent (writer, metadata.Types, values);
		}

		private void WriteObjectMetadata (BinaryWriter writer, TypeMetadata metadata, int assemblyId)
		{
			Type[] types = metadata.Types;
			string[] names = metadata.Names;

			writer.Write (types.Length);

			// Names of fields
			foreach (string name in names)
				writer.Write (name);

			// Types of fields
			foreach (Type type in types)
				WriteTypeCode (writer, type);

			// Type specs of fields
			foreach (Type type in types)
				WriteTypeSpec (writer, type);

			if (assemblyId != -1) writer.Write (assemblyId);
		}

		private void WriteObjectContent (BinaryWriter writer, Type[] types, object[] values)
		{
			for (int n=0; n<values.Length; n++)
				WriteValue (writer, types[n], values[n]);
		}

		private void GetObjectData (object obj, out TypeMetadata metadata, out object[] values)
		{
			metadata = new TypeMetadata();
			metadata.InstanceType = obj.GetType();
			metadata.TypeAssembly = metadata.InstanceType.Assembly;

			// Check if the formatter has a surrogate selector – if it does, 
			// check if the surrogate selector handles objects of the given type. 

			if (_surrogateSelector != null)
			{
				ISurrogateSelector selector;
				ISerializationSurrogate surrogate = _surrogateSelector.GetSurrogate (metadata.InstanceType, _context, out selector);
				if (surrogate != null)
				{
					SerializationInfo info = new SerializationInfo (metadata.InstanceType, new FormatterConverter ());
					surrogate.GetObjectData(obj, info, _context);
					GetDataFromSerializationInfo (info, ref metadata, out values);
					return;
				}
			}

			// Check if the object is marked with the Serializable attribute

			if (!metadata.InstanceType.IsSerializable)
				throw new SerializationException ("Type " + metadata.InstanceType +
								  " is not marked as Serializable " + 
								  "and does not implement ISerializable.");

			ISerializable ser = obj as ISerializable;

			if (ser != null) 
			{
				SerializationInfo info = new SerializationInfo (metadata.InstanceType, new FormatterConverter ());
				ser.GetObjectData (info, _context);
				GetDataFromSerializationInfo (info, ref metadata, out values);
			} 
			else 
				GetDataFromObjectFields (obj, ref metadata, out values);
		}

		private void GetDataFromSerializationInfo (SerializationInfo info, ref TypeMetadata metadata, out object[] values)
		{
			Type[] types = types = new Type [info.MemberCount];
			string[] names = new string [info.MemberCount];
			values = new object [info.MemberCount];

			SerializationInfoEnumerator e = info.GetEnumerator ();

			int n = 0;
			while (e.MoveNext ())
			{
				values[n] = e.Value;
				types[n] = e.ObjectType;
				names[n] = e.Name;
				n++;
			}

			if (info.FullTypeName != metadata.InstanceType.FullName || info.AssemblyName != metadata.TypeAssembly.FullName) 
			{
				metadata.TypeAssembly = Assembly.Load (info.AssemblyName);
				metadata.InstanceType = metadata.TypeAssembly.GetType (info.FullTypeName);
			}

			metadata.Types = types;
			metadata.Names = names;
			metadata.CustomSerialization = true;
		}

		private void GetDataFromObjectFields (object obj, ref TypeMetadata metadata, out object[] values)
		{
			MemberInfo[] members = FormatterServices.GetSerializableMembers (obj.GetType(), _context);
			values = FormatterServices.GetObjectData (obj, members);

			Type[] types = new Type [members.Length];
			string[] names = new string [members.Length];

			for (int n=0; n<members.Length; n++)
			{
				MemberInfo member = members[n];
				names[n] = member.Name;
				if (member is FieldInfo)
					types[n] = ((FieldInfo)member).FieldType;
				else if (member is PropertyInfo)
					types[n] = ((PropertyInfo)member).PropertyType;
			}

			metadata.Types = types;
			metadata.Names = names;

			metadata.CustomSerialization = false;
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

			if (!elementType.IsArray && elementType.Assembly != _corlibAssembly)
			{
				bool firstTime;
				int aid = RegisterAssembly (elementType.Assembly, out firstTime);
				if (firstTime) WriteAssembly (writer, aid, elementType.Assembly);
			}

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

			for (int n=0; n<array.Length; n++)
				WritePrimitiveValue (writer, array.GetValue (n));
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

		private void WriteValue (BinaryWriter writer, Type valueType, object val)
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
			else if (val.GetType() == typeof(string))
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

		private void WriteAssembly (BinaryWriter writer, int id, Assembly assembly)
		{
			writer.Write ((byte) BinaryElement.Assembly);
			writer.Write (id);
			writer.Write (assembly.GetName ().FullName);
		}

		private int GetAssemblyId (Assembly assembly)
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
			else if (type.Assembly == _corlibAssembly) {
				return TypeTag.RuntimeType;
			}
			else
				return TypeTag.GenericType;
		}

		public void WriteTypeSpec (BinaryWriter writer, Type type)
		{
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
