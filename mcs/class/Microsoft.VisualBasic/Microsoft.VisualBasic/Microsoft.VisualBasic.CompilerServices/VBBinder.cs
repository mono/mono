//
// LateBinding.cs
//
// Author:
//   Marco Ridoni    (marco.ridoni@virgilio.it)
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
			if(match == null)
				throw new ArgumentNullException();
				
			ConversionType bestMatch = ConversionType.None;
			int numWideningConversions = 0, numNarrowingConversions = 0;
			for(int x = 0; x < match.Length; x++)
			{
				ParameterInfo[] parameters = match[x].GetParameters();
				ConversionType ctype = GetConversionType (parameters, args);
				if (bestMatch == ConversionType.None || ctype < bestMatch) {
					bestMatch = ctype;
					if (ctype == ConversionType.Narrowing)
						numNarrowingConversions ++;
					if (ctype == ConversionType.Widening)
						numWideningConversions ++;
					mbase = match [x];
				} else if (bestMatch == ctype) {
					if (bestMatch == ConversionType.Widening || bestMatch == ConversionType.Exact) {
						// Got a widening conversion before also.
						// Find the best among the two
						int closestMatch = GetClosestMatch (mbase, match [x]);
						if (closestMatch == -1) {
							numWideningConversions ++;
						}
						else if (closestMatch == 1)
							mbase = match [x];
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

			if (mbase != null) {
				int count = 0;
				ParameterInfo[] parameters = mbase.GetParameters ();
				for(int y = 0; y < args.Length; y++)
				{
					if((args [y] = ObjectType.CTypeHelper (args[y], parameters[y].ParameterType)) != null)
						count++;
					else
						break;
				}
				if (count != args.Length)
					return null;
			}

			ParameterInfo [] pars = mbase.GetParameters ();
			int index = 0;
			if (byRefFlags == null || pars.Length == 0 || !ByRefParamsExist (pars)) {
				return mbase;
			}
			for (index = 0; index < pars.Length; index ++) {
				ParameterInfo p = pars [index];
				if (p.ParameterType.IsByRef) {
					if (byRefFlags [index] != false)
						byRefFlags [index] = true;
				} else
					byRefFlags [index] = false;
			}
			return mbase;
		}

		private int GetClosestMatch (MethodBase bestMatch, MethodBase candidate) {
			// flag to indicate which one has been better so far
			// -1 : none is better than other
			// 0 : bestMatch has been better so far
			// 1 : candidate is better than bestMatch
			int isBetter = -2;
			ParameterInfo[] bestMatchParams = bestMatch.GetParameters ();
			ParameterInfo[] candidateParams = candidate.GetParameters ();
			int numParams = Math.Min (bestMatchParams.Length, candidateParams.Length);
			for (int i = 0; i < numParams; i ++) {
				if (bestMatchParams [i].ParameterType == candidateParams [i].ParameterType)
					continue;

				if (ObjectType.IsWideningConversion (bestMatchParams [i].ParameterType, candidateParams [i].ParameterType)) {
					// ith param of candidate is wider than that of bestMatch
					if (isBetter == -2) {
						isBetter = 0;
						continue;
					} else if (isBetter != 0) {
						isBetter = -1;
						continue;
					}
					isBetter = 0;
					
				} else if (ObjectType.IsWideningConversion (candidateParams [i].ParameterType, bestMatchParams [i].ParameterType)) {
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
			return isBetter;
		}

		private ConversionType GetConversionType (ParameterInfo[] parameters, object[] args) {
			int numParams = parameters.Length;
			int numArgs = args.Length;
			int minParams = Math.Min (numParams, numArgs);
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
			MethodBase[] methodbase = GetMostDerivedMembers (memberinfo);
			if (methodbase == null || methodbase.Length == 0) {
				throw new MissingMemberException ("No member '" + name + "' defined for type '" + objType + "'");
			}

			object objState = null;
			MethodBase mbase = BindToMethod (flags, methodbase, ref args, modifiers, culture, paramNames, out objState);
			if (mbase == null) {
				throw new MissingMemberException ("No member '" + name + "' defined for type '" + objType + "' which takes the given set of arguments");
			}

			if (objState != null && ((BinderState)objState).byRefFlags != null) {
				this.byRefFlags = ((BinderState)objState).byRefFlags; 
			}

			MethodInfo mi = (MethodInfo) mbase;
			object retVal =  mi.Invoke (target, args);


			return retVal;
		}

		private bool ByRefParamsExist (ParameterInfo [] pars) {
			foreach (ParameterInfo p in pars) {
				if (p.ParameterType.IsByRef)
					return true;
			}
			return false;
		}
		private static MethodBase [] GetMostDerivedMembers (MemberInfo[] memberinfo) {
			int i = 0;
			int numElementsEliminated = 0;
			for (i = 0; i < memberinfo.Length; i++) {
				MemberInfo mi = memberinfo [i];
				for (int j = i + 1; j < memberinfo.Length; j++) {
					bool eliminateBaseMembers = false;
					Type t1 = mi.DeclaringType;
					Type t2 = memberinfo [j].DeclaringType;
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

			MethodBase [] newMemberList = new MethodBase [memberinfo.Length - numElementsEliminated];
			int newIndex = 0;
			for (int index = 0; index < memberinfo.Length; index ++) {
				if (memberinfo [index] != null) {
					newMemberList [newIndex ++] = (MethodBase) memberinfo [index];
				}
			}
			return newMemberList;
		}
	}
}
