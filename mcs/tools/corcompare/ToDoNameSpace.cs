// Mono.Util.CorCompare.ToDoNameSpace
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Collections;
using System.Reflection;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a namespace that has missing and/or MonoTODO classes.
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class ToDoNameSpace 
	{
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

		public ToDoNameSpace(string nameSpace, Type[] types, 
		                     Type[] referenceTypes) {
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
				if (null != filteredReferenceTypes && 
				    	filteredReferenceTypes.Length > 0) {
					int needLoveCount = 0;
					if (null != missingTypes) {
						needLoveCount += missingTypes.Length;
					}
					if (null != todoTypes) {
						needLoveCount += todoTypes.Length;
					}
					complete = 100 * needLoveCount / 
								filteredReferenceTypes.Length;
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
			// todo types are those marked with [MonoTODO] or having missing or
			// todo members
			ArrayList TypesList = new ArrayList();
			ArrayList ToDoTypes = new ArrayList();

			bool foundIt = false;
			Object[] myAttributes;

			int index;

			// look at all the existing types in this namespace for MonoTODO attrib
			foreach(Type t in existingTypes) {
				if (t.IsPublic) {
					// assume we won't find it
					foundIt = false;

					// get all the custom attributes on the type
					myAttributes = t.GetCustomAttributes(false);
					foreach (object o in myAttributes) {
						// check to see if any of them are the MonoTODO attrib
						if (o.ToString() == "System.MonoTODOAttribute"){
							// if so, this is a todo type 
							ToDoTypes.Add(new ToDoType(t));
							// and we can stop look at the custom attribs
							break;
						}
					}

					// look at all the members of the type
					foreach (MemberInfo mi in t.GetMembers()) {
						// see if any of them have the MonoTODO attrib
						myAttributes = mi.GetCustomAttributes(false);
						foreach (object o in myAttributes) {
							if (o.ToString() == "System.MonoTODOAttribute") {
								// the first time we find one for this type add the type to the list
								if (!foundIt) {
									index = ToDoType.IndexOf(t, ToDoTypes);
									if (index < 0) {
										ToDoTypes.Add(new ToDoType(t));
									}
									foundIt = true;
								}
								// add any todo member infos to the todo type
								((ToDoType)(ToDoTypes[ToDoTypes.Count-1])).AddToDoMember(t, mi);
							}
						}
					}
				}
			}
			// find types with missing members
			foreach (Type t in referenceTypes) {
				if (t.IsPublic && !IsMissingType(t)) {
					bool addedIt = false;
					index = -1;

					foreach (MemberInfo mi in t.GetMembers()) {
						if (t.Name == mi.DeclaringType.Name && IsMissingMember(mi, t.Name, existingTypes)) {
							if (!addedIt) {
								index = ToDoType.IndexOf(t, ToDoTypes);
								if (index >= 0) {
								}
								else {
									index = ToDoTypes.Add(new ToDoType(t));
								}
								addedIt = true;
							}
							if (index >= 0) {
								((ToDoType)(ToDoTypes[index])).AddMissingMember(mi);
							}
							else {
								throw new Exception("Don't know which ToDoType to add this missing member");
							}
						}
					}
				}
			}

			return (ToDoType[])ToDoTypes.ToArray(typeof(ToDoType));
		}

		bool IsMissingType(Type t) {
			foreach (MissingType mt in missingTypes) {
				if (t.Name == mt.Name) {
					return true;
				}
			}
			return false;
		}

		static bool IsMissingMember(MemberInfo mi, string typeName, Type[] typesToSearch) {
			foreach (Type t in typesToSearch) {
				if (t.Name == typeName) {
					foreach (MemberInfo trialMI in t.GetMembers()) {
						if (mi.Name == trialMI.Name && mi.MemberType == trialMI.MemberType) {
							if (mi.MemberType == MemberTypes.Method) {
								if (IsParameterListEqual(((MethodInfo)mi).GetParameters(), ((MethodInfo)trialMI).GetParameters())) {
									return false;
								}
							}
							else {
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		static bool IsParameterListEqual(ParameterInfo[] piArray1, ParameterInfo[] piArray2) {
			if (piArray1.Length != piArray2.Length) {
				return false;
			}

			foreach (ParameterInfo pi1 in piArray1) {
				if (pi1.ParameterType.Name != GetTypeAt(pi1.Position, piArray2).Name) {
					return false;
				}
			}

			return true;
		}

		static Type GetTypeAt(int position, ParameterInfo[] piArray){
			foreach (ParameterInfo pi in piArray) {
				if (pi.Position == position) {
						return pi.ParameterType;
				}
			}
			throw new Exception("contract violation: need to call with existing position");
		}

		public MissingType[] MissingTypes {
			get {
				return missingTypes;
			}
		}

		public ToDoType[] ToDoTypes {
			get {
				return todoTypes;
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
}
