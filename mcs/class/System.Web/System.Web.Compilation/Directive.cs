//
// System.Web.Compilation.Directive
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;

namespace System.Web.Compilation
{
	sealed class Directive
	{
		static Hashtable directivesHash;
		static string [] page_atts = {  "AspCompat", "AutoEventWireup", "Buffer",
						"ClassName", "ClientTarget", "CodePage",
						"CompilerOptions", "ContentType", "Culture", "Debug",
						"Description", "EnableSessionState", "EnableViewState",
						"EnableViewStateMac", "ErrorPage", "Explicit",
						"Inherits", "Language", "LCID", "ResponseEncoding",
						"Src", "SmartNavigation", "Strict", "Trace",
						"TraceMode", "Transaction", "UICulture",
						"WarningLevel", "CodeBehind" };

		static string [] control_atts = { "AutoEventWireup", "ClassName", "CompilerOptions",
						  "Debug", "Description", "EnableViewState",
						  "Explicit", "Inherits", "Language", "Strict", "Src",
						  "WarningLevel", "CodeBehind", "TargetSchema" };

		static string [] import_atts = { "namespace" };
		static string [] implements_atts = { "interface" };
		static string [] assembly_atts = { "name", "src" };
		static string [] register_atts = { "tagprefix", "tagname", "Namespace", "Src", "Assembly" };

		static string [] outputcache_atts = { "Duration", "Location", "VaryByControl", 
						      "VaryByCustom", "VaryByHeader", "VaryByParam" };

		static string [] reference_atts = { "page", "control" };

		static string [] webservice_atts = { "class", "codebehind", "debug", "language" };

		static string [] application_atts = { "description", "inherits", "codebehind" };

		static Directive ()
		{
			InitHash ();
		}
		
		private static void InitHash ()
		{
			CaseInsensitiveHashCodeProvider provider = new CaseInsensitiveHashCodeProvider ();
			CaseInsensitiveComparer comparer =  new CaseInsensitiveComparer ();

			directivesHash = new Hashtable (provider, comparer); 

			// Use Hashtable 'cause is O(1) in Contains (ArrayList is O(n))
			Hashtable valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in page_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("PAGE", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in control_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("CONTROL", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in import_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("IMPORT", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in implements_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("IMPLEMENTS", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in register_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("REGISTER", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in assembly_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("ASSEMBLY", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in outputcache_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("OUTPUTCACHE", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in reference_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("REFERENCE", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in webservice_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("WEBSERVICE", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			// same attributes as webservice
			foreach (string att in webservice_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("WEBHANDLER", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in application_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("APPLICATION", valid_attributes);
		}
		
		private Directive () { }

		public static bool IsDirective (string id)
		{
			return directivesHash.Contains (id);
		}
	}
}

