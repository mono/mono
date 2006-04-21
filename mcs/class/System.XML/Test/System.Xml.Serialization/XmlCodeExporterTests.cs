//
// System.Xml.Serialization.XmlCodeExporterTests
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Novell
// 

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Microsoft.CSharp;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlCodeExporterTests
	{
		[Test]
		public void ExportTypeMapping_ArrayClass ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (ArrayClass));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
				"[System.Diagnostics.DebuggerStepThroughAttribute()]{0}" +
				"[System.ComponentModel.DesignerCategoryAttribute(\"code\")]{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public partial class ArrayClass {{{0}" +
				"    {0}" +
				"    private object namesField;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public object names {{{0}" +
				"        get {{{0}" +
				"            return this.namesField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.namesField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
#else
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public class ArrayClass {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public object names;{0}" +
#endif
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		public void ExportTypeMapping_ArrayContainer ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (ArrayContainer));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
				"[System.Diagnostics.DebuggerStepThroughAttribute()]{0}" +
				"[System.ComponentModel.DesignerCategoryAttribute(\"code\")]{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public partial class ArrayContainer {{{0}" +
				"    {0}" +
				"    private object[] itemsField;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public object[] items {{{0}" +
				"        get {{{0}" +
				"            return this.itemsField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.itemsField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
#else
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public class ArrayContainer {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public object[] items;{0}" +
#endif
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		public void ExportTypeMapping_CDataContainer ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (CDataContainer));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
				"[System.Diagnostics.DebuggerStepThroughAttribute()]{0}" +
				"[System.ComponentModel.DesignerCategoryAttribute(\"code\")]{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public partial class CDataContainer {{{0}" +
				"    {0}" +
				"    private System.Xml.XmlCDataSection cdataField;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public System.Xml.XmlCDataSection cdata {{{0}" +
				"        get {{{0}" +
				"            return this.cdataField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.cdataField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
#else
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public class CDataContainer {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public System.Xml.XmlCDataSection cdata;{0}" +
#endif
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // order of XmlElementAttribute's does not match that of MSFT
		[Category ("NotDotNet")] // Mono bug ##77117 and MS.NET randomly modifies the order of the elements!
		public void ExportTypeMapping_Choices ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (Choices));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
				"[System.Diagnostics.DebuggerStepThroughAttribute()]{0}" +
				"[System.ComponentModel.DesignerCategoryAttribute(\"code\")]{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public partial class Choices {{{0}" +
				"    {0}" +
				"    private string myChoiceField;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlElementAttribute(\"ChoiceZero\", typeof(string))]{0}" +
				"    [System.Xml.Serialization.XmlElementAttribute(\"ChoiceOne\", typeof(string))]{0}" +
				"    [System.Xml.Serialization.XmlElementAttribute(\"ChoiceTwo\", typeof(string))]{0}" +
				"    [System.Xml.Serialization.XmlChoiceIdentifierAttribute(\"ItemType\")]{0}" +
				"    public string MyChoice {{{0}" +
				"        get {{{0}" +
				"            return this.myChoiceField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.myChoiceField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
#else
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public class Choices {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlElementAttribute(\"ChoiceZero\", typeof(string))]{0}" +
				"    [System.Xml.Serialization.XmlElementAttribute(\"ChoiceTwo\", typeof(string))]{0}" +
				"    [System.Xml.Serialization.XmlElementAttribute(\"ChoiceOne\", typeof(string))]{0}" +
				"    [System.Xml.Serialization.XmlChoiceIdentifierAttribute(\"ItemType\")]{0}" +
				"    public string MyChoice;{0}" +
#endif
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
#if NET_2_0
		[Category ("NotDotNet")] // regression in MS.NET 2.0
#endif
		[Category ("NotWorking")] // TODO: order of DefaultValueAttribute, ...
		public void ExportTypeMapping_Field ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (Field));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(\"field\", Namespace=\"\", IsNullable=true)]{0}" +
				"public class Field {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers\")]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers2\")]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers2;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers3\")]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.MapModifiers.Public)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers3 = MonoTests.System.Xml.TestClasses.MapModifiers.Public;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers4\")]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.MapModifiers.Public)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers4 = MonoTests.System.Xml.TestClasses.MapModifiers.Public;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"names\")]{0}" +
				"    public string[] Names;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"street\")]{0}" +
				"    public string Street;{0}" +
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"[System.FlagsAttribute()]{0}" +
				"public enum MapModifiers {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"public\")]{0}" +
				"    Public = 1,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"protected\")]{0}" +
				"    Protected = 2,{0}" +
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		public void ExportTypeMapping_ItemChoiceType ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (ItemChoiceType));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
#endif
				"[System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=false)]{0}" +
				"public enum ItemChoiceType {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    ChoiceZero,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"ChoiceOne\")]{0}" +
				"    StrangeOne,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    ChoiceTwo,{0}" +
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // Mono outputs XmlRootAttribute for SimpleClass too
		public void ExportTypeMapping_ClassArrayContainer ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (ClassArrayContainer));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
				"[System.Diagnostics.DebuggerStepThroughAttribute()]{0}" +
				"[System.ComponentModel.DesignerCategoryAttribute(\"code\")]{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public partial class ClassArrayContainer {{{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.SimpleClass[] itemsField;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public MonoTests.System.Xml.TestClasses.SimpleClass[] items {{{0}" +
				"        get {{{0}" +
				"            return this.itemsField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.itemsField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
				"[System.Diagnostics.DebuggerStepThroughAttribute()]{0}" +
				"[System.ComponentModel.DesignerCategoryAttribute(\"code\")]{0}" +
				"public partial class SimpleClass {{{0}" +
				"    {0}" +
				"    private string somethingField;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public string something {{{0}" +
				"        get {{{0}" +
				"            return this.somethingField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.somethingField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
#else
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=true)]{0}" +
				"public class ClassArrayContainer {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public MonoTests.System.Xml.TestClasses.SimpleClass[] items;{0}" +
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"public class SimpleClass {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public string something;{0}" +
#endif
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		public void ExportTypeMapping_SimpleClassWithXmlAttributes ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (SimpleClassWithXmlAttributes));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
				"[System.Diagnostics.DebuggerStepThroughAttribute()]{0}" +
				"[System.ComponentModel.DesignerCategoryAttribute(\"code\")]{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(\"simple\", Namespace=\"\", IsNullable=true)]{0}" +
				"public partial class SimpleClassWithXmlAttributes {{{0}" +
				"    {0}" +
				"    private string somethingField;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"member\")]{0}" +
				"    public string something {{{0}" +
				"        get {{{0}" +
				"            return this.somethingField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.somethingField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
#else
				"[System.Xml.Serialization.XmlRootAttribute(\"simple\", Namespace=\"\", IsNullable=true)]{0}" +
				"public class SimpleClassWithXmlAttributes {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"member\")]{0}" +
				"    public string something;{0}" +
#endif
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
		public void ExportTypeMapping_ZeroFlagEnum ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (ZeroFlagEnum));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
				"[System.FlagsAttribute()]{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
#endif
				"[System.Xml.Serialization.XmlRootAttribute(Namespace=\"\", IsNullable=false)]{0}" +
				"public enum ZeroFlagEnum {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"zero\")]{0}" +
				"    e0 = 1,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"o<n>e\")]{0}" +
				"    e1 = 2,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"tns:t<w>o\")]{0}" +
				"    e2 = 4,{0}" +
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		CodeNamespace ExportCode (Type type)
		{
			XmlReflectionImporter imp = new XmlReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (type);
			CodeNamespace codeNamespace = new CodeNamespace ();
			XmlCodeExporter exp = new XmlCodeExporter (codeNamespace);
			exp.ExportTypeMapping (map);
			return codeNamespace;
		}
	}
}
