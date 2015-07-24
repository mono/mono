using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace System.Runtime.Serialization
{
	internal partial class XmlFormatReaderGenerator
	{
		partial class CriticalHelper
		{
			internal XmlFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
			{
				return (XmlReaderDelegator xr, XmlObjectSerializerReadContext ctx, XmlDictionaryString [] memberNames, XmlDictionaryString [] memberNamespaces) => new XmlFormatReaderInterpreter (classContract).ReadFromXml (xr, ctx, memberNames, memberNamespaces);
			}

			internal XmlFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
			{
				return (XmlReaderDelegator xr, XmlObjectSerializerReadContext ctx, XmlDictionaryString inm, XmlDictionaryString ins, CollectionDataContract cc) => new XmlFormatReaderInterpreter (collectionContract, false).ReadCollectionFromXml (xr, ctx, inm, ins, cc);
			}
			
			internal XmlFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
			{
				return (XmlReaderDelegator xr, XmlObjectSerializerReadContext ctx, XmlDictionaryString inm, XmlDictionaryString ins, CollectionDataContract cc) => new XmlFormatReaderInterpreter (collectionContract, true).ReadGetOnlyCollectionFromXml (xr, ctx, inm, ins, cc);
			}
		}
	}

	class XmlFormatReaderInterpreter
	{
		public XmlFormatReaderInterpreter (ClassDataContract classContract)
		{
			this.classContract = classContract;
		}

		public XmlFormatReaderInterpreter (CollectionDataContract collectionContract, bool isGetOnly)
		{
			this.collectionContract = collectionContract;
			this.is_get_only_collection = isGetOnly;
		}

		bool is_get_only_collection;

		ClassDataContract classContract;

		CollectionDataContract collectionContract;

		object objectLocal;
		Type objectType;
		XmlReaderDelegator xmlReader;
		XmlObjectSerializerReadContext context;

		XmlDictionaryString [] memberNames = null;
		XmlDictionaryString [] memberNamespaces = null;
		XmlDictionaryString itemName = null;
		XmlDictionaryString itemNamespace = null;

		public object ReadFromXml (XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString[] memberNames, XmlDictionaryString[] memberNamespaces)
		{
			// InitArgs()
			this.xmlReader = xmlReader;
			this.context = context;
			this.memberNames = memberNames;
			this.memberNamespaces = memberNamespaces;
			
			//DemandSerializationFormatterPermission(classContract);
			//DemandMemberAccessPermission(memberAccessFlag);
			CreateObject (classContract);
			
			context.AddNewObject (objectLocal);
			InvokeOnDeserializing (classContract);
            
            string objectId = null;
            
			if (HasFactoryMethod (classContract))
				objectId = context.GetObjectId ();
			if (classContract.IsISerializable)
				ReadISerializable (classContract);
			else
				ReadClass (classContract);
			bool isFactoryType = InvokeFactoryMethod (classContract, objectId);
			if (Globals.TypeOfIDeserializationCallback.IsAssignableFrom (classContract.UnderlyingType))
				((IDeserializationCallback) objectLocal).OnDeserialization (null);
			InvokeOnDeserialized(classContract);
			if (objectId == null || !isFactoryType) {

				// Do a conversion back from DateTimeOffsetAdapter to DateTimeOffset after deserialization.
				// DateTimeOffsetAdapter is used here for deserialization purposes to bypass the ISerializable implementation
				// on DateTimeOffset; which does not work in partial trust.

				if (classContract.UnderlyingType == Globals.TypeOfDateTimeOffsetAdapter)
					objectLocal = DateTimeOffsetAdapter.GetDateTimeOffset ((DateTimeOffsetAdapter) objectLocal);
				// else - do we have to call CodeInterpreter.ConvertValue()? I guess not...
			}
			return objectLocal;
		}
		
		public object ReadCollectionFromXml (XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, CollectionDataContract collectionContract)
		{
			#region GenerateCollectionReaderHelper
			// InitArgs()
			this.xmlReader = xmlReader;
			this.context = context;
			this.itemName = itemName;
			this.itemNamespace = itemNamespace;

			this.collectionContract = collectionContract;

			#endregion

			ReadCollection (collectionContract);

			return objectLocal;
		}
		
		public void ReadGetOnlyCollectionFromXml (XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, CollectionDataContract collectionContract)
		{
			#region GenerateCollectionReaderHelper
			// InitArgs()
			this.xmlReader = xmlReader;
			this.context = context;
			this.itemName = itemName;
			this.itemNamespace = itemNamespace;

			this.collectionContract = collectionContract;

			#endregion

			ReadGetOnlyCollection (collectionContract);
		}

		void CreateObject (ClassDataContract classContract)
		{
			Type type = objectType = classContract.UnderlyingType;
			if (type.IsValueType && !classContract.IsNonAttributedType)
				type = Globals.TypeOfValueType;

			if (classContract.UnderlyingType == Globals.TypeOfDBNull)
				objectLocal = DBNull.Value;
			else if (classContract.IsNonAttributedType) {
				if (type.IsValueType)
					objectLocal = FormatterServices.GetUninitializedObject (type);
				else
					objectLocal = classContract.GetNonAttributedTypeConstructor ().Invoke (new object [0]);
			}
			else
				objectLocal = CodeInterpreter.ConvertValue (XmlFormatReaderGenerator.UnsafeGetUninitializedObject (DataContract.GetIdForInitialization (classContract)), Globals.TypeOfObject, type);
		}

		void InvokeOnDeserializing (ClassDataContract classContract)
		{
			if (classContract.BaseContract != null)
				InvokeOnDeserializing (classContract.BaseContract);
			if (classContract.OnDeserializing != null)
				classContract.OnDeserializing.Invoke (objectLocal, new object [] {context.GetStreamingContext ()});
		}

		void InvokeOnDeserialized (ClassDataContract classContract)
		{
			if (classContract.BaseContract != null)
				InvokeOnDeserialized (classContract.BaseContract);
			if (classContract.OnDeserialized != null)
				classContract.OnDeserialized.Invoke (objectLocal, new object [] {context.GetStreamingContext ()});
		}

		bool HasFactoryMethod (ClassDataContract classContract)
		{
			return Globals.TypeOfIObjectReference.IsAssignableFrom (classContract.UnderlyingType);
		}

		bool InvokeFactoryMethod (ClassDataContract classContract, string objectId)
		{
			if (HasFactoryMethod (classContract)) {
				objectLocal = CodeInterpreter.ConvertValue (context.GetRealObject ((IObjectReference) objectLocal, objectId), Globals.TypeOfObject, classContract.UnderlyingType);
				return true;
			}
			return false;
		}

		void ReadISerializable (ClassDataContract classContract)
		{
			ConstructorInfo ctor = classContract.GetISerializableConstructor ();
			context.ReadSerializationInfo (xmlReader, classContract.UnderlyingType);
			ctor.Invoke (objectLocal, new object [] {context.GetStreamingContext ()});
		}

		void ReadClass (ClassDataContract classContract)
		{
			if (classContract.HasExtensionData) {
				ExtensionDataObject extensionData = new ExtensionDataObject ();
				ReadMembers (classContract, extensionData);
				ClassDataContract currentContract = classContract;
				while (currentContract != null) {
					MethodInfo extensionDataSetMethod = currentContract.ExtensionDataSetMethod;
					if (extensionDataSetMethod != null)
						extensionDataSetMethod.Invoke (objectLocal, new object [] {extensionData});
					currentContract = currentContract.BaseContract;
				}
			}
			else
				ReadMembers (classContract, null);
		}

		void ReadMembers (ClassDataContract classContract, ExtensionDataObject  extensionData)
		{
			int memberCount = classContract.MemberNames.Length;
			context.IncrementItemCount (memberCount);

			int memberIndex = -1;
			
			int firstRequiredMember;
			bool[] requiredMembers = GetRequiredMembers (classContract, out firstRequiredMember);
			bool hasRequiredMembers = (firstRequiredMember < memberCount);
			int requiredIndex = hasRequiredMembers ? firstRequiredMember : memberCount;

			while (XmlObjectSerializerReadContext.MoveToNextElement (xmlReader)) {
				int idx; // used as in "switch (idx)" in the original source.
				if (hasRequiredMembers)
					idx = context.GetMemberIndexWithRequiredMembers (xmlReader, memberNames, memberNamespaces, memberIndex, (int) requiredIndex, extensionData);
				else
					idx = context.GetMemberIndex (xmlReader, memberNames, memberNamespaces, memberIndex, extensionData);

				if (memberCount > 0)
					ReadMembers (idx, classContract, requiredMembers, ref memberIndex, ref requiredIndex);
			}

			if (hasRequiredMembers)
			{
				if (requiredIndex < memberCount)
					XmlObjectSerializerReadContext.ThrowRequiredMemberMissingException (xmlReader, memberIndex, requiredIndex, memberNames);
			}
		}

		int ReadMembers (int index, ClassDataContract classContract, bool [] requiredMembers, ref int memberIndex, ref int requiredIndex)
		{
			int memberCount = (classContract.BaseContract == null) ? 0 : ReadMembers (index, classContract.BaseContract, requiredMembers,
			ref memberIndex, ref requiredIndex);
			
			if (memberCount <= index && index < memberCount + classContract.Members.Count) {
				DataMember dataMember = classContract.Members [index - memberCount];
				Type memberType = dataMember.MemberType;
				if (dataMember.IsRequired) {
					int nextRequiredIndex = index + 1;
					for (; nextRequiredIndex < requiredMembers.Length; nextRequiredIndex++)
						if (requiredMembers [nextRequiredIndex])
							break;
					requiredIndex = nextRequiredIndex;
				}

				if (dataMember.IsGetOnlyCollection) {
					var value = CodeInterpreter.GetMember (dataMember.MemberInfo, objectLocal);
					context.StoreCollectionMemberInfo (value);
					ReadValue (memberType, dataMember.Name, classContract.StableName.Namespace);
				} else {
					var value = ReadValue (memberType, dataMember.Name, classContract.StableName.Namespace);
					CodeInterpreter.SetMember (dataMember.MemberInfo, objectLocal, value);
				}
				memberIndex = index;
			}
			return memberCount + classContract.Members.Count;
		}

		bool[] GetRequiredMembers (ClassDataContract contract, out int firstRequiredMember)
		{
			int memberCount = contract.MemberNames.Length;
			bool [] requiredMembers = new bool [memberCount];
			GetRequiredMembers (contract, requiredMembers);
			for (firstRequiredMember = 0; firstRequiredMember < memberCount; firstRequiredMember++)
				if (requiredMembers [firstRequiredMember])
					break;
			return requiredMembers;
		}

		int GetRequiredMembers (ClassDataContract contract, bool[] requiredMembers)
		{
			int memberCount = (contract.BaseContract == null) ? 0 : GetRequiredMembers (contract.BaseContract, requiredMembers);
			List<DataMember> members = contract.Members;
			for (int i = 0; i < members.Count; i++, memberCount++)
				requiredMembers [memberCount] = members [i].IsRequired;
			return memberCount;
		}

		object ReadValue (Type type, string name, string ns)
		{
			var valueType = type;
			object value = null;
			bool shouldAssignNullableValue = false;
			int nullables = 0;
			while (type.IsGenericType && type.GetGenericTypeDefinition () == Globals.TypeOfNullable) {
				nullables++;
				type = type.GetGenericArguments () [0];
			}
			
			PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract (type);
			if ((primitiveContract != null && primitiveContract.UnderlyingType != Globals.TypeOfObject) || nullables != 0 || type.IsValueType) {
				context.ReadAttributes (xmlReader);
				string objectId = context.ReadIfNullOrRef (xmlReader, type, DataContract.IsTypeSerializable (type));
				// Deserialize null
                if (objectId == Globals.NullObjectId) {
					
					if (nullables != 0)
						value = Activator.CreateInstance (valueType);
					else if (type.IsValueType)
						throw new SerializationException (SR.GetString (SR.ValueTypeCannotBeNull, DataContract.GetClrTypeFullName (type)));
					else
						value = null;
				} else if (objectId == string.Empty) {
					// Deserialize value

					// Compare against Globals.NewObjectId, which is set to string.Empty
					
					objectId = context.GetObjectId ();
					
					if (type.IsValueType) {
						if (!string.IsNullOrEmpty (objectId))
							throw new SerializationException (SR.GetString (SR.ValueTypeCannotHaveId, DataContract.GetClrTypeFullName(type)));
					}
					object innerValueRead = null;
					if (nullables != 0)
						shouldAssignNullableValue = true;

					if (primitiveContract != null && primitiveContract.UnderlyingType != Globals.TypeOfObject) {
						value = primitiveContract.XmlFormatReaderMethod.Invoke (xmlReader, new object [0]);
						if (!type.IsValueType)
							context.AddNewObject (value);
					}
					else
							value = InternalDeserialize (type, name, ns);
				} else {
					// Deserialize ref
					if (type.IsValueType)
						throw new SerializationException (SR.GetString (SR.ValueTypeCannotHaveRef, DataContract.GetClrTypeFullName (type)));
					else
						value = CodeInterpreter.ConvertValue (context.GetExistingObject (objectId, type, name, ns), Globals.TypeOfObject, type);
				}

				if (shouldAssignNullableValue) {
					if (objectId != Globals.NullObjectId)
						value = WrapNullableObject (type, value, valueType, nullables);
				}
			}
			else
				value = InternalDeserialize (type, name, ns);

			return value;
		}

		object InternalDeserialize (Type type, string name, string ns)
		{
			Type declaredType = type.IsPointer ? Globals.TypeOfReflectionPointer : type;
			var obj = context.InternalDeserialize (xmlReader, DataContract.GetId (declaredType.TypeHandle), declaredType.TypeHandle, name, ns);

			if (type.IsPointer)
				// wow, there is no way to convert void* to object in strongly typed way...
				return XmlFormatGeneratorStatics.UnboxPointer.Invoke (null, new object [] {obj});
			else
				return CodeInterpreter.ConvertValue (obj, Globals.TypeOfObject, type);
		}

		object WrapNullableObject (Type innerType, object innerValue, Type outerType, int nullables)
		{
			var outerValue = innerValue;
			for (int i = 1; i < nullables; i++) {
				Type type = Globals.TypeOfNullable.MakeGenericType (innerType);
				outerValue = Activator.CreateInstance (type, new object[] { outerValue });
				innerType = type;
			}
			return Activator.CreateInstance (outerType, new object[] { outerValue });
		}


		void ReadCollection (CollectionDataContract collectionContract)
		{
			Type type = collectionContract.UnderlyingType;
			Type itemType = collectionContract.ItemType;
			bool isArray = (collectionContract.Kind == CollectionKind.Array);

			ConstructorInfo constructor = collectionContract.Constructor;

			if (type.IsInterface) {
				switch (collectionContract.Kind) {
				case CollectionKind.GenericDictionary:
					type = Globals.TypeOfDictionaryGeneric.MakeGenericType (itemType.GetGenericArguments ());
					constructor = type.GetConstructor (BindingFlags.Instance | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
					break;
				case CollectionKind.Dictionary:
					type = Globals.TypeOfHashtable;
					constructor = XmlFormatGeneratorStatics.HashtableCtor;
					break;
				case CollectionKind.Collection:
				case CollectionKind.GenericCollection:
				case CollectionKind.Enumerable:
				case CollectionKind.GenericEnumerable:
				case CollectionKind.List:
				case CollectionKind.GenericList:
					type = itemType.MakeArrayType ();
					isArray = true;
					break;
				}
			}
			string itemName = collectionContract.ItemName;
			string itemNs = collectionContract.StableName.Namespace;

			if (!isArray) {
				if (type.IsValueType)
					// FIXME: this is not what the original code does.
					objectLocal = FormatterServices.GetUninitializedObject (type);
				else {
					objectLocal = constructor.Invoke (new object [0]);
					context.AddNewObject (objectLocal);
				}
			}

			int size = context.GetArraySize ();

			string objectId = context.GetObjectId ();

			bool canReadPrimitiveArray = false, readResult = false;
			if (isArray && TryReadPrimitiveArray (type, itemType, size, out readResult))
				canReadPrimitiveArray = true;

			if (!readResult) {
				if (size == -1) {

					object growingCollection = null;
					if (isArray)
						growingCollection = Array.CreateInstance (itemType, 32);

					int i = 0;
					// FIXME: I cannot find i++ part, but without that it won't work as expected.
					for (; i < int.MaxValue; i++) {
						if (IsStartElement (this.itemName, this.itemNamespace)) {
							context.IncrementItemCount (1);
							object value = ReadCollectionItem (collectionContract, itemType, itemName, itemNs);
							if (isArray) {
								MethodInfo ensureArraySizeMethod = XmlFormatGeneratorStatics.EnsureArraySizeMethod.MakeGenericMethod (itemType);
								growingCollection = ensureArraySizeMethod.Invoke (null, new object [] {growingCollection, i});
								((Array) growingCollection).SetValue (value, i);
							} else {
								StoreCollectionValue (objectLocal, itemType, value, collectionContract);
							}
						}
						else if (IsEndElement ())
							break;
						else
							HandleUnexpectedItemInCollection (ref i);
					}

					if (isArray) {
						MethodInfo trimArraySizeMethod = XmlFormatGeneratorStatics.TrimArraySizeMethod.MakeGenericMethod (itemType);
						objectLocal = trimArraySizeMethod.Invoke (null, new object [] {growingCollection, i});
						context.AddNewObjectWithId (objectId, objectLocal);
					}
				} else {
					context.IncrementItemCount (size);
					if (isArray) {
						objectLocal = Array.CreateInstance (itemType, size);
						context.AddNewObject (objectLocal);
					}
					// FIXME: I cannot find j++ part, but without that it won't work as expected.
					for (int j = 0; j < size; j++) {
						if (IsStartElement (this.itemName, this.itemNamespace)) {
							var itemValue = ReadCollectionItem (collectionContract, itemType, itemName, itemNs);
							if (isArray)
								((Array) objectLocal).SetValue (itemValue, j);
							else
								StoreCollectionValue (objectLocal, itemType, itemValue, collectionContract);
						}
						else
							HandleUnexpectedItemInCollection (ref j);
					}
					context.CheckEndOfArray (xmlReader, size, this.itemName, this.itemNamespace);
				}
			}
			if (canReadPrimitiveArray)
				context.AddNewObjectWithId (objectId, objectLocal);
		}

		void ReadGetOnlyCollection (CollectionDataContract collectionContract)
		{
			Type type = collectionContract.UnderlyingType;
			Type itemType = collectionContract.ItemType;
			bool isArray = (collectionContract.Kind == CollectionKind.Array);
			string itemName = collectionContract.ItemName;
			string itemNs = collectionContract.StableName.Namespace;

			objectLocal = context.GetCollectionMember ();

			//check that items are actually going to be deserialized into the collection
			if (IsStartElement (this.itemName, this.itemNamespace)) {
				if (objectLocal == null)
					XmlObjectSerializerReadContext.ThrowNullValueReturnedForGetOnlyCollectionException (type);
				else {
					int size = 0;
					if (isArray)
						size = ((Array) objectLocal).Length;
					context.AddNewObject (objectLocal);
					for (int i = 0; i < int.MaxValue;) {
						if (IsStartElement (this.itemName, this.itemNamespace)) {
							context.IncrementItemCount (1);
							var value = ReadCollectionItem (collectionContract, itemType, itemName, itemNs);
							if (isArray) {
								if (size == i)
									XmlObjectSerializerReadContext.ThrowArrayExceededSizeException (size, type);
								else
									((Array) objectLocal).SetValue (value, i);
							} else {
								StoreCollectionValue (objectLocal, itemType, value, collectionContract);
							}
						}
						else if (IsEndElement())
							break;
						else
							HandleUnexpectedItemInCollection (ref i);
					}
					context.CheckEndOfArray (xmlReader, size, this.itemName, this.itemNamespace);
				}
			}
		}

		bool TryReadPrimitiveArray (Type type, Type itemType, int size, out bool readResult)
		{
			readResult = false;
			PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract (itemType);
			if (primitiveContract == null)
				return false;

			string readArrayMethod = null;
			switch (Type.GetTypeCode (itemType))
			{
			case TypeCode.Boolean:
				readArrayMethod = "TryReadBooleanArray";
			break;
			case TypeCode.DateTime:
				readArrayMethod = "TryReadDateTimeArray";
			break;
			case TypeCode.Decimal:
				readArrayMethod = "TryReadDecimalArray";
			break;
			case TypeCode.Int32:
				readArrayMethod = "TryReadInt32Array";
			break;
			case TypeCode.Int64:
				readArrayMethod = "TryReadInt64Array";
			break;
			case TypeCode.Single:
				readArrayMethod = "TryReadSingleArray";
			break;
			case TypeCode.Double:
				readArrayMethod = "TryReadDoubleArray";
				break;
			default:
				break;
			}
			if (readArrayMethod != null) {
				var mi = typeof (XmlReaderDelegator).GetMethod (readArrayMethod, Globals.ScanAllMembers);
				var args = new object [] {context, itemName, itemNamespace, size, objectLocal};
				readResult = (bool) mi.Invoke (xmlReader, args);
				objectLocal = args.Last ();
				return true;
			}
			return false;
		}

		object ReadCollectionItem (CollectionDataContract collectionContract, Type itemType, string itemName, string itemNs)
		{
			if (collectionContract.Kind == CollectionKind.Dictionary || collectionContract.Kind == CollectionKind.GenericDictionary) {
				context.ResetAttributes ();
				return CodeInterpreter.ConvertValue (collectionContract.ItemContract.ReadXmlValue (xmlReader, context), Globals.TypeOfObject, itemType);
			}
			else
				return ReadValue (itemType, itemName, itemNs);
		}

		void StoreCollectionValue (object collection, Type valueType, object value, CollectionDataContract collectionContract)
		{
			if (collectionContract.Kind == CollectionKind.GenericDictionary || collectionContract.Kind == CollectionKind.Dictionary) {
				ClassDataContract keyValuePairContract = DataContract.GetDataContract (valueType) as ClassDataContract;
				if (keyValuePairContract == null)
					Fx.Assert ("Failed to create contract for KeyValuePair type");
				DataMember keyMember = keyValuePairContract.Members [0];
				DataMember valueMember = keyValuePairContract.Members [1];
				object pkey = CodeInterpreter.GetMember (keyMember.MemberInfo, value);
				object pvalue = CodeInterpreter.GetMember (valueMember.MemberInfo, value);
				
				collectionContract.AddMethod.Invoke (collection, new object [] {pkey, pvalue});
			}
			else
				collectionContract.AddMethod.Invoke (collection, new object [] {value});
		}

		void HandleUnexpectedItemInCollection (ref int iterator)
		{
			if (IsStartElement ()) {
				context.SkipUnknownElement (xmlReader);
				iterator--;
			}
			else 
				throw XmlObjectSerializerReadContext.CreateUnexpectedStateException (XmlNodeType.Element, xmlReader);
		}

		bool IsStartElement(XmlDictionaryString name, XmlDictionaryString ns)
		{
			return xmlReader.IsStartElement (name, ns);
		}

		bool IsStartElement()
		{
			return xmlReader.IsStartElement ();
		}

		bool IsEndElement ()
		{
			return xmlReader.NodeType == XmlNodeType.EndElement;
		}
	}
}
