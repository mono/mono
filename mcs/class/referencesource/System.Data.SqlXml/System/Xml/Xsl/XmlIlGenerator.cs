//------------------------------------------------------------------------------
// <copyright file="XmlIlGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="false">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Xml.XPath;
using System.Xml.Xsl.IlGen;
using System.Xml.Xsl.Qil;
using System.Xml.Xsl.Runtime;
using System.Runtime.Versioning;

namespace System.Xml.Xsl {

    internal delegate void ExecuteDelegate(XmlQueryRuntime runtime);


    /// <summary>
    /// This internal class is the entry point for creating Msil assemblies from QilExpression.
    /// </summary>
    /// <remarks>
    /// Generate will return an AssemblyBuilder with the following setup:
    /// Assembly Name = "MS.Internal.Xml.CompiledQuery"
    /// Module Dll Name = "MS.Internal.Xml.CompiledQuery.dll"
    /// public class MS.Internal.Xml.CompiledQuery.Test {
    ///     public static void Execute(XmlQueryRuntime runtime);
    ///     public static void Root(XmlQueryRuntime runtime);
    ///     private static ... UserMethod1(XmlQueryRuntime runtime, ...);
    ///     ...
    ///     private static ... UserMethodN(XmlQueryRuntime runtime, ...);
    /// }
    ///
    /// XmlILGenerator incorporates a number of different technologies in order to generate efficient code that avoids caching
    /// large result sets in memory:
    ///
    /// 1. Code Iterators - Query results are computed using a set of composable, interlocking iterators that alone perform a
    /// simple task, but together execute complex queries.  The iterators are actually little blocks of code
    /// that are connected to each other using a series of jumps.  Because each iterator is not instantiated
    /// as a separate object, the number of objects and number of function calls is kept to a minimum during
    /// execution.  Also, large result sets are often computed incrementally, with each iterator performing one step in a
    /// pipeline of sequence items.
    ///
    /// 2. Analyzers - During code generation, QilToMsil traverses the semantic tree representation of the query (QIL) several times.
    /// As visits to each node in the tree start and end, various Analyzers are invoked.  These Analyzers incrementally
    /// collect and store information that is later used to generate faster and smaller code.
    /// </remarks>
    internal class XmlILGenerator {
        private QilExpression qil;
        private GenerateHelper helper;
        private XmlILOptimizerVisitor optVisitor;
        private XmlILVisitor xmlIlVisitor;
        private XmlILModule module;

        /// <summary>
        /// Always output debug information in debug mode.
        /// </summary>
        public XmlILGenerator() {
        }

        /// <summary>
        /// Given the logical query plan (QilExpression) generate a physical query plan (MSIL) that can be executed.
        /// </summary>
        // SxS Note: The way the trace file names are created (hardcoded) is NOT SxS safe. However the files are
        // created only for internal tracing purposes. In addition XmlILTrace class is not compiled into retail 
        // builds. As a result it is fine to suppress the FxCop SxS warning.  
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        public XmlILCommand Generate(QilExpression query, TypeBuilder typeBldr) {
            this.qil = query;

            bool useLRE = (
                !this.qil.IsDebug &&
                (typeBldr == null)
#if DEBUG
                && !XmlILTrace.IsEnabled // Dump assembly to disk; can't do this when using LRE
#endif
            );
            bool emitSymbols = this.qil.IsDebug;

            // In debug code, ensure that input QIL is correct
            QilValidationVisitor.Validate(this.qil);

        #if DEBUG
            // Trace Qil before optimization
            XmlILTrace.WriteQil(this.qil, "qilbefore.xml");

            // Trace optimizations
            XmlILTrace.TraceOptimizations(this.qil, "qilopt.xml");
        #endif

            // Optimize and annotate the Qil graph
            this.optVisitor = new XmlILOptimizerVisitor(this.qil, !this.qil.IsDebug);
            this.qil = this.optVisitor.Optimize();

            // In debug code, ensure that output QIL is correct
            QilValidationVisitor.Validate(this.qil);

        #if DEBUG
            // Trace Qil after optimization
            XmlILTrace.WriteQil(this.qil, "qilafter.xml");
        #endif

            XmlILModule.CreateModulePermissionSet.Assert();

            // Create module in which methods will be generated
            if (typeBldr != null) {
                this.module = new XmlILModule(typeBldr);
            } else {
                this.module = new XmlILModule(useLRE, emitSymbols);
            }

            // Create a code generation helper for the module; enable optimizations if IsDebug is false
            this.helper = new GenerateHelper(this.module, this.qil.IsDebug);

            // Create helper methods
            CreateHelperFunctions();

            // Create metadata for the Execute function, which is the entry point to the query
            // public static void Execute(XmlQueryRuntime);
            MethodInfo methExec = this.module.DefineMethod("Execute", typeof(void), new Type[] { }, new string[] { }, XmlILMethodAttributes.NonUser);

            // Create metadata for the root expression
            // public void Root()
            Debug.Assert(this.qil.Root != null);
            XmlILMethodAttributes methAttrs = (this.qil.Root.SourceLine == null) ? XmlILMethodAttributes.NonUser : XmlILMethodAttributes.None;
            MethodInfo methRoot = this.module.DefineMethod("Root", typeof(void), new Type[] { }, new string[] { }, methAttrs);

            // Declare all early bound function objects
            foreach (EarlyBoundInfo info in this.qil.EarlyBoundTypes) {
                this.helper.StaticData.DeclareEarlyBound(info.NamespaceUri, info.EarlyBoundType);
            }

            // Create metadata for each QilExpression function that has at least one caller
            CreateFunctionMetadata(this.qil.FunctionList);

            // Create metadata for each QilExpression global variable and parameter
            CreateGlobalValueMetadata(this.qil.GlobalVariableList);
            CreateGlobalValueMetadata(this.qil.GlobalParameterList);

            // Generate Execute method
            GenerateExecuteFunction(methExec, methRoot);

            // Visit the QilExpression graph
            this.xmlIlVisitor = new XmlILVisitor();
            this.xmlIlVisitor.Visit(this.qil, this.helper, methRoot);

            // Collect all static information required by the runtime
            XmlQueryStaticData staticData = new XmlQueryStaticData(
                this.qil.DefaultWriterSettings,
                this.qil.WhitespaceRules,
                this.helper.StaticData
            );

            // Create static constructor that initializes XmlQueryStaticData instance at runtime
            if (typeBldr != null) {
                CreateTypeInitializer(staticData);

                // Finish up creation of the type
                this.module.BakeMethods();

                return null;
            } else {
                // Finish up creation of the type
                this.module.BakeMethods();

                // Create delegate over "Execute" method
                ExecuteDelegate delExec = (ExecuteDelegate)this.module.CreateDelegate("Execute", typeof(ExecuteDelegate));
                return new XmlILCommand(delExec, staticData);
            }
        }

        /// <summary>
        /// Create MethodBuilder metadata for the specified QilExpression function.  Annotate ndFunc with the
        /// MethodBuilder.  Also, each QilExpression argument type should be converted to a corresponding Clr type.
        /// Each argument QilExpression node should be annotated with the resulting ParameterBuilder.
        /// </summary>
        private void CreateFunctionMetadata(IList<QilNode> funcList) {
            MethodInfo methInfo;
            Type[] paramTypes;
            string[] paramNames;
            Type typReturn;
            XmlILMethodAttributes methAttrs;

            foreach (QilFunction ndFunc in funcList) {
                paramTypes = new Type[ndFunc.Arguments.Count];
                paramNames = new string[ndFunc.Arguments.Count];

                // Loop through all other parameters and save their types in the array
                for (int arg = 0; arg < ndFunc.Arguments.Count; arg ++) {
                    QilParameter ndParam = (QilParameter) ndFunc.Arguments[arg];
                    Debug.Assert(ndParam.NodeType == QilNodeType.Parameter);

                    // Get the type of each argument as a Clr type
                    paramTypes[arg] = XmlILTypeHelper.GetStorageType(ndParam.XmlType);

                    // Get the name of each argument
                    if (ndParam.DebugName != null)
                        paramNames[arg] = ndParam.DebugName;
                }

                // Get the type of the return value
                if (XmlILConstructInfo.Read(ndFunc).PushToWriterLast) {
                    // Push mode functions do not have a return value
                    typReturn = typeof(void);
                }
                else {
                    // Pull mode functions have a return value
                    typReturn = XmlILTypeHelper.GetStorageType(ndFunc.XmlType);
                }

                // Create the method metadata
                methAttrs = ndFunc.SourceLine == null ? XmlILMethodAttributes.NonUser : XmlILMethodAttributes.None;
                methInfo = this.module.DefineMethod(ndFunc.DebugName, typReturn, paramTypes, paramNames, methAttrs);

                for (int arg = 0; arg < ndFunc.Arguments.Count; arg ++) {
                    // Set location of parameter on Let node annotation
                    XmlILAnnotation.Write(ndFunc.Arguments[arg]).ArgumentPosition = arg;
                }

                // Annotate function with the MethodInfo
                XmlILAnnotation.Write(ndFunc).FunctionBinding = methInfo;
            }
        }

        /// <summary>
        /// Generate metadata for a method that calculates a global value.
        /// </summary>
        private void CreateGlobalValueMetadata(IList<QilNode> globalList) {
            MethodInfo methInfo;
            Type typReturn;
            XmlILMethodAttributes methAttrs;

            foreach (QilReference ndRef in globalList) {
                // public T GlobalValue()
                typReturn = XmlILTypeHelper.GetStorageType(ndRef.XmlType);
                methAttrs = ndRef.SourceLine == null ? XmlILMethodAttributes.NonUser : XmlILMethodAttributes.None;
                methInfo = this.module.DefineMethod(ndRef.DebugName.ToString(), typReturn, new Type[] {}, new string[] {}, methAttrs);

                // Annotate function with MethodBuilder
                XmlILAnnotation.Write(ndRef).FunctionBinding = methInfo;
            }
        }

        /// <summary>
        /// Generate the "Execute" method, which is the entry point to the query.
        /// </summary>
        private MethodInfo GenerateExecuteFunction(MethodInfo methExec, MethodInfo methRoot) {
            this.helper.MethodBegin(methExec, null, false);

            // Force some or all global values to be evaluated at start of query
            EvaluateGlobalValues(this.qil.GlobalVariableList);
            EvaluateGlobalValues(this.qil.GlobalParameterList);

            // Root(runtime);
            this.helper.LoadQueryRuntime();
            this.helper.Call(methRoot);

            this.helper.MethodEnd();

            return methExec;
        }

        /// <summary>
        /// Create and generate various helper methods, which are called by the generated code.
        /// </summary>
        private void CreateHelperFunctions() {
            MethodInfo meth;
            Label lblClone;

            // public static XPathNavigator SyncToNavigator(XPathNavigator, XPathNavigator);
            meth = this.module.DefineMethod(
                            "SyncToNavigator",
                            typeof(XPathNavigator),
                            new Type[] {typeof(XPathNavigator), typeof(XPathNavigator)},
                            new string[] {null, null},
                            XmlILMethodAttributes.NonUser | XmlILMethodAttributes.Raw);

            this.helper.MethodBegin(meth, null, false);

            // if (navigatorThis != null && navigatorThis.MoveTo(navigatorThat))
            //     return navigatorThis;
            lblClone = this.helper.DefineLabel();
            this.helper.Emit(OpCodes.Ldarg_0);
            this.helper.Emit(OpCodes.Brfalse, lblClone);
            this.helper.Emit(OpCodes.Ldarg_0);
            this.helper.Emit(OpCodes.Ldarg_1);
            this.helper.Call(XmlILMethods.NavMoveTo);
            this.helper.Emit(OpCodes.Brfalse, lblClone);
            this.helper.Emit(OpCodes.Ldarg_0);
            this.helper.Emit(OpCodes.Ret);

            // LabelClone:
            // return navigatorThat.Clone();
            this.helper.MarkLabel(lblClone);
            this.helper.Emit(OpCodes.Ldarg_1);
            this.helper.Call(XmlILMethods.NavClone);

            this.helper.MethodEnd();
        }

        /// <summary>
        /// Generate code to force evaluation of some or all global variables and/or parameters.
        /// </summary>
        private void EvaluateGlobalValues(IList<QilNode> iterList) {
            MethodInfo methInfo;

            foreach (QilIterator ndIter in iterList) {
                // Evaluate global if generating debug code, or if global might have side effects
                if (this.qil.IsDebug || OptimizerPatterns.Read(ndIter).MatchesPattern(OptimizerPatternName.MaybeSideEffects)) {
                    // Get MethodInfo that evaluates the global value and discard its return value
                    methInfo = XmlILAnnotation.Write(ndIter).FunctionBinding;
                    Debug.Assert(methInfo != null, "MethodInfo for global value should have been created previously.");

                    this.helper.LoadQueryRuntime();
                    this.helper.Call(methInfo);
                    this.helper.Emit(OpCodes.Pop);
                }
            }
        }

        /// <summary>
        /// Create static constructor that initializes XmlQueryStaticData instance at runtime.
        /// </summary>
        public void CreateTypeInitializer(XmlQueryStaticData staticData) {
            byte[] data;
            Type[] ebTypes;
            FieldInfo fldInitData, fldData, fldTypes;
            ConstructorInfo cctor;

            staticData.GetObjectData(out data, out ebTypes);
            fldInitData = this.module.DefineInitializedData("__" + XmlQueryStaticData.DataFieldName, data);
            fldData = this.module.DefineField(XmlQueryStaticData.DataFieldName, typeof(object));
            fldTypes = this.module.DefineField(XmlQueryStaticData.TypesFieldName, typeof(Type[]));

            cctor = this.module.DefineTypeInitializer();
            this.helper.MethodBegin(cctor, null, false);

            // s_data = new byte[s_initData.Length] { s_initData };
            this.helper.LoadInteger(data.Length);
            this.helper.Emit(OpCodes.Newarr, typeof(byte));
            this.helper.Emit(OpCodes.Dup);
            this.helper.Emit(OpCodes.Ldtoken, fldInitData);
            this.helper.Call(XmlILMethods.InitializeArray);
            this.helper.Emit(OpCodes.Stsfld, fldData);

            if (ebTypes != null) {
                // Type[] types = new Type[s_ebTypes.Length];
                LocalBuilder locTypes = this.helper.DeclareLocal("$$$types", typeof(Type[]));
                this.helper.LoadInteger(ebTypes.Length);
                this.helper.Emit(OpCodes.Newarr, typeof(Type));
                this.helper.Emit(OpCodes.Stloc, locTypes);

                for (int idx = 0; idx < ebTypes.Length; idx++) {
                    // types[idx] = ebTypes[idx];
                    this.helper.Emit(OpCodes.Ldloc, locTypes);
                    this.helper.LoadInteger(idx);
                    this.helper.LoadType(ebTypes[idx]);
                    this.helper.Emit(OpCodes.Stelem_Ref);
                }

                // s_types = types;
                this.helper.Emit(OpCodes.Ldloc, locTypes);
                this.helper.Emit(OpCodes.Stsfld, fldTypes);
            }

            this.helper.MethodEnd();
        }
    }
}
