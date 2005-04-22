//
// LateBinding.cs
//
// Author:
//   Marco Ridoni    (marco.ridoni@virgilio.it)
//   Satya Sudha K   (ksathyasudha@novell.com)
//
// (C) 2003 Marco Ridoni
//

//
// Copyright (c) 2002-2003 Mainsoft Corporation.
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Reflection;
using System.Globalization;
using Microsoft.VisualBasic;
using System.ComponentModel;


namespace Microsoft.VisualBasic.CompilerServices
{
	[StandardModule, EditorBrowsableAttribute(EditorBrowsableState.Never)]
	public class VBBinder : Binder
	{
		public VBBinder() : base()
		{
		}

		public VBBinder(bool [] CopyBack) : base()
		{
			byRefFlags = CopyBack;
		}

		private class BinderState
		{
			public object[] args;
			public bool[] byRefFlags;
		}


		public enum ConversionType {
			Exact = 0,
			Widening,
			Narrowing,
			None = -1
		}

		public override FieldInfo BindToField(
			BindingFlags bindingAttr,
			FieldInfo[] match,
			object value,
			CultureInfo culture
			)
		{
			return null;
		}

		private bool[] byRefFlags;
		private Type objectType;
		private string bindToName;

		public override MethodBase BindToMethod(
			BindingFlags bindingAttr,
			MethodBase[] match,
			ref object[] args,
			ParameterModifier[] modifiers,
			CultureInfo culture,
			string[] names,
			out object state
			)
		{
			// Store the arguments to the method in a state object.
			BinderState binderState = new BinderState();
			object[] arguments = new Object[args.Length];
			args.CopyTo(arguments, 0);
			binderState.args = arguments;
			state = binderState;

			MethodBase mbase = null;
			Type applicable_type = null;

			if(match == null || match.Length == 0)
				throw new ArgumentNullException();

			/*
				// Handle delegates
				if (match [0].Name == "Invoke") {
					// TODO
				}
			*/

			ArrayList candidates = new ArrayList ();
			for (int index = 0; index < match.Length; index ++) {
				if (IsApplicable (match [index], args))
					candidates.Add (match [index]);
			}

			MemberInfo[] tempMatchList = GetMostDerivedMembers (candidates);
			MethodBase[] filteredMatchList = new MethodBase [tempMatchList.Length];
			for (int index = 0; index < tempMatchList.Length; index ++)
				filteredMatchList [index] = (MethodBase) tempMatchList [index];

				
			ConversionType bestMatch = ConversionType.None;
			int numWideningConversions = 0, numNarrowingConversions = 0;
			ArrayList narrowingConv = new ArrayList ();
			ArrayList wideningConv = new ArrayList ();
			for(int x = 0; x < filteredMatchList.Length; x++)
			{
				ParameterInfo[] parameters = filteredMatchList [x].GetParameters();
				ConversionType ctype = GetConversionType (parameters, args);
				if (ctype == ConversionType.None)
					continue;
				if (ctype == ConversionType.Widening)
					wideningConv.Add (filteredMatchList[x]);
				if (ctype == ConversionType.Narrowing)
					narrowingConv.Add (filteredMatchList[x]);
				if (bestMatch == ConversionType.None || ctype < bestMatch) {
					bestMatch = ctype;
					if (ctype == ConversionType.Narrowing)
						numNarrowingConversions ++;
					if (ctype == ConversionType.Widening)
						numWideningConversions ++;
					mbase = filteredMatchList [x];
				} else if (bestMatch == ctype) {
					if (bestMatch == ConversionType.Widening || bestMatch == ConversionType.Exact) {
						// Got a widening conversion before also.
						// Find the best among the two
						int closestMatch = GetClosestMatch (mbase, filteredMatchList [x], args.Length);
						if (closestMatch == -1) {
							numWideningConversions ++;
						}
						else if (closestMatch == 1)
							mbase = filteredMatchList [x];
					} else {
						numNarrowingConversions ++;
					}
				}
			}

			if (bestMatch == ConversionType.Narrowing && numNarrowingConversions > 1) {
				//TODO : print the methods too
				throw new AmbiguousMatchException ("No overloaded '" + this.objectType + "." + this.bindToName + "' can be called without a narrowing conversion");
			}
			if ((bestMatch == ConversionType.Widening || bestMatch == ConversionType.Exact) && numWideningConversions > 1) {
				//TODO : print the methods too
				throw new AmbiguousMatchException ("No overloaded '" + this.objectType + "." + this.bindToName + "' can be called without a widening conversion");
			}


			if (mbase == null)
				return null;

			int count = 0;
			ParameterInfo[] pars = mbase.GetParameters ();
			if (pars.Length == 0)
				return mbase;
			int numFixedParams = pars.Length;
			if (UsesParamArray (mbase))
				numFixedParams --;

			for(int y = 0; y < numFixedParams; y++)
			{
				if((args [y] = ObjectType.CTypeHelper (args[y], pars[y].ParameterType)) != null)
					count++;
				else
					break;
			}


			if (UsesParamArray (mbase)) {
				int index = 0;
				Type paramArrayType = pars [pars.GetUpperBound (0)].ParameterType;
				Array paramArgs = Array.CreateInstance (paramArrayType.GetElementType (), args.Length - numFixedParams);
				bool isArgArray = false;
				if (numFixedParams + 1 == args.Length) {
					if (args [numFixedParams].GetType().IsArray) {
						isArgArray = true;
						count ++;
					}
				}

				if (!isArgArray) {
					for (int y = numFixedParams; y < args.Length; y ++) {
						Type dest_type = paramArrayType;
						if (!args [y].GetType ().IsArray) {
							dest_type = paramArrayType.GetElementType ();
						}
						if((args [y] = ObjectType.CTypeHelper (args[y], dest_type)) != null) {
							paramArgs.SetValue (args [y], index);
						} else
							break;
						count++;
						index ++;
					}

					object[] newArgs = new object [pars.Length];
					Array.Copy (args, newArgs, numFixedParams);
					newArgs [newArgs.GetUpperBound (0)] = paramArgs;
					args = newArgs;
				}
			}

			if (count != arguments.Length)
				return null;

			if (byRefFlags == null || pars.Length == 0) {
				return mbase;
			}

			for (int index = 0; index < pars.Length; index ++) {
				ParameterInfo p = pars [index];
				if (p.ParameterType.IsByRef) {
					if (byRefFlags [index] != false)
						byRefFlags [index] = true;
				} else
					byRefFlags [index] = false;
			}

			return mbase;
		}

		private int GetClosestMatch (MethodBase bestMatch, MethodBase candidate, int argCount) {
			// flag to indicate which one has been better so far
			// -1 : none is better than other
			// 0 : bestMatch has been better so far
			// 1 : candidate is better than bestMatch
			int isBetter = -2;
			ParameterInfo[] bestMatchParams = bestMatch.GetParameters ();
			ParameterInfo[] candidateParams = candidate.GetParameters ();
			int numParams = Math.Min (bestMatchParams.Length, candidateParams.Length);
			int paramArrayIndex1 = -1, paramArrayIndex2 = -1;
			if (UsesParamArray (bestMatch)) {
				paramArrayIndex1 = (bestMatchParams.Length > 0) ? (bestMatchParams.Length - 1) : -1;
			}

			if (UsesParamArray (candidate)) {
				paramArrayIndex2 = (candidateParams.Length > 0) ? (candidateParams.Length - 1) : -1;
			}

			for (int i = 0; i < argCount; i ++) {
				int index1 = i, index2 = i;
				Type bestMatchParamsType = null;
				Type candParamType = null;
				if (i >= paramArrayIndex1 && paramArrayIndex1 != -1) {
					index1 = paramArrayIndex1;
					bestMatchParamsType = bestMatchParams [index1].ParameterType.GetElementType ();
				} else 
					bestMatchParamsType = bestMatchParams [index1].ParameterType;
				if (i >= paramArrayIndex2 && paramArrayIndex2 != -1) {
					index2 = paramArrayIndex2;
					candParamType = candidateParams [index2].ParameterType.GetElementType ();
				} else
					candParamType = candidateParams [index2].ParameterType;


				if (bestMatchParamsType == candParamType)
					continue;

				if (ObjectType.IsWideningConversion (bestMatchParamsType, candParamType)) {
					// ith param of candidate is wider than that of bestMatch
					if (isBetter == -2) {
						isBetter = 0;
						continue;
					} else if (isBetter != 0) {
						isBetter = -1;
						continue;
					}
					isBetter = 0;
					
				} else if (ObjectType.IsWideningConversion (candParamType, bestMatchParamsType)) {
					// ith param of bestMatch is wider than that of candidate
					if (isBetter == -2) {
						isBetter = 1;
						continue;
					} else if (isBetter != 1) {
						isBetter = -1;
						continue;
					}
					isBetter = 1;
				}
			}

			if (isBetter == -2) {
				// corresponding parameters of both methods have same types
				// the method having max no of fixed parameters is better
				if (paramArrayIndex1 == -1 && paramArrayIndex2 != -1)
						return 0;
				if (paramArrayIndex1 != -1 && paramArrayIndex2 == -1)
						return 1;
				return ((paramArrayIndex1 < paramArrayIndex2) ? 1 : 0);
			}

			return isBetter;
		}

		internal static bool UsesParamArray (MethodBase mb) {
			ParameterInfo[] pars = mb.GetParameters ();
			if (pars.Length == 0)
				return false;
			ParameterInfo lastParam = pars [pars.GetUpperBound (0)];
			object[] attrs = lastParam.GetCustomAttributes (typeof (System.ParamArrayAttribute), false);
			if (attrs == null || attrs.Length == 0)
				return false;
			return true;
		}

		private ConversionType GetConversionType (ParameterInfo[] parameters, object[] args) {
			int numParams = parameters.Length;
			int numArgs = args.Length;
			int paramArrayIndex = -1;
			int minParams = parameters.Length;
			if (numParams == 0) {
				if (numArgs == 0)
					return ConversionType.Exact;
				else
					return ConversionType.None;
			}

			ParameterInfo lastParam = parameters [parameters.GetUpperBound (0)];
			object[] attrs = lastParam.GetCustomAttributes (typeof (System.ParamArrayAttribute), false);
			bool usesParamArray = false;
			if (attrs != null && attrs.Length > 0) {
				usesParamArray = true;
				paramArrayIndex = parameters.GetUpperBound (0) - 1;
				minParams --;
			}
			if (minParams == 0 && numArgs == 0)
				return ConversionType.Exact;
			if (numArgs < minParams)
				return ConversionType.None;
			if (! usesParamArray && numArgs != numParams) {
				return ConversionType.None;
			}

			ConversionType ctype = ConversionType.None;
			for (int index = 0; index < minParams; index ++) {
				ConversionType currentCType = ConversionType.None;
				Type type1 = args [index].GetType ();
				Type type2 = parameters [index].ParameterType;
				if (type1 == type2) {
					currentCType = ConversionType.Exact;
					if (ctype < currentCType) {
						ctype = ConversionType.Exact;
					}
				} else if (ObjectType.IsWideningConversion (type1, type2)) {
					currentCType = ConversionType.Widening;
					if (ctype < currentCType)
						ctype = ConversionType.Widening;
				} else 
					ctype = ConversionType.Narrowing;
			}

			if (usesParamArray) {
				Type paramArrayType = lastParam.ParameterType;
				if (paramArrayType.IsByRef)
					paramArrayType = paramArrayType.GetElementType ();

				for (int index = minParams; index < numArgs; index ++) {
					ConversionType currentCType = ConversionType.None;
					Type argType = args [index].GetType ();
					if (argType.IsArray) {
						if (argType.GetElementType () == paramArrayType.GetElementType ())
							currentCType = ConversionType.Exact;
						else if (ObjectType.IsWideningConversion (argType, paramArrayType))
							currentCType = ConversionType.Widening;
						else 
							currentCType = ConversionType.Narrowing;
					} else {
						Type elementType = paramArrayType.GetElementType ();
						if (argType == elementType)
							currentCType = ConversionType.Exact;
						else if (ObjectType.IsWideningConversion (argType, elementType))
							currentCType = ConversionType.Widening;
						else 
							currentCType = ConversionType.Narrowing;
					}
					if (currentCType == ConversionType.Narrowing || ctype < currentCType)
						ctype = currentCType;
				}
			}

			return ctype;
		}

		public override object ChangeType(
			object value,
			Type myChangeType,
			CultureInfo culture
			)
		{		
			TypeCode src_type = Type.GetTypeCode (value.GetType());			
			TypeCode dest_type = Type.GetTypeCode (myChangeType);
			
			switch (dest_type) {
				case TypeCode.String:
					switch (src_type) {
						case TypeCode.SByte:						
						case TypeCode.Byte:
							return (StringType.FromByte ((byte)value));
						case TypeCode.UInt16:
						case TypeCode.Int16:
							return (StringType.FromShort ((short)value));	
						case TypeCode.UInt32:					
						case TypeCode.Int32:
							return (StringType.FromInteger ((int)value));						
						case TypeCode.UInt64:	
						case TypeCode.Int64:
							return (StringType.FromLong ((long)value));						
						case TypeCode.Char:
							return (StringType.FromChar ((char)value));							
						case TypeCode.Single:
							return (StringType.FromSingle ((float)value));	
						case TypeCode.Double:
							return (StringType.FromDouble ((double)value));																		
						case TypeCode.Boolean:
							return (StringType.FromBoolean ((bool)value));	
						case TypeCode.Object:
							return (StringType.FromObject (value));																												
					}
					break;
					
				case TypeCode.Int32:
				case TypeCode.UInt32:	
					switch (src_type) {						
						case TypeCode.String:				
							return (IntegerType.FromString ((string)value));	
						case TypeCode.Object:				
							return (IntegerType.FromObject (value));										
					}
					break;	

				case TypeCode.Int16:
				case TypeCode.UInt16:	
					switch (src_type) {						
						case TypeCode.String:				
							return (ShortType.FromString ((string)value));		
						case TypeCode.Object:				
							return (ShortType.FromObject (value));										
					}
					break;	
				case TypeCode.Object:
					return ((Object) value);												
			}
			return null;
		}

		public override void ReorderArgumentArray(
			ref object[] args,
			object state
			)
		{

		}

		public override MethodBase SelectMethod(
			BindingFlags bindingAttr,
			MethodBase[] match,
			Type[] types,
			ParameterModifier[] modifiers
			)
		{
			return null;
		}

		public override PropertyInfo SelectProperty(
			BindingFlags bindingAttr,
			PropertyInfo[] match,
			Type returnType,
			Type[] indexes,
			ParameterModifier[] modifiers
			)		
		{
			return null;
		}
		
		public Object InvokeMember (string name, 
					    BindingFlags flags,
					    Type objType,
					    IReflect objReflect,
					    object target,
					    object[] args,
					    ParameterModifier[] modifiers,
					    CultureInfo culture,
					    string[] paramNames) {

			this.objectType = objType;
			this.bindToName = name;
			if (name == null) {
				// Must be a default property
				Type t = objType;
				while (t != null) {
					object[] attrArray = t.GetCustomAttributes (typeof (DefaultMemberAttribute), false);
					if (attrArray != null && attrArray.Length != 0) {
						name = ((DefaultMemberAttribute) attrArray[0]).MemberName;
						break;
					}
					// not found, search in the base type
					t = t.BaseType;
				}
			}

			if (name == null) {
				throw new MissingMemberException ("No default members defined for type '" + objType + "'");
			}

			MemberInfo[] memberinfo = objReflect.GetMember (name, flags);
			if (memberinfo == null || memberinfo.Length == 0) {
				throw new MissingMemberException ("No member '" + name + "' defined for type '" + objType + "'");
			}
			
			object objState = null;
			object retVal = null;
			if (memberinfo [0] is MethodBase) {
				MethodBase[] methodbase = new MethodBase [memberinfo.Length];
				for (int index = 0; index < memberinfo.Length; index ++)
					methodbase [index] = (MethodBase) memberinfo [index];
				MethodBase mbase = BindToMethod (flags, methodbase, ref args, modifiers, culture, paramNames, out objState);
				if (mbase == null) {
					throw new MissingMemberException ("No member '" + name + "' defined for type '" + objType + "' which takes the given set of arguments");
				}
				MethodInfo mi = (MethodInfo) mbase;
				retVal =  mi.Invoke (target, args);
			}

			if (objState != null && ((BinderState)objState).byRefFlags != null) {
				this.byRefFlags = ((BinderState)objState).byRefFlags; 
			}

			return retVal;
		}

		private bool IsApplicable (MethodBase mb, object [] args) {
			ParameterInfo [] parameters = mb.GetParameters ();
			int numFixedParams = parameters.Length;
			int argCount = 0;
			if (args != null)
				argCount = args.Length;

			if (numFixedParams == 0)
				return (argCount == 0);

			ParameterInfo lastParam =  parameters [parameters.GetUpperBound (0)];
			bool usesParamArray = UsesParamArray (mb);
			if (usesParamArray)
				numFixedParams --;
			else if (numFixedParams != argCount)
				return false;

			if (argCount < numFixedParams)
				return false;
			if (argCount == 0 && numFixedParams == 0)
				return true;
 
			for (int index = 0; index < numFixedParams; index ++) {
				Type argType = args [index].GetType ();
				Type paramType = parameters [index].ParameterType;
				if (paramType.IsByRef)
					paramType = paramType.GetElementType ();
				if (!ObjectType.ImplicitConversionExists (argType, paramType))
					return false;
			}

			if (usesParamArray) {
				Type paramArrayType = lastParam.ParameterType;

				for (int index = numFixedParams; index < argCount; index ++) {
					Type argType = args [index].GetType ();
					if (!argType.IsArray) {
						Type elementType = paramArrayType.GetElementType ();
						if (!ObjectType.ImplicitConversionExists (argType, elementType))
							return false;
					} else {
						Type elementType = paramArrayType.GetElementType ();
						argType = argType.GetElementType ();
						if (!elementType.IsAssignableFrom (argType))
							return false;
					}
				}
			}
			return true;
		}

		private static MemberInfo [] GetMostDerivedMembers (ArrayList memberinfo) {
			int i = 0;
			int numElementsEliminated = 0;
			for (i = 0; i < memberinfo.Count; i++) {
				MemberInfo mi = (MemberInfo) memberinfo [i];
				for (int j = i + 1; j < memberinfo.Count; j++) {
					bool eliminateBaseMembers = false;
					MemberInfo thisMember = (MemberInfo) memberinfo [j];
					Type t1 = mi.DeclaringType;
					Type t2 = thisMember.DeclaringType;
					if (mi.MemberType == MemberTypes.Field)
						eliminateBaseMembers = true;
					if (mi.MemberType == MemberTypes.Method) {
						MethodInfo methodinfo = (MethodInfo) mi;
						if (methodinfo.IsVirtual)
							eliminateBaseMembers = true;
					}
					if (mi.MemberType == MemberTypes.Property) {
						PropertyInfo propertyinfo = (PropertyInfo) mi;
						MethodInfo method = propertyinfo.GetGetMethod ();
						if (method.IsVirtual)
							eliminateBaseMembers = true;
					}
					if (eliminateBaseMembers) {
						if (t1.IsSubclassOf (t2)) {
							memberinfo [j] = null;
							numElementsEliminated ++;
						} else if (t2.IsSubclassOf (t1)) {
							memberinfo [i] = null;
							numElementsEliminated ++;
						}
					}
				}
			}

			MemberInfo [] newMemberList = new MemberInfo [memberinfo.Count - numElementsEliminated];
			int newIndex = 0;
			for (int index = 0; index < memberinfo.Count; index ++) {
				if (memberinfo [index] != null) {
					newMemberList [newIndex ++] = (MemberInfo) memberinfo [index];
				}
			}
			return newMemberList;
		}
	}
}
