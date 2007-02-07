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
#if NET_2_0
		[Ignore ("This test does not really consider upgrades.")]
#endif
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

			codeNamespace = ExportCode (typeof (ArrayClass[]));
			Assert.IsNotNull (codeNamespace, "#3");

			sw.GetStringBuilder ().Length = 0;
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
				"[System.Diagnostics.DebuggerStepThroughAttribute()]{0}" +
				"[System.ComponentModel.DesignerCategoryAttribute(\"code\")]{0}" +
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
				"public class ArrayClass {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public object names;{0}" +
#endif
			"}}{0}", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
#if NET_2_0
		[Ignore ("This test does not really consider upgrades.")]
#endif
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
#if NET_2_0
		[Ignore ("This test does not really consider upgrades.")]
#endif
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
#if NET_2_0
		[Ignore ("This test does not really consider upgrades.")]
#endif
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
		[Ignore ("This test does not really consider upgrades.")]
#endif
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
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
				"[System.Diagnostics.DebuggerStepThroughAttribute()]{0}" +
				"[System.ComponentModel.DesignerCategoryAttribute(\"code\")]{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(\"field\", Namespace=\"\", IsNullable=true)]{0}" +
				"public partial class Field {{{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.FlagEnum flags1Field = MonoTests.System.Xml.TestClasses.FlagEnum.e1;{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.FlagEnum flags2Field = MonoTests.System.Xml.TestClasses.FlagEnum.e1;{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.FlagEnum flags3Field = (MonoTests.System.Xml.TestClasses.FlagEnum.e1 | MonoTests.System.Xml.TestClasses.FlagEnum.e2);{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.FlagEnum flags4Field;{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.MapModifiers modifiersField;{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.MapModifiers modifiers2Field;{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.MapModifiers modifiers3Field = MonoTests.System.Xml.TestClasses.MapModifiers.Public;{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.MapModifiers modifiers4Field = MonoTests.System.Xml.TestClasses.MapModifiers.Protected;{0}" +
				"    {0}" +
				"    private MonoTests.System.Xml.TestClasses.MapModifiers modifiers5Field = MonoTests.System.Xml.TestClasses.MapModifiers.Public;{0}" +
				"    {0}" +
				"    private string[] namesField;{0}" +
				"    {0}" +
				"    private string streetField;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"flag1\")]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.FlagEnum.e1)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Flags1 {{{0}" +
				"        get {{{0}" +
				"            return this.flags1Field;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.flags1Field = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"flag2\")]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.FlagEnum.e1)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Flags2 {{{0}" +
				"        get {{{0}" +
				"            return this.flags2Field;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.flags2Field = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"flag3\", Form=System.Xml.Schema.XmlSchemaForm.Qualified)]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute((MonoTests.System.Xml.TestClasses.FlagEnum.e1 | MonoTests.System.Xml.TestClasses.FlagEnum.e2))]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Flags3 {{{0}" +
				"        get {{{0}" +
				"            return this.flags3Field;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.flags3Field = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"flag4\")]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Flags4 {{{0}" +
				"        get {{{0}" +
				"            return this.flags4Field;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.flags4Field = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers\")]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers {{{0}" +
				"        get {{{0}" +
				"            return this.modifiersField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.modifiersField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers2\")]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers2 {{{0}" +
				"        get {{{0}" +
				"            return this.modifiers2Field;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.modifiers2Field = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers3\")]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.MapModifiers.Public)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers3 {{{0}" +
				"        get {{{0}" +
				"            return this.modifiers3Field;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.modifiers3Field = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers4\")]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.MapModifiers.Protected)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers4 {{{0}" +
				"        get {{{0}" +
				"            return this.modifiers4Field;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.modifiers4Field = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers5\", Form=System.Xml.Schema.XmlSchemaForm.Qualified)]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.MapModifiers.Public)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers5 {{{0}" +
				"        get {{{0}" +
				"            return this.modifiers5Field;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.modifiers5Field = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"names\")]{0}" +
				"    public string[] Names {{{0}" +
				"        get {{{0}" +
				"            return this.namesField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.namesField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"street\")]{0}" +
				"    public string Street {{{0}" +
				"        get {{{0}" +
				"            return this.streetField;{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"            this.streetField = value;{0}" +
				"        }}{0}" +
				"    }}{0}" +
#else
				"[System.Xml.Serialization.XmlRootAttribute(\"field\", Namespace=\"\", IsNullable=true)]{0}" +
				"public class Field {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"flag1\")]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.FlagEnum.e1)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Flags1 = MonoTests.System.Xml.TestClasses.FlagEnum.e1;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"flag2\")]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.FlagEnum.e1)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Flags2 = MonoTests.System.Xml.TestClasses.FlagEnum.e1;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"flag3\", Form=System.Xml.Schema.XmlSchemaForm.Qualified)]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute((MonoTests.System.Xml.TestClasses.FlagEnum.e1 | MonoTests.System.Xml.TestClasses.FlagEnum.e2))]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Flags3 = (MonoTests.System.Xml.TestClasses.FlagEnum.e1 | MonoTests.System.Xml.TestClasses.FlagEnum.e2);{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"flag4\")]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Flags4;{0}" +
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
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.MapModifiers.Protected)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers4 = MonoTests.System.Xml.TestClasses.MapModifiers.Protected;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"modifiers5\", Form=System.Xml.Schema.XmlSchemaForm.Qualified)]{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.MapModifiers.Public)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.MapModifiers Modifiers5 = MonoTests.System.Xml.TestClasses.MapModifiers.Public;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"names\")]{0}" +
				"    public string[] Names;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlAttributeAttribute(\"street\")]{0}" +
				"    public string Street;{0}" +
#endif
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"[System.FlagsAttribute()]{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
#endif
				"public enum FlagEnum {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"one\")]{0}" +
				"    e1 = 1,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"two\")]{0}" +
				"    e2 = 2,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"four\")]{0}" +
				"    e4 = 4,{0}" +
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"[System.FlagsAttribute()]{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
#endif
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
#if NET_2_0
		[Ignore ("This test does not really consider upgrades.")]
#endif
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

			codeNamespace = ExportCode (typeof (ItemChoiceType[]));
			Assert.IsNotNull (codeNamespace, "#3");

			sw.GetStringBuilder ().Length = 0;
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
#if NET_2_0
				"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"System.Xml\", \"2.0.50727.42\")]{0}" +
				"[System.SerializableAttribute()]{0}" +
#endif
				"[System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]{0}" +
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
				"}}{0}", Environment.NewLine), sw.ToString (), "#4");
		}

		[Test]
#if NET_2_0
		[Ignore ("This test does not really consider upgrades.")]
#endif
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
#if NET_2_0
		[Category ("NotDotNet")] // regression in MS.NET 2.0
#endif
		[Category ("NotWorking")] // bug #78214
		public void ExportTypeMapping_Root ()
		{
			CodeNamespace codeNamespace = ExportCode (typeof (Root));
			Assert.IsNotNull (codeNamespace, "#1");

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();
			generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}{0}" +
				"/// <remarks/>{0}" +
				"[System.Xml.Serialization.XmlTypeAttribute(Namespace=\"urn:aNS\")]{0}" +
				"[System.Xml.Serialization.XmlRootAttribute(\"root\", Namespace=\"urn:aNS\", IsNullable=false)]{0}" +
				"public class Root {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public MonoTests.System.Xml.TestClasses.OptionalValueTypeContainer OptionalValue;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public MonoTests.System.Xml.TestClasses.TestDefault Default;{0}" +
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"[System.Xml.Serialization.XmlTypeAttribute(TypeName=\"optionalValueType\", Namespace=\"some:urn\")]{0}" +
				"public class OptionalValueTypeContainer {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute((MonoTests.System.Xml.TestClasses.FlagEnum.e1 | MonoTests.System.Xml.TestClasses.FlagEnum.e4))]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Attributes = (MonoTests.System.Xml.TestClasses.FlagEnum.e1 | MonoTests.System.Xml.TestClasses.FlagEnum.e4);{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(MonoTests.System.Xml.TestClasses.FlagEnum.e1)]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum Flags = MonoTests.System.Xml.TestClasses.FlagEnum.e1;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlIgnoreAttribute()]{0}" +
				"    public bool FlagsSpecified;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(false)]{0}" +
				"    public bool IsEmpty = false;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlIgnoreAttribute()]{0}" +
				"    public bool IsEmptySpecified;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(false)]{0}" +
				"    public bool IsNull = false;{0}" +
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"[System.Xml.Serialization.XmlTypeAttribute(Namespace=\"some:urn\")]{0}" +
				"[System.FlagsAttribute()]{0}" +
				"public enum FlagEnum {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"one\")]{0}" +
				"    e1 = 1,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"two\")]{0}" +
				"    e2 = 2,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"four\")]{0}" +
				"    e4 = 4,{0}" +
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"[System.Xml.Serialization.XmlTypeAttribute(Namespace=\"urn:myNS\")]{0}" +
				"public class TestDefault {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    public string str;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(\"Default Value\")]{0}" +
				"    public string strDefault = \"Default Value\";{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(true)]{0}" +
				"    public bool boolT = true;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(false)]{0}" +
				"    public bool boolF = false;{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute(typeof(System.Decimal), \"10\")]{0}" +
				"    public System.Decimal decimalval = ((System.Decimal)(10m));{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute((MonoTests.System.Xml.TestClasses.FlagEnum.e1 | MonoTests.System.Xml.TestClasses.FlagEnum.e4))]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum flag = (MonoTests.System.Xml.TestClasses.FlagEnum.e1 | MonoTests.System.Xml.TestClasses.FlagEnum.e4);{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.ComponentModel.DefaultValueAttribute((MonoTests.System.Xml.TestClasses.FlagEnum_Encoded.e1 | MonoTests.System.Xml.TestClasses.FlagEnum_Encoded.e4))]{0}" +
				"    public MonoTests.System.Xml.TestClasses.FlagEnum_Encoded flagencoded = (MonoTests.System.Xml.TestClasses.FlagEnum_Encoded.e1 | MonoTests.System.Xml.TestClasses.FlagEnum_Encoded.e4);{0}" +
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"[System.Xml.Serialization.XmlTypeAttribute(Namespace=\"urn:myNS\")]{0}" +
				"[System.FlagsAttribute()]{0}" +
				"public enum FlagEnum {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"one\")]{0}" +
				"    e1 = 1,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"two\")]{0}" +
				"    e2 = 2,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    [System.Xml.Serialization.XmlEnumAttribute(\"four\")]{0}" +
				"    e4 = 4,{0}" +
				"}}{0}" +
				"{0}" +
				"/// <remarks/>{0}" +
				"[System.Xml.Serialization.XmlTypeAttribute(Namespace=\"urn:myNS\")]{0}" +
				"[System.FlagsAttribute()]{0}" +
				"public enum FlagEnum_Encoded {{{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    e1 = 1,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    e2 = 2,{0}" +
				"    {0}" +
				"    /// <remarks/>{0}" +
				"    e4 = 4,{0}" +
				"}}{0}", Environment.NewLine), sw.ToString (), "#2");
		}

		[Test]
#if NET_2_0
		[Ignore ("This test does not really consider upgrades.")]
#endif
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
		public void ExportTypeMapping_XsdPrimitive_Arrays ()
		{
			ArrayList types = new ArrayList ();
			types.Add (typeof (sbyte[]));
			types.Add (typeof (bool[]));
			types.Add (typeof (short[]));
			types.Add (typeof (int[]));
			types.Add (typeof (long[]));
			types.Add (typeof (float[]));
			types.Add (typeof (double[]));
			types.Add (typeof (decimal[]));
			types.Add (typeof (ushort[]));
			types.Add (typeof (uint[]));
			types.Add (typeof (ulong[]));
			types.Add (typeof (DateTime[]));
			types.Add (typeof (XmlQualifiedName[]));
			types.Add (typeof (string[]));

			StringWriter sw = new StringWriter ();
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator generator = provider.CreateGenerator ();

			foreach (Type type in types) {
				CodeNamespace codeNamespace = ExportCode (type);
				Assert.IsNotNull (codeNamespace, type.FullName + "#1");

				generator.GenerateCodeFromNamespace (codeNamespace, sw, new CodeGeneratorOptions ());

				Assert.AreEqual (Environment.NewLine, sw.ToString (), 
					type.FullName + "#2");

				sw.GetStringBuilder ().Length = 0;
			}
		}

		[Test]
#if NET_2_0
		[Ignore ("This test does not really consider upgrades.")]
#endif
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

		[XmlRootAttribute ("root", Namespace="urn:aNS", IsNullable=false)]
		public class Root
		{
			public OptionalValueTypeContainer OptionalValue;
			public TestDefault Default;
		}
	}
}
