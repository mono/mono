//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xaml;
    using System.Xml;
    using System.Security;
    using System.Security.Permissions;
    using System.Xaml.Permissions;
    using System.Activities.Expressions;
    using System.Activities.Validation;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    public static class ActivityXamlServices
    {
        static readonly XamlSchemaContext dynamicActivityReaderSchemaContext = new DynamicActivityReaderSchemaContext();

        public static Activity Load(Stream stream)
        {
            if (stream == null)
            {
                throw FxTrace.Exception.ArgumentNull("stream");
            }

            return Load(stream, new ActivityXamlServicesSettings());
        }

        public static Activity Load(Stream stream, ActivityXamlServicesSettings settings)
        {
            if (stream == null)
            {
                throw FxTrace.Exception.ArgumentNull("stream");
            }

            if (settings == null)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            using (XmlReader xmlReader = XmlReader.Create(stream))
            {
                return Load(xmlReader, settings);
            }
        }

        public static Activity Load(string fileName)
        {
            if (fileName == null)
            {
                throw FxTrace.Exception.ArgumentNull("fileName");
            }

            return Load(fileName, new ActivityXamlServicesSettings());
        }
        
        public static Activity Load(string fileName, ActivityXamlServicesSettings settings)
        {
            if (fileName == null)
            {
                throw FxTrace.Exception.ArgumentNull("fileName");
            }

            if (settings == null)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            using (XmlReader xmlReader = XmlReader.Create(fileName))
            {
                return Load(xmlReader, settings);
            }
        }

        public static Activity Load(TextReader textReader)
        {
            if (textReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("textReader");
            }

            return Load(textReader, new ActivityXamlServicesSettings());
        }

        public static Activity Load(TextReader textReader, ActivityXamlServicesSettings settings)
        {
            if (textReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("textReader");
            }

            if (settings == null)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            using (XmlReader xmlReader = XmlReader.Create(textReader))
            {
                return Load(xmlReader, settings);
            }
        }

        public static Activity Load(XmlReader xmlReader)
        {
            if (xmlReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("xmlReader");
            }

            return Load(xmlReader, new ActivityXamlServicesSettings());
        }

        public static Activity Load(XmlReader xmlReader, ActivityXamlServicesSettings settings)
        {
            if (xmlReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("xmlReader");
            }
            
            if (settings == null)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            using (XamlXmlReader xamlReader = new XamlXmlReader(xmlReader, dynamicActivityReaderSchemaContext))
            {
                return Load(xamlReader, settings);
            }
        }

        public static Activity Load(XamlReader xamlReader)
        {
            if (xamlReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("xamlReader");
            }

            return Load(xamlReader, new ActivityXamlServicesSettings());
        }

        public static Activity Load(XamlReader xamlReader, ActivityXamlServicesSettings settings)
        {
            if (xamlReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("xamlReader");
            }

            if (settings == null)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            DynamicActivityXamlReader dynamicActivityReader = new DynamicActivityXamlReader(xamlReader);
            object xamlObject = XamlServices.Load(dynamicActivityReader);
            Activity result = xamlObject as Activity;
            if (result == null)
            {
                throw FxTrace.Exception.Argument("reader", SR.ActivityXamlServicesRequiresActivity(
                    xamlObject != null ? xamlObject.GetType().FullName : string.Empty));
            }

            IDynamicActivity dynamicActivity = result as IDynamicActivity;
            if (dynamicActivity != null && settings.CompileExpressions)
            {
                Compile(dynamicActivity, settings.LocationReferenceEnvironment);
            }

            return result;
        }

        public static XamlReader CreateReader(Stream stream)
        {
            if (stream == null)
            {
                throw FxTrace.Exception.ArgumentNull("stream");
            }

            return CreateReader(new XamlXmlReader(XmlReader.Create(stream), dynamicActivityReaderSchemaContext), dynamicActivityReaderSchemaContext);
        }

        public static XamlReader CreateReader(XamlReader innerReader)
        {
            if (innerReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerReader");
            }

            return new DynamicActivityXamlReader(innerReader);
        }

        public static XamlReader CreateReader(XamlReader innerReader, XamlSchemaContext schemaContext)
        {
            if (innerReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerReader");
            }

            if (schemaContext == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaContext");
            }

            return new DynamicActivityXamlReader(innerReader, schemaContext);
        }

        public static XamlReader CreateBuilderReader(XamlReader innerReader)
        {
            if (innerReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerReader");
            }

            return new DynamicActivityXamlReader(true, innerReader, null);
        }

        public static XamlReader CreateBuilderReader(XamlReader innerReader, XamlSchemaContext schemaContext)
        {
            if (innerReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerReader");
            }

            if (schemaContext == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaContext");
            }

            return new DynamicActivityXamlReader(true, innerReader, schemaContext);
        }

        public static XamlWriter CreateBuilderWriter(XamlWriter innerWriter)
        {
            if (innerWriter == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerWriter");
            }

            return new ActivityBuilderXamlWriter(innerWriter);
        }

        public static Func<object> CreateFactory(XamlReader reader, Type resultType)
        {
            if (reader == null)
            {
                throw FxTrace.Exception.ArgumentNull("reader");
            }
            if (resultType == null)
            {
                throw FxTrace.Exception.ArgumentNull("resultType");
            }
            return FuncFactory.CreateFunc(reader, resultType);
        }

        public static Func<T> CreateFactory<T>(XamlReader reader) where T : class
        {
            if (reader == null)
            {
                throw FxTrace.Exception.ArgumentNull("reader");
            }
            return FuncFactory.CreateFunc<T>(reader);
        }

        static void Compile(IDynamicActivity dynamicActivity, LocationReferenceEnvironment environment)
        {
            string language = null;
            if (RequiresCompilation(dynamicActivity, environment, out language))
            {
                TextExpressionCompiler compiler = new TextExpressionCompiler(GetCompilerSettings(dynamicActivity, language));
                TextExpressionCompilerResults results = compiler.Compile();

                if (results.HasErrors)
                {
                    StringBuilder messages = new StringBuilder();
                    messages.Append("\r\n");
                    messages.Append("\r\n");

                    foreach (TextExpressionCompilerError message in results.CompilerMessages)
                    {
                        messages.Append("\t");
                        if (results.HasSourceInfo)
                        {
                            messages.Append(string.Concat(" ", SR.ActivityXamlServiceLineString, " ", message.SourceLineNumber, ": "));
                        }
                        messages.Append(message.Message);

                    }

                    messages.Append("\r\n");
                    messages.Append("\r\n");

                    InvalidOperationException exception = new InvalidOperationException(SR.ActivityXamlServicesCompilationFailed(messages.ToString()));

                    foreach (TextExpressionCompilerError message in results.CompilerMessages)
                    {
                        exception.Data.Add(message, message.Message);
                    }
                    throw FxTrace.Exception.AsError(exception);
                }

                Type compiledExpressionRootType = results.ResultType;

                ICompiledExpressionRoot compiledExpressionRoot = Activator.CreateInstance(compiledExpressionRootType, new object[] { dynamicActivity }) as ICompiledExpressionRoot;
                CompiledExpressionInvoker.SetCompiledExpressionRootForImplementation(dynamicActivity, compiledExpressionRoot);
            }
        }

        static bool RequiresCompilation(IDynamicActivity dynamicActivity, LocationReferenceEnvironment environment, out string language)
        {
            language = null;

            if (!((Activity)dynamicActivity).IsMetadataCached)
            {
                IList<ValidationError> validationErrors = null;
                if (environment == null)
                {
                    environment = new ActivityLocationReferenceEnvironment();
                }

                try
                {
                    ActivityUtilities.CacheRootMetadata((Activity)dynamicActivity, environment, ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CompiledExpressionsCacheMetadataException(dynamicActivity.Name, e.ToString())));
                }

            }

            DynamicActivityVisitor vistor = new DynamicActivityVisitor();
            vistor.Visit((Activity)dynamicActivity, true);

            if (!vistor.RequiresCompilation)
            {
                return false;
            }
            if (vistor.HasLanguageConflict)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DynamicActivityMultipleExpressionLanguages(vistor.GetConflictingLanguages().AsCommaSeparatedValues())));
            }
            language = vistor.Language;
            return true;
        }

        static TextExpressionCompilerSettings GetCompilerSettings(IDynamicActivity dynamicActivity, string language)
        {
            int lastIndexOfDot = dynamicActivity.Name.LastIndexOf('.');
            int lengthOfName = dynamicActivity.Name.Length;

            string activityName = lastIndexOfDot > 0 ? dynamicActivity.Name.Substring(lastIndexOfDot + 1) : dynamicActivity.Name;
            activityName += "_CompiledExpressionRoot";
            string activityNamespace = lastIndexOfDot > 0 ? dynamicActivity.Name.Substring(0, lastIndexOfDot) : null;

            return new TextExpressionCompilerSettings()
            {
                Activity = (Activity)dynamicActivity,
                ActivityName = activityName,
                ActivityNamespace = activityNamespace,
                RootNamespace = null,
                GenerateAsPartialClass = false,
                AlwaysGenerateSource = true,
                Language = language
            };
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we use SecurityCritical methods that do Asserts.",
            Safe = "Safe because no critical resources are leaked. And we guarantee that the XAML we are accessing is coming from the assembly to which we are asserting access.")]
        [SecuritySafeCritical]
        public static void InitializeComponent(
            Type componentType,
            Object componentInstance
        )
        {
            if (componentType == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("componentType"));
            }

            if (componentInstance == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("componentInstance"));
            }

            Assembly typesAssembly = componentType.Assembly;

            // Get the set of resources from the type's assembly.
            string typeName = componentType.Name;
            string typeNamespace = componentType.Namespace;
            string[] resources = typesAssembly.GetManifestResourceNames();

            // Look for the special resource that is generated by the BeforeInitializeComponentExtension.
            string beforeInitializeResourceName;
            if (string.IsNullOrWhiteSpace(typeNamespace))
            {
                beforeInitializeResourceName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}.{2}", typeName, "BeforeInitializeComponentHelper", "txt");
            }
            else
            {
                beforeInitializeResourceName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}.{3}", typeNamespace, typeName, "BeforeInitializeComponentHelper", "txt");
            }

            string beforeInitializeResource = FindResource(resources, beforeInitializeResourceName);
            if (beforeInitializeResource == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BeforeInitializeComponentXBTExtensionResourceNotFound));
            }

            // Get the name of the XAML resource from the BeforeInitializeComponentHelper resource.
            string xamlResourceName = null;
            string helperClassName = null;
            GetContentsOfBeforeInitializeExtensionResource(typesAssembly, beforeInitializeResource, out xamlResourceName, out helperClassName);

            // Now look for the resource containing the XAML.
            string fullXamlResourceName = FindResource(resources, xamlResourceName);
            if (fullXamlResourceName == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.XamlBuildTaskResourceNotFound(xamlResourceName)));
            }

            // Get the schema context for the type.
            XamlSchemaContext typeSchemaContext = GetXamlSchemaContext(typesAssembly, helperClassName);

            InitializeComponentFromXamlResource(componentType, fullXamlResourceName, componentInstance, typeSchemaContext);
        }

        static string FindResource(string[] resources, string partialResourceName)
        {
            bool foundResourceString = false;
            int resourceIndex;
            for (resourceIndex = 0; (resourceIndex < resources.Length); resourceIndex = (resourceIndex + 1))
            {
                string resource = resources[resourceIndex];
                if ((resource.Contains("." + partialResourceName) || resource.Equals(partialResourceName)))
                {
                    foundResourceString = true;
                    break;
                }
            }
            if (!foundResourceString)
            {
                return null;
            }
            return resources[resourceIndex];
        }

        static void GetContentsOfBeforeInitializeExtensionResource(Assembly assembly, string resource, out string xamlResourceName, out string helperClassName)
        {
            Stream beforeInitializeStream = assembly.GetManifestResourceStream(resource);
            using (StreamReader beforeInitializeReader = new StreamReader(beforeInitializeStream))
            {
                xamlResourceName = beforeInitializeReader.ReadLine();
                helperClassName = beforeInitializeReader.ReadLine();
            }
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts,
            Justification = "The schema context is not critical data because it is exposed through the assembly manifest and we are asserting to go get that data.")]
        [Fx.Tag.SecurityNote(Critical = "Critical because it Asserts ReflectionPermission(MemberAccess) to the calling assembly.")]
        [SecurityCritical]
        static XamlSchemaContext GetXamlSchemaContext(Assembly assembly, string helperClassName)
        {
            XamlSchemaContext typeSchemaContext = null;
            ReflectionPermission reflectionPerm = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
            reflectionPerm.Assert();
            try
            {
                Type schemaContextType = assembly.GetType(helperClassName);
                if (schemaContextType == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.SchemaContextFromBeforeInitializeComponentXBTExtensionNotFound(helperClassName)));
                }

                // The "official" BeforeInitializeComponent XBT Extension will not create a generic type for this helper class.
                // This check is here so that the assembly manifest can't lure us into creating a type with a generic argument from a different assembly.
                if (schemaContextType.IsGenericType || schemaContextType.IsGenericTypeDefinition)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.SchemaContextFromBeforeInitializeComponentXBTExtensionCannotBeGeneric(helperClassName)));
                }

                PropertyInfo schemaContextPropertyInfo = schemaContextType.GetProperty("SchemaContext",
                    BindingFlags.NonPublic | BindingFlags.Static);
                typeSchemaContext = (XamlSchemaContext)schemaContextPropertyInfo.GetValue(null,
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetProperty, null, null, null);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return typeSchemaContext;
        }

        [SuppressMessage(FxCop.Category.Security, "CA2103:ReviewImperativeSecurity",
            Justification = "Passing XamlAccessLevel to XamlLoadPermission is okay.")]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts,
            Justification = "We are asserting to get private access to the componentType only so that we can initialize it.")]
        [Fx.Tag.SecurityNote(Critical = "Critical because it Asserts XamlLoadPermission(XamlAccessLevel.PrivateAccessTo(type).")]
        [SecurityCritical]
        static void InitializeComponentFromXamlResource(Type componentType, string resource, object componentInstance, XamlSchemaContext schemaContext)
        {
            Stream initializeXaml = componentType.Assembly.GetManifestResourceStream(resource);
            XmlReader xmlReader = null;
            XamlReader reader = null;
            XamlObjectWriter objectWriter = null;
            try
            {
                xmlReader = XmlReader.Create(initializeXaml);
                XamlXmlReaderSettings readerSettings = new XamlXmlReaderSettings();
                readerSettings.LocalAssembly = componentType.Assembly;
                readerSettings.AllowProtectedMembersOnRoot = true;
                reader = new XamlXmlReader(xmlReader, schemaContext, readerSettings);
                XamlObjectWriterSettings writerSettings = new XamlObjectWriterSettings();
                writerSettings.RootObjectInstance = componentInstance;
                writerSettings.AccessLevel = XamlAccessLevel.PrivateAccessTo(componentType);
                objectWriter = new XamlObjectWriter(schemaContext, writerSettings);

                // We need the XamlLoadPermission for the assembly we are dealing with.
                XamlLoadPermission perm = new XamlLoadPermission(XamlAccessLevel.PrivateAccessTo(componentType));
                perm.Assert();
                try
                {
                    XamlServices.Transform(reader, objectWriter);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            finally
            {
                if ((xmlReader != null))
                {
                    ((IDisposable)(xmlReader)).Dispose();
                }
                if ((reader != null))
                {
                    ((IDisposable)(reader)).Dispose();
                }
                if ((objectWriter != null))
                {
                    ((IDisposable)(objectWriter)).Dispose();
                }
            }
        }

        class DynamicActivityReaderSchemaContext : XamlSchemaContext
        {
            static bool serviceModelLoaded;

            const string serviceModelDll = "System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            const string serviceModelActivitiesDll = "System.ServiceModel.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            const string serviceModelNamespace = "http://schemas.microsoft.com/netfx/2009/xaml/servicemodel";

            // Eventually this will be unnecessary since XAML team has changed the default behavior
            public DynamicActivityReaderSchemaContext()
                : base(new XamlSchemaContextSettings())
            {
            }

            protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
            {
                XamlType xamlType = base.GetXamlType(xamlNamespace, name, typeArguments);

                if (xamlType == null)
                {
                    if (xamlNamespace == serviceModelNamespace && !serviceModelLoaded)
                    {
                        Assembly.Load(serviceModelDll);
                        Assembly.Load(serviceModelActivitiesDll);
                        serviceModelLoaded = true;
                        xamlType = base.GetXamlType(xamlNamespace, name, typeArguments);
                    }                        
                }
                return xamlType;
            }
        }

        class DynamicActivityVisitor : CompiledExpressionActivityVisitor
        {
            ISet<string> languages;

            public string Language
            {
                get
                {
                    if (this.languages == null || this.languages.Count == 0 || this.languages.Count > 1)
                    {
                        return null;
                    }

                    IEnumerator<string> languagesEnumerator = this.languages.GetEnumerator();

                    if (languagesEnumerator.MoveNext())
                    {
                        return languagesEnumerator.Current;
                    }

                    return null;
                }
            }

            public bool RequiresCompilation
            {
                get;
                private set;
            }

            public bool HasLanguageConflict
            {
                get
                {
                    return this.languages != null && this.languages.Count > 1;
                }
            }

            public IEnumerable<string> GetConflictingLanguages()
            {
                if (this.languages.Count > 1)
                {
                    return this.languages;
                }
                else
                {
                    return null;
                }
            }

            protected override void VisitITextExpression(Activity activity, out bool exit)
            {
                ITextExpression textExpression = activity as ITextExpression;

                if (textExpression != null)
                {
                    if (textExpression.RequiresCompilation)
                    {
                        this.RequiresCompilation = true;

                        if (this.languages == null)
                        {
                            this.languages = new HashSet<string>();
                        }

                        if (!this.languages.Contains(textExpression.Language))
                        {
                            this.languages.Add(textExpression.Language);
                        }
                    }
                }

                base.VisitITextExpression(activity, out exit);
            }
        }
    }
}
