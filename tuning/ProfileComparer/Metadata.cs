//
// Metadata.cs
//
// (C) 2007 - 2008 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections.Generic;

namespace GuiCompare {

	public enum CompType {
		Assembly,
		Namespace,
		Attribute,
		Interface,
		Class,
		Struct,
		Enum,
		Method,
		Property,
		Field,
		Delegate,
		Event,
		GenericParameter
	}

	public interface ICompAttributeContainer
	{
		List<CompNamed> GetAttributes ();
	}

	public interface ICompHasBaseType
	{
		string GetBaseType ();

		bool IsSealed { get; }
		bool IsAbstract { get; }
	}
	
	public interface ICompTypeContainer
	{
		List<CompNamed> GetNestedClasses();
		List<CompNamed> GetNestedInterfaces ();
		List<CompNamed> GetNestedStructs ();
		List<CompNamed> GetNestedEnums ();
		List<CompNamed> GetNestedDelegates ();
	}

	public interface ICompMemberContainer
	{
		List<CompNamed> GetInterfaces ();
		List<CompNamed> GetConstructors();
		List<CompNamed> GetMethods();
 		List<CompNamed> GetProperties();
 		List<CompNamed> GetFields();
 		List<CompNamed> GetEvents();
	}
	
	public interface ICompGenericParameter
	{
		List<CompGenericParameter> GetTypeParameters ();
	}

	[Flags]
	public enum UnityProfiles {
		None = 0,
		Unity = 1,
		UnityWeb = 2,
		Micro = 4,
		All = 7,
		Last = 8
	}
	
	public static class UnityProfilesUtils {
		public static string DirectoryNameFromProfile (this UnityProfiles profile) {
			switch (profile) {
			case UnityProfiles.None : return "2.0";
			case UnityProfiles.Unity : return "unity";
			case UnityProfiles.UnityWeb : return "unity_web";
			case UnityProfiles.Micro : return "micro";
			default : throw new ArgumentException (String.Format ("Invalid profle {0}", profile));
			}
		}
		
		public static IEnumerable<UnityProfiles> ListProfiles () {
			for (int bit = 0; (1 << bit) < ((int) UnityProfiles.Last); bit ++) {
				yield return (UnityProfiles) (1 << bit);
			}
			yield break;
		}
		
		public static bool Contains (this UnityProfiles profiles, UnityProfiles profile) {
			return (profiles & profile) != 0;
		}
		public static void AddTo (this UnityProfiles profile, ref UnityProfiles profiles) {
			profiles |= profile;
		}
	}
	
	public abstract class DocumentedItem {
		string name;
		public string Name {
			get {
				return name;
			}
		}
		
		public virtual string FullName {
			get {
				return Name;
			}
		}
		
		UnityProfiles supportedProfiles;
		public UnityProfiles SupportedProfiles {
			get {
				return supportedProfiles;
			}
		}
		public void AddSupportedProfile (UnityProfiles profile) {
			profile.AddTo (ref supportedProfiles);
		}
		
		UnityProfiles securityCriticalProfiles;
		public UnityProfiles SecurityCriticalProfiles {
			get {
				return securityCriticalProfiles;
			}
		}
		public void AddSecurityCriticalProfile (UnityProfiles profile) {
			profile.AddTo (ref securityCriticalProfiles);
		}
		
		public DocumentedItem (string name) {
			this.name = name;
		}
	}
	
	public abstract class DocumentedMember : DocumentedItem {
		DocumentedClass parent;
		public DocumentedClass Parent {
			get {
				return parent;
			}
		}
		
		public void DebugDump () {
			Console.WriteLine ("\t\t{0} supported in profile {1}", Name, SupportedProfiles);
		}
		
		public DocumentedMember (string name, DocumentedClass parent) : base (name) {
			this.parent = parent;
		}
	}
	
	public class DocumentedField : DocumentedMember {
		public DocumentedField (string name, DocumentedClass parent) : base (name, parent) {
		}
	}
	public class DocumentedMethod : DocumentedMember {
		public DocumentedMethod (string name, DocumentedClass parent) : base (name, parent) {
		}
	}
	public class DocumentedProperty : DocumentedMember {
		public DocumentedProperty (string name, DocumentedClass parent) : base (name, parent) {
		}
	}
	
	public class DocumentedClass : DocumentedItem {
		Dictionary<string,DocumentedMember> members;
		public DocumentedMember[] Members {
			get {
				DocumentedMember[] result = new DocumentedMember [members.Count];
				members.Values.CopyTo (result, 0);
				return result;
			}
		}
		
		public DocumentedMember FindMember (string name) {
			if (members.ContainsKey (name)) {
				return members [name];
			} else {
				return null;
			}
		}
		
		public DocumentedField AddField (string name) {
			if (members.ContainsKey (name)) {
				return null;
			} else {
				DocumentedField result = new DocumentedField (name, this);
				members [name] = result;
				return result;
			}
		}
		public DocumentedField AddReferenceField (string name) {
			if (members.ContainsKey (name)) {
				return (DocumentedField) members [name];
			} else {
				return AddField (name);
			}
		}

		public DocumentedMethod AddMethod (string name) {
			if (members.ContainsKey (name)) {
				return null;
			} else {
				DocumentedMethod result = new DocumentedMethod (name, this);
				members [name] = result;
				return result;
			}
		}
		public DocumentedMethod AddReferenceMethod (string name) {
			if (members.ContainsKey (name)) {
				return (DocumentedMethod) members [name];
			} else {
				return AddMethod (name);
			}
		}
		
		public DocumentedProperty AddProperty (string name) {
			if (members.ContainsKey (name)) {
				return null;
			} else {
				DocumentedProperty result = new DocumentedProperty (name, this);
				members [name] = result;
				return result;
			}
		}
		public DocumentedProperty AddReferenceProperty (string name) {
			if (members.ContainsKey (name)) {
				return (DocumentedProperty) members [name];
			} else {
				return AddProperty (name);
			}
		}
		
		public void DebugDump () {
			Console.WriteLine ("\tType {0} supported in profile {1}", Name, SupportedProfiles);
			foreach (DocumentedMember member in Members) {
				member.DebugDump ();
			}
		}
		
		public DocumentedClass (string name) : base (name) {
			members = new Dictionary<string, DocumentedMember> ();
		}
	}

	public class DocumentedNamespace {
		string name;
		public string Name {
			get {
				return name;
			}
		}
		
		Dictionary<string,DocumentedClass> classes;
		public DocumentedClass[] Classes {
			get {
				DocumentedClass[] result = new DocumentedClass [classes.Count];
				classes.Values.CopyTo (result, 0);
				return result;
			}
		}
		
		public DocumentedClass FindClass (string name) {
			if (classes.ContainsKey (name)) {
				return classes [name];
			} else {
				return null;
			}
		}
		
		public DocumentedClass AddClass (string name) {
			if (classes.ContainsKey (name)) {
				return null;
			} else {
				DocumentedClass result = new DocumentedClass (name);
				classes [name] = result;
				return result;
			}
		}
		public DocumentedClass AddReferenceClass (string name) {
			if (classes.ContainsKey (name)) {
				return (DocumentedClass) classes [name];
			} else {
				return AddClass (name);
			}
		}
		
		public void DebugDump () {
			Console.WriteLine ("Namespace {0}", Name);
			foreach (DocumentedClass c in Classes) {
				c.DebugDump ();
			}
		}
		
		public DocumentedNamespace (string name) {
			this.name = name;
			classes = new Dictionary<string, DocumentedClass> ();
		}
	}
	
	public class UnityProfileError {
		string description;
		public string Description {
			get {
				return description;
			}
		}
		
		public UnityProfileError (string description) {
			this.description = description;
		}
	}
	
	public class UnityProfilesDocumentation {
		Dictionary<string,DocumentedNamespace> namespaces;
		
		public DocumentedNamespace[] Namespaces {
			get {
				DocumentedNamespace[] result = new DocumentedNamespace [namespaces.Count];
				namespaces.Values.CopyTo (result, 0);
				return result;
			}
		}
		
		public DocumentedNamespace FindNamespace (string name) {
			if (namespaces.ContainsKey (name)) {
				currentNamespace = namespaces [name];
				return currentNamespace;
			} else {
				return null;
			}
		}
		
		public DocumentedNamespace AddNamespace (string name) {
			if (namespaces.ContainsKey (name)) {
				return null;
			} else {
				DocumentedNamespace result = new DocumentedNamespace (name);
				namespaces [name] = result;
				currentNamespace = result;
				return result;
			}
		}
		public DocumentedNamespace AddReferenceNamespace (string name) {
			if (namespaces.ContainsKey (name)) {
				return (DocumentedNamespace) namespaces [name];
			} else {
				return AddNamespace (name);
			}
		}
		
		List<UnityProfileError> errors;
		public bool HasErrors {
			get {
				return errors.Count != 0;
			}
		}
		public UnityProfileError[] Errors {
			get {
				UnityProfileError[] result = new UnityProfileError [errors.Count];
				errors.CopyTo (result, 0);
				return result;
			}
		}
		public void AddError (string description) {
			errors.Add (new UnityProfileError (description));
		}
		
		DocumentedNamespace currentNamespace;
		public DocumentedNamespace CurrentNamespace {
			get {
				return currentNamespace;
			}
			set {
				currentNamespace = value;
			}
		}
		DocumentedClass currentClass;
		public DocumentedClass CurrentClass {
			get {
				return currentClass;
			}
			set {
				currentClass = value;
			}
		}
		DocumentedMember currentMember;
		public DocumentedMember CurrentMember {
			get {
				return currentMember;
			}
			set {
				currentMember = value;
			}
		}
		
		public struct State {
			DocumentedNamespace currentNamespace;
			public DocumentedNamespace CurrentNamespace {
				get {
					return currentNamespace;
				}
			}
			DocumentedClass currentClass;
			public DocumentedClass CurrentClass {
				get {
					return currentClass;
				}
			}
			DocumentedMember currentMember;
			public DocumentedMember CurrentMember {
				get {
					return currentMember;
				}
			}
			
			public State (UnityProfilesDocumentation currentState) {
				this.currentNamespace = currentState.CurrentNamespace;
				this.currentClass = currentState.CurrentClass;
				this.currentMember = currentState.CurrentMember;
			}
		}
		
		public State CurrentState {
			get {
				return new State (this);
			}
			set {
				this.currentNamespace = value.CurrentNamespace;
				this.currentClass = value.CurrentClass;
				this.currentMember = value.CurrentMember;
			}
		}
		
		public DocumentedClass FindClass (string name) {
			if (currentNamespace == null) {
				return null;
			} else {
				DocumentedClass result = currentNamespace.FindClass (name);
				currentClass = result;
				return result;
			}
		}
		public DocumentedClass AddClass (string name) {
			if (currentNamespace == null) {
				return null;
			} else {
				DocumentedClass result = currentNamespace.AddClass (name);
				currentClass = result;
				return result;
			}
		}		
		public DocumentedClass AddReferenceClass (string name) {
			if (currentNamespace == null) {
				return null;
			} else {
				DocumentedClass result = currentNamespace.AddReferenceClass (name);
				currentClass = result;
				return result;
			}
		}		
		
		public DocumentedMember FindMember (string name) {
			if (currentClass == null) {
				return null;
			} else {
				DocumentedMember result = currentClass.FindMember (name);
				currentMember = result;
				return result;
			}
		}
		public DocumentedField AddReferenceField (string name) {
			if (currentClass == null) {
				return null;
			} else {
				DocumentedField result = currentClass.AddReferenceField (name);
				currentMember = result;
				return result;
			}
		}		
		public DocumentedField AddField (string name) {
			if (currentClass == null) {
				return null;
			} else {
				DocumentedField result = currentClass.AddField (name);
				currentMember = result;
				return result;
			}
		}		
		public DocumentedMethod AddMethod (string name) {
			if (currentClass == null) {
				return null;
			} else {
				DocumentedMethod result = currentClass.AddMethod (name);
				currentMember = result;
				return result;
			}
		}
		public DocumentedMethod AddReferenceMethod (string name) {
			if (currentClass == null) {
				return null;
			} else {
				DocumentedMethod result = currentClass.AddReferenceMethod (name);
				currentMember = result;
				return result;
			}
		}
		public DocumentedProperty AddProperty (string name) {
			if (currentClass == null) {
				return null;
			} else {
				DocumentedProperty result = currentClass.AddProperty (name);
				currentMember = result;
				return result;
			}
		}
		public DocumentedProperty AddReferenceProperty (string name) {
			if (currentClass == null) {
				return null;
			} else {
				DocumentedProperty result = currentClass.AddReferenceProperty (name);
				currentMember = result;
				return result;
			}
		}
		
		public UnityProfilesDocumentation () {
			namespaces = new Dictionary<string, DocumentedNamespace> ();
			errors = new List<UnityProfileError> ();
		}
		
		public void DebugDump () {
			if (HasErrors) {
				Console.WriteLine ("Errors!");
				foreach (UnityProfileError e in Errors) {
					Console.WriteLine (e.Description);
				}
			} else {
				Console.WriteLine ("Rejoice: no errors!");
			}
			foreach (DocumentedNamespace ns in Namespaces) {
				ns.DebugDump ();
			}
		}
	}
	
	

	public abstract class CompNamed {
		public CompNamed (string name, CompType type)
		{
			this.DisplayName = null;
			this.name = name;
			this.type = type;
			this.todos = new List<string>();
		}

		public string MemberName {
			set { memberName = value; }
			get { return memberName; }
		}
		
		public string Name {
			set { name = value; }
			get { return name; }
		}

		public string DisplayName {
			set { displayName = value; }
			get { return displayName == null ? name : displayName; }
		}

		public string ExtraInfo {
			set { extraInfo = value; }
			get { return extraInfo; }
		}

		public CompType Type {
			set { type = value; }
			get { return type; }
		}

		public ComparisonNode GetComparisonNode ()
		{
			ComparisonNode node = new ComparisonNode (type, DisplayName, MemberName, ExtraInfo);
			node.Todos.AddRange (todos);
			return node;
		}

		public static int Compare (CompNamed x, CompNamed y)
		{
			return String.Compare (x.Name, y.Name);
		}

		string displayName;
		string name;
		string memberName;
		string extraInfo;
		CompType type;
		public List<string> todos;

	}

	public abstract class CompAssembly : CompNamed, ICompAttributeContainer {
		public CompAssembly (string name)
			: base (name, CompType.Assembly)
		{
		}

		public abstract List<CompNamed> GetNamespaces ();
		public abstract List<CompNamed> GetAttributes ();
	}

	public abstract class CompNamespace : CompNamed, ICompTypeContainer {
		public CompNamespace (string name)
			: base (name, CompType.Namespace)
		{
		}

		// ICompTypeContainer implementation
		public abstract List<CompNamed> GetNestedClasses();
		public abstract List<CompNamed> GetNestedInterfaces ();
		public abstract List<CompNamed> GetNestedStructs ();
		public abstract List<CompNamed> GetNestedEnums ();
		public abstract List<CompNamed> GetNestedDelegates ();
	}

	public abstract class CompInterface : CompNamed, ICompAttributeContainer, ICompMemberContainer, ICompHasBaseType, ICompGenericParameter {
		public CompInterface (string name)
			: base (name, CompType.Interface)
		{
		}

		public abstract List<CompNamed> GetAttributes ();

		public abstract List<CompNamed> GetInterfaces ();
		public abstract List<CompNamed> GetConstructors();
		public abstract List<CompNamed> GetMethods();
 		public abstract List<CompNamed> GetProperties();
 		public abstract List<CompNamed> GetFields();
 		public abstract List<CompNamed> GetEvents();
		
		public abstract string GetBaseType();
		
		public bool IsSealed { get { return false; } }
		public bool IsAbstract { get { return false; } }
		
		public abstract List<CompGenericParameter> GetTypeParameters ();
	}

	public abstract class CompEnum : CompNamed, ICompAttributeContainer, ICompMemberContainer, ICompHasBaseType {
		public CompEnum (string name)
			: base (name, CompType.Enum)
		{
		}

		public List<CompNamed> GetInterfaces () { return new List<CompNamed>(); }
		public List<CompNamed> GetConstructors() { return new List<CompNamed>(); }
		public List<CompNamed> GetMethods() { return new List<CompNamed>(); }
 		public List<CompNamed> GetProperties() { return new List<CompNamed>(); }
 		public List<CompNamed> GetEvents() { return new List<CompNamed>(); }

 		public abstract List<CompNamed> GetFields();

		public abstract List<CompNamed> GetAttributes ();
		
		public abstract string GetBaseType();
		
		public bool IsSealed { get { return true; } }
		public bool IsAbstract { get { return false; } }
	}

	public abstract class CompDelegate : CompNamed, ICompAttributeContainer, ICompHasBaseType, ICompGenericParameter {
		public CompDelegate (string name)
			: base (name, CompType.Delegate)
		{
		}
		
		public abstract List<CompNamed> GetAttributes ();

		public abstract string GetBaseType();
		
		public bool IsSealed { get { return true; } }
		public bool IsAbstract { get { return false; } }		
		
		public abstract List<CompGenericParameter> GetTypeParameters ();
	}

	public abstract class CompClass : CompNamed, ICompAttributeContainer, ICompTypeContainer, ICompMemberContainer, ICompHasBaseType, ICompGenericParameter {
		public CompClass (string name, CompType type)
			: base (name, type)
		{
		}

		public abstract List<CompNamed> GetInterfaces();
		public abstract List<CompNamed> GetConstructors();
		public abstract List<CompNamed> GetMethods();
 		public abstract List<CompNamed> GetProperties();
 		public abstract List<CompNamed> GetFields();
 		public abstract List<CompNamed> GetEvents();

		public abstract List<CompNamed> GetAttributes ();

		public abstract List<CompNamed> GetNestedClasses();
		public abstract List<CompNamed> GetNestedInterfaces ();
		public abstract List<CompNamed> GetNestedStructs ();
		public abstract List<CompNamed> GetNestedEnums ();
		public abstract List<CompNamed> GetNestedDelegates ();
		
		public abstract string GetBaseType();
		public abstract bool IsSealed { get; }
		public abstract bool IsAbstract { get; }
		
		public abstract List<CompGenericParameter> GetTypeParameters ();
	}

	public abstract class CompMember : CompNamed, ICompAttributeContainer {
		public CompMember (string name, CompType type)
			: base (name, type)
		{
		}

		public abstract string GetMemberAccess();
		public abstract string GetMemberType();
		
		public abstract List<CompNamed> GetAttributes ();
	}

	public abstract class CompMethod : CompMember, ICompGenericParameter {
		public CompMethod (string name)
			: base (name, CompType.Method)
		{
		}

		public abstract bool ThrowsNotImplementedException ();
		
		public abstract List<CompGenericParameter> GetTypeParameters ();
	}

	public abstract class CompProperty : CompMember, ICompMemberContainer {
		public CompProperty (string name)
			: base (name, CompType.Property)
		{
		}
		
		public abstract List<CompNamed> GetMethods();
		public List<CompNamed> GetInterfaces() { return new List<CompNamed>(); }
		public List<CompNamed> GetConstructors() { return new List<CompNamed>(); }
		public List<CompNamed> GetEvents() { return new List<CompNamed>(); }
		public List<CompNamed> GetFields() { return new List<CompNamed>(); }
		public List<CompNamed> GetProperties() { return new List<CompNamed>(); }
	}

	public abstract class CompField : CompMember {
		public CompField (string name)
			: base (name, CompType.Field)
		{
		}
		public abstract string GetLiteralValue ();
	}

	public abstract class CompEvent : CompMember {
		public CompEvent (string name)
			: base (name, CompType.Event)
		{
		}		
	}
	
	public abstract class CompAttribute : CompNamed {
		public CompAttribute (string typename)
			: base (typename, CompType.Attribute)
		{
		}
	}
	
	public abstract class CompGenericParameter : CompNamed, ICompAttributeContainer {
		
		public readonly Mono.Cecil.GenericParameterAttributes GenericAttributes;
		
		public CompGenericParameter (string name, Mono.Cecil.GenericParameterAttributes attr)
			: base (name, CompType.GenericParameter)
		{
			GenericAttributes = attr;
		}
		
		public abstract List<CompNamed> GetAttributes ();
		
		public static string GetGenericAttributeDesc (Mono.Cecil.GenericParameterAttributes ga)
		{
			return ga.ToString ();
		}
	}
}
