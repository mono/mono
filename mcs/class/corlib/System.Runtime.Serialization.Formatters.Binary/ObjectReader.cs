// ObjectReader.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003 Lluis Sanchez Gual

// FIXME: Implement the missing binary elements

using System;
using System.Runtime.Serialization;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal class ObjectReader
	{
		ISurrogateSelector _surrogateSelector;
		StreamingContext _context;

		ObjectManager _manager;
		Hashtable _registeredAssemblies = new Hashtable();
		Hashtable _typeMetadataCache = new Hashtable();

		object _lastObject = null;

		class TypeMetadata
		{
			public Type Type;
			public Type[] MemberTypes;
			public string[] MemberNames;
			public int FieldCount;
			public bool NeedsSerializationInfo;
		}

		class ArrayNullFiller
		{
			public ArrayNullFiller(int count) { NullCount = count; }
			public int NullCount;
		}

		public ObjectReader(ISurrogateSelector surrogateSelector, StreamingContext context)
		{
			_manager = new ObjectManager (surrogateSelector, context);
			_surrogateSelector = surrogateSelector;
			_context = context;
		}

		public object ReadObjectGraph (BinaryReader reader, bool readHeaders, HeaderHandler headerHandler)
		{
			object rootObject = null;
			Header[] headers = null;

			// Reads the objects. The first object in the stream is the
			// root object.

			while (ReadNextObject (reader))
			{
				if (readHeaders && (headers == null))
					headers = (Header[])CurrentObject;
				else
					if (rootObject == null) rootObject = CurrentObject;
			}

			if (readHeaders && headerHandler != null)
				headerHandler (headers);

			return rootObject;
		}

		public bool ReadNextObject (BinaryReader reader)
		{
			BinaryElement element = (BinaryElement)reader.ReadByte ();
			if (element == BinaryElement.End)
			{
				_manager.DoFixups();
				return false;
			}

			SerializationInfo info;
			long objectId;

			ReadObject (element, reader, out objectId, out _lastObject, out info);

			if (objectId != 0) 
				RegisterObject (objectId, _lastObject, info, 0, null, null);

			return true;
		}

		public object CurrentObject
		{
			get { return _lastObject; }
		}

		// Reads an object from the stream. The object is registered in the ObjectManager.
		// The result can be either the object instance
		// or the id of the object (when what is found in the stream is an object reference).
		// If an object instance is read, the objectId is set to 0.
		
		private void ReadObject (BinaryElement element, BinaryReader reader, out long objectId, out object value, out SerializationInfo info)
		{
			switch (element)
			{
				case BinaryElement.RefTypeObject:
					ReadRefTypeObjectInstance (reader, out objectId, out value, out info);
					break;

				case BinaryElement.RuntimeObject:
					ReadObjectInstance (reader, true, out objectId, out value, out info);
					break;

				case BinaryElement.ExternalObject:
					ReadObjectInstance (reader, false, out objectId, out value, out info);
					break;

				case BinaryElement.String:
					info = null;
					ReadStringIntance (reader, out objectId, out value);
					break;

				case BinaryElement.GenericArray:
					info = null;
					ReadGenericArray (reader, out objectId, out value);
					break;

				case BinaryElement.BoxedPrimitiveTypeValue:
					value = ReadBoxedPrimitiveTypeValue (reader);
					objectId = 0;
					info = null;
					break;

				case BinaryElement.NullValue:
					value = null;
					objectId = 0;
					info = null;
					break;

				case BinaryElement.Assembly:
					ReadAssembly (reader);
					ReadObject ((BinaryElement)reader.ReadByte (), reader, out objectId, out value, out info);
					break;

				case BinaryElement.ArrayFiller8b:
					value = new ArrayNullFiller(reader.ReadByte());
					objectId = 0;
					info = null;
					break;

				case BinaryElement.ArrayFiller32b:
					value = new ArrayNullFiller(reader.ReadInt32());
					objectId = 0;
					info = null;
					break;

				case BinaryElement.ArrayOfPrimitiveType:
					ReadArrayOfPrimitiveType (reader, out objectId, out value);
					info = null;
					break;

				case BinaryElement.ArrayOfObject:
					ReadArrayOfObject (reader, out objectId, out value);
					info = null;
					break;

				case BinaryElement.ArrayOfString:
					ReadArrayOfString (reader, out objectId, out value);
					info = null;
					break;

				default:
					throw new SerializationException ("Unexpected binary element: " + (int)element);
			}
		}

		private void ReadAssembly (BinaryReader reader)
		{
			long id = (long) reader.ReadUInt32 ();
			string assemblyName = reader.ReadString ();
			Assembly assembly = Assembly.Load (assemblyName);
			_registeredAssemblies [id] = assembly;
		}

		private void ReadObjectInstance (BinaryReader reader, bool isRuntimeObject, out long objectId, out object value, out SerializationInfo info)
		{
			objectId = (long) reader.ReadUInt32 ();

			TypeMetadata metadata = ReadTypeMetadata (reader, isRuntimeObject);
			ReadObjectContent (reader, metadata, objectId, out value, out info);
		}

		private void ReadRefTypeObjectInstance (BinaryReader reader, out long objectId, out object value, out SerializationInfo info)
		{
			objectId = (long) reader.ReadUInt32 ();
			long refTypeObjectId = (long) reader.ReadUInt32 ();

			// Gets the type of the referred object and its metadata

			object refObj = _manager.GetObject (refTypeObjectId);
			if (refObj == null) throw new SerializationException ("Invalid binary format");
			TypeMetadata metadata = (TypeMetadata)_typeMetadataCache [refObj.GetType()];

			ReadObjectContent (reader, metadata, objectId, out value, out info);
		}

		private void ReadObjectContent (BinaryReader reader, TypeMetadata metadata, long objectId, out object objectInstance, out SerializationInfo info)
		{
			objectInstance = FormatterServices.GetUninitializedObject (metadata.Type);
			info = metadata.NeedsSerializationInfo ? new SerializationInfo(metadata.Type, new FormatterConverter()) : null;

			for (int n=0; n<metadata.FieldCount; n++)
				ReadValue (reader, objectInstance, objectId, info, metadata.MemberTypes[n], metadata.MemberNames[n], null);
		}

		private void RegisterObject (long objectId, object objectInstance, SerializationInfo info, long parentObjectId, MemberInfo parentObjectMemeber, int[] indices)
		{
			if (!objectInstance.GetType().IsValueType || parentObjectId == 0)
				_manager.RegisterObject (objectInstance, objectId, info, 0, null, indices);
			else
				_manager.RegisterObject (objectInstance, objectId, info, parentObjectId, parentObjectMemeber, indices);
		}

		private void ReadStringIntance (BinaryReader reader, out long objectId, out object value)
		{
			objectId = (long) reader.ReadUInt32 ();
			value = reader.ReadString ();
		}

		private void ReadGenericArray (BinaryReader reader, out long objectId, out object val)
		{
			objectId = (long) reader.ReadUInt32 ();
			ArrayStructure structure = (ArrayStructure) reader.ReadByte();

			int rank = reader.ReadInt32();

			bool emptyDim = false;
			int[] lengths = new int[rank];
			for (int n=0; n<rank; n++)
			{
				lengths[n] = reader.ReadInt32();
				if (lengths[n] == 0) emptyDim = true;
			}

			TypeTag code = (TypeTag) reader.ReadByte ();
			Type elementType = ReadType (reader, code);

			Array array = Array.CreateInstance (elementType, lengths);

			if (emptyDim) 
			{ 
				val = array;
				return;
			}

			int[] indices = new int[rank];

			// Initialize indexes
			for (int dim = rank-1; dim >= 0; dim--)
				indices[dim] = array.GetLowerBound (dim);

			bool end = false;
			while (!end)
			{
				ReadValue (reader, array, objectId, null, elementType, null, indices);

				for (int dim = array.Rank-1; dim >= 0; dim--)
				{
					indices[dim]++;
					if (indices[dim] > array.GetUpperBound (dim))
					{
						if (dim > 0) 
						{
							indices[dim] = array.GetLowerBound (dim);
							continue;	// Increment the next dimension's index
						}
						end = true;	// That was the last dimension. Finished.
					}
					break;
				}
			}
			val = array;
		}

		private object ReadBoxedPrimitiveTypeValue (BinaryReader reader)
		{
			Type type = ReadType (reader, TypeTag.PrimitiveType);
			return ReadPrimitiveTypeValue (reader, type);
		}

		private void ReadArrayOfPrimitiveType (BinaryReader reader, out long objectId, out object val)
		{
			objectId = (long) reader.ReadUInt32 ();
			int length = reader.ReadInt32 ();
			Type elementType = ReadType (reader, TypeTag.PrimitiveType);

			Array array = Array.CreateInstance (elementType, length);
			for (int n = 0; n < length; n++)
				array.SetValue (ReadPrimitiveTypeValue (reader, elementType), n);

			val = array;
		}

		private void ReadArrayOfObject (BinaryReader reader, out long objectId, out object array)
		{
			ReadSimpleArray (reader, typeof (object), out objectId, out array);
		}
		
		private void ReadArrayOfString (BinaryReader reader, out long objectId, out object array)
		{
			ReadSimpleArray (reader, typeof (string), out objectId, out array);
		}

		private void ReadSimpleArray (BinaryReader reader, Type elementType, out long objectId, out object val)
		{
			objectId = (long) reader.ReadUInt32 ();
			int length = reader.ReadInt32 ();
			int[] indices = new int[1];

			Array array = Array.CreateInstance (elementType, length);
			for (int n = 0; n < length; n++)
			{
				indices[0] = n;
				ReadValue (reader, array, objectId, null, elementType, null, indices);
				n = indices[0];
			}
			val = array;
		}

		private TypeMetadata ReadTypeMetadata (BinaryReader reader, bool isRuntimeObject)
		{
			TypeMetadata metadata = new TypeMetadata();

			string className = reader.ReadString ();
			int fieldCount = reader.ReadInt32 ();

			Type[] types = new Type[fieldCount];
			string[] names = new string[fieldCount];

			TypeTag[] codes = new TypeTag[fieldCount];

			for (int n=0; n<fieldCount; n++)
				names [n] = reader.ReadString ();

			for (int n=0; n<fieldCount; n++)
				codes [n] = (TypeTag) reader.ReadByte ();

			for (int n=0; n<fieldCount; n++)
				types [n] = ReadType (reader, codes[n]);

			// Gets the type

			if (!isRuntimeObject) 
			{
				long assemblyId = (long)reader.ReadUInt32();
				Assembly asm = (Assembly)_registeredAssemblies[assemblyId];
				metadata.Type = asm.GetType (className, true);
			}
			else
				metadata.Type = Type.GetType (className, true);

			metadata.MemberTypes = types;
			metadata.MemberNames = names;
			metadata.FieldCount = names.Length;

			// Now check if this objects needs a SerializationInfo struct for deserialziation.
			// SerializationInfo is needed if the object has to be deserialized using
			// a serialization surrogate, or if it implements ISerializable.

			if (_surrogateSelector != null)
			{
				// check if the surrogate selector handles objects of the given type. 
				ISurrogateSelector selector;
				ISerializationSurrogate surrogate = _surrogateSelector.GetSurrogate (metadata.Type, _context, out selector);
				metadata.NeedsSerializationInfo = (surrogate != null);
			}

			if (!metadata.NeedsSerializationInfo)
			{
				// Check if the object is marked with the Serializable attribute

				if (!metadata.Type.IsSerializable)
					throw new SerializationException("Serializable objects must be marked with the Serializable attribute");

				metadata.NeedsSerializationInfo = (metadata.Type.GetInterface ("ISerializable") != null);
			}

			// Registers the type's metadata so it can be reused later if
			// a RefTypeObject element is found

			if (!_typeMetadataCache.ContainsKey (metadata.Type))
				_typeMetadataCache [metadata.Type] = metadata;

			return metadata;
		}


		private void ReadValue (BinaryReader reader, object parentObject, long parentObjectId, SerializationInfo info, Type valueType, string fieldName, int[] indices)
		{
			// Reads a value from the stream and assigns it to the member of an object

			object val;

			if (BinaryCommon.IsPrimitive (valueType))
			{
				val = ReadPrimitiveTypeValue (reader, valueType);
				SetObjectValue (parentObject, fieldName, info, val, valueType, indices);
				return;
			}

			// Gets the object

			BinaryElement element = (BinaryElement)reader.ReadByte ();

			if (element == BinaryElement.ObjectReference)
			{
				// Just read the id of the referred object and record a fixup
				long childObjectId = (long) reader.ReadUInt32();
				RecordFixup (parentObjectId, childObjectId, parentObject, info, fieldName, indices);
				return;
			}

			long objectId;
			SerializationInfo objectInfo;

			ReadObject (element, reader, out objectId, out val, out objectInfo);

			// There are two cases where the object cannot be assigned to the parent
			// and a fixup must be used:
			//  1) When what has been read is not an object, but an id of an object that
			//     has not been read yet (an object reference). This is managed in the
			//     previous block of code.
			//  2) When the read object is a value type object. Value type fields hold
			//     copies of objects, not references. Thus, if the value object that
			//     has been read has pending fixups, those fixups would be made to the
			//     boxed copy in the ObjectManager, and not in the required object instance

			// First of all register the fixup, and then the object. ObjectManager is more
			// efficient if done in this order

			bool hasFixup = false;
			if (objectId != 0)
			{
				if (val.GetType().IsValueType)
				{
					RecordFixup (parentObjectId, objectId, parentObject, info, fieldName, indices);
					hasFixup = true;
				}

				// Register the value

				if (info == null && !parentObject.GetType().IsArray)
					RegisterObject (objectId, val, objectInfo, parentObjectId, GetObjectMember(parentObject, fieldName), null);
				else
					RegisterObject (objectId, val, objectInfo, parentObjectId, null, indices);
			}
			// Assign the value to the parent object, unless there is a fixup
			
			if (!hasFixup) 
				SetObjectValue (parentObject, fieldName, info, val, valueType, indices);
		}

		private void SetObjectValue (object parentObject, string fieldName, SerializationInfo info, object value, Type valueType, int[] indices)
		{
			if (value is IObjectReference)
				value = ((IObjectReference)value).GetRealObject (_context);

			if (parentObject.GetType().IsArray) 
			{
				if (value is ArrayNullFiller) 
				{
					// It must be a single dimension array of objects.
					// Just increase the index. Elements are null by default.
					int count = ((ArrayNullFiller)value).NullCount;
					indices[0] += count - 1;
				}
				else
					((Array)parentObject).SetValue (value, indices);
			}
			else if (info != null) {
				info.AddValue (fieldName, value, valueType);
			}
			else {
				MemberInfo member = GetObjectMember(parentObject, fieldName);
				if (member is FieldInfo)
					((FieldInfo)member).SetValue (parentObject, value);
				else
					((PropertyInfo)member).SetValue (parentObject, value, null);
			}
		}

		private void RecordFixup (long parentObjectId, long childObjectId, object parentObject, SerializationInfo info, string fieldName, int[] indices)
		{
			if (info != null) {
				_manager.RecordDelayedFixup (parentObjectId, fieldName, childObjectId);
			}
			else if (parentObject.GetType().IsArray) {
				if (indices.Length == 1)
					_manager.RecordArrayElementFixup (parentObjectId, indices[0], childObjectId);
				else
					_manager.RecordArrayElementFixup (parentObjectId, (int[])indices.Clone(), childObjectId);
			}
			else {
				_manager.RecordFixup (parentObjectId, GetObjectMember(parentObject, fieldName), childObjectId);
			}
		}

		private MemberInfo GetObjectMember (object parentObject, string fieldName)
		{
			MemberInfo[] members = parentObject.GetType().GetMember (fieldName, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (members.Length > 1) throw new SerializationException ("There are two public members named \"" + fieldName + "\" in the class hirearchy of " + parentObject.GetType().FullName);
			if (members.Length == 0) throw new SerializationException ("Field \"" + fieldName + "\" not found in class " + parentObject.GetType().FullName);
			return members[0];
		}

		public Type ReadType (BinaryReader reader, TypeTag code)
		{
			switch (code)
			{
				case TypeTag.PrimitiveType:
					return BinaryCommon.GetTypeFromCode (reader.ReadByte());

				case TypeTag.String:
					return typeof(string);

				case TypeTag.ObjectType:
					return typeof(object);

				case TypeTag.RuntimeType:
				{
					string name = reader.ReadString ();
					return Type.GetType (name, true);
				}

				case TypeTag.GenericType:
				{
					string name = reader.ReadString ();
					long asmid = (long) reader.ReadUInt32();
					Assembly asm = (Assembly)_registeredAssemblies[asmid];
					return asm.GetType (name, true);
				}

				case TypeTag.ArrayOfObject:
					return typeof(object[]);

				case TypeTag.ArrayOfString:
					return typeof(string[]);

				case TypeTag.ArrayOfPrimitiveType:
					Type elementType = BinaryCommon.GetTypeFromCode (reader.ReadByte());
					return Type.GetType(elementType.FullName + "[]");

				default:
					throw new NotSupportedException ("Unknow type tag");
			}
		}
		
		public static object ReadPrimitiveTypeValue (BinaryReader reader, Type type)
		{
			if (type == null) return null;

			switch (Type.GetTypeCode (type))
			{
				case TypeCode.Boolean:
					return reader.ReadBoolean();

				case TypeCode.Byte:
					return reader.ReadByte();

				case TypeCode.Char:
					return reader.ReadChar();

				case TypeCode.DateTime: 
					long ticks = reader.ReadInt64();
					return new DateTime (ticks);

				case TypeCode.Decimal:
					return reader.ReadDecimal();

				case TypeCode.Double:
					return reader.ReadDouble();

				case TypeCode.Int16:
					return reader.ReadInt16();

				case TypeCode.Int32:
					return reader.ReadInt32();

				case TypeCode.Int64:
					return reader.ReadInt64();

				case TypeCode.SByte:
					return reader.ReadSByte();

				case TypeCode.Single:
					return reader.ReadSingle();

				case TypeCode.UInt16:
					return reader.ReadUInt16();

				case TypeCode.UInt32:
					return reader.ReadUInt32();

				case TypeCode.UInt64:
					return reader.ReadUInt64();

				case TypeCode.String:
					return reader.ReadString();

				default:
					throw new NotSupportedException ("Unsupported primitive type: " + type.FullName);
			}
		}
	}
}
