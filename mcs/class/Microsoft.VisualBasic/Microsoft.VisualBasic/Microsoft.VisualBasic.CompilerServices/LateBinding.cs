//
// LateBinding.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Marco Ridoni    (marco.ridoni@virgilio.it)
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Chris J Breisch
// (C) 2003 Marco Ridoni
//
 /*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
  * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */
/**
 *
 */

using System;
using System.Reflection;
using Microsoft.VisualBasic;
using System.ComponentModel;


namespace Microsoft.VisualBasic.CompilerServices {
	[StandardModule, EditorBrowsable(EditorBrowsableState.Never)]
	sealed public class LateBinding {
		private LateBinding () {}

		[System.Diagnostics.DebuggerHiddenAttribute] 
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		public static object LateGet(
			object o,
			Type objType,
			string name,
			object[] args,
			string[] paramnames,
			bool[] CopyBack) {
			if (objType == null) {
				if (o == null)
					throw new NullReferenceException();
				objType = o.GetType();
			}
			Type[] typeArr = null;
			if (args != null) {
				typeArr = new Type[args.Length];
				for (int i = 0; i < typeArr.Length; i++) {
					typeArr[i] = args[i].GetType();
				}
			}

			// get the object's member by the name specified
			MemberInfo[] memberInfo = objType.GetMember(name);

			if (((memberInfo == null) || (memberInfo.Length == 0))) {
				// no member with the name specified found
				throw new NullReferenceException();
			}
			//TODO removed to fix compile warning, add when used.
			//int invokeAttr = 0;

			if (memberInfo[0] is MethodInfo) {
				// the member is a method
				// TODO: 
				//invokeAttr = BindingFlags._InvokeMethod.getValue();
				throw new NotImplementedException("LateBinding not implmented");
			}
			else if (memberInfo[0] is PropertyInfo) {
				// the member is a property
				// TODO: 
				//invokeAttr = BindingFlags._GetProperty.getValue();
				throw new NotImplementedException("LateBinding not implmented");
			}
			else if (memberInfo[0] is FieldInfo) {
				// the member is a field
				// TODO: 
				//invokeAttr = BindingFlags._GetField.getValue();
				throw new NotImplementedException("LateBinding not implmented");
			}
			else {
				throw new NullReferenceException();
			}

			// invoke member - takes care about all three cases : field,property and field.
			// TODO: 
			//throw new NotImplementedException("LateBinding not implmented");
			//return objType.InvokeMember(name, invokeAttr, null, o, args);
		}

		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static void LateSetComplex(
			object o,
			Type objType,
			string name,
			object[] args,
			string[] paramnames,
			bool OptimisticSet,
			bool RValueBase) {
		}

		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static void LateSet(
			object o,
			Type objType,
			string name,
			object[] args,
			string[] paramnames) {
			if (objType == null) {
				if (o == null)
					throw new NullReferenceException();
				objType = o.GetType();
			}
			Type[] typeArr = null;
			if (args != null) {
				typeArr = new Type[args.Length];
				for (int i = 0; i < typeArr.Length; i++) {
					typeArr[i] = args[i].GetType();
				}
			}

			// get the object's member by the name specified
			MemberInfo[] memberInfo = objType.GetMember(name);

			if (((memberInfo == null) || (memberInfo.Length == 0))) {
				// no member with the name specified found
				throw new NullReferenceException();
			}

			// TODO: readd when used
			//int invokeAttr = 0;

			if (memberInfo[0] is PropertyInfo) {
				// the member is a property
				// TODO: 
				throw new NotImplementedException("LateBinding not implmented");
				//invokeAttr = BindingFlags._SetProperty.getValue();
			}
			else if (memberInfo[0] is FieldInfo) {
				// the member is a field
				// TODO: 
				throw new NotImplementedException("LateBinding not implmented");
				//invokeAttr = BindingFlags._SetField.getValue();
			}
			else {
				throw new NullReferenceException();
			}

			// invoke member - takes care about the cases : field and property.
			// TODO: 
			//throw new NotImplementedException("LateBinding not implmented");
			//objType.InvokeMember(name, invokeAttr, null, o, args);
		}
		//mono implmentation
		//		[System.Diagnostics.DebuggerStepThroughAttribute] 
		//		[System.Diagnostics.DebuggerHiddenAttribute] 
		//		public static System.Object LateIndexGet (System.Object o, System.Object[] args, System.String[] paramnames)
		//		{
		//			Type objType;
		//			Object binderState = null;
		//	
		//			if (o == null || args == null)
		//				throw new ArgumentException();
		//	
		//			objType = o.GetType();
		//			if (objType.IsArray) {
		//				Array a = (Array) o;
		//				int[] idxs = new int[args.Length];
		//				Array.Copy (args, idxs, args.Length);
		//	
		//				return a.GetValue(idxs);
		//			}
		//			else
		//			{
		//				MemberInfo[] defaultMembers = objType.GetDefaultMembers();
		//				if (defaultMembers == null)
		//					throw new Exception();  // FIXME: Which exception should we throw?
		//					
		//				// We try to find a default method/property/field we can invoke/use
		//				VBBinder MyBinder = new VBBinder();
		//				BindingFlags bindingFlags = BindingFlags.IgnoreCase |
		//						BindingFlags.Instance |
		//						BindingFlags.Static |
		//						BindingFlags.Public |
		//						BindingFlags.GetProperty |
		//						BindingFlags.GetField |
		//						BindingFlags.InvokeMethod;
		//	
		//				MethodBase[] mb = new MethodBase[defaultMembers.Length];
		//				try {
		//					for (int x = 0; x < defaultMembers.Length; x++)
		//						if (defaultMembers[x].MemberType == MemberTypes.Property)
		//							mb[x] = ((PropertyInfo) defaultMembers[x]).GetGetMethod();
		//						else
		//							mb[x] = (MethodBase) defaultMembers[x];
		//				} catch (Exception e) {	}
		//	
		//				MethodBase TheMethod = MyBinder.BindToMethod (bindingFlags,
		//										mb,
		//										ref args,
		//										null,
		//										null,
		//										paramnames,
		//										out binderState);
		//				if (TheMethod == null)
		//					throw new TargetInvocationException(new ArgumentNullException());
		//				
		//				return TheMethod.Invoke (o, args);		
		//			}
		//		}

		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static object LateIndexGet(
			object o,
			object[] args,
			string[] paramnames) {
			if (o == null)
				throw new NullReferenceException();
			if (args == null)
				throw new NullReferenceException();
			Type type = o.GetType();
			//late binding for array

			if (type.IsArray) {
			// TODO: 
			throw new NotImplementedException("LateBinding not implmented");
				//int rank = ArrayStaticWrapper.get_Rank(o);
				//if (rank != args.Length)
				//	throw new RankException();
				//int[] indices = new int[args.Length];
				//for (int i = 0; i < indices.Length; i++)
				//	indices[i] = IntegerType.FromObject(args[i]);
				//return ArrayStaticWrapper.GetValue(o, indices);
			}
			//late binding for default property
			Type[] types = new Type[args.Length];
			for (int i = 0; i < types.Length; i++) {
				types[i] = args[i].GetType();
			}
			// TODO: 
			//string defaultPropName;
			throw new NotImplementedException("LateBinding not implmented");
			//if (type is TypeInfo)
			//	defaultPropName = getDefaultMemberName(type);
			//else if (type == Type.StringType ||
			//	type == Type.GetType("System.Text.StringBuilder"))
			//	defaultPropName = "Chars";
			//else
			//	defaultPropName = "Item";
			//PropertyInfo propertyInfo = null;
			//if (defaultPropName != null)
			//	propertyInfo = type.GetProperty(defaultPropName, types);
			//if (propertyInfo != null) {
			//	return propertyInfo.GetValue(o, args);
			//}
			//else
			//	throw new NotSupportedException();
		}

		private static string getDefaultMemberName(Type type) {
			string defaultName = null;
			while (type != null) {
				// TODO: 
				throw new NotImplementedException("LateBinding not implmented");
				//object[] locals =
				//	type.GetCustomAttributes(
				//	Type.GetType("System.Reflection.DefaultMemberAttribute"),
				//	false);
				//if (locals != null && locals.Length != 0) {
				//	defaultName =
				//		((DefaultMemberAttribute) locals[0]).get_MemberName();
				//	break;
				//}
				//type = type.get_BaseType();
			}
			return defaultName;
		}
		// mono implmentation
		//		[System.Diagnostics.DebuggerStepThroughAttribute]
		//		[System.Diagnostics.DebuggerHiddenAttribute]
		//		public static void LateIndexSet (System.Object o, System.Object[] args, System.String[] paramnames) 
		//		{
		//			Type objType;
		//			Object binderState = null;
		//			Object myValue;
		//
		//			if (o == null || args == null)
		//				throw new ArgumentException();
		//	
		//			myValue = args[args.Length - 1];
		//			objType = o.GetType();
		//			if (objType.IsArray) {
		//				Array a = (Array) o;
		//				int[] idxs = new int[args.Length - 1];
		//				Array.Copy (args, idxs, args.Length -1);
		//				a.SetValue(myValue, idxs);
		//			}
		//			else
		//			{
		//				MemberInfo[] defaultMembers = objType.GetDefaultMembers();
		//				if (defaultMembers == null)
		//					throw new Exception();  // FIXME: Which exception should we throw?
		//									
		//				// We try to find a default method/property/field we can invoke/use
		//				VBBinder MyBinder = new VBBinder();
		//				BindingFlags bindingFlags = BindingFlags.IgnoreCase |
		//						BindingFlags.Instance |
		//						BindingFlags.Static |
		//						BindingFlags.Public |
		//						BindingFlags.GetProperty |
		//						BindingFlags.GetField |
		//						BindingFlags.InvokeMethod;
		//
		//				MethodBase[] mb = new MethodBase[defaultMembers.Length];
		//				try {
		//					for (int x = 0; x < defaultMembers.Length; x++)
		//						if (defaultMembers[x].MemberType == MemberTypes.Property)
		//							mb[x] = ((PropertyInfo) defaultMembers[x]).GetSetMethod();
		//						else
		//							mb[x] = (MethodBase) defaultMembers[x];
		//				} catch (Exception e) {	}
		//	
		//				MethodBase TheMethod = MyBinder.BindToMethod (bindingFlags,
		//										mb,
		//										ref args,
		//										null,
		//										null,
		//										paramnames,
		//										out binderState);
		//				if (TheMethod == null)
		//					throw new TargetInvocationException(new ArgumentNullException());
		//				
		//				TheMethod.Invoke (o, args);	
		//			}	
		//		}



		[System.Diagnostics.DebuggerHiddenAttribute]
		[System.Diagnostics.DebuggerStepThroughAttribute]
		public static void LateIndexSet(
			object o,
			object[] args,
			string[] paramnames) {
			if (o == null)
				throw new NullReferenceException();
			if (args == null)
				throw new NullReferenceException();
			Type type = o.GetType();
			//late binding for array
			if (type.IsArray) {
				// TODO: 
				throw new NotImplementedException("LateBinding not implmented");
				//int rank = ArrayStaticWrapper.get_Rank(o);
				//if (rank != (args.Length - 1))
				//	throw new RankException();
				//int[] indices = new int[args.Length - 1];
				//for (int i = 0; i < (indices.Length - 1); i++)
				//	indices[i] = IntegerType.FromObject(args[i]);
				//ArrayStaticWrapper.SetValue(o, args[args.Length - 1], indices);
				//return;
			}
			//late binding for default property
			Type[] types = new Type[args.Length - 1];
			for (int i = 0; i < types.Length; i++) {
				// TODO: 
				throw new NotImplementedException("LateBinding not implmented");
				//types[i] = ObjectStaticWrapper.GetType(args[i]);
				//System.out.println("in Set:" + types[i].get_FullName());
			}
			//string defaultPropName;
				// TODO: 
				throw new NotImplementedException("LateBinding not implmented");
			//if (type is TypeInfo)
			//	defaultPropName = getDefaultMemberName(type);
			//else if (type == Type.StringType ||
			//	type == Type.GetType("System.Text.StringBuilder"))
			//	defaultPropName = "Chars";
			//else
			//	defaultPropName = "Item";
			//PropertyInfo propertyInfo = null;
			//if (defaultPropName != null)
			//	propertyInfo = type.GetProperty(defaultPropName, types);
			//if (propertyInfo != null) {
			//	object newVal = args[args.Length - 1];
			//	object[] Params = new object[args.Length - 1];

			//	Array.Copy(args, 0, Params, 0, args.Length - 1);
			//	// java System.arraycopy(args, 0, Params, 0, args.Length - 1);
			//	propertyInfo.SetValue(o, newVal, Params);
			//}
			//else
			//	throw new NotSupportedException();
		}

		[System.Diagnostics.DebuggerHiddenAttribute]
		[System.Diagnostics.DebuggerStepThroughAttribute]
		public static void LateIndexSetComplex(
			object o,
			object[] args,
			string[] paramnames,
			bool OptimisticSet,
			bool RValueBase) {
			LateIndexSet(o, args, paramnames);
		}

		[System.Diagnostics.DebuggerStepThroughAttribute]
		[System.Diagnostics.DebuggerHiddenAttribute]
		public static void LateCall(
			object o,
			Type objType,
			string name,
			object[] args,
			string[] paramnames,
			bool[] CopyBack) {
			if (objType == null) {
				if (o == null)
					throw new NullReferenceException();
				objType = o.GetType();
			}
			Type[] typeArr = null;
			if (args != null) {
				typeArr = new Type[args.Length];
				for (int i = 0; i < typeArr.Length; i++) {
					typeArr[i] = args[i].GetType();
				}
			}
			MethodInfo methodInfo = objType.GetMethod(name, typeArr);
			if (methodInfo == null)
				throw new NullReferenceException();
			methodInfo.Invoke(o, args);
		}
	}
}
