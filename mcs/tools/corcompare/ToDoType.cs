// Mono.Util.CorCompare.ToDoType
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;
using System.Collections;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class that is marked with MonoTODO
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class ToDoType : MissingType 
	{
		// e.g. <class name="System.Array" status="todo" missing="5" todo="6" complete="45">
		
		ArrayList missingMethodList = new ArrayList();
		public ArrayList MissingMethods {
			get {
				return missingMethodList;
			}
		}

		ArrayList todoMethodList = new ArrayList();
		public ToDoMethod[] ToDoMethods {
			get {
				Array retVal=Array.CreateInstance( typeof(ToDoMethod), todoMethodList.Count );
				todoMethodList.CopyTo(retVal);
				return (ToDoMethod[])retVal;
			}
		}

		ArrayList missingPropertyList = new ArrayList();
		ArrayList todoPropertyList = new ArrayList();

		int complete;

		public ToDoType(Type t) : base(t) {
		}

		public int MissingCount {
			get {
				return missingMethodList.Count + missingPropertyList.Count;
			}
		}

		public int ToDoCount {
			get {
				return todoMethodList.Count + todoPropertyList.Count;
			}
		}
		
		public int Complete {
			get {
				return complete;
			}
		}
		
		public static int IndexOf(Type t, ArrayList todoTypes) {
			for(int index = 0; index < todoTypes.Count; index++) {
				if (((ToDoType)todoTypes[index]).Name == t.Name) {
					return index;
				}
			}
			return -1;
		}

		public void CompareWith(Type referenceType) {
			//TODO: Next discover the missing methods, properties, etc.
			GetMissingMethods(referenceType.GetMethods());
			complete = 0;
		}

		void GetMissingMethods(MethodInfo[] referenceMethods) {
			ArrayList referenceMethodList = new ArrayList(referenceMethods);
			ArrayList MethodList = new ArrayList(theType.GetMethods());
			foreach (MethodInfo method in referenceMethods) {
				if (!MethodList.Contains(method)) {
					missingMethodList.Add(method);
				}
			}
		}

		public override string Status {
			get {
				return "todo";
			}
		}

		public void AddToDoMember(Type t, MemberInfo info){
			switch (info.MemberType){
				case MemberTypes.Method:
					todoMethodList.Add(new ToDoMethod(info));
					break;
				case MemberTypes.Property:
					todoPropertyList.Add(new ToDoProperty(info));
					break;
				default:
					break;
					//throw new Exception("Didn't code that member type yet");
			}
		}

		public void AddMissingMember(MemberInfo info){
			switch (info.MemberType){
				case MemberTypes.Method:
					missingMethodList.Add(new MissingMethod(info));
					break;
				case MemberTypes.Property:
					missingPropertyList.Add(new MissingProperty(info));
					break;
				default:
					break;
					//throw new Exception("Didn't code that member type yet");
			}
		}
	}
}
