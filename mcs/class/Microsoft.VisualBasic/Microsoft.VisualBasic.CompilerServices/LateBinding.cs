//
// LateBinding.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Marco Ridoni    (marco.ridoni@virgilio.it)
//   Dennis Hayes (dennish@raytek.com)
//   Satya Sudha K (ksathyasudha@novell.com)
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
		public static object LateGet(object o,
					     Type objType,
					     string name,
					     object[] args,
					     string[] paramnames,
					     bool[] CopyBack) {

			if (objType == null) {
				if (o == null) {
					throw new NullReferenceException();
				}
				objType = o.GetType();
			}

			IReflect objReflect = (IReflect) objType;

			BindingFlags flags = BindingFlags.FlattenHierarchy |
					     BindingFlags.IgnoreCase |
					     BindingFlags.Instance |
					     BindingFlags.Public |
					     BindingFlags.Static |
					     BindingFlags.GetProperty |
					     BindingFlags.InvokeMethod;

			if (name == null) {
				name = "";
			}
			MemberInfo [] memberinfo = objReflect.GetMember (name, flags);

			if (memberinfo == null || memberinfo.Length == 0) {
				throw new MissingMemberException ("Public Member '" + name + "' not found on type '" + objType + "'");
			}

			MemberInfo mi = GetMostDerivedMemberInfo (memberinfo);
			if (mi.MemberType == MemberTypes.Field) {
				FieldInfo fi = (FieldInfo) mi;
				object ret = fi.GetValue (o);
				if (args != null && args.Length > 0) 
					return LateIndexGet (ret, args, paramnames);
				return ret;
			}

			VBBinder binder = new VBBinder (CopyBack);
			return binder.InvokeMember (name, flags, objType, objReflect, o, args, null, null, paramnames);
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
			bool RValueBase) 
		{
			LateSet(o, objType, name, args, paramnames);
		}

		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static void LateSet (object o,
					    Type objType,
					    string name,
					    object[] args,
					    string[] paramnames) {

			if (objType == null) {
				if (o == null)
					throw new NullReferenceException();
				objType = o.GetType();
			}

			IReflect objReflect = (IReflect) objType;

			BindingFlags flags = BindingFlags.FlattenHierarchy |
					     BindingFlags.IgnoreCase |
					     BindingFlags.Instance |
					     BindingFlags.Public |
					     BindingFlags.Static |
					     BindingFlags.SetProperty |
					     BindingFlags.InvokeMethod;

			if (name == null) {
				name = "";
			}

			MemberInfo [] memberinfo = objReflect.GetMember (name, flags);

			if (memberinfo == null || memberinfo.Length == 0) {
				throw new MissingMemberException ("Public Member '" + name + "' not found on type '" + objType + "'");
			}

			MemberInfo mi = GetMostDerivedMemberInfo (memberinfo);
			if (mi.MemberType == MemberTypes.Field) {
				FieldInfo fi = (FieldInfo) mi;
				if (args == null || args.Length == 0) {
					throw new MissingMemberException ("Public Member '" + name + "' not found on type '" + objType + "' that can be assigned the given set of arguments");
				}

				if (fi.IsInitOnly || fi.IsPrivate) {
					throw new MissingMemberException ("Member '" + name + "' is a readonly field");
				}

				object value = null;
				if (args.Length == 1)
					value = args [0];

				fi.SetValue (o, value);
				return;
			}

			VBBinder binder = new VBBinder (null);
			binder.InvokeMember (name, flags, objType, objReflect, o, args, null, null, paramnames);
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
				Array objAsArray = (Array) o;
				if (objAsArray.Rank != args.Length)
					throw new RankException ();

				int numArgs = args.Length;
				int [] indexArray = new int [numArgs];
				for (int index = 0; index < numArgs; index ++) 
					indexArray [index] = IntegerType.FromObject (args [index]);
				return objAsArray.GetValue (indexArray);
			}

			//late binding for default property
			VBBinder binder = new VBBinder (null);
			BindingFlags flags = BindingFlags.FlattenHierarchy |
					     BindingFlags.IgnoreCase |
					     BindingFlags.Instance |
					     BindingFlags.Public |
					     BindingFlags.Static |
					     BindingFlags.GetProperty |
					     BindingFlags.InvokeMethod;
			IReflect objReflect = (IReflect) type;	
			return binder.InvokeMember ("", flags, type, objReflect, o, args, null, null, paramnames);
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
			if (args == null || args.Length == 0)
				throw new NullReferenceException();
			Type type = o.GetType();
			//late binding for array
			if (type.IsArray) {
				Array array = (Array) o;
				if (array.Rank != args.Length - 1) 
					throw new RankException ();
				object setValue = args [args.GetUpperBound (0)];
				int [] indexArray = new int [args.GetUpperBound (0)];
				for (int index = 0; index < indexArray.Length; index ++) {
					indexArray [index] = IntegerType.FromObject (args [index]);
				}
				array.SetValue (setValue, indexArray);
				return;
			}
			//late binding for default property
			VBBinder binder = new VBBinder (null);
			BindingFlags flags = BindingFlags.FlattenHierarchy |
					     BindingFlags.IgnoreCase |
					     BindingFlags.Instance |
					     BindingFlags.Public |
					     BindingFlags.Static |
					     BindingFlags.SetProperty |
					     BindingFlags.InvokeMethod;
			IReflect objReflect = (IReflect) type;	
			binder.InvokeMember ("", flags, type, objReflect, o, args, null, null, paramnames);
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

				InternalLateCall (o, objType, name, args, paramnames, CopyBack, true);
		}

		[System.Diagnostics.DebuggerStepThroughAttribute]
		[System.Diagnostics.DebuggerHiddenAttribute]
		internal static object InternalLateCall( object o,
							 Type objType,
							 string name,
							 object[] args,
							 string[] paramnames,
							 bool[] CopyBack, 
							 bool IgnoreReturn) {
			if (objType == null) {
				if (o == null) {
					throw new NullReferenceException();
				}
				objType = o.GetType();
			}

			IReflect objReflect = (IReflect) objType;

			BindingFlags flags = BindingFlags.FlattenHierarchy |
					     BindingFlags.IgnoreCase |
					     BindingFlags.Instance |
					     BindingFlags.Public |
					     BindingFlags.Static |
					     BindingFlags.InvokeMethod;

			if (name == null) {
				name = "";
			}
			MemberInfo [] memberinfo = objReflect.GetMember (name, flags);

			if (memberinfo == null || memberinfo.Length == 0) {
				throw new MissingMemberException ("Public Member '" + name + "' not found on type '" + objType + "'");
			}

			if (args != null) {
				foreach (MemberInfo mi in memberinfo) {
					if (mi.MemberType == MemberTypes.Field) 
						throw new ArgumentException ("Expression '" + name + "' is not a procedure, but occurs as a target of a procedure call");
				}
			}

			VBBinder binder = new VBBinder (CopyBack);
			return binder.InvokeMember (name, flags, objType, objReflect, o, args, null, null, paramnames);
		}

		private static MemberInfo GetMostDerivedMemberInfo (MemberInfo [] mi) 
		{
			if (mi == null || mi.Length == 0)
				return null;
			MemberInfo m = mi [0];
			for (int index = 1; index < mi.Length; index ++) {
				MemberInfo m1 = mi [index];
				if (m1.DeclaringType.IsSubclassOf (m.DeclaringType))
					m = m1;
			}
			return m;
		}		
	}
}
