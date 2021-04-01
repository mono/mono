using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace System.Runtime.Serialization.Json
{
	internal partial class JsonFormatWriterGenerator
	{
		partial class CriticalHelper
		{
			internal JsonFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
			{
				return (XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, ClassDataContract dataContract, XmlDictionaryString [] memberNames) => new JsonFormatWriterInterpreter (classContract).WriteToJson (xmlWriter, obj, context, dataContract, memberNames);
			}
			internal JsonFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
			{
				return (XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, CollectionDataContract dataContract) => new JsonFormatWriterInterpreter (collectionContract).WriteCollectionToJson (xmlWriter, obj, context, dataContract);
			}
		}
	}

	class JsonFormatWriterInterpreter
	{
		public JsonFormatWriterInterpreter (ClassDataContract classContract)
		{
			this.classContract = classContract;
		}

		public JsonFormatWriterInterpreter (CollectionDataContract collectionContract)
		{
			this.collectionContract = collectionContract;
		}

		ClassDataContract classContract;

		CollectionDataContract collectionContract;

		XmlWriterDelegator writer = null;
		object obj = null;
		XmlObjectSerializerWriteContextComplexJson context = null;
		DataContract dataContract = null;
		object objLocal = null;

		ClassDataContract classDataContract {
			get { return (ClassDataContract) dataContract; }
		}
		CollectionDataContract collectionDataContract {
			get {return (CollectionDataContract) dataContract; }
		}

		XmlDictionaryString [] memberNames = null;
		int typeIndex = 1;
		int childElementIndex = 0;

		public void WriteToJson (XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, ClassDataContract dataContract, XmlDictionaryString [] memberNames)
		{
			this.writer = xmlWriter;
			this.obj = obj;
			this.context = context;
			this.dataContract = dataContract;
			this.memberNames = memberNames;

			InitArgs (classContract.UnderlyingType);

			// DemandSerializationFormatterPermission (classContract) - irrelevant
			// DemandMemberAccessPermission (memberAccessFlag) - irrelevant

			if (classContract.IsReadOnlyContract)
			{
				DataContract.ThrowInvalidDataContractException (classContract.SerializationExceptionMessage, null);
			}

			WriteClass (classContract);
		}

		public void WriteCollectionToJson (XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, CollectionDataContract dataContract)
		{
			this.writer = xmlWriter;
			this.obj = obj;
			this.context = context;
			this.dataContract = dataContract;

			InitArgs (collectionContract.UnderlyingType);			

			// DemandMemberAccessPermission(memberAccessFlag);
			if (collectionContract.IsReadOnlyContract)
			{
				DataContract.ThrowInvalidDataContractException (collectionContract.SerializationExceptionMessage, null);
			}

			WriteCollection (collectionContract);
		}

		void InitArgs (Type objType)
		{
			if (objType == Globals.TypeOfDateTimeOffsetAdapter) {
				objLocal = DateTimeOffsetAdapter.GetDateTimeOffsetAdapter ((DateTimeOffset) obj);
			}
			else
				objLocal = CodeInterpreter.ConvertValue (obj, typeof (object), objType);
		}

		void InvokeOnSerializing (ClassDataContract classContract, object objSerialized, XmlObjectSerializerWriteContext context)
		{
			if (classContract.BaseContract != null)
				InvokeOnSerializing (classContract.BaseContract, objSerialized, context);
			if (classContract.OnSerializing != null) {
				classContract.OnSerializing.Invoke (objSerialized, new object [] {context.GetStreamingContext ()});
			}
		}

		void InvokeOnSerialized (ClassDataContract classContract, object objSerialized, XmlObjectSerializerWriteContext context)
		{
			if (classContract.BaseContract != null)
				InvokeOnSerialized (classContract.BaseContract, objSerialized, context);
			if (classContract.OnSerialized != null) {
				classContract.OnSerialized.Invoke (objSerialized, new object [] {context.GetStreamingContext ()});
			}
		}

		void WriteClass (ClassDataContract classContract)
		{
			InvokeOnSerializing (classContract, objLocal, context);

			if (classContract.IsISerializable)
				context.WriteJsonISerializable (writer, (ISerializable) objLocal);
			else
			{
				if (classContract.HasExtensionData)
				{
					ExtensionDataObject extensionData = ((IExtensibleDataObject) objLocal).ExtensionData;
					context.WriteExtensionData (writer, extensionData, -1);

					WriteMembers (classContract, extensionData, classContract);
				}
				else
					WriteMembers (classContract, null, classContract);
			}
			InvokeOnSerialized (classContract, objLocal, context);
		}

		void WriteCollection(CollectionDataContract collectionContract)
		{
			XmlDictionaryString itemName = context.CollectionItemName;

			if (collectionContract.Kind == CollectionKind.Array)
			{
				Type itemType = collectionContract.ItemType;
				int i;

				// This check does not exist in the original dynamic code,
				// but there is no other way to check type mismatch.
				// CollectionSerialization.ArrayContract() shows that it is required.
				if (objLocal.GetType ().GetElementType () != itemType)
					throw new InvalidCastException (string.Format ("Cannot cast array of {0} to array of {1}", objLocal.GetType ().GetElementType (), itemType));

				context.IncrementArrayCount (writer, (Array) objLocal);

				if (!TryWritePrimitiveArray(collectionContract.UnderlyingType, itemType, () => objLocal, itemName))
				{
					WriteArrayAttribute ();
					var arr = (Array) objLocal;
					var idx = new int [1];
					for (i = 0; i < arr.Length; i++) {
						if (!TryWritePrimitive(itemType, null, null, i, itemName, 0)) {
							WriteStartElement (itemName, 0);
							idx [0] = i;
							var mbrVal = arr.GetValue (idx);
							WriteValue (itemType, mbrVal);
							WriteEndElement ();
						}
					}
				}
			}
			else
			{
				// This check does not exist in the original dynamic code,
				// but there is no other way to check type mismatch.
				// CollectionSerialization.ArrayContract() shows that it is required.
				if (!collectionContract.UnderlyingType.IsAssignableFrom (objLocal.GetType ()))
					throw new InvalidCastException (string.Format ("Cannot cast {0} to {1}", objLocal.GetType (), collectionContract.UnderlyingType));
				
				MethodInfo incrementCollectionCountMethod = null;
				switch (collectionContract.Kind)
				{
				case CollectionKind.Collection:
				case CollectionKind.List:
				case CollectionKind.Dictionary:
					incrementCollectionCountMethod = XmlFormatGeneratorStatics.IncrementCollectionCountMethod;
					break;
				case CollectionKind.GenericCollection:
				case CollectionKind.GenericList:
					incrementCollectionCountMethod = XmlFormatGeneratorStatics.IncrementCollectionCountGenericMethod.MakeGenericMethod(collectionContract.ItemType);
					break;
				case CollectionKind.GenericDictionary:
					incrementCollectionCountMethod = XmlFormatGeneratorStatics.IncrementCollectionCountGenericMethod.MakeGenericMethod(Globals.TypeOfKeyValuePair.MakeGenericType(collectionContract.ItemType.GetGenericArguments()));
					break;
				}
				if (incrementCollectionCountMethod != null)
					incrementCollectionCountMethod.Invoke (context, new object [] {writer, objLocal});

				bool isDictionary = false, isGenericDictionary = false;
				Type enumeratorType = null;
				Type [] keyValueTypes = null;
				if (collectionContract.Kind == CollectionKind.GenericDictionary)
				{
					isGenericDictionary = true;
					keyValueTypes = collectionContract.ItemType.GetGenericArguments ();
					enumeratorType = Globals.TypeOfGenericDictionaryEnumerator.MakeGenericType (keyValueTypes);
				}
				else if (collectionContract.Kind == CollectionKind.Dictionary)
				{
					isDictionary = true;
					keyValueTypes = new Type[] { Globals.TypeOfObject, Globals.TypeOfObject };
					enumeratorType = Globals.TypeOfDictionaryEnumerator;
				}
				else
				{
					enumeratorType = collectionContract.GetEnumeratorMethod.ReturnType;
				}
				MethodInfo moveNextMethod = enumeratorType.GetMethod (Globals.MoveNextMethodName, BindingFlags.Instance | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
				MethodInfo getCurrentMethod = enumeratorType.GetMethod (Globals.GetCurrentMethodName, BindingFlags.Instance | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
				if (moveNextMethod == null || getCurrentMethod == null)
				{
					if (enumeratorType.IsInterface)
					{
						if (moveNextMethod == null)
							moveNextMethod = JsonFormatGeneratorStatics.MoveNextMethod;
						if (getCurrentMethod == null)
							getCurrentMethod = JsonFormatGeneratorStatics.GetCurrentMethod;
					}
					else
					{
						Type ienumeratorInterface = Globals.TypeOfIEnumerator;
						CollectionKind kind = collectionContract.Kind;
						if (kind == CollectionKind.GenericDictionary || kind == CollectionKind.GenericCollection || kind == CollectionKind.GenericEnumerable)
						{
							Type[] interfaceTypes = enumeratorType.GetInterfaces();
							foreach (Type interfaceType in interfaceTypes)
							{
								if (interfaceType.IsGenericType
									&& interfaceType.GetGenericTypeDefinition() == Globals.TypeOfIEnumeratorGeneric
									&& interfaceType.GetGenericArguments()[0] == collectionContract.ItemType)
								{
									ienumeratorInterface = interfaceType;
									break;
								}
							}
						}
						if (moveNextMethod == null)
							moveNextMethod = CollectionDataContract.GetTargetMethodWithName(Globals.MoveNextMethodName, enumeratorType, ienumeratorInterface);
						if (getCurrentMethod == null)
							getCurrentMethod = CollectionDataContract.GetTargetMethodWithName(Globals.GetCurrentMethodName, enumeratorType, ienumeratorInterface);
					}
				}
				Type elementType = getCurrentMethod.ReturnType;
				object currentValue = null; // of elementType

				var enumerator = (IEnumerator) collectionContract.GetEnumeratorMethod.Invoke (objLocal, new object [0]);
				if (isDictionary)
				{
					ConstructorInfo dictEnumCtor = enumeratorType.GetConstructor (Globals.ScanAllMembers, null, new Type[] { Globals.TypeOfIDictionaryEnumerator }, null);
					enumerator = (IEnumerator) dictEnumCtor.Invoke (new object [] {enumerator});
				}
				else if (isGenericDictionary)
				{
					Type ctorParam = Globals.TypeOfIEnumeratorGeneric.MakeGenericType(Globals.TypeOfKeyValuePair.MakeGenericType(keyValueTypes));
					ConstructorInfo dictEnumCtor = enumeratorType.GetConstructor(Globals.ScanAllMembers, null, new Type[] { ctorParam }, null);
					enumerator = (IEnumerator) Activator.CreateInstance (enumeratorType, new object [] {enumerator});
				}

				bool canWriteSimpleDictionary = isDictionary || isGenericDictionary;
				
				bool writeSimpleDictionary = canWriteSimpleDictionary && context.UseSimpleDictionaryFormat;
				PropertyInfo genericDictionaryKeyProperty = null, genericDictionaryValueProperty = null;
				
				if (canWriteSimpleDictionary)
				{
					Type genericDictionaryKeyValueType = Globals.TypeOfKeyValue.MakeGenericType (keyValueTypes);
					genericDictionaryKeyProperty = genericDictionaryKeyValueType.GetProperty (JsonGlobals.KeyString);
					genericDictionaryValueProperty = genericDictionaryKeyValueType.GetProperty (JsonGlobals.ValueString);
				}

				if (writeSimpleDictionary) {
					WriteObjectAttribute ();
					object key, value;
					var empty_args = new object [0];
					while ((bool) moveNextMethod.Invoke (enumerator, empty_args)) {
						currentValue = getCurrentMethod.Invoke (enumerator, empty_args);
						key = CodeInterpreter.GetMember (genericDictionaryKeyProperty, currentValue);
						value = CodeInterpreter.GetMember (genericDictionaryValueProperty, currentValue);

						WriteStartElement (key, 0 /*nameIndex*/);
						WriteValue (genericDictionaryValueProperty.PropertyType, value);
						WriteEndElement ();
					}
				} else {
					WriteArrayAttribute ();

					var emptyArray = new object [0];
					while (enumerator != null && enumerator.MoveNext ()) {
						currentValue = getCurrentMethod.Invoke (enumerator, emptyArray);

						if (incrementCollectionCountMethod == null)
							XmlFormatGeneratorStatics.IncrementItemCountMethod.Invoke (context, new object [] {1});

						if (!TryWritePrimitive (elementType, () => currentValue, null, null, itemName, 0))
						{
							WriteStartElement (itemName, 0);
							if (isGenericDictionary || isDictionary) {
								var jc = JsonDataContract.GetJsonDataContract (XmlObjectSerializerWriteContextComplexJson.GetRevisedItemContract (
								collectionDataContract.ItemContract));
								// FIXME: this TypeHandle might be wrong; there is no easy way to get Type for currentValue though.
								DataContractJsonSerializer.WriteJsonValue (jc, writer, currentValue, context, currentValue.GetType ().TypeHandle);
							}
							else
								WriteValue (elementType, currentValue);
							WriteEndElement ();
						}
					}
				}
			}
		}

		int WriteMembers (ClassDataContract classContract, ExtensionDataObject extensionData, ClassDataContract derivedMostClassContract)
		{
			int memberCount = (classContract.BaseContract == null) ? 0 : WriteMembers (classContract.BaseContract, extensionData, derivedMostClassContract);

			context.IncrementItemCount (classContract.Members.Count);

			for (int i = 0; i < classContract.Members.Count; i++, memberCount++) {

				DataMember member = classContract.Members[i];
				Type memberType = member.MemberType;
				object memberValue = null;
				if (member.IsGetOnlyCollection)
					context.StoreIsGetOnlyCollection ();
				bool doWrite = true, hasMemberValue = false;
				if (!member.EmitDefaultValue)
				{
					hasMemberValue = true;
					memberValue = LoadMemberValue (member);
					doWrite = !IsDefaultValue (memberType, memberValue);
				}

				if (doWrite) {

					bool requiresNameAttribute = DataContractJsonSerializer.CheckIfXmlNameRequiresMapping (classContract.MemberNames [i]);
					
					if (requiresNameAttribute || !TryWritePrimitive(memberType, hasMemberValue ? () => memberValue : (Func<object>) null, member.MemberInfo, null /*arrayItemIndex*/, null /*nameLocal*/, i + childElementIndex)) {

						// Note: DataContractSerializer has member-conflict logic here to deal with the schema export
						//       requirement that the same member can't be of two different types.
						if (requiresNameAttribute)
							XmlObjectSerializerWriteContextComplexJson.WriteJsonNameWithMapping (writer, memberNames, i + childElementIndex);
						else
							WriteStartElement (null /*nameLocal*/, i + childElementIndex);

						if (memberValue == null)
							memberValue = LoadMemberValue (member);
						WriteValue (memberType, memberValue);
						WriteEndElement ();
					}

					if (classContract.HasExtensionData)
						context.WriteExtensionData (writer, extensionData, memberCount);
				} else if (!member.EmitDefaultValue) {
					if (member.IsRequired)
						XmlObjectSerializerWriteContext.ThrowRequiredMemberMustBeEmitted (member.Name, classContract.UnderlyingType);
				}
			}

			typeIndex++;
			childElementIndex += classContract.Members.Count;
			return memberCount;
		}


		internal bool IsDefaultValue (Type type, object value)
		{
			var def = GetDefaultValue (type);
			return def == null ? (object) value == null : def.Equals (value);
		}

		internal object GetDefaultValue(Type type)
		{
			if (type.IsValueType)
			{
				switch (Type.GetTypeCode(type))
				{
				case TypeCode.Boolean:
					return false;
				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
					return 0;
				case TypeCode.Int64:
				case TypeCode.UInt64:
					return 0L;
				case TypeCode.Single:
					return 0.0F;
				case TypeCode.Double:
					return 0.0;
				case TypeCode.Decimal:
					return default (decimal);
				case TypeCode.DateTime:
					return default (DateTime);
				}
			}
			return null;
		}

		void WriteStartElement (object nameLocal, int nameIndex)
		{
			var name = nameLocal ?? memberNames [nameIndex];
			XmlDictionaryString namespaceLocal = null;
			if (nameLocal != null && nameLocal is string)
				writer.WriteStartElement ((string) name, null);
			else if (name is XmlDictionaryString)
				writer.WriteStartElement ((XmlDictionaryString) name, null);
			else
				writer.WriteStartElement (name.ToString(), null);
		}

		void WriteEndElement ()
		{
			writer.WriteEndElement ();
		}

		void WriteArrayAttribute ()
		{
			writer.WriteAttributeString (
				null /* prefix */,
				JsonGlobals.typeString /* local name */,
				string.Empty /* namespace */,
				JsonGlobals.arrayString /* value */);
		}

		void WriteObjectAttribute ()
		{
			writer.WriteAttributeString (
				null /* prefix */,
				JsonGlobals.typeString /* local name */,
				null /* namespace */,
				JsonGlobals.objectString /* value */);
		}

		void WriteValue (Type memberType, object memberValue)
		{
			Pointer memberValueRefPointer = null;
			if (memberType.IsPointer)
				memberValueRefPointer = (Pointer) JsonFormatGeneratorStatics.BoxPointer.Invoke (null, new object [] {memberValue, memberType});
			bool isNullableOfT = (memberType.IsGenericType &&
				memberType.GetGenericTypeDefinition() == Globals.TypeOfNullable);
			if (memberType.IsValueType && !isNullableOfT)
			{
				PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(memberType);
				if (primitiveContract != null)
					primitiveContract.XmlFormatContentWriterMethod.Invoke (writer, new object [] {memberValue});
				else
					InternalSerialize (XmlFormatGeneratorStatics.InternalSerializeMethod, () => memberValue, memberType, false);
			}
			else
			{
				bool isNull;
				if (isNullableOfT)
					memberValue = UnwrapNullableObject(() => memberValue, ref memberType, out isNull); //Leaves !HasValue on stack
				else
					isNull = memberValue == null;
				if (isNull)
					XmlFormatGeneratorStatics.WriteNullMethod.Invoke (context, new object [] {writer, memberType, DataContract.IsTypeSerializable(memberType)});
				else {
					PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(memberType);
					if (primitiveContract != null && primitiveContract.UnderlyingType != Globals.TypeOfObject) {
						if (isNullableOfT)
							primitiveContract.XmlFormatContentWriterMethod.Invoke (writer, new object [] {memberValue});
						else							
							primitiveContract.XmlFormatContentWriterMethod.Invoke (context, new object [] {writer, memberValue});
					} else {
						bool isNull2 = false;
						if (memberType == Globals.TypeOfObject || //boxed Nullable<T>
							memberType == Globals.TypeOfValueType ||
							((IList)Globals.TypeOfNullable.GetInterfaces()).Contains(memberType)) {
							var unwrappedMemberValue = CodeInterpreter.ConvertValue (memberValue, memberType.GetType (), Globals.TypeOfObject);
							memberValue = unwrappedMemberValue;
							isNull2 = memberValue == null;
						}
						if (isNull2) {
							XmlFormatGeneratorStatics.WriteNullMethod.Invoke (context, new object [] {writer, memberType, DataContract.IsTypeSerializable(memberType)});
						} else {
							InternalSerialize((isNullableOfT ? XmlFormatGeneratorStatics.InternalSerializeMethod : XmlFormatGeneratorStatics.InternalSerializeReferenceMethod),
								() => memberValue, memberType, false);
						}
					}
				}
			}
		}

		void InternalSerialize (MethodInfo methodInfo, Func<object> memberValue, Type memberType, bool writeXsiType)
		{
			var v = memberValue ();
			var typeHandleValue = Type.GetTypeHandle (v);
			var isDeclaredType = typeHandleValue.Equals (CodeInterpreter.ConvertValue (v, memberType, Globals.TypeOfObject));
			try {
				methodInfo.Invoke (context, new object [] {writer, memberValue != null ? v : null, isDeclaredType, writeXsiType, DataContract.GetId (memberType.TypeHandle), memberType.TypeHandle});
			} catch (TargetInvocationException ex) {
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw;
			}
		}

		object UnwrapNullableObject(Func<object> memberValue, ref Type memberType, out bool isNull)// Leaves !HasValue on stack
		{
			object v = memberValue ();
			isNull = false;
			while (memberType.IsGenericType && memberType.GetGenericTypeDefinition () == Globals.TypeOfNullable) {
				Type innerType = memberType.GetGenericArguments () [0];
				if ((bool) XmlFormatGeneratorStatics.GetHasValueMethod.MakeGenericMethod (innerType).Invoke (null, new object [] {v}))
					v = XmlFormatGeneratorStatics.GetNullableValueMethod.MakeGenericMethod (innerType).Invoke (null, new object [] {v});
				else {
					isNull = true;
					v = XmlFormatGeneratorStatics.GetDefaultValueMethod.MakeGenericMethod (memberType).Invoke (null, new object [0]);
				}
				memberType = innerType;
			}
			
			return v;
		}

		bool TryWritePrimitive(Type type, Func<object> value, MemberInfo memberInfo, int? arrayItemIndex, XmlDictionaryString name, int nameIndex)
		{
			PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(type);
			if (primitiveContract == null || primitiveContract.UnderlyingType == Globals.TypeOfObject)
				return false;

			object callee = null;
			var args = new List<object> ();

			// load writer
			if (type.IsValueType)
				callee = writer;
			else {
				callee = context;
				args.Add (writer);
			}
			// load primitive value 
			if (value != null)
				args.Add (value ());
			else if (memberInfo != null)
				args.Add (CodeInterpreter.GetMember (memberInfo, objLocal));
			else
				args.Add (((Array) objLocal).GetValue (new int [] {(int) arrayItemIndex}));
			// load name
			if (name != null)
				args.Add (name);
			else
				args.Add (memberNames [nameIndex]);
			// load namespace
			args.Add (null);
			// call method to write primitive
			primitiveContract.XmlFormatWriterMethod.Invoke (callee, args.ToArray ());
			return true;
		}

		bool TryWritePrimitiveArray (Type type, Type itemType, Func<object> value, XmlDictionaryString itemName)
		{
			PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(itemType);
			if (primitiveContract == null)
				return false;

			string writeArrayMethod = null;
			switch (Type.GetTypeCode(itemType))
			{
			case TypeCode.Boolean:
				writeArrayMethod = "WriteJsonBooleanArray";
				break;
			case TypeCode.DateTime:
				writeArrayMethod = "WriteJsonDateTimeArray";
				break;
			case TypeCode.Decimal:
				writeArrayMethod = "WriteJsonDecimalArray";
				break;
			case TypeCode.Int32:
				writeArrayMethod = "WriteJsonInt32Array";
				break;
			case TypeCode.Int64:
				writeArrayMethod = "WriteJsonInt64Array";
				break;
			case TypeCode.Single:
				writeArrayMethod = "WriteJsonSingleArray";
				break;
			case TypeCode.Double:
				writeArrayMethod = "WriteJsonDoubleArray";
				break;
			default:
				break;
			}
			if (writeArrayMethod != null)
			{
				WriteArrayAttribute ();
				typeof(JsonWriterDelegator).GetMethod(writeArrayMethod, Globals.ScanAllMembers, null, new Type[] { type, typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null).Invoke (writer, new object [] {value (), itemName, null});
				return true;
			}
			return false;
		}

		object LoadMemberValue (DataMember member)
		{
			return CodeInterpreter.GetMember (member.MemberInfo, objLocal);
		}
	}
}

