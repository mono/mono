// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities.Common;

#region Grammar
//
// Grammar (left-factored with empty productions removed):
// ----------------------------------------------------------
// 
// condition    -->     binary-expression
//
// binary-expression    --> unary-expresssion binary-expression-tail
//                      --> unary-expression
//
// binary-expression-tail --> binary-operator-precedence unary-expression binary-expression-tail
//                        --> binary-operator-precedence unary-expression
//
// binary-operator-precedence   --> 0:{ ||  OR }
//                              --> 1:{ &&  AND }
//                              --> 2:{ | }
//                              --> 3:{ & }
//                              --> 4:{ == != }
//                              --> 5:{ <  >  <=  >= }
//                              --> 6:{ +  - }
//                              --> 7:{ *  /  %  MOD }
//              
// unary-expression     -->     unary-operator unary-expression
//                      -->     postfix-expression
//              
// unary-operator       --> -
//                      --> !
//                      --> NOT
//                      --> ( type-spec )
//
// postfix-expression   --> primary-expression postfix-expression-tail
//                      --> primary-expression
//
// postfix-expression-tail  --> postfix-operator postfix-expression-tail
//                          --> postfix-operator
//
// postfix-operator --> member-operator
//                  --> element-operator
//
// member-operator --> . IDENTIFIER method-call-arguments
//                 --> . IDENTIFIER
//
// element-operator --> [  expression-list  ]
//
// expression-list --> logical-expression  expression-list-tail
//                 --> logical-expression
//
// expression-list-tail --> ,  logical-expression  expression-list-tail
//                      --> ,  logical-expression
//              
// method-call-arguments    --> ( argument-list )
//                          --> ( )
//
// argument-list    --> argument argument-list-tail
//                  --> argument
//
// argument-list-tail   --> , argument argument-list-tail
//                      --> , argument
//              
// argument     -->     direction logical-expression
//              -->     logical-expression
//              
// direction    -->     IN
//              -->     OUT 
//              -->     REF 
//              
// primary-expression --> ( logical-expression )
//                    --> IDENTIFIER
//                    --> IDENTIFIER  method-call-arguments
//                    --> type-name
//                    --> object-creation-expression
//                    --> array-creation-expression
//                    --> THIS
//                    --> integer-constant
//                    --> decimal-constant
//                    --> float-constant
//                    --> character-constant
//                    --> string-constant
//                    --> TRUE
//                    --> FALSE
//                    --> NULL
//
// object-creation-expression --> NEW type-name method-call-arguments
//
// array-creation-expression --> NEW array-spec
//                           --> NEW array-spec array-initializer
//
// array-spec --> type-name  array-rank-specifiers
//
// array-rank-specifiers -->  [  binary-expression  ]
//                       -->  [  ]
//
// array-initializer --> {  variable-initializer-list  }
//                       {  }
//
// variable-initializer-list --> variable-initializer variable-initializer-list-tail
//                           --> variable-initializer
//
// variable-initializer-list-tail --> , variable-initializer variable-initializer-list-tail
//                                --> , variable-initializer
//
// variable-initializer --> binary-expression
//
// type-spec --> type-name
//           --> type-name  rank-specifiers
//
// rank-specifiers --> rank-specifier  rank-specifier-tail
//                 --> rank-specifier
//
// rank-specifier-tail -->  rank-specifier  rank-specifier-tail
//
// rank-specifier --> [  dim-separators  ]
//                --> [  ]
//
// dim-separators --> ,  dim-separators-tail
//                --> ,
//
// dim-separators-tail --> ,  dim-separators-tail
//
// type-name    --> CHAR
//              --> BYTE
//              --> SBYTE
//              --> SHORT
//              --> USHORT
//              --> INT
//              --> UINT
//              --> LONG
//              --> ULONG
//              --> FLOAT
//              --> DOUBLE
//              --> DECIMAL
//              --> BOOL
//              --> STRING
//              --> namespace-qualified-type-name
//
// namespace-qualified-type-name --> NAMESPACE-NAME namespace-qualifier-tail . TYPE-NAME
//                               --> NAMESPACE-NAME . TYPE-NAME
//                               --> TYPE-NAME
//
// namespace-qualifier-tail --> . NAMESPACE-NAME namespace-qualifier-tail
//
// statement-list --> statement statement-list-tail
//                --> statement
//
// statement-list-tail --> statement statement-list-tail
//                     --> statement
//
// statement    --> assign-statement
//              --> update-statement
//              --> HALT
//
// assign-statement --> postfix-expression ASSIGN logical-expression
//                  --> postfix-expression
//
// update-statement --> UPDATE ( "path" )
//                  --> UPDATE postfix-expression
#endregion

namespace System.Workflow.Activities.Rules
{
    #region ParserContext class
    internal class ParserContext
    {
        private List<Token> tokens;
        private int currentToken;

        internal Dictionary<object, int> exprPositions = new Dictionary<object, int>();
        internal bool provideIntellisense;
        internal ICollection completions;

        internal ParserContext(string expressionString)
        {
            Scanner scanner = new Scanner(expressionString);
            tokens = new List<Token>();
            scanner.Tokenize(tokens);
        }

        internal ParserContext(List<Token> tokens)
        {
            this.provideIntellisense = true;
            this.tokens = tokens;
        }

        #region Token methods
        internal Token CurrentToken
        {
            get { return (currentToken < tokens.Count) ? tokens[currentToken] : null; }
        }

        internal Token NextToken()
        {
            if (currentToken == tokens.Count - 1)
            {
                ++currentToken; // point one past the end.
                return null;
            }

            ++currentToken;
            return tokens[currentToken];
        }

        internal int SaveCurrentToken()
        {
            return currentToken;
        }
        internal void RestoreCurrentToken(int tokenValue)
        {
            currentToken = tokenValue;
        }
        #endregion

        #region Intellisense methods

        internal void SetNamespaceCompletions(NamespaceSymbol nsSym)
        {
            completions = nsSym.GetMembers();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal void SetTypeMemberCompletions(Type computedType, Type thisType, bool isStatic, RuleValidation validation)
        {
            BindingFlags flags = BindingFlags.Public;
            if (isStatic)
                flags |= BindingFlags.Static | BindingFlags.FlattenHierarchy;
            else
                flags |= BindingFlags.Instance;
            if (computedType.Assembly == thisType.Assembly)
                flags |= BindingFlags.NonPublic;

            // Initialize the list with the computed type's members.
            List<MemberInfo> members = new List<MemberInfo>(computedType.GetMembers(flags));
            if (computedType.IsInterface)
            {
                // If it's an interface, we need to chase up the parents and add their members too.
                List<Type> baseInterfaces = new List<Type>(computedType.GetInterfaces());
                for (int i = 0; i < baseInterfaces.Count; ++i)
                {
                    Type baseInterface = baseInterfaces[i];
                    baseInterfaces.AddRange(baseInterface.GetInterfaces());
                    members.AddRange(baseInterface.GetMembers(flags));
                }

                // Finally, we need to add members of System.Object, since all types intrinsically
                // derive from that.
                members.AddRange(typeof(object).GetMembers(flags));
            }

            // add in any extension methods that may be applicable to this type
            List<ExtensionMethodInfo> ext = validation.ExtensionMethods;
            foreach (ExtensionMethodInfo extension in ext)
            {
                ValidationError error;
                if (RuleValidation.TypesAreAssignable(computedType, extension.AssumedDeclaringType, null, out error))
                {
                    members.Add(extension);
                }
            }

            // Filter out the duplicates & special names.
            Dictionary<string, MemberInfo> filteredMembers = new Dictionary<string, MemberInfo>();
            foreach (MemberInfo member in members)
            {
                if (member == null)
                    continue;

                switch (member.MemberType)
                {
                    case MemberTypes.Method:
                        MethodInfo method = (MethodInfo)member;

                        // If method, exclude special names & generic methods.
                        if (!method.IsSpecialName && !method.IsGenericMethod)
                        {
                            // Add all members of this's type, but only non-private members
                            // of other types.
                            if (method.DeclaringType == thisType || IsNonPrivate(method, thisType) || (method is ExtensionMethodInfo))
                                filteredMembers[member.Name] = member;
                        }
                        break;

                    case MemberTypes.NestedType:
                    case MemberTypes.TypeInfo:
                        // Only add nested types if "isStatic" is true.
                        if (isStatic)
                        {
                            if (member.DeclaringType == thisType || IsNonPrivate((Type)member, thisType))
                            {
                                filteredMembers[member.Name] = member;
                            }
                        }
                        break;

                    case MemberTypes.Field:
                        // Add all members of this's type, but only non-private members
                        // of other types.
                        if (member.DeclaringType == thisType || IsNonPrivate((FieldInfo)member, thisType))
                            filteredMembers[member.Name] = member;
                        break;

                    case MemberTypes.Property:
                        PropertyInfo prop = (PropertyInfo)member;
                        ParameterInfo[] propParams = prop.GetIndexParameters();
                        if (propParams != null && propParams.Length > 0)
                        {
                            // If the property has arguments, it can only be accessed by directly calling
                            // its accessor methods.
                            MethodInfo[] accessors = prop.GetAccessors((flags & BindingFlags.NonPublic) != 0);
                            foreach (MethodInfo accessor in accessors)
                            {
                                if (accessor.DeclaringType == thisType || IsNonPrivate(accessor, thisType))
                                    filteredMembers[accessor.Name] = accessor;
                            }
                        }
                        else
                        {
                            if (member.DeclaringType == thisType)
                            {
                                // It's a property on "this", so add it even if it's private.
                                filteredMembers[member.Name] = member;
                            }
                            else
                            {
                                // Add the property if at least one of its accessors is non-private.
                                MethodInfo[] accessors = prop.GetAccessors((flags & BindingFlags.NonPublic) != 0);
                                foreach (MethodInfo accessor in accessors)
                                {
                                    if (IsNonPrivate(accessor, thisType))
                                    {
                                        filteredMembers[member.Name] = member;
                                        break;
                                    }
                                }
                            }
                        }
                        break;

                    default:
                        // Don't add constructors or other non-method/field/property things
                        // to the completion list.
                        break;
                }
            }

            completions = filteredMembers.Values;
        }

        internal void SetConstructorCompletions(Type computedType, Type thisType)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;
            if (computedType.Assembly == thisType.Assembly)
                flags |= BindingFlags.NonPublic;

            // Initialize the list with the computed type's members.
            List<Type> types = new List<Type>(1);
            types.Add(computedType);
            completions = RuleValidation.GetConstructors(types, flags);
        }

        internal void SetNestedClassCompletions(Type computedType, Type thisType)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;
            if (computedType.Assembly == thisType.Assembly)
                flags |= BindingFlags.NonPublic;

            // Initialize the list with the computed type's members (no need for interfaces or extension methods)
            List<MemberInfo> members = new List<MemberInfo>(computedType.GetMembers(flags));

            // Filter out the duplicates & special names.
            Dictionary<string, MemberInfo> filteredMembers = new Dictionary<string, MemberInfo>();
            foreach (MemberInfo member in members)
            {
                if (member == null)
                    continue;

                switch (member.MemberType)
                {
                    case MemberTypes.NestedType:
                    case MemberTypes.TypeInfo:
                        if (member.DeclaringType == thisType || IsNonPrivate((Type)member, thisType))
                        {
                            filteredMembers[member.Name] = member;
                        }
                        break;

                    default:
                        // Don't add constructors/method/field/property things to the completion list.
                        break;
                }
            }

            completions = filteredMembers.Values;
        }

        internal void SetMethodCompletions(Type computedType, Type thisType, string methodName, bool includeStatic, bool includeInstance, RuleValidation validation)
        {
            BindingFlags flags = BindingFlags.Public;
            if (computedType.Assembly == thisType.Assembly)
                flags |= BindingFlags.NonPublic;
            if (includeInstance)
                flags |= BindingFlags.Instance;
            if (includeStatic)
                flags |= BindingFlags.Static | BindingFlags.FlattenHierarchy;

            List<MemberInfo> candidateMethods = new List<MemberInfo>();

            MemberInfo[] methods = computedType.GetMember(methodName, MemberTypes.Method, flags);
            AddCandidates(candidateMethods, methods);

            if (computedType.IsInterface)
            {
                List<Type> parentInterfaces = new List<Type>();
                parentInterfaces.AddRange(computedType.GetInterfaces());

                for (int i = 0; i < parentInterfaces.Count; ++i)
                {
                    methods = parentInterfaces[i].GetMember(methodName, MemberTypes.Method, flags);
                    AddCandidates(candidateMethods, methods);

                    Type[] pInterfaces = parentInterfaces[i].GetInterfaces();
                    if (pInterfaces.Length > 0)
                        parentInterfaces.AddRange(pInterfaces);
                }

                // Add members from System.Object as well.
                methods = typeof(object).GetMember(methodName, MemberTypes.Method, flags);
                AddCandidates(candidateMethods, methods);
            }

            // add in any extension methods
            List<ExtensionMethodInfo> ext = validation.ExtensionMethods;
            foreach (ExtensionMethodInfo extension in ext)
            {
                // does it have the right name and is the type compatible
                ValidationError error;
                if ((extension.Name == methodName) &&
                    (RuleValidation.TypesAreAssignable(computedType, extension.AssumedDeclaringType, null, out error)))
                {
                    candidateMethods.Add(extension);
                }
            }

            completions = candidateMethods;
        }

        private static void AddCandidates(List<MemberInfo> candidateMethods, MemberInfo[] methods)
        {
            if (methods != null)
            {
                for (int m = 0; m < methods.Length; ++m)
                {
                    System.Diagnostics.Debug.Assert(methods[m].MemberType == MemberTypes.Method, "expect methods only");
                    MethodInfo method = (MethodInfo)methods[m];

                    if (!method.IsGenericMethod) // Skip generic methods.
                        candidateMethods.Add(method);
                }
            }
        }

        internal static bool IsNonPrivate(
            MethodInfo methodInfo, Type thisType)
        {
            return methodInfo.IsPublic
                || methodInfo.IsFamily
                || methodInfo.IsFamilyOrAssembly
                || (methodInfo.IsAssembly || methodInfo.IsFamilyAndAssembly)
                && (methodInfo.DeclaringType.Assembly == thisType.Assembly);
        }

        internal static bool IsNonPrivate(
            FieldInfo fieldInfo, Type thisType)
        {
            return fieldInfo.IsPublic
                || fieldInfo.IsFamily
                || fieldInfo.IsFamilyOrAssembly
                || (fieldInfo.IsAssembly || fieldInfo.IsFamilyAndAssembly)
                && (fieldInfo.DeclaringType.Assembly == thisType.Assembly);
        }

        internal static bool IsNonPrivate(
            Type type, Type thisType)
        {
            return (type.IsPublic || type.IsNestedPublic
                || (type.IsNestedAssembly || type.IsNestedFamANDAssem || type.IsNestedFamORAssem)
                && (type.Assembly == thisType.Assembly));
        }

        internal int NumTokens
        {
            get { return tokens.Count; }
        }

        #endregion
    }
    #endregion

    internal class Parser
    {
        #region Binary Operator Precedence-Parsing Descriptors

        private class BinaryOperationDescriptor
        {
            private TokenID token;
            private CodeBinaryOperatorType codeDomOperator;

            internal BinaryOperationDescriptor(TokenID token, CodeBinaryOperatorType codeDomOperator)
            {
                this.token = token;
                this.codeDomOperator = codeDomOperator;
            }

            internal TokenID Token { get { return token; } }

            internal virtual CodeBinaryOperatorExpression CreateBinaryExpression(CodeExpression left, CodeExpression right, int operatorPosition, Parser parser, ParserContext parserContext, bool assignIsEquality)
            {
                CodeBinaryOperatorExpression binaryExpr = new CodeBinaryOperatorExpression(left, codeDomOperator, right);
                parserContext.exprPositions[binaryExpr] = operatorPosition;
                parser.ValidateExpression(parserContext, binaryExpr, assignIsEquality, ValueCheck.Read);
                return binaryExpr;
            }
        }

        private class NotEqualOperationDescriptor : BinaryOperationDescriptor
        {
            internal NotEqualOperationDescriptor(TokenID token)
                : base(token, CodeBinaryOperatorType.IdentityInequality) // kludge
            {
            }

            internal override CodeBinaryOperatorExpression CreateBinaryExpression(CodeExpression left, CodeExpression right, int operatorPosition, Parser parser, ParserContext parserContext, bool assignIsEquality)
            {
                CodePrimitiveExpression falseExpr = new CodePrimitiveExpression(false);
                parserContext.exprPositions[falseExpr] = operatorPosition;

                // Compare the comperands using "value-equality"
                CodeBinaryOperatorExpression binaryExpr = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, right);
                parserContext.exprPositions[binaryExpr] = operatorPosition;

                // Compare the result of that with false to simulate "value-inequality"
                binaryExpr = new CodeBinaryOperatorExpression(binaryExpr, CodeBinaryOperatorType.ValueEquality, falseExpr);
                parserContext.exprPositions[binaryExpr] = operatorPosition;
                parser.ValidateExpression(parserContext, binaryExpr, assignIsEquality, ValueCheck.Read);

                return binaryExpr;
            }
        }

        private class BinaryPrecedenceDescriptor
        {
            private BinaryOperationDescriptor[] operations;

            internal BinaryPrecedenceDescriptor(params BinaryOperationDescriptor[] operations)
            {
                this.operations = operations;
            }

            internal BinaryOperationDescriptor FindOperation(TokenID token)
            {
                foreach (BinaryOperationDescriptor operation in operations)
                {
                    if (operation.Token == token)
                        return operation;
                }

                return null;
            }
        }

        private static readonly BinaryPrecedenceDescriptor[] precedences = new BinaryPrecedenceDescriptor[] {
            new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor(TokenID.Or, CodeBinaryOperatorType.BooleanOr)),
            new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor(TokenID.And, CodeBinaryOperatorType.BooleanAnd)),

            new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor(TokenID.BitOr, CodeBinaryOperatorType.BitwiseOr)),

            new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor(TokenID.BitAnd, CodeBinaryOperatorType.BitwiseAnd)),

            new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor(TokenID.Equal, CodeBinaryOperatorType.ValueEquality),
                                           new BinaryOperationDescriptor(TokenID.Assign, CodeBinaryOperatorType.ValueEquality),
                                           new NotEqualOperationDescriptor(TokenID.NotEqual)),

            new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor(TokenID.Less, CodeBinaryOperatorType.LessThan),
                                           new BinaryOperationDescriptor(TokenID.LessEqual, CodeBinaryOperatorType.LessThanOrEqual),
                                           new BinaryOperationDescriptor(TokenID.Greater, CodeBinaryOperatorType.GreaterThan),
                                           new BinaryOperationDescriptor(TokenID.GreaterEqual, CodeBinaryOperatorType.GreaterThanOrEqual)),

            new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor(TokenID.Plus, CodeBinaryOperatorType.Add),
                                           new BinaryOperationDescriptor(TokenID.Minus, CodeBinaryOperatorType.Subtract)),

            new BinaryPrecedenceDescriptor(new BinaryOperationDescriptor(TokenID.Multiply, CodeBinaryOperatorType.Multiply),
                                           new BinaryOperationDescriptor(TokenID.Divide, CodeBinaryOperatorType.Divide),
                                           new BinaryOperationDescriptor(TokenID.Modulus, CodeBinaryOperatorType.Modulus))
        };

        #endregion

        // Data members
        private RuleValidation validation;
        private Dictionary<string, Symbol> globalUniqueSymbols = new Dictionary<string, Symbol>();
        private Dictionary<string, Symbol> localUniqueSymbols = new Dictionary<string, Symbol>();

        [Flags]
        enum ValueCheck
        {
            Unknown = 0,
            Read = 1,
            Write = 2
        }

        #region Constructor

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal Parser(RuleValidation validation)
        {
            this.validation = validation;

            Type[] allTypes = null;

            ITypeProvider provider = validation.GetTypeProvider();
            if (provider == null)
            {
                // No type provider.  The only type we know about is "This".
                //allTypes = new Type[] { validation.ThisType };
                try
                {
                    allTypes = validation.ThisType.Assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    // problems loading all the types, take what we can get
                    allTypes = e.Types;
                }
            }
            else
            {
                allTypes = provider.GetTypes();
            }


            // Go through all the known types and gather namespace information.
            // Also note which types are uniquely named; these can be looked up without
            // qualification.

            Dictionary<string, NamespaceSymbol> rootNamespaces = new Dictionary<string, NamespaceSymbol>();
            Dictionary<string, object> duplicateNames = new Dictionary<string, object>();
            NamespaceSymbol nsSym = null;
            Symbol existingSymbol = null;
            NamespaceSymbol globalNS = null; // In case we encounter a type without a namespace

            for (int i = 0; i < allTypes.Length; ++i)
            {
                Type type = allTypes[i];

                // If we got a ReflectionTypeLoadException, some types may be null, so skip them
                if (type == null)
                    continue;

                // Skip types that are not visible.
                // (If type.Assembly == null, we assume it's a design-time type, and let it through.)
                if (type.IsNotPublic && (type.Assembly != null && type.Assembly != validation.ThisType.Assembly))
                    continue;

                // Skip nested types.
                if (type.IsNested)
                    continue;

                // Add the namespaces.
                string typeNamespace = type.Namespace;
                if (string.IsNullOrEmpty(typeNamespace))
                {
                    if (globalNS == null)
                    {
                        globalNS = new NamespaceSymbol();
                        rootNamespaces.Add("", globalNS);
                    }

                    nsSym = globalNS;
                }
                else
                {
                    string[] namespaces = typeNamespace.Split('.');
                    System.Diagnostics.Debug.Assert(namespaces.Length > 0);

                    if (!rootNamespaces.TryGetValue(namespaces[0], out nsSym))
                    {
                        nsSym = new NamespaceSymbol(namespaces[0], null);
                        rootNamespaces.Add(namespaces[0], nsSym);

                        // Also add the root namespace to the global unique symbol dictionary.
                        // Replace anything that was there.  I.e., we had MS.Test.Foo,
                        // and this current one is Test.Bar.  It wins.
                        globalUniqueSymbols[namespaces[0]] = nsSym;
                    }

                    if (namespaces.Length > 1)
                    {
                        for (int j = 1; j < namespaces.Length; ++j)
                        {
                            nsSym = nsSym.AddNamespace(namespaces[j]);

                            if (globalUniqueSymbols.TryGetValue(namespaces[j], out existingSymbol))
                            {
                                // This sub-namespace is already in global unique symbols.

                                // If it's the same one as what's there, no problem.
                                NamespaceSymbol existingNS = existingSymbol as NamespaceSymbol;
                                if (existingNS != null && existingNS.Parent != nsSym.Parent)
                                {
                                    // It was different.  If the levels are the same, it's a duplicate name.
                                    if (existingNS.Level == nsSym.Level)
                                    {
                                        duplicateNames[namespaces[j]] = null;
                                    }
                                    else
                                    {
                                        // If the new one is at a lower level than the existing one,
                                        // replace it.  Otherwise, leave the existing one there.
                                        if (nsSym.Level < existingNS.Level)
                                            globalUniqueSymbols[namespaces[j]] = nsSym;
                                    }
                                }
                            }
                            else
                            {
                                globalUniqueSymbols.Add(namespaces[j], nsSym);
                            }
                        }
                    }
                }

                // Add the type to its namespace.
                nsSym.AddType(type);
            }

            // Remove non-unique namespaces.
            foreach (string name in duplicateNames.Keys)
                globalUniqueSymbols.Remove(name);

            Queue<NamespaceSymbol> nsQueue = new Queue<NamespaceSymbol>();
            foreach (NamespaceSymbol rootNS in rootNamespaces.Values)
                nsQueue.Enqueue(rootNS);

            // Add the unique types as well.
            duplicateNames.Clear();
            while (nsQueue.Count > 0)
            {
                nsSym = nsQueue.Dequeue();

                foreach (Symbol nestedSym in nsSym.NestedSymbols.Values)
                {
                    NamespaceSymbol nestedNS = nestedSym as NamespaceSymbol;
                    if (nestedNS != null)
                    {
                        nsQueue.Enqueue(nestedNS);
                    }
                    else
                    {
                        string name = nestedSym.Name;

                        if (globalUniqueSymbols.TryGetValue(name, out existingSymbol))
                        {
                            // Found an existing one with the same name.
                            if (existingSymbol is NamespaceSymbol)
                            {
                                // A type name matches a namespace name... namespace wins.
                                continue;
                            }
                            else
                            {
                                TypeSymbolBase existingTypeSymBase = (TypeSymbolBase)existingSymbol;
                                TypeSymbolBase typeSymBase = (TypeSymbolBase)nestedSym;
                                OverloadedTypeSymbol overloadSym = existingTypeSymBase.OverloadType(typeSymBase);
                                if (overloadSym == null)
                                    duplicateNames[name] = null; // Couldn't overload it.
                                else
                                    globalUniqueSymbols[name] = overloadSym;
                            }
                        }
                        else
                        {
                            globalUniqueSymbols.Add(name, nestedSym);
                        }
                    }
                }
            }

            // Remove non-unique types.
            foreach (string name in duplicateNames.Keys)
                globalUniqueSymbols.Remove(name);


            // Finally, deal with the members of "this".
            //
            // Nested types override/hide items in the global unique symbols list.
            //
            // All other members get added to the local unique symbols list.  In most
            // contexts, these will override (replace, hide) any global symbols with the same name.
            // In contexts where the parser is only looking for types and/or namespaces, local
            // symbols do NOT hide global ones.
            Type thisType = validation.ThisType;
            MemberInfo[] members = thisType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (MemberInfo mi in members)
            {
                switch (mi.MemberType)
                {
                    case MemberTypes.Field:
                        if (mi.DeclaringType == thisType || ParserContext.IsNonPrivate((FieldInfo)mi, thisType))
                            localUniqueSymbols[mi.Name] = new MemberSymbol(mi);
                        break;

                    case MemberTypes.Property:
                        PropertyInfo prop = (PropertyInfo)mi;
                        ParameterInfo[] propParams = prop.GetIndexParameters();
                        if (propParams != null && propParams.Length > 0)
                        {
                            // If the property has arguments, it can only be accessed by directly calling
                            // its accessor methods.
                            MethodInfo[] accessors = prop.GetAccessors(true);
                            foreach (MethodInfo accessor in accessors)
                            {
                                if (accessor.DeclaringType == thisType || ParserContext.IsNonPrivate(accessor, thisType))
                                    localUniqueSymbols[mi.Name] = new MemberSymbol(accessor);
                            }
                        }
                        else
                        {
                            if (mi.DeclaringType == thisType)
                            {
                                // It's a property on "this", so add it even if it's private.
                                localUniqueSymbols[mi.Name] = new MemberSymbol(mi);
                            }
                            else
                            {
                                // Add the property if at least one of its accessors is non-private.
                                MethodInfo[] accessors = prop.GetAccessors(true);
                                foreach (MethodInfo accessor in accessors)
                                {
                                    if (ParserContext.IsNonPrivate(accessor, thisType))
                                    {
                                        localUniqueSymbols[mi.Name] = new MemberSymbol(mi);
                                        break;
                                    }
                                }
                            }
                        }
                        break;

                    case MemberTypes.Method:
                        MethodInfo method = (MethodInfo)mi;
                        if (!method.IsSpecialName && !method.IsGenericMethod)
                        {
                            if (mi.DeclaringType == thisType || ParserContext.IsNonPrivate(method, thisType))
                            {
                                // These simply hide anything else of the same name.
                                localUniqueSymbols[mi.Name] = new MemberSymbol(mi);
                            }
                        }
                        break;

                    case MemberTypes.NestedType:
                    case MemberTypes.TypeInfo: // Same thing but only happens with DesignTimeTypes
                        // These can overload or hide global unique symbols.
                        Type miType = (Type)mi;
                        TypeSymbol memberSym = new TypeSymbol(miType);
                        if (globalUniqueSymbols.TryGetValue(memberSym.Name, out existingSymbol))
                        {
                            TypeSymbolBase existingTypeSymBase = existingSymbol as TypeSymbolBase;
                            if (existingTypeSymBase != null)
                            {
                                // Try to overload.
                                OverloadedTypeSymbol overloadSym = existingTypeSymBase.OverloadType(memberSym);
                                if (overloadSym == null)
                                {
                                    if (mi.DeclaringType == thisType || ParserContext.IsNonPrivate(miType, thisType))
                                    {
                                        // We couldn't overload it, so hide it.
                                        globalUniqueSymbols[memberSym.Name] = memberSym;
                                    }
                                }
                                else if (mi.DeclaringType == thisType || ParserContext.IsNonPrivate(miType, thisType))
                                {
                                    globalUniqueSymbols[memberSym.Name] = overloadSym;
                                }
                            }
                            else
                            {
                                // The name clashed with something that wasn't a type name.
                                // Hide the outer one.
                                if (mi.DeclaringType == thisType || ParserContext.IsNonPrivate((Type)mi, thisType))
                                    globalUniqueSymbols[memberSym.Name] = memberSym;
                            }
                        }
                        else
                        {
                            if (mi.DeclaringType == thisType || ParserContext.IsNonPrivate(miType, thisType))
                            {
                                globalUniqueSymbols[memberSym.Name] = memberSym;
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }
        #endregion


        private RuleValidation Validator
        {
            get { return validation; }
        }

        #region Intellisense Methods

        internal ICollection GetExpressionCompletions(string expressionString)
        {
            try
            {
                IntellisenseParser intellisenseParser = new IntellisenseParser(expressionString);
                ParserContext parserContext = intellisenseParser.BackParse();
                if (parserContext != null)
                {
                    Token token = parserContext.CurrentToken;

                    // Check to see if the only relevant token (there's always an EndOfInput padded
                    // at the end) is an identifier with only one character.
                    if (parserContext.NumTokens == 2 && token.TokenID == TokenID.Identifier)
                    {
                        string ident = (string)token.Value;
                        System.Diagnostics.Debug.Assert(parserContext.NextToken().TokenID == TokenID.EndOfInput);

                        if (ident.Length == 1)
                        {
                            // The postfix expression consisted of a single character which was the beginning
                            // of an identifier or keyword.  Don't parse anything; just return all the root completions.
                            return GetRootCompletions(ident[0]);
                        }

                        // Otherwise, we don't do anything.
                    }
                    else
                    {
                        // We have a set of tokens we need to parse to figure out what's going on.
                        validation.Errors.Clear();

                        ParsePostfixExpression(parserContext, true, ValueCheck.Read);
                        return parserContext.completions;
                    }
                }
            }
            catch (RuleSyntaxException ex)
            {
                // Just ignore these, but when this happens, the completion list will be null.
                if (ex.ErrorNumber != 0)
                    return null;
            }

            return null;
        }

        private ICollection GetRootCompletions(char firstCharacter)
        {
            ArrayList rootCompletions = new ArrayList();

            char upperFirstCharacter = char.ToUpper(firstCharacter, CultureInfo.InvariantCulture);

            // Find all the global namespaces & types that start with the first character.
            foreach (KeyValuePair<string, Symbol> kvp in globalUniqueSymbols)
            {
                string key = kvp.Key;
                if (char.ToUpper(key[0], CultureInfo.InvariantCulture) == upperFirstCharacter)
                {
                    // Add this to the root completions, but only if it is NOT in the 'local' unique
                    // symbols.
                    Symbol localSym = null;
                    if (!localUniqueSymbols.TryGetValue(key, out localSym))
                        kvp.Value.RecordSymbol(rootCompletions);
                }
            }

            // Now add all local symbols that start with the first character.
            foreach (KeyValuePair<string, Symbol> kvp in localUniqueSymbols)
            {
                string key = kvp.Key;
                if (char.ToUpper(key[0], CultureInfo.InvariantCulture) == upperFirstCharacter)
                    kvp.Value.RecordSymbol(rootCompletions);
            }

            // Also add keywords.
            Scanner.AddKeywordsStartingWith(upperFirstCharacter, rootCompletions);

            return rootCompletions;
        }

        #endregion

        #region Condition & action parsing methods

        // Parse:
        //              condition --> logical-expression
        internal RuleExpressionCondition ParseCondition(string expressionString)
        {
            validation.Errors.Clear();
            ParserContext parserContext = new ParserContext(expressionString);

            if (parserContext.CurrentToken.TokenID == TokenID.EndOfInput)
                throw new RuleSyntaxException(ErrorNumbers.Error_EmptyExpression, Messages.Parser_EmptyExpression, parserContext.CurrentToken.StartPosition);

            CodeExpression exprResult = ParseBinaryExpression(parserContext, 0, true, ValueCheck.Read); //ParseLogicalExpression();

            if (parserContext.CurrentToken.TokenID != TokenID.EndOfInput)
                throw new RuleSyntaxException(ErrorNumbers.Error_ExtraCharactersIgnored, Messages.Parser_ExtraCharactersIgnored, parserContext.CurrentToken.StartPosition);

            if (exprResult == null)
                return null;

            RuleExpressionInfo exprInfo = validation.ExpressionInfo(exprResult);
            if (exprInfo == null)
                return null;

            Type resultType = exprInfo.ExpressionType;
            if (!RuleValidation.IsValidBooleanResult(resultType))
                throw new RuleSyntaxException(ErrorNumbers.Error_ConditionMustBeBoolean, Messages.ConditionMustBeBoolean, 0);

            return new RuleExpressionCondition(exprResult);
        }

        // Parse a single statement.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal RuleAction ParseSingleStatement(string statementString)
        {
            validation.Errors.Clear();
            ParserContext parserContext = new ParserContext(statementString);
            RuleAction result = ParseStatement(parserContext);
            if (parserContext.CurrentToken.TokenID != TokenID.EndOfInput)
                throw new RuleSyntaxException(ErrorNumbers.Error_ExtraCharactersIgnored, Messages.Parser_ExtraCharactersIgnored, parserContext.CurrentToken.StartPosition);

            return result;
        }

        internal List<RuleAction> ParseStatementList(string statementString)
        {
            validation.Errors.Clear();
            ParserContext parserContext = new ParserContext(statementString);
            return ParseStatements(parserContext);
        }

        #endregion

        // Parse:
        //      statement-list --> statement statement-list-tail
        //                     --> statement
        //
        //      statement-list-tail --> statement statement-list-tail
        //                          --> statement
        private List<RuleAction> ParseStatements(ParserContext parserContext)
        {
            List<RuleAction> statements = new List<RuleAction>();
            while (parserContext.CurrentToken.TokenID != TokenID.EndOfInput)
            {
                RuleAction statement = ParseStatement(parserContext);
                if (statement == null)
                    break;

                statements.Add(statement);

                // Eat any (optional) semi-colons. They aren't necessary but are comfortable
                // for a lot of programmers.
                while (parserContext.CurrentToken.TokenID == TokenID.Semicolon)
                    parserContext.NextToken();
            }

            return statements;
        }

        // Parse:
        //              statement       --> assign-statement
        //                              --> update-statement
        //                              --> HALT
        //
        //              update-statement --> UPDATE ( "path" )
        //                               --> UPDATE ( postfix-expr )
        private RuleAction ParseStatement(ParserContext parserContext)
        {
            RuleAction action = null;
            Token statementToken = parserContext.CurrentToken;
            if (statementToken.TokenID == TokenID.Halt)
            {
                parserContext.NextToken(); // eat the "halt"
                action = new RuleHaltAction();
                parserContext.exprPositions[action] = statementToken.StartPosition;
                ValidateAction(parserContext, action);
            }
            else if (statementToken.TokenID == TokenID.Update)
            {
                string message;

                parserContext.NextToken(); // eat the "update"

                if (parserContext.CurrentToken.TokenID != TokenID.LParen)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_MissingLparenAfterCommand, "UPDATE");
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingLparenAfterCommand, message, parserContext.CurrentToken.StartPosition);
                }

                parserContext.NextToken(); // Eat the "("

                string pathString = null;

                Token updateArgToken = parserContext.CurrentToken;
                if (updateArgToken.TokenID == TokenID.StringLiteral)
                {
                    // Treat UPDATE("foo/bar") as a literal path.
                    pathString = (string)updateArgToken.Value;
                    parserContext.NextToken(); // Eat the path string.
                }
                else
                {
                    CodeExpression pathExpr = ParsePostfixExpression(parserContext, true, ValueCheck.Read);

                    RuleAnalysis analysis = new RuleAnalysis(validation, true);
                    RuleExpressionWalker.AnalyzeUsage(analysis, pathExpr, false, true, null);
                    ICollection<string> paths = analysis.GetSymbols();

                    if (paths.Count == 0 || paths.Count > 1)
                    {
                        // The expression did not modify anything, or it modified more than one.
                        throw new RuleSyntaxException(ErrorNumbers.Error_InvalidUpdateExpression, Messages.Parser_InvalidUpdateExpression, updateArgToken.StartPosition);
                    }
                    else
                    {
                        IEnumerator<string> enumerator = paths.GetEnumerator();
                        enumerator.MoveNext();
                        pathString = enumerator.Current;
                    }
                }

                if (parserContext.CurrentToken.TokenID != TokenID.RParen)
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingRParenAfterArgumentList, Messages.Parser_MissingRParenAfterArgumentList, parserContext.CurrentToken.StartPosition);

                parserContext.NextToken(); // Eat the ")"

                action = new RuleUpdateAction((string)pathString);
                parserContext.exprPositions[action] = statementToken.StartPosition;
                ValidateAction(parserContext, action);
            }
            else
            {
                // Try to parse a custom RuleAction.

                int savedTokenState = parserContext.SaveCurrentToken();

                Type type = TryParseTypeSpecifier(parserContext, false);

                if (type != null &&
                    parserContext.CurrentToken.TokenID == TokenID.LParen &&
                    TypeProvider.IsAssignable(typeof(RuleAction), type))
                {
                    // The statement started with a "type (", and the type derived from RuleAction.
                    // This is a custom rule action.

                    int lparenPosition = parserContext.CurrentToken.StartPosition;
                    parserContext.NextToken(); // Eat the '('

                    List<CodeExpression> arguments = ParseArgumentList(parserContext);

                    action = (RuleAction)ConstructCustomType(type, arguments, lparenPosition);

                    parserContext.exprPositions[action] = statementToken.StartPosition;
                    ValidateAction(parserContext, action);
                }
                else
                {
                    // It wasn't a custom action.
                    // In some cases it may have looked like one up to a point, such as:
                    //
                    //      MyType.MyMember(
                    //
                    // but "MyMember" is a static method.

                    // Reset the scanner state, and re-parse as an assignment.
                    parserContext.RestoreCurrentToken(savedTokenState);

                    CodeStatement statement = ParseAssignmentStatement(parserContext);
                    if (statement != null)
                    {
                        // Create a rule statement action around it.  No need to validate it, as
                        // the underlying CodeDom statement has been validated already.
                        action = new RuleStatementAction(statement);
                    }
                }
            }

            return action;
        }

        // Parse:
        //      assign-statement --> postfix-expression ASSIGN logical-expression
        //                       --> postfix-expression
        private CodeStatement ParseAssignmentStatement(ParserContext parserContext)
        {
            CodeStatement result = null;

            // Parse the postfix-expression
            CodeExpression postfixExpr = ParsePostfixExpression(parserContext, false, ValueCheck.Read);

            // See if we need to parse the assignment statement.
            Token token = parserContext.CurrentToken;
            if (token.TokenID == TokenID.Assign)
            {
                int assignPosition = token.StartPosition;
                parserContext.NextToken(); // eat the '='

                CodeExpression rhsExpr = ParseBinaryExpression(parserContext, 0, true, ValueCheck.Read);

                result = new CodeAssignStatement(postfixExpr, rhsExpr);
                parserContext.exprPositions[result] = assignPosition;
            }
            else
            {
                result = new CodeExpressionStatement(postfixExpr);
                parserContext.exprPositions[result] = parserContext.exprPositions[postfixExpr];
            }

            ValidateStatement(parserContext, result);

            return result;
        }

        // Parse:
        //
        // binary-expression    --> unary-expresssion binary-expression-tail
        //                      --> unary-expression
        //
        // binary-expression-tail --> binary-operator-precedence unary-expression binary-expression-tail
        //                        --> binary-operator-precedence unary-expression
        //
        // binary-operator-precedence   --> 0:{ ||  OR }
        //                              --> 1:{ &&  AND }
        //                              --> 2:{ | }
        //                              --> 3:{ & }
        //                              --> 4:{ =  ==  != }
        //                              --> 5:{ <  >  <=  >= }
        //                              --> 6:{ +  - }
        //                              --> 7:{ *  /  %  MOD }
        //
        // This method is still recursive descent, but parses each precedence group by using the operator precedence
        // tables defined in this class.
        private CodeExpression ParseBinaryExpression(ParserContext parserContext, int precedence, bool assignIsEquality, ValueCheck check)
        {
            // Must parse at least one left-hand operand.
            CodeExpression leftResult = (precedence == precedences.Length - 1) ? ParseUnaryExpression(parserContext, assignIsEquality, check) : ParseBinaryExpression(parserContext, precedence + 1, assignIsEquality, check);
            if (leftResult != null)
            {
                for (;;)
                {
                    Token operatorToken = parserContext.CurrentToken;

                    BinaryPrecedenceDescriptor precedenceDescriptor = precedences[precedence];

                    BinaryOperationDescriptor operationDescriptor = precedenceDescriptor.FindOperation(operatorToken.TokenID);
                    if (operationDescriptor == null)
                        break; // we're finished; no applicable binary operator token at this precedence level.

                    parserContext.NextToken();

                    // Parse the right-hand side now.
                    CodeExpression rightResult = (precedence == precedences.Length - 1) ? ParseUnaryExpression(parserContext, true, check) : ParseBinaryExpression(parserContext, precedence + 1, true, check);

                    leftResult = operationDescriptor.CreateBinaryExpression(leftResult, rightResult, operatorToken.StartPosition, this, parserContext, assignIsEquality);
                }
            }

            return leftResult;
        }


        // Parse:
        //              unary-expression --> unary-operator unary-expression
        //                               --> postfix-expression
        private CodeExpression ParseUnaryExpression(ParserContext parserContext, bool assignIsEquality, ValueCheck check)
        {
            Token currentToken = parserContext.CurrentToken;

            CodeExpression unaryResult = null;
            if (currentToken.TokenID == TokenID.Not)
            {
                int notPosition = currentToken.StartPosition;
                parserContext.NextToken();

                unaryResult = ParseUnaryExpression(parserContext, true, check);

                // This becomes "subExpr == false"
                unaryResult = new CodeBinaryOperatorExpression(unaryResult, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
                parserContext.exprPositions[unaryResult] = notPosition;
                ValidateExpression(parserContext, unaryResult, assignIsEquality, check);
            }
            else if (currentToken.TokenID == TokenID.Minus)
            {
                int negativePosition = currentToken.StartPosition;
                parserContext.NextToken();

                unaryResult = ParseUnaryExpression(parserContext, true, check);

                // This becomes "0 - subExpr"
                unaryResult = new CodeBinaryOperatorExpression(new CodePrimitiveExpression(0), CodeBinaryOperatorType.Subtract, unaryResult);
                parserContext.exprPositions[unaryResult] = negativePosition;
                ValidateExpression(parserContext, unaryResult, assignIsEquality, check);
            }
            else if (currentToken.TokenID == TokenID.LParen)
            {
                int lparenPosition = currentToken.StartPosition;

                // Save the state.  This may actually be a parenthesized subexpression.
                int savedTokenState = parserContext.SaveCurrentToken();

                currentToken = parserContext.NextToken(); // Eat the '('

                Type type = TryParseTypeSpecifier(parserContext, assignIsEquality);

                if (type == null || parserContext.CurrentToken.TokenID != TokenID.RParen)
                {
                    // It wasn't a cast.
                    // In some cases it may have looked like a cast up to a point, such as:
                    //
                    //      (MyType.MyMember
                    //
                    // but "MyMember" is a static field, property, or enum.

                    // Reset the scanner state, and re-parse as a postfix-expr
                    parserContext.RestoreCurrentToken(savedTokenState);
                    unaryResult = ParsePostfixExpression(parserContext, assignIsEquality, check);
                }
                else
                {
                    // It is a cast.  It must have a balancing ')'.
                    if (parserContext.CurrentToken.TokenID != TokenID.RParen)
                        throw new RuleSyntaxException(ErrorNumbers.Error_MissingRParenInSubexpression, Messages.Parser_MissingRParenInSubexpression, parserContext.CurrentToken.StartPosition);

                    parserContext.NextToken();

                    unaryResult = ParseUnaryExpression(parserContext, true, check);

                    CodeTypeReference typeRef = new CodeTypeReference(type);
                    validation.AddTypeReference(typeRef, type);

                    unaryResult = new CodeCastExpression(typeRef, unaryResult);
                    parserContext.exprPositions[unaryResult] = lparenPosition;
                    ValidateExpression(parserContext, unaryResult, assignIsEquality, check);
                }
            }
            else
            {
                unaryResult = ParsePostfixExpression(parserContext, assignIsEquality, check);
            }

            return unaryResult;
        }

        // Parse:
        //              postfix-expression --> primary-expression postfix-expression-tail
        //                                 --> primary-expression
        //
        //              postfix-expression-tail --> postfix-operator postfix-expression-tail
        //                                      --> postfix-operator
        //
        //              postfix-operator --> member-operator
        //                               --> element-operator
        private CodeExpression ParsePostfixExpression(ParserContext parserContext, bool assignIsEquality, ValueCheck check)
        {
            CodeExpression resultExpr = ParsePrimaryExpression(parserContext, assignIsEquality);

            CodeExpression postfixExpr = TryParsePostfixOperator(parserContext, resultExpr, assignIsEquality, check);
            while (postfixExpr != null)
            {
                resultExpr = postfixExpr;
                postfixExpr = TryParsePostfixOperator(parserContext, resultExpr, assignIsEquality, check);
            }

            return resultExpr;
        }

        // Parse:
        //              postfix-operator --> member-operator
        //                               --> element-operator
        private CodeExpression TryParsePostfixOperator(ParserContext parserContext, CodeExpression primaryExpr, bool assignIsEquality, ValueCheck check)
        {
            CodeExpression postfixExpr = null;

            if (parserContext.CurrentToken.TokenID == TokenID.Dot)
            {
                postfixExpr = ParseMemberOperator(parserContext, primaryExpr, assignIsEquality, check);
            }
            else if (parserContext.CurrentToken.TokenID == TokenID.LBracket)
            {
                postfixExpr = ParseElementOperator(parserContext, primaryExpr, assignIsEquality);
            }

            return postfixExpr;
        }

        // Parse:
        //              element-operator --> [  expression-list  ]
        private CodeExpression ParseElementOperator(ParserContext parserContext, CodeExpression primaryExpr, bool assignIsEquality)
        {
            System.Diagnostics.Debug.Assert(parserContext.CurrentToken.TokenID == TokenID.LBracket);
            int lbracketPosition = parserContext.CurrentToken.StartPosition;

            parserContext.NextToken(); // Consume the '['

            List<CodeExpression> indexList = ParseIndexList(parserContext);
            CodeExpression[] indices = indexList.ToArray();

            CodeExpression resultExpr = null;

            RuleExpressionInfo primaryExprInfo = validation.ExpressionInfo(primaryExpr);
            if (primaryExprInfo.ExpressionType.IsArray)
            {
                // The primary is an array type, so create an array indexer expression.
                resultExpr = new CodeArrayIndexerExpression(primaryExpr, indices);
            }
            else
            {
                // The primary isn't an array, so assume it has an indexer property.
                resultExpr = new CodeIndexerExpression(primaryExpr, indices);
            }

            parserContext.exprPositions[resultExpr] = lbracketPosition;
            ValidateExpression(parserContext, resultExpr, assignIsEquality, ValueCheck.Read);

            return resultExpr;
        }

        // Parse:
        //              expression-list --> logical-expression  expression-list-tail
        //                              --> logical-expression
        //
        //              expression-list-tail --> ,  logical-expression  expression-list-tail
        //                                   --> ,  logical-expression
        private List<CodeExpression> ParseIndexList(ParserContext parserContext)
        {
            List<CodeExpression> indexList = new List<CodeExpression>();

            CodeExpression indexExpr = ParseBinaryExpression(parserContext, 0, true, ValueCheck.Read); //ParseLogicalExpression();
            indexList.Add(indexExpr);

            while (parserContext.CurrentToken.TokenID == TokenID.Comma)
            {
                parserContext.NextToken(); // eat the comma

                indexExpr = ParseBinaryExpression(parserContext, 0, true, ValueCheck.Read); //ParseLogicalExpression();
                indexList.Add(indexExpr);
            }

            if (parserContext.CurrentToken.TokenID != TokenID.RBracket)
                throw new RuleSyntaxException(ErrorNumbers.Error_MissingCloseSquareBracket, Messages.Parser_MissingCloseSquareBracket, parserContext.CurrentToken.StartPosition);

            parserContext.NextToken(); // consume the ']'

            return indexList;
        }

        // Parse:
        //              member-operator --> . IDENTIFIER method-call-arguments
        //                              --> . IDENTIFIER
        //              
        //              method-call-arguments --> ( argument-list )
        //                                    --> ( )
        //
        //              argument-list --> argument argument-list-tail
        //                            --> argument
        //
        //              argument-list-tail --> , argument argument-list-tail
        //                                 --> , argument
        private CodeExpression ParseMemberOperator(ParserContext parserContext, CodeExpression primaryExpr, bool assignIsEquality, ValueCheck check)
        {
            System.Diagnostics.Debug.Assert(parserContext.CurrentToken.TokenID == TokenID.Dot);

            Token token = parserContext.NextToken(); // Consume the '.'
            if (token.TokenID != TokenID.Identifier)
            {
                if (parserContext.provideIntellisense && token.TokenID == TokenID.EndOfInput)
                {
                    parserContext.SetTypeMemberCompletions(validation.ExpressionInfo(primaryExpr).ExpressionType, validation.ThisType, primaryExpr is CodeTypeReferenceExpression, validation);
                    return null;
                }
                else
                {
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingIdentifierAfterDot, Messages.Parser_MissingIdentifierAfterDot, parserContext.CurrentToken.StartPosition);
                }
            }

            string idName = (string)token.Value;
            int idPosition = token.StartPosition;

            CodeExpression postfixExpr = null;

            if (parserContext.NextToken().TokenID == TokenID.LParen)
            {
                postfixExpr = ParseMethodInvoke(parserContext, primaryExpr, idName, true);
            }
            else
            {
                postfixExpr = ParseFieldOrProperty(parserContext, primaryExpr, idName, idPosition, assignIsEquality, check);
            }

            return postfixExpr;
        }

        private CodeExpression ParseMethodInvoke(ParserContext parserContext, CodeExpression postfixExpr, string methodName, bool assignIsEquality)
        {
            System.Diagnostics.Debug.Assert(parserContext.CurrentToken.TokenID == TokenID.LParen);

            // Start of a method call parameter list.
            int lparenPosition = parserContext.CurrentToken.StartPosition;

            parserContext.NextToken();

            if (parserContext.CurrentToken.TokenID == TokenID.EndOfInput && parserContext.provideIntellisense)
            {
                bool isStatic = postfixExpr is CodeTypeReferenceExpression;
                parserContext.SetMethodCompletions(validation.ExpressionInfo(postfixExpr).ExpressionType, validation.ThisType, methodName, isStatic, !isStatic, validation);
                return null;
            }

            List<CodeExpression> arguments = ParseArgumentList(parserContext);

            postfixExpr = new CodeMethodInvokeExpression(postfixExpr, methodName, arguments.ToArray());
            parserContext.exprPositions[postfixExpr] = lparenPosition;
            ValidateExpression(parserContext, postfixExpr, assignIsEquality, ValueCheck.Read);

            return postfixExpr;
        }

        private List<CodeExpression> ParseArgumentList(ParserContext parserContext)
        {
            List<CodeExpression> argList = new List<CodeExpression>();

            if (parserContext.CurrentToken.TokenID != TokenID.RParen)
            {
                CodeExpression argResult = ParseArgument(parserContext, true);
                argList.Add(argResult);
                while (parserContext.CurrentToken.TokenID == TokenID.Comma)
                {
                    parserContext.NextToken(); // eat the comma

                    argResult = ParseArgument(parserContext, true);
                    argList.Add(argResult);
                }

                if (parserContext.CurrentToken.TokenID != TokenID.RParen)
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingRParenAfterArgumentList, Messages.Parser_MissingRParenAfterArgumentList, parserContext.CurrentToken.StartPosition);
            }

            parserContext.NextToken(); // consume the ')'

            return argList;
        }

        private CodeExpression ParseFieldOrProperty(ParserContext parserContext, CodeExpression postfixExpr, string name, int namePosition, bool assignIsEquality, ValueCheck check)
        {
            CodeExpression fieldOrPropExpr = null;

            Type postFixExprType = Validator.ExpressionInfo(postfixExpr).ExpressionType;

            MemberInfo member = Validator.ResolveFieldOrProperty(postFixExprType, name);
            if (member == null)
            {
                // We could not find the field or property.
                Type type = Validator.ExpressionInfo(postfixExpr).ExpressionType;
                string message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownFieldOrProperty, name, RuleDecompiler.DecompileType(type));
                throw new RuleSyntaxException(ErrorNumbers.Error_UnknownFieldOrProperty, message, namePosition);
            }
            else
            {
                if (member.MemberType == MemberTypes.Field)
                    fieldOrPropExpr = new CodeFieldReferenceExpression(postfixExpr, name);
                else
                    fieldOrPropExpr = new CodePropertyReferenceExpression(postfixExpr, name);

                parserContext.exprPositions[fieldOrPropExpr] = namePosition;
                ValidateExpression(parserContext, fieldOrPropExpr, assignIsEquality, check);
            }

            return fieldOrPropExpr;
        }

        private CodeExpression ParseUnadornedFieldOrProperty(ParserContext parserContext, string name, int namePosition, bool assignIsEquality)
        {
            Type thisType = Validator.ThisType;

            // Resolve the field or property relative to the type of "this".  This will find all static & non-static
            // fields and properties.
            MemberInfo member = Validator.ResolveFieldOrProperty(thisType, name);
            if (member == null)
            {
                // We could not find the field or property.
                string message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownFieldOrProperty, name, RuleDecompiler.DecompileType(thisType));
                throw new RuleSyntaxException(ErrorNumbers.Error_UnknownFieldOrProperty, message, namePosition);
            }

            bool isStatic = false;

            FieldInfo fieldInfo = member as FieldInfo;
            if (fieldInfo != null)
            {
                isStatic = fieldInfo.IsStatic;
            }
            else
            {
                PropertyInfo propInfo = member as PropertyInfo;
                if (propInfo != null)
                {
                    // Q: I wonder why I can't just ask "propInfo.IsStatic"?
                    MethodInfo[] accessors = propInfo.GetAccessors(true);
                    for (int i = 0; i < accessors.Length; ++i)
                    {
                        if (accessors[i].IsStatic)
                        {
                            isStatic = true;
                            break;
                        }
                    }
                }
            }

            // If static, implicitly prefix with the type name; else implicitly prefix with "this".
            CodeExpression primaryExpr = null;
            if (isStatic)
                primaryExpr = new CodeTypeReferenceExpression(thisType);
            else
                primaryExpr = new CodeThisReferenceExpression();

            // Create field or property reference expression, as appropriate.
            CodeExpression fieldOrPropExpr = null;
            if (fieldInfo != null)
                fieldOrPropExpr = new CodeFieldReferenceExpression(primaryExpr, name);
            else
                fieldOrPropExpr = new CodePropertyReferenceExpression(primaryExpr, name);

            parserContext.exprPositions[fieldOrPropExpr] = namePosition;
            ValidateExpression(parserContext, fieldOrPropExpr, assignIsEquality, ValueCheck.Read);

            return fieldOrPropExpr;
        }

        private CodeExpression ParseUnadornedMethodInvoke(ParserContext parserContext, string methodName, bool assignIsEquality)
        {
            System.Diagnostics.Debug.Assert(parserContext.CurrentToken.TokenID == TokenID.LParen);

            Type thisType = Validator.ThisType;

            // Start of a method call parameter list.
            int lparenPosition = parserContext.CurrentToken.StartPosition;
            parserContext.NextToken();

            if (parserContext.CurrentToken.TokenID == TokenID.EndOfInput && parserContext.provideIntellisense)
            {
                parserContext.SetMethodCompletions(thisType, thisType, methodName, true, true, validation);
                return null;
            }

            List<CodeExpression> arguments = ParseArgumentList(parserContext);

            // Binding flags include all public & non-public, all instance, and all static.
            // All are possible candidates for unadorned method references.
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Instance;
            ValidationError error = null;
            RuleMethodInvokeExpressionInfo methodInvokeInfo = validation.ResolveMethod(thisType, methodName, bindingFlags, arguments, out error);

            if (methodInvokeInfo == null)
                throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, lparenPosition);

            MethodInfo mi = methodInvokeInfo.MethodInfo;

            CodeExpression primaryExpr = null;
            if (mi.IsStatic)
                primaryExpr = new CodeTypeReferenceExpression(thisType);
            else
                primaryExpr = new CodeThisReferenceExpression();

            CodeExpression postfixExpr = new CodeMethodInvokeExpression(primaryExpr, methodName, arguments.ToArray());
            parserContext.exprPositions[postfixExpr] = lparenPosition;
            ValidateExpression(parserContext, postfixExpr, assignIsEquality, ValueCheck.Read);

            return postfixExpr;
        }

        // Parse:
        //              argument --> direction logical-expression
        //                       --> logical-expression
        //              
        //              direction --> IN
        //                        --> OUT 
        //                        --> REF 
        private CodeExpression ParseArgument(ParserContext parserContext, bool assignIsEquality)
        {
            CodeExpression argResult = null;

            Token token = parserContext.CurrentToken;
            int directionPosition = token.StartPosition;
            FieldDirection? direction = null;
            ValueCheck check = ValueCheck.Read;
            switch (token.TokenID)
            {
                case TokenID.In:
                    direction = FieldDirection.In;
                    parserContext.NextToken(); // eat the direction token
                    break;
                case TokenID.Out:
                    direction = FieldDirection.Out;
                    parserContext.NextToken();
                    check = ValueCheck.Write;
                    break;
                case TokenID.Ref:
                    direction = FieldDirection.Ref;
                    parserContext.NextToken();
                    check = ValueCheck.Read | ValueCheck.Write;
                    break;
            }

            argResult = ParseBinaryExpression(parserContext, 0, true, check);
            if (direction != null)
            {
                argResult = new CodeDirectionExpression(direction.Value, argResult);
                parserContext.exprPositions[argResult] = directionPosition;
                ValidateExpression(parserContext, argResult, assignIsEquality, ValueCheck.Read);
            }

            return argResult;
        }

        // Parse:
        // primary-expression   --> ( logical-expression )
        //                      --> IDENTIFIER
        //                      --> IDENTIFIER  method-call-arguments  
        //                      --> type-name
        //                      --> object-creation-expression
        //                      --> array-creation-expression
        //                      --> integer-constant
        //                      --> decimal-constant
        //                      --> float-constant
        //                      --> character-constant
        //                      --> string-constant
        //                      --> NULL
        //                      --> THIS
        //                      --> TRUE
        //                      --> FALSE
        private CodeExpression ParsePrimaryExpression(ParserContext parserContext, bool assignIsEquality)
        {
            CodeExpression primaryExpr = null;

            Token token = parserContext.CurrentToken;

            switch (token.TokenID)
            {
                case TokenID.LParen:
                    // A parenthesized subexpression
                    parserContext.NextToken();

                    primaryExpr = ParseBinaryExpression(parserContext, 0, assignIsEquality, ValueCheck.Read);
                    parserContext.exprPositions[primaryExpr] = token.StartPosition;

                    token = parserContext.CurrentToken;
                    if (token.TokenID != TokenID.RParen)
                        throw new RuleSyntaxException(ErrorNumbers.Error_MissingRParenInSubexpression, Messages.Parser_MissingRParenInSubexpression, parserContext.CurrentToken.StartPosition);

                    parserContext.NextToken(); // eat the ')'
                    break;

                case TokenID.Identifier:
                    primaryExpr = ParseRootIdentifier(parserContext, assignIsEquality);
                    break;

                case TokenID.This:
                    parserContext.NextToken(); // eat "this"

                    primaryExpr = new CodeThisReferenceExpression();
                    parserContext.exprPositions[primaryExpr] = token.StartPosition;
                    ValidateExpression(parserContext, primaryExpr, assignIsEquality, ValueCheck.Read);
                    break;

                case TokenID.TypeName:
                    parserContext.NextToken(); // eat the type name

                    Type type = (Type)token.Value;
                    CodeTypeReference typeRef = new CodeTypeReference(type);
                    validation.AddTypeReference(typeRef, type);

                    primaryExpr = new CodeTypeReferenceExpression(typeRef);
                    parserContext.exprPositions[primaryExpr] = token.StartPosition;
                    ValidateExpression(parserContext, primaryExpr, assignIsEquality, ValueCheck.Read);
                    break;

                case TokenID.New:
                    parserContext.NextToken(); // eat "new"
                    primaryExpr = ParseObjectCreation(parserContext, assignIsEquality);
                    break;

                case TokenID.IntegerLiteral:
                case TokenID.FloatLiteral:
                case TokenID.DecimalLiteral:
                case TokenID.CharacterLiteral:
                case TokenID.StringLiteral:
                case TokenID.True:
                case TokenID.False:
                case TokenID.Null:
                    parserContext.NextToken(); // eat the literal

                    primaryExpr = new CodePrimitiveExpression(token.Value);
                    parserContext.exprPositions[primaryExpr] = token.StartPosition;
                    ValidateExpression(parserContext, primaryExpr, assignIsEquality, ValueCheck.Read);
                    break;

                case TokenID.EndOfInput:
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingOperand, Messages.Parser_MissingOperand, token.StartPosition);

                default:
                    throw new RuleSyntaxException(ErrorNumbers.Error_UnknownLiteral, Messages.Parser_UnknownLiteral, token.StartPosition);
            }

            return primaryExpr;
        }

        // Parse:
        //     object-creation-expression --> NEW type-name method-call-arguments
        //     array-creation-expression --> NEW array-spec
        //                               --> NEW array-spec array-initializer
        private CodeExpression ParseObjectCreation(ParserContext parserContext, bool assignIsEquality)
        {
            CodeExpression primaryExpr = null;
            Token token = parserContext.CurrentToken;
            CodeExpression size;
            Type type = TryParseTypeSpecifierWithOptionalSize(parserContext, assignIsEquality, out size);

            // handle intellisense, regardless of whether we get a type back or not
            if (parserContext.provideIntellisense && parserContext.CurrentToken.TokenID == TokenID.EndOfInput)
            {
                // if we have a type, get only nested classes for it
                // if we don't have a type, then take whatever is already set for completions
                if (type != null)
                    parserContext.SetNestedClassCompletions(type, validation.ThisType);
                return null;
            }

            if (type == null)
                throw new RuleSyntaxException(ErrorNumbers.Error_InvalidTypeArgument, Messages.Parser_InvalidTypeArgument, token.StartPosition);

            if (size == null)
            {
                // must be an object-creation-expression
                if (parserContext.CurrentToken.TokenID != TokenID.LParen)
                {
                    // [] are already handled by TryParseTypeSpecifierWithOptionalSize
                    throw new RuleSyntaxException(ErrorNumbers.Error_InvalidTypeArgument, Messages.Parser_InvalidNew, token.StartPosition);
                }
                primaryExpr = ParseConstructorArguments(parserContext, type, assignIsEquality);
            }
            else
            {
                // it's an array
                List<CodeExpression> initializers = ParseArrayCreationArguments(parserContext);
                if (initializers != null)
                {
                    if (size == defaultSize)
                        primaryExpr = new CodeArrayCreateExpression(type, initializers.ToArray());
                    else
                    {
                        // both specified
                        primaryExpr = new CodeArrayCreateExpression(type, size);
                        ((CodeArrayCreateExpression)primaryExpr).Initializers.AddRange(initializers.ToArray());
                    }
                }
                else
                {
                    // no initializers, so size matters
                    if (size != defaultSize)
                        primaryExpr = new CodeArrayCreateExpression(type, size);
                    else
                    {
                        // neither specified, so error
                        throw new RuleSyntaxException(ErrorNumbers.Error_NoArrayCreationSize,
                            Messages.Parser_NoArrayCreationSize,
                            parserContext.CurrentToken.StartPosition);
                    }
                }
                ValidateExpression(parserContext, primaryExpr, assignIsEquality, ValueCheck.Read);
            }

            return primaryExpr;
        }

        private CodeExpression ParseConstructorArguments(ParserContext parserContext, Type type, bool assignIsEquality)
        {
            System.Diagnostics.Debug.Assert(parserContext.CurrentToken.TokenID == TokenID.LParen);

            // Start of a constructor parameter list.
            int lparenPosition = parserContext.CurrentToken.StartPosition;
            parserContext.NextToken();

            if (parserContext.CurrentToken.TokenID == TokenID.EndOfInput && parserContext.provideIntellisense)
            {
                parserContext.SetConstructorCompletions(type, Validator.ThisType);
                return null;
            }

            List<CodeExpression> arguments = ParseArgumentList(parserContext);

            if ((type.IsValueType) && (arguments.Count == 0))
            {
                // this is always allowed
            }
            else if (type.IsAbstract)
            {
                // this is not allowed
                string message = string.Format(CultureInfo.CurrentCulture,
                    Messages.UnknownConstructor,
                    RuleDecompiler.DecompileType(type));
                throw new RuleSyntaxException(ErrorNumbers.Error_MethodNotExists, message, lparenPosition);
            }
            else
            {
                // Binding flags include all public & non-public, all instance, and all static.
                // All are possible candidates for unadorned method references.
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;
                if (type.Assembly == validation.ThisType.Assembly)
                    bindingFlags |= BindingFlags.NonPublic;
                ValidationError error = null;
                RuleConstructorExpressionInfo constructorInvokeInfo = validation.ResolveConstructor(type, bindingFlags, arguments, out error);

                if (constructorInvokeInfo == null)
                    throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, lparenPosition);
            }

            CodeExpression postfixExpr = new CodeObjectCreateExpression(type, arguments.ToArray());
            parserContext.exprPositions[postfixExpr] = lparenPosition;
            ValidateExpression(parserContext, postfixExpr, assignIsEquality, ValueCheck.Read);

            return postfixExpr;
        }

        // Parse:
        //     array-initializer --> {  variable-initializer-list  }
        //                           {  }
        //     variable-initializer-list --> variable-initializer variable-initializer-list-tail
        //                               --> variable-initializer
        //     variable-initializer-list-tail --> , variable-initializer variable-initializer-list-tail
        //                                    --> , variable-initializer
        private List<CodeExpression> ParseArrayCreationArguments(ParserContext parserContext)
        {
            // if there are no initializers, return null
            if (parserContext.CurrentToken.TokenID != TokenID.LCurlyBrace)
                return null;

            List<CodeExpression> initializers = new List<CodeExpression>();
            parserContext.NextToken();     // skip '{'

            if (parserContext.CurrentToken.TokenID != TokenID.RCurlyBrace)
            {
                initializers.Add(ParseInitializer(parserContext, true));
                while (parserContext.CurrentToken.TokenID == TokenID.Comma)
                {
                    parserContext.NextToken(); // eat the comma
                    initializers.Add(ParseInitializer(parserContext, true));
                }

                if (parserContext.CurrentToken.TokenID != TokenID.RCurlyBrace)
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingRCurlyAfterInitializers,
                        Messages.Parser_MissingRCurlyAfterInitializers,
                        parserContext.CurrentToken.StartPosition);
            }

            parserContext.NextToken();     // eat the '}'
            return initializers;
        }

        // Parse:
        //     variable-initializer --> logical-expression
        private CodeExpression ParseInitializer(ParserContext parserContext, bool assignIsEquality)
        {
            // size we only handle 1 level arrays, initializers must be regular expressions
            return ParseBinaryExpression(parserContext, 0, assignIsEquality, ValueCheck.Read);
        }

        // Parse a root identifier which may be:
        //      1. A field/property/method with an implicit "this." prepended to it.
        //      2. A nested type within the type of this.
        //      2. An unqualified type name
        //      3. A namespace name.
        private CodeExpression ParseRootIdentifier(ParserContext parserContext, bool assignIsEquality)
        {
            Token token = parserContext.CurrentToken;
            string name = (string)token.Value;
            Symbol sym = null;

            // Consult the local unique symbol list first.  If we find a symbol here, that's the one.
            if (!localUniqueSymbols.TryGetValue(name, out sym))
            {
                // Wasn't found in the local unique symbols, try the global unique symbols.
                globalUniqueSymbols.TryGetValue(name, out sym);
            }

            if (sym == null)
            {
                // We couldn't find it in either location.  This is an error.
                string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_UnknownIdentifier, name);
                throw new RuleSyntaxException(ErrorNumbers.Error_UnknownIdentifier, message, token.StartPosition);
            }

            return sym.ParseRootIdentifier(this, parserContext, assignIsEquality);
        }

        // Parser:
        //      primary-expr --> ...
        //                   --> IDENTIFIER
        //                   --> IDENTIFIER  method-call-arguments  
        //                   --> ...
        internal CodeExpression ParseUnadornedMemberIdentifier(ParserContext parserContext, MemberSymbol symbol, bool assignIsEquality)
        {
            // This is an implicit member reference off "this", so add the "this".  (Or an implicit
            // static member reference of the type of "this", so add the type name.)
            Token token = parserContext.CurrentToken;
            int namePosition = token.StartPosition;

            parserContext.NextToken(); // eat the identifier

            CodeExpression primaryExpr = null;
            if (parserContext.CurrentToken.TokenID == TokenID.LParen)
                primaryExpr = ParseUnadornedMethodInvoke(parserContext, symbol.Name, true);
            else
                primaryExpr = ParseUnadornedFieldOrProperty(parserContext, symbol.Name, namePosition, assignIsEquality);

            return primaryExpr;
        }

        // Parse:
        //      namespace-qualified-type-name --> NAMESPACE-NAME namespace-qualifier-tail . TYPE-NAME
        //                                    --> NAMESPACE-NAME . TYPE-NAME
        //                                    --> TYPE-NAME
        //
        //      namespace-qualifier-tail --> . NAMESPACE-NAME namespace-qualifier-tail
        internal CodeExpression ParseRootNamespaceIdentifier(ParserContext parserContext, NamespaceSymbol nsSym, bool assignIsEquality)
        {
            // Loop through all the namespace qualifiers until we find something that's not a namespace.
            Symbol nestedSym = null;
            while (nsSym != null)
            {
                Token token = parserContext.NextToken();
                if (token.TokenID != TokenID.Dot)
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingDotAfterNamespace, Messages.Parser_MissingDotAfterNamespace, token.StartPosition);

                token = parserContext.NextToken();
                if (token.TokenID != TokenID.Identifier)
                {
                    if (parserContext.provideIntellisense && token.TokenID == TokenID.EndOfInput)
                    {
                        parserContext.SetNamespaceCompletions(nsSym);
                        return null;
                    }
                    else
                    {
                        throw new RuleSyntaxException(ErrorNumbers.Error_MissingIdentifierAfterDot, Messages.Parser_MissingIdentifierAfterDot, token.StartPosition);
                    }
                }

                string name = (string)token.Value;
                nestedSym = nsSym.FindMember(name);
                if (nestedSym == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_UnknownNamespaceMember, name, nsSym.GetQualifiedName());
                    throw new RuleSyntaxException(ErrorNumbers.Error_UnknownNamespaceMember, message, token.StartPosition);
                }

                nsSym = nestedSym as NamespaceSymbol;
            }

            // We are sitting at a type (or overloaded type).
            return nestedSym.ParseRootIdentifier(this, parserContext, assignIsEquality);
        }

        internal CodeExpression ParseRootTypeIdentifier(ParserContext parserContext, TypeSymbol typeSym, bool assignIsEquality)
        {
            string message = null;
            int typePosition = parserContext.CurrentToken.StartPosition;

            Token token = parserContext.NextToken();

            if (typeSym.GenericArgCount > 0 && token.TokenID != TokenID.Less)
            {
                // This is a generic type, but no argument list was provided.
                message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_MissingTypeArguments, typeSym.Name);
                throw new RuleSyntaxException(ErrorNumbers.Error_MissingTypeArguments, message, token.StartPosition);
            }

            Type type = typeSym.Type;

            if (token.TokenID == TokenID.Less)
            {
                // Start of a generic argument list... the type had better be generic.
                if (typeSym.GenericArgCount == 0)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_NotAGenericType, RuleDecompiler.DecompileType(type));
                    throw new RuleSyntaxException(ErrorNumbers.Error_NotAGenericType, message, token.StartPosition);
                }

                Type[] typeArgs = ParseGenericTypeArgList(parserContext);

                if (typeArgs.Length != typeSym.GenericArgCount)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_BadTypeArgCount, RuleDecompiler.DecompileType(type));
                    throw new RuleSyntaxException(ErrorNumbers.Error_BadTypeArgCount, message, parserContext.CurrentToken.StartPosition);
                }

                // if we are creating generics with design-time types, then the generic needs to be 
                // a wrapped type to create the generic properly, so we look up the generic to get back the wrapper
                type = Validator.ResolveType(type.AssemblyQualifiedName);
                type = type.MakeGenericType(typeArgs);
            }

            token = parserContext.CurrentToken;
            if (token.TokenID == TokenID.Dot)
            {
                Type nestedType = ParseNestedType(parserContext, type);
                if (nestedType != null)
                    type = nestedType;
            }

            return ParseTypeRef(parserContext, type, typePosition, assignIsEquality);
        }

        internal CodeExpression ParseRootOverloadedTypeIdentifier(ParserContext parserContext, List<TypeSymbol> candidateTypeSymbols, bool assignIsEquality)
        {
            Token token = parserContext.CurrentToken;
            string typeName = (string)token.Value;
            int namePosition = token.StartPosition;

            // Get the next token after the identifier.
            token = parserContext.NextToken();
            Type type = null;

            if (token.TokenID == TokenID.Less)
            {
                // Choose from the generic candidates.
                List<Type> candidateTypes = new List<Type>(candidateTypeSymbols.Count);
                foreach (TypeSymbol typeSym in candidateTypeSymbols)
                {
                    if (typeSym.GenericArgCount > 0)
                        candidateTypes.Add(typeSym.Type);
                }

                type = ParseGenericType(parserContext, candidateTypes, typeName);
            }
            else
            {
                // See if there's a non-generic candidate.
                TypeSymbol typeSym = candidateTypeSymbols.Find(delegate(TypeSymbol s) { return s.GenericArgCount == 0; });
                if (typeSym == null)
                {
                    // No argument list was provided, but there's no non-generic overload.
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_MissingTypeArguments, typeName);
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingTypeArguments, message, namePosition);
                }

                type = typeSym.Type;
            }

            if (parserContext.CurrentToken.TokenID == TokenID.Dot)
            {
                Type nestedType = ParseNestedType(parserContext, type);
                if (nestedType != null)
                    type = nestedType;
            }

            return ParseTypeRef(parserContext, type, namePosition, assignIsEquality);
        }

        private CodeExpression ParseTypeRef(ParserContext parserContext, Type type, int typePosition, bool assignIsEquality)
        {
            CodeExpression result = null;

            if (parserContext.CurrentToken.TokenID == TokenID.LParen)
            {
                // A '(' after a typename is only valid if it's an IRuleExpression.
                if (TypeProvider.IsAssignable(typeof(IRuleExpression), type))
                {
                    int lparenPosition = parserContext.CurrentToken.StartPosition;
                    parserContext.NextToken(); // Eat the '('

                    List<CodeExpression> arguments = ParseArgumentList(parserContext);

                    result = (CodeExpression)ConstructCustomType(type, arguments, lparenPosition);

                    parserContext.exprPositions[result] = lparenPosition;
                    ValidateExpression(parserContext, result, assignIsEquality, ValueCheck.Read);
                    return result;
                }
            }

            CodeTypeReference typeRef = new CodeTypeReference(type);
            validation.AddTypeReference(typeRef, type);

            result = new CodeTypeReferenceExpression(typeRef);
            parserContext.exprPositions[result] = typePosition;
            ValidateExpression(parserContext, result, assignIsEquality, ValueCheck.Read);
            return result;
        }

        // Parse nested types.
        private Type ParseNestedType(ParserContext parserContext, Type currentType)
        {
            System.Diagnostics.Debug.Assert(parserContext.CurrentToken.TokenID == TokenID.Dot);

            Type nestedType = null;

            while (parserContext.CurrentToken.TokenID == TokenID.Dot)
            {
                // Save the state of the scanner.  Since we can't tell if we're parsing a nested
                // type or a member, we'll need to backtrack if we go too far.
                int savedTokenState = parserContext.SaveCurrentToken();

                Token token = parserContext.NextToken();
                if (token.TokenID != TokenID.Identifier)
                {
                    if (parserContext.provideIntellisense && token.TokenID == TokenID.EndOfInput)
                    {
                        parserContext.SetTypeMemberCompletions(currentType, validation.ThisType, true, validation);
                        return null;
                    }
                    else
                    {
                        throw new RuleSyntaxException(ErrorNumbers.Error_MissingIdentifierAfterDot, Messages.Parser_MissingIdentifierAfterDot, parserContext.CurrentToken.StartPosition);
                    }
                }

                string name = (string)token.Value;

                BindingFlags bindingFlags = BindingFlags.Public;
                if (currentType.Assembly == validation.ThisType.Assembly)
                    bindingFlags |= BindingFlags.NonPublic;

                if (parserContext.NextToken().TokenID == TokenID.Less)
                {
                    // Might be a generic type.
                    List<Type> candidateGenericTypes = new List<Type>();

                    Type[] nestedTypes = currentType.GetNestedTypes(bindingFlags);
                    string prefix = name + "`";
                    for (int i = 0; i < nestedTypes.Length; ++i)
                    {
                        Type candidateType = nestedTypes[i];
                        if (candidateType.Name.StartsWith(prefix, StringComparison.Ordinal))
                            candidateGenericTypes.Add(candidateType);
                    }

                    if (candidateGenericTypes.Count == 0)
                    {
                        // It wasn't a generic type.  Reset the scanner to the saved state.
                        parserContext.RestoreCurrentToken(savedTokenState);
                        // Also reset the deepenst nested type.
                        nestedType = currentType;
                        break;
                    }

                    nestedType = ParseGenericType(parserContext, candidateGenericTypes, name);
                    currentType = nestedType;
                }
                else
                {
                    // Might be a non-generic type.
                    MemberInfo[] mi = currentType.GetMember(name, bindingFlags);
                    if (mi == null || mi.Length != 1 || (mi[0].MemberType != MemberTypes.NestedType && mi[0].MemberType != MemberTypes.TypeInfo))
                    {
                        // We went too far, reset the state.
                        parserContext.RestoreCurrentToken(savedTokenState);
                        // Also reset the deepest nested type.
                        nestedType = currentType;
                        break;
                    }

                    nestedType = (Type)mi[0];

                    if (currentType.IsGenericType && nestedType.IsGenericTypeDefinition)
                    {
                        // The outer type was generic (and bound), but the nested type is not.  We have
                        // to re-bind the generic arguments.
                        nestedType = nestedType.MakeGenericType(currentType.GetGenericArguments());
                    }

                    currentType = nestedType;
                }
            }

            return nestedType;
        }

        private Type ParseGenericType(ParserContext parserContext, List<Type> candidateGenericTypes, string typeName)
        {
            System.Diagnostics.Debug.Assert(parserContext.CurrentToken.TokenID == TokenID.Less);

            Type[] typeArgs = ParseGenericTypeArgList(parserContext);

            foreach (Type candidateType in candidateGenericTypes)
            {
                Type[] genericArgs = candidateType.GetGenericArguments();
                if (genericArgs.Length == typeArgs.Length)
                    return candidateType.MakeGenericType(typeArgs);
            }

            // No valid candidate found.
            string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_BadTypeArgCount, typeName);
            throw new RuleSyntaxException(ErrorNumbers.Error_BadTypeArgCount, message, parserContext.CurrentToken.StartPosition);
        }

        private Type[] ParseGenericTypeArgList(ParserContext parserContext)
        {
            System.Diagnostics.Debug.Assert(parserContext.CurrentToken.TokenID == TokenID.Less);

            List<Type> typeArgs = new List<Type>();

            Token token;
            do
            {
                // Eat the opening '<' or ','.
                token = parserContext.NextToken();

                Type type = TryParseTypeSpecifier(parserContext, true);
                if (type == null)
                    throw new RuleSyntaxException(ErrorNumbers.Error_InvalidTypeArgument, Messages.Parser_InvalidTypeArgument, token.StartPosition);

                typeArgs.Add(type);
            } while (parserContext.CurrentToken.TokenID == TokenID.Comma);

            if (parserContext.CurrentToken.TokenID != TokenID.Greater)
                throw new RuleSyntaxException(ErrorNumbers.Error_MissingCloseAngleBracket, Messages.Parser_MissingCloseAngleBracket, parserContext.CurrentToken.StartPosition);
            parserContext.NextToken(); // Eat the '>'

            return typeArgs.ToArray();
        }

        // Parse:
        //      type-spec --> type-name
        //                --> type-name  rank-specifiers
        private Type TryParseTypeSpecifier(ParserContext parserContext, bool assignIsEquality)
        {
            Type type = TryParseTypeName(parserContext, assignIsEquality);
            if (type != null)
                type = ParseArrayType(parserContext, type);

            return type;
        }

        private Type TryParseTypeName(ParserContext parserContext, bool assignIsEquality)
        {
            Type type = null;

            Token currentToken = parserContext.CurrentToken;
            if (currentToken.TokenID == TokenID.TypeName)
            {
                type = (Type)currentToken.Value;
                parserContext.NextToken(); // eat the type name
            }
            else if (currentToken.TokenID == TokenID.Identifier)
            {
                Symbol sym = null;
                if (globalUniqueSymbols.TryGetValue((string)currentToken.Value, out sym))
                {
                    CodeExpression identExpr = sym.ParseRootIdentifier(this, parserContext, assignIsEquality);

                    if (identExpr is CodeTypeReferenceExpression)
                        type = validation.ExpressionInfo(identExpr).ExpressionType;
                }
            }

            return type;
        }

        // size returned if array-rank-specifier is empty (e.g. [])
        private static CodeExpression defaultSize = new CodePrimitiveExpression(0);

        // Parse:
        //     array-spec --> type-name
        //                --> type-name  array-rank-specifiers
        //     array-rank-specifiers -->  [  binary-expression  ]
        //                           -->  [  ]
        private Type TryParseTypeSpecifierWithOptionalSize(ParserContext parserContext, bool assignIsEquality, out CodeExpression size)
        {
            Type type = null;
            size = null;

            Token currentToken = parserContext.CurrentToken;
            type = TryParseTypeName(parserContext, assignIsEquality);

            // see if size specified
            if ((type != null) && (parserContext.CurrentToken.TokenID == TokenID.LBracket))
            {
                Token next = parserContext.NextToken();    // skip '['
                // get the size, if specified
                if (next.TokenID != TokenID.RBracket)
                    size = ParseBinaryExpression(parserContext, 0, false, ValueCheck.Read);
                else
                    size = defaultSize;

                if (parserContext.CurrentToken.TokenID != TokenID.RBracket)
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingCloseSquareBracket,
                        Messages.Parser_MissingCloseSquareBracket1,
                        parserContext.CurrentToken.StartPosition);

                parserContext.NextToken();     // Eat the ']'
            }

            return type;
        }

        // Parse:
        //      rank-specifiers --> rank-specifier  rank-specifier-tail
        //                      --> rank-specifier
        //
        //      rank-specifier-tail -->  rank-specifier  rank-specifier-tail
        //
        //      rank-specifier --> [  dim-separators  ]
        //                     --> [  ]
        //
        //      dim-separators --> ,  dim-separators-tail
        //                     --> ,
        //
        //      dim-separators-tail --> ,  dim-separators-tail
        private static Type ParseArrayType(ParserContext parserContext, Type baseType)
        {
            Type type = baseType;

            while (parserContext.CurrentToken.TokenID == TokenID.LBracket)
            {
                int rank = 1;
                while (parserContext.NextToken().TokenID == TokenID.Comma)
                    ++rank;

                if (parserContext.CurrentToken.TokenID == TokenID.RBracket)
                    parserContext.NextToken(); // Eat the ']'
                else
                    throw new RuleSyntaxException(ErrorNumbers.Error_MissingCloseSquareBracket, Messages.Parser_MissingCloseSquareBracket, parserContext.CurrentToken.StartPosition);

                if (rank == 1)
                    type = type.MakeArrayType();
                else
                    type = type.MakeArrayType(rank);
            }

            return type;
        }

        #region Constructor overloading for custom types

        private class CandidateConstructor
        {
            private ConstructorInfo ctor;
            private object[] ctorArgs;
            private bool isExpandedMatch;

            internal CandidateConstructor(ConstructorInfo ctor, object[] ctorArgs, bool isExpandedMatch)
            {
                this.ctor = ctor;
                this.ctorArgs = ctorArgs;
                this.isExpandedMatch = isExpandedMatch;
            }

            internal int CompareConstructor(CandidateConstructor other)
            {
                int better = 1;
                int worse = -1;
                int equal = 0;

                // Try some disambiguating rules for expanded signatures vs normal signatures.
                if (!this.isExpandedMatch && other.isExpandedMatch)
                {
                    // This candidate matched in its normal form, but the other one matched only after
                    // expansion of a params array.  This one is better.
                    return better;
                }
                else if (this.isExpandedMatch && !other.isExpandedMatch)
                {
                    // This candidate matched in its expanded form, but the other one matched in its
                    // normal form.  The other one was better.
                    return worse;
                }
                else if (this.isExpandedMatch && other.isExpandedMatch)
                {
                    // Both candidates matched in their expanded forms.  

                    int thisParameterCount = this.ctor.GetParameters().Length;
                    int otherParameterCount = other.ctor.GetParameters().Length;

                    if (thisParameterCount > otherParameterCount)
                    {
                        // This candidate had more declared parameters, so it is better.
                        return better;
                    }
                    else if (otherParameterCount > thisParameterCount)
                    {
                        // The other candidate had more declared parameters, so it was better.
                        return worse;
                    }
                }

                // Nothing worked, the two candidates are equally applicable.
                return equal;
            }

            internal object InvokeConstructor()
            {
                return ctor.Invoke(ctorArgs);
            }
        }

        private object MatchArgument(Type parameterType, CodeExpression arg)
        {
            Type argExprType = arg.GetType();

            if (TypeProvider.IsAssignable(parameterType, argExprType))
            {
                // The argument expression type is assignable to the parameter type,
                // so it goes through unscathed.
                return arg;
            }
            else
            {
                // See if the argument is a constant value, whose type is compatible with
                // the parameter.
                CodePrimitiveExpression argPrimitive = arg as CodePrimitiveExpression;
                if (argPrimitive != null)
                {
                    ValidationError error = null;
                    Type argPrimitiveType = validation.ExpressionInfo(argPrimitive).ExpressionType;
                    if (RuleValidation.TypesAreAssignable(argPrimitiveType, parameterType, argPrimitive, out error))
                    {
                        // The constant expression's type matched the parameter, so
                        // use the actual primitive's value as the argument.
                        return argPrimitive.Value;
                    }
                }
            }

            return null;
        }

        private List<CandidateConstructor> GetCandidateConstructors(ConstructorInfo[] allCtors, List<CodeExpression> arguments)
        {
            if (allCtors == null || allCtors.Length == 0)
                return null;

            int numArgs = arguments.Count;

            List<CandidateConstructor> candidates = new List<CandidateConstructor>(allCtors.Length);
            for (int c = 0; c < allCtors.Length; ++c)
            {
                ConstructorInfo ctor = allCtors[c];

                ParameterInfo[] parms = ctor.GetParameters();
                if (parms.Length == 0)
                {
                    if (numArgs == 0)
                    {
                        // Trivial match...
                        candidates.Add(new CandidateConstructor(ctor, new object[0], false));
                        break; // No other candidates will match.
                    }
                }
                else
                {
                    int parameterCount = parms.Length;
                    int fixedParameterCount = parameterCount;

                    ParameterInfo lastParm = parms[parameterCount - 1];
                    if (lastParm.ParameterType.IsArray)
                    {
                        object[] attrs = lastParm.GetCustomAttributes(typeof(ParamArrayAttribute), false);
                        if (attrs != null && attrs.Length > 0)
                            fixedParameterCount -= 1;
                    }

                    if (numArgs < fixedParameterCount)
                    {
                        // Too few arguments to match
                        continue;
                    }
                    else if (fixedParameterCount == parameterCount && numArgs != parameterCount)
                    {
                        // Too many arguments were passed for this to be a candidate.
                        continue;
                    }

                    object[] ctorArgs = new object[parameterCount];

                    // Make sure all the fixed arguments match the fixed parameters.
                    int p;
                    for (p = 0; p < fixedParameterCount; ++p)
                    {
                        object matchedArg = MatchArgument(parms[p].ParameterType, arguments[p]);
                        if (matchedArg == null)
                            break;

                        ctorArgs[p] = matchedArg;
                    }

                    if (p != fixedParameterCount)
                    {
                        // At least one of the fixed arguments didn't match the corresponding parameter, so this
                        // can't be a candidate.
                        continue;
                    }

                    if (fixedParameterCount == parameterCount)
                    {
                        // We had a match, and there was no params expansion.
                        candidates.Add(new CandidateConstructor(ctor, ctorArgs, false));
                    }
                    else
                    {
                        // Handle the 'params' portion.

                        if (numArgs == fixedParameterCount)
                        {
                            // We have a match, in its expanded form, with nothing being passed as the last
                            // argument.
                            candidates.Add(new CandidateConstructor(ctor, ctorArgs, true));
                        }
                        else
                        {
                            if (numArgs == fixedParameterCount + 1 && validation.ExpressionInfo(arguments[p]).ExpressionType == typeof(NullLiteral))
                            {
                                // Another special case.  The last argument, which matches the "params" array,
                                // is the null literal.  That's all it is allowed to be, since we allow no other
                                // way to pass an 'array literal'.  The constructor matches WITHOUT expansion.
                                candidates.Add(new CandidateConstructor(ctor, ctorArgs, false));
                            }
                            else
                            {
                                Type paramType = parms[p].ParameterType;
                                System.Diagnostics.Debug.Assert(paramType.IsArray, "last parameter in 'params' list must have an array type");
                                Type elementType = paramType.GetElementType();

                                Array paramsArgs = (Array)paramType.InvokeMember(paramType.Name, BindingFlags.CreateInstance, null, null, new object[] { numArgs - fixedParameterCount }, CultureInfo.CurrentCulture);
                                ctorArgs[fixedParameterCount] = paramsArgs;

                                // Try matching the rest of the arguments to the params array element type.
                                for (; p < numArgs; ++p)
                                {
                                    object matchedArg = MatchArgument(elementType, arguments[p]);
                                    if (matchedArg == null)
                                        break;

                                    paramsArgs.SetValue(matchedArg, p - fixedParameterCount);
                                }

                                if (p != numArgs)
                                {
                                    // At least one of the params arguments didn't match the last parameter's element type, so this
                                    // can't be a candidate.
                                    continue;
                                }

                                // We passed all the tests, it's a candidate.
                                candidates.Add(new CandidateConstructor(ctor, ctorArgs, fixedParameterCount != parameterCount));
                            }
                        }
                    }
                }
            }

            return candidates;
        }

        static CandidateConstructor FindBestConstructor(List<CandidateConstructor> candidates)
        {
            int numCandidates = candidates.Count;
            System.Diagnostics.Debug.Assert(numCandidates > 0, "expected at least one candidate");

            // Start by assuming the first candidate is the best one.
            List<CandidateConstructor> bestCandidates = new List<CandidateConstructor>(1);
            bestCandidates.Add(candidates[0]);

            // Go through the rest of the candidates and try to find a better one.  (If
            // there are no more candidates, then there was only one, and that's the right
            // one.)
            for (int i = 1; i < numCandidates; ++i)
            {
                CandidateConstructor newCandidate = candidates[i];

                // Compare this new candidate one if the current "best" ones.  (If there
                // is currently more than one best candidate, then so far its ambiguous, which 
                // means all the best ones are equally good.  Thus if this new candidate
                // is better than one, it's better than all.
                CandidateConstructor bestCandidate = bestCandidates[0];

                int comparison = newCandidate.CompareConstructor(bestCandidate);
                if (comparison > 0)
                {
                    // The new one was better than at least one of the best ones.  It
                    // becomes the new best one.
                    bestCandidates.Clear();
                    bestCandidates.Add(newCandidate);
                }
                else if (comparison == 0)
                {
                    // The new one was no better, so add it to the list of current best.
                    // (Unless we find a better one, it's ambiguous so far.)
                    bestCandidates.Add(newCandidate);
                }
            }

            if (bestCandidates.Count == 1)
            {
                // Good, there was exactly one best match.
                return bestCandidates[0];
            }

            // Otherwise, it must have been ambiguous.
            return null;
        }


        private object ConstructCustomType(Type type, List<CodeExpression> arguments, int lparenPosition)
        {
            string message;

            // Build a list of candidate constructors.

            ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            List<CandidateConstructor> candidates = GetCandidateConstructors(ctors, arguments);
            if (candidates == null || candidates.Count == 0)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownMethod, type.Name, RuleDecompiler.DecompileType(type));
                throw new RuleSyntaxException(ErrorNumbers.Error_MethodNotExists, message, lparenPosition);
            }

            // Select the best constructor from the list of candidates.

            CandidateConstructor bestCandidate = FindBestConstructor(candidates);

            if (bestCandidate == null)
            {
                // It was ambiguous.
                message = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousConstructor, type.Name);
                throw new RuleSyntaxException(ErrorNumbers.Error_CannotResolveMember, message, lparenPosition);
            }

            // Invoke the constructor.
            object result = null;
            try
            {
                result = bestCandidate.InvokeConstructor();
            }
            catch (TargetInvocationException invokeEx)
            {
                if (invokeEx.InnerException == null)
                    throw; // just rethrow this one in the unlikely case the inner one is null.

                // Rethrow the inner exception's message as a RuleSyntaxException.
                throw new RuleSyntaxException(ErrorNumbers.Error_MethodNotExists, invokeEx.InnerException.Message, lparenPosition);
            }

            return result;
        }

        #endregion


        #region Validation Helpers

        private void ValidateExpression(ParserContext parserContext, CodeExpression expression, bool assignIsEquality, ValueCheck check)
        {
            // If the current token is an assignment operator, then make sure the expression is validated
            // correctly (written-to).  Note that because we allow "=" (Token.Assign) as a synonym for
            // "==" (Token.Equal), we need to distinguish whether we're parsing a condition vs an action.
            // In other words, we need to be sure that the "=" really is an "=".
            if (parserContext.CurrentToken.TokenID == TokenID.Assign && !assignIsEquality)
                check = ValueCheck.Write;

            // use value in check
            RuleExpressionInfo exprInfo = null;
            if ((check & ValueCheck.Read) != 0)
            {
                exprInfo = RuleExpressionWalker.Validate(Validator, expression, false);
                // check write if set and first validate succeeded
                if ((exprInfo != null) && ((check & ValueCheck.Write) != 0))
                    exprInfo = RuleExpressionWalker.Validate(Validator, expression, true);
            }
            else if ((check & ValueCheck.Write) != 0)
                exprInfo = RuleExpressionWalker.Validate(Validator, expression, true);

            if (exprInfo == null && Validator.Errors.Count > 0)
            {
                // Choose the first one and throw it.
                ValidationError error = Validator.Errors[0];

                // Try to get the position, or just use zero if we can't.
                object errorObject = error.UserData[RuleUserDataKeys.ErrorObject];
                int position = 0;
                parserContext.exprPositions.TryGetValue(errorObject, out position);

                throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, position);
            }
        }

        private void ValidateStatement(ParserContext parserContext, CodeStatement statement)
        {
            if (!CodeDomStatementWalker.Validate(Validator, statement) && Validator.Errors.Count > 0)
            {
                // Choose the first one and throw it.
                ValidationError error = Validator.Errors[0];

                // Try to get the position, or just use zero if we can't.
                object errorObject = error.UserData[RuleUserDataKeys.ErrorObject];
                int position = 0;
                parserContext.exprPositions.TryGetValue(errorObject, out position);

                throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, position);
            }
        }

        private void ValidateAction(ParserContext parserContext, RuleAction action)
        {
            if (!action.Validate(validation))
            {
                // Choose the first one and throw it.
                ValidationError error = Validator.Errors[0];

                // Try to get the position, or just use zero if we can't.
                object errorObject = error.UserData[RuleUserDataKeys.ErrorObject];
                int position = 0;
                parserContext.exprPositions.TryGetValue(errorObject, out position);

                throw new RuleSyntaxException(error.ErrorNumber, error.ErrorText, position);
            }
        }

        #endregion
    }
}
