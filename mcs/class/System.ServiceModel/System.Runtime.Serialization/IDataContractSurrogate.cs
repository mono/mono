using System;
using System.CodeDom;
using System.Reflection;

namespace System.Runtime.Serialization
{
	public interface IDataContractSurrogate
	{
		object GetCustomDataToExport (MemberInfo memberInfo,
			Type dataContractType);
		object GetCustomDataToExport (Type clrType,
			Type dataContractType);

		Type GetDataContractType (Type type);

		object GetDeserializedObject (object obj, Type targetType);

		void GetKnownCustomDataTypes (
			KnownTypeCollection customDataTypes);

		object GetObjectToSerialize (object obj, Type targetType);

		Type GetReferencedTypeOnImport (string typeName,
			string typeNamespace, object customData);

		CodeTypeDeclaration ProcessImportedType (
			CodeTypeDeclaration typeDeclaration,
			CodeCompileUnit compileUnit);

	}
}
