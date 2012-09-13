//
// StronglyTypedResourceBuilder.cs
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//	Gary Barnett (gary.barnett.mono@gmail.com)
// 
// Copyright (C) 2007 Novell, Inc.
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_2_0

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace System.Resources.Tools
{
	public static class StronglyTypedResourceBuilder
	{

		static char [] specialChars = { ' ', '\u00A0', '.', ',', ';', '|', '~', '@', '#', '%', '^', '&', 
						'*', '+', '-', '/', '\\', '<', '>', '?', '[', ']', '(', ')', '{', 
						'}', '\"', '\'', ':', '!'};

		static char [] specialCharsNameSpace = { ' ', '\u00A0', ',', ';', '|', '~', '@', '#', '%', '^', '&', 
						'*', '+', '-', '/', '\\', '<', '>', '?', '[', ']', '(', ')', '{', 
						'}', '\"', '\'', '!'};

		public static CodeCompileUnit Create (string resxFile,
						      string baseName,
						      string generatedCodeNamespace,
						      CodeDomProvider codeProvider, bool internalClass,
						      out string [] unmatchable)
		{
			
			return Create (resxFile,
			               baseName,
			               generatedCodeNamespace,
			               null,
			               codeProvider,
			               internalClass,
			               out unmatchable);
		}

		public static CodeCompileUnit Create (string resxFile,
						      string baseName,
						      string generatedCodeNamespace,
						      string resourcesNamespace,
						      CodeDomProvider codeProvider, bool internalClass,
						      out string [] unmatchable)
		{
			// unpack resfile into dictionary, pass to overload

			// validate resxFile
			if (resxFile == null)
				throw new ArgumentNullException ("Parameter resxFile must not be null");

			List<char> invalidPathChars = new List<char> (Path.GetInvalidPathChars ());
			foreach (char c in resxFile.ToCharArray ()) {
				if (invalidPathChars.Contains (c))
					throw new ArgumentException ("Invalid character in resxFileName");
			}

			Dictionary<string,object> resourcesList = new Dictionary<string,object> ();

			using (ResXResourceReader reader = new ResXResourceReader (resxFile)) {

				foreach (DictionaryEntry d in reader)
					resourcesList.Add ((string) d.Key, d.Value);	
			}

			return Create (resourcesList,
			               baseName,
			               generatedCodeNamespace,
			               resourcesNamespace,
			               codeProvider,
			               internalClass,
			               out unmatchable);

		}

		public static CodeCompileUnit Create (IDictionary resourceList,
						      string baseName,
						      string generatedCodeNamespace,
						      CodeDomProvider codeProvider, bool internalClass,
						      out string [] unmatchable)
		{
			
			return Create (resourceList, 
			               baseName, 
			               generatedCodeNamespace, 
			               null, 
			               codeProvider, 
			               internalClass, 
			               out unmatchable);
		}

		public static CodeCompileUnit Create (IDictionary resourceList, 
						      string baseName,
						      string generatedCodeNamespace,
						      string resourcesNamespace,
						      CodeDomProvider codeProvider, bool internalClass,
						      out string [] unmatchable)
		{
			string baseNameToUse, generatedCodeNamespaceToUse;
			string resourcesToUse;
			
			// validate parameters, convert into useable form where necessary / possible
			if (resourceList == null)
				throw new ArgumentNullException ("Parameter resourceList must not be null");
			
			if (codeProvider == null)
				throw new ArgumentNullException ("Parameter: codeProvider must not be null");
			
			if (baseName == null)
				throw new ArgumentNullException ("Parameter: baseName must not be null");

			baseNameToUse = VerifyResourceName (baseName, codeProvider);
			
			if (baseNameToUse == null)
				throw new ArgumentException ("Parameter: baseName is invalid");
			
			if (generatedCodeNamespace == null) {
				generatedCodeNamespaceToUse = "";
			} else {
				generatedCodeNamespaceToUse = CleanNamespaceChars (generatedCodeNamespace);
				generatedCodeNamespaceToUse = codeProvider.CreateValidIdentifier (
											generatedCodeNamespaceToUse);
			}

			if (resourcesNamespace == null)
				resourcesToUse = generatedCodeNamespaceToUse + "." + baseNameToUse;
			else if (resourcesNamespace == String.Empty)
				resourcesToUse = baseNameToUse;
			else
				resourcesToUse = resourcesNamespace + "." + baseNameToUse;

			// validate ResourceList IDictionary

			Dictionary<string,ResourceItem> resourceItemDict;
			resourceItemDict = new Dictionary<string,ResourceItem> (StringComparer.OrdinalIgnoreCase);

			//allow ArgumentException to be raised on case insensitive dupes,InvalidCastException on key not being string
			foreach (DictionaryEntry de in resourceList)
				resourceItemDict.Add ((string) de.Key, new ResourceItem (de.Value));

			ProcessResourceList (resourceItemDict, codeProvider);

			// Generate CodeDOM
			CodeCompileUnit ccu = GenerateCodeDOMBase (baseNameToUse, generatedCodeNamespaceToUse, 
			                                           resourcesToUse, internalClass);

			// add properties for resources
			unmatchable = ResourcePropertyGeneration (ccu.Namespaces [0].Types [0], 
			                                          resourceItemDict, internalClass);

			return ccu;
		}

		static string[] ResourcePropertyGeneration (CodeTypeDeclaration resType, 
		                                            Dictionary<string, ResourceItem> resourceItemDict, 
		                                            bool internalClass)
		{
			// either create properties for resources, ignore or add to unmatchableList
			List<string> unmatchableList = new List<string> ();

			foreach (KeyValuePair<string, ResourceItem> kvp in resourceItemDict) {
				if (kvp.Value.isUnmatchable)
					unmatchableList.Add (kvp.Key); // orig key
				else if (!kvp.Value.toIgnore) {
						if (kvp.Value.Resource is Stream)
							resType.Members.Add (GenerateStreamResourceProp (kvp.Value.VerifiedKey,
													kvp.Key,
													internalClass));
						else if (kvp.Value.Resource is String)
							resType.Members.Add (GenerateStringResourceProp (kvp.Value.VerifiedKey,
													kvp.Key,
													internalClass));
						else
							resType.Members.Add (GenerateStandardResourceProp (kvp.Value.VerifiedKey,
													kvp.Key,
													kvp.Value.Resource.GetType (),
													internalClass));
				}
			}

			return unmatchableList.ToArray ();
		}

		static CodeCompileUnit GenerateCodeDOMBase (string baseNameToUse, string generatedCodeNamespaceToUse, 
		                                    	    string resourcesToUse, bool internalClass)
		{
			CodeCompileUnit ccu = new CodeCompileUnit ();
			ccu.ReferencedAssemblies.Add ("System.dll");
			CodeNamespace nsMain = new CodeNamespace (generatedCodeNamespaceToUse);
			ccu.Namespaces.Add (nsMain);
			nsMain.Imports.Add (new CodeNamespaceImport ("System"));
			
			//class
			CodeTypeDeclaration resType = GenerateBaseType (baseNameToUse, internalClass);
			nsMain.Types.Add (resType);

			GenerateFields (resType);

			resType.Members.Add (GenerateConstructor ());

			// Default Properties
			resType.Members.Add (GenerateResourceManagerProp (baseNameToUse, resourcesToUse, internalClass));
			resType.Members.Add (GenerateCultureProp (internalClass));

			return ccu;
		}

		static void ProcessResourceList (Dictionary<string, ResourceItem> resourceItemDict, 
		                                 CodeDomProvider codeProvider)
		{
			foreach (KeyValuePair<string, ResourceItem> kvp in resourceItemDict) {
				//deal with ignored keys
				if (kvp.Key.StartsWith (">>") || kvp.Key.StartsWith ("$")) {
					kvp.Value.toIgnore = true;
					continue;
				}
				//deal with specified invalid names (case sensitive)
				if (kvp.Key == "ResourceManager" || kvp.Key == "Culture") {
					kvp.Value.isUnmatchable = true;
					continue;
				}

				kvp.Value.VerifiedKey = VerifyResourceName (kvp.Key, codeProvider);
				// will be null if codeProvider deems invalid
				if (kvp.Value.VerifiedKey == null) {
					kvp.Value.isUnmatchable = true;
					continue;
				}
				//dupe check
				foreach (KeyValuePair<string, ResourceItem> item in resourceItemDict) {
					// skip on encountering kvp or if VerifiedKey on object null (ie hasnt been processed yet)
					if (Object.ReferenceEquals (item.Value, kvp.Value)
					    || item.Value.VerifiedKey == null)
					    continue;
					// if case insensitive dupe found mark both
					if (String.Equals (item.Value.VerifiedKey, kvp.Value.VerifiedKey, 
					                   StringComparison.OrdinalIgnoreCase)) {
						item.Value.isUnmatchable = true;
						kvp.Value.isUnmatchable = true;
					}
				}
			}
		}

		static CodeTypeDeclaration GenerateBaseType (string baseNameToUse, bool internalClass)
		{
			CodeTypeDeclaration resType = new CodeTypeDeclaration (baseNameToUse);
			resType.IsClass = true;
			// set access modifier for class
			if (internalClass)
				resType.TypeAttributes =  TypeAttributes.NotPublic;
			else
				resType.TypeAttributes =  TypeAttributes.Public;
			
			//class CustomAttributes
			resType.CustomAttributes.Add (new CodeAttributeDeclaration (
							"System.CodeDom.Compiler.GeneratedCodeAttribute",
							new CodeAttributeArgument (
							new CodePrimitiveExpression (
							"System.Resources.Tools.StronglyTypedResourceBuilder")),
							new CodeAttributeArgument (
							new CodePrimitiveExpression ("4.0.0.0"))));


			resType.CustomAttributes.Add (new CodeAttributeDeclaration (
							"System.Diagnostics.DebuggerNonUserCodeAttribute"));

			resType.CustomAttributes.Add (new CodeAttributeDeclaration (
							"System.Runtime.CompilerServices.CompilerGeneratedAttribute"));

			return resType;
		}
		static void GenerateFields (CodeTypeDeclaration resType)
		{
			//resourceMan field
			CodeMemberField resourceManField = new CodeMemberField ();
			resourceManField.Attributes = (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.Assembly
							| MemberAttributes.FamilyOrAssembly);
			resourceManField.Name = "resourceMan";
			resourceManField.Type = new CodeTypeReference (typeof (System.Resources.ResourceManager));
			resType.Members.Add (resourceManField);
			
			//resourceCulture field
			CodeMemberField resourceCultureField = new CodeMemberField ();
			resourceCultureField.Attributes = (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.Assembly
							| MemberAttributes.FamilyOrAssembly);
			resourceCultureField.Name = "resourceCulture";
			resourceCultureField.Type = new CodeTypeReference (typeof (System.Globalization.CultureInfo));
			resType.Members.Add (resourceCultureField);
		}

		static CodeConstructor GenerateConstructor ()
		{
			CodeConstructor ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.FamilyAndAssembly; // always internal

			ctor.CustomAttributes.Add (new CodeAttributeDeclaration (
						"System.Diagnostics.CodeAnalysis.SuppressMessageAttribute",
						new CodeAttributeArgument (
						new CodePrimitiveExpression ("Microsoft.Performance")),
						new CodeAttributeArgument (
						new CodePrimitiveExpression ("CA1811:AvoidUncalledPrivateCode"))));

			return ctor;
		}

		static CodeAttributeDeclaration DefaultPropertyAttribute ()
		{
			// CustomAttributes for ResourceManager and Culture 
			return new CodeAttributeDeclaration ("System.ComponentModel.EditorBrowsableAttribute",
	                                                        new CodeAttributeArgument (
								new CodeFieldReferenceExpression (
								new CodeTypeReferenceExpression (
								"System.ComponentModel.EditorBrowsableState"),
								"Advanced")));
		}

		static CodeMemberProperty GenerateCultureProp (bool internalClass)
		{
			// Culture property
			CodeMemberProperty cultureProp = GeneratePropertyBase ("Culture",
			                                               typeof (System.Globalization.CultureInfo),
			                                               internalClass,
			                                               true,
			                                               true);
			
			// attributes - same as ResourceManager
			cultureProp.CustomAttributes.Add (DefaultPropertyAttribute ());

			// getter
			cultureProp.GetStatements.Add (new CodeMethodReturnStatement (
							new CodeFieldReferenceExpression (
							null,"resourceCulture")));

			// setter
			cultureProp.SetStatements.Add (new CodeAssignStatement (
				new CodeFieldReferenceExpression (
				null,"resourceCulture"),
				new CodePropertySetValueReferenceExpression ()));

			return cultureProp;
		}

		static CodeMemberProperty GenerateResourceManagerProp (string baseNameToUse, string resourcesToUse, 
		                                                       bool internalClass)
		{
			// ResourceManager property
			CodeMemberProperty resourceManagerProp = GeneratePropertyBase ("ResourceManager",
			                                                       typeof (System.Resources.ResourceManager),
			                                                       internalClass,
			                                                       true,
			                                                       false);

			resourceManagerProp.CustomAttributes.Add (DefaultPropertyAttribute ());
			// getter
			
			// true statments for check if resourceMan null to go inside getter
			CodeStatement [] trueStatements = new CodeStatement [2];
			
			trueStatements [0] = new CodeVariableDeclarationStatement (
										new CodeTypeReference (
										"System.Resources.ResourceManager"),
				                                                "temp", new CodeObjectCreateExpression (
										new CodeTypeReference (
										"System.Resources.ResourceManager"),
										new CodePrimitiveExpression (resourcesToUse),
										new CodePropertyReferenceExpression (
										new CodeTypeOfExpression (baseNameToUse),
										"Assembly")));

			trueStatements [1] = new CodeAssignStatement (new CodeFieldReferenceExpression (null, "resourceMan"),
			                                              new CodeVariableReferenceExpression ("temp"));
			
			resourceManagerProp.GetStatements.Add (new CodeConditionStatement (
								new CodeMethodInvokeExpression (
								new CodeMethodReferenceExpression (
								new CodeTypeReferenceExpression ("System.Object"), "Equals"),
								new CodePrimitiveExpression(null),
								new CodeFieldReferenceExpression (
								null,"resourceMan")),trueStatements));
			
			resourceManagerProp.GetStatements.Add (new CodeMethodReturnStatement ( 
			                                       new CodeFieldReferenceExpression ( null,"resourceMan")));

			return resourceManagerProp;

		}

		static CodeMemberProperty GenerateStandardResourceProp (string propName, string resName, 
		                                                        Type propertyType, bool isInternal)
		{

			CodeMemberProperty prop = GeneratePropertyBase (propName, propertyType, isInternal, true, false);

			prop.GetStatements.Add (new CodeVariableDeclarationStatement (
						new CodeTypeReference ("System.Object"),
						"obj",
						new CodeMethodInvokeExpression (
						new CodePropertyReferenceExpression (null,"ResourceManager"),
						"GetObject",
						new CodePrimitiveExpression (resName),
						new CodeFieldReferenceExpression (null,"resourceCulture"))));

			prop.GetStatements.Add (new CodeMethodReturnStatement (
						new CodeCastExpression (
						new CodeTypeReference (propertyType),
						new CodeVariableReferenceExpression ("obj"))));

			return prop;
		}

		static CodeMemberProperty GenerateStringResourceProp (string propName, string resName, bool isInternal)
		{
			CodeMemberProperty prop = GeneratePropertyBase (propName, typeof (String), isInternal, true, false);

			prop.GetStatements.Add (new CodeMethodReturnStatement (
						new CodeMethodInvokeExpression (
						new CodeMethodReferenceExpression (
						new CodePropertyReferenceExpression (null,"ResourceManager"),
						"GetString"),
						new CodePrimitiveExpression (resName),
						new CodeFieldReferenceExpression (null,"resourceCulture"))));
						
			return prop;
		}

		static CodeMemberProperty GenerateStreamResourceProp (string propName, string resName, bool isInternal)
		{
			CodeMemberProperty prop = GeneratePropertyBase (propName, typeof (UnmanagedMemoryStream), 
			                                                isInternal, true, false);

			prop.GetStatements.Add (new CodeMethodReturnStatement (
						new CodeMethodInvokeExpression (
						new CodeMethodReferenceExpression (
						new CodePropertyReferenceExpression (null,"ResourceManager"),
						"GetStream"),
						new CodePrimitiveExpression (resName),
						new CodeFieldReferenceExpression (null,"resourceCulture"))));

			return prop;
		}

		static CodeMemberProperty GeneratePropertyBase (string name, Type propertyType, bool isInternal, 
		                                                bool hasGet, bool hasSet)
		{
			CodeMemberProperty prop = new CodeMemberProperty ();

			prop.Name = name;
			prop.Type = new CodeTypeReference (propertyType);
			
			// accessor
			if (isInternal)
				prop.Attributes = (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.Assembly);
			else
				prop.Attributes = (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.FamilyAndAssembly
							| MemberAttributes.FamilyOrAssembly);
			
			prop.HasGet = hasGet;
			prop.HasSet = hasSet;
			return prop;
		}

		public static string VerifyResourceName (string key, CodeDomProvider provider)
		{
			string keyToUse;
			char [] charKey;

			// check params
			if (key == null)
				throw new ArgumentNullException ("Parameter: key must not be null");
			if (provider == null)
				throw new ArgumentNullException ("Parameter: provider must not be null");

			if (key == String.Empty) {
				keyToUse = "_";
			} else {
				// replaces special chars
				charKey = key.ToCharArray ();
				for (int i = 0; i < charKey.Length; i++)
					charKey [i] = VerifySpecialChar (charKey [i]);
				keyToUse = new string(charKey);
			}
			// resolve if keyword
			keyToUse = provider.CreateValidIdentifier (keyToUse);
			// check if still not valid for provider
			if (provider.IsValidIdentifier (keyToUse))
				return keyToUse;
			else
				return null;
		}

		static char VerifySpecialChar (char ch)
		{
			for (int i = 0; i < specialChars.Length; i++) {
				if (specialChars [i] == ch)
					return '_';
			}
			return ch;
		}

		static string CleanNamespaceChars (string name)
		{
			char [] nameChars = name.ToCharArray ();
			for (int i = 0; i < nameChars.Length ;i++) {
				foreach (char c in specialCharsNameSpace) {
					if (nameChars [i] == c)
						nameChars [i] = '_';
				}
			}
			return new string (nameChars);
		}

		class ResourceItem {
			public string VerifiedKey { get;set; }
			public object Resource { get;set; }
			public bool isUnmatchable { get;set; }
			public bool toIgnore { get;set; }

			public ResourceItem (object value)
			{
				Resource = value;
			}
		}

	}
}

#endif
