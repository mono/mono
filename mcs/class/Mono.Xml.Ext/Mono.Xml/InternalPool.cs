
#if NET_2_0

using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml
{
	internal class InternalPool
	{
		public const string XdtNamespace = "http://www.w3.org/2003/11/xpath-datatypes";

		internal static readonly XmlQualifiedName XsAnyTypeName
			= new XmlQualifiedName ("any", XmlSchema.Namespace);
		internal static readonly XmlSchemaComplexType XsAnyType
			= XmlSchemaType.GetBuiltInComplexType (XsAnyTypeName);

		internal static readonly XmlQualifiedName XdtUntypedAtomicName
			= new XmlQualifiedName ("untypedAtomic", XdtNamespace);
		internal static readonly XmlSchemaSimpleType XdtUntypedAtomic
			= XmlSchemaType.GetBuiltInSimpleType (XdtUntypedAtomicName);

		internal static readonly XmlQualifiedName XsStringName
			= new XmlQualifiedName ("string", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsString
			= XmlSchemaType.GetBuiltInSimpleType (XsStringName);
		internal static readonly XmlQualifiedName XsNormalizedStringName
			= new XmlQualifiedName ("normalizedString", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsNormalizedString
			= XmlSchemaType.GetBuiltInSimpleType (XsNormalizedStringName);
		internal static readonly XmlQualifiedName XsTokenName
			= new XmlQualifiedName ("token", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsToken
			= XmlSchemaType.GetBuiltInSimpleType (XsTokenName);
		internal static readonly XmlQualifiedName XsIDName
			= new XmlQualifiedName ("ID", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsID
			= XmlSchemaType.GetBuiltInSimpleType (XsIDName);
		internal static readonly XmlQualifiedName XsIDREFName
			= new XmlQualifiedName ("IDREF", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsIDREF
			= XmlSchemaType.GetBuiltInSimpleType (XsIDREFName);
		internal static readonly XmlQualifiedName XsIDREFSName
			= new XmlQualifiedName ("IDREFS", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsIDREFS
			= XmlSchemaType.GetBuiltInSimpleType (XsIDREFSName);
		internal static readonly XmlQualifiedName XsQNameName
			= new XmlQualifiedName ("QName", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsQName
			= XmlSchemaType.GetBuiltInSimpleType (XsQNameName);
		internal static readonly XmlQualifiedName XsBooleanName
			= new XmlQualifiedName ("boolean", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsBoolean
			= XmlSchemaType.GetBuiltInSimpleType (XsBooleanName);

		internal static readonly XmlQualifiedName XsDurationName
			= new XmlQualifiedName ("duration", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsDuration
			= XmlSchemaType.GetBuiltInSimpleType (XsDurationName);
		internal static readonly XmlQualifiedName XdtDayTimeDurationName
			= new XmlQualifiedName ("dayTimeDuration", InternalPool.XdtNamespace);
		internal static readonly XmlSchemaSimpleType XdtDayTimeDuration
			= XmlSchemaType.GetBuiltInSimpleType (XdtDayTimeDurationName);
		internal static readonly XmlQualifiedName XdtYearMonthDurationName
			= new XmlQualifiedName ("yearMonthDuration", InternalPool.XdtNamespace);
		internal static readonly XmlSchemaSimpleType XdtYearMonthDuration
			= XmlSchemaType.GetBuiltInSimpleType (XdtYearMonthDurationName);

		internal static readonly XmlQualifiedName XsDateTimeName
			= new XmlQualifiedName ("dateTime", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsDateTime
			= XmlSchemaType.GetBuiltInSimpleType (XsDateTimeName);

		internal static readonly XmlQualifiedName XsFloatName
			= new XmlQualifiedName ("float", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsFloat
			= XmlSchemaType.GetBuiltInSimpleType (XsFloatName);
		internal static readonly XmlQualifiedName XsDoubleName
			= new XmlQualifiedName ("double", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsDouble
			= XmlSchemaType.GetBuiltInSimpleType (XsDoubleName);
		internal static readonly XmlQualifiedName XsDecimalName
			= new XmlQualifiedName ("decimal", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsDecimal
			= XmlSchemaType.GetBuiltInSimpleType (XsDecimalName);
		internal static readonly XmlQualifiedName XsIntegerName
			= new XmlQualifiedName ("integer", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsInteger
			= XmlSchemaType.GetBuiltInSimpleType (XsIntegerName);

		internal static readonly XmlQualifiedName XsLongName
			= new XmlQualifiedName ("long", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsLong
			= XmlSchemaType.GetBuiltInSimpleType (XsLongName);
		internal static readonly XmlQualifiedName XsIntName
			= new XmlQualifiedName ("int", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsInt
			= XmlSchemaType.GetBuiltInSimpleType (XsIntName);
		internal static readonly XmlQualifiedName XsShortName
			= new XmlQualifiedName ("short", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsShort
			= XmlSchemaType.GetBuiltInSimpleType (XsShortName);
		internal static readonly XmlQualifiedName XsUnsignedLongName
			= new XmlQualifiedName ("unsignedLong", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsUnsignedLong
			= XmlSchemaType.GetBuiltInSimpleType (XsUnsignedLongName);
		internal static readonly XmlQualifiedName XsUnsignedIntName
			= new XmlQualifiedName ("unsignedInt", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsUnsignedInt
			= XmlSchemaType.GetBuiltInSimpleType (XsUnsignedIntName);
		internal static readonly XmlQualifiedName XsUnsignedShortName
			= new XmlQualifiedName ("unsignedShort", XmlSchema.Namespace);
		internal static readonly XmlSchemaSimpleType XsUnsignedShort
			= XmlSchemaType.GetBuiltInSimpleType (XsUnsignedShortName);

		// Methods

		public static XmlQualifiedName ParseQName (string name, IXmlNamespaceResolver resolver)
		{
			int index = name.IndexOf (':');
			if (index < 0)
				return new XmlQualifiedName (name);
			string ns = resolver.LookupNamespace (name.Substring (0, index));
			if (ns == null)
				throw new ArgumentException ("Invalid qualified name.");
			return new XmlQualifiedName (name.Substring (index + 1), ns);
		}

		public static XmlSchemaType GetBuiltInType (XmlQualifiedName name)
		{
			XmlSchemaType t = XmlSchemaType.GetBuiltInSimpleType (name);
			if (t != null)
				return t;
			return XmlSchemaType.GetBuiltInComplexType (name);
		}

		public static XmlSchemaType GetBuiltInType (XmlTypeCode type)
		{
			XmlSchemaType t = XmlSchemaType.GetBuiltInSimpleType (type);
			if (t != null)
				return t;
			return XmlSchemaType.GetBuiltInComplexType (type);
		}

		internal static Type RuntimeTypeFromXmlTypeCode (XmlTypeCode typeCode)
		{
			switch (typeCode) {
			case XmlTypeCode.Int:
				return typeof (int);
			case XmlTypeCode.Decimal:
				return typeof (decimal);
			case XmlTypeCode.Double:
				return typeof (double);
			case XmlTypeCode.Float:
				return typeof (float);
			case XmlTypeCode.Long:
				return typeof (long);
			case XmlTypeCode.Short:
				return typeof (short);
			case XmlTypeCode.UnsignedShort:
				return typeof (ushort);
			case XmlTypeCode.UnsignedInt:
				return typeof (uint);
			case XmlTypeCode.String:
				return typeof (string);
			case XmlTypeCode.DateTime:
				return typeof (DateTime);
			case XmlTypeCode.Boolean:
				return typeof (bool);
			case XmlTypeCode.Item:
				return typeof (object);
			}
			throw new NotSupportedException (String.Format ("XQuery internal error: Cannot infer Runtime Type from XmlTypeCode {0}.", typeCode));
		}

		internal static XmlTypeCode XmlTypeCodeFromRuntimeType (Type cliType, bool raiseError)
		{
			switch (Type.GetTypeCode (cliType)) {
			case TypeCode.Int32:
				return XmlTypeCode.Int;
			case TypeCode.Decimal:
				return XmlTypeCode.Decimal;
			case TypeCode.Double:
				return XmlTypeCode.Double;
			case TypeCode.Single:
				return XmlTypeCode.Float;
			case TypeCode.Int64:
				return XmlTypeCode.Long;
			case TypeCode.Int16:
				return XmlTypeCode.Short;
			case TypeCode.UInt16:
				return XmlTypeCode.UnsignedShort;
			case TypeCode.UInt32:
				return XmlTypeCode.UnsignedInt;
			case TypeCode.String:
				return XmlTypeCode.String;
			case TypeCode.DateTime:
				return XmlTypeCode.DateTime;
			case TypeCode.Boolean:
				return XmlTypeCode.Boolean;
			case TypeCode.Object:
				return XmlTypeCode.Item;
			}
			if (raiseError)
				throw new NotSupportedException (String.Format ("XQuery internal error: Cannot infer XmlTypeCode from Runtime Type {0}", cliType));
			else
				return XmlTypeCode.None;
		}
	}
}


#endif
