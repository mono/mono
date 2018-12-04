// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Windows.Markup;
using System.Xaml;

namespace System.Xaml.Schema
{
    /// <SecurityNote>
    /// This class uses SafeReflectionInvoker to invoke all user-supplied methods
    /// and constructors, including the "add methods" for collections.   Normally
    /// these are merely public methods from standard interfaces like IList, but
    /// they can be spoofed by a derived class that overrides GetAddMethod.
    /// </SecurityNote>
    public class XamlTypeInvoker
    {
        private static XamlTypeInvoker s_Unknown;
        private static object[] s_emptyObjectArray = new object[0];

        private Dictionary<XamlType, MethodInfo> _addMethods;
        internal MethodInfo EnumeratorMethod { get; set; }
        private XamlType _xamlType;

        /// <SecurityNote>
        /// Critical: Used in combination with GetUninitializedObject to ensure that the object
        ///           is initialized.
        ///           Can be used to instantiate object bypassing ctor access checks.
        /// </SecurityNote>
        [SecurityCritical]
        private Action<object> _constructorDelegate;

        /// <SecurityNote>
        /// Critical: Used to determine whether it's safe to instantiate this object via _constructorDelegate,
        ///           and thus bypass security checks.
        /// </SecurityNote>
        [SecurityCritical]
        private ThreeValuedBool _isPublic;

        // vvvvv---- Unused members.  Servicing policy is to retain these anyway.  -----vvvvv
        /// <SecurityNote>
        /// Critical: Used to determine whether we need to demand ReflectionPermission before instantiating this type
        /// </SecurityNote>
        [SecurityCritical]
        private ThreeValuedBool _isInSystemXaml;
        // ^^^^^----- End of unused members.  -----^^^^^

        protected XamlTypeInvoker()
        {
        }

        public XamlTypeInvoker(XamlType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            _xamlType = type;
        }

        public static XamlTypeInvoker UnknownInvoker
        {
            get
            {
                if (s_Unknown == null)
                {
                    s_Unknown = new XamlTypeInvoker();
                }
                return s_Unknown;
            }
        }

        public EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler
        {
            get { return _xamlType != null ? _xamlType.SetMarkupExtensionHandler : null; }
        }

        public EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler
        {
            get { return _xamlType != null ? _xamlType.SetTypeConverterHandler : null; }
        }

        public virtual void AddToCollection(object instance, object item)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            IList list = instance as IList;
            if (list != null)
            {
                list.Add(item);
                return;
            }

            ThrowIfUnknown();
            if (!_xamlType.IsCollection)
            {
                throw new NotSupportedException(SR.Get(SRID.OnlySupportedOnCollections));
            }
            XamlType itemType;
            if (item != null)
            {
                itemType = _xamlType.SchemaContext.GetXamlType(item.GetType());
            }
            else
            {
                itemType = _xamlType.ItemType;
            }
            MethodInfo addMethod = GetAddMethod(itemType);
            if (addMethod == null)
            {
                throw new XamlSchemaException(SR.Get(SRID.NoAddMethodFound, _xamlType, itemType));
            }
            SafeReflectionInvoker.InvokeMethod(addMethod, instance, new object[] { item });
        }

        public virtual void AddToDictionary(object instance, object key, object item)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            IDictionary dictionary = instance as IDictionary;
            if (dictionary != null)
            {
                dictionary.Add(key, item);
                return;
            }

            ThrowIfUnknown();
            if (!_xamlType.IsDictionary)
            {
                throw new NotSupportedException(SR.Get(SRID.OnlySupportedOnDictionaries));
            }
            XamlType itemType;
            if (item != null)
            {
                itemType = _xamlType.SchemaContext.GetXamlType(item.GetType());
            }
            else
            {
                itemType = _xamlType.ItemType;
            }
            MethodInfo addMethod = GetAddMethod(itemType);
            if (addMethod == null)
            {
                throw new XamlSchemaException(SR.Get(SRID.NoAddMethodFound, _xamlType, itemType));
            }
            SafeReflectionInvoker.InvokeMethod(addMethod, instance, new object[] { key, item });
        }

        public virtual object CreateInstance(object[] arguments)
        {
            ThrowIfUnknown();
            if (!_xamlType.UnderlyingType.IsValueType && (arguments == null || arguments.Length == 0))
            {
                object result = DefaultCtorXamlActivator.CreateInstance(this);
                if (result != null)
                {
                    return result;
                }
            }
            return CreateInstanceWithActivator(_xamlType.UnderlyingType, arguments);
        }

        /// <SecurityNote>
        /// Because this is a public virtual method, idempotence cannot be guaranteed.
        /// S.X doesn't use this method at all; but any externals consumers who are doing security checks
        /// based on the returned method should make sure that they are resillient to changing results.
        /// </SecurityNote>
        public virtual MethodInfo GetAddMethod(XamlType contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            if (IsUnknown || _xamlType.ItemType == null)
            {
                return null;
            }

            // Common case is that we match the item type. Short-circuit any additional lookup.
            if (contentType == _xamlType.ItemType ||
                (_xamlType.AllowedContentTypes.Count == 1 && contentType.CanAssignTo(_xamlType.ItemType)))
            {
                return _xamlType.AddMethod;
            }

            // Only collections can have additional content types
            if (!_xamlType.IsCollection)
            {
                return null;
            }

            // Populate the dictionary of all available Add methods
            MethodInfo addMethod;
            if (_addMethods == null)
            {
                Dictionary<XamlType, MethodInfo> addMethods = new Dictionary<XamlType, MethodInfo>();
                addMethods.Add(_xamlType.ItemType, _xamlType.AddMethod);
                foreach (XamlType type in _xamlType.AllowedContentTypes)
                {
                    addMethod = CollectionReflector.GetAddMethod(
                        _xamlType.UnderlyingType, type.UnderlyingType);
                    if (addMethod != null)
                    {
                        addMethods.Add(type, addMethod);
                    }
                }
                _addMethods = addMethods;
            }

            // First try the fast path.  Look for an exact match.
            if (_addMethods.TryGetValue(contentType, out addMethod))
            {
                return addMethod;
            }

            // Next the slow path.  Check each one for is assignable from.
            foreach (KeyValuePair<XamlType, MethodInfo> pair in _addMethods)
            {
                if (contentType.CanAssignTo(pair.Key))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        /// <SecurityNote>
        /// Because this is a public virtual method, idempotence cannot be guaranteed.
        /// S.X doesn't use this method at all; but any externals consumers who are doing security checks
        /// based on the returned method should make sure that they are resillient to changing results.
        /// </SecurityNote>
        public virtual MethodInfo GetEnumeratorMethod()
        {
            return _xamlType.GetEnumeratorMethod;
        }

        public virtual IEnumerator GetItems(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            IEnumerable enumerable = instance as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.GetEnumerator();
            }
            ThrowIfUnknown();
            if (!_xamlType.IsCollection && !_xamlType.IsDictionary)
            {
                throw new NotSupportedException(SR.Get(SRID.OnlySupportedOnCollectionsAndDictionaries));
            }
            MethodInfo getEnumMethod = GetEnumeratorMethod();
            return (IEnumerator)SafeReflectionInvoker.InvokeMethod(getEnumMethod, instance, s_emptyObjectArray);
        }

        // vvvvv---- Unused members.  Servicing policy is to retain these anyway.  -----vvvvv
        /// <SecurityNote>
        /// Critical: Sets critical field _isInSystemXaml
        /// Safe: Gets the result from SafeCritical SafeReflectionInvoker.IsInSystemXaml.
        ///       Uses the type's UnderlyingSystemType, which is what's actually created by Activator.CreateInstance.
        /// </SecurityNote>
        private bool IsInSystemXaml
        {
            [SecuritySafeCritical]
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Retained per servicing policy.")]
            get
            {
                if (_isInSystemXaml == ThreeValuedBool.NotSet)
                {
                    Type type = _xamlType.UnderlyingType.UnderlyingSystemType;
                    bool result = SafeReflectionInvoker.IsInSystemXaml(type);
                    _isInSystemXaml = result ? ThreeValuedBool.True : ThreeValuedBool.False;
                }
                return _isInSystemXaml == ThreeValuedBool.True;
            }
        }
        // ^^^^^----- End of unused members.  -----^^^^^

        /// <SecurityNote>
        /// Critical: Sets critical field _isPublic
        /// Safe: Gets the result from SafeCritical method TypeReflector.IsPublic.
        ///       Uses the type's UnderlyingSystemType, which is what's actually created by Activator.CreateInstance.
        /// </SecurityNote>
        private bool IsPublic
        {
            [SecuritySafeCritical]
            get
            {
                if (_isPublic == ThreeValuedBool.NotSet)
                {
                    Type type = _xamlType.UnderlyingType.UnderlyingSystemType;
                    _isPublic = type.IsVisible ? ThreeValuedBool.True : ThreeValuedBool.False;
                }
                return _isPublic == ThreeValuedBool.True;
            }
        }

        private bool IsUnknown
        {
            get { return _xamlType == null || _xamlType.UnderlyingType == null; }
        }

        /// <SecurityNote>
        /// Critical: See explanation in SafeReflectionInvoker.
        /// Safe: See explanation in SafeReflectionInvoker.
        /// </SecurityNote>
        [SecuritySafeCritical]
        private object CreateInstanceWithActivator(Type type, object[] arguments)
        {
            return SafeReflectionInvoker.CreateInstance(type, arguments);
        }

        private void ThrowIfUnknown()
        {
            if (IsUnknown)
            {
                throw new NotSupportedException(SR.Get(SRID.NotSupportedOnUnknownType));
            }
        }

        private static class DefaultCtorXamlActivator
        {
            private static ThreeValuedBool s_securityFailureWithCtorDelegate;
            private static ConstructorInfo s_actionCtor =
                typeof(Action<object>).GetConstructor(new Type[] { typeof(Object), typeof(IntPtr) });


            public static object CreateInstance(XamlTypeInvoker type)
            {
                if (!EnsureConstructorDelegate(type))
                {
                    return null;
                }
                object inst = CallCtorDelegate(type);
                return inst;
            }

            /// <SecurityNote>
            /// Critical: Calls critical method FormatterServices.GetUninitializedObject
            /// Safe: Never leaks the uninitialized object, always calls constructor first.
            /// </SecurityNote>
#if TARGETTING35SP1
            [SecurityTreatAsSafe, SecurityCritical]
#else
            [SecuritySafeCritical]
#endif
            private static object CallCtorDelegate(XamlTypeInvoker type)
            {
                object inst = FormatterServices.GetUninitializedObject(type._xamlType.UnderlyingType);
                InvokeDelegate(type._constructorDelegate, inst);
                return inst;
            }

            /// <SecurityNote>
            /// Must NOT be critical: we don't want to accidentally satisfy SecurityCritical or
            /// LinkDemand from the target of the invocation.
            /// </SecurityNote>
            private static void InvokeDelegate(Action<object> action, object argument)
            {
                action.Invoke(argument);
            }

            /// <SecurityNote>
            /// Critical: sets critical field XamlType.ConstructorDelegate
            /// Safe: gets the value from reflection. Doesn't set it if it's non-public
            ///       (so it can't be accidentally reused on a partial-trust callstack).
            /// </SecurityNote>
#if TARGETTING35SP1
            [SecurityTreatAsSafe, SecurityCritical]
#else
            [SecuritySafeCritical]
#endif
            // returns true if a delegate is available, false if not
            private static bool EnsureConstructorDelegate(XamlTypeInvoker type)
            {
                if (type._constructorDelegate != null)
                {
                    return true;
                }
                if (!type.IsPublic)
                {
                    return false;
                }
                if (s_securityFailureWithCtorDelegate == ThreeValuedBool.NotSet)
                {
                    s_securityFailureWithCtorDelegate =
#if PARTIALTRUST
                        !AppDomain.CurrentDomain.PermissionSet.IsUnrestricted() ? ThreeValuedBool.True : ThreeValuedBool.False;
#else
                        ThreeValuedBool.False;
#endif
                }
                if (s_securityFailureWithCtorDelegate == ThreeValuedBool.True)
                {
                    return false;
                }

                try
                {
                    Type underlyingType = type._xamlType.UnderlyingType.UnderlyingSystemType;
                    // Look up public ctors only, for equivalence with Activator.CreateInstance
                    ConstructorInfo tConstInfo = underlyingType.GetConstructor(Type.EmptyTypes);
                    if (tConstInfo == null)
                    {
                        // Throwing MissingMethodException for equivalence with Activator.CreateInstance
                        throw new MissingMethodException(SR.Get(SRID.NoDefaultConstructor, underlyingType.FullName));
                    }
                    if ((tConstInfo.IsSecurityCritical && !tConstInfo.IsSecuritySafeCritical) ||
                        (tConstInfo.Attributes & MethodAttributes.HasSecurity) == MethodAttributes.HasSecurity ||
                        (underlyingType.Attributes & TypeAttributes.HasSecurity) == TypeAttributes.HasSecurity)
                    {
                        // We don't want to bypass security checks for a critical or demanding ctor,
                        // so just treat it as if it were non-public
                        type._isPublic = ThreeValuedBool.False;
                        return false;
                    }
                    IntPtr constPtr = tConstInfo.MethodHandle.GetFunctionPointer();
                    // This requires Reflection Permission
                    Action<object> ctorDelegate = ctorDelegate =
                        (Action<object>)s_actionCtor.Invoke(new object[] { null, constPtr });
                    type._constructorDelegate = ctorDelegate;
                    return true;

                }
                catch (SecurityException)
                {
                    s_securityFailureWithCtorDelegate = ThreeValuedBool.True;
                    return false;
                }
            }
        }

        private class UnknownTypeInvoker : XamlTypeInvoker
        {
            public override void AddToCollection(object instance, object item)
            {
                throw new NotSupportedException(SR.Get(SRID.NotSupportedOnUnknownType));
            }

            public override void AddToDictionary(object instance, object key, object item)
            {
                throw new NotSupportedException(SR.Get(SRID.NotSupportedOnUnknownType));
            }

            public override object CreateInstance(object[] arguments)
            {
                throw new NotSupportedException(SR.Get(SRID.NotSupportedOnUnknownType));
            }

            public override IEnumerator GetItems(object instance)
            {
                throw new NotSupportedException(SR.Get(SRID.NotSupportedOnUnknownType));
            }
        }
    }
}
