using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace System.Runtime.Serialization
{
	internal partial class XmlFormatWriterGenerator
	{
		partial class CriticalHelper
		{
			internal XmlFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
			{
				return (XmlWriterDelegator xw, object obj, XmlObjectSerializerWriteContext ctx, ClassDataContract ctr) => new XmlFormatWriterInterpreter (classContract).WriteToXml (xw, obj, ctx, ctr);
			}

			internal XmlFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
			{
				return (XmlWriterDelegator xw, object obj, XmlObjectSerializerWriteContext ctx, CollectionDataContract ctr) => new XmlFormatWriterInterpreter (collectionContract).WriteCollectionToXml (xw, obj, ctx, ctr);
			}
		}
	}

	class XmlFormatWriterInterpreter
	{
		public XmlFormatWriterInterpreter (ClassDataContract classContract)
		{
			this.classContract = classContract;
		}

		public XmlFormatWriterInterpreter (CollectionDataContract collectionContract)
		{
			this.collectionContract = collectionContract;
		}

		ClassDataContract classContract;

		CollectionDataContract collectionContract;

		XmlWriterDelegator writer = null;
		object obj = null;
		XmlObjectSerializerWriteContext ctx = null;
		DataContract dataContract = null;
		object objLocal = null;

		ClassDataContract classDataContract {
			get { return (ClassDataContract) dataContract; }
		}
		CollectionDataContract collectionDataContract {
			get {return (CollectionDataContract) dataContract; }
		}

		XmlDictionaryString [] contractNamespaces = null;
		XmlDictionaryString [] memberNames = null;
		XmlDictionaryString [] childElementNamespaces = null;
		int typeIndex = 1;
		int childElementIndex = 0;

		public void WriteToXml (XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context, ClassDataContract dataContract)
		{
			this.writer = xmlWriter;
			this.obj = obj;
			this.ctx = context;
			this.dataContract = dataContract;

			InitArgs (classContract.UnderlyingType);

			// DemandSerializationFormatterPermission (classContract) - irrelevant
			// DemandMemberAccessPermission (memberAccessFlag) - irrelevant

			if (classContract.IsReadOnlyContract)
			{
				DataContract.ThrowInvalidDataContractException (classContract.SerializationExceptionMessage, null);
			}

			WriteClass (classContract);
		}

		public void WriteCollectionToXml (XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context, CollectionDataContract collectionContract)
		{
			this.writer = xmlWriter;
			this.obj = obj;
			this.ctx = context;
			this.dataContract = collectionContract;

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

		void InvokeOnSerializing (ClassDataContract classContract, object objSerialized, XmlObjectSerializerWriteContext ctx)
		{
			if (classContract.BaseContract != null)
				InvokeOnSerializing (classContract.BaseContract, objSerialized, ctx);
			if (classContract.OnSerializing != null) {
				classContract.OnSerializing.Invoke (objSerialized, new object [] {ctx.GetStreamingContext ()});
			}
		}

		void InvokeOnSerialized (ClassDataContract classContract, object objSerialized, XmlObjectSerializerWriteContext ctx)
		{
			if (classContract.BaseContract != null)
				InvokeOnSerialized (classContract.BaseContract, objSerialized, ctx);
			if (classContract.OnSerialized != null) {
				classContract.OnSerialized.Invoke (objSerialized, new object [] {ctx.GetStreamingContext ()});
			}
		}

		void WriteClass (ClassDataContract classContract)
		{
			InvokeOnSerializing (classContract, objLocal, ctx);

			if (classContract.IsISerializable)
				ctx.WriteISerializable (writer, (ISerializable) objLocal);
			else
			{
				if (classContract.ContractNamespaces.Length > 1)
					contractNamespaces = classDataContract.ContractNamespaces;
				memberNames = classDataContract.MemberNames;

				for (int i = 0; i < classContract.ChildElementNamespaces.Length; i++)
				{
					if (classContract.ChildElementNamespaces[i] != null)
					{
						childElementNamespaces = classDataContract.ChildElementNamespaces;
					}
				}

				if (classContract.HasExtensionData)
				{
					ExtensionDataObject extensionData = ((IExtensibleDataObject) objLocal).ExtensionData;
					ctx.WriteExtensionData (writer, extensionData, -1);

					WriteMembers (classContract, extensionData, classContract);
				}
				else
					WriteMembers (classContract, null, classContract);
			}
			InvokeOnSerialized (classContract, objLocal, ctx);
		}

		void WriteCollection(CollectionDataContract collectionContract)
		{
			XmlDictionaryString itemNamespace = dataContract.Namespace;

			XmlDictionaryString itemName = collectionDataContract.CollectionItemName;

			if (collectionContract.ChildElementNamespace != null)
				writer.WriteNamespaceDecl (collectionDataContract.ChildElementNamespace);

			if (collectionContract.Kind == CollectionKind.Array)
			{
				Type itemType = collectionContract.ItemType;
				int i;

				// This check does not exist in the original dynamic code,
				// but there is no other way to check type mismatch.
				// CollectionSerialization.ArrayContract() shows that it is required.
				if (objLocal.GetType ().GetElementType () != itemType)
					throw new InvalidCastException (string.Format ("Cannot cast array of {0} to array of {1}", objLocal.GetType ().GetElementType (), itemType));

				ctx.IncrementArrayCount (writer, (Array) objLocal);

				if (!TryWritePrimitiveArray(collectionContract.UnderlyingType, itemType, () => objLocal, itemName, itemNamespace))
				{
					var arr = (Array) objLocal;
					var idx = new int [1];
					for (i = 0; i < arr.Length; i++) {
						if (!TryWritePrimitive(itemType, null, null, i, itemNamespace, itemName, 0)) {
							WriteStartElement (itemType, collectionContract.Namespace, itemNamespace, itemName, 0);
							idx [0] = i;
							var mbrVal = arr.GetValue (idx);
							WriteValue (itemType, mbrVal, false);
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
					incrementCollectionCountMethod.Invoke (ctx, new object [] {writer, objLocal});

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
							moveNextMethod = XmlFormatGeneratorStatics.MoveNextMethod;
						if (getCurrentMethod == null)
							getCurrentMethod = XmlFormatGeneratorStatics.GetCurrentMethod;
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
					enumerator = new CollectionDataContract.DictionaryEnumerator ((IDictionaryEnumerator) enumerator);
				}
				else if (isGenericDictionary)
				{
					Type ctorParam = Globals.TypeOfIEnumeratorGeneric.MakeGenericType(Globals.TypeOfKeyValuePair.MakeGenericType(keyValueTypes));
					ConstructorInfo dictEnumCtor = enumeratorType.GetConstructor(Globals.ScanAllMembers, null, new Type[] { ctorParam }, null);
					enumerator = (IEnumerator) Activator.CreateInstance (enumeratorType, new object [] {enumerator});
				}

				var emptyArray = new object [0];
				while (enumerator != null && enumerator.MoveNext ()) {
					currentValue = getCurrentMethod.Invoke (enumerator, emptyArray);

					if (incrementCollectionCountMethod == null)
						XmlFormatGeneratorStatics.IncrementItemCountMethod.Invoke (ctx, new object [] {1});

					if (!TryWritePrimitive (elementType, () => currentValue, null, null, itemNamespace, itemName, 0))
					{
						WriteStartElement (elementType, collectionContract.Namespace, itemNamespace, itemName, 0);
						if (isGenericDictionary || isDictionary)
							collectionDataContract.ItemContract.WriteXmlValue (writer, currentValue, ctx);
						else
							WriteValue (elementType, currentValue, false);
						WriteEndElement();
					}
				}
			}
		}

		int WriteMembers (ClassDataContract classContract, ExtensionDataObject extensionData, ClassDataContract derivedMostClassContract)
		{
			int memberCount = (classContract.BaseContract == null) ? 0 : WriteMembers (classContract.BaseContract, extensionData, derivedMostClassContract);

			XmlDictionaryString ns = 
				(contractNamespaces == null) ? dataContract.Namespace :
				contractNamespaces [typeIndex - 1];

			ctx.IncrementItemCount (classContract.Members.Count);

			for (int i = 0; i < classContract.Members.Count; i++, memberCount++) {

				DataMember member = classContract.Members[i];
				Type memberType = member.MemberType;
				object memberValue = null;
				if (member.IsGetOnlyCollection)
					ctx.StoreIsGetOnlyCollection ();
				bool doWrite = true, hasMemberValue = false;
				if (!member.EmitDefaultValue)
				{
					hasMemberValue = true;
					memberValue = LoadMemberValue (member);
					doWrite = !IsDefaultValue (memberType, memberValue);
				}

				if (doWrite) {

					bool writeXsiType = CheckIfMemberHasConflict (member, classContract, derivedMostClassContract);
					if (writeXsiType || !TryWritePrimitive (memberType, hasMemberValue ? () => memberValue : (Func<object>) null, member.MemberInfo, null /*arrayItemIndex*/, ns, null /*nameLocal*/, i + childElementIndex)) {
						WriteStartElement (memberType, classContract.Namespace, ns, null /*nameLocal*/, i + childElementIndex);
						if (classContract.ChildElementNamespaces [i + childElementIndex] != null)
							writer.WriteNamespaceDecl (childElementNamespaces [i + childElementIndex]);
						if (memberValue == null)
							memberValue = LoadMemberValue (member);
						WriteValue (memberType, memberValue, writeXsiType);
						WriteEndElement ();
					}

					if (classContract.HasExtensionData)
						ctx.WriteExtensionData (writer, extensionData, memberCount);
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

		bool CheckIfMemberHasConflict(DataMember member, ClassDataContract classContract, ClassDataContract derivedMostClassContract)
		{
			// Check for conflict with base type members
			if (CheckIfConflictingMembersHaveDifferentTypes(member))
				return true;

			// Check for conflict with derived type members
			string name = member.Name;
			string ns = classContract.StableName.Namespace;
			ClassDataContract currentContract = derivedMostClassContract;
			while (currentContract != null && currentContract != classContract)
			{
				if (ns == currentContract.StableName.Namespace)
				{
					List<DataMember> members = currentContract.Members;
					for (int j = 0; j < members.Count; j++)
					{
						if (name == members[j].Name)
							return CheckIfConflictingMembersHaveDifferentTypes(members[j]);
					}
				}
				currentContract = currentContract.BaseContract;
			}

			return false;
		}

		bool CheckIfConflictingMembersHaveDifferentTypes(DataMember member)
		{
			while (member.ConflictingMember != null)
			{
				if (member.MemberType != member.ConflictingMember.MemberType)
					return true;
				member = member.ConflictingMember;
			}
			return false;
		}

		bool NeedsPrefix(Type type, XmlDictionaryString ns)
		{
			return type == Globals.TypeOfXmlQualifiedName && (ns != null && ns.Value != null && ns.Value.Length > 0);
		}

		void WriteStartElement (Type type, XmlDictionaryString ns, XmlDictionaryString namespaceLocal, XmlDictionaryString nameLocal, int nameIndex)
		{
			bool needsPrefix = NeedsPrefix(type, ns);
			nameLocal = nameLocal ?? memberNames [nameIndex];
			if (needsPrefix)
				writer.WriteStartElement (Globals.ElementPrefix, nameLocal, namespaceLocal);
			else
				writer.WriteStartElement (nameLocal, namespaceLocal);
		}

		void WriteEndElement ()
		{
			writer.WriteEndElement ();
		}

		void WriteValue (Type memberType, object memberValue, bool writeXsiType)
		{
			Pointer memberValueRefPointer = null;
			if (memberType.IsPointer)
				memberValueRefPointer = (Pointer) XmlFormatGeneratorStatics.BoxPointer.Invoke (null, new object [] {memberValue, memberType});
			bool isNullableOfT = (memberType.IsGenericType &&
				memberType.GetGenericTypeDefinition() == Globals.TypeOfNullable);
			if (memberType.IsValueType && !isNullableOfT)
			{
				PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(memberType);
				if (primitiveContract != null && !writeXsiType)
					primitiveContract.XmlFormatContentWriterMethod.Invoke (writer, new object [] {memberValue});
				else
					InternalSerialize(XmlFormatGeneratorStatics.InternalSerializeMethod, () => memberValue, memberType, writeXsiType);
			}
			else
			{
				bool isNull;
				if (isNullableOfT)
					memberValue = UnwrapNullableObject(() => memberValue, ref memberType, out isNull); //Leaves !HasValue on stack
				else
					isNull = memberValue == null;
				if (isNull)
					XmlFormatGeneratorStatics.WriteNullMethod.Invoke (ctx, new object [] {writer, memberType, DataContract.IsTypeSerializable(memberType)});
				else {
					PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(memberType);
					if (primitiveContract != null && primitiveContract.UnderlyingType != Globals.TypeOfObject && !writeXsiType) {
						if (isNullableOfT)
							primitiveContract.XmlFormatContentWriterMethod.Invoke (writer, new object [] {memberValue});
						else							
							primitiveContract.XmlFormatContentWriterMethod.Invoke (ctx, new object [] {writer, memberValue});
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
							XmlFormatGeneratorStatics.WriteNullMethod.Invoke (ctx, new object [] {writer, memberType, DataContract.IsTypeSerializable(memberType)});
						} else {
							InternalSerialize((isNullableOfT ? XmlFormatGeneratorStatics.InternalSerializeMethod : XmlFormatGeneratorStatics.InternalSerializeReferenceMethod),
								() => memberValue, memberType, writeXsiType);
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
			methodInfo.Invoke (ctx, new object [] {writer, memberValue != null ? v : null, isDeclaredType, writeXsiType, DataContract.GetId (memberType.TypeHandle), memberType.TypeHandle});
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

		bool TryWritePrimitive(Type type, Func<object> value, MemberInfo memberInfo, int? arrayItemIndex, XmlDictionaryString ns, XmlDictionaryString name, int nameIndex)
		{
			PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(type);
			if (primitiveContract == null || primitiveContract.UnderlyingType == Globals.TypeOfObject)
				return false;

			object callee = null;
			var args = new List<object> ();

			// load xmlwriter
			if (type.IsValueType)
				callee = writer;
			else {
				callee = ctx;
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
			args.Add (ns);
			// call method to write primitive
			primitiveContract.XmlFormatWriterMethod.Invoke (callee, args.ToArray ());
			return true;
		}

		bool TryWritePrimitiveArray (Type type, Type itemType, Func<object> value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
		{
			PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(itemType);
			if (primitiveContract == null)
				return false;

			string writeArrayMethod = null;
			switch (Type.GetTypeCode(itemType))
			{
			case TypeCode.Boolean:
				writeArrayMethod = "WriteBooleanArray";
				break;
			case TypeCode.DateTime:
				writeArrayMethod = "WriteDateTimeArray";
				break;
			case TypeCode.Decimal:
				writeArrayMethod = "WriteDecimalArray";
				break;
			case TypeCode.Int32:
				writeArrayMethod = "WriteInt32Array";
				break;
			case TypeCode.Int64:
				writeArrayMethod = "WriteInt64Array";
				break;
			case TypeCode.Single:
				writeArrayMethod = "WriteSingleArray";
				break;
			case TypeCode.Double:
				writeArrayMethod = "WriteDoubleArray";
				break;
			default:
				break;
			}
			if (writeArrayMethod != null)
			{
				typeof (XmlWriterDelegator).GetMethod (writeArrayMethod, Globals.ScanAllMembers, null, new Type[] { type, typeof (XmlDictionaryString), typeof (XmlDictionaryString) }, null).Invoke (writer, new object [] {value (), itemName, itemNamespace});
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
