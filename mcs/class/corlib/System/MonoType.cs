//
// System.MonoType
//
// Authors: 
// 	Sean MacIsaac (macisaac@ximian.com)
// 	Paolo Molaro (lupus@ximian.com)
// 	Patrik Torstensson (patrik.torstensson@labs2.com)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//  Marek Safar (marek.safar@gmail.com)
//
// (c) 2001-2003 Ximian, Inc.
// Copyright (C) 2003-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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

using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;
using System.Diagnostics.Contracts;

namespace System
{
	// Contains information about the type which is expensive to compute
	[StructLayout (LayoutKind.Sequential)]
	internal class MonoTypeInfo {
		public string full_name;
		public MonoCMethod default_ctor;
	}
		
	abstract class RuntimeType : TypeInfo
	{
		internal RuntimeAssembly GetRuntimeAssembly ()
		{
			return (RuntimeAssembly) Assembly;
		}

		internal virtual bool IsSzArray {
			get {
				return IsArrayImpl () && GetArrayRank () == 1;
			}
		}

		internal Object CheckValue (Object value, Binder binder, CultureInfo culture, BindingFlags invokeAttr)
		{
			bool failed = false;
			var res = TryConvertToType (value, ref failed);
			if (!failed)
				return res;

			if ((invokeAttr & BindingFlags.ExactBinding) == BindingFlags.ExactBinding)
				throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_ObjObjEx"), value.GetType(), this));

			if (binder != null && binder != Type.DefaultBinder)
				return binder.ChangeType (value, this, culture);

			throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_ObjObjEx"), value.GetType(), this));
		}

		object TryConvertToType (object value, ref bool failed)
		{
			if (IsInstanceOfType (value)) {
				return value;
			}

			if (IsByRef) {
				var elementType = GetElementType ();
				if (value == null || elementType.IsInstanceOfType (value)) {
					return value;
				}
			}

			if (value == null)
				return value;

			if (IsEnum) {
				var type = Enum.GetUnderlyingType (this);
				if (type == value.GetType ())
					return value;
				var res = IsConvertibleToPrimitiveType (value, this);
				if (res != null)
					return res;
			} else if (IsPrimitive) {
				var res = IsConvertibleToPrimitiveType (value, this);
				if (res != null)
					return res;
			} else if (IsPointer) {
				var vtype = value.GetType ();
				if (vtype == typeof (IntPtr) || vtype == typeof (UIntPtr))
					return value;
			}

			failed = true;
			return null;
		}

		// Binder uses some incompatible conversion rules. For example
		// int value cannot be used with decimal parameter but in other
		// ways it's more flexible than normal convertor, for example
		// long value can be used with int based enum
		static object IsConvertibleToPrimitiveType (object value, Type targetType)
		{
			var type = value.GetType ();
			if (type.IsEnum) {
				type = Enum.GetUnderlyingType (type);
				if (type == targetType)
					return value;
			}

			var from = Type.GetTypeCode (type);
			var to = Type.GetTypeCode (targetType);

			switch (to) {
				case TypeCode.Char:
					switch (from) {
						case TypeCode.Byte:
							return (Char) (Byte) value;
						case TypeCode.UInt16:
							return value;
					}
					break;
				case TypeCode.Int16:
					switch (from) {
						case TypeCode.Byte:
							return (Int16) (Byte) value;
						case TypeCode.SByte:
							return (Int16) (SByte) value;
					}
					break;
				case TypeCode.UInt16:
					switch (from) {
						case TypeCode.Byte:
							return (UInt16) (Byte) value;
						case TypeCode.Char:
							return value;
					}
					break;
				case TypeCode.Int32:
					switch (from) {
						case TypeCode.Byte:
							return (Int32) (Byte) value;
						case TypeCode.SByte:
							return (Int32) (SByte) value;
						case TypeCode.Char:
							return (Int32) (Char) value;
						case TypeCode.Int16:
							return (Int32) (Int16) value;
						case TypeCode.UInt16:
							return (Int32) (UInt16) value;
					}
					break;
				case TypeCode.UInt32:
					switch (from) {
						case TypeCode.Byte:
							return (UInt32) (Byte) value;
						case TypeCode.Char:
							return (UInt32) (Char) value;
						case TypeCode.UInt16:
							return (UInt32) (UInt16) value;
					}
					break;
				case TypeCode.Int64:
					switch (from) {
						case TypeCode.Byte:
							return (Int64) (Byte) value;
						case TypeCode.SByte:
							return (Int64) (SByte) value;
						case TypeCode.Int16:
							return (Int64) (Int16) value;
						case TypeCode.Char:
							return (Int64) (Char) value;
						case TypeCode.UInt16:
							return (Int64) (UInt16) value;
						case TypeCode.Int32:
							return (Int64) (Int32) value;
						case TypeCode.UInt32:
							return (Int64) (UInt32) value;
					}
					break;
				case TypeCode.UInt64:
					switch (from) {
						case TypeCode.Byte:
							return (UInt64) (Byte) value;
						case TypeCode.Char:
							return (UInt64) (Char) value;
						case TypeCode.UInt16:
							return (UInt64) (UInt16) value;
						case TypeCode.UInt32:
							return (UInt64) (UInt32) value;
					}
					break;
				case TypeCode.Single:
					switch (from) {
						case TypeCode.Byte:
							return (Single) (Byte) value;
						case TypeCode.SByte:
							return (Single) (SByte) value;
						case TypeCode.Int16:
							return (Single) (Int16) value;
						case TypeCode.Char:
							return (Single) (Char) value;
						case TypeCode.UInt16:
							return (Single) (UInt16) value;
						case TypeCode.Int32:
							return (Single) (Int32) value;
						case TypeCode.UInt32:
							return (Single) (UInt32) value;
						case TypeCode.Int64:
							return (Single) (Int64) value;
						case TypeCode.UInt64:
							return (Single) (UInt64) value;
					}
					break;
				case TypeCode.Double:
					switch (from) {
						case TypeCode.Byte:
							return (Double) (Byte) value;
						case TypeCode.SByte:
							return (Double) (SByte) value;
						case TypeCode.Char:
							return (Double) (Char) value;
						case TypeCode.Int16:
							return (Double) (Int16) value;
						case TypeCode.UInt16:
							return (Double) (UInt16) value;
						case TypeCode.Int32:
							return (Double) (Int32) value;
						case TypeCode.UInt32:
							return (Double) (UInt32) value;
						case TypeCode.Int64:
							return (Double) (Int64) value;
						case TypeCode.UInt64:
							return (Double) (UInt64) value;
						case TypeCode.Single:
							return (Double) (Single) value;
					}
					break;
			}

			// Everything else is rejected
			return null;
		}

        protected ListBuilder<MethodInfo> GetMethodCandidates (MethodInfo[] cache, BindingFlags bindingAttr, CallingConventions callConv, Type[] types)
        {
        	var candidates = new ListBuilder<MethodInfo> ();

            for (int i = 0; i < cache.Length; i++) {
				var methodInfo = cache[i];
				if (FilterApplyMethodBase(methodInfo, /*methodInfo.BindingFlags,*/ bindingAttr, callConv, types)) {
					candidates.Add (methodInfo);
				}
			}

			return candidates;
		}

        private static bool FilterApplyMethodBase(
            MethodBase methodBase, /*BindingFlags methodFlags,*/ BindingFlags bindingFlags, CallingConventions callConv, Type[] argumentTypes)
        {
            Contract.Requires(methodBase != null);

            bindingFlags ^= BindingFlags.DeclaredOnly;
/*
            #region Apply Base Filter
            if ((bindingFlags & methodFlags) != methodFlags)
                return false;
            #endregion
*/
            #region Check CallingConvention
            if ((callConv & CallingConventions.Any) == 0)
            {
                if ((callConv & CallingConventions.VarArgs) != 0 && 
                    (methodBase.CallingConvention & CallingConventions.VarArgs) == 0)
                    return false;

                if ((callConv & CallingConventions.Standard) != 0 && 
                    (methodBase.CallingConvention & CallingConventions.Standard) == 0)
                    return false;
            }
            #endregion

            #region If argumentTypes supplied
            if (argumentTypes != null)
            {
                ParameterInfo[] parameterInfos = methodBase.GetParametersNoCopy();

                if (argumentTypes.Length != parameterInfos.Length)
                {
                    #region Invoke Member, Get\Set & Create Instance specific case
                    // If the number of supplied arguments differs than the number in the signature AND
                    // we are not filtering for a dynamic call -- InvokeMethod or CreateInstance -- filter out the method.
                    if ((bindingFlags & 
                        (BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetProperty | BindingFlags.SetProperty)) == 0)
                        return false;
                    
                    bool testForParamArray = false;
                    bool excessSuppliedArguments = argumentTypes.Length > parameterInfos.Length;

                    if (excessSuppliedArguments) 
                    { // more supplied arguments than parameters, additional arguments could be vararg
                        #region Varargs
                        // If method is not vararg, additional arguments can not be passed as vararg
                        if ((methodBase.CallingConvention & CallingConventions.VarArgs) == 0)
                        {
                            testForParamArray = true;
                        }
                        else 
                        {
                            // If Binding flags did not include varargs we would have filtered this vararg method.
                            // This Invariant established during callConv check.
                            Contract.Assert((callConv & CallingConventions.VarArgs) != 0);
                        }
                        #endregion
                    }
                    else 
                    {// fewer supplied arguments than parameters, missing arguments could be optional
                        #region OptionalParamBinding
                        if ((bindingFlags & BindingFlags.OptionalParamBinding) == 0)
                        {
                            testForParamArray = true;
                        }
                        else
                        {
                            // From our existing code, our policy here is that if a parameterInfo 
                            // is optional then all subsequent parameterInfos shall be optional. 

                            // Thus, iff the first parameterInfo is not optional then this MethodInfo is no longer a canidate.
                            if (!parameterInfos[argumentTypes.Length].IsOptional)
                                testForParamArray = true;
                        }
                        #endregion
                    }

                    #region ParamArray
                    if (testForParamArray)
                    {
                        if  (parameterInfos.Length == 0)
                            return false;

                        // The last argument of the signature could be a param array. 
                        bool shortByMoreThanOneSuppliedArgument = argumentTypes.Length < parameterInfos.Length - 1;

                        if (shortByMoreThanOneSuppliedArgument)
                            return false;

                        ParameterInfo lastParameter = parameterInfos[parameterInfos.Length - 1];

                        if (!lastParameter.ParameterType.IsArray)
                            return false;

                        if (!lastParameter.IsDefined(typeof(ParamArrayAttribute), false))
                            return false;
                    }
                    #endregion

                    #endregion
                }
                else
                {
                    #region Exact Binding
                    if ((bindingFlags & BindingFlags.ExactBinding) != 0)
                    {
                        // Legacy behavior is to ignore ExactBinding when InvokeMember is specified.
                        // Why filter by InvokeMember? If the answer is we leave this to the binder then why not leave
                        // all the rest of this  to the binder too? Further, what other semanitc would the binder
                        // use for BindingFlags.ExactBinding besides this one? Further, why not include CreateInstance 
                        // in this if statement? That's just InvokeMethod with a constructor, right?
                        if ((bindingFlags & (BindingFlags.InvokeMethod)) == 0)
                        {
                            for(int i = 0; i < parameterInfos.Length; i ++)
                            {
                                // a null argument type implies a null arg which is always a perfect match
                                if ((object)argumentTypes[i] != null && !Object.ReferenceEquals(parameterInfos[i].ParameterType, argumentTypes[i]))
                                    return false;
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion
        
            return true;
        }

        // Helper to build lists of MemberInfos. Special cased to avoid allocations for lists of one element.
        protected struct ListBuilder<T> where T : class
        {
            T[] _items;
            T _item;
            int _count;
            int _capacity;

            public ListBuilder(int capacity)
            {
                _items = null;
                _item = null;
                _count = 0;
                _capacity = capacity;
            }

            public T this[int index]
            {
                get
                {
                    Contract.Requires(index < Count);
                    return (_items != null) ? _items[index] : _item;
                }
#if FEATURE_LEGACYNETCF
                // added for Dev11 466969 quirk
                set
                {
                    Contract.Requires(index < Count);
                    if (_items != null)
                        _items[index] = value;
                    else
                        _item = value;
                }
#endif
            }

            public T[] ToArray()
            {
                if (_count == 0)
                    return EmptyArray<T>.Value;
                if (_count == 1)
                    return new T[1] { _item };

                Array.Resize(ref _items, _count);
                _capacity = _count;
                return _items;
            }

            public void CopyTo(Object[] array, int index)
            {
                if (_count == 0)
                    return;

                if (_count == 1)
                {
                    array[index] = _item;
                    return;
                }

                Array.Copy(_items, 0, array, index, _count);
            }

            public int Count
            {
                get
                {
                    return _count;
                }
            }

            public void Add(T item)
            {
                if (_count == 0)
                {
                    _item = item;
                }
                else                
                {
                    if (_count == 1)
                    {
                        if (_capacity < 2)
                            _capacity = 4;
                        _items = new T[_capacity];
                        _items[0] = _item;
                    }
                    else
                    if (_capacity == _count)
                    {
                        int newCapacity = 2 * _capacity;
                        Array.Resize(ref _items, newCapacity);
                        _capacity = newCapacity;
                    }

                    _items[_count] = item;
                }
                _count++;
            }
        }
	}

	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	sealed class MonoType : RuntimeType, ISerializable
	{
		[NonSerialized]
		MonoTypeInfo type_info;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void type_from_obj (MonoType type, Object obj);
		
		internal MonoType (Object obj)
		{
			// this should not be used - lupus
			type_from_obj (this, obj);
			
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern TypeAttributes get_attributes (Type type);

		public MonoCMethod GetDefaultConstructor ()
		{
			MonoCMethod ctor = null;
			
			if (type_info == null)
				type_info = new MonoTypeInfo ();
			else
				ctor = type_info.default_ctor;

			if (ctor == null) {
				var ctors = GetConstructors (BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

				for (int i = 0; i < ctors.Length; ++i) {
					if (ctors [i].GetParametersCount () == 0) {
						type_info.default_ctor = ctor = (MonoCMethod) ctors [i];
						break;
					}
				}
			}

			return ctor;
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			return get_attributes (this);
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr,
								       Binder binder,
								       CallingConventions callConvention,
								       Type[] types,
								       ParameterModifier[] modifiers)
		{
			ConstructorInfo[] candidates = GetConstructors (bindingAttr);

			if (candidates.Length == 0)
				return null;

			if (types.Length == 0 && candidates.Length == 1) {
				ConstructorInfo firstCandidate = candidates [0];
				var parameters = firstCandidate.GetParametersNoCopy ();
				if (parameters == null || parameters.Length == 0)
					return firstCandidate;
			}

			if ((bindingAttr & BindingFlags.ExactBinding) != 0)
				return (ConstructorInfo) System.DefaultBinder.ExactBinding (candidates, types, modifiers);

 			if (binder == null)
				binder = DefaultBinder;

			return (ConstructorInfo) CheckMethodSecurity (binder.SelectMethod (bindingAttr, candidates, types, modifiers));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern ConstructorInfo[] GetConstructors_internal (BindingFlags bindingAttr, Type reflected_type);

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			return GetConstructors_internal (bindingAttr, this);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern EventInfo InternalGetEvent (string name, BindingFlags bindingAttr);

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			return InternalGetEvent (name, bindingAttr);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern EventInfo[] GetEvents_internal (BindingFlags bindingAttr, Type reflected_type);

		public override EventInfo[] GetEvents (BindingFlags bindingAttr)
		{
			return GetEvents_internal (bindingAttr, this);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override FieldInfo GetField (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern FieldInfo[] GetFields_internal (BindingFlags bindingAttr, Type reflected_type);

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			return GetFields_internal (bindingAttr, this);
		}
		
		public override Type GetInterface (string name, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException ();

			Type[] interfaces = GetInterfaces();

			foreach (Type type in interfaces) {
				/*We must compare against the generic type definition*/
				Type t = type.IsGenericType ? type.GetGenericTypeDefinition () : type;

				if (String.Compare (t.Name, name, ignoreCase, CultureInfo.InvariantCulture) == 0)
					return type;
				if (String.Compare (t.FullName, name, ignoreCase, CultureInfo.InvariantCulture) == 0)
					return type;
			}

			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type[] GetInterfaces();

		public override InterfaceMapping GetInterfaceMap (Type interfaceType)
		{
			if (!IsSystemType)
				throw new NotSupportedException ("Derived classes must provide an implementation.");
			if (interfaceType == null)
				throw new ArgumentNullException ("interfaceType");
			if (!interfaceType.IsSystemType)
				throw new ArgumentException ("interfaceType", "Type is an user type");
			InterfaceMapping res;
			if (!interfaceType.IsInterface)
				throw new ArgumentException (Locale.GetText ("Argument must be an interface."), "interfaceType");
			if (IsInterface)
				throw new ArgumentException ("'this' type cannot be an interface itself");
			res.TargetType = this;
			res.InterfaceType = interfaceType;
			GetInterfaceMapData (this, interfaceType, out res.TargetMethods, out res.InterfaceMethods);
			if (res.TargetMethods == null)
				throw new ArgumentException (Locale.GetText ("Interface not found"), "interfaceType");

			return res;
		}
		
		public override MemberInfo[] GetMembers( BindingFlags bindingAttr)
		{
			return FindMembers (MemberTypes.All, bindingAttr, null, null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern MethodInfo [] GetMethodsByName (string name, BindingFlags bindingAttr, bool ignoreCase, Type reflected_type);

		public override MethodInfo [] GetMethods (BindingFlags bindingAttr)
		{
			return GetMethodsByName (null, bindingAttr, false, this);
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr,
							     Binder binder,
							     CallingConventions callConvention,
							     Type[] types, ParameterModifier[] modifiers)
		{
			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);
			var candidates = GetMethodCandidates (GetMethodsByName (name, bindingAttr, ignoreCase, this), bindingAttr, callConvention, types);

			if (candidates.Count == 0)
				return null;
			
			if (types == null || types.Length == 0) {
				var firstCandidate = candidates [0];
				if (candidates.Count == 1)
					return firstCandidate;

				if (types == null) {
					for (int j = 1; j < candidates.Count; j++) {
						MethodInfo methodInfo = candidates [j];
						if (!System.DefaultBinder.CompareMethodSigAndName (methodInfo, firstCandidate))
							throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
					}

					// All the methods have the exact same name and sig so return the most derived one.
					return (MethodInfo) System.DefaultBinder.FindMostDerivedNewSlotMeth (candidates.ToArray (), candidates.Count);
				}
			}

			if (binder == null)
				binder = DefaultBinder;
			
			return (MethodInfo) CheckMethodSecurity (binder.SelectMethod (bindingAttr, candidates.ToArray (), types, modifiers));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo GetCorrespondingInflatedMethod (MethodInfo generic);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern ConstructorInfo GetCorrespondingInflatedConstructor (ConstructorInfo generic);

		internal override MethodInfo GetMethod (MethodInfo fromNoninstanciated)
                {
			if (fromNoninstanciated == null)
				throw new ArgumentNullException ("fromNoninstanciated");
                        return GetCorrespondingInflatedMethod (fromNoninstanciated);
                }

		internal override ConstructorInfo GetConstructor (ConstructorInfo fromNoninstanciated)
		{
			if (fromNoninstanciated == null)
				throw new ArgumentNullException ("fromNoninstanciated");
                        return GetCorrespondingInflatedConstructor (fromNoninstanciated);
		}

		internal override FieldInfo GetField (FieldInfo fromNoninstanciated)
		{
			/* create sensible flags from given FieldInfo */
			BindingFlags flags = fromNoninstanciated.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
			flags |= fromNoninstanciated.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
			return GetField (fromNoninstanciated.Name, flags);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetNestedType (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type[] GetNestedTypes (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern PropertyInfo[] GetPropertiesByName (string name, BindingFlags bindingAttr, bool icase, Type reflected_type);

		public override PropertyInfo [] GetProperties (BindingFlags bindingAttr)
		{
			return GetPropertiesByName (null, bindingAttr, false, this);
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr,
								 Binder binder, Type returnType,
								 Type[] types,
								 ParameterModifier[] modifiers)
		{
			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);
			PropertyInfo [] props = GetPropertiesByName (name, bindingAttr, ignoreCase, this);
			int count = props.Length;
			if (count == 0)
				return null;
			
			if (types == null || types.Length == 0) {
				if (count == 1) {
					var firstCandidate = props [0];

					if ((object)returnType != null && !returnType.IsEquivalentTo (firstCandidate.PropertyType))
						return null;

					return firstCandidate;
				}

				throw new AmbiguousMatchException (Environment.GetResourceString("Arg_AmbiguousMatchException"));
			}

			if ((bindingAttr & BindingFlags.ExactBinding) != 0)
				return System.DefaultBinder.ExactPropertyBinding (props, returnType, types, modifiers);

			if (binder == null)
				binder = DefaultBinder;

			return binder.SelectProperty (bindingAttr, props, returnType, types, modifiers);
		}

		protected override bool HasElementTypeImpl ()
		{
			return IsArrayImpl() || IsByRefImpl() || IsPointerImpl ();
		}

		protected override bool IsArrayImpl ()
		{
			return Type.IsArrayImpl (this);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsByRefImpl ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		protected extern override bool IsCOMObjectImpl ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsPointerImpl ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsPrimitiveImpl ();

		public override bool IsSubclassOf (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			return base.IsSubclassOf (type);
		}

		public override object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, object target, object[] args,
						     ParameterModifier[] modifiers,
						     CultureInfo culture, string[] namedParameters)
		{
			const string bindingflags_arg = "bindingFlags";


			if ((invokeAttr & BindingFlags.CreateInstance) != 0) {
				if ((invokeAttr & (BindingFlags.GetField |
						BindingFlags.GetField | BindingFlags.GetProperty |
						BindingFlags.SetProperty)) != 0)
					throw new ArgumentException (bindingflags_arg);
			} else if (name == null)
				throw new ArgumentNullException ("name");
			if ((invokeAttr & BindingFlags.GetField) != 0 && (invokeAttr & BindingFlags.SetField) != 0)
				throw new ArgumentException ("Cannot specify both Get and Set on a field.", bindingflags_arg);
			if ((invokeAttr & BindingFlags.GetProperty) != 0 && (invokeAttr & BindingFlags.SetProperty) != 0)
				throw new ArgumentException ("Cannot specify both Get and Set on a property.", bindingflags_arg);
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0) {
				if ((invokeAttr & BindingFlags.SetField) != 0)
					throw new ArgumentException ("Cannot specify Set on a field and Invoke on a method.", bindingflags_arg);
				if ((invokeAttr & BindingFlags.SetProperty) != 0)
					throw new ArgumentException ("Cannot specify Set on a property and Invoke on a method.", bindingflags_arg);
			}
			if ((namedParameters != null) && ((args == null) || args.Length < namedParameters.Length))
				throw new ArgumentException ("namedParameters cannot be more than named arguments in number");
			if ((invokeAttr & (BindingFlags.InvokeMethod|BindingFlags.CreateInstance|BindingFlags.GetField|BindingFlags.SetField|BindingFlags.GetProperty|BindingFlags.SetProperty)) == 0)
				throw new ArgumentException ("Must specify binding flags describing the invoke operation required.", bindingflags_arg);

			/* set some defaults if none are provided :-( */
			if ((invokeAttr & (BindingFlags.Public|BindingFlags.NonPublic)) == 0)
				invokeAttr |= BindingFlags.Public;
			if ((invokeAttr & (BindingFlags.Static|BindingFlags.Instance)) == 0)
				invokeAttr |= BindingFlags.Static|BindingFlags.Instance;

			if (binder == null)
				binder = DefaultBinder;

			if ((invokeAttr & BindingFlags.CreateInstance) != 0) {
				return Activator.CreateInstance (this, invokeAttr, binder, args, culture);
			}
			if (name == String.Empty && Attribute.IsDefined (this, typeof (DefaultMemberAttribute))) {
				DefaultMemberAttribute attr = (DefaultMemberAttribute) Attribute.GetCustomAttribute (this, typeof (DefaultMemberAttribute));
				name = attr.MemberName;
			}
			bool ignoreCase = (invokeAttr & BindingFlags.IgnoreCase) != 0;
			string throwMissingMethodDescription = null;
			bool throwMissingFieldException = false;
			
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0) {
				MethodInfo[] methods = GetMethodsByName (name, invokeAttr, ignoreCase, this);
				object state = null;
				if (args == null)
					args = EmptyArray<object>.Value;
				MethodBase m = binder.BindToMethod (invokeAttr, methods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null) {
					if (methods.Length > 0)
						throwMissingMethodDescription = "The best match for method " + name + " has some invalid parameter.";
					else
						throwMissingMethodDescription = "Cannot find method " + name + ".";
				} else {
					ParameterInfo[] parameters = m.GetParametersInternal();
					for (int i = 0; i < parameters.Length; ++i) {
						if (System.Reflection.Missing.Value == args [i] && (parameters [i].Attributes & ParameterAttributes.HasDefault) != ParameterAttributes.HasDefault)
							throw new ArgumentException ("Used Missing.Value for argument without default value", "parameters");
					}
					object result = m.Invoke (target, invokeAttr, binder, args, culture);
					if (state != null)
						binder.ReorderArgumentArray (ref args, state);
					return result;
				}
			}
			if ((invokeAttr & BindingFlags.GetField) != 0) {
				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					return f.GetValue (target);
				} else if ((invokeAttr & BindingFlags.GetProperty) == 0) {
					throwMissingFieldException = true;
				}
				/* try GetProperty */
			} else if ((invokeAttr & BindingFlags.SetField) != 0) {
				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					if (args == null)
						throw new ArgumentNullException ("providedArgs");
					if ((args == null) || args.Length != 1)
						throw new ArgumentException ("Only the field value can be specified to set a field value.", bindingflags_arg);
					f.SetValue (target, args [0]);
					return null;
				} else if ((invokeAttr & BindingFlags.SetProperty) == 0) {
					throwMissingFieldException = true;
				}
				/* try SetProperty */
			}
			if ((invokeAttr & BindingFlags.GetProperty) != 0) {
				PropertyInfo[] properties = GetPropertiesByName (name, invokeAttr, ignoreCase, this);
				object state = null;
				if (args == null)
					args = EmptyArray<object>.Value;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if ((properties [i].GetGetMethod (true) != null))
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetGetMethod (true);
					if (mb != null)
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null) {
					throwMissingFieldException = true;
				} else {
					object result = m.Invoke (target, invokeAttr, binder, args, culture);
					if (state != null)
						binder.ReorderArgumentArray (ref args, state);
					return result;
				}
			} else if ((invokeAttr & BindingFlags.SetProperty) != 0) {
				PropertyInfo[] properties = GetPropertiesByName (name, invokeAttr, ignoreCase, this);
				object state = null;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if (properties [i].GetSetMethod (true) != null)
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetSetMethod (true);
					if (mb != null)
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null) {
					throwMissingFieldException = true;
				} else {
					object result = m.Invoke (target, invokeAttr, binder, args, culture);
					if (state != null)
						binder.ReorderArgumentArray (ref args, state);
					return result;
				}
			}
			if (throwMissingMethodDescription != null)
				throw new MissingMethodException(throwMissingMethodDescription);
			if (throwMissingFieldException)
				throw new MissingFieldException("Cannot find variable " + name + ".");

			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetElementType ();

		public override Type UnderlyingSystemType {
			get {
				// This has _nothing_ to do with getting the base type of an enum etc.
				return this;
			}
		}

		public extern override Assembly Assembly {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override string AssemblyQualifiedName {
			get {
				return getFullName (true, true);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern string getFullName(bool full_name, bool assembly_qualified);

		public extern override Type BaseType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override string FullName {
			get {
				string fullName;
				// This doesn't need locking
				if (type_info == null)
					type_info = new MonoTypeInfo ();
				if ((fullName = type_info.full_name) == null)
					fullName = type_info.full_name = getFullName (true, false);

				return fullName;
			}
		}

		public override Guid GUID {
			get {
				object[] att = GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), true);
				if (att.Length == 0)
					return Guid.Empty;
				return new Guid(((System.Runtime.InteropServices.GuidAttribute)att[0]).Value);
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override MemberTypes MemberType {
			get {
				if (DeclaringType != null && !IsGenericParameter)
					return MemberTypes.NestedType;
				else
					return MemberTypes.TypeInfo;
			}
		}

		public extern override string Name {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override string Namespace {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override Module Module {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override Type DeclaringType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override Type ReflectedType {
			get {
				return DeclaringType;
			}
		}

		public override RuntimeTypeHandle TypeHandle {
			get {
				return _impl;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override int GetArrayRank ();

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			UnitySerializationHolder.GetUnitySerializationInfo(info, this);
		}

		public override string ToString()
		{
			return getFullName (false, false);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type [] GetGenericArguments ();

		public override bool ContainsGenericParameters {
			get {
				if (IsGenericParameter)
					return true;

				if (IsGenericType) {
					foreach (Type arg in GetGenericArguments ())
						if (arg.ContainsGenericParameters)
							return true;
				}

				if (HasElementType)
					return GetElementType ().ContainsGenericParameters;

				return false;
			}
		}

		public extern override bool IsGenericParameter {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override MethodBase DeclaringMethod {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override Type GetGenericTypeDefinition () {
			Type res = GetGenericTypeDefinition_impl ();
			if (res == null)
				throw new InvalidOperationException ();

			return res;
		}

		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}


		public override Array GetEnumValues () {
			if (!IsEnum)
				throw new ArgumentException ("Type is not an enumeration", "enumType");

			return Enum.GetValues (this);
		}

		static MethodBase CheckMethodSecurity (MethodBase mb)
		{
#if NET_2_1
			return mb;
#else
			if (!SecurityManager.SecurityEnabled || (mb == null))
				return mb;

			// Sadly we have no way to know which kind of security action this is
			// so we must do it the hard way. Actually this isn't so bad 
			// because we can skip the (mb.Attributes & MethodAttributes.HasSecurity)
			// icall required (and do it ourselves)

			// this (unlike the Invoke step) is _and stays_ a LinkDemand (caller)
			return SecurityManager.ReflectedLinkDemandQuery (mb) ? mb : null;
#endif
		}

		//seclevel { transparent = 0, safe-critical = 1, critical = 2}
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int get_core_clr_security_level ();

		public override bool IsSecurityTransparent
		{
			get { return get_core_clr_security_level () == 0; }
		}

		public override bool IsSecurityCritical
		{
			get { return get_core_clr_security_level () > 0; }
		}

		public override bool IsSecuritySafeCritical
		{
			get { return get_core_clr_security_level () == 1; }
		}

		public override StructLayoutAttribute StructLayoutAttribute {
			get {
				return GetStructLayoutAttribute ();
			}
		}

		internal override bool IsUserType {
			get {
				return false;
			}
		}

		public override bool IsConstructedGenericType {
			get {
				return IsGenericType && !ContainsGenericParameters;
			}
		}
	}
}
