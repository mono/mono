//
// CSharpNodeFormatter.cs: Formats nodes with C# syntax
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector.Formatters
{
	public class CSharpNodeFormatter : LanguageNodeFormatter {

		public CSharpNodeFormatter ()
		{
		}

		protected override string LineComment       {get {return "//";}}

		protected override string KeywordClass      {get {return "class";}}
		protected override string KeywordEnum       {get {return "enum";}}
		protected override string KeywordValueType  {get {return "struct";}}
		protected override string KeywordInterface  {get {return "interface";}}
		protected override string KeywordInherits   {get {return ":";}}
		protected override string KeywordImplements {get {return ",";}}
		protected override string KeywordMulticast  {get {return "event";}}
		protected override string KeywordStatementTerminator {get {return ";";}}
		protected override string KeywordStatementSeparator  {get {return ",";}}

		protected override string QualifierPublic   {get {return "public";}}
		protected override string QualifierFamily   {get {return "protected";}}
		protected override string QualifierAssembly {get {return "internal";}}
		protected override string QualifierPrivate  {get {return "private";}}
		protected override string QualifierFinal    {get {return "sealed";}}
		protected override string QualifierStatic   {get {return "static";}}
		protected override string QualifierLiteral  {get {return "const";}}
		protected override string QualifierAbstract {get {return "abstract";}}
		protected override string QualifierVirtual  {get {return "virtual";}}

		private static readonly string[] attributeDelimeters = new string[]{"[", "]"};

		protected override string[] AttributeDelimeters {
			get {return attributeDelimeters;}
		}

		protected override string GetConstructorName (ConstructorInfo ctor)
		{
			return ctor.DeclaringType.Name;
		}
	}
}

