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

namespace Mono.Util
{

	class CorCompare {
		// these types are in mono corlib, but not in the dll we are going to examine.
		static string[] ghostTypes = {"System.Object", "System.ValueType", "System.Delegate", "System.Enum"};

		public static void Main(string[] args) {
			bool text = false;
			bool namespaceReport = false;
			bool xml = false;

			// make sure we were called with the proper usage
			if (args.Length < 1) {
				Console.WriteLine ("Usage: CorCompare [-t][-n][-x] assembly_to_compare");
				return;
			}
			Assembly monoAsmbl = null;

			for (int i = 0; i < args.Length-1; i++)
			{
				if (args [i] == "-t")
				{
					text = true;
				}
				if (args [i] == "-n")
				{
					namespaceReport = true;
				}
				if (args [i] == "-x")
				{
					xml = true;
				}
			}

			// find which corlib we are running with
			try{
				monoAsmbl = Assembly.LoadFrom(args[args.Length-1]);
			}
			catch(FileNotFoundException)
			{
				Console.WriteLine("Could not find corlib file: {0}", args[args.Length-1]);
				return;
			}

			// get the types in the corlib we are running with
			Assembly msAsmbl = Assembly.GetAssembly(typeof (System.Object));
			Type[] mscorlibTypes = msAsmbl.GetTypes();
			Type[] monocorlibTypes;
			ArrayList TypesList = new ArrayList();

			// load the classes we know should exist, but aren't in the dll
			foreach (string name in ghostTypes){
				TypesList.Add(name);
			}

			// GetTypes() doesn't seem to like loading our dll, so use jujitsu
			try {
				monocorlibTypes = monoAsmbl.GetTypes();
			}
			catch(ReflectionTypeLoadException e) {
				// the exception holds all the types in the dll anyway
				// some are in Types and some are in the LoaderExceptions array
				 monocorlibTypes = e.Types;
				foreach(TypeLoadException loadException in e.LoaderExceptions)
				{
					TypesList.Add(loadException.TypeName);
				}
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
			
			ArrayList MissingTypes = new ArrayList();
			ArrayList PartialClasses = new ArrayList();
			ArrayList MissingMethods = new ArrayList();
			ArrayList namespaces = new ArrayList();
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
									
									if (!namespaces.Contains(t.Namespace))
									{
										namespaces.Add(t.Namespace);
									}
								}

							}
						}
					}
					else
					{
						MissingTypes.Add(t.FullName);
						if (!namespaces.Contains(t.Namespace))
						{
							namespaces.Add(t.Namespace);
						}
					}
				}
			}
			if (namespaceReport)
			{
				foreach (string ns in namespaces)
				{
					int missingInNamespace = GetClassCount(MissingTypes, ns);
					int totalInMSCorlib = GetMSClassCount(mscorlibTypes, ns);
					if (totalInMSCorlib > 0) 
					{
						int complete = 100 * missingInNamespace / totalInMSCorlib ;
						Console.WriteLine("{0} is {1}% complete", ns, complete);
					}
				}
			}
			foreach (string pc in PartialClasses)
			{
				int missingInClass = GetMethodCount(MissingMethods, pc);
			}

			// sort for easy reading
			MissingTypes.Sort();
			if (text){
				foreach (string t in MissingTypes)
				{
					Console.WriteLine (t);
				}

			} 
			if (xml) {
				Console.WriteLine(XMLUtil.ToXML(MissingTypes, "Type", "MissingTypes"));
			}
		}

		static int GetMethodCount(ArrayList methods, string className)
		{
			int count = 0;
			foreach (string method in methods)
			{
				// starts with is not enough. for instance, they all start with "System"
				if (method.StartsWith(className))
				{
					count++;
				}
			}
			return count;
		}
			
		static int GetClassCount(ArrayList types, string ns)
		{
			int count = 0;
			foreach (string type in types)
			{
				// starts with is not enough. for instance, they all start with "System"
				if (type.StartsWith(ns+".") && type.IndexOf(".", ns.Length+1) < 0)
				{
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
