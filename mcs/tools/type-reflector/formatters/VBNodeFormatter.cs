//
// VBNodeFormatter.cs: Formats nodes with VB.NET syntax
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
	public class VBNodeFormatter : LanguageNodeFormatter {

		public VBNodeFormatter ()
		{
		}

		protected override string LineComment       {get {return "'";}}

		protected override string PropertyFormat    {get {return "Property {1} As {0}\n{2}{3}End Property";}}
		protected override string PropertyGetFormat {get {return "\tGet\n\t\t' Return {0}\n\tEnd Get\n";}}
		protected override string PropertySet       {get {return "\tSet\n\tEnd Set\n";}}

		protected override string KeywordClass      {get {return "Class";}}
		protected override string KeywordEnum       {get {return "Enum";}}
		protected override string KeywordValueType  {get {return "Struct";}}
		protected override string KeywordInterface  {get {return "Interface";}}
		protected override string KeywordInherits   {get {return "Inherits";}}
		protected override string KeywordImplements {get {return "Implements";}}
		protected override string KeywordMulticast  {get {return "Event";}}
		protected override string KeywordStatementTerminator {get {return "";}}
		protected override string KeywordStatementSeparator  {get {return ",";}}

		protected override string QualifierPublic   {get {return "Public";}}
		protected override string QualifierFamily   {get {return "Family";}}
		protected override string QualifierAssembly {get {return "Internal";}}
		protected override string QualifierPrivate  {get {return "Private";}}
		protected override string QualifierFinal    {get {return "Final";}}
		protected override string QualifierStatic   {get {return "Shared";}}
		protected override string QualifierLiteral  {get {return "Const";}}
		protected override string QualifierAbstract {get {return "Abstract";}}
		protected override string QualifierVirtual  {get {return "Overridable";}}

		private static readonly string[] attributeDelimeters = new string[]{"<", ">"};

		protected override string[] AttributeDelimeters {
			get {return attributeDelimeters;}
		}

		protected override string GetConstructorName (ConstructorInfo ctor)
		{
      return "Sub New";
		}

		protected override void AddMethodDeclaration (StringBuilder sb, MethodInfo method)
		{
			string type = "Function";

			bool sub = method.ReturnType == typeof (void);

			if (sub) {
				type = "Sub";
			}

			sb.AppendFormat ("{0} {1}", type, method.Name);
			AddMethodArgs (sb, method);

			if (!sub)
				sb.AppendFormat (" As {0}", method.ReturnType);
		}
	}
}

