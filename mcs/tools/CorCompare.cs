// CorCompare
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;

namespace Mono.Util {

	class CorCompare {
		public static void Main(string[] args) {
			// make sure we were called with the proper usage
			if (args.Length < 1) 	{
				Console.WriteLine ("Usage: CorCompare [-t][-n][-x outfile] assembly_to_compare");
				return;
			}

			ToDoAssembly td = new ToDoAssembly(args[args.Length-1], "corlib");

			for (int i = 0; i < args.Length-1; i++) {
				if (args [i] == "-t") {
					Console.WriteLine(td.CreateClassListReport());
				}
				if (args [i] == "-n") {
				}
				if (args [i] == "-x") {
                    td.CreateXMLReport(args[++i]);
				}
			}
		}
	}

	class ToDoNameSpace {
		// e.g. <namespace name="System" missing="267" todo="453" complete="21">
		MissingType[] missingTypes;
		ToDoType[] todoTypes;
		public string name;
		int complete = 0;
		Type[] existingTypes;
		int referenceTypeCount;

		public static ArrayList GetNamespaces(Type[] types) {
			ArrayList nsList = new ArrayList();
			foreach (Type t in types) {
				if (!nsList.Contains(t.Namespace)) {
					nsList.Add(t.Namespace);
				}
			}
			return nsList;
		}

		public ToDoNameSpace(string nameSpace, Type[] types) {
			name = nameSpace;
			existingTypes = Filter(types, name);
		}

		public ToDoNameSpace(string nameSpace, Type[] types, Type[] referenceTypes) {
			name = nameSpace;
			existingTypes = Filter(types, name);
			CompareWith(referenceTypes);
		}

		public int MissingCount {
			get {
				return missingTypes.Length;
			}
		}

		public int ToDoCount {
			get {
				return todoTypes.Length;
			}
		}

		public int ReferenceTypeCount {
			get {
				return referenceTypeCount;
			}
		}

		Type[] Filter(Type[] types, string ns) {
			ArrayList filteredTypes = new ArrayList();
			foreach(Type t in types) {
				if (t.Namespace == ns) {
					filteredTypes.Add(t);
				}
			}
			return (Type[])filteredTypes.ToArray(typeof(Type));
		}

		public int Complete {
			get {
				return complete;
			}
		}

		public void CompareWith(Type[] referenceTypes) {
			Type[] filteredReferenceTypes = Filter(referenceTypes, name);
			referenceTypeCount = 0;
			if (null != existingTypes) {
				referenceTypeCount = filteredReferenceTypes.Length;
				missingTypes = GetMissingTypes(filteredReferenceTypes);
				todoTypes = GetToDoTypes(filteredReferenceTypes);
				if (null != filteredReferenceTypes && filteredReferenceTypes.Length > 0) {
					int needLoveCount = 0;
					if (null != missingTypes) {
						needLoveCount += missingTypes.Length;
					}
					if (null != todoTypes) {
						needLoveCount += todoTypes.Length;
					}
					complete = 100 * needLoveCount / filteredReferenceTypes.Length;
				}
			}
		}

		MissingType[] GetMissingTypes(Type[] referenceTypes) {
			ArrayList TypesList = new ArrayList();
			ArrayList MissingTypes = new ArrayList();
			bool foundIt;
			foreach(Type subt in existingTypes) {
				if (null != subt && !TypesList.Contains(subt.Name)) {
					TypesList.Add(subt.Name);
				}
			} 
			TypesList.Sort();
			foreach(Type t in referenceTypes) {
				foundIt = (TypesList.BinarySearch(t.Name) >= 0);
				if (t.IsPublic && !foundIt) {
					MissingTypes.Add(new MissingType(t));
				}
			}
			return (MissingType[])MissingTypes.ToArray(typeof(MissingType));
		}

		ToDoType[] GetToDoTypes(Type[] referenceTypes) {
			// todo types are those marked with [MonoTODO] or having missing or todo members
			ArrayList TypesList = new ArrayList();
			ArrayList ToDoTypes = new ArrayList();
			ArrayList ReferenceTypesList = new ArrayList(referenceTypes);
			bool foundIt = false;
			Object[] myAttributes;
			foreach(Type t in existingTypes) {
				foundIt = false;
				myAttributes = t.GetCustomAttributes(false);
				foreach (object o in myAttributes) {
					if (o.ToString() == "System.MonoTODOAttribute"){
						ToDoTypes.Add(new ToDoType(t));
						break;
					}
				}
				foreach (MemberInfo mi in t.GetMembers()) {
					myAttributes = mi.GetCustomAttributes(false);
					foreach (object o in myAttributes) {
						if (o.ToString() == "System.MonoTODOAttribute") {
							if (!foundIt) {
								ToDoTypes.Add(new ToDoType(t));
								foundIt = true;
							}
							((ToDoType)(ToDoTypes[ToDoTypes.Count-1])).AddToDoMember(mi);
						}
					}
				}
			}
			return (ToDoType[])ToDoTypes.ToArray(typeof(ToDoType));
		}

		public MissingType[] MissingTypes {
			get {
				return missingTypes;
			}
		}

		public string[] MissingTypeNames(bool qualify){
			ArrayList names = new ArrayList();
			if (qualify) {
				foreach (MissingType t in missingTypes) {
					names.Add(t.NameSpace + "." + t.Name);
				}
			}
			else {
				foreach (MissingType t in missingTypes) {
					names.Add(t.Name);
				}
			}
			return (string[])names.ToArray(typeof(string));
		}
	}

	class ToDoProperty : MissingProperty {
		// e.g. <property name="Count" status="todo" note="another note"/>
		string todoNote = "";

		public ToDoProperty(PropertyInfo pInfo) : base(pInfo) {
		}
		public ToDoProperty(PropertyInfo pInfo, string note) :base(pInfo) {
			todoNote = note;
		}
		public string Note {
			get {
				return todoNote;
			}
		}
		public override string Status {
			get {
				return "todo";
			}
		}
	}

	class MissingProperty {
		// e.g. <property name="Length" status="missing"/>
		PropertyInfo info;

		public MissingProperty(PropertyInfo pInfo) {
			info = pInfo;
		}

		public string Name {
			get {
				return info.Name;
			}
		}
		public virtual string Status {
			get {
				return "missing";
			}
		}
	}

	class ToDoMethod : MissingMethod {
		// e.g. <method name="ToString" status="todo" note="this is the note from MonoTODO"/>
		string todoNote = "";

		public ToDoMethod(MethodInfo info) : base(info) {
		}
		public ToDoMethod(MethodInfo info, string note) :base(info) {
			todoNote = note;
		}
		public string Note {
			get {
				return todoNote;
			}
		}
		public override string Status {
			get {
				return "todo";
			}
		}
	}

	class MissingMethod {
		// e.g. <method name="Equals" status="missing"/>
		MethodInfo mInfo;

		public MissingMethod(MethodInfo info) {
			mInfo = info;
		}

		public string Name {
			get {
				return mInfo.Name;
			}
		}
		public virtual string Status {
			get {
				return "missing";
			}
		}
	}

	class ToDoType : MissingType {
		// e.g. <class name="System.Array" status="todo" missing="5" todo="6" complete="45">
		
		ArrayList missingMethodList = new ArrayList();
		public MissingMethod[] MissingMethods {
			get {
				return (MissingMethod[])missingMethodList.ToArray(typeof(MissingMethod));
			}
		}

		ArrayList todoMethodList = new ArrayList();
		public ToDoMethod[] ToDoMethods {
			get {
				return (ToDoMethod[])todoMethodList.ToArray(typeof(ToDoMethod));
			}
		}

		public MissingProperty[] MissingProperties;
		public ToDoProperty[] ToDoProperties;
		int complete;

		public ToDoType(Type t) : base(t) {
		}

		public int MissingCount {
			get {
				return MissingMethods.Length + MissingProperties.Length;
			}
		}

		public int ToDoCount {
			get {
				return ToDoMethods.Length + ToDoProperties.Length;
			}
		}
		public int Complete {
			get {
				return complete;
			}
		}
		public void CompareWith(Type referenceType) {
			//TODO: Next discover the missing methods, properties, etc.
		}
		public override string Status {
			get {
				return "todo";
			}
		}

		public void AddToDoMember(MemberInfo info){
			if (info.MemberType == MemberTypes.Method) {
				todoMethodList.Add(info);
			}
		}
	}

	class MissingType {
		// e.g. <class name="System.Byte" status="missing"/>
		Type theType;
		public MissingType(Type t) {
			theType = t;
		}

		public string Name {
			get {
				return theType.Name;
			}
		}

		public string NameSpace {
			get {
				return theType.Namespace;
			}
		}

		public virtual string Status {
			get {
				return "missing";
			}
		}
	}

	class ToDoAssembly
	{
		// these types are in mono corlib, but not in the dll we are going to examine.
		static string[] ghostTypes = {"System.Object", "System.ValueType", "System.Delegate", "System.Enum"};
		ArrayList MissingTypes = new ArrayList();
		string assemblyToCompare;
		bool analyzed = false;
		ArrayList todoNameSpaces = new ArrayList();
		string name;

		public ToDoAssembly(string fileName, string friendlyName)
		{
			assemblyToCompare = fileName;
			name = friendlyName;
		}

		public string Name {
			get {
				return name;
			}
		}

		public int MissingCount {
			get {
				int sum = 0;
				foreach(ToDoNameSpace ns in todoNameSpaces) {
					sum += ns.MissingCount;
				}
				return sum;
			}
		}

		public int ReferenceTypeCount {
			get {
				int sum = 0;
				foreach(ToDoNameSpace ns in todoNameSpaces) {
					sum += ns.ReferenceTypeCount;
				}
				return sum;
			}
		}

		bool Analyze()
		{
			if (analyzed) return true;

			Type[] mscorlibTypes = GetReferenceTypes();
			if (mscorlibTypes == null)
			{
				Console.WriteLine("Could not find corlib file: {0}", assemblyToCompare);
				return false;
			}

			Type[] monocorlibTypes = GetMonoTypes();

			foreach(string ns in ToDoNameSpace.GetNamespaces(monocorlibTypes)) {
				todoNameSpaces.Add(new ToDoNameSpace(ns, monocorlibTypes, mscorlibTypes));
			}

			analyzed = true;
			return true;
/* ///////////////////////////////////////////////////////////////////////////////// stuff below here needs to be moved into classes above.

			ArrayList TypesList = new ArrayList();

			// load the classes we know should exist, but aren't in the dll
			foreach (string name in ghostTypes){
				TypesList.Add(name);
			}

			Hashtable monoMethodMap = new Hashtable();
			MethodInfo[] methods;
			ArrayList nameList = new ArrayList();
			// whether GetTypes() worked or not, we will have _some_ types here
			foreach(Type subt in monocorlibTypes)
			{
				if (null != subt && !TypesList.Contains(subt.FullName)) {
					TypesList.Add(subt.FullName);

					methods = subt.GetMethods();

					nameList = GetMungedNames(methods);

					monoMethodMap.Add(subt.FullName, nameList);
				}
			}

			// going to use BinarySearch, so sort first
			TypesList.Sort();
			
			ArrayList PartialClasses = new ArrayList();
			ArrayList MissingMethods = new ArrayList();
			bool foundit = false;

			// make list of ms types not in mono
			foreach(Type t in mscorlibTypes) {
				if (t.IsPublic) {
					foundit = (TypesList.BinarySearch(t.FullName) >= 0);
					if (foundit) 
					{
						// look for missing members
						nameList = GetMungedNames(t.GetMethods());
						foreach (string name in nameList)
						{
							ArrayList monoNames = (ArrayList)monoMethodMap[t.FullName];
							if (monoNames != null)
							{
								monoNames.Sort();
								if (monoNames.BinarySearch(name) < 0)
								{
									MissingMethods.Add(t.FullName + "::" + name);

									if (!PartialClasses.Contains(t.FullName))
									{
										PartialClasses.Add(t.FullName);
									}
									
								}

							}
						}
					}
					else
					{
						MissingTypes.Add(t.FullName);
					}
				}
			}
			foreach (string pc in PartialClasses)
			{
				int missingInClass = GetMethodCount(MissingMethods, pc);
			}
*/
		}

		public Type[] GetReferenceTypes()
		{
			// get the types in the corlib we are running with
			Assembly msAsmbl = Assembly.GetAssembly(typeof (System.Object));
			Type[] mscorlibTypes = msAsmbl.GetTypes();
			return mscorlibTypes;
		}

		public Type[] GetMonoTypes()
		{
			Type[] monocorlibTypes;
			Assembly monoAsmbl = null;
			try
			{
				monoAsmbl = Assembly.LoadFrom(assemblyToCompare);
			}
			catch(FileNotFoundException)
			{
				return null;
			}

			monocorlibTypes = monoAsmbl.GetTypes();

			return monocorlibTypes;
		}

		public string CreateClassListReport() {
			if (!Analyze() || todoNameSpaces.Count == 0) return "";

			StringBuilder output = new StringBuilder();
			foreach (ToDoNameSpace ns in todoNameSpaces)
			{
				string[] missingTypes = ns.MissingTypeNames(true);
				if (missingTypes.Length > 0) {
					string joinedNames = String.Join("\n", missingTypes);
					output.Append(joinedNames + "\n");
				}
			}
			return output.ToString();
		}

		public void CreateXMLReport(string filename) {
			bool analyzedOK = Analyze();

			XmlDocument outDoc;
			outDoc = new XmlDocument();
			outDoc.AppendChild(outDoc.CreateXmlDeclaration("1.0", null, null));
			XmlElement assembliesElem = outDoc.CreateElement("assemblies");
			outDoc.AppendChild(assembliesElem);
			XmlElement assemblyElem = outDoc.CreateElement("assembly");
			assemblyElem.SetAttribute("name", this.Name);
			assemblyElem.SetAttribute("missing", this.MissingCount.ToString());
			assemblyElem.SetAttribute("complete", (100 - 100 * this.MissingCount / this.ReferenceTypeCount).ToString());
			assembliesElem.AppendChild(assemblyElem);
			XmlElement namespacesElem = outDoc.CreateElement("namespaces");
			assemblyElem.AppendChild(namespacesElem);

			if (analyzedOK && todoNameSpaces.Count > 0) {
				XmlElement namespaceElem;
				XmlElement classesElem;
				XmlElement classElem;
				foreach (ToDoNameSpace ns in todoNameSpaces) {
					namespaceElem = outDoc.CreateElement("namespace");
					namespaceElem.SetAttribute("name", ns.name);
					MissingType[] missingTypes = ns.MissingTypes;
					if (missingTypes.Length > 0) {
						classesElem = outDoc.CreateElement("classes");
						namespaceElem.AppendChild(classesElem);
						foreach (MissingType type in missingTypes) {
							classElem = outDoc.CreateElement("class");
							classElem.SetAttribute("name", type.Name);
							classElem.SetAttribute("status", type.Status);
							classesElem.AppendChild(classElem);
						}
						namespaceElem.SetAttribute("missing", ns.MissingCount.ToString());
						namespaceElem.SetAttribute("complete", (100 - 100 * ns.MissingCount / ns.ReferenceTypeCount).ToString());
						namespacesElem.AppendChild(namespaceElem);
					}
				}
			}
			
			outDoc.Save(filename);
		}

		static int GetMethodCount(ArrayList methods, string className) {
			int count = 0;
			foreach (string method in methods) {
				// starts with is not enough. for instance, they all start with "System"
				if (method.StartsWith(className)) {
					count++;
				}
			}
			return count;
		}
			
		static int GetClassCount(ArrayList types, string ns) {
			int count = 0;
			foreach (string type in types) {
				// starts with is not enough. for instance, they all start with "System"
				if (type.StartsWith(ns+".") && type.IndexOf(".", ns.Length+1) < 0) {
					count++;
				}
			}
			return count;
		}

		static int GetMSClassCount(Type[] types, string ns)
		{
			int count = 0;
			for (int i=0; i < types.Length; i++)
			{
				if (types[i].Namespace == ns)
				{
					count++;
				}
			}
			return count;
		}

		static ArrayList GetMungedNames(MethodInfo[] methods)
		{
			ArrayList nameList = new ArrayList();
			foreach(MethodInfo method in methods)
			{
				StringBuilder methodMungedName = new StringBuilder(method.Name + "(");
				ParameterInfo[] parameters = method.GetParameters();
				ArrayList parameterTypes = new ArrayList();
				foreach(ParameterInfo parameter in parameters)
				{
					parameterTypes.Insert(parameter.Position, parameter.ParameterType.Name);
				}
						
				foreach(string parameterTypeName in parameterTypes)
				{
					if (!methodMungedName.ToString().EndsWith("("))
						methodMungedName.Append(",");
					methodMungedName.Append(parameterTypeName);
				}

				methodMungedName.Append(")");
				nameList.Add(methodMungedName.ToString());
			}

			return nameList;
		}
	}
}
