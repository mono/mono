//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Text;
    using System.Activities;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Reflection;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Collections.Generic;
    using Microsoft.VisualBasic.Activities;
    using Microsoft.VisualBasic;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Activities.Debugger;
    using System.IO;
    using System.Activities.Expressions;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Activities.Debugger.Symbol;
    using System.Linq.Expressions;
    using System.Diagnostics;

    public class TextExpressionCompiler
    {
        static string typedDataContextName = "_TypedDataContext";
        static string expressionGetString = "__Expr{0}Get";
        static string expressionSetString = "__Expr{0}Set";
        static string expressionStatementString = "__Expr{0}Statement";
        static string expressionGetTreeString = "__Expr{0}GetTree";
        static string getValueTypeValuesString = "GetValueTypeValues";
        static string setValueTypeValuesString = "SetValueTypeValues";
        static string valueTypeAccessorString = "ValueType_";
        static string forReadOnly = "_ForReadOnly";
        static string xamlIntegrationNamespace = "System.Activities.XamlIntegration";
        static string rootActivityFieldName = "rootActivity";
        static string dataContextActivitiesFieldName = "dataContextActivities";
        static string forImplementationName = "forImplementation";
        static CodeAttributeDeclaration generatedCodeAttribute;
        static CodeAttributeDeclaration browsableCodeAttribute;
        static CodeAttributeDeclaration editorBrowsableCodeAttribute;
        static string csharpLambdaString = "() => ";
        static string vbLambdaString = "Function() ";
        static string locationsOffsetFieldName = "locationsOffset";
        static string expectedLocationsCountFieldName = "expectedLocationsCount";

        Dictionary<int, IList<string>> expressionIdToLocationReferences = new Dictionary<int, IList<string>>();

        string activityFullName;
        int nextContextId;
        bool? isCS = null;
        bool? isVB = null;
        bool generateSource;

        TextExpressionCompilerSettings settings;
        
        List<CompiledExpressionDescriptor> expressionDescriptors;
        Stack<CompiledDataContextDescriptor> compiledDataContexts;

        CodeNamespace codeNamespace;
        CodeTypeDeclaration classDeclaration;
        CodeCompileUnit compileUnit;

        Dictionary<object, SourceLocation> symbols = null;
        string fileName = null;

        // Dictionary of namespace name => [Line#]
        Dictionary<string, int> lineNumbersForNSes;
        Dictionary<string, int> lineNumbersForNSesForImpl;

        public TextExpressionCompiler(TextExpressionCompilerSettings settings)
        {
            if (settings == null)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            if (settings.Activity == null)
            {
                throw FxTrace.Exception.Argument("settings", SR.TextExpressionCompilerActivityRequired);
            }

            if (settings.ActivityName == null)
            {
                throw FxTrace.Exception.Argument("settings", SR.TextExpressionCompilerActivityNameRequired);
            }

            if (settings.Language == null)
            {
                throw FxTrace.Exception.Argument("settings", SR.TextExpressionCompilerLanguageRequired);
            }

            this.expressionDescriptors = new List<CompiledExpressionDescriptor>();
            this.compiledDataContexts = new Stack<CompiledDataContextDescriptor>();
            this.nextContextId = 0;

            this.settings = settings;

            this.activityFullName = activityFullName = GetActivityFullName(settings);

            this.generateSource = this.settings.AlwaysGenerateSource;

            this.lineNumbersForNSes = new Dictionary<string, int>();
            this.lineNumbersForNSesForImpl = new Dictionary<string, int>();
        }        

        bool IsCS
        {
            get
            {
                if (!isCS.HasValue)
                {
                    isCS = TextExpression.LanguagesAreEqual(this.settings.Language, "C#");
                }
                return isCS.Value;
            }
        }

        bool IsVB
        {
            get
            {
                if (!isVB.HasValue)
                {
                    isVB = TextExpression.LanguagesAreEqual(this.settings.Language, "VB");
                }
                return isVB.Value;
            }
        }

        bool InVariableScopeArgument
        {
            get;
            set;
        }

        static CodeAttributeDeclaration GeneratedCodeAttribute
        {
            get
            {
                if (generatedCodeAttribute == null)
                {
                    AssemblyName currentAssemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
                    generatedCodeAttribute = new CodeAttributeDeclaration(
                        new CodeTypeReference(typeof(GeneratedCodeAttribute)),
                        new CodeAttributeArgument(new CodePrimitiveExpression(currentAssemblyName.Name)),
                        new CodeAttributeArgument(new CodePrimitiveExpression(currentAssemblyName.Version.ToString())));
                }

                return generatedCodeAttribute;
            }
        }

        static CodeAttributeDeclaration BrowsableCodeAttribute
        {
            get
            {
                if (browsableCodeAttribute == null)
                {
                   browsableCodeAttribute = new CodeAttributeDeclaration(
                       new CodeTypeReference(typeof(BrowsableAttribute)),
                       new CodeAttributeArgument(new CodePrimitiveExpression(false)));
                }
                return browsableCodeAttribute;
            }
        }

        static CodeAttributeDeclaration EditorBrowsableCodeAttribute
        {
            get
            {
                if (editorBrowsableCodeAttribute == null)
                {
                    editorBrowsableCodeAttribute = new CodeAttributeDeclaration(
                        new CodeTypeReference(typeof(EditorBrowsableAttribute)),
                        new CodeAttributeArgument(new CodeFieldReferenceExpression(
                            new CodeTypeReferenceExpression(
                            new CodeTypeReference(typeof(EditorBrowsableState))), "Never")));
                }
                return editorBrowsableCodeAttribute;
            }
        }

                
        public bool GenerateSource(TextWriter textWriter)
        {
            if (textWriter == null)
            {
                throw FxTrace.Exception.ArgumentNull("textWriter");
            }

            Parse();

            if (this.generateSource)
            {
                WriteCode(textWriter);
                return true;
            }

            return false;
        }

        public TextExpressionCompilerResults Compile()
        {
            Parse();

            if (this.generateSource)
            {
                return CompileInMemory();
            }

            return new TextExpressionCompilerResults();
        }
        
        void Parse()
        {                    
            if (!this.settings.Activity.IsMetadataCached)
            {
                IList<ValidationError> validationErrors = null;
                try
                {
                    ActivityUtilities.CacheRootMetadata(this.settings.Activity, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CompiledExpressionsCacheMetadataException(this.settings.Activity.GetType().AssemblyQualifiedName, e.ToString())));
                }
            }

            // Get the source location for an activity             
            if (this.TryGetSymbols(this.settings.Activity, out this.symbols, out this.fileName))
            {
                // Get line number info for namespaces
                TextExpressionCompilerHelper.GetNamespacesLineInfo(this.fileName, this.lineNumbersForNSes, this.lineNumbersForNSesForImpl);
            }
           
            this.compileUnit = new CodeCompileUnit();
            this.codeNamespace = GenerateCodeNamespace();
            this.classDeclaration = GenerateClass();

            this.codeNamespace.Types.Add(classDeclaration);
            this.compileUnit.Namespaces.Add(this.codeNamespace);

            //
            // Generate data contexts with properties and expression methods
            // Use the shared, public tree walk for expressions routine for consistency.       
            ExpressionCompilerActivityVisitor visitor = new ExpressionCompilerActivityVisitor(this)
            {
                NextExpressionId = 0,
            };

            try
            {
                visitor.Visit(this.settings.Activity, this.settings.ForImplementation);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                //
                // Note that unlike the above where the exception from CacheMetadata is always going to be from the user's code 
                // an exception here is more likely to be from our code and unexpected.  However it could be from user code in some cases.
                // Output a message that attempts to normalize this and presents enough info to the user to determine if they can take action.                
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CompiledExpressionsActivityException(e.GetType().FullName, this.settings.Activity.GetType().AssemblyQualifiedName, e.ToString())));
            }

            if (this.generateSource)
            {
                GenerateInvokeExpressionMethod(true);
                GenerateInvokeExpressionMethod(false);

                GenerateCanExecuteMethod();

                GenerateGetRequiredLocationsMethod();

                GenerateGetExpressionTreeForExpressionMethod();
            }

        }

        void OnRootActivity()
        {
            //
            // Always generate a CDC for the root
            // This will contain expressions for the default value of the root arguments
            // These expressions cannot see other root arguments or variables so they need 
            // to be at the very root, before we add any properties
            PushDataContextDescriptor();
        }

        void OnAfterRootActivity()
        {
            //
            // First pop the root arguments descriptor pushed in OnAfterRootArguments
            PopDataContextDescriptor();
            //
            // If we are walking the implementation there will be a second root context descriptor
            // that holds the member declarations for root arguments.   
            // This isn't generatedwhen walking the public surface
            if (this.settings.ForImplementation)
            {
                PopDataContextDescriptor();
            }
        }
                
        void OnAfterRootArguments(Activity activity)
        {
            //
            // Generate the properties for root arguments in a context below the context
            // that contains the default expressions for the root arguments
            CompiledDataContextDescriptor contextDescriptor = PushDataContextDescriptor();
            if (activity.RuntimeArguments != null && activity.RuntimeArguments.Count > 0)
            {
                //
                // Walk the arguments
                foreach (RuntimeArgument runtimeArgument in activity.RuntimeArguments)
                {
                    if (runtimeArgument.IsBound)
                    {
                        AddMember(runtimeArgument.Name, runtimeArgument.Type, contextDescriptor);
                    }
                }
            }
        }
       
        void OnActivityDelegateScope()
        {
            PushDataContextDescriptor();
        }

        void OnDelegateArgument(RuntimeDelegateArgument delegateArgument)
        {
            AddMember(delegateArgument.BoundArgument.Name, delegateArgument.BoundArgument.Type, this.compiledDataContexts.Peek());
        }

        void OnAfterActivityDelegateScope()
        {
            PopDataContextDescriptor();
        }
        
        void OnVariableScope(Activity activity)
        {
            CompiledDataContextDescriptor contextDescriptor = PushDataContextDescriptor();
            //
            // Generate the variable accessors
            foreach (Variable v in activity.RuntimeVariables)
            {
                AddMember(v.Name, v.Type, contextDescriptor);
            }
        }

        void OnRootImplementationScope(Activity activity, out CompiledDataContextDescriptor rootArgumentAccessorContext)
        {
            Fx.Assert(this.compiledDataContexts.Count == 2, "The stack of data contexts should contain the root argument default expression and accessor contexts");

            rootArgumentAccessorContext = this.compiledDataContexts.Pop();

            if (activity.RuntimeVariables != null && activity.RuntimeVariables.Count > 0)
            {
                this.OnVariableScope(activity);
            }
        }

        void OnAfterRootImplementationScope(Activity activity, CompiledDataContextDescriptor rootArgumentAccessorContext)
        {
            if (activity.RuntimeVariables != null && activity.RuntimeVariables.Count > 0)
            {
                OnAfterVariableScope();
            }

            this.compiledDataContexts.Push(rootArgumentAccessorContext);
        }

        void AddMember(string name, Type type, CompiledDataContextDescriptor contextDescriptor)
        {
            if (IsValidTextIdentifierName(name))
            {
                //
                // These checks will be invariantlowercase if the language is VB
                if (contextDescriptor.Fields.ContainsKey(name) || contextDescriptor.Properties.ContainsKey(name))
                {
                    if (!contextDescriptor.Duplicates.Contains(name))
                    {
                        contextDescriptor.Duplicates.Add(name.ToUpperInvariant());
                    }
                }
                else
                {
                    MemberData memberData = new MemberData();
                    memberData.Type = type;
                    memberData.Name = name;
                    memberData.Index = contextDescriptor.NextMemberIndex;

                    if (type.IsValueType)
                    {
                        contextDescriptor.Fields.Add(name, memberData);
                    }
                    else
                    {
                        contextDescriptor.Properties.Add(name, memberData);
                    }
                }
            }
            //
            // Regardless of whether or not this member name is an invalid, duplicate, or valid identifier
            // always increment the member count so that the indexes we generate always match
            // the list that the runtime gives to the ITextExpression
            // The exception here is if the name is null
            if (name != null)
            {
                contextDescriptor.NextMemberIndex++;
            }
        }

        void GenerateMembers(CompiledDataContextDescriptor descriptor)
        {
            foreach (KeyValuePair<string, MemberData> property in descriptor.Properties)
            {
                GenerateProperty(property.Value, descriptor);
            }

            if (descriptor.Fields.Count > 0)
            {
                foreach (KeyValuePair<string, MemberData> field in descriptor.Fields)
                {
                    GenerateField(field.Value, descriptor);
                }

                CodeMemberMethod getValueTypeValuesMethod = GenerateGetValueTypeValues(descriptor);

                descriptor.CodeTypeDeclaration.Members.Add(getValueTypeValuesMethod);
                descriptor.CodeTypeDeclaration.Members.Add(GenerateSetValueTypeValues(descriptor));

                descriptor.CodeTypeDeclarationForReadOnly.Members.Add(getValueTypeValuesMethod);
            }

            if (descriptor.Duplicates.Count > 0 && this.IsVB)
            {
                foreach (string duplicate in descriptor.Duplicates)
                {
                    AddPropertyForDuplicates(duplicate, descriptor);
                }
            }
        }

        void GenerateField(MemberData memberData, CompiledDataContextDescriptor contextDescriptor)
        {
            if (contextDescriptor.Duplicates.Contains(memberData.Name))
            {
                return;
            }

            CodeMemberField accessorField = new CodeMemberField();
            accessorField.Attributes = MemberAttributes.Family | MemberAttributes.Final;
            accessorField.Name = memberData.Name;
            accessorField.Type = new CodeTypeReference(memberData.Type);

            if (IsRedefinition(memberData.Name))
            {
                accessorField.Attributes |= MemberAttributes.New;
            }

            contextDescriptor.CodeTypeDeclaration.Members.Add(accessorField);

            contextDescriptor.CodeTypeDeclarationForReadOnly.Members.Add(accessorField);
        }

        void GenerateProperty(MemberData memberData, CompiledDataContextDescriptor contextDescriptor)
        {
            if (contextDescriptor.Duplicates.Contains(memberData.Name))
            {
                return;
            }

            bool isRedefinition = IsRedefinition(memberData.Name);

            CodeMemberProperty accessorProperty = GenerateCodeMemberProperty(memberData, isRedefinition);

            //
            // Generate a get accessor that looks like this:
            // return (Foo) this.GetVariableValue(contextId, locationIndexId)
            CodeMethodReturnStatement getterStatement = new CodeMethodReturnStatement(
                new CodeCastExpression(memberData.Type, new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeThisReferenceExpression(),
                        "GetVariableValue"), 
                        new CodeBinaryOperatorExpression(
                        new CodePrimitiveExpression(memberData.Index), 
                        CodeBinaryOperatorType.Add, 
                        new CodeVariableReferenceExpression("locationsOffset")))));

            accessorProperty.GetStatements.Add(getterStatement);
            
            // Generate a set accessor that looks something like this:
            // this.SetVariableValue(contextId, locationIndexId, value)
            accessorProperty.SetStatements.Add(new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeThisReferenceExpression(),
                    "SetVariableValue"),
                     new CodeBinaryOperatorExpression(
                        new CodePrimitiveExpression(memberData.Index),
                        CodeBinaryOperatorType.Add,
                        new CodeVariableReferenceExpression("locationsOffset")),
                    new CodePropertySetValueReferenceExpression()));

            contextDescriptor.CodeTypeDeclaration.Members.Add(accessorProperty);

            //
            // Create another property for the read only class.
            // This will only have a getter so we can't just re-use the property from above
            CodeMemberProperty accessorPropertyForReadOnly = GenerateCodeMemberProperty(memberData, isRedefinition);
            //
            // OK to share the getter statement from above
            accessorPropertyForReadOnly.GetStatements.Add(getterStatement);

            contextDescriptor.CodeTypeDeclarationForReadOnly.Members.Add(accessorPropertyForReadOnly);
        }

        CodeMemberProperty GenerateCodeMemberProperty(MemberData memberData, bool isRedefinition)
        {
            CodeMemberProperty accessorProperty = new CodeMemberProperty();
            accessorProperty.Attributes = MemberAttributes.Family | MemberAttributes.Final;
            accessorProperty.Name = memberData.Name;
            accessorProperty.Type = new CodeTypeReference(memberData.Type);

            if (isRedefinition)
            {
                accessorProperty.Attributes |= MemberAttributes.New;
            }

            return accessorProperty;
        }

        void AddPropertyForDuplicates(string name, CompiledDataContextDescriptor contextDescriptor)
        {
            CodeMemberProperty accessorProperty = new CodeMemberProperty();
            accessorProperty.Attributes = MemberAttributes.Family | MemberAttributes.Final;
            accessorProperty.Name = name;
            accessorProperty.Type = new CodeTypeReference(typeof(object));

            CodeThrowExceptionStatement exception = new CodeThrowExceptionStatement(
                new CodeObjectCreateExpression(typeof(InvalidOperationException), new CodePrimitiveExpression(SR.CompiledExpressionsDuplicateName(name))));

            accessorProperty.GetStatements.Add(exception);
            accessorProperty.SetStatements.Add(exception);

            contextDescriptor.CodeTypeDeclaration.Members.Add(accessorProperty);

            //
            // Create another property for the read only class.
            // This will only have a getter so we can't just re-use the property from above
            CodeMemberProperty accessorPropertyForReadOnly = new CodeMemberProperty();
            accessorPropertyForReadOnly.Attributes = MemberAttributes.Family | MemberAttributes.Final;
            accessorPropertyForReadOnly.Name = name;
            accessorPropertyForReadOnly.Type = new CodeTypeReference(typeof(object));
            //
            // OK to share the exception from above
            accessorPropertyForReadOnly.GetStatements.Add(exception);

            contextDescriptor.CodeTypeDeclarationForReadOnly.Members.Add(accessorPropertyForReadOnly);
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are accessing CodeDom.",
            Safe = "Safe because we are demanding FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        bool IsValidTextIdentifierName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                if (this.settings.LogSourceGenerationMessage != null)
                {
                    this.settings.LogSourceGenerationMessage(SR.CompiledExpressionsIgnoringUnnamedVariable);
                }
                return false;
            }

            if (!CodeDomProvider.CreateProvider(this.settings.Language).IsValidIdentifier(name))
            {
                if (this.settings.LogSourceGenerationMessage != null)
                {
                    this.settings.LogSourceGenerationMessage(SR.CompiledExpressionsIgnoringInvalidIdentifierVariable(name));
                }
                return false;
            }
            
            return true;
        }

        bool IsRedefinition(string variableName)
        {
            if (this.compiledDataContexts == null)
            {
                return false;
            }

            foreach (CompiledDataContextDescriptor contextDescriptor in this.compiledDataContexts)
            {
                foreach (KeyValuePair<string, MemberData> field in contextDescriptor.Fields)
                {
                    if (NamesMatch(variableName, field.Key))
                    {
                        return true;
                    }
                }
                foreach (KeyValuePair<string, MemberData> property in contextDescriptor.Properties)
                {
                    if (NamesMatch(variableName, property.Key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool NamesMatch(string toCheck, string current)
        {
            if (IsVB && string.Compare(toCheck, current, true, CultureInfo.CurrentCulture) == 0)
            {
                return true;
            }
            else if (!IsVB && toCheck == current)
            {
                return true;
            }

            return false;
        }

        void OnAfterVariableScope()
        {
            PopDataContextDescriptor();
        }

        void OnITextExpressionFound(Activity activity, ExpressionCompilerActivityVisitor visitor)
        {
            CompiledDataContextDescriptor contextDescriptor = null;
            CompiledDataContextDescriptor currentContextDescriptor = this.compiledDataContexts.Peek();

            if (this.InVariableScopeArgument)
            {
                //
                // Temporarily popping the stack so don't use PopDataContextDescriptor
                // because that is for when the descriptor is done being built
                this.compiledDataContexts.Pop();
                contextDescriptor = PushDataContextDescriptor();
            }
            else
            {
                contextDescriptor = currentContextDescriptor;
            }
            

            if (TryGenerateExpressionCode(activity, contextDescriptor, visitor.NextExpressionId, this.settings.Language))
            {
                expressionIdToLocationReferences.Add(visitor.NextExpressionId, this.FindLocationReferences(activity));
                visitor.NextExpressionId++;
                this.generateSource = true;
            }

            if (this.InVariableScopeArgument)
            {
                PopDataContextDescriptor();
                this.compiledDataContexts.Push(currentContextDescriptor);
            }
        }

        IList<string> FindLocationReferences(Activity activity)
        {
            ActivityWithResult boundExpression;
            LocationReference locationReference;
            List<string> requiredLocationReferences = new List<string>();

            foreach (RuntimeArgument runtimeArgument in activity.RuntimeArguments)
            {
                boundExpression = runtimeArgument.BoundArgument.Expression;

                if (boundExpression != null && boundExpression is ILocationReferenceWrapper)
                {
                    locationReference = ((ILocationReferenceWrapper)boundExpression).LocationReference;

                    if (locationReference != null)
                    {
                        requiredLocationReferences.Add(locationReference.Name);
                    }
                }
            }
            return requiredLocationReferences;
        }

        CodeTypeDeclaration GenerateClass()
        {
            CodeTypeDeclaration classDeclaration = new CodeTypeDeclaration(this.settings.ActivityName);
            classDeclaration.BaseTypes.Add(new CodeTypeReference(typeof(ICompiledExpressionRoot)));
            classDeclaration.IsPartial = this.settings.GenerateAsPartialClass;

            CodeMemberField compiledRootField = new CodeMemberField(new CodeTypeReference(typeof(Activity)), rootActivityFieldName);
            classDeclaration.Members.Add(compiledRootField);

            CodeMemberMethod languageProperty = new CodeMemberMethod();
            languageProperty.Attributes = MemberAttributes.Final | MemberAttributes.Public;
            languageProperty.Name = "GetLanguage";
            languageProperty.ReturnType = new CodeTypeReference(typeof(string));
            languageProperty.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(this.settings.Language)));
            languageProperty.ImplementationTypes.Add(new CodeTypeReference(typeof(ICompiledExpressionRoot)));            
            languageProperty.CustomAttributes.Add(GeneratedCodeAttribute);
            languageProperty.CustomAttributes.Add(BrowsableCodeAttribute);
            languageProperty.CustomAttributes.Add(EditorBrowsableCodeAttribute);

            classDeclaration.Members.Add(languageProperty);

            CodeMemberField dataContextActivitiesField = new CodeMemberField();
            dataContextActivitiesField.Attributes = MemberAttributes.Private;
            dataContextActivitiesField.Name = dataContextActivitiesFieldName;
            dataContextActivitiesField.Type = new CodeTypeReference(typeof(object));

            classDeclaration.Members.Add(dataContextActivitiesField);

            CodeMemberField forImplementationField = new CodeMemberField();
            forImplementationField.Attributes = MemberAttributes.Private;
            forImplementationField.Name = forImplementationName;
            forImplementationField.Type = new CodeTypeReference(typeof(bool));
            forImplementationField.InitExpression = new CodePrimitiveExpression(this.settings.ForImplementation);

            classDeclaration.Members.Add(forImplementationField);

            if (!this.settings.GenerateAsPartialClass)
            {
                classDeclaration.Members.Add(GenerateCompiledExpressionRootConstructor());
            }

            return classDeclaration;
        }

        CodeConstructor GenerateCompiledExpressionRootConstructor()
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;

            constructor.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    new CodeTypeReference(typeof(Activity)),
                    rootActivityFieldName));

            CodeBinaryOperatorExpression nullArgumentExpression = new CodeBinaryOperatorExpression(
                new CodeVariableReferenceExpression(rootActivityFieldName),
                CodeBinaryOperatorType.IdentityEquality,
                new CodePrimitiveExpression(null));

            CodeConditionStatement nullArgumentCondition = new CodeConditionStatement(
                nullArgumentExpression,
                new CodeThrowExceptionStatement(
                    new CodeObjectCreateExpression(
                        new CodeTypeReference(typeof(ArgumentNullException)),
                        new CodePrimitiveExpression(rootActivityFieldName))));

            constructor.Statements.Add(nullArgumentCondition);

            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        rootActivityFieldName),
                    new CodeVariableReferenceExpression(rootActivityFieldName)));

            return constructor;
        }

        Dictionary<string, int> GetCacheIndicies()
        {
            Dictionary<string, int> contexts = new Dictionary<string, int>();
            int currentIndex = 0;

            foreach (CompiledExpressionDescriptor descriptor in this.expressionDescriptors)
            {
                string name = descriptor.TypeName;
                if (!contexts.ContainsKey(name))
                {
                    contexts.Add(name, currentIndex++);
                }
            }

            return contexts;
        }

        void GenerateGetRequiredLocationsMethod()
        {
            CodeMemberMethod getLocationsMethod = new CodeMemberMethod();
            getLocationsMethod.Name = "GetRequiredLocations";
            getLocationsMethod.Attributes = MemberAttributes.Final | MemberAttributes.Public;
            getLocationsMethod.CustomAttributes.Add(GeneratedCodeAttribute);
            getLocationsMethod.CustomAttributes.Add(BrowsableCodeAttribute);
            getLocationsMethod.CustomAttributes.Add(EditorBrowsableCodeAttribute);
            getLocationsMethod.ImplementationTypes.Add(new CodeTypeReference(typeof(ICompiledExpressionRoot)));

            getLocationsMethod.ReturnType = new CodeTypeReference(typeof(IList<string>));

            getLocationsMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "expressionId"));

            if (this.IsVB)
            {
                GenerateRequiredLocationsBody(getLocationsMethod);
            }
            else
            {
                GenerateEmptyRequiredLocationsBody(getLocationsMethod);
            }

            classDeclaration.Members.Add(getLocationsMethod);
        }

        void GenerateEmptyRequiredLocationsBody(CodeMemberMethod getLocationsMethod)
        {
            getLocationsMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
        }

        void GenerateRequiredLocationsBody(CodeMemberMethod getLocationsMethod)
        {
            CodeVariableDeclarationStatement returnLocationsVar = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(List<string>)),
                "returnLocations",
                new CodeObjectCreateExpression(new CodeTypeReference(typeof(List<string>))));

            getLocationsMethod.Statements.Add(returnLocationsVar);
            foreach (CompiledExpressionDescriptor descriptor in expressionDescriptors)
            {
                IList<string> requiredLocations = null;
                bool found = expressionIdToLocationReferences.TryGetValue(descriptor.Id, out requiredLocations);
                if (!found)
                {
                    return;
                }
                CodeStatement[] conditionStatements = null;
                conditionStatements = GetRequiredLocationsConditionStatements(requiredLocations);

                CodeBinaryOperatorExpression idExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("expressionId"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(descriptor.Id));
                CodeConditionStatement idCondition = new CodeConditionStatement(idExpression, conditionStatements);

                getLocationsMethod.Statements.Add(idCondition);
            }

            getLocationsMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("returnLocations")));
        }

        static CodeStatement[] GetRequiredLocationsConditionStatements(IList<string> requiredLocations)
        {
            CodeStatementCollection statementCollection = new CodeStatementCollection();
            foreach (string locationName in requiredLocations)
            {
                CodeMethodInvokeExpression invokeValidateExpression = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("returnLocations"), "Add"),
                new CodePrimitiveExpression(locationName));
                statementCollection.Add(invokeValidateExpression);
            }

            CodeStatement[] returnStatements = new CodeStatement[statementCollection.Count];
            statementCollection.CopyTo(returnStatements, 0);

            return returnStatements;
        }

        void GenerateGetExpressionTreeForExpressionMethod()
        {
            CodeMemberMethod getExpressionTreeForExpressionMethod = new CodeMemberMethod();
            getExpressionTreeForExpressionMethod.Name = "GetExpressionTreeForExpression";
            getExpressionTreeForExpressionMethod.Attributes = MemberAttributes.Final | MemberAttributes.Public;
            getExpressionTreeForExpressionMethod.ReturnType = new CodeTypeReference(typeof(Expression));
            getExpressionTreeForExpressionMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "expressionId"));
            getExpressionTreeForExpressionMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IList<LocationReference>)), "locationReferences"));
            getExpressionTreeForExpressionMethod.ImplementationTypes.Add(new CodeTypeReference(typeof(ICompiledExpressionRoot)));
            
            // Mark this type as tool generated code
            getExpressionTreeForExpressionMethod.CustomAttributes.Add(GeneratedCodeAttribute);

            // Mark it as Browsable(false) 
            // Note that this does not prevent intellisense within a single project, just at the metadata level
            getExpressionTreeForExpressionMethod.CustomAttributes.Add(BrowsableCodeAttribute);

            // Mark it as EditorBrowsable(EditorBrowsableState.Never)
            // Note that this does not prevent intellisense within a single project, just at the metadata level
            getExpressionTreeForExpressionMethod.CustomAttributes.Add(EditorBrowsableCodeAttribute);

            foreach (CompiledExpressionDescriptor descriptor in expressionDescriptors)
            {
                CodeMethodReturnStatement conditionStatement = new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeObjectCreateExpression(new CodeTypeReference(descriptor.TypeName), new CodeExpression[] { new CodeVariableReferenceExpression("locationReferences") }),
                            descriptor.GetExpressionTreeMethodName)));

                CodeBinaryOperatorExpression idExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("expressionId"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(descriptor.Id));
                CodeConditionStatement idCondition = new CodeConditionStatement(idExpression, conditionStatement);

                getExpressionTreeForExpressionMethod.Statements.Add(idCondition);
            }

            getExpressionTreeForExpressionMethod.Statements.Add(new CodeMethodReturnStatement(
                    new CodePrimitiveExpression(null)));

            classDeclaration.Members.Add(getExpressionTreeForExpressionMethod);
        }

        void GenerateInvokeExpressionMethod(bool withLocationReferences)
        {
            CodeMemberMethod invokeExpressionMethod = new CodeMemberMethod();
            invokeExpressionMethod.Name = "InvokeExpression";
            invokeExpressionMethod.Attributes = MemberAttributes.Final | MemberAttributes.Public;
            invokeExpressionMethod.CustomAttributes.Add(GeneratedCodeAttribute);
            invokeExpressionMethod.CustomAttributes.Add(BrowsableCodeAttribute);
            invokeExpressionMethod.CustomAttributes.Add(EditorBrowsableCodeAttribute);
            invokeExpressionMethod.ImplementationTypes.Add(new CodeTypeReference(typeof(ICompiledExpressionRoot)));

            invokeExpressionMethod.ReturnType = new CodeTypeReference(typeof(object));
            
            invokeExpressionMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "expressionId"));

            if (withLocationReferences)
            {
                invokeExpressionMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IList<LocationReference>)), "locations"));
                invokeExpressionMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ActivityContext)), "activityContext"));
            }
            else
            {
                invokeExpressionMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IList<Location>)), "locations"));
            }

            if (this.settings.GenerateAsPartialClass)
            {
                invokeExpressionMethod.Statements.Add(GenerateInitializeDataContextActivity());
            }

            if (withLocationReferences)
            {
                if (this.expressionDescriptors != null && this.expressionDescriptors.Count > 0)
                {
                    //
                    // We only generate the helper method on the root data context/context 0
                    // No need to have it on all contexts.  This is just a slight of hand
                    // so that we don't need to make GetDataContextActivities public on CompiledDataContext.
                    invokeExpressionMethod.Statements.Add(GenerateDataContextActivitiesCheck(this.expressionDescriptors[0]));
                }
            }

            Dictionary<string, int> cacheIndicies = GetCacheIndicies();
                        
            foreach (CompiledExpressionDescriptor descriptor in expressionDescriptors)
            {
                //
                // if ((expressionId == [descriptor.Id]))
                // {
                //   if (!CheckExpressionText(expressionId, activityContext)
                //   {
                //     throw new Exception();
                //   }
                //   System.Activities.XamlIntegration.CompiledDataContext[] cachedCompiledDataContext = Workflow1_TypedDataContext1_ForReadOnly.GetCompiledDataContextCacheHelper(this, activityContext, 1);
                //   if ((cachedCompiledDataContext[0] == null))
                //   {
                //     cachedCompiledDataContext[0] = new Workflow1_TypedDataContext1_ForReadOnly(locations, activityContext);
                //   }
                //   Workflow1_TypedDataContext1_ForReadOnly valDataContext0 = ((Workflow1_TypedDataContext1_ForReadOnly)(cachedCompiledDataContext[0]));
                //   return valDataContext0.ValueType___Expr0Get();
                // }
                //
                CodeStatement[] conditionStatements = null;
                if (descriptor.IsReference)
                {
                    conditionStatements = GenerateReferenceExpressionInvocation(descriptor, withLocationReferences, cacheIndicies);
                }
                else if (descriptor.IsValue)
                {
                    conditionStatements = GenerateValueExpressionInvocation(descriptor, withLocationReferences, cacheIndicies);
                }
                else if (descriptor.IsStatement)
                {
                    conditionStatements = GenerateStatementInvocation(descriptor, withLocationReferences, cacheIndicies);
                }

                CodeBinaryOperatorExpression idExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("expressionId"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(descriptor.Id));
                CodeConditionStatement idCondition = new CodeConditionStatement(idExpression, conditionStatements);

                invokeExpressionMethod.Statements.Add(idCondition);
            }

            invokeExpressionMethod.Statements.Add(new CodeMethodReturnStatement(
                    new CodePrimitiveExpression(null)));

            classDeclaration.Members.Add(invokeExpressionMethod);
        }

        CodeConditionStatement GenerateDataContextActivitiesCheck(CompiledExpressionDescriptor descriptor)
        {
            CodeBinaryOperatorExpression dataContextActivitiesNullExpression = new CodeBinaryOperatorExpression(
                       new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dataContextActivitiesFieldName),
                       CodeBinaryOperatorType.IdentityEquality,
                       new CodePrimitiveExpression(null));

            CodeConditionStatement dataContextActivitiesNullStatement = new CodeConditionStatement(
                dataContextActivitiesNullExpression,
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dataContextActivitiesFieldName),
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(new CodeTypeReference(descriptor.TypeName)),
                            "GetDataContextActivitiesHelper"),
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            rootActivityFieldName),
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            forImplementationName))));

            return dataContextActivitiesNullStatement;
        }


        CodeStatement GenerateInitializeDataContextActivity()
        {
            //
            // if (this.rootActivity == null)
            // {
            //   this.rootActivity == this;
            // }
            CodeBinaryOperatorExpression dataContextActivityExpression = new CodeBinaryOperatorExpression(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(),
                    rootActivityFieldName),
                CodeBinaryOperatorType.IdentityEquality,
                new CodePrimitiveExpression(null));

            CodeConditionStatement dataContextActivityCheck = new CodeConditionStatement(
                dataContextActivityExpression,
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        rootActivityFieldName),
                    new CodeThisReferenceExpression()));

            return dataContextActivityCheck;
        }

        void GenerateGetDataContextVariable(CompiledExpressionDescriptor descriptor, CodeVariableDeclarationStatement dataContextVariable, CodeStatementCollection statements, bool withLocationReferences, Dictionary<string, int> cacheIndicies)
        {
            CodeObjectCreateExpression dataContext = GenerateDataContextCreateExpression(descriptor.TypeName, withLocationReferences);

            if (withLocationReferences)
            {
                //
                // System.Activities.XamlIntegration.CompiledDataContext[] cachedCompiledDataContext = CompiledExpressions_TypedDataContext2.GetCompiledDataContextCacheHelper(this, activityContext, 2);
                // if ((cachedCompiledDataContext[1] == null))
                // {
                //   if (!CompiledExpressions_TypedDataContext2.Validate(locations, activityContext))
                //   {
                //     return false;
                //   }
                //   cachedCompiledDataContext[1] = new CompiledExpressions_TypedDataContext2(locations, activityContext);
                // }
                //
                CodeVariableDeclarationStatement cachedCompiledDataContextArray = new CodeVariableDeclarationStatement(
                    typeof(CompiledDataContext[]),
                    "cachedCompiledDataContext",
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(descriptor.TypeName),
                            "GetCompiledDataContextCacheHelper"),
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            dataContextActivitiesFieldName),
                        new CodeVariableReferenceExpression("activityContext"),
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            rootActivityFieldName),
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            forImplementationName),
                        new CodePrimitiveExpression(cacheIndicies.Count)));

                CodeIndexerExpression compiledDataContextIndexer = new CodeIndexerExpression(
                    new CodeVariableReferenceExpression("cachedCompiledDataContext"),
                    new CodePrimitiveExpression(cacheIndicies[descriptor.TypeName]));

                //
                // if (cachedCompiledDataContext[index] == null)
                // {
                //     cachedCompiledDataContext[index] = new TCDC(locations, activityContext);
                // }
                //

                CodeBinaryOperatorExpression nullCacheItemExpression = new CodeBinaryOperatorExpression(
                    compiledDataContextIndexer,
                    CodeBinaryOperatorType.IdentityEquality,
                    new CodePrimitiveExpression(null));

                CodeAssignStatement cacheIndexInitializer = new CodeAssignStatement(
                    compiledDataContextIndexer,
                    dataContext);

                CodeConditionStatement conditionStatement = new CodeConditionStatement(
                    nullCacheItemExpression,
                    cacheIndexInitializer);
                       
                //
                // [compiledDataContextVariable] = cachedCompiledDataContext[index]
                //

                dataContextVariable.InitExpression = new CodeCastExpression(descriptor.TypeName, compiledDataContextIndexer);


                statements.Add(cachedCompiledDataContextArray);
                statements.Add(conditionStatement);
            }
            else
            {
                //
                // [compiledDataContextVariable] = new [compiledDataContextType](locations);
                //

                dataContextVariable.InitExpression = dataContext;
            }
        }

        CodeStatement[] GenerateReferenceExpressionInvocation(CompiledExpressionDescriptor descriptor, bool withLocationReferences, Dictionary<string, int> cacheIndicies)
        {
            string indexString = descriptor.Id.ToString(CultureInfo.InvariantCulture);
            string dataContextVariableName = "refDataContext" + indexString;

            CodeVariableDeclarationStatement dataContextVariable = new CodeVariableDeclarationStatement(
                     new CodeTypeReference(descriptor.TypeName), dataContextVariableName);

            CodeStatementCollection compiledDataContextStatements = new CodeStatementCollection();

            GenerateGetDataContextVariable(descriptor, dataContextVariable, compiledDataContextStatements, withLocationReferences, cacheIndicies);
            compiledDataContextStatements.Add(dataContextVariable);

            CodeExpression getExpression = null;
            CodeExpression setExpression = null;

            if (this.IsVB)
            {
                getExpression = new CodeDelegateCreateExpression(
                        new CodeTypeReference(descriptor.TypeName),
                        new CodeVariableReferenceExpression(dataContextVariableName),
                        descriptor.GetMethodName);
                setExpression = new CodeDelegateCreateExpression(
                        new CodeTypeReference(descriptor.TypeName),
                        new CodeVariableReferenceExpression(dataContextVariableName),
                        descriptor.SetMethodName);
            }
            else
            {
                getExpression = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(dataContextVariableName), descriptor.GetMethodName);
                setExpression = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(dataContextVariableName), descriptor.SetMethodName);
            }

            CodeMethodReferenceExpression getLocationMethod = new CodeMethodReferenceExpression(
                new CodeVariableReferenceExpression(dataContextVariableName),
                "GetLocation",
                new CodeTypeReference[] { new CodeTypeReference(descriptor.ResultType) });

            CodeExpression[] getLocationParameters = null;
            if (withLocationReferences)
            {
                getLocationParameters = new CodeExpression[] { 
                    getExpression,  
                    setExpression,
                    new CodeVariableReferenceExpression("expressionId"),
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        rootActivityFieldName),
                    new CodeVariableReferenceExpression("activityContext") };
            }
            else
            {
                getLocationParameters = new CodeExpression[] { 
                    getExpression,  
                    setExpression };
            }

            CodeMethodInvokeExpression getLocationExpression = new CodeMethodInvokeExpression(
                getLocationMethod,
                getLocationParameters);


            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(getLocationExpression);

            compiledDataContextStatements.Add(returnStatement);
            
            CodeStatement[] returnStatements = new CodeStatement[compiledDataContextStatements.Count];
            compiledDataContextStatements.CopyTo(returnStatements, 0);

            return returnStatements;
        }

        CodeStatement[] GenerateValueExpressionInvocation(CompiledExpressionDescriptor descriptor, bool withLocationReferences, Dictionary<string, int> cacheIndicies)
        {
            CodeStatementCollection compiledDataContextStatements = new CodeStatementCollection();

            string indexString = descriptor.Id.ToString(CultureInfo.InvariantCulture);
            string dataContextVariableName = "valDataContext" + indexString;

            CodeVariableDeclarationStatement dataContextVariable = new CodeVariableDeclarationStatement(
                     new CodeTypeReference(descriptor.TypeName), dataContextVariableName);

            GenerateGetDataContextVariable(descriptor, dataContextVariable, compiledDataContextStatements, withLocationReferences, cacheIndicies);
            compiledDataContextStatements.Add(dataContextVariable);

            CodeMethodInvokeExpression expressionInvoke = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeVariableReferenceExpression(dataContextVariableName), descriptor.GetMethodName));

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(expressionInvoke);

            compiledDataContextStatements.Add(returnStatement);

            CodeStatement[] returnStatements = new CodeStatement[compiledDataContextStatements.Count];
            compiledDataContextStatements.CopyTo(returnStatements, 0);

            return returnStatements;
        }

        CodeStatement[] GenerateStatementInvocation(CompiledExpressionDescriptor descriptor, bool withLocationReferences, Dictionary<string, int> cacheIndicies)
        {
            string indexString = descriptor.Id.ToString(CultureInfo.InvariantCulture);
            string dataContextVariableName = "valDataContext" + indexString;

            CodeVariableDeclarationStatement dataContextVariable = new CodeVariableDeclarationStatement(
                     new CodeTypeReference(descriptor.TypeName), dataContextVariableName);

            CodeStatementCollection compiledDataContextStatements = new CodeStatementCollection();

            GenerateGetDataContextVariable(descriptor, dataContextVariable, compiledDataContextStatements, withLocationReferences, cacheIndicies);
            compiledDataContextStatements.Add(dataContextVariable);
                        
            CodeMethodInvokeExpression expressionInvoke = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeVariableReferenceExpression(dataContextVariableName), descriptor.StatementMethodName));
            
            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodePrimitiveExpression(null));

            compiledDataContextStatements.Add(expressionInvoke);
            compiledDataContextStatements.Add(returnStatement);

            CodeStatement[] returnStatements = new CodeStatement[compiledDataContextStatements.Count];
            compiledDataContextStatements.CopyTo(returnStatements, 0);

            return returnStatements;
        }
           
        void GenerateCanExecuteMethod()
        {
            CodeMemberMethod isValidMethod = new CodeMemberMethod();
            isValidMethod.Name = "CanExecuteExpression";
            isValidMethod.ReturnType = new CodeTypeReference(typeof(bool));
            isValidMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            isValidMethod.CustomAttributes.Add(GeneratedCodeAttribute);
            isValidMethod.CustomAttributes.Add(BrowsableCodeAttribute);
            isValidMethod.CustomAttributes.Add(EditorBrowsableCodeAttribute);
            isValidMethod.ImplementationTypes.Add(new CodeTypeReference(typeof(ICompiledExpressionRoot)));

            isValidMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "expressionText"));
            isValidMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(bool)), "isReference"));
            isValidMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IList<LocationReference>)), "locations"));

            CodeParameterDeclarationExpression expressionIdParam = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "expressionId");
            expressionIdParam.Direction = FieldDirection.Out;
            isValidMethod.Parameters.Add(expressionIdParam);
           
            //
            // if (((isReference == false)
            //              && ((expressionText == [expression text])
            //              && ([data context type name].Validate(locations, true) == true))))
            // {
            //     expressionId = [id for expression text and data context];
            //     return true;
            // }
            // 
            foreach (CompiledExpressionDescriptor descriptor in expressionDescriptors)
            {
                CodeBinaryOperatorExpression checkIsReferenceExpression = new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("isReference"),
                    CodeBinaryOperatorType.ValueEquality,
                    new CodePrimitiveExpression(descriptor.IsReference));

                CodeBinaryOperatorExpression checkTextExpression = new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("expressionText"),
                    CodeBinaryOperatorType.ValueEquality,
                    new CodePrimitiveExpression(descriptor.ExpressionText));

                CodeMethodInvokeExpression invokeValidateExpression = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeTypeReferenceExpression(descriptor.TypeName),
                        "Validate"),
                    new CodeVariableReferenceExpression("locations"),
                    new CodePrimitiveExpression(true),
                    new CodePrimitiveExpression(0));
                
                CodeBinaryOperatorExpression checkValidateExpression = new CodeBinaryOperatorExpression(
                    invokeValidateExpression,
                    CodeBinaryOperatorType.ValueEquality,
                    new CodePrimitiveExpression(true));

                CodeBinaryOperatorExpression checkTextAndValidateExpression = new CodeBinaryOperatorExpression(
                    checkTextExpression,
                    CodeBinaryOperatorType.BooleanAnd,
                    checkValidateExpression);

                CodeBinaryOperatorExpression checkIsReferenceAndTextAndValidateExpression = new CodeBinaryOperatorExpression(
                    checkIsReferenceExpression,
                    CodeBinaryOperatorType.BooleanAnd,
                    checkTextAndValidateExpression);
                
                CodeAssignStatement assignId = new CodeAssignStatement(
                    new CodeVariableReferenceExpression("expressionId"),
                    new CodePrimitiveExpression(descriptor.Id));

                CodeConditionStatement matchCondition = new CodeConditionStatement(
                    checkIsReferenceAndTextAndValidateExpression);

                matchCondition.TrueStatements.Add(assignId);
                matchCondition.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
                
                isValidMethod.Statements.Add(matchCondition);
            }

            isValidMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("expressionId"),
                    new CodePrimitiveExpression(-1)));

            isValidMethod.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodePrimitiveExpression(false)));

            classDeclaration.Members.Add(isValidMethod);
        }

        CodeObjectCreateExpression GenerateDataContextCreateExpression(string typeName, bool withLocationReferences)
        {
            if (withLocationReferences)
            {
                return new CodeObjectCreateExpression(
                    new CodeTypeReference(typeName),
                    new CodeVariableReferenceExpression("locations"),
                    new CodeVariableReferenceExpression("activityContext"),
                    new CodePrimitiveExpression(true));
            }
            else
            {
                return new CodeObjectCreateExpression(
                    new CodeTypeReference(typeName),
                    new CodeExpression[] { new CodeVariableReferenceExpression("locations"),
                    new CodePrimitiveExpression(true) });
            }
        }

        bool TryGenerateExpressionCode(Activity activity, CompiledDataContextDescriptor dataContextDescriptor, int nextExpressionId, string language)
        {
            ITextExpression textExpression = (ITextExpression)activity;
            if (!TextExpression.LanguagesAreEqual(textExpression.Language, language)
                || string.IsNullOrWhiteSpace(textExpression.ExpressionText))
            {
                //
                // We can only compile expressions that match the project's flavor
                // and expression activities with no expressions don't need anything generated.
                return false;
            }

            Type resultType = (activity is ActivityWithResult) ? ((ActivityWithResult)activity).ResultType : null;

            string expressionText = textExpression.ExpressionText;
                        
            bool isReference = false;
            bool isValue = false;
            bool isStatement = false;

            if (resultType == null)
            {
                isStatement = true;
            }
            else
            {
                isReference = TypeHelper.AreTypesCompatible(resultType, typeof(Location));
                isValue = !isReference;
            }

            CodeTypeDeclaration typeDeclaration;
            if (isValue)
            {
                typeDeclaration = dataContextDescriptor.CodeTypeDeclarationForReadOnly;
            }
            else
            {
                //
                // Statement and reference get read/write context
                typeDeclaration = dataContextDescriptor.CodeTypeDeclaration;
            }
             
            CompiledExpressionDescriptor descriptor = new CompiledExpressionDescriptor();
            descriptor.TypeName = typeDeclaration.Name;
            descriptor.Id = nextExpressionId;
            descriptor.ExpressionText = textExpression.ExpressionText;
            
            if (isReference)
            {
                if (resultType.IsGenericType)
                {
                    resultType = resultType.GetGenericArguments()[0];
                }
                else
                {
                    resultType = typeof(object);
                }
            }

            descriptor.ResultType = resultType;
            
            GenerateExpressionGetTreeMethod(activity, descriptor, dataContextDescriptor, isValue, isStatement, nextExpressionId);

            if (isValue || isReference)
            {
                CodeMemberMethod expressionGetMethod = GenerateGetMethod(activity, resultType, expressionText, nextExpressionId);
                typeDeclaration.Members.Add(expressionGetMethod);

                CodeMemberMethod expressionGetValueTypeAccessorMethod = GenerateGetMethodWrapper(expressionGetMethod);
                typeDeclaration.Members.Add(expressionGetValueTypeAccessorMethod);

                descriptor.GetMethodName = expressionGetValueTypeAccessorMethod.Name;
            }

            if (isReference)
            {
                CodeMemberMethod expressionSetMethod = GenerateSetMethod(activity, resultType, expressionText, nextExpressionId);
                dataContextDescriptor.CodeTypeDeclaration.Members.Add(expressionSetMethod);

                CodeMemberMethod expressionSetValueTypeAccessorMethod = GenerateSetMethodWrapper(expressionSetMethod);
                dataContextDescriptor.CodeTypeDeclaration.Members.Add(expressionSetValueTypeAccessorMethod);

                descriptor.SetMethodName = expressionSetValueTypeAccessorMethod.Name;
            }

            if (isStatement)
            {
                CodeMemberMethod statementMethod = GenerateStatementMethod(activity, expressionText, nextExpressionId);
                dataContextDescriptor.CodeTypeDeclaration.Members.Add(statementMethod);

                CodeMemberMethod expressionSetValueTypeAccessorMethod = GenerateStatementMethodWrapper(statementMethod);
                dataContextDescriptor.CodeTypeDeclaration.Members.Add(expressionSetValueTypeAccessorMethod);

                descriptor.StatementMethodName = expressionSetValueTypeAccessorMethod.Name;
            }

            expressionDescriptors.Add(descriptor);

            return true;
        }

        void GenerateExpressionGetTreeMethod(Activity activity, CompiledExpressionDescriptor expressionDescriptor, CompiledDataContextDescriptor dataContextDescriptor, bool isValue, bool isStatement, int nextExpressionId)
        {
            CodeMemberMethod expressionMethod = new CodeMemberMethod();
            expressionMethod.Attributes = MemberAttributes.Assembly | MemberAttributes.Final;
            expressionMethod.Name = string.Format(CultureInfo.InvariantCulture, expressionGetTreeString, nextExpressionId);
            expressionMethod.ReturnType = new CodeTypeReference(typeof(Expression));
            expressionDescriptor.GetExpressionTreeMethodName = expressionMethod.Name;

            if (isStatement)
            {
                // Can't generate expression tree for a statement
                expressionMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                dataContextDescriptor.CodeTypeDeclaration.Members.Add(expressionMethod);
                return;
            }

            string coreExpressionText = expressionDescriptor.ExpressionText;
            CodeLinePragma pragma;
            AlignText(activity, ref coreExpressionText, out pragma);

            Type returnType = typeof(Expression<>).MakeGenericType(typeof(Func<>).MakeGenericType(expressionDescriptor.ResultType));
            string expressionText = null;
            if (IsVB)
            {
                expressionText = string.Concat(vbLambdaString, coreExpressionText);
            }
            else if (IsCS)
            {
                expressionText = string.Concat(csharpLambdaString, coreExpressionText);
            }

            if (expressionText != null)
            {
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(returnType, "expression", new CodeSnippetExpression(expressionText));
                statement.LinePragma = pragma;
                expressionMethod.Statements.Add(statement);

                CodeMethodInvokeExpression invokeExpression = new CodeMethodInvokeExpression(
                    new CodeBaseReferenceExpression(),
                    "RewriteExpressionTree",
                    new CodeExpression[] { new CodeVariableReferenceExpression("expression") });

                expressionMethod.Statements.Add(new CodeMethodReturnStatement(invokeExpression));
            }
            else
            {
                expressionMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
            }            

            if (isValue)
            {
                dataContextDescriptor.CodeTypeDeclarationForReadOnly.Members.Add(expressionMethod);
            }
            else
            {
                dataContextDescriptor.CodeTypeDeclaration.Members.Add(expressionMethod);
            }            
        }

        CodeMemberMethod GenerateGetMethod(Activity activity, Type resultType, string expressionText, int nextExpressionId)
        {
            CodeMemberMethod expressionMethod = new CodeMemberMethod();
            expressionMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            expressionMethod.Name = string.Format(CultureInfo.InvariantCulture, expressionGetString, nextExpressionId);
            expressionMethod.ReturnType = new CodeTypeReference(resultType);
            expressionMethod.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(DebuggerHiddenAttribute))));

            CodeLinePragma pragma;
            AlignText(activity, ref expressionText, out pragma);
            CodeStatement statement = new CodeMethodReturnStatement(new CodeSnippetExpression(expressionText));
            statement.LinePragma = pragma;
            expressionMethod.Statements.Add(statement);

            return expressionMethod;
        }

        CodeMemberMethod GenerateGetMethodWrapper(CodeMemberMethod expressionMethod)
        {
            CodeMemberMethod wrapperMethod = new CodeMemberMethod();
            wrapperMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            wrapperMethod.Name = valueTypeAccessorString + expressionMethod.Name;
            wrapperMethod.ReturnType = expressionMethod.ReturnType;

            wrapperMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeThisReferenceExpression(),
                    getValueTypeValuesString)));

            wrapperMethod.Statements.Add(new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeThisReferenceExpression(),
                        expressionMethod.Name))));

            return wrapperMethod;
        }

        CodeMemberMethod GenerateSetMethod(Activity activity, Type resultType, string expressionText, int nextExpressionId)
        {
            string paramName = "value";

            if (string.Compare(expressionText, paramName, true, Globalization.CultureInfo.CurrentCulture) == 0)
            {
                paramName += "1";
            }

            CodeMemberMethod expressionMethod = new CodeMemberMethod();
            expressionMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            expressionMethod.Name = string.Format(CultureInfo.InvariantCulture, expressionSetString, nextExpressionId);
            expressionMethod.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(DebuggerHiddenAttribute))));

            var exprValueParam = new CodeParameterDeclarationExpression(resultType, paramName);
            expressionMethod.Parameters.Add(exprValueParam);

            CodeLinePragma pragma;
            AlignText(activity, ref expressionText, out pragma);
            CodeAssignStatement statement = new CodeAssignStatement(new CodeSnippetExpression(expressionText), new CodeArgumentReferenceExpression(paramName));
            statement.LinePragma = pragma;
            expressionMethod.Statements.Add(statement);
            
            return expressionMethod;
        }

        CodeMemberMethod GenerateSetMethodWrapper(CodeMemberMethod expressionMethod)
        {
            CodeMemberMethod wrapperMethod = new CodeMemberMethod();
            wrapperMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            wrapperMethod.Name = valueTypeAccessorString + expressionMethod.Name;

            CodeParameterDeclarationExpression exprValueParam = new CodeParameterDeclarationExpression(expressionMethod.Parameters[0].Type, expressionMethod.Parameters[0].Name);
            wrapperMethod.Parameters.Add(exprValueParam);

            wrapperMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeThisReferenceExpression(),
                    getValueTypeValuesString)));

            CodeMethodInvokeExpression setExpression = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeThisReferenceExpression(),
                    expressionMethod.Name));

            setExpression.Parameters.Add(new CodeVariableReferenceExpression(expressionMethod.Parameters[0].Name));

            wrapperMethod.Statements.Add(setExpression);

            wrapperMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeThisReferenceExpression(),
                    setValueTypeValuesString)));

            return wrapperMethod;
        }

        CodeMemberMethod GenerateStatementMethod(Activity activity, string expressionText, int nextExpressionId)
        {
            CodeMemberMethod expressionMethod = new CodeMemberMethod();
            expressionMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            expressionMethod.Name = string.Format(CultureInfo.InvariantCulture, expressionStatementString, nextExpressionId);
            expressionMethod.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(DebuggerHiddenAttribute))));

            CodeLinePragma pragma;
            AlignText(activity, ref expressionText, out pragma);
            CodeStatement statement = new CodeSnippetStatement(expressionText);
            statement.LinePragma = pragma;
            expressionMethod.Statements.Add(statement);

            return expressionMethod;
        }

        CodeMemberMethod GenerateStatementMethodWrapper(CodeMemberMethod expressionMethod)
        {
            CodeMemberMethod wrapperMethod = new CodeMemberMethod();
            wrapperMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            wrapperMethod.Name = valueTypeAccessorString + expressionMethod.Name;

            wrapperMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeThisReferenceExpression(),
                    getValueTypeValuesString)));

            CodeMethodInvokeExpression setExpression = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeThisReferenceExpression(),
                    expressionMethod.Name));

            wrapperMethod.Statements.Add(setExpression);

            wrapperMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeThisReferenceExpression(),
                    setValueTypeValuesString)));

            return wrapperMethod;
        }
        
        CodeMemberMethod GenerateGetValueTypeValues(CompiledDataContextDescriptor descriptor)
        {
            CodeMemberMethod fetchMethod = new CodeMemberMethod();
            fetchMethod.Name = getValueTypeValuesString;
            fetchMethod.Attributes = MemberAttributes.Override | MemberAttributes.Family;
            
            foreach (KeyValuePair<string, MemberData> valueField in descriptor.Fields)
            {
                if (descriptor.Duplicates.Contains(valueField.Key))
                {
                    continue;
                }
                
                CodeExpression getValue = new CodeCastExpression(
                    valueField.Value.Type,
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeThisReferenceExpression(), 
                            "GetVariableValue"), 
                             new CodeBinaryOperatorExpression(
                                 new CodePrimitiveExpression(valueField.Value.Index), 
                                 CodeBinaryOperatorType.Add,
                                 new CodeVariableReferenceExpression("locationsOffset"))));

                CodeFieldReferenceExpression fieldReference = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), valueField.Key);

                fetchMethod.Statements.Add(
                    new CodeAssignStatement(fieldReference, getValue));
            }

            fetchMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeBaseReferenceExpression(),
                    fetchMethod.Name)));
            
            return fetchMethod;
        }

        CodeMemberMethod GenerateSetValueTypeValues(CompiledDataContextDescriptor descriptor)
        {
            CodeMemberMethod pushMethod = new CodeMemberMethod();
            pushMethod.Name = setValueTypeValuesString;
            pushMethod.Attributes = MemberAttributes.Override | MemberAttributes.Family;

            foreach (KeyValuePair<string, MemberData> valueField in descriptor.Fields)
            {
                if (descriptor.Duplicates.Contains(valueField.Key))
                {
                    continue;
                }

                CodeMethodInvokeExpression setValue = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeThisReferenceExpression(),
                        "SetVariableValue"),
                        new CodeBinaryOperatorExpression(
                        new CodePrimitiveExpression(valueField.Value.Index),
                        CodeBinaryOperatorType.Add, 
                        new CodeVariableReferenceExpression("locationsOffset")),
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(), valueField.Key));
                
                pushMethod.Statements.Add(setValue);
            }

            pushMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeBaseReferenceExpression(),
                    pushMethod.Name)));

            return pushMethod;
        }

        CodeTypeDeclaration GenerateCompiledDataContext(bool forReadOnly)
        {
            string forReadOnlyString = forReadOnly ? TextExpressionCompiler.forReadOnly : string.Empty;
            string contextName = string.Concat(this.settings.ActivityName, TextExpressionCompiler.typedDataContextName, this.nextContextId, forReadOnlyString);

            CodeTypeDeclaration typedDataContext = new CodeTypeDeclaration(contextName);
            typedDataContext.TypeAttributes = TypeAttributes.NestedPrivate;
            //
            // data context classes are declared inside of the main class via the partial class to reduce visibility/surface area.
            this.classDeclaration.Members.Add(typedDataContext);

            if (this.compiledDataContexts != null && this.compiledDataContexts.Count > 0)
            {
                string baseTypeName = null;
                if (forReadOnly)
                {
                    baseTypeName = this.compiledDataContexts.Peek().CodeTypeDeclarationForReadOnly.Name;
                }
                else
                {
                    baseTypeName = this.compiledDataContexts.Peek().CodeTypeDeclaration.Name;
                }
                typedDataContext.BaseTypes.Add(baseTypeName);
            }
            else
            {
                typedDataContext.BaseTypes.Add(typeof(CompiledDataContext));
                //
                // We only generate the helper method on the root data context/context 0
                // No need to have it on all contexts.  This is just a slight of hand
                // so that we don't need to make GetDataContextActivities public on CompiledDataContext.
                typedDataContext.Members.Add(GenerateDataContextActivitiesHelper());
            }

            CodeMemberField offsetField = new CodeMemberField();
            offsetField.Attributes = MemberAttributes.Private;
            offsetField.Name = locationsOffsetFieldName;
            offsetField.Type = new CodeTypeReference(typeof(int));

            typedDataContext.Members.Add(offsetField);

            CodeMemberField expectedLocationsCountField = new CodeMemberField();
            expectedLocationsCountField.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            expectedLocationsCountField.Name = expectedLocationsCountFieldName;
            expectedLocationsCountField.Type = new CodeTypeReference(typeof(int));

            typedDataContext.Members.Add(expectedLocationsCountField);

            typedDataContext.Members.Add(GenerateLocationReferenceActivityContextConstructor());
            typedDataContext.Members.Add(GenerateLocationConstructor());
            typedDataContext.Members.Add(GenerateLocationReferenceConstructor());
            typedDataContext.Members.Add(GenerateCacheHelper());
            typedDataContext.Members.Add(GenerateSetLocationsOffsetMethod());

            //
            // Mark this type as tool generated code
            typedDataContext.CustomAttributes.Add(GeneratedCodeAttribute);
            //
            // Mark it as Browsable(false) 
            // Note that this does not prevent intellisense within a single project, just at the metadata level            
            typedDataContext.CustomAttributes.Add(BrowsableCodeAttribute);
            //
            // Mark it as EditorBrowsable(EditorBrowsableState.Never)
            // Note that this does not prevent intellisense within a single project, just at the metadata level
            typedDataContext.CustomAttributes.Add(EditorBrowsableCodeAttribute);

            return typedDataContext;
        }

        CodeMemberMethod GenerateDataContextActivitiesHelper()
        {
            CodeMemberMethod dataContextActivitiesHelper = new CodeMemberMethod();

            dataContextActivitiesHelper.Name = "GetDataContextActivitiesHelper";

            dataContextActivitiesHelper.Attributes = MemberAttributes.Assembly | MemberAttributes.Final | MemberAttributes.Static;

            if (this.compiledDataContexts != null && this.compiledDataContexts.Count > 0)
            {
                dataContextActivitiesHelper.Attributes |= MemberAttributes.New;
            }

            dataContextActivitiesHelper.ReturnType = new CodeTypeReference(typeof(object));

            dataContextActivitiesHelper.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    new CodeTypeReference(typeof(Activity)),
                    "compiledRoot"));

            dataContextActivitiesHelper.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    new CodeTypeReference(typeof(bool)),
                    forImplementationName));

            dataContextActivitiesHelper.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(CompiledDataContext)),
                            "GetDataContextActivities"),
                        new CodeVariableReferenceExpression("compiledRoot"),
                        new CodeVariableReferenceExpression(forImplementationName))));

            return dataContextActivitiesHelper;
        }

        CodeMemberMethod GenerateSetLocationsOffsetMethod()
        {
            CodeMemberMethod setLocationsOffsetMethod = new CodeMemberMethod();
            setLocationsOffsetMethod.Name = "SetLocationsOffset";
            setLocationsOffsetMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)),
                    "locationsOffsetValue"));
            setLocationsOffsetMethod.Attributes = MemberAttributes.Public;
            if (this.compiledDataContexts.Count > 0)
            {
                setLocationsOffsetMethod.Attributes |= MemberAttributes.New;
            }

            CodeAssignStatement assignLocationsOffsetStatement = new CodeAssignStatement(
                new CodeVariableReferenceExpression("locationsOffset"),
                new CodeVariableReferenceExpression("locationsOffsetValue"));
            setLocationsOffsetMethod.Statements.Add(assignLocationsOffsetStatement);

            if (this.nextContextId > 0)
            {
                CodeMethodInvokeExpression baseSetLocationsOffsetMethod = new CodeMethodInvokeExpression(
                    new CodeBaseReferenceExpression(), "SetLocationsOffset", new CodeVariableReferenceExpression("locationsOffset"));
                setLocationsOffsetMethod.Statements.Add(baseSetLocationsOffsetMethod);
            }

            return setLocationsOffsetMethod;
        }

        CodeMemberMethod GenerateCacheHelper()
        {
            CodeMemberMethod cacheHelper = new CodeMemberMethod();
            cacheHelper.Name = "GetCompiledDataContextCacheHelper";
            cacheHelper.Attributes = MemberAttributes.Assembly | MemberAttributes.Final | MemberAttributes.Static;

            if (this.compiledDataContexts != null && this.compiledDataContexts.Count > 0)
            {
                cacheHelper.Attributes |= MemberAttributes.New;
            }

            cacheHelper.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), dataContextActivitiesFieldName));
            cacheHelper.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ActivityContext), "activityContext"));
            cacheHelper.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Activity), "compiledRoot"));
            cacheHelper.Parameters.Add(new CodeParameterDeclarationExpression(typeof(bool), forImplementationName));
            cacheHelper.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "compiledDataContextCount"));

            cacheHelper.ReturnType = new CodeTypeReference(typeof(CompiledDataContext[]));

            cacheHelper.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(CompiledDataContext)),
                            "GetCompiledDataContextCache"),
                        new CodeVariableReferenceExpression(dataContextActivitiesFieldName),
                        new CodeVariableReferenceExpression("activityContext"),
                        new CodeVariableReferenceExpression("compiledRoot"),
                        new CodeVariableReferenceExpression(forImplementationName),
                        new CodeVariableReferenceExpression("compiledDataContextCount"))));

            return cacheHelper;
        }

        CodeConstructor GenerateLocationReferenceActivityContextConstructor()
        {
            //
            // public [typename](IList<LocationReference> locations, ActivityContext activityContext)
            //   : base(locations, activityContext)
            //
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;

            CodeParameterDeclarationExpression constructorLocationsParam =
                new CodeParameterDeclarationExpression(typeof(IList<LocationReference>), "locations");
            constructor.Parameters.Add(constructorLocationsParam);

            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("locations"));

            CodeParameterDeclarationExpression constructorActivityContextParam = 
                new CodeParameterDeclarationExpression(typeof(ActivityContext), "activityContext");
            constructor.Parameters.Add(constructorActivityContextParam);

            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("activityContext"));

            CodeParameterDeclarationExpression computelocationsOffsetParam = 
                new CodeParameterDeclarationExpression(typeof(bool), "computelocationsOffset");
            constructor.Parameters.Add(computelocationsOffsetParam);

            if (this.nextContextId > 0)
            {
                constructor.BaseConstructorArgs.Add(new CodePrimitiveExpression(false));
            }

            InvokeSetLocationsOffsetMethod(constructor);           
            
            return constructor;
        }

        CodeConstructor GenerateLocationConstructor()
        {
            //
            // public [typename](IList<Location> locations, ActivityContext activityContext)
            //   : base(locations)
            //
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;

            CodeParameterDeclarationExpression constructorLocationsParam =
                new CodeParameterDeclarationExpression(typeof(IList<Location>), "locations");
            constructor.Parameters.Add(constructorLocationsParam);

            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("locations"));

            CodeParameterDeclarationExpression computelocationsOffsetParam =
                new CodeParameterDeclarationExpression(typeof(bool), "computelocationsOffset");
            constructor.Parameters.Add(computelocationsOffsetParam);

            if (this.nextContextId > 0)
            {                
                constructor.BaseConstructorArgs.Add(new CodePrimitiveExpression(false));
            }

            InvokeSetLocationsOffsetMethod(constructor);
   
            return constructor;
        }

        CodeConstructor GenerateLocationReferenceConstructor()
        {
            //
            // public [typename](IList<LocationReference> locationReferences)
            //   : base(locationReferences)
            //
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;

            CodeParameterDeclarationExpression constructorLocationsParam = new CodeParameterDeclarationExpression(typeof(IList<LocationReference>), "locationReferences");
            constructor.Parameters.Add(constructorLocationsParam);

            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("locationReferences"));

            return constructor;
        }

        void InvokeSetLocationsOffsetMethod(CodeConstructor constructor)
        {
            CodeExpressionStatement setLocationsOffsetMethod = new CodeExpressionStatement(
                new CodeMethodInvokeExpression(
                new CodeThisReferenceExpression(),
                "SetLocationsOffset",
                new CodeBinaryOperatorExpression(
                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("locations"), "Count"),
                    CodeBinaryOperatorType.Subtract,
                    new CodeVariableReferenceExpression("expectedLocationsCount"))));
            
            CodeConditionStatement offsetCheckStatement = new CodeConditionStatement(new CodeBinaryOperatorExpression(
                new CodeVariableReferenceExpression("computelocationsOffset"),
                CodeBinaryOperatorType.ValueEquality,
                new CodePrimitiveExpression(true)),
                new CodeStatement[] { setLocationsOffsetMethod });
            
            constructor.Statements.Add(offsetCheckStatement);            
        }

        CodeNamespace GenerateCodeNamespace()
        {
            CodeNamespace codeNamespace = new CodeNamespace(this.settings.ActivityNamespace);            

            bool seenXamlIntegration = false;
            foreach (string nsReference in GetNamespaceReferences())
            {
                if (!seenXamlIntegration && nsReference == xamlIntegrationNamespace)
                {
                    seenXamlIntegration = true;
                }
                codeNamespace.Imports.Add(new CodeNamespaceImport(nsReference)
                {
                    LinePragma = GenerateLinePragmaForNamespace(nsReference),
                });
            }

            if (!seenXamlIntegration)
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport(xamlIntegrationNamespace)
                {
                    LinePragma = GenerateLinePragmaForNamespace(xamlIntegrationNamespace),
                });
            }

            return codeNamespace;
        }

        bool AssemblyContainsTypeWithActivityNamespace()
        {
            // We need to include the ActivityNamespace in the imports if there are any types in
            // the Activity's assembly that are contained in that namespace.
            Type[] types;
            try
            {
                types = this.settings.Activity.GetType().Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                // We had a problem loading all the types. Take the safe route and assume we need to include the ActivityNamespace.
                return true;
            }

            if (types != null)
            {
                foreach (Type type in types)
                {
                    if (type.Namespace == this.settings.ActivityNamespace)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        IEnumerable<string> GetNamespaceReferences()
        {
            HashSet<string> nsReferences = new HashSet<string>();
            // Add some namespace imports, use the same base set for C# as for VB, they aren't lang specific
            foreach (string nsReference in TextExpression.DefaultNamespaces)
            {
                nsReferences.Add(nsReference);
            }


            VisualBasicSettings vbSettings = null;
            if (IsVB)
            {
                vbSettings = VisualBasic.GetSettings(this.settings.Activity);
            }
            if (vbSettings != null)
            {
                foreach (VisualBasicImportReference nsReference in vbSettings.ImportReferences)
                {
                    if (!string.IsNullOrWhiteSpace(nsReference.Import))
                    {
                        // For VB, the ActivityNamespace has the RootNamespace stripped off. We don't need an Imports reference
                        // to ActivityNamespace, if this reference is in the same assembly and there is a RootNamespace specified.
                        // We check both Assembly.FullName and
                        // Assembly.GetName().Name because testing has shown that nsReference.Assembly sometimes gives fully qualified
                        // names and sometimes not.
                        if (
                            (nsReference.Import == this.settings.ActivityNamespace)
                            &&
                            ((nsReference.Assembly == this.settings.Activity.GetType().Assembly.FullName) ||
                             (nsReference.Assembly == this.settings.Activity.GetType().Assembly.GetName().Name))
                            &&
                            !string.IsNullOrWhiteSpace(this.settings.RootNamespace)
                            &&
                            !AssemblyContainsTypeWithActivityNamespace()
                            )
                        {
                            continue;
                        }

                        nsReferences.Add(nsReference.Import);
                    }
                }
            }
            else
            {
                IList<string> references = this.settings.ForImplementation ?
                    TextExpression.GetNamespacesForImplementation(this.settings.Activity) :
                    TextExpression.GetNamespaces(this.settings.Activity);

                foreach (string nsReference in references)
                {
                    if (!string.IsNullOrWhiteSpace(nsReference))
                    {
                        nsReferences.Add(nsReference);
                    }
                }
            }

            return nsReferences;
        }

        CompiledDataContextDescriptor PushDataContextDescriptor()
        {
            CompiledDataContextDescriptor contextDescriptor = new CompiledDataContextDescriptor(() => this.IsVB)
            {
                CodeTypeDeclaration = GenerateCompiledDataContext(false),
                CodeTypeDeclarationForReadOnly = GenerateCompiledDataContext(true),
                NextMemberIndex = GetStartMemberIndex()
            };
            this.compiledDataContexts.Push(contextDescriptor);
            this.nextContextId++;
            
            return contextDescriptor;
        }

        void PopDataContextDescriptor()
        {
            CompiledDataContextDescriptor descriptor = this.compiledDataContexts.Pop();
            if (descriptor != null)
            {
                GenerateMembers(descriptor);
                GenerateValidate(descriptor, true);
                GenerateValidate(descriptor, false);
            }
        }

        int GetStartMemberIndex()
        {
            if (this.compiledDataContexts == null || this.compiledDataContexts.Count == 0)
            {
                return 0;
            }
            else
            {
                return this.compiledDataContexts.Peek().NextMemberIndex;
            }
        }
                
        void GenerateValidate(CompiledDataContextDescriptor descriptor, bool forReadOnly)
        {
            //
            //
            // Validate the locations at runtime match the set at compile time
            //
            // protected override bool Validate(IList<LocationReference> locationReferences)
            // {
            //   if (validateLocationCount && locationReferences.Count != [generated count of location references])
            //   {
            //     return false;
            //   }
            //   if (locationReferences[0].Name != [generated name for index] ||
            //       locationReferences[0].Type != typeof([generated type for index]))
            //   {
            //     return false;
            //   }
            //
            //   ...
            //
            // }
            CodeMemberMethod validateMethod = new CodeMemberMethod();
            validateMethod.Name = "Validate";
            validateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;

            if (this.compiledDataContexts.Count > 0)
            {
                validateMethod.Attributes |= MemberAttributes.New;
            }


            validateMethod.ReturnType = new CodeTypeReference(typeof(bool));

            validateMethod.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    new CodeTypeReference(typeof(IList<LocationReference>)),
                    "locationReferences"));

            validateMethod.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    new CodeTypeReference(typeof(bool)),
                    "validateLocationCount"));

            validateMethod.Parameters.Add(
               new CodeParameterDeclarationExpression(
                   new CodeTypeReference(typeof(int)),
                   "offset") );

            CodeBinaryOperatorExpression shouldCheckLocationCountExpression = new CodeBinaryOperatorExpression(
                new CodeVariableReferenceExpression("validateLocationCount"),
                CodeBinaryOperatorType.ValueEquality,
                new CodePrimitiveExpression(true));

            CodeBinaryOperatorExpression compareLocationCountExpression = new CodeBinaryOperatorExpression(
                    new CodePropertyReferenceExpression(
                    new CodeVariableReferenceExpression("locationReferences"),            
                    "Count"),
                    CodeBinaryOperatorType.LessThan,
                    new CodePrimitiveExpression(descriptor.NextMemberIndex)
                    );

            CodeBinaryOperatorExpression checkLocationCountExpression = new CodeBinaryOperatorExpression(
                shouldCheckLocationCountExpression,
                CodeBinaryOperatorType.BooleanAnd,
                compareLocationCountExpression);

            CodeConditionStatement checkLocationCountStatement = new CodeConditionStatement(
                checkLocationCountExpression,
                new CodeMethodReturnStatement(
                    new CodePrimitiveExpression(false)));

            validateMethod.Statements.Add(checkLocationCountStatement);

            if (descriptor.NextMemberIndex > 0)
            {
                CodeConditionStatement generateNewOffset = new CodeConditionStatement(shouldCheckLocationCountExpression,
                    new CodeStatement[] 
                {
                    new CodeAssignStatement(new CodeVariableReferenceExpression("offset"), 
                    new CodeBinaryOperatorExpression(
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("locationReferences"), "Count"),
                        CodeBinaryOperatorType.Subtract,
                        new CodePrimitiveExpression(descriptor.NextMemberIndex)))
                });
                validateMethod.Statements.Add(generateNewOffset);
            }

            CodeAssignStatement setexpectedLocationsCountStatement = new CodeAssignStatement(
                new CodeVariableReferenceExpression("expectedLocationsCount"),
                new CodePrimitiveExpression(descriptor.NextMemberIndex));

            validateMethod.Statements.Add(setexpectedLocationsCountStatement);

            foreach (KeyValuePair<string, MemberData> kvp in descriptor.Properties)
            {
                validateMethod.Statements.Add(GenerateLocationReferenceCheck(kvp.Value));
            }

            foreach (KeyValuePair<string, MemberData> kvp in descriptor.Fields)
            {
                validateMethod.Statements.Add(GenerateLocationReferenceCheck(kvp.Value));
            }
                        
            if (this.compiledDataContexts.Count >= 1)
            {
                CompiledDataContextDescriptor baseDescriptor = this.compiledDataContexts.Peek();
                CodeTypeDeclaration baseType = forReadOnly ? baseDescriptor.CodeTypeDeclarationForReadOnly : baseDescriptor.CodeTypeDeclaration;

                CodeMethodInvokeExpression invokeBase = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeTypeReferenceExpression(baseType.Name),
                        "Validate"),
                    new CodeVariableReferenceExpression("locationReferences"),
                    new CodePrimitiveExpression(false),
                    new CodeVariableReferenceExpression("offset"));

                validateMethod.Statements.Add(
                    new CodeMethodReturnStatement(invokeBase));
            }
            else
            {
                validateMethod.Statements.Add(
                    new CodeMethodReturnStatement(
                        new CodePrimitiveExpression(true)));
            }

            if (forReadOnly)
            {
                descriptor.CodeTypeDeclarationForReadOnly.Members.Add(validateMethod);
            }
            else
            {
                descriptor.CodeTypeDeclaration.Members.Add(validateMethod);
            }
        }

        CodeConditionStatement GenerateLocationReferenceCheck(MemberData memberData)
        {
            CodeIndexerExpression indexer = new CodeIndexerExpression(
                new CodeVariableReferenceExpression("locationReferences"),
                new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("offset"),
                    CodeBinaryOperatorType.Add,
                    new CodePrimitiveExpression(memberData.Index)));

            CodeBinaryOperatorExpression locationNameExpression = new CodeBinaryOperatorExpression(
                new CodePropertyReferenceExpression(indexer, "Name"),
                CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression(memberData.Name));

            CodeBinaryOperatorExpression locationTypeExpression = new CodeBinaryOperatorExpression(
                new CodePropertyReferenceExpression(indexer, "Type"),
                CodeBinaryOperatorType.IdentityInequality,
                new CodeTypeOfExpression(memberData.Type));

            CodeBinaryOperatorExpression locationExpression = new CodeBinaryOperatorExpression(
                locationNameExpression,
                CodeBinaryOperatorType.BooleanOr,
                locationTypeExpression);
            
            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement();

            returnStatement.Expression = new CodePrimitiveExpression(false);

            CodeConditionStatement locationStatement = new CodeConditionStatement(
                locationExpression,
                returnStatement);

            return locationStatement;
        }
        
        [Fx.Tag.SecurityNote(Critical = "Critical because we are accessing CodeDom.",
            Safe = "Safe because we are demanding FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        void WriteCode(TextWriter textWriter)
        {
            using (CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider(this.settings.Language))
            {
                using (IndentedTextWriter indentedTextWriter = new IndentedTextWriter(textWriter))
                {
                    codeDomProvider.GenerateCodeFromNamespace(this.codeNamespace, indentedTextWriter, new CodeGeneratorOptions());
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are using the CodeDomProvider class, which has a link demand for Full Trust.",
            Safe = "Safe because we are demanding FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        TextExpressionCompilerResults CompileInMemory()
        {
            List<TextExpressionCompilerError> messages = new List<TextExpressionCompilerError>();
            CompilerParameters compilerParameters = GetCompilerParameters(messages);
            
            CompilerResults compilerResults = null;
            using (CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider(this.settings.Language))
            {
                compilerResults = codeDomProvider.CompileAssemblyFromDom(compilerParameters, this.compileUnit);
            }

            TextExpressionCompilerResults results = new TextExpressionCompilerResults();

            if (compilerResults.Errors == null || !compilerResults.Errors.HasErrors)
            {
                results.ResultType = compilerResults.CompiledAssembly.GetType(this.activityFullName);
            }

            results.HasSourceInfo = this.symbols != null;

            bool hasErrors = false;
            if (compilerResults.Errors != null && (compilerResults.Errors.HasWarnings || compilerResults.Errors.HasErrors))
            {

                foreach (CompilerError ce in compilerResults.Errors)
                {
                    TextExpressionCompilerError message = new TextExpressionCompilerError();

                    message.Message = ce.ErrorText;
                    message.Number = ce.ErrorNumber;

                    if (results.HasSourceInfo)
                    {
                        message.SourceLineNumber = ce.Line;
                    }
                    else
                    {
                        message.SourceLineNumber = -1;
                    }

                    message.IsWarning = ce.IsWarning;

                    messages.Add(message);

                    hasErrors |= !message.IsWarning;
                }
            }

            if (messages != null && messages.Count > 0)
            {
                results.SetMessages(messages, hasErrors);
            }

            return results;
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are using the CompilerParameters class, which has a link demand for Full Trust.",
            Safe = "Safe because we are demanding FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        CompilerParameters GetCompilerParameters(IList<TextExpressionCompilerError> messages)
        {
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = false;

            if (this.IsVB && !string.IsNullOrWhiteSpace(this.settings.RootNamespace))
            {
                compilerParameters.CompilerOptions = string.Concat("/rootnamespace:", this.settings.RootNamespace);
            }

            List<AssemblyReference> assemblies = this.settings.ForImplementation ?
                new List<AssemblyReference>(TextExpression.GetReferencesForImplementation(this.settings.Activity)) :
                new List<AssemblyReference>(TextExpression.GetReferences(this.settings.Activity));

            assemblies.AddRange(TextExpression.DefaultReferences);

            foreach (AssemblyReference assemblyReference in assemblies)
            {
                if (assemblyReference == null)
                {
                    continue;
                }

                assemblyReference.LoadAssembly();

                if (assemblyReference.Assembly == null)
                {
                    TextExpressionCompilerError warning = new TextExpressionCompilerError();
                    warning.IsWarning = true;
                    warning.Message = SR.TextExpressionCompilerUnableToLoadAssembly(assemblyReference.AssemblyName);

                    messages.Add(warning);

                    continue;
                }

                if (assemblyReference.Assembly.CodeBase == null)
                {
                    TextExpressionCompilerError warning = new TextExpressionCompilerError();
                    warning.IsWarning = true;
                    warning.Message = SR.TextExpressionCompilerNoCodebase(assemblyReference.AssemblyName);

                    messages.Add(warning);

                    continue;
                }

                string fileName = new Uri(assemblyReference.Assembly.CodeBase).LocalPath;
                compilerParameters.ReferencedAssemblies.Add(fileName);
            }

            return compilerParameters;
        }
        
        void AlignText(Activity activity, ref string expressionText, out CodeLinePragma pragma)
        {
            pragma = null;
            if (this.symbols != null)
            {
                SourceLocation sourceLocation = null;

                Activity currentActivity = activity;
                while (currentActivity != null && !symbols.TryGetValue(currentActivity, out sourceLocation))
                {
                    currentActivity = currentActivity.Parent;
                }

                if (sourceLocation != null && !string.IsNullOrWhiteSpace(sourceLocation.FileName))
                {
                    int startLine = sourceLocation.StartLine;
                    if (startLine > 1)
                    {
                        bool aligned = TryAlign(sourceLocation.StartColumn, ref expressionText);
                        if (aligned)
                        {
                            // Alignment inserts an extra blank line at the beginning of the expression
                            startLine--;
                        }
                    }

                    pragma = new CodeLinePragma(sourceLocation.FileName, startLine);
                }
            }
        }

        bool TryAlign(int alignment, ref string expressionText)
        {
            if (!this.IsVB && !this.IsCS)
            {
                return false;
            }

            StringBuilder builder = new StringBuilder();
            if (this.IsVB)
            {
                builder.Append('_');
            }
            builder.Append(Environment.NewLine);
            builder.Append(new string(' ', alignment - 1));
            builder.Append(expressionText);
            expressionText = builder.ToString();
            return true;
        }

        CodeLinePragma GenerateLinePragmaForNamespace(string namespaceName)
        {
            if (this.fileName != null)
            {
                // if source xaml file doesn't exist or it doesn't contain TextExpression
                // it defaults to line number 1
                int lineNumber = 1;
                Dictionary<string, int> lineNumberDictionary = this.settings.ForImplementation ? this.lineNumbersForNSesForImpl : this.lineNumbersForNSes;

                int lineNumReturend;
                if (lineNumberDictionary.TryGetValue(namespaceName, out lineNumReturend))
                {
                    lineNumber = lineNumReturend;
                }

                return new CodeLinePragma(this.fileName, lineNumber);
            }
            return null;
        }

        bool TryGetSymbols(Activity rootActivity, out Dictionary<object, SourceLocation> symbols, out string fileName)
        {
            symbols = null;
            fileName = null;
            Activity implementationRoot = null;
            if (this.settings.ForImplementation)
            {
                // Regular compilation case via XamlBuildTask or for DynamicActivity  
                // Debugger Symbols are attached to the first implementation child of rootActivity
                IEnumerable<Activity> children = WorkflowInspectionServices.GetActivities(rootActivity);
                foreach (Activity child in children)
                {
                    // Find the first implementation child of an activity
                    if (child.Id.Contains("."))
                    {
                        implementationRoot = child;
                        break;
                    }
                }
            }
            else
            {
                // XamlX case
                // Debugger Symbols are attached to WorkflowService.Body which is passed in the rootActivity
                implementationRoot = rootActivity;
            }

            if (implementationRoot == null)
            {
                return false;
            }

            try
            {
                string symbolString = DebugSymbol.GetSymbol(implementationRoot) as string;
                if (!string.IsNullOrEmpty(symbolString))
                {
                    WorkflowSymbol wfSymbol = WorkflowSymbol.Decode(symbolString);
                    if (wfSymbol != null)
                    {
                        symbols = SourceLocationProvider.GetSourceLocations(rootActivity, wfSymbol);
                        fileName = wfSymbol.FileName;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                // Ignore invalid symbols.
            }

            return false;
        }

        string GetActivityFullName(TextExpressionCompilerSettings settings)
        {
            string rootNamespacePrefix = null;
            string namespacePrefix = null;
            string activityFullName = "";
            if (this.IsVB && !String.IsNullOrWhiteSpace(this.settings.RootNamespace))
            {
                rootNamespacePrefix = this.settings.RootNamespace + ".";
            }

            if (!String.IsNullOrWhiteSpace(this.settings.ActivityNamespace))
            {
                namespacePrefix = this.settings.ActivityNamespace + ".";
            }

            if (rootNamespacePrefix != null)
            {
                if (namespacePrefix != null)
                {
                    activityFullName = rootNamespacePrefix + namespacePrefix + settings.ActivityName;
                }
                else
                {
                    activityFullName = rootNamespacePrefix + settings.ActivityName;
                }
            }
            else
            {
                if (namespacePrefix != null)
                {
                    activityFullName = namespacePrefix + settings.ActivityName;
                }
                else
                {
                    activityFullName = settings.ActivityName;
                }
            }

            return activityFullName;
        }

        class ExpressionCompilerActivityVisitor : CompiledExpressionActivityVisitor
        {
            TextExpressionCompiler compiler;

            public ExpressionCompilerActivityVisitor(TextExpressionCompiler compiler)
            {
                this.compiler = compiler;
            }

            public int NextExpressionId
            {
                get;
                set;
            }

            protected override void Visit(Activity activity, out bool exit)
            {
                base.Visit(activity, out exit);
            }

            protected override void VisitRoot(Activity activity, out bool exit)
            {
                this.compiler.OnRootActivity();
                                
                base.VisitRoot(activity, out exit);
                
                this.compiler.OnAfterRootActivity();
            }

            protected override void VisitRootImplementationArguments(Activity activity, out bool exit)
            {
                base.VisitRootImplementationArguments(activity, out exit);

                if (this.ForImplementation)
                {
                    this.compiler.OnAfterRootArguments(activity);
                }
            }
            
            protected override void VisitVariableScope(Activity activity, out bool exit)
            {
                this.compiler.OnVariableScope(activity);

                base.VisitVariableScope(activity, out exit);
                this.compiler.OnAfterVariableScope();
            }

            protected override void VisitRootImplementationScope(Activity activity, out bool exit)
            {
                CompiledDataContextDescriptor rootArgumentAccessorContext = null;
                this.compiler.OnRootImplementationScope(activity, out rootArgumentAccessorContext);

                base.VisitRootImplementationScope(activity, out exit);

                this.compiler.OnAfterRootImplementationScope(activity, rootArgumentAccessorContext);
            }

            protected override void VisitVariableScopeArgument(RuntimeArgument runtimeArgument, out bool exit)
            {
                this.compiler.InVariableScopeArgument = true;
                base.VisitVariableScopeArgument(runtimeArgument, out exit);
                this.compiler.InVariableScopeArgument = false;
            }

            protected override void VisitITextExpression(Activity activity, out bool exit)
            {
                this.compiler.OnITextExpressionFound(activity, this);
                exit = false;
            }

            protected override void VisitDelegate(ActivityDelegate activityDelegate, out bool exit)
            {
                this.compiler.OnActivityDelegateScope();
                                
                base.VisitDelegate(activityDelegate, out exit);

                this.compiler.OnAfterActivityDelegateScope();

                exit = false;
            }

            protected override void VisitDelegateArgument(RuntimeDelegateArgument delegateArgument, out bool exit)
            {
                this.compiler.OnDelegateArgument(delegateArgument);

                base.VisitDelegateArgument(delegateArgument, out exit);
            }  
        }

        class CompiledExpressionDescriptor
        {
            internal bool IsValue
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(this.GetMethodName) &&
                        string.IsNullOrWhiteSpace(this.SetMethodName) &&
                        string.IsNullOrWhiteSpace(this.StatementMethodName);
                }
            }

            internal bool IsReference
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(this.SetMethodName);
                }
            }

            internal bool IsStatement
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(this.StatementMethodName);
                }
            }

            internal string TypeName
            {
                get;
                set;
            }

            internal Type ResultType
            {
                get;
                set;
            }

            internal string GetMethodName
            {
                get;
                set;
            }

            internal string SetMethodName
            {
                get;
                set;
            }

            internal string StatementMethodName
            {
                get;
                set;
            }

            internal int Id
            {
                get;
                set;
            }

            internal string ExpressionText
            {
                get;
                set;
            }
            
            internal string GetExpressionTreeMethodName
            {
                get;
                set;
            }
        }

        class CompiledDataContextDescriptor
        {
            IDictionary<string, MemberData> fields;
            IDictionary<string, MemberData> properties;
            ISet<string> duplicates;
            Func<bool> isVb;

            public CompiledDataContextDescriptor(Func<bool> isVb)
            {
                this.isVb = isVb;
            }
                        
            public IDictionary<string, MemberData> Fields
            {
                get
                {
                    if (this.fields == null)
                    {
                        if (this.isVb())
                        {
                            this.fields = new Dictionary<string, MemberData>(StringComparer.OrdinalIgnoreCase);
                        }
                        else
                        {
                            this.fields = new Dictionary<string, MemberData>();
                        }
                    }
                    return this.fields;
                }
            }

            public IDictionary<string, MemberData> Properties
            {
                get
                {
                    if (this.properties == null)
                    {
                        if (this.isVb())
                        {
                            this.properties = new Dictionary<string, MemberData>(StringComparer.OrdinalIgnoreCase);
                        }
                        else
                        {
                            this.properties = new Dictionary<string, MemberData>();
                        }
                    }
                    return this.properties;
                }
            }

            public ISet<string> Duplicates
            {
                get
                {
                    if (this.duplicates == null)
                    {
                        if (this.isVb())
                        {
                            this.duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }
                        else
                        {
                            this.duplicates = new HashSet<string>();
                        }                            
                    }
                    return this.duplicates;
                }
            }

            public CodeTypeDeclaration CodeTypeDeclaration
            {
                get;
                set;
            }

            public CodeTypeDeclaration CodeTypeDeclarationForReadOnly
            {
                get;
                set;
            }

            public int NextMemberIndex
            {
                get;
                set;
            }
        }

        struct MemberData
        {
            public int Index;
            public string Name;
            public Type Type;
        }
    }
}
