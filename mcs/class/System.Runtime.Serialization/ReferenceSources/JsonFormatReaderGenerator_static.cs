using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Xml;

namespace System.Runtime.Serialization.Json
{
	internal partial class JsonFormatReaderGenerator
	{
		partial class CriticalHelper
		{
			internal JsonFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
			{
				return (XmlReaderDelegator xr, XmlObjectSerializerReadContextComplexJson ctx, XmlDictionaryString emptyDictionaryString, XmlDictionaryString [] memberNames) => new JsonFormatReaderInterpreter (classContract).ReadFromJson (xr, ctx, emptyDictionaryString, memberNames);
			}

			internal JsonFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
			{
				return (XmlReaderDelegator xr, XmlObjectSerializerReadContextComplexJson ctx, XmlDictionaryString emptyDS, XmlDictionaryString inm, CollectionDataContract cc) => new JsonFormatReaderInterpreter (collectionContract, false).ReadCollectionFromJson (xr, ctx, emptyDS, inm, cc);
			}
			
			internal JsonFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
			{
				return (XmlReaderDelegator xr, XmlObjectSerializerReadContextComplexJson ctx, XmlDictionaryString emptyDS, XmlDictionaryString inm, CollectionDataContract cc) => new JsonFormatReaderInterpreter (collectionContract, true).ReadGetOnlyCollectionFromJson (xr, ctx, emptyDS, inm, cc);
			}
		}
	}

	class JsonFormatReaderInterpreter
	{
		public JsonFormatReaderInterpreter (ClassDataContract classContract)
		{
			this.classContract = classContract;
		}

		public JsonFormatReaderInterpreter (CollectionDataContract collectionContract, bool isGetOnly)
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
		XmlObjectSerializerReadContextComplexJson context;

		XmlDictionaryString [] memberNames = null;
		XmlDictionaryString emptyDictionaryString = null;
		XmlDictionaryString itemName = null;
		XmlDictionaryString itemNamespace = null;

		public object ReadFromJson (XmlReaderDelegator xmlReader, XmlObjectSerializerReadContextComplexJson context, XmlDictionaryString emptyDictionaryString, XmlDictionaryString[] memberNames)
		{
			// InitArgs()
			this.xmlReader = xmlReader;
			this.context = context;
			this.emptyDictionaryString = emptyDictionaryString;
			this.memberNames = memberNames;
			
			//DemandSerializationFormatterPermission(classContract);
			//DemandMemberAccessPermission(memberAccessFlag);
			CreateObject (classContract);
			
			context.AddNewObject (objectLocal);
			InvokeOnDeserializing (classContract);
            
            string objectId = null;
            
			if (classContract.IsISerializable)
				ReadISerializable (classContract);
			else
				ReadClass (classContract);
			if (Globals.TypeOfIDeserializationCallback.IsAssignableFrom (classContract.UnderlyingType))
				((IDeserializationCallback) objectLocal).OnDeserialization (null);
			InvokeOnDeserialized(classContract);
			if (!InvokeFactoryMethod (classContract)) {

				// Do a conversion back from DateTimeOffsetAdapter to DateTimeOffset after deserialization.
				// DateTimeOffsetAdapter is used here for deserialization purposes to bypass the ISerializable implementation
				// on DateTimeOffset; which does not work in partial trust.

				if (classContract.UnderlyingType == Globals.TypeOfDateTimeOffsetAdapter)
					objectLocal = DateTimeOffsetAdapter.GetDateTimeOffset ((DateTimeOffsetAdapter) objectLocal);
				// else - do we have to call CodeInterpreter.ConvertValue()? I guess not...
			}
			return objectLocal;
		}
		
		public object ReadCollectionFromJson (XmlReaderDelegator xmlReader, XmlObjectSerializerReadContextComplexJson context, XmlDictionaryString emptyDictionaryString, XmlDictionaryString itemName, CollectionDataContract collectionContract)
		{
			#region GenerateCollectionReaderHelper
			// InitArgs()
			this.xmlReader = xmlReader;
			this.context = context;
			this.emptyDictionaryString = emptyDictionaryString;
			this.itemName = itemName;

			this.collectionContract = collectionContract;

			#endregion

			ReadCollection (collectionContract);

			return objectLocal;
		}
		
		public void ReadGetOnlyCollectionFromJson (XmlReaderDelegator xmlReader, XmlObjectSerializerReadContextComplexJson context, XmlDictionaryString emptyDictionaryString, XmlDictionaryString itemName, CollectionDataContract collectionContract)
		{
			#region GenerateCollectionReaderHelper
			// InitArgs()
			this.xmlReader = xmlReader;
			this.context = context;
			this.emptyDictionaryString = emptyDictionaryString;
			this.itemName = itemName;

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

		bool InvokeFactoryMethod (ClassDataContract classContract)
		{
			if (HasFactoryMethod (classContract)) {
				objectLocal = CodeInterpreter.ConvertValue (context.GetRealObject ((IObjectReference) objectLocal, Globals.NewObjectId), Globals.TypeOfObject, classContract.UnderlyingType);
				return true;
			}
			return false;
		}

		void ReadISerializable (ClassDataContract classContract)
		{
			ConstructorInfo ctor = classContract.UnderlyingType.GetConstructor (Globals.ScanAllMembers, null, JsonFormatGeneratorStatics.SerInfoCtorArgs, null);
			if (ctor == null)
				throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError (XmlObjectSerializer.CreateSerializationException (SR.GetString (SR.SerializationInfo_ConstructorNotFound, DataContract.GetClrTypeFullName (classContract.UnderlyingType))));
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
			
			// JSON intrinsic part.
			BitFlagsGenerator expectedElements = new BitFlagsGenerator (memberCount);
			byte [] requiredElements = new byte [expectedElements.GetLocalCount ()];
			SetRequiredElements (classContract, requiredElements);
			SetExpectedElements (expectedElements, 0 /*startIndex*/);

			while (XmlObjectSerializerReadContext.MoveToNextElement (xmlReader)) {
				int idx; // used as in "switch (idx)" in the original source.
				idx = context.GetJsonMemberIndex (xmlReader, memberNames, memberIndex, extensionData);

				if (memberCount > 0)
					ReadMembers (idx, classContract, expectedElements, ref memberIndex);
			}

			if (!CheckRequiredElements (expectedElements, requiredElements))
				XmlObjectSerializerReadContextComplexJson.ThrowMissingRequiredMembers (objectLocal, memberNames, expectedElements.LoadArray (), requiredElements);
		}

		int ReadMembers (int index, ClassDataContract classContract, BitFlagsGenerator expectedElements, ref int memberIndex)
		{
			int memberCount = (classContract.BaseContract == null) ? 0 : ReadMembers (index, classContract.BaseContract, expectedElements,
			ref memberIndex);
			
			if (memberCount <= index && index < memberCount + classContract.Members.Count) {
				DataMember dataMember = classContract.Members [index - memberCount];
				Type memberType = dataMember.MemberType;
				
				memberIndex = memberCount;
				if (!expectedElements.Load (index))
					XmlObjectSerializerReadContextComplexJson.ThrowDuplicateMemberException (objectLocal, memberNames, memberIndex);

				if (dataMember.IsGetOnlyCollection) {
					var value = CodeInterpreter.GetMember (dataMember.MemberInfo, objectLocal);
					context.StoreCollectionMemberInfo (value);
					ReadValue (memberType, dataMember.Name);
				} else {
					var value = ReadValue (memberType, dataMember.Name);
					CodeInterpreter.SetMember (dataMember.MemberInfo, objectLocal, value);
				}
				memberIndex = index;
				ResetExpectedElements (expectedElements, index);
			}
			return memberCount + classContract.Members.Count;
		}

		bool CheckRequiredElements (BitFlagsGenerator expectedElements, byte [] requiredElements)
		{
			for (int i = 0; i < requiredElements.Length; i++)
				if ((expectedElements.GetLocal(i) & requiredElements[i]) != 0)
					return false;
			return true;
		}

		int SetRequiredElements (ClassDataContract contract, byte [] requiredElements)
		{
			int memberCount = (contract.BaseContract == null) ? 0 :
			SetRequiredElements (contract.BaseContract, requiredElements);
			List<DataMember> members = contract.Members;
			for (int i = 0; i < members.Count; i++, memberCount++) {
				if (members[i].IsRequired)
					BitFlagsGenerator.SetBit (requiredElements, memberCount);
			}
			return memberCount;
		}

		void SetExpectedElements (BitFlagsGenerator expectedElements, int startIndex)
		{
			int memberCount = expectedElements.GetBitCount ();
			for (int i = startIndex; i < memberCount; i++)
				expectedElements.Store (i, true);
		}

		void ResetExpectedElements (BitFlagsGenerator expectedElements, int index)
		{
			expectedElements.Store (index, false);
		}

		object ReadValue (Type type, string name)
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
							value = InternalDeserialize (type, name);
				} else {
					// Deserialize ref
					if (type.IsValueType)
						throw new SerializationException (SR.GetString (SR.ValueTypeCannotHaveRef, DataContract.GetClrTypeFullName (type)));
					else
						value = CodeInterpreter.ConvertValue (context.GetExistingObject (objectId, type, name, string.Empty), Globals.TypeOfObject, type);
				}

				if (shouldAssignNullableValue) {
					if (objectId != Globals.NullObjectId)
						value = WrapNullableObject (type, value, valueType, nullables);
				}
			}
			else
				value = InternalDeserialize (type, name);

			return value;
		}

		object InternalDeserialize (Type type, string name)
		{
			Type declaredType = type.IsPointer ? Globals.TypeOfReflectionPointer : type;
			var obj = context.InternalDeserialize (xmlReader, DataContract.GetId (declaredType.TypeHandle), declaredType.TypeHandle, name, string.Empty);

			if (type.IsPointer)
				// wow, there is no way to convert void* to object in strongly typed way...
				return JsonFormatGeneratorStatics.UnboxPointer.Invoke (null, new object [] {obj});
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
					constructor = type.GetConstructor (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Globals.EmptyTypeArray, null);
					break;
				case CollectionKind.Dictionary:
					type = Globals.TypeOfHashtable;
					constructor = type.GetConstructor (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Globals.EmptyTypeArray, null);
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

			if (!isArray) {
				if (type.IsValueType)
					// FIXME: this is not what the original code does.
					objectLocal = FormatterServices.GetUninitializedObject (type);
				else {
					objectLocal = constructor.Invoke (new object [0]);
					context.AddNewObject (objectLocal);
				}
			}

			bool canReadSimpleDictionary = collectionContract.Kind == CollectionKind.Dictionary ||
			collectionContract.Kind == CollectionKind.GenericDictionary;

			bool readSimpleDictionary = canReadSimpleDictionary & context.UseSimpleDictionaryFormat;
			if (readSimpleDictionary)
				ReadSimpleDictionary (collectionContract, itemType);
			else {	 
				string objectId = context.GetObjectId ();

				bool canReadPrimitiveArray = false, readResult = false;
				if (isArray && TryReadPrimitiveArray (itemType, out readResult))
					canReadPrimitiveArray = true;

				if (!canReadPrimitiveArray) {
					object growingCollection = null;
					if (isArray)
						growingCollection = Array.CreateInstance (itemType, 32);

					int i = 0;
					// FIXME: I cannot find i++ part, but without that it won't work as expected.
					for (; i < int.MaxValue; i++) {
						if (IsStartElement (this.itemName, this.emptyDictionaryString)) {
							context.IncrementItemCount (1);
							object value = ReadCollectionItem (collectionContract, itemType);
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
				}
				else
					context.AddNewObjectWithId (objectId, objectLocal);
			}
		}

		void ReadSimpleDictionary (CollectionDataContract collectionContract, Type keyValueType)
		{
			Type[] keyValueTypes = keyValueType.GetGenericArguments ();
			Type keyType = keyValueTypes [0];
			Type valueType = keyValueTypes [1];

			int keyTypeNullableDepth = 0;
			Type keyTypeOriginal = keyType;
			while (keyType.IsGenericType && keyType.GetGenericTypeDefinition () == Globals.TypeOfNullable) {
				keyTypeNullableDepth++;
				keyType = keyType.GetGenericArguments () [0];
			}

			ClassDataContract keyValueDataContract = (ClassDataContract)collectionContract.ItemContract;
			DataContract keyDataContract = keyValueDataContract.Members [0].MemberTypeContract;

			KeyParseMode keyParseMode = KeyParseMode.Fail;

			if (keyType == Globals.TypeOfString || keyType == Globals.TypeOfObject) {
				keyParseMode = KeyParseMode.AsString;
			} else if (keyType.IsEnum) {
				keyParseMode = KeyParseMode.UsingParseEnum;
			} else if (keyDataContract.ParseMethod != null) {
				keyParseMode = KeyParseMode.UsingCustomParse;
			}

			if (keyParseMode == KeyParseMode.Fail) {
				ThrowSerializationException (
				SR.GetString (
				SR.KeyTypeCannotBeParsedInSimpleDictionary,
				DataContract.GetClrTypeFullName (collectionContract.UnderlyingType),
				DataContract.GetClrTypeFullName (keyType)));
			} else {
				XmlNodeType nodeType;

				while ((nodeType = xmlReader.MoveToContent ()) != XmlNodeType.EndElement) {
					if (nodeType != XmlNodeType.Element)
						ThrowUnexpectedStateException (XmlNodeType.Element);

					context.IncrementItemCount (1);

					var jsonMemberName = XmlObjectSerializerReadContextComplexJson.GetJsonMemberName (xmlReader);
					object key = null;

					if (keyParseMode == KeyParseMode.AsString)
						key = jsonMemberName;
					else if (keyParseMode == KeyParseMode.UsingParseEnum)
						key = Enum.Parse (keyType, jsonMemberName);
					else if (keyParseMode == KeyParseMode.UsingCustomParse)
						key = keyDataContract.ParseMethod.Invoke (null, new object [] {jsonMemberName});

					if (keyTypeNullableDepth > 0) {
						var keyOriginal = WrapNullableObject (keyType, key, valueType, keyTypeNullableDepth);
						key = keyOriginal;
					}

					var value = ReadValue (valueType, String.Empty);
					collectionContract.AddMethod.Invoke (objectLocal, new object[] {key, value});
				}
			}
		}

		void ReadGetOnlyCollection (CollectionDataContract collectionContract)
		{
			Type type = collectionContract.UnderlyingType;
			Type itemType = collectionContract.ItemType;
			bool isArray = (collectionContract.Kind == CollectionKind.Array);
			int size = 0;

			objectLocal = context.GetCollectionMember ();
 
			bool canReadSimpleDictionary = 
				collectionContract.Kind == CollectionKind.Dictionary ||
				collectionContract.Kind == CollectionKind.GenericDictionary;

			bool readSimple = canReadSimpleDictionary && context.UseSimpleDictionaryFormat;
			if (readSimple) {
				if (objectLocal == null)
					XmlObjectSerializerReadContext.ThrowNullValueReturnedForGetOnlyCollectionException (type);
				else {
					ReadSimpleDictionary(collectionContract, itemType);
					context.CheckEndOfArray (xmlReader, size, this.itemName, emptyDictionaryString);
				}
			} else {

				//check that items are actually going to be deserialized into the collection
				if (IsStartElement (this.itemName, this.emptyDictionaryString)) {
					if (objectLocal == null)
						XmlObjectSerializerReadContext.ThrowNullValueReturnedForGetOnlyCollectionException (type);
					else {
						size = 0;
						if (isArray)
							size = ((Array) objectLocal).Length;
						for (int i = 0; i < int.MaxValue;) {
							if (IsStartElement (this.itemName, this.emptyDictionaryString)) {
								context.IncrementItemCount (1);
								var value = ReadCollectionItem (collectionContract, itemType);
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
						context.CheckEndOfArray (xmlReader, size, this.itemName, this.emptyDictionaryString);
					}
				}
			}
		}

		bool TryReadPrimitiveArray (Type itemType, out bool readResult)
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
			case TypeCode.DateTime:
				readArrayMethod = "TryReadJsonDateTimeArray";
			break;
			default:
				break;
			}
			if (readArrayMethod != null) {
				var mi = typeof (JsonReaderDelegator).GetMethod (readArrayMethod, Globals.ScanAllMembers);
				var args = new object [] {context, itemName, emptyDictionaryString, -1, objectLocal};
				readResult = (bool) mi.Invoke ((JsonReaderDelegator) xmlReader, args);
				objectLocal = args.Last ();
				return true;
			}
			return false;
		}

		object ReadCollectionItem (CollectionDataContract collectionContract, Type itemType)
		{
			if (collectionContract.Kind == CollectionKind.Dictionary || collectionContract.Kind == CollectionKind.GenericDictionary) {
				context.ResetAttributes ();
				var revisedContract = XmlObjectSerializerWriteContextComplexJson.GetRevisedItemContract (collectionContract.ItemContract);
				var v = DataContractJsonSerializer.ReadJsonValue (revisedContract, xmlReader, context);
				return CodeInterpreter.ConvertValue (v, Globals.TypeOfObject, itemType);
			}
			else
				return ReadValue (itemType, JsonGlobals.itemString);
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
				
				try {
					collectionContract.AddMethod.Invoke (collection, new object [] {pkey, pvalue});
				} catch (TargetInvocationException ex) {
					if (ex.InnerException != null)
						throw ex.InnerException;
					else
						throw;
				}
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

		void ThrowUnexpectedStateException (XmlNodeType expectedState)
		{
			throw XmlObjectSerializerReadContext.CreateUnexpectedStateException (expectedState, xmlReader);
		}

		void ThrowSerializationException (string msg, params object [] values)
		{
			if (values != null && values.Length > 0)
				msg = string.Format (msg, values);
			throw new SerializationException (msg);
		}

		enum KeyParseMode
		{
			Fail,
			AsString,
			UsingParseEnum,
			UsingCustomParse
		}
	}
}
