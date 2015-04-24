using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace System.Runtime.Serialization
{
	internal partial class XmlDataContract
	{
        internal CreateXmlSerializableDelegate GenerateCreateXmlSerializableDelegate()
        {
				return () => new XmlDataContractInterpreter (this).CreateXmlSerializable ();
		}
	}
	
	internal class XmlDataContractInterpreter
	{
		XmlDataContract contract;
		
		public XmlDataContractInterpreter (XmlDataContract contract)
		{
			this.contract = contract;
		}
		
		public IXmlSerializable CreateXmlSerializable ()
		{
			Type type = contract.UnderlyingType;
			object value = null;
			if (type.IsValueType)
				value = FormatterServices.GetUninitializedObject (type);
			else
				value = GetConstructor ().Invoke (new object [0]);
			return (IXmlSerializable) value;
		}

		ConstructorInfo GetConstructor ()
		{
			Type type = contract.UnderlyingType;

			if (type.IsValueType)
				return null;

			ConstructorInfo ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
			if (ctor == null)
				throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.IXmlSerializableMustHaveDefaultConstructor, DataContract.GetClrTypeFullName(type))));

			return ctor;
		}
	}
}

