// It is automatically generated
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace DbLinq.Schema.Dbml
{
	#if !MONO_STRICT
	public
	#endif
	class GeneratedReader : XmlSerializationReader
	{
		static readonly System.Reflection.MethodInfo fromBinHexStringMethod = typeof (XmlConvert).GetMethod ("FromBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new System.Type [] {typeof (string)}, null);
		static byte [] FromBinHexString (string input)
		{
			return input == null ? null : (byte []) fromBinHexStringMethod.Invoke (null, new object [] {input});
		}
		public object ReadRoot_Database ()
		{
			Reader.MoveToContent();
			if (Reader.LocalName != "Database" || Reader.NamespaceURI != "http://schemas.microsoft.com/linqtosql/dbml/2007")
				throw CreateUnknownNodeException();
			return ReadObject_Database (false, true);
		}

		public DbLinq.Schema.Dbml.Database ReadObject_Database (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.Database ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Database" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.Database) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.Database), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "EntityNamespace" && Reader.NamespaceURI == "") {
					ob.@EntityNamespace = Reader.Value;
				}
				else if (Reader.LocalName == "ContextNamespace" && Reader.NamespaceURI == "") {
					ob.@ContextNamespace = Reader.Value;
				}
				else if (Reader.LocalName == "Class" && Reader.NamespaceURI == "") {
					ob.@Class = Reader.Value;
				}
				else if (Reader.LocalName == "AccessModifier" && Reader.NamespaceURI == "") {
					ob.@AccessModifier = GetEnumValue_AccessModifier (Reader.Value);
					ob.AccessModifierSpecified = true;
				}
				else if (Reader.LocalName == "Modifier" && Reader.NamespaceURI == "") {
					ob.@Modifier = GetEnumValue_ClassModifier (Reader.Value);
					ob.ModifierSpecified = true;
				}
				else if (Reader.LocalName == "BaseType" && Reader.NamespaceURI == "") {
					ob.@BaseType = Reader.Value;
				}
				else if (Reader.LocalName == "Provider" && Reader.NamespaceURI == "") {
					ob.@Provider = Reader.Value;
				}
				else if (Reader.LocalName == "ExternalMapping" && Reader.NamespaceURI == "") {
					ob.@ExternalMapping = XmlConvert.ToBoolean (Reader.Value);
					ob.ExternalMappingSpecified = true;
				}
				else if (Reader.LocalName == "Serialization" && Reader.NamespaceURI == "") {
					ob.@Serialization = GetEnumValue_SerializationMode (Reader.Value);
					ob.SerializationSpecified = true;
				}
				else if (Reader.LocalName == "EntityBase" && Reader.NamespaceURI == "") {
					ob.@EntityBase = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b0=false, b1=false, b2=false;

			DbLinq.Schema.Dbml.Table[] o4;
			o4 = null;
			DbLinq.Schema.Dbml.Function[] o6;
			o6 = null;
			int n3=0, n5=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Table" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b1) {
						o4 = (DbLinq.Schema.Dbml.Table[]) EnsureArrayIndex (o4, n3, typeof(DbLinq.Schema.Dbml.Table));
						o4[n3] = ReadObject_Table (false, true);
						n3++;
					}
					else if (Reader.LocalName == "Function" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b2) {
						o6 = (DbLinq.Schema.Dbml.Function[]) EnsureArrayIndex (o6, n5, typeof(DbLinq.Schema.Dbml.Function));
						o6[n5] = ReadObject_Function (false, true);
						n5++;
					}
					else if (Reader.LocalName == "Connection" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b0) {
						b0 = true;
						ob.@Connection = ReadObject_Connection (false, true);
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			o4 = (DbLinq.Schema.Dbml.Table[]) ShrinkArray (o4, n3, typeof(DbLinq.Schema.Dbml.Table), true);
			ob.@Table = o4;
			o6 = (DbLinq.Schema.Dbml.Function[]) ShrinkArray (o6, n5, typeof(DbLinq.Schema.Dbml.Function), true);
			ob.@Function = o6;

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.AccessModifier ReadObject_AccessModifier (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.AccessModifier res = GetEnumValue_AccessModifier (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.AccessModifier GetEnumValue_AccessModifier (string xmlName)
		{
			switch (xmlName)
			{
				case "Public": return DbLinq.Schema.Dbml.AccessModifier.Public;
				case "Internal": return DbLinq.Schema.Dbml.AccessModifier.Internal;
				case "Protected": return DbLinq.Schema.Dbml.AccessModifier.Protected;
				case "ProtectedInternal": return DbLinq.Schema.Dbml.AccessModifier.ProtectedInternal;
				case "Private": return DbLinq.Schema.Dbml.AccessModifier.Private;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.AccessModifier));
			}
		}

		public DbLinq.Schema.Dbml.ClassModifier ReadObject_ClassModifier (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.ClassModifier res = GetEnumValue_ClassModifier (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.ClassModifier GetEnumValue_ClassModifier (string xmlName)
		{
			switch (xmlName)
			{
				case "Sealed": return DbLinq.Schema.Dbml.ClassModifier.Sealed;
				case "Abstract": return DbLinq.Schema.Dbml.ClassModifier.Abstract;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.ClassModifier));
			}
		}

		public DbLinq.Schema.Dbml.SerializationMode ReadObject_SerializationMode (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.SerializationMode res = GetEnumValue_SerializationMode (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.SerializationMode GetEnumValue_SerializationMode (string xmlName)
		{
			switch (xmlName)
			{
				case "None": return DbLinq.Schema.Dbml.SerializationMode.None;
				case "Unidirectional": return DbLinq.Schema.Dbml.SerializationMode.Unidirectional;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.SerializationMode));
			}
		}

		public DbLinq.Schema.Dbml.Table ReadObject_Table (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.Table ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Table" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.Table) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.Table), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "Member" && Reader.NamespaceURI == "") {
					ob.@Member = Reader.Value;
				}
				else if (Reader.LocalName == "AccessModifier" && Reader.NamespaceURI == "") {
					ob.@AccessModifier = GetEnumValue_AccessModifier (Reader.Value);
					ob.AccessModifierSpecified = true;
				}
				else if (Reader.LocalName == "Modifier" && Reader.NamespaceURI == "") {
					ob.@Modifier = GetEnumValue_MemberModifier (Reader.Value);
					ob.ModifierSpecified = true;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b7=false, b8=false, b9=false, b10=false;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "UpdateFunction" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b9) {
						b9 = true;
						ob.@UpdateFunction = ReadObject_TableFunction (false, true);
					}
					else if (Reader.LocalName == "Type" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b7) {
						b7 = true;
						ob.@Type = ReadObject_Type (false, true);
					}
					else if (Reader.LocalName == "InsertFunction" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b8) {
						b8 = true;
						ob.@InsertFunction = ReadObject_TableFunction (false, true);
					}
					else if (Reader.LocalName == "DeleteFunction" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b10) {
						b10 = true;
						ob.@DeleteFunction = ReadObject_TableFunction (false, true);
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.Function ReadObject_Function (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.Function ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Function" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.Function) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.Function), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "Id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "Method" && Reader.NamespaceURI == "") {
					ob.@Method = Reader.Value;
				}
				else if (Reader.LocalName == "AccessModifier" && Reader.NamespaceURI == "") {
					ob.@AccessModifier = GetEnumValue_AccessModifier (Reader.Value);
					ob.AccessModifierSpecified = true;
				}
				else if (Reader.LocalName == "Modifier" && Reader.NamespaceURI == "") {
					ob.@Modifier = GetEnumValue_MemberModifier (Reader.Value);
					ob.ModifierSpecified = true;
				}
				else if (Reader.LocalName == "HasMultipleResults" && Reader.NamespaceURI == "") {
					ob.@HasMultipleResults = XmlConvert.ToBoolean (Reader.Value);
					ob.HasMultipleResultsSpecified = true;
				}
				else if (Reader.LocalName == "IsComposable" && Reader.NamespaceURI == "") {
					ob.@IsComposable = XmlConvert.ToBoolean (Reader.Value);
					ob.IsComposableSpecified = true;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b11=false, b12=false;

			DbLinq.Schema.Dbml.Parameter[] o14;
			o14 = null;
			System.Object[] o16;
			o16 = null;
			int n13=0, n15=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Return" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b12) {
						o16 = (System.Object[]) EnsureArrayIndex (o16, n15, typeof(System.Object));
						o16[n15] = ReadObject_Return (false, true);
						n15++;
					}
					else if (Reader.LocalName == "ElementType" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b12) {
						o16 = (System.Object[]) EnsureArrayIndex (o16, n15, typeof(System.Object));
						o16[n15] = ReadObject_Type (false, true);
						n15++;
					}
					else if (Reader.LocalName == "Parameter" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b11) {
						o14 = (DbLinq.Schema.Dbml.Parameter[]) EnsureArrayIndex (o14, n13, typeof(DbLinq.Schema.Dbml.Parameter));
						o14[n13] = ReadObject_Parameter (false, true);
						n13++;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			o14 = (DbLinq.Schema.Dbml.Parameter[]) ShrinkArray (o14, n13, typeof(DbLinq.Schema.Dbml.Parameter), true);
			ob.@Parameter = o14;
			o16 = (System.Object[]) ShrinkArray (o16, n15, typeof(System.Object), true);
			ob.@Items = o16;

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.Connection ReadObject_Connection (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.Connection ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Connection" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.Connection) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.Connection), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Provider" && Reader.NamespaceURI == "") {
					ob.@Provider = Reader.Value;
				}
				else if (Reader.LocalName == "Mode" && Reader.NamespaceURI == "") {
					ob.@Mode = GetEnumValue_ConnectionMode (Reader.Value);
					ob.ModeSpecified = true;
				}
				else if (Reader.LocalName == "ConnectionString" && Reader.NamespaceURI == "") {
					ob.@ConnectionString = Reader.Value;
				}
				else if (Reader.LocalName == "SettingsObjectName" && Reader.NamespaceURI == "") {
					ob.@SettingsObjectName = Reader.Value;
				}
				else if (Reader.LocalName == "SettingsPropertyName" && Reader.NamespaceURI == "") {
					ob.@SettingsPropertyName = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.MemberModifier ReadObject_MemberModifier (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.MemberModifier res = GetEnumValue_MemberModifier (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.MemberModifier GetEnumValue_MemberModifier (string xmlName)
		{
			switch (xmlName)
			{
				case "Virtual": return DbLinq.Schema.Dbml.MemberModifier.Virtual;
				case "Override": return DbLinq.Schema.Dbml.MemberModifier.Override;
				case "New": return DbLinq.Schema.Dbml.MemberModifier.New;
				case "NewVirtual": return DbLinq.Schema.Dbml.MemberModifier.NewVirtual;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.MemberModifier));
			}
		}

		public DbLinq.Schema.Dbml.TableFunction ReadObject_TableFunction (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.TableFunction ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "TableFunction" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.TableFunction) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.TableFunction), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "FunctionId" && Reader.NamespaceURI == "") {
					ob.@FunctionId = Reader.Value;
				}
				else if (Reader.LocalName == "AccessModifier" && Reader.NamespaceURI == "") {
					ob.@AccessModifier = GetEnumValue_AccessModifier (Reader.Value);
					ob.AccessModifierSpecified = true;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b17=false, b18=false;

			DbLinq.Schema.Dbml.TableFunctionParameter[] o20;
			o20 = null;
			int n19=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Return" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b18) {
						b18 = true;
						ob.@Return = ReadObject_TableFunctionReturn (false, true);
					}
					else if (Reader.LocalName == "Argument" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b17) {
						o20 = (DbLinq.Schema.Dbml.TableFunctionParameter[]) EnsureArrayIndex (o20, n19, typeof(DbLinq.Schema.Dbml.TableFunctionParameter));
						o20[n19] = ReadObject_TableFunctionParameter (false, true);
						n19++;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			o20 = (DbLinq.Schema.Dbml.TableFunctionParameter[]) ShrinkArray (o20, n19, typeof(DbLinq.Schema.Dbml.TableFunctionParameter), true);
			ob.@Argument = o20;

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.Type ReadObject_Type (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.Type ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Type" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.Type) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.Type), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "IdRef" && Reader.NamespaceURI == "") {
					ob.@IdRef = Reader.Value;
				}
				else if (Reader.LocalName == "Id" && Reader.NamespaceURI == "") {
					ob.@Id = Reader.Value;
				}
				else if (Reader.LocalName == "Name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "InheritanceCode" && Reader.NamespaceURI == "") {
					ob.@InheritanceCode = Reader.Value;
				}
				else if (Reader.LocalName == "IsInheritanceDefault" && Reader.NamespaceURI == "") {
					ob.@IsInheritanceDefault = XmlConvert.ToBoolean (Reader.Value);
					ob.IsInheritanceDefaultSpecified = true;
				}
				else if (Reader.LocalName == "AccessModifier" && Reader.NamespaceURI == "") {
					ob.@AccessModifier = GetEnumValue_AccessModifier (Reader.Value);
					ob.AccessModifierSpecified = true;
				}
				else if (Reader.LocalName == "Modifier" && Reader.NamespaceURI == "") {
					ob.@Modifier = GetEnumValue_ClassModifier (Reader.Value);
					ob.ModifierSpecified = true;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			bool b21=false, b22=false;

			System.Object[] o24;
			o24 = null;
			DbLinq.Schema.Dbml.Type[] o26;
			o26 = null;
			int n23=0, n25=0;

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					if (Reader.LocalName == "Column" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b21) {
						o24 = (System.Object[]) EnsureArrayIndex (o24, n23, typeof(System.Object));
						o24[n23] = ReadObject_Column (false, true);
						n23++;
					}
					else if (Reader.LocalName == "Association" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b21) {
						o24 = (System.Object[]) EnsureArrayIndex (o24, n23, typeof(System.Object));
						o24[n23] = ReadObject_Association (false, true);
						n23++;
					}
					else if (Reader.LocalName == "Type" && Reader.NamespaceURI == "http://schemas.microsoft.com/linqtosql/dbml/2007" && !b22) {
						o26 = (DbLinq.Schema.Dbml.Type[]) EnsureArrayIndex (o26, n25, typeof(DbLinq.Schema.Dbml.Type));
						o26[n25] = ReadObject_Type (false, true);
						n25++;
					}
					else {
						UnknownNode (ob);
					}
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			o24 = (System.Object[]) ShrinkArray (o24, n23, typeof(System.Object), true);
			ob.@Items = o24;
			o26 = (DbLinq.Schema.Dbml.Type[]) ShrinkArray (o26, n25, typeof(DbLinq.Schema.Dbml.Type), true);
			ob.@Type1 = o26;

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.Return ReadObject_Return (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.Return ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Return" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.Return) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.Return), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Type" && Reader.NamespaceURI == "") {
					ob.@Type = Reader.Value;
				}
				else if (Reader.LocalName == "DbType" && Reader.NamespaceURI == "") {
					ob.@DbType = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.Parameter ReadObject_Parameter (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.Parameter ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Parameter" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.Parameter) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.Parameter), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "Parameter" && Reader.NamespaceURI == "") {
					ob.@Parameter1 = Reader.Value;
				}
				else if (Reader.LocalName == "Type" && Reader.NamespaceURI == "") {
					ob.@Type = Reader.Value;
				}
				else if (Reader.LocalName == "DbType" && Reader.NamespaceURI == "") {
					ob.@DbType = Reader.Value;
				}
				else if (Reader.LocalName == "Direction" && Reader.NamespaceURI == "") {
					ob.@Direction = GetEnumValue_ParameterDirection (Reader.Value);
					ob.DirectionSpecified = true;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.ConnectionMode ReadObject_ConnectionMode (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.ConnectionMode res = GetEnumValue_ConnectionMode (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.ConnectionMode GetEnumValue_ConnectionMode (string xmlName)
		{
			switch (xmlName)
			{
				case "ConnectionString": return DbLinq.Schema.Dbml.ConnectionMode.ConnectionString;
				case "AppSettings": return DbLinq.Schema.Dbml.ConnectionMode.AppSettings;
				case "WebSettings": return DbLinq.Schema.Dbml.ConnectionMode.WebSettings;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.ConnectionMode));
			}
		}

		public DbLinq.Schema.Dbml.TableFunctionReturn ReadObject_TableFunctionReturn (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.TableFunctionReturn ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "TableFunctionReturn" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.TableFunctionReturn) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.TableFunctionReturn), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Member" && Reader.NamespaceURI == "") {
					ob.@Member = Reader.Value;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.TableFunctionParameter ReadObject_TableFunctionParameter (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.TableFunctionParameter ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "TableFunctionParameter" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.TableFunctionParameter) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.TableFunctionParameter), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Parameter" && Reader.NamespaceURI == "") {
					ob.@Parameter = Reader.Value;
				}
				else if (Reader.LocalName == "Member" && Reader.NamespaceURI == "") {
					ob.@Member = Reader.Value;
				}
				else if (Reader.LocalName == "Version" && Reader.NamespaceURI == "") {
					ob.@Version = GetEnumValue_Version (Reader.Value);
					ob.VersionSpecified = true;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.Column ReadObject_Column (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.Column ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Column" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.Column) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.Column), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "Member" && Reader.NamespaceURI == "") {
					ob.@Member = Reader.Value;
				}
				else if (Reader.LocalName == "Storage" && Reader.NamespaceURI == "") {
					ob.@Storage = Reader.Value;
				}
				else if (Reader.LocalName == "AccessModifier" && Reader.NamespaceURI == "") {
					ob.@AccessModifier = GetEnumValue_AccessModifier (Reader.Value);
					ob.AccessModifierSpecified = true;
				}
				else if (Reader.LocalName == "Modifier" && Reader.NamespaceURI == "") {
					ob.@Modifier = GetEnumValue_MemberModifier (Reader.Value);
					ob.ModifierSpecified = true;
				}
				else if (Reader.LocalName == "Type" && Reader.NamespaceURI == "") {
					ob.@Type = Reader.Value;
				}
				else if (Reader.LocalName == "DbType" && Reader.NamespaceURI == "") {
					ob.@DbType = Reader.Value;
				}
				else if (Reader.LocalName == "IsReadOnly" && Reader.NamespaceURI == "") {
					ob.@IsReadOnly = XmlConvert.ToBoolean (Reader.Value);
					ob.IsReadOnlySpecified = true;
				}
				else if (Reader.LocalName == "IsPrimaryKey" && Reader.NamespaceURI == "") {
					ob.@IsPrimaryKey = XmlConvert.ToBoolean (Reader.Value);
					ob.IsPrimaryKeySpecified = true;
				}
				else if (Reader.LocalName == "IsDbGenerated" && Reader.NamespaceURI == "") {
					ob.@IsDbGenerated = XmlConvert.ToBoolean (Reader.Value);
					ob.IsDbGeneratedSpecified = true;
				}
				else if (Reader.LocalName == "CanBeNull" && Reader.NamespaceURI == "") {
					ob.@CanBeNull = XmlConvert.ToBoolean (Reader.Value);
					ob.CanBeNullSpecified = true;
				}
				else if (Reader.LocalName == "UpdateCheck" && Reader.NamespaceURI == "") {
					ob.@UpdateCheck = GetEnumValue_UpdateCheck (Reader.Value);
					ob.UpdateCheckSpecified = true;
				}
				else if (Reader.LocalName == "IsDiscriminator" && Reader.NamespaceURI == "") {
					ob.@IsDiscriminator = XmlConvert.ToBoolean (Reader.Value);
					ob.IsDiscriminatorSpecified = true;
				}
				else if (Reader.LocalName == "Expression" && Reader.NamespaceURI == "") {
					ob.@Expression = Reader.Value;
				}
				else if (Reader.LocalName == "IsVersion" && Reader.NamespaceURI == "") {
					ob.@IsVersion = XmlConvert.ToBoolean (Reader.Value);
					ob.IsVersionSpecified = true;
				}
				else if (Reader.LocalName == "IsDelayLoaded" && Reader.NamespaceURI == "") {
					ob.@IsDelayLoaded = XmlConvert.ToBoolean (Reader.Value);
					ob.IsDelayLoadedSpecified = true;
				}
				else if (Reader.LocalName == "AutoSync" && Reader.NamespaceURI == "") {
					ob.@AutoSync = GetEnumValue_AutoSync (Reader.Value);
					ob.AutoSyncSpecified = true;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.Association ReadObject_Association (bool isNullable, bool checkType)
		{
			DbLinq.Schema.Dbml.Association ob = null;
			if (isNullable && ReadNull()) return null;

			if (checkType) 
			{
				System.Xml.XmlQualifiedName t = GetXsiType();
				if (t == null)
				{ }
				else if (t.Name != "Association" || t.Namespace != "http://schemas.microsoft.com/linqtosql/dbml/2007")
					throw CreateUnknownTypeException(t);
			}

			ob = (DbLinq.Schema.Dbml.Association) Activator.CreateInstance(typeof(DbLinq.Schema.Dbml.Association), true);

			Reader.MoveToElement();

			while (Reader.MoveToNextAttribute())
			{
				if (Reader.LocalName == "Name" && Reader.NamespaceURI == "") {
					ob.@Name = Reader.Value;
				}
				else if (Reader.LocalName == "Member" && Reader.NamespaceURI == "") {
					ob.@Member = Reader.Value;
				}
				else if (Reader.LocalName == "Storage" && Reader.NamespaceURI == "") {
					ob.@Storage = Reader.Value;
				}
				else if (Reader.LocalName == "AccessModifier" && Reader.NamespaceURI == "") {
					ob.@AccessModifier = GetEnumValue_AccessModifier (Reader.Value);
					ob.AccessModifierSpecified = true;
				}
				else if (Reader.LocalName == "Modifier" && Reader.NamespaceURI == "") {
					ob.@Modifier = GetEnumValue_MemberModifier (Reader.Value);
					ob.ModifierSpecified = true;
				}
				else if (Reader.LocalName == "Type" && Reader.NamespaceURI == "") {
					ob.@Type = Reader.Value;
				}
				else if (Reader.LocalName == "ThisKey" && Reader.NamespaceURI == "") {
					ob.@ThisKey = Reader.Value;
				}
				else if (Reader.LocalName == "OtherKey" && Reader.NamespaceURI == "") {
					ob.@OtherKey = Reader.Value;
				}
				else if (Reader.LocalName == "IsForeignKey" && Reader.NamespaceURI == "") {
					ob.@IsForeignKey = XmlConvert.ToBoolean (Reader.Value);
					ob.IsForeignKeySpecified = true;
				}
				else if (Reader.LocalName == "Cardinality" && Reader.NamespaceURI == "") {
					ob.@Cardinality = GetEnumValue_Cardinality (Reader.Value);
					ob.CardinalitySpecified = true;
				}
				else if (Reader.LocalName == "DeleteRule" && Reader.NamespaceURI == "") {
					ob.@DeleteRule = Reader.Value;
				}
				else if (Reader.LocalName == "DeleteOnNull" && Reader.NamespaceURI == "") {
					ob.@DeleteOnNull = XmlConvert.ToBoolean (Reader.Value);
					ob.DeleteOnNullSpecified = true;
				}
				else if (IsXmlnsAttribute (Reader.Name)) {
				}
				else {
					UnknownNode (ob);
				}
			}

			Reader.MoveToElement ();
			Reader.MoveToElement();
			if (Reader.IsEmptyElement) {
				Reader.Skip ();
				return ob;
			}

			Reader.ReadStartElement();
			Reader.MoveToContent();

			while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) 
			{
				if (Reader.NodeType == System.Xml.XmlNodeType.Element) 
				{
					UnknownNode (ob);
				}
				else
					UnknownNode(ob);

				Reader.MoveToContent();
			}

			ReadEndElement();

			return ob;
		}

		public DbLinq.Schema.Dbml.ParameterDirection ReadObject_ParameterDirection (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.ParameterDirection res = GetEnumValue_ParameterDirection (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.ParameterDirection GetEnumValue_ParameterDirection (string xmlName)
		{
			switch (xmlName)
			{
				case "In": return DbLinq.Schema.Dbml.ParameterDirection.In;
				case "Out": return DbLinq.Schema.Dbml.ParameterDirection.Out;
				case "InOut": return DbLinq.Schema.Dbml.ParameterDirection.InOut;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.ParameterDirection));
			}
		}

		public DbLinq.Schema.Dbml.Version ReadObject_Version (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.Version res = GetEnumValue_Version (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.Version GetEnumValue_Version (string xmlName)
		{
			switch (xmlName)
			{
				case "Current": return DbLinq.Schema.Dbml.Version.Current;
				case "Original": return DbLinq.Schema.Dbml.Version.Original;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.Version));
			}
		}

		public DbLinq.Schema.Dbml.UpdateCheck ReadObject_UpdateCheck (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.UpdateCheck res = GetEnumValue_UpdateCheck (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.UpdateCheck GetEnumValue_UpdateCheck (string xmlName)
		{
			switch (xmlName)
			{
				case "Always": return DbLinq.Schema.Dbml.UpdateCheck.Always;
				case "Never": return DbLinq.Schema.Dbml.UpdateCheck.Never;
				case "WhenChanged": return DbLinq.Schema.Dbml.UpdateCheck.WhenChanged;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.UpdateCheck));
			}
		}

		public DbLinq.Schema.Dbml.AutoSync ReadObject_AutoSync (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.AutoSync res = GetEnumValue_AutoSync (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.AutoSync GetEnumValue_AutoSync (string xmlName)
		{
			switch (xmlName)
			{
				case "Never": return DbLinq.Schema.Dbml.AutoSync.Never;
				case "OnInsert": return DbLinq.Schema.Dbml.AutoSync.OnInsert;
				case "OnUpdate": return DbLinq.Schema.Dbml.AutoSync.OnUpdate;
				case "Always": return DbLinq.Schema.Dbml.AutoSync.Always;
				case "Default": return DbLinq.Schema.Dbml.AutoSync.Default;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.AutoSync));
			}
		}

		public DbLinq.Schema.Dbml.Cardinality ReadObject_Cardinality (bool isNullable, bool checkType)
		{
			Reader.ReadStartElement ();
			DbLinq.Schema.Dbml.Cardinality res = GetEnumValue_Cardinality (Reader.ReadString());
			if (Reader.NodeType != XmlNodeType.None)
			Reader.ReadEndElement ();
			return res;
		}

		DbLinq.Schema.Dbml.Cardinality GetEnumValue_Cardinality (string xmlName)
		{
			switch (xmlName)
			{
				case "One": return DbLinq.Schema.Dbml.Cardinality.One;
				case "Many": return DbLinq.Schema.Dbml.Cardinality.Many;
				default:
					throw CreateUnknownConstantException (xmlName, typeof(DbLinq.Schema.Dbml.Cardinality));
			}
		}

		protected override void InitCallbacks ()
		{
		}

		protected override void InitIDs ()
		{
		}

	}

	#if !MONO_STRICT
    public
    #endif
	class GeneratedWriter : XmlSerializationWriter
	{
		const string xmlNamespace = "http://www.w3.org/2000/xmlns/";
		static readonly System.Reflection.MethodInfo toBinHexStringMethod = typeof (XmlConvert).GetMethod ("ToBinHexString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new System.Type [] {typeof (byte [])}, null);
		static string ToBinHexString (byte [] input)
		{
			return input == null ? null : (string) toBinHexStringMethod.Invoke (null, new object [] {input});
		}
		public void WriteRoot_Database (object o)
		{
			WriteStartDocument ();
			DbLinq.Schema.Dbml.Database ob = (DbLinq.Schema.Dbml.Database) o;
			TopLevelElement ();
			WriteObject_Database (ob, "Database", "http://schemas.microsoft.com/linqtosql/dbml/2007", true, false, true);
		}

		void WriteObject_Database (DbLinq.Schema.Dbml.Database ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Database))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Database", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Name", "", ob.@Name);
			WriteAttribute ("EntityNamespace", "", ob.@EntityNamespace);
			WriteAttribute ("ContextNamespace", "", ob.@ContextNamespace);
			WriteAttribute ("Class", "", ob.@Class);
			if (ob.@AccessModifierSpecified) {
				WriteAttribute ("AccessModifier", "", GetEnumValue_AccessModifier (ob.@AccessModifier));
			}
			if (ob.@ModifierSpecified) {
				WriteAttribute ("Modifier", "", GetEnumValue_ClassModifier (ob.@Modifier));
			}
			WriteAttribute ("BaseType", "", ob.@BaseType);
			WriteAttribute ("Provider", "", ob.@Provider);
			if (ob.@ExternalMappingSpecified) {
				WriteAttribute ("ExternalMapping", "", (ob.@ExternalMapping?"true":"false"));
			}
			if (ob.@SerializationSpecified) {
				WriteAttribute ("Serialization", "", GetEnumValue_SerializationMode (ob.@Serialization));
			}
			WriteAttribute ("EntityBase", "", ob.@EntityBase);

			WriteObject_Connection (ob.@Connection, "Connection", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
			if (ob.@Table != null) {
				for (int n27 = 0; n27 < ob.@Table.Length; n27++) {
					WriteObject_Table (ob.@Table[n27], "Table", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
				}
			}
			if (ob.@Function != null) {
				for (int n28 = 0; n28 < ob.@Function.Length; n28++) {
					WriteObject_Function (ob.@Function[n28], "Function", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_AccessModifier (DbLinq.Schema.Dbml.AccessModifier ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.AccessModifier))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("AccessModifier", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_AccessModifier (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_AccessModifier (DbLinq.Schema.Dbml.AccessModifier val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.AccessModifier.Public: return "Public";
				case DbLinq.Schema.Dbml.AccessModifier.Internal: return "Internal";
				case DbLinq.Schema.Dbml.AccessModifier.Protected: return "Protected";
				case DbLinq.Schema.Dbml.AccessModifier.ProtectedInternal: return "ProtectedInternal";
				case DbLinq.Schema.Dbml.AccessModifier.Private: return "Private";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.AccessModifier).FullName);
			}
		}

		void WriteObject_ClassModifier (DbLinq.Schema.Dbml.ClassModifier ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.ClassModifier))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("ClassModifier", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_ClassModifier (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_ClassModifier (DbLinq.Schema.Dbml.ClassModifier val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.ClassModifier.Sealed: return "Sealed";
				case DbLinq.Schema.Dbml.ClassModifier.Abstract: return "Abstract";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.ClassModifier).FullName);
			}
		}

		void WriteObject_SerializationMode (DbLinq.Schema.Dbml.SerializationMode ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.SerializationMode))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("SerializationMode", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_SerializationMode (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_SerializationMode (DbLinq.Schema.Dbml.SerializationMode val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.SerializationMode.None: return "None";
				case DbLinq.Schema.Dbml.SerializationMode.Unidirectional: return "Unidirectional";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.SerializationMode).FullName);
			}
		}

		void WriteObject_Connection (DbLinq.Schema.Dbml.Connection ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Connection))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Connection", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Provider", "", ob.@Provider);
			if (ob.@ModeSpecified) {
				WriteAttribute ("Mode", "", GetEnumValue_ConnectionMode (ob.@Mode));
			}
			WriteAttribute ("ConnectionString", "", ob.@ConnectionString);
			WriteAttribute ("SettingsObjectName", "", ob.@SettingsObjectName);
			WriteAttribute ("SettingsPropertyName", "", ob.@SettingsPropertyName);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Table (DbLinq.Schema.Dbml.Table ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Table))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Table", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Name", "", ob.@Name);
			WriteAttribute ("Member", "", ob.@Member);
			if (ob.@AccessModifierSpecified) {
				WriteAttribute ("AccessModifier", "", GetEnumValue_AccessModifier (ob.@AccessModifier));
			}
			if (ob.@ModifierSpecified) {
				WriteAttribute ("Modifier", "", GetEnumValue_MemberModifier (ob.@Modifier));
			}

			WriteObject_Type (ob.@Type, "Type", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
			WriteObject_TableFunction (ob.@InsertFunction, "InsertFunction", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
			WriteObject_TableFunction (ob.@UpdateFunction, "UpdateFunction", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
			WriteObject_TableFunction (ob.@DeleteFunction, "DeleteFunction", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Function (DbLinq.Schema.Dbml.Function ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Function))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Function", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Name", "", ob.@Name);
			WriteAttribute ("Id", "", ob.@Id);
			WriteAttribute ("Method", "", ob.@Method);
			if (ob.@AccessModifierSpecified) {
				WriteAttribute ("AccessModifier", "", GetEnumValue_AccessModifier (ob.@AccessModifier));
			}
			if (ob.@ModifierSpecified) {
				WriteAttribute ("Modifier", "", GetEnumValue_MemberModifier (ob.@Modifier));
			}
			if (ob.@HasMultipleResultsSpecified) {
				WriteAttribute ("HasMultipleResults", "", (ob.@HasMultipleResults?"true":"false"));
			}
			if (ob.@IsComposableSpecified) {
				WriteAttribute ("IsComposable", "", (ob.@IsComposable?"true":"false"));
			}

			if (ob.@Parameter != null) {
				for (int n29 = 0; n29 < ob.@Parameter.Length; n29++) {
					WriteObject_Parameter (ob.@Parameter[n29], "Parameter", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
				}
			}
			if (ob.@Items != null) {
				for (int n30 = 0; n30 < ob.@Items.Length; n30++) {
					if (((object)ob.@Items[n30]) == null) { }
					else if (ob.@Items[n30].GetType() == typeof(DbLinq.Schema.Dbml.Return)) {
						WriteObject_Return (((DbLinq.Schema.Dbml.Return) ob.@Items[n30]), "Return", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
					}
					else if (ob.@Items[n30].GetType() == typeof(DbLinq.Schema.Dbml.Type)) {
						WriteObject_Type (((DbLinq.Schema.Dbml.Type) ob.@Items[n30]), "ElementType", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n30]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_ConnectionMode (DbLinq.Schema.Dbml.ConnectionMode ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.ConnectionMode))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("ConnectionMode", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_ConnectionMode (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_ConnectionMode (DbLinq.Schema.Dbml.ConnectionMode val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.ConnectionMode.ConnectionString: return "ConnectionString";
				case DbLinq.Schema.Dbml.ConnectionMode.AppSettings: return "AppSettings";
				case DbLinq.Schema.Dbml.ConnectionMode.WebSettings: return "WebSettings";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.ConnectionMode).FullName);
			}
		}

		void WriteObject_MemberModifier (DbLinq.Schema.Dbml.MemberModifier ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.MemberModifier))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("MemberModifier", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_MemberModifier (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_MemberModifier (DbLinq.Schema.Dbml.MemberModifier val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.MemberModifier.Virtual: return "Virtual";
				case DbLinq.Schema.Dbml.MemberModifier.Override: return "Override";
				case DbLinq.Schema.Dbml.MemberModifier.New: return "New";
				case DbLinq.Schema.Dbml.MemberModifier.NewVirtual: return "NewVirtual";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.MemberModifier).FullName);
			}
		}

		void WriteObject_Type (DbLinq.Schema.Dbml.Type ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Type))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Type", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("IdRef", "", ob.@IdRef);
			WriteAttribute ("Id", "", ob.@Id);
			WriteAttribute ("Name", "", ob.@Name);
			WriteAttribute ("InheritanceCode", "", ob.@InheritanceCode);
			if (ob.@IsInheritanceDefaultSpecified) {
				WriteAttribute ("IsInheritanceDefault", "", (ob.@IsInheritanceDefault?"true":"false"));
			}
			if (ob.@AccessModifierSpecified) {
				WriteAttribute ("AccessModifier", "", GetEnumValue_AccessModifier (ob.@AccessModifier));
			}
			if (ob.@ModifierSpecified) {
				WriteAttribute ("Modifier", "", GetEnumValue_ClassModifier (ob.@Modifier));
			}

			if (ob.@Items != null) {
				for (int n31 = 0; n31 < ob.@Items.Length; n31++) {
					if (((object)ob.@Items[n31]) == null) { }
					else if (ob.@Items[n31].GetType() == typeof(DbLinq.Schema.Dbml.Column)) {
						WriteObject_Column (((DbLinq.Schema.Dbml.Column) ob.@Items[n31]), "Column", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
					}
					else if (ob.@Items[n31].GetType() == typeof(DbLinq.Schema.Dbml.Association)) {
						WriteObject_Association (((DbLinq.Schema.Dbml.Association) ob.@Items[n31]), "Association", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n31]);
				}
			}
			if (ob.@Type1 != null) {
				for (int n32 = 0; n32 < ob.@Type1.Length; n32++) {
					WriteObject_Type (ob.@Type1[n32], "Type", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_TableFunction (DbLinq.Schema.Dbml.TableFunction ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.TableFunction))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("TableFunction", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("FunctionId", "", ob.@FunctionId);
			if (ob.@AccessModifierSpecified) {
				WriteAttribute ("AccessModifier", "", GetEnumValue_AccessModifier (ob.@AccessModifier));
			}

			if (ob.@Argument != null) {
				for (int n33 = 0; n33 < ob.@Argument.Length; n33++) {
					WriteObject_TableFunctionParameter (ob.@Argument[n33], "Argument", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
				}
			}
			WriteObject_TableFunctionReturn (ob.@Return, "Return", "http://schemas.microsoft.com/linqtosql/dbml/2007", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Parameter (DbLinq.Schema.Dbml.Parameter ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Parameter))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Parameter", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Name", "", ob.@Name);
			WriteAttribute ("Parameter", "", ob.@Parameter1);
			WriteAttribute ("Type", "", ob.@Type);
			WriteAttribute ("DbType", "", ob.@DbType);
			if (ob.@DirectionSpecified) {
				WriteAttribute ("Direction", "", GetEnumValue_ParameterDirection (ob.@Direction));
			}

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Return (DbLinq.Schema.Dbml.Return ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Return))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Return", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Type", "", ob.@Type);
			WriteAttribute ("DbType", "", ob.@DbType);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Column (DbLinq.Schema.Dbml.Column ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Column))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Column", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Name", "", ob.@Name);
			WriteAttribute ("Member", "", ob.@Member);
			WriteAttribute ("Storage", "", ob.@Storage);
			if (ob.@AccessModifierSpecified) {
				WriteAttribute ("AccessModifier", "", GetEnumValue_AccessModifier (ob.@AccessModifier));
			}
			if (ob.@ModifierSpecified) {
				WriteAttribute ("Modifier", "", GetEnumValue_MemberModifier (ob.@Modifier));
			}
			WriteAttribute ("Type", "", ob.@Type);
			WriteAttribute ("DbType", "", ob.@DbType);
			if (ob.@IsReadOnlySpecified) {
				WriteAttribute ("IsReadOnly", "", (ob.@IsReadOnly?"true":"false"));
			}
			if (ob.@IsPrimaryKeySpecified) {
				WriteAttribute ("IsPrimaryKey", "", (ob.@IsPrimaryKey?"true":"false"));
			}
			if (ob.@IsDbGeneratedSpecified) {
				WriteAttribute ("IsDbGenerated", "", (ob.@IsDbGenerated?"true":"false"));
			}
			if (ob.@CanBeNullSpecified) {
				WriteAttribute ("CanBeNull", "", (ob.@CanBeNull?"true":"false"));
			}
			if (ob.@UpdateCheckSpecified) {
				WriteAttribute ("UpdateCheck", "", GetEnumValue_UpdateCheck (ob.@UpdateCheck));
			}
			if (ob.@IsDiscriminatorSpecified) {
				WriteAttribute ("IsDiscriminator", "", (ob.@IsDiscriminator?"true":"false"));
			}
			WriteAttribute ("Expression", "", ob.@Expression);
			if (ob.@IsVersionSpecified) {
				WriteAttribute ("IsVersion", "", (ob.@IsVersion?"true":"false"));
			}
			if (ob.@IsDelayLoadedSpecified) {
				WriteAttribute ("IsDelayLoaded", "", (ob.@IsDelayLoaded?"true":"false"));
			}
			if (ob.@AutoSyncSpecified) {
				WriteAttribute ("AutoSync", "", GetEnumValue_AutoSync (ob.@AutoSync));
			}

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_Association (DbLinq.Schema.Dbml.Association ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Association))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Association", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Name", "", ob.@Name);
			WriteAttribute ("Member", "", ob.@Member);
			WriteAttribute ("Storage", "", ob.@Storage);
			if (ob.@AccessModifierSpecified) {
				WriteAttribute ("AccessModifier", "", GetEnumValue_AccessModifier (ob.@AccessModifier));
			}
			if (ob.@ModifierSpecified) {
				WriteAttribute ("Modifier", "", GetEnumValue_MemberModifier (ob.@Modifier));
			}
			WriteAttribute ("Type", "", ob.@Type);
			WriteAttribute ("ThisKey", "", ob.@ThisKey);
			WriteAttribute ("OtherKey", "", ob.@OtherKey);
			if (ob.@IsForeignKeySpecified) {
				WriteAttribute ("IsForeignKey", "", (ob.@IsForeignKey?"true":"false"));
			}
			if (ob.@CardinalitySpecified) {
				WriteAttribute ("Cardinality", "", GetEnumValue_Cardinality (ob.@Cardinality));
			}
			WriteAttribute ("DeleteRule", "", ob.@DeleteRule);
			if (ob.@DeleteOnNullSpecified) {
				WriteAttribute ("DeleteOnNull", "", (ob.@DeleteOnNull?"true":"false"));
			}

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_TableFunctionParameter (DbLinq.Schema.Dbml.TableFunctionParameter ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.TableFunctionParameter))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("TableFunctionParameter", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Parameter", "", ob.@Parameter);
			WriteAttribute ("Member", "", ob.@Member);
			if (ob.@VersionSpecified) {
				WriteAttribute ("Version", "", GetEnumValue_Version (ob.@Version));
			}

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_TableFunctionReturn (DbLinq.Schema.Dbml.TableFunctionReturn ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (((object)ob) == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.TableFunctionReturn))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("TableFunctionReturn", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			WriteAttribute ("Member", "", ob.@Member);

			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_ParameterDirection (DbLinq.Schema.Dbml.ParameterDirection ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.ParameterDirection))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("ParameterDirection", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_ParameterDirection (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_ParameterDirection (DbLinq.Schema.Dbml.ParameterDirection val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.ParameterDirection.In: return "In";
				case DbLinq.Schema.Dbml.ParameterDirection.Out: return "Out";
				case DbLinq.Schema.Dbml.ParameterDirection.InOut: return "InOut";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.ParameterDirection).FullName);
			}
		}

		void WriteObject_UpdateCheck (DbLinq.Schema.Dbml.UpdateCheck ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.UpdateCheck))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("UpdateCheck", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_UpdateCheck (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_UpdateCheck (DbLinq.Schema.Dbml.UpdateCheck val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.UpdateCheck.Always: return "Always";
				case DbLinq.Schema.Dbml.UpdateCheck.Never: return "Never";
				case DbLinq.Schema.Dbml.UpdateCheck.WhenChanged: return "WhenChanged";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.UpdateCheck).FullName);
			}
		}

		void WriteObject_AutoSync (DbLinq.Schema.Dbml.AutoSync ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.AutoSync))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("AutoSync", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_AutoSync (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_AutoSync (DbLinq.Schema.Dbml.AutoSync val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.AutoSync.Never: return "Never";
				case DbLinq.Schema.Dbml.AutoSync.OnInsert: return "OnInsert";
				case DbLinq.Schema.Dbml.AutoSync.OnUpdate: return "OnUpdate";
				case DbLinq.Schema.Dbml.AutoSync.Always: return "Always";
				case DbLinq.Schema.Dbml.AutoSync.Default: return "Default";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.AutoSync).FullName);
			}
		}

		void WriteObject_Cardinality (DbLinq.Schema.Dbml.Cardinality ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Cardinality))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Cardinality", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_Cardinality (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_Cardinality (DbLinq.Schema.Dbml.Cardinality val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.Cardinality.One: return "One";
				case DbLinq.Schema.Dbml.Cardinality.Many: return "Many";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.Cardinality).FullName);
			}
		}

		void WriteObject_Version (DbLinq.Schema.Dbml.Version ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(DbLinq.Schema.Dbml.Version))
			{ }
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("Version", "http://schemas.microsoft.com/linqtosql/dbml/2007");

			Writer.WriteString (GetEnumValue_Version (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}

		string GetEnumValue_Version (DbLinq.Schema.Dbml.Version val)
		{
			switch (val) {
				case DbLinq.Schema.Dbml.Version.Current: return "Current";
				case DbLinq.Schema.Dbml.Version.Original: return "Original";
				default: throw CreateInvalidEnumValueException ((long) val, typeof (DbLinq.Schema.Dbml.Version).FullName);
			}
		}

		protected override void InitCallbacks ()
		{
		}

	}


	#if !MONO_STRICT
    public
      #endif
	class BaseXmlSerializer : System.Xml.Serialization.XmlSerializer
	{
		protected override System.Xml.Serialization.XmlSerializationReader CreateReader () {
			return new GeneratedReader ();
		}

		protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter () {
			return new GeneratedWriter ();
		}

		public override bool CanDeserialize (System.Xml.XmlReader xmlReader) {
			return true;
		}
	}

    #if !MONO_STRICT
    public
    #endif
	sealed class DatabaseSerializer : BaseXmlSerializer
	{
		protected override void Serialize (object obj, System.Xml.Serialization.XmlSerializationWriter writer) {
			((GeneratedWriter)writer).WriteRoot_Database(obj);
		}

		protected override object Deserialize (System.Xml.Serialization.XmlSerializationReader reader) {
			return ((GeneratedReader)reader).ReadRoot_Database();
		}
	}

	#if !TARGET_JVM
	#if !MONO_STRICT
    public
    #endif
	class XmlSerializerContract : System.Xml.Serialization.XmlSerializerImplementation
	{
		System.Collections.Hashtable readMethods = null;
		System.Collections.Hashtable writeMethods = null;
		System.Collections.Hashtable typedSerializers = null;

		public override System.Xml.Serialization.XmlSerializationReader Reader {
			get {
				return new GeneratedReader();
			}
		}

		public override System.Xml.Serialization.XmlSerializationWriter Writer {
			get {
				return new GeneratedWriter();
			}
		}

		public override System.Collections.Hashtable ReadMethods {
			get {
				lock (this) {
					if (readMethods == null) {
						readMethods = new System.Collections.Hashtable ();
						readMethods.Add (@"DbLinq.Schema.Dbml.Database", @"ReadRoot_Database");
					}
					return readMethods;
				}
			}
		}

		public override System.Collections.Hashtable WriteMethods {
			get {
				lock (this) {
					if (writeMethods == null) {
						writeMethods = new System.Collections.Hashtable ();
						writeMethods.Add (@"DbLinq.Schema.Dbml.Database", @"WriteRoot_Database");
					}
					return writeMethods;
				}
			}
		}

		public override System.Collections.Hashtable TypedSerializers {
			get {
				lock (this) {
					if (typedSerializers == null) {
						typedSerializers = new System.Collections.Hashtable ();
						typedSerializers.Add (@"DbLinq.Schema.Dbml.Database", new DatabaseSerializer());
					}
					return typedSerializers;
				}
			}
		}

		public XmlSerializer GetSerializer (System.Type type)
		{
			switch (type.FullName) {
			case "DbLinq.Schema.Dbml.Database":
				return (XmlSerializer) TypedSerializers ["DbLinq.Schema.Dbml.Database"];

			}
			return base.GetSerializer (type);
		}

		public override bool CanSerialize (System.Type type) {
			if (type == typeof(DbLinq.Schema.Dbml.Database)) return true;
			return false;
		}
	}

	#endif
}

