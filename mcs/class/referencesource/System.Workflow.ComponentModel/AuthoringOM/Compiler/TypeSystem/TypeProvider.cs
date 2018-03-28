#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Resources;
    using System.Reflection;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class TypeProvider : ITypeProvider, IServiceProvider, IDisposable
    {
        internal static readonly char[] nameSeparators = new char[] { '.', '+' };

        private IServiceProvider serviceProvider = null;

        private Hashtable designTimeTypes = new Hashtable();
        private Hashtable assemblyLoaders = new Hashtable();
        private Hashtable rawAssemblyLoaders = new Hashtable();
        private Hashtable compileUnitLoaders = new Hashtable();
        private Hashtable hashOfRTTypes = new Hashtable();
        private Hashtable hashOfDTTypes = new Hashtable();

        // these variables will cache all the information which is passed to 
        private List<string> addedAssemblies = null;
        private List<CodeCompileUnit> addedCompileUnits = null;
        private Dictionary<CodeCompileUnit, EventHandler> needRefreshCompileUnits = null;
        private bool executingEnsureCurrentTypes = false;
        private Hashtable typeLoadErrors = new Hashtable();
        private Assembly localAssembly = null;

        public TypeProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #region Public methods

        public void SetLocalAssembly(Assembly assembly)
        {
            this.localAssembly = assembly;
            if (this.TypesChanged != null)
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
        }

        public void AddAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            if (!rawAssemblyLoaders.Contains(assembly))
            {
                try
                {
                    rawAssemblyLoaders[assembly] = new AssemblyLoader(this, assembly, this.localAssembly == assembly);
                    if (this.TypesChanged != null)
                        FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
                }
                catch (Exception e)
                {
                    this.typeLoadErrors[assembly.FullName] = e;
                    if (this.TypeLoadErrorsChanged != null)
                        FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
                }
            }
        }
        public void RemoveAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            AssemblyLoader assemblyLoader = (AssemblyLoader)this.rawAssemblyLoaders[assembly];
            if (assemblyLoader != null)
            {
                this.rawAssemblyLoaders.Remove(assembly);
                RemoveCachedAssemblyWrappedTypes(assembly);

                if (this.TypesChanged != null)
                    FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
            }
        }

        public void AddAssemblyReference(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (File.Exists(path) &&
                !this.assemblyLoaders.ContainsKey(path) &&
                (this.addedAssemblies == null || !this.addedAssemblies.Contains(path)))
            {
                // lets put these changes into our cache
                if (this.addedAssemblies == null)
                    this.addedAssemblies = new List<string>();
                this.addedAssemblies.Add(path);

                if (this.TypesChanged != null)
                    FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
            }
        }
        public void RemoveAssemblyReference(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            AssemblyLoader assemblyLoader = this.assemblyLoaders[path] as AssemblyLoader;
            if (assemblyLoader != null)
            {
                this.assemblyLoaders.Remove(path);
                RemoveCachedAssemblyWrappedTypes(assemblyLoader.Assembly);
            }

            if (this.addedAssemblies != null && this.addedAssemblies.Contains(path))
                this.addedAssemblies.Remove(path);

            if (this.typeLoadErrors.ContainsKey(path))
            {
                this.typeLoadErrors.Remove(path);
                if (this.TypeLoadErrorsChanged != null)
                    FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
            }

            if (this.TypesChanged != null)
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
        }
        public void AddCodeCompileUnit(CodeCompileUnit codeCompileUnit)
        {
            if (codeCompileUnit == null)
                throw new ArgumentNullException("codeCompileUnit");

            if (this.compileUnitLoaders.ContainsKey(codeCompileUnit) || (this.addedCompileUnits != null && this.addedCompileUnits.Contains(codeCompileUnit)))
                throw new ArgumentException(TypeSystemSR.GetString("Error_DuplicateCodeCompileUnit"), "codeCompileUnit");

            // lets put these changes into our cache
            if (this.addedCompileUnits == null)
                this.addedCompileUnits = new List<CodeCompileUnit>();
            this.addedCompileUnits.Add(codeCompileUnit);
            if (this.needRefreshCompileUnits != null && this.needRefreshCompileUnits.ContainsKey(codeCompileUnit))
                this.needRefreshCompileUnits.Remove(codeCompileUnit);

            if (this.TypesChanged != null)
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
        }
        public void RemoveCodeCompileUnit(CodeCompileUnit codeCompileUnit)
        {
            if (codeCompileUnit == null)
                throw new ArgumentNullException("codeCompileUnit");

            // lets put these changes into our cache
            CodeDomLoader codeDomLoader = this.compileUnitLoaders[codeCompileUnit] as CodeDomLoader;
            if (codeDomLoader != null)
            {
                codeDomLoader.Dispose();
                this.compileUnitLoaders.Remove(codeCompileUnit);
            }

            if (this.addedCompileUnits != null && this.addedCompileUnits.Contains(codeCompileUnit))
                this.addedCompileUnits.Remove(codeCompileUnit);
            if (this.needRefreshCompileUnits != null && this.needRefreshCompileUnits.ContainsKey(codeCompileUnit))
                this.needRefreshCompileUnits.Remove(codeCompileUnit);

            if (this.typeLoadErrors.ContainsKey(codeCompileUnit))
            {
                this.typeLoadErrors.Remove(codeCompileUnit);
                if (this.TypeLoadErrorsChanged != null)
                    FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
            }

            if (this.TypesChanged != null)
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
        }
        public void RefreshCodeCompileUnit(CodeCompileUnit codeCompileUnit, EventHandler refresher)
        {
            if (codeCompileUnit == null)
                throw new ArgumentNullException("codeCompileUnit");

            if (!this.compileUnitLoaders.Contains(codeCompileUnit) && (this.addedCompileUnits != null && !this.addedCompileUnits.Contains(codeCompileUnit)))
                throw new ArgumentException(TypeSystemSR.GetString("Error_NoCodeCompileUnit"), "codeCompileUnit");

            if (this.needRefreshCompileUnits == null)
                this.needRefreshCompileUnits = new Dictionary<CodeCompileUnit, EventHandler>();
            this.needRefreshCompileUnits[codeCompileUnit] = refresher;

            if (this.TypesChanged != null)
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
        }
        #endregion

        #region TargetFrameworkProvider Support
        //
        // In general this func can be used by anyone that wants to configure TypeProvder
        // to do non standard type to assembly name mapping
        // Specifically this func is set by Microsoft.Workflow.VSDesigner when running within VS
        // The func encapsulates VS multi-targeting functionality so that System.Workflow.ComponentModel
        // does not need to take a dependency on VS bits.
        public Func<Type, string> AssemblyNameResolver
        {
            get;
            set;
        }
        public Func<PropertyInfo, object, bool> IsSupportedPropertyResolver
        {
            get;
            set;
        }
        //
        // VS multi-targeting uses LMR which, unlike reflection, does not cache
        // Caching in the caller (here) is critical for performance
        // GetAssemblyName, IsSupportedProperty provide both default behavior 
        // if *Resolver is null and a cache over the LMR methods behind the Resolvers
        // Caches rely on a single Type universe and object equality however due to issues in reflection it is 
        // possible to get redundant items in the cache.  This is because reflection may return different instances
        // of PropertyInfo depending on what API is called.  This is rare but it can happen.  Worst case is
        // redundant entries in the cache; this will not cause incorrect behavior.
        Dictionary<Type, string> typeToAssemblyName = null;
        Dictionary<PropertyInfo, bool> supportedProperties = null;

        public string GetAssemblyName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (typeToAssemblyName == null)
            {
                typeToAssemblyName = new Dictionary<Type, string>();
            }

            string assemblyName = null;
            if (!typeToAssemblyName.TryGetValue(type, out assemblyName))
            {
                //
                // DesignTimeType will not have an assembly
                if (type.Assembly != null)
                {
                    if (this.AssemblyNameResolver != null)
                    {
                        assemblyName = this.AssemblyNameResolver(type);
                    }
                    else
                    {
                        assemblyName = type.Assembly.FullName;
                    }
                    typeToAssemblyName.Add(type, assemblyName);
                }
            }

            if (assemblyName == null)
            {
                assemblyName = string.Empty;
            }

            return assemblyName;
        }

        public bool IsSupportedProperty(PropertyInfo property, object declaringInstance)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            if (declaringInstance == null)
            {
                throw new ArgumentNullException("declaringInstance");
            }
            //
            // If we don't have a resolver to determine if a property is supported
            // just return true
            if (IsSupportedPropertyResolver == null)
            {
                return true;
            }

            if (supportedProperties == null)
            {
                supportedProperties = new Dictionary<PropertyInfo, bool>();
            }

            bool supported = false;
            if (!supportedProperties.TryGetValue(property, out supported))
            {
                supported = IsSupportedPropertyResolver(property, declaringInstance);
                supportedProperties.Add(property, supported);
            }

            return supported;
        }

        #endregion

        #region ITypeProvider Members

        public Type GetType(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return GetType(name, false);
        }

        public Type GetType(string name, bool throwOnError)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            EnsureCurrentTypes();

            bool hasTypeLoadErrors = false;
            Type returnType = null;
            string typeName = string.Empty;
            string[] parameters = null;
            string elementDecorator = string.Empty;

            if (ParseHelpers.ParseTypeName(name, ParseHelpers.ParseTypeNameLanguage.NetFramework, out typeName, out parameters, out elementDecorator))
            {
                if ((parameters != null) && (parameters.Length > 0))
                {
                    //Generic type
                    Type templateType = GetType(typeName, throwOnError);
                    if ((templateType == null) || (!templateType.IsGenericTypeDefinition))
                        return null;
                    Type[] templateParamTypes = new Type[parameters.Length];
                    for (int index = 0; index < parameters.Length; index++)
                    {
                        Type templateParameter = GetType(parameters[index], throwOnError);
                        if (templateParameter == null)
                            return null;
                        templateParamTypes[index] = templateParameter;
                    }
                    return templateType.MakeGenericType(templateParamTypes);
                }
                else if (elementDecorator != string.Empty)
                {
                    //type with element (Array, ByRef, Pointer)
                    Type elementType = this.GetType(typeName);
                    if (elementType != null)
                    {
                        // first we verify the name is formated well (AssemblyQualifiedName for generic
                        // parameters + no spaces in array brackets)
                        System.Text.StringBuilder nameBuilder = new System.Text.StringBuilder(elementType.FullName);
                        for (int loop = 0; loop < elementDecorator.Length; loop++)
                            if (elementDecorator[loop] != ' ')
                                nameBuilder.Append(elementDecorator[loop]);

                        name = nameBuilder.ToString();

                        // let tha assembly of the element type a chance to find a type (will fail only
                        // if element contains parameter from external assembly
                        if (elementType.Assembly != null)
                            returnType = elementType.Assembly.GetType(name, false);

                        if (returnType == null)
                        {
                            // now we can fetch or create the type
                            if (this.hashOfDTTypes.Contains(name))
                            {
                                returnType = this.hashOfDTTypes[name] as Type;
                            }
                            else
                            {
                                returnType = new DesignTimeType(null, name, this);
                                this.hashOfDTTypes.Add(name, returnType);
                            }
                            return returnType;
                        }
                    }
                }
                else
                {
                    // regular type, get the type name
                    string assemblyName = string.Empty;
                    int indexOfComma = name.IndexOf(',');
                    if (indexOfComma != -1)
                    {
                        typeName = name.Substring(0, indexOfComma);
                        assemblyName = name.Substring(indexOfComma + 1).Trim();
                    }
                    typeName = typeName.Trim();
                    if (typeName.Length > 0)
                    {
                        returnType = this.designTimeTypes[typeName] as Type;
                        if (returnType == null)
                        {
                            foreach (DictionaryEntry dictionaryEntry in this.rawAssemblyLoaders)
                            {
                                AssemblyLoader assemblyLoader = dictionaryEntry.Value as AssemblyLoader;
                                if ((assemblyName.Length == 0) || (ParseHelpers.AssemblyNameEquals(assemblyLoader.AssemblyName, assemblyName)))
                                {
                                    try
                                    {
                                        returnType = assemblyLoader.GetType(typeName);
                                    }
                                    catch (Exception e)
                                    {
                                        if (!this.typeLoadErrors.Contains(dictionaryEntry.Key))
                                        {
                                            this.typeLoadErrors[dictionaryEntry.Key] = e;
                                            hasTypeLoadErrors = true;
                                        }
                                        // bubble up exceptions only when appropiate 
                                        if (throwOnError)
                                            throw e;
                                    }
                                    if (returnType != null)
                                        break;
                                }
                            }
                        }

                        if (returnType == null)
                        {
                            foreach (DictionaryEntry dictionaryEntry in this.assemblyLoaders)
                            {
                                AssemblyLoader assemblyLoader = dictionaryEntry.Value as AssemblyLoader;
                                if ((assemblyName.Length == 0) || (ParseHelpers.AssemblyNameEquals(assemblyLoader.AssemblyName, assemblyName)))
                                {
                                    try
                                    {
                                        returnType = assemblyLoader.GetType(typeName);
                                    }
                                    catch (Exception e)
                                    {
                                        if (!this.typeLoadErrors.Contains(dictionaryEntry.Key))
                                        {
                                            this.typeLoadErrors[dictionaryEntry.Key] = e;
                                            hasTypeLoadErrors = true;
                                        }
                                        // bubble up exceptions only when appropiate 
                                        if (throwOnError)
                                            throw e;
                                    }
                                    if (returnType != null)
                                        break;
                                }
                            }
                        }

                        if (hasTypeLoadErrors)
                        {
                            if (this.TypeLoadErrorsChanged != null)
                                FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
                        }

                        if (returnType == null && this.localAssembly != null && assemblyName == this.localAssembly.FullName)
                            returnType = this.localAssembly.GetType(typeName);
                    }
                }
            }

            if (returnType == null)
            {
                if (throwOnError)
                    throw new Exception(TypeSystemSR.GetString(CultureInfo.CurrentCulture, "Error_TypeResolution", name));
                else
                    return null;
            }

            // replace the System.Type with RTTypeWrapper for generic types.
            // WinOE Bug 16560: The type provider may be used at runtime.  No RTTypeWrapper should ever be returned
            // at runtime.  
            // At design time, we need to wrap all generic types even if the parameter types are not 
            // design time types.  This is because our parsing function creates a base generic type before it binds
            // all the parameters.  The RTTypeWrapper.MakeGenericType override will then take care of binding to 
            // design time types.
            if (this.designTimeTypes != null && this.designTimeTypes.Count > 0 && returnType.Assembly != null && returnType.IsGenericTypeDefinition)
            {
                if (this.hashOfRTTypes.Contains(returnType))
                {
                    returnType = (Type)this.hashOfRTTypes[returnType];
                }
                else
                {
                    Type returnType2 = new RTTypeWrapper(this, returnType);
                    this.hashOfRTTypes.Add(returnType, returnType2);
                    returnType = returnType2;
                }
            }
            return returnType;
        }

        public Type[] GetTypes()
        {
            EnsureCurrentTypes();

            bool hasTypeLoadErrors = false;
            this.typeLoadErrors.Clear(); //clear all old errors

            List<Type> typeList = new List<Type>();

            // Design time types
            foreach (Type type in this.designTimeTypes.Values)
                typeList.Add(type);

            foreach (DictionaryEntry dictionaryEntry in this.assemblyLoaders)
            {
                AssemblyLoader assemblyLoader = dictionaryEntry.Value as AssemblyLoader;
                try
                {
                    typeList.AddRange(assemblyLoader.GetTypes());
                }
                catch (Exception e)
                {
                    ReflectionTypeLoadException typeLoadException = e as ReflectionTypeLoadException;
                    if (typeLoadException != null)
                    {
                        //we should at least add the types that did get loaded
                        foreach (Type type in typeLoadException.Types)
                        {
                            if (type != null)
                                typeList.Add(type);
                        }
                    }

                    //we should have the latest exception for every assembly (user might have copied required dlls over)
                    if (this.typeLoadErrors.Contains(dictionaryEntry.Key))
                        this.typeLoadErrors.Remove(dictionaryEntry.Key);

                    this.typeLoadErrors[dictionaryEntry.Key] = e;
                    hasTypeLoadErrors = true;
                }
            }

            foreach (DictionaryEntry dictionaryEntry in this.rawAssemblyLoaders)
            {
                AssemblyLoader assemblyLoader = dictionaryEntry.Value as AssemblyLoader;
                try
                {
                    typeList.AddRange(assemblyLoader.GetTypes());
                }
                catch (Exception e)
                {
                    ReflectionTypeLoadException typeLoadException = e as ReflectionTypeLoadException;
                    if (typeLoadException != null)
                    {
                        //we should at least add the types that did get loaded
                        foreach (Type type in typeLoadException.Types)
                        {
                            if (type != null)
                                typeList.Add(type);
                        }
                    }
                    //we should have the latest exception for every assembly (user might have copied required dlls over)
                    if (this.typeLoadErrors.Contains(dictionaryEntry.Key))
                        this.typeLoadErrors.Remove(dictionaryEntry.Key);

                    this.typeLoadErrors[dictionaryEntry.Key] = e;
                    hasTypeLoadErrors = true;
                }
            }

            if (hasTypeLoadErrors)
            {
                if (this.TypeLoadErrorsChanged != null)
                    FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
            }

            return typeList.ToArray();
        }

        public IDictionary<object, Exception> TypeLoadErrors
        {
            get
            {
                Dictionary<object, Exception> collection = new Dictionary<object, Exception>();

                foreach (DictionaryEntry entry in this.typeLoadErrors)
                {
                    Exception e = entry.Value as Exception;
                    while (e is TargetInvocationException)
                        e = e.InnerException;

                    if (e != null)
                    {
                        string typeLoadError = null;

                        if (entry.Key is CodeCompileUnit)
                            typeLoadError = TypeSystemSR.GetString("Error_CodeCompileUnitNotLoaded", new object[] { e.Message });
                        else if (entry.Key is String)
                            typeLoadError = TypeSystemSR.GetString("Error_AssemblyRefNotLoaded", new object[] { entry.Key.ToString(), e.Message });

                        //wrap the original exception with a new one with a custom error message
                        if (typeLoadError != null)
                            e = new Exception(typeLoadError, e);

                        collection.Add(entry.Key, e);
                    }
                }

                return collection;
            }
        }

        public Assembly LocalAssembly
        {
            get
            {
                return this.localAssembly;
            }
        }
        public event EventHandler TypeLoadErrorsChanged;
        public event EventHandler TypesChanged;

        public ICollection<Assembly> ReferencedAssemblies
        {
            get
            {
                EnsureCurrentTypes();

                List<Assembly> referencedAssemblies = new List<Assembly>();
                foreach (AssemblyLoader loader in this.assemblyLoaders.Values)
                {
                    if (loader.Assembly != null)
                        referencedAssemblies.Add(loader.Assembly);
                }

                foreach (Assembly assembly in this.rawAssemblyLoaders.Keys)
                    referencedAssemblies.Add(assembly);

                return referencedAssemblies.AsReadOnly();
            }
        }

        #endregion

        #region TypeProvider Static Methods

        public static Type GetEventHandlerType(EventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            MethodInfo m = eventInfo.GetAddMethod(true);
            if (m != null)
            {
                ParameterInfo[] p = m.GetParameters();
                Type del = typeof(Delegate);
                for (int i = 0; i < p.Length; i++)
                {
                    Type c = p[i].ParameterType;
                    if (TypeProvider.IsSubclassOf(c, del))
                        return c;
                }
            }
            return null;
        }

        internal static bool IsRepresentingTheSameType(Type firstType, Type secondType)
        {
            if (firstType == null || secondType == null)
                return false;

            if (firstType == secondType)
                return true;

            if (firstType.FullName != secondType.FullName)
                return false;

            if (firstType.Assembly != secondType.Assembly)
                return false;

            if (firstType.Assembly != null)
                if (firstType.AssemblyQualifiedName != secondType.AssemblyQualifiedName)
                    return false;

            return true;
        }

        internal static bool IsAssignable(Type toType, Type fromType, bool equalBasedOnSameTypeRepresenting)
        {
            if (toType == null || fromType == null)
                return false;

            if (equalBasedOnSameTypeRepresenting)
            {
                if (IsRepresentingTheSameType(fromType, toType))
                    return true;
            }
            else
            {
                if (fromType == toType)
                    return true;
            }

            if (toType.IsGenericTypeDefinition)
                return toType.IsAssignableFrom(fromType);

            // runtime type can never be assigned to design time type
            if (toType.Assembly == null && fromType.Assembly != null)
                return false;
            if (fromType is RTTypeWrapper || fromType is DesignTimeType)
            {
                if (!(toType is RTTypeWrapper) && !(toType is DesignTimeType))
                {
#pragma warning suppress 56506
                    ITypeProvider provider = fromType is RTTypeWrapper ? (fromType as RTTypeWrapper).Provider : (fromType as DesignTimeType).Provider;
                    if (provider != null)
                        toType = provider.GetType(toType.FullName);
                }
            }
            else if (toType is RTTypeWrapper || toType is DesignTimeType)
            {
                if (!(fromType is RTTypeWrapper) && !(fromType is DesignTimeType))
                {
#pragma warning suppress 56506
                    ITypeProvider provider = toType is RTTypeWrapper ? (toType as RTTypeWrapper).Provider : (toType as DesignTimeType).Provider;
                    if (provider != null)
                        fromType = provider.GetType(fromType.FullName);
                }
            }
            else
            {
                return toType.IsAssignableFrom(fromType);
            }

            //We need to check not null as there might be cases in which to and from types may not be found
            if (toType == null || fromType == null)
                return false;

            if (equalBasedOnSameTypeRepresenting)
            {
                if (IsRepresentingTheSameType(fromType, toType))
                    return true;
            }
            else
            {
                if (fromType == toType)
                    return true;
            }

            if (TypeProvider.IsSubclassOf(fromType, toType))
                return true;

            if (toType.IsInterface == false)
                return false;

            Type[] interfaces = fromType.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                // unfortunately, IsSubclassOf does not cover the case when they are the same type.
                if (interfaces[i] == toType)
                    return true;

                if (TypeProvider.IsSubclassOf(interfaces[i], toType))
                    return true;
            }
            return false;
        }
        public static bool IsAssignable(Type toType, Type fromType)
        {
            return IsAssignable(toType, fromType, false);
        }
        public static bool IsSubclassOf(Type subclass, Type superClass)
        {
            if (superClass == subclass)
                return false;

            if (subclass == null || superClass == null)
                return false;

            if (superClass == typeof(object))
                return true; // object is superclass of everything.

            subclass = subclass.BaseType;
            while (subclass != null)
            {
                if (superClass == subclass)
                    return true;

                subclass = subclass.BaseType;
            }
            return false;
        }
        public static bool IsEnum(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return TypeProvider.IsSubclassOf(type, typeof(Enum));
        }
        public static string[] GetEnumNames(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException("enumType");

            if (!TypeProvider.IsSubclassOf(enumType, typeof(Enum)))
                throw new ArgumentException(TypeSystemSR.GetString("Error_TypeIsNotEnum"));

            FieldInfo[] flds = enumType.GetFields();

            List<string> names = new List<String>();
            for (int i = 0; i < flds.Length; i++)
                names.Add(flds[i].Name);

            names.Sort();
            return names.ToArray();


        }
        #endregion

        #region Helper methods

        // This function could be re-entrant, so I have kept the flag to make it non-reentrant
        // I had one call stack in whcih ----eblyLoader.GetTypes() called ServiceProvider.GetService()for which
        // some one did Marshal.GetObjectForIUnknown() which caused the message pump to be executed
        // which caused EnsureCurrentTypes to be called once again.
        private void EnsureCurrentTypes()
        {
            if (this.executingEnsureCurrentTypes)
                return;

            try
            {
                bool hasTypeLoadErrors = false;
                this.executingEnsureCurrentTypes = true;
                if (this.addedAssemblies != null)
                {
                    // cache it to local variable
                    string[] addedAssemblies2 = this.addedAssemblies.ToArray();
                    this.addedAssemblies = null;

                    foreach (string path in addedAssemblies2)
                    {
                        AssemblyLoader loader = null;
                        try
                        {
                            loader = new AssemblyLoader(this, path);
                            this.assemblyLoaders[path] = loader;
                        }
                        catch (Exception e)
                        {
                            this.typeLoadErrors[path] = e;
                            hasTypeLoadErrors = true;
                        }
                    }
                }

                if (this.addedCompileUnits != null)
                {
                    // cache it to local variable
                    CodeCompileUnit[] addedCompileUnits2 = this.addedCompileUnits.ToArray();
                    this.addedCompileUnits = null;

                    foreach (CodeCompileUnit codeCompileUnit in addedCompileUnits2)
                    {
                        CodeDomLoader loader = null;
                        try
                        {
                            loader = new CodeDomLoader(this, codeCompileUnit);
                            this.compileUnitLoaders[codeCompileUnit] = loader;
                        }
                        catch (Exception e)
                        {
                            // this will cause it to remove types
                            if (loader != null)
                                loader.Dispose();

                            this.typeLoadErrors[codeCompileUnit] = e;
                            hasTypeLoadErrors = true;
                        }
                    }
                }

                if (this.needRefreshCompileUnits != null)
                {
                    // cache it to local variable
                    Dictionary<CodeCompileUnit, EventHandler> needRefreshCompileUnits2 = new Dictionary<CodeCompileUnit, EventHandler>();
                    foreach (KeyValuePair<CodeCompileUnit, EventHandler> entry in this.needRefreshCompileUnits)
                        needRefreshCompileUnits2.Add(entry.Key, entry.Value);
                    this.needRefreshCompileUnits = null;

                    foreach (KeyValuePair<CodeCompileUnit, EventHandler> entry in needRefreshCompileUnits2)
                    {
                        CodeDomLoader codeDomLoader = this.compileUnitLoaders[entry.Key] as CodeDomLoader;

                        Debug.Assert(codeDomLoader != null, "How come we don't have CodeDOMLoader for the guy who needs refresh?");
                        if (codeDomLoader != null)
                        {
                            try
                            {
                                codeDomLoader.Refresh(entry.Value);
                            }
                            catch (Exception e)
                            {
                                this.typeLoadErrors[entry.Value] = e;
                                hasTypeLoadErrors = true;
                            }
                        }
                    }
                }

                if (hasTypeLoadErrors)
                {
                    if (this.TypeLoadErrorsChanged != null)
                        FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
                }
            }
            finally
            {
                this.executingEnsureCurrentTypes = false;
            }
        }

        internal void AddType(Type type)
        {
            string typeName = type.FullName;

            if (!this.designTimeTypes.Contains(typeName))
                this.designTimeTypes[typeName] = type;
        }

        internal void RemoveTypes(Type[] types)
        {
            foreach (Type type in types)
            {
                string typeName = type.FullName;
                Debug.Assert(this.designTimeTypes.Contains(typeName), "How come you are removing type which you did not push in.");

                // collect all related cashed types (arrays, ref etc') to be deleted.
                // Note: we gather the names first as types might be dependant on each other.
                StringCollection removedTypeNames = new StringCollection();
                foreach (Type cachedtype in this.hashOfDTTypes.Values)
                {
                    Type elementType = cachedtype;
                    while (elementType != null && elementType.HasElementType)
                        elementType = elementType.GetElementType();

                    if (elementType == type)
                        removedTypeNames.Add(cachedtype.FullName);
                }

                // remove cached types
                foreach (string hashedTypeName in removedTypeNames)
                    this.hashOfDTTypes.Remove(hashedTypeName);

                // remove the type
                this.designTimeTypes.Remove(typeName);
            }
        }

        private static void FireEventsNoThrow(Delegate eventDelegator, object[] args)
        {
            if (eventDelegator != null)
            {
                foreach (Delegate invokee in eventDelegator.GetInvocationList())
                {
                    try
                    {
                        invokee.DynamicInvoke(args);
                    }
                    catch (Exception e)
                    {
                        Debug.Assert(false, "One of the event listener threw an Exception. \n" + e.ToString());
                    }
                }
            }
        }

        private void RemoveCachedAssemblyWrappedTypes(Assembly assembly)
        {
            ArrayList types = new ArrayList(this.hashOfRTTypes.Keys);
            foreach (Type type in types)
            {
                if (type.IsGenericTypeDefinition)
                    ((RTTypeWrapper)this.hashOfRTTypes[type]).OnAssemblyRemoved(assembly);
                if (type.Assembly == assembly)
                    this.hashOfRTTypes.Remove(type);
            }
        }

        #endregion

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            if (this.serviceProvider == null)
                return null;

            return this.serviceProvider.GetService(serviceType);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this.compileUnitLoaders != null)
            {
                foreach (CodeDomLoader codeDomLoader in this.compileUnitLoaders.Values)
                    codeDomLoader.Dispose();
                this.compileUnitLoaders.Clear();
            }

            this.addedAssemblies = null;
            this.addedCompileUnits = null;
            this.needRefreshCompileUnits = null;
        }

        #endregion
    }

    internal class TypeSystemSR
    {
        static TypeSystemSR loader = null;
        ResourceManager resources;
        internal TypeSystemSR()
        {
            resources = new ResourceManager("System.Workflow.ComponentModel.Compiler.StringResources", Assembly.GetExecutingAssembly());
        }
        private static TypeSystemSR GetLoader()
        {
            if (loader == null)
                loader = new TypeSystemSR();

            return loader;
        }
        private static CultureInfo Culture
        {
            get { return null/*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
        }
        internal static string GetString(string name, params object[] args)
        {
            return GetString(TypeSystemSR.Culture, name, args);
        }
        internal static string GetString(CultureInfo culture, string name, params object[] args)
        {
            TypeSystemSR sys = GetLoader();

            if (sys == null)
                return null;

            string res = sys.resources.GetString(name, culture);

            if (args != null && args.Length > 0)
            {
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else
            {
                return res;
            }
        }
        internal static string GetString(string name)
        {
            return GetString(TypeSystemSR.Culture, name);
        }
        internal static string GetString(CultureInfo culture, string name)
        {
            TypeSystemSR sys = GetLoader();

            if (sys == null)
                return null;

            return sys.resources.GetString(name, culture);
        }
    }
}
