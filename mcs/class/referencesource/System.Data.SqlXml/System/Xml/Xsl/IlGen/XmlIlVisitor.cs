//------------------------------------------------------------------------------
// <copyright file="XmlIlVisitor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Xsl;
using System.Xml.Xsl.Qil;
using System.Xml.Xsl.Runtime;

namespace System.Xml.Xsl.IlGen {
    using TypeFactory   = System.Xml.Xsl.XmlQueryTypeFactory;
    using Res           = System.Xml.Utils.Res;

    /// <summary>
    /// Creates Msil code for an entire QilExpression graph.  Code is generated in one of two modes: push or
    /// pull.  In push mode, code is generated to push the values in an iterator to the XmlWriter
    /// interface.  In pull mode, the values in an iterator are stored in a physical location such as
    /// the stack or a local variable by an iterator.  The iterator is passive, and will just wait for
    /// a caller to pull the data and/or instruct the iterator to enumerate the next value.
    /// </summary>
    internal class XmlILVisitor : QilVisitor {
        private QilExpression qil;
        private GenerateHelper helper;
        private IteratorDescriptor iterCurr, iterNested;
        private int indexId;


        //-----------------------------------------------
        // Entry
        //-----------------------------------------------

        /// <summary>
        /// Visits the specified QilExpression graph and generates MSIL code.
        /// </summary>
        public void Visit(QilExpression qil, GenerateHelper helper, MethodInfo methRoot) {
            this.qil = qil;
            this.helper = helper;
            this.iterNested = null;
            this.indexId = 0;

            // Prepare each global parameter and global variable to be visited
            PrepareGlobalValues(qil.GlobalParameterList);
            PrepareGlobalValues(qil.GlobalVariableList);

            // Visit each global parameter and global variable
            VisitGlobalValues(qil.GlobalParameterList);
            VisitGlobalValues(qil.GlobalVariableList);

            // Build each function
            foreach (QilFunction ndFunc in qil.FunctionList) {
                // Visit each parameter and the function body
                Function(ndFunc);
            }

            // Build the root expression
            this.helper.MethodBegin(methRoot, null, true);
            StartNestedIterator(qil.Root);
            Visit(qil.Root);
            Debug.Assert(this.iterCurr.Storage.Location == ItemLocation.None, "Root expression should have been pushed to the writer.");
            EndNestedIterator(qil.Root);
            this.helper.MethodEnd();
        }

        /// <summary>
        /// Create IteratorDescriptor for each global value.  This pre-visit is necessary because a global early
        /// in the list may reference a global later in the list and therefore expect its IteratorDescriptor to already
        /// be initialized.
        /// </summary>
        private void PrepareGlobalValues(QilList globalIterators) {
            MethodInfo methGlobal;
            IteratorDescriptor iterInfo;

            foreach (QilIterator iter in globalIterators) {
                Debug.Assert(iter.NodeType == QilNodeType.Let || iter.NodeType == QilNodeType.Parameter);

                // Get metadata for method which computes this global's value
                methGlobal = XmlILAnnotation.Write(iter).FunctionBinding;
                Debug.Assert(methGlobal != null, "Metadata for global value should have already been computed");

                // Create an IteratorDescriptor for this global value
                iterInfo = new IteratorDescriptor(this.helper);

                // Iterator items will be stored in a global location
                iterInfo.Storage = StorageDescriptor.Global(methGlobal, GetItemStorageType(iter), !iter.XmlType.IsSingleton);

                // Associate IteratorDescriptor with parameter
                XmlILAnnotation.Write(iter).CachedIteratorDescriptor = iterInfo;
            }
        }

        /// <summary>
        /// Visit each global variable or parameter.  Create a IteratorDescriptor for each global value.  Generate code for
        /// default values.
        /// </summary>
        private void VisitGlobalValues(QilList globalIterators) {
            MethodInfo methGlobal;
            Label lblGetGlobal, lblComputeGlobal;
            bool isCached;
            int idxValue;

            foreach (QilIterator iter in globalIterators) {
                QilParameter param = iter as QilParameter;

                // Get MethodInfo for method that computes the value of this global
                methGlobal = XmlILAnnotation.Write(iter).CachedIteratorDescriptor.Storage.GlobalLocation;
                isCached = !iter.XmlType.IsSingleton;

                // Notify the StaticDataManager of the new global value
                idxValue = this.helper.StaticData.DeclareGlobalValue(iter.DebugName);

                // Generate code for this method
                this.helper.MethodBegin(methGlobal, iter.SourceLine, false);

                lblGetGlobal = this.helper.DefineLabel();
                lblComputeGlobal = this.helper.DefineLabel();

                // if (runtime.IsGlobalComputed(idx)) goto LabelGetGlobal;
                this.helper.LoadQueryRuntime();
                this.helper.LoadInteger(idxValue);
                this.helper.Call(XmlILMethods.GlobalComputed);
                this.helper.Emit(OpCodes.Brtrue, lblGetGlobal);

                // Compute value of global value
                StartNestedIterator(iter);

                if (param != null) {
                    Debug.Assert(iter.XmlType == TypeFactory.ItemS, "IlGen currently only supports parameters of type item*.");

                    // param = runtime.ExternalContext.GetParameter(localName, namespaceUri);
                    // if (param == null) goto LabelComputeGlobal;
                    LocalBuilder locParam = this.helper.DeclareLocal("$$$param", typeof(object));
                    this.helper.CallGetParameter(param.Name.LocalName, param.Name.NamespaceUri);
                    this.helper.Emit(OpCodes.Stloc, locParam);
                    this.helper.Emit(OpCodes.Ldloc, locParam);
                    this.helper.Emit(OpCodes.Brfalse, lblComputeGlobal);

                    // runtime.SetGlobalValue(idxValue, runtime.ChangeTypeXsltResult(idxType, value));
                    // Ensure that the storage type of the parameter corresponds to static type
                    this.helper.LoadQueryRuntime();
                    this.helper.LoadInteger(idxValue);

                    this.helper.LoadQueryRuntime();
                    this.helper.LoadInteger(this.helper.StaticData.DeclareXmlType(XmlQueryTypeFactory.ItemS));
                    this.helper.Emit(OpCodes.Ldloc, locParam);
                    this.helper.Call(XmlILMethods.ChangeTypeXsltResult);

                    this.helper.CallSetGlobalValue(typeof(object));

                    // goto LabelGetGlobal;
                    this.helper.EmitUnconditionalBranch(OpCodes.Br, lblGetGlobal);
                }

                // LabelComputeGlobal:
                this.helper.MarkLabel(lblComputeGlobal);

                if (iter.Binding != null) {
                    // runtime.SetGlobalValue(idxValue, (object) value);
                    this.helper.LoadQueryRuntime();
                    this.helper.LoadInteger(idxValue);

                    // Compute value of global value
                    NestedVisitEnsureStack(iter.Binding, GetItemStorageType(iter), isCached);

                    this.helper.CallSetGlobalValue(GetStorageType(iter));
                }
                else {
                    // Throw exception, as there is no default value for this parameter
                    // XmlQueryRuntime.ThrowException("...");
                    Debug.Assert(iter.NodeType == QilNodeType.Parameter, "Only parameters may not have a default value");
                    this.helper.LoadQueryRuntime();
                    this.helper.Emit(OpCodes.Ldstr, Res.GetString(Res.XmlIl_UnknownParam, new string[] {param.Name.LocalName, param.Name.NamespaceUri}));
                    this.helper.Call(XmlILMethods.ThrowException);
                }
                
                EndNestedIterator(iter);

                // LabelGetGlobal:
                // return (T) runtime.GetGlobalValue(idxValue);
                this.helper.MarkLabel(lblGetGlobal);
                this.helper.CallGetGlobalValue(idxValue, GetStorageType(iter));

                this.helper.MethodEnd();
            }
        }

        /// <summary>
        /// Generate code for the specified function.
        /// </summary>
        private void Function(QilFunction ndFunc) {
            MethodInfo methFunc;
            int paramId;
            IteratorDescriptor iterInfo;
            bool useWriter;

            // Annotate each function parameter with a IteratorDescriptor
            foreach (QilIterator iter in ndFunc.Arguments) {
                Debug.Assert(iter.NodeType == QilNodeType.Parameter);

                // Create an IteratorDescriptor for this parameter
                iterInfo = new IteratorDescriptor(this.helper);

                // Add one to parameter index, as 0th parameter is always "this"
                paramId = XmlILAnnotation.Write(iter).ArgumentPosition + 1;

                // The ParameterInfo for each argument should be set as its location
                iterInfo.Storage = StorageDescriptor.Parameter(paramId, GetItemStorageType(iter), !iter.XmlType.IsSingleton);

                // Associate IteratorDescriptor with Let iterator
                XmlILAnnotation.Write(iter).CachedIteratorDescriptor = iterInfo;
            }

            methFunc = XmlILAnnotation.Write(ndFunc).FunctionBinding;
            useWriter = (XmlILConstructInfo.Read(ndFunc).ConstructMethod == XmlILConstructMethod.Writer);

            // Generate query code from QilExpression tree
            this.helper.MethodBegin(methFunc, ndFunc.SourceLine, useWriter);

            foreach (QilIterator iter in ndFunc.Arguments) {
                // DebugInfo: Sequence point just before generating code for the bound expression
                if (this.qil.IsDebug && iter.SourceLine != null)
                    this.helper.DebugSequencePoint(iter.SourceLine);

                // Calculate default value of this parameter
                if (iter.Binding != null) {
                    Debug.Assert(iter.XmlType == TypeFactory.ItemS, "IlGen currently only supports default values in parameters of type item*.");
                    paramId = (iter.Annotation as XmlILAnnotation).ArgumentPosition + 1;

                    // runtime.MatchesXmlType(param, XmlTypeCode.QName);
                    Label lblLocalComputed = this.helper.DefineLabel();
                    this.helper.LoadQueryRuntime();
                    this.helper.LoadParameter(paramId);
                    this.helper.LoadInteger((int)XmlTypeCode.QName);
                    this.helper.Call(XmlILMethods.SeqMatchesCode);

                    this.helper.Emit(OpCodes.Brfalse, lblLocalComputed);

                    // Compute default value of this parameter
                    StartNestedIterator(iter);
                    NestedVisitEnsureStack(iter.Binding, GetItemStorageType(iter), /*isCached:*/!iter.XmlType.IsSingleton);
                    EndNestedIterator(iter);

                    this.helper.SetParameter(paramId);
                    this.helper.MarkLabel(lblLocalComputed);
                }
            }

            StartNestedIterator(ndFunc);

            // If function did not push results to writer, then function will return value(s) (rather than void)
            if (useWriter)
                NestedVisit(ndFunc.Definition);
            else
                NestedVisitEnsureStack(ndFunc.Definition, GetItemStorageType(ndFunc), !ndFunc.XmlType.IsSingleton);

            EndNestedIterator(ndFunc);

            this.helper.MethodEnd();
        }

        //-----------------------------------------------
        // QilVisitor
        //-----------------------------------------------

        /// <summary>
        /// Generate a query plan for the QilExpression subgraph.
        /// </summary>
        protected override QilNode Visit(QilNode nd) {
            if (nd == null)
                return null;

            // DebugInfo: Sequence point just before generating code for this expression
            if (this.qil.IsDebug && nd.SourceLine != null && !(nd is QilIterator))
                this.helper.DebugSequencePoint(nd.SourceLine);

            // Expressions are constructed using one of several possible methods
            switch (XmlILConstructInfo.Read(nd).ConstructMethod) {
                case XmlILConstructMethod.WriterThenIterator:
                    // Push results of expression to cached writer; then iterate over cached results
                    NestedConstruction(nd);
                    break;

                case XmlILConstructMethod.IteratorThenWriter:
                    // Iterate over items in the sequence; send items to writer
                    CopySequence(nd);
                    break;

                case XmlILConstructMethod.Iterator:
                    Debug.Assert(nd.XmlType.IsSingleton || CachesResult(nd) || this.iterCurr.HasLabelNext,
                                 "When generating code for a non-singleton expression, LabelNext must be defined.");
                    goto default;

                default:
                    // Allow base internal class to dispatch to correct Visit method
                    base.Visit(nd);
                    break;
            }

            return nd;
        }

        /// <summary>
        /// VisitChildren should never be called.
        /// </summary>
        protected override QilNode VisitChildren(QilNode parent) {
            Debug.Fail("Visit" + parent.NodeType + " should never be called");
            return parent;
        }

        /// <summary>
        /// Generate code to cache a sequence of items that are pushed to output.
        /// </summary>
        private void NestedConstruction(QilNode nd) {
            // Start nested construction of a sequence of items
            this.helper.CallStartSequenceConstruction();

            // Allow base internal class to dispatch to correct Visit method
            base.Visit(nd);

            // Get the result sequence
            this.helper.CallEndSequenceConstruction();
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XPathItem), true);
        }

        /// <summary>
        /// Iterate over items produced by the "nd" expression and copy each item to output.
        /// </summary>
        private void CopySequence(QilNode nd) {
            XmlQueryType typ = nd.XmlType;
            bool hasOnEnd;
            Label lblOnEnd;

            StartWriterLoop(nd, out hasOnEnd, out lblOnEnd);

            if (typ.IsSingleton) {
                // Always write atomic values via XmlQueryOutput
                this.helper.LoadQueryOutput();

                // Allow base internal class to dispatch to correct Visit method
                base.Visit(nd);
                this.iterCurr.EnsureItemStorageType(nd.XmlType, typeof(XPathItem));
            }
            else {
                // Allow base internal class to dispatch to correct Visit method
                base.Visit(nd);
                this.iterCurr.EnsureItemStorageType(nd.XmlType, typeof(XPathItem));

                // Save any stack values in a temporary local
                this.iterCurr.EnsureNoStackNoCache("$$$copyTemp");

                this.helper.LoadQueryOutput();
            }

            // Write value to output
            this.iterCurr.EnsureStackNoCache();
            this.helper.Call(XmlILMethods.WriteItem);

            EndWriterLoop(nd, hasOnEnd, lblOnEnd);
        }

        /// <summary>
        /// Generate code for QilNodeType.DataSource.
        /// </summary>
        /// <remarks>
        /// Generates code to retrieve a document using the XmlResolver.
        /// </remarks>
        protected override QilNode VisitDataSource(QilDataSource ndSrc) {
            LocalBuilder locNav;

            // XPathNavigator navDoc = runtime.ExternalContext.GetEntity(uri)
            this.helper.LoadQueryContext();
            NestedVisitEnsureStack(ndSrc.Name);
            NestedVisitEnsureStack(ndSrc.BaseUri);
            this.helper.Call(XmlILMethods.GetDataSource);

            locNav = this.helper.DeclareLocal("$$$navDoc", typeof(XPathNavigator));
            this.helper.Emit(OpCodes.Stloc, locNav);

            // if (navDoc == null) goto LabelNextCtxt;
            this.helper.Emit(OpCodes.Ldloc, locNav);
            this.helper.Emit(OpCodes.Brfalse, this.iterCurr.GetLabelNext());

            this.iterCurr.Storage = StorageDescriptor.Local(locNav, typeof(XPathNavigator), false);

            return ndSrc;
        }

        /// <summary>
        /// Generate code for QilNodeType.Nop.
        /// </summary>
        protected override QilNode VisitNop(QilUnary ndNop) {
            return Visit(ndNop.Child);
        }
        
        /// <summary>
        /// Generate code for QilNodeType.OptimizeBarrier.
        /// </summary>
        protected override QilNode VisitOptimizeBarrier(QilUnary ndBarrier) {
            return Visit(ndBarrier.Child);
        }

        /// <summary>
        /// Generate code for QilNodeType.Error.
        /// </summary>
        protected override QilNode VisitError(QilUnary ndErr) {
            // XmlQueryRuntime.ThrowException(strErr);
            this.helper.LoadQueryRuntime();
            NestedVisitEnsureStack(ndErr.Child);
            this.helper.Call(XmlILMethods.ThrowException);

            if (XmlILConstructInfo.Read(ndErr).ConstructMethod == XmlILConstructMethod.Writer) {
                this.iterCurr.Storage = StorageDescriptor.None();
            }
            else {
                // Push dummy value so that Location is not None and IL rules are met
                this.helper.Emit(OpCodes.Ldnull);
                this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XPathItem), false);
            }

            return ndErr;
        }

        /// <summary>
        /// Generate code for QilNodeType.Warning.
        /// </summary>
        protected override QilNode VisitWarning(QilUnary ndWarning) {
            // runtime.SendMessage(strWarning);
            this.helper.LoadQueryRuntime();
            NestedVisitEnsureStack(ndWarning.Child);
            this.helper.Call(XmlILMethods.SendMessage);

            if (XmlILConstructInfo.Read(ndWarning).ConstructMethod == XmlILConstructMethod.Writer)
                this.iterCurr.Storage = StorageDescriptor.None();
            else
                VisitEmpty(ndWarning);

            return ndWarning;
        }

        /// <summary>
        /// Generate code for QilNodeType.True.
        /// </summary>
        /// <remarks>
        /// BranchingContext.OnFalse context: [nothing]
        /// BranchingContext.OnTrue context:  goto LabelParent;
        /// BranchingContext.None context:  push true();
        /// </remarks>
        protected override QilNode VisitTrue(QilNode ndTrue) {
            if (this.iterCurr.CurrentBranchingContext != BranchingContext.None) {
                // Make sure there's an IL code path to both the true and false branches in order to avoid dead
                // code which can cause IL verification errors.
                this.helper.EmitUnconditionalBranch(this.iterCurr.CurrentBranchingContext == BranchingContext.OnTrue ?
                        OpCodes.Brtrue : OpCodes.Brfalse, this.iterCurr.LabelBranch);

                this.iterCurr.Storage = StorageDescriptor.None();
            }
            else {
                // Push boolean result onto the stack
                this.helper.LoadBoolean(true);
                this.iterCurr.Storage = StorageDescriptor.Stack(typeof(bool), false);
            }

            return ndTrue;
        }

        /// <summary>
        /// Generate code for QilNodeType.False.
        /// </summary>
        /// <remarks>
        /// BranchingContext.OnFalse context: goto LabelParent;
        /// BranchingContext.OnTrue context:  [nothing]
        /// BranchingContext.None context:  push false();
        /// </remarks>
        protected override QilNode VisitFalse(QilNode ndFalse) {
            if (this.iterCurr.CurrentBranchingContext != BranchingContext.None) {
                // Make sure there's an IL code path to both the true and false branches in order to avoid dead
                // code which can cause IL verification errors.
                this.helper.EmitUnconditionalBranch(this.iterCurr.CurrentBranchingContext == BranchingContext.OnFalse ?
                        OpCodes.Brtrue : OpCodes.Brfalse, this.iterCurr.LabelBranch);

                this.iterCurr.Storage = StorageDescriptor.None();
            }
            else {
                // Push boolean result onto the stack
                this.helper.LoadBoolean(false);
                this.iterCurr.Storage = StorageDescriptor.Stack(typeof(bool), false);
            }

            return ndFalse;
        }

        /// <summary>
        /// Generate code for QilNodeType.LiteralString.
        /// </summary>
        protected override QilNode VisitLiteralString(QilLiteral ndStr) {
            this.helper.Emit(OpCodes.Ldstr, (string) ndStr);
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(string), false);
            return ndStr;
        }

        /// <summary>
        /// Generate code for QilNodeType.LiteralInt32.
        /// </summary>
        protected override QilNode VisitLiteralInt32(QilLiteral ndInt) {
            this.helper.LoadInteger((int) ndInt);
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(int), false);
            return ndInt;
        }

        /// <summary>
        /// Generate code for QilNodeType.LiteralInt64.
        /// </summary>
        protected override QilNode VisitLiteralInt64(QilLiteral ndLong) {
            this.helper.Emit(OpCodes.Ldc_I8, (long) ndLong);
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(long), false);
            return ndLong;
        }

        /// <summary>
        /// Generate code for QilNodeType.LiteralDouble.
        /// </summary>
        protected override QilNode VisitLiteralDouble(QilLiteral ndDbl) {
            this.helper.Emit(OpCodes.Ldc_R8, (double) ndDbl);
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(double), false);
            return ndDbl;
        }

        /// <summary>
        /// Generate code for QilNodeType.LiteralDecimal.
        /// </summary>
        protected override QilNode VisitLiteralDecimal(QilLiteral ndDec) {
            this.helper.ConstructLiteralDecimal((decimal) ndDec);
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(decimal), false);
            return ndDec;
        }

        /// <summary>
        /// Generate code for QilNodeType.LiteralQName.
        /// </summary>
        protected override QilNode VisitLiteralQName(QilName ndQName) {
            this.helper.ConstructLiteralQName(ndQName.LocalName, ndQName.NamespaceUri);
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XmlQualifiedName), false);
            return ndQName;
        }

        /// <summary>
        /// Generate code for QilNodeType.And.
        /// </summary>
        /// <remarks>
        /// BranchingContext.OnFalse context: (expr1) and (expr2)
        /// ==> if (!expr1) goto LabelParent;
        ///     if (!expr2) goto LabelParent;
        ///
        /// BranchingContext.OnTrue context: (expr1) and (expr2)
        /// ==> if (!expr1) goto LabelTemp;
        ///     if (expr1) goto LabelParent;
        ///     LabelTemp:
        ///
        /// BranchingContext.None context: (expr1) and (expr2)
        /// ==> if (!expr1) goto LabelTemp;
        ///     if (!expr1) goto LabelTemp;
        ///     push true();
        ///     goto LabelSkip;
        ///     LabelTemp:
        ///     push false();
        ///     LabelSkip:
        ///
        /// </remarks>
        protected override QilNode VisitAnd(QilBinary ndAnd) {
            IteratorDescriptor iterParent = this.iterCurr;
            Label lblOnFalse;

            // Visit left branch
            StartNestedIterator(ndAnd.Left);
            lblOnFalse = StartConjunctiveTests(iterParent.CurrentBranchingContext, iterParent.LabelBranch);
            Visit(ndAnd.Left);
            EndNestedIterator(ndAnd.Left);

            // Visit right branch
            StartNestedIterator(ndAnd.Right);
            StartLastConjunctiveTest(iterParent.CurrentBranchingContext, iterParent.LabelBranch, lblOnFalse);
            Visit(ndAnd.Right);
            EndNestedIterator(ndAnd.Right);

            // End And expression
            EndConjunctiveTests(iterParent.CurrentBranchingContext, iterParent.LabelBranch, lblOnFalse);

            return ndAnd;
        }

        /// <summary>
        /// Fixup branching context for all but the last test in a conjunctive (Logical And) expression.
        /// Return a temporary label which will be passed to StartLastAndBranch() and EndAndBranch().
        /// </summary>
        private Label StartConjunctiveTests(BranchingContext brctxt, Label lblBranch) {
            Label lblOnFalse;

            switch (brctxt) {
                case BranchingContext.OnFalse:
                    // If condition evaluates to false, branch to false label
                    this.iterCurr.SetBranching(BranchingContext.OnFalse, lblBranch);
                    return lblBranch;

                default:
                    // If condition evaluates to false:
                    //   1. Jump to new false label that will be fixed just beyond the second condition
                    //   2. Or, jump to code that pushes "false"
                    lblOnFalse = this.helper.DefineLabel();
                    this.iterCurr.SetBranching(BranchingContext.OnFalse, lblOnFalse);
                    return lblOnFalse;
            }
        }

        /// <summary>
        /// Fixup branching context for the last test in a conjunctive (Logical And) expression.
        /// </summary>
        private void StartLastConjunctiveTest(BranchingContext brctxt, Label lblBranch, Label lblOnFalse) {
            switch (brctxt) {
                case BranchingContext.OnTrue:
                    // If last condition evaluates to true, branch to true label
                    this.iterCurr.SetBranching(BranchingContext.OnTrue, lblBranch);
                    break;

                default:
                    // If last condition evalutes to false, branch to false label
                    // Else fall through to true code path
                    this.iterCurr.SetBranching(BranchingContext.OnFalse, lblOnFalse);
                    break;
            }
        }

        /// <summary>
        /// Anchor any remaining labels.
        /// </summary>
        private void EndConjunctiveTests(BranchingContext brctxt, Label lblBranch, Label lblOnFalse) {
            switch (brctxt) {
                case BranchingContext.OnTrue:
                    // Anchor false label
                    this.helper.MarkLabel(lblOnFalse);
                    goto case BranchingContext.OnFalse;

                case BranchingContext.OnFalse:
                    this.iterCurr.Storage = StorageDescriptor.None();
                    break;

                case BranchingContext.None:
                    // Convert branch targets into push of true/false
                    this.helper.ConvBranchToBool(lblOnFalse, false);
                    this.iterCurr.Storage = StorageDescriptor.Stack(typeof(bool), false);
                    break;
            }
        }

        /// <summary>
        /// Generate code for QilNodeType.Or.
        /// </summary>
        /// <remarks>
        /// BranchingContext.OnFalse context: (expr1) or (expr2)
        /// ==> if (expr1) goto LabelTemp;
        ///     if (!expr2) goto LabelParent;
        ///     LabelTemp:
        ///
        /// BranchingContext.OnTrue context: (expr1) or (expr2)
        /// ==> if (expr1) goto LabelParent;
        ///     if (expr1) goto LabelParent;
        ///
        /// BranchingContext.None context: (expr1) or (expr2)
        /// ==> if (expr1) goto LabelTemp;
        ///     if (expr1) goto LabelTemp;
        ///     push false();
        ///     goto LabelSkip;
        ///     LabelTemp:
        ///     push true();
        ///     LabelSkip:
        ///
        /// </remarks>
        protected override QilNode VisitOr(QilBinary ndOr) {
            Label lblTemp = new Label();

            // Visit left branch
            switch (this.iterCurr.CurrentBranchingContext) {
                case BranchingContext.OnFalse:
                    // If left condition evaluates to true, jump to new label that will be fixed
                    // just beyond the second condition
                    lblTemp = this.helper.DefineLabel();
                    NestedVisitWithBranch(ndOr.Left, BranchingContext.OnTrue, lblTemp);
                    break;

                case BranchingContext.OnTrue:
                    // If left condition evaluates to true, branch to true label
                    NestedVisitWithBranch(ndOr.Left, BranchingContext.OnTrue, this.iterCurr.LabelBranch);
                    break;

                default:
                    // If left condition evalutes to true, jump to code that pushes "true"
                    Debug.Assert(this.iterCurr.CurrentBranchingContext == BranchingContext.None);
                    lblTemp = this.helper.DefineLabel();
                    NestedVisitWithBranch(ndOr.Left, BranchingContext.OnTrue, lblTemp);
                    break;
            }

            // Visit right branch
            switch (this.iterCurr.CurrentBranchingContext) {
                case BranchingContext.OnFalse:
                    // If right condition evaluates to false, branch to false label
                    NestedVisitWithBranch(ndOr.Right, BranchingContext.OnFalse, this.iterCurr.LabelBranch);
                    break;

                case BranchingContext.OnTrue:
                    // If right condition evaluates to true, branch to true label
                    NestedVisitWithBranch(ndOr.Right, BranchingContext.OnTrue, this.iterCurr.LabelBranch);
                    break;

                default:
                    // If right condition evalutes to true, jump to code that pushes "true".
                    // Otherwise, if both conditions evaluate to false, fall through code path
                    // will push "false".
                    NestedVisitWithBranch(ndOr.Right, BranchingContext.OnTrue, lblTemp);
                    break;
            }

            switch (this.iterCurr.CurrentBranchingContext) {
                case BranchingContext.OnFalse:
                    // Anchor true label
                    this.helper.MarkLabel(lblTemp);
                    goto case BranchingContext.OnTrue;

                case BranchingContext.OnTrue:
                    this.iterCurr.Storage = StorageDescriptor.None();
                    break;

                case BranchingContext.None:
                    // Convert branch targets into push of true/false
                    this.helper.ConvBranchToBool(lblTemp, true);
                    this.iterCurr.Storage = StorageDescriptor.Stack(typeof(bool), false);
                    break;
            }

            return ndOr;
        }

        /// <summary>
        /// Generate code for QilNodeType.Not.
        /// </summary>
        /// <remarks>
        /// BranchingContext.OnFalse context: not(expr1)
        /// ==> if (expr1) goto LabelParent;
        ///
        /// BranchingContext.OnTrue context: not(expr1)
        /// ==> if (!expr1) goto LabelParent;
        ///
        /// BranchingContext.None context: not(expr1)
        /// ==> if (expr1) goto LabelTemp;
        ///     push false();
        ///     goto LabelSkip;
        ///     LabelTemp:
        ///     push true();
        ///     LabelSkip:
        ///
        /// </remarks>
        protected override QilNode VisitNot(QilUnary ndNot) {
            Label lblTemp = new Label();

            // Visit operand
            // Reverse branch types
            switch (this.iterCurr.CurrentBranchingContext) {
                case BranchingContext.OnFalse:
                    NestedVisitWithBranch(ndNot.Child, BranchingContext.OnTrue, this.iterCurr.LabelBranch);
                    break;

                case BranchingContext.OnTrue:
                    NestedVisitWithBranch(ndNot.Child, BranchingContext.OnFalse, this.iterCurr.LabelBranch);
                    break;

                default:
                    // Replace boolean argument on top of stack with its inverse
                    Debug.Assert(this.iterCurr.CurrentBranchingContext == BranchingContext.None);
                    lblTemp = this.helper.DefineLabel();
                    NestedVisitWithBranch(ndNot.Child, BranchingContext.OnTrue, lblTemp);
                    break;
            }

            if (this.iterCurr.CurrentBranchingContext == BranchingContext.None) {
                // If condition evaluates to true, then jump to code that pushes false
                this.helper.ConvBranchToBool(lblTemp, false);
                this.iterCurr.Storage = StorageDescriptor.Stack(typeof(bool), false);
            }
            else {
                this.iterCurr.Storage = StorageDescriptor.None();
            }

            return ndNot;
        }

        /// <summary>
        /// Generate code for QilNodeType.Conditional.
        /// </summary>
        protected override QilNode VisitConditional(QilTernary ndCond) {
            XmlILConstructInfo info = XmlILConstructInfo.Read(ndCond);

            if (info.ConstructMethod == XmlILConstructMethod.Writer) {
                Label lblFalse, lblDone;

                // Evaluate if test
                lblFalse = this.helper.DefineLabel();
                NestedVisitWithBranch(ndCond.Left, BranchingContext.OnFalse, lblFalse);

                // Generate true branch code
                NestedVisit(ndCond.Center);

                // Generate false branch code.  If false branch is the empty list,
                if (ndCond.Right.NodeType == QilNodeType.Sequence && ndCond.Right.Count == 0) {
                    // Then generate simplified code that doesn't contain a false branch
                    this.helper.MarkLabel(lblFalse);
                    NestedVisit(ndCond.Right);
                }
                else {
                    // Jump past false branch
                    lblDone = this.helper.DefineLabel();
                    this.helper.EmitUnconditionalBranch(OpCodes.Br, lblDone);

                    // Generate false branch code
                    this.helper.MarkLabel(lblFalse);
                    NestedVisit(ndCond.Right);

                    this.helper.MarkLabel(lblDone);
                }

                this.iterCurr.Storage = StorageDescriptor.None();
            }
            else {
                IteratorDescriptor iterInfoTrue;
                LocalBuilder locBool = null, locCond = null;
                Label lblFalse, lblDone, lblNext;
                Type itemStorageType = GetItemStorageType(ndCond);
                Debug.Assert(info.ConstructMethod == XmlILConstructMethod.Iterator);

                // Evaluate conditional test -- save boolean result in boolResult
                Debug.Assert(ndCond.Left.XmlType.TypeCode == XmlTypeCode.Boolean);
                lblFalse = this.helper.DefineLabel();

                if (ndCond.XmlType.IsSingleton) {
                    // if (!bool-expr) goto LabelFalse;
                    NestedVisitWithBranch(ndCond.Left, BranchingContext.OnFalse, lblFalse);
                }
                else {
                    // CondType itemCond;
                    // int boolResult = bool-expr;
                    locCond = this.helper.DeclareLocal("$$$cond", itemStorageType);
                    locBool = this.helper.DeclareLocal("$$$boolResult", typeof(bool));
                    NestedVisitEnsureLocal(ndCond.Left, locBool);

                    // if (!boolResult) goto LabelFalse;
                    this.helper.Emit(OpCodes.Ldloc, locBool);
                    this.helper.Emit(OpCodes.Brfalse, lblFalse);
                }

                // Generate code for true branch
                ConditionalBranch(ndCond.Center, itemStorageType, locCond);
                iterInfoTrue = this.iterNested;

                // goto LabelDone;
                lblDone = this.helper.DefineLabel();
                this.helper.EmitUnconditionalBranch(OpCodes.Br, lblDone);

                // Generate code for false branch
                // LabelFalse:
                this.helper.MarkLabel(lblFalse);
                ConditionalBranch(ndCond.Right, itemStorageType, locCond);

                // If conditional is not cardinality one, then need to iterate through all values
                if (!ndCond.XmlType.IsSingleton) {
                    Debug.Assert(!ndCond.Center.XmlType.IsSingleton || !ndCond.Right.XmlType.IsSingleton);

                    // IL's rules do not allow OpCodes.Br here
                    // goto LabelDone;
                    this.helper.EmitUnconditionalBranch(OpCodes.Brtrue, lblDone);

                    // LabelNext:
                    lblNext = this.helper.DefineLabel();
                    this.helper.MarkLabel(lblNext);

                    // if (boolResult) goto LabelNextTrue else goto LabelNextFalse;
                    this.helper.Emit(OpCodes.Ldloc, locBool);
                    this.helper.Emit(OpCodes.Brtrue, iterInfoTrue.GetLabelNext());
                    this.helper.EmitUnconditionalBranch(OpCodes.Br, this.iterNested.GetLabelNext());

                    this.iterCurr.SetIterator(lblNext, StorageDescriptor.Local(locCond, itemStorageType, false));
                }

                // LabelDone:
                this.helper.MarkLabel(lblDone);
            }

            return ndCond;
        }

        /// <summary>
        /// Generate code for one of the branches of QilNodeType.Conditional.
        /// </summary>
        private void ConditionalBranch(QilNode ndBranch, Type itemStorageType, LocalBuilder locResult) {
            if (locResult == null) {
                Debug.Assert(ndBranch.XmlType.IsSingleton, "Conditional must produce a singleton");

                // If in a branching context, then inherit branch target from parent context
                if (this.iterCurr.IsBranching) {
                    Debug.Assert(itemStorageType == typeof(bool));
                    NestedVisitWithBranch(ndBranch, this.iterCurr.CurrentBranchingContext, this.iterCurr.LabelBranch);
                }
                else {
                    NestedVisitEnsureStack(ndBranch, itemStorageType, false);
                }
            }
            else {
                // Link nested iterator to parent conditional's iterator
                NestedVisit(ndBranch, this.iterCurr.GetLabelNext());
                this.iterCurr.EnsureItemStorageType(ndBranch.XmlType, itemStorageType);
                this.iterCurr.EnsureLocalNoCache(locResult);
            }
        }

        /// <summary>
        /// Generate code for QilNodeType.Choice.
        /// </summary>
        protected override QilNode VisitChoice(QilChoice ndChoice) {
            QilNode ndBranches;
            Label[] switchLabels;
            Label lblOtherwise, lblDone;
            int regBranches, idx;
            Debug.Assert(XmlILConstructInfo.Read(ndChoice).PushToWriterFirst);

            // Evaluate the expression
            NestedVisit(ndChoice.Expression);

            // Generate switching code
            ndBranches = ndChoice.Branches;
            regBranches = ndBranches.Count - 1;
            switchLabels = new Label[regBranches];
            for (idx = 0; idx < regBranches; idx++)
                switchLabels[idx] = this.helper.DefineLabel();

            lblOtherwise = this.helper.DefineLabel();
            lblDone = this.helper.DefineLabel();

            // switch (value)
            //   case 0: goto Label[0];
            //   ...
            //   case N-1: goto Label[N-1];
            //   default: goto LabelOtherwise;
            this.helper.Emit(OpCodes.Switch, switchLabels);
            this.helper.EmitUnconditionalBranch(OpCodes.Br, lblOtherwise);

            for (idx = 0; idx < regBranches; idx++) {
                // Label[i]:
                this.helper.MarkLabel(switchLabels[idx]);

                // Generate regular branch code
                NestedVisit(ndBranches[idx]);

                // goto LabelDone
                this.helper.EmitUnconditionalBranch(OpCodes.Br, lblDone);
            }

            // LabelOtherwise:
            this.helper.MarkLabel(lblOtherwise);

            // Generate otherwise branch code
            NestedVisit(ndBranches[idx]);

            // LabelDone:
            this.helper.MarkLabel(lblDone);

            this.iterCurr.Storage = StorageDescriptor.None();

            return ndChoice;
        }

        /// <summary>
        /// Generate code for QilNodeType.Length.
        /// </summary>
        /// <remarks>
        /// int length = 0;
        /// foreach (item in expr)
        ///   length++;
        /// </remarks>
        protected override QilNode VisitLength(QilUnary ndSetLen) {
            Label lblOnEnd = this.helper.DefineLabel();
            OptimizerPatterns patt = OptimizerPatterns.Read(ndSetLen);

            if (CachesResult(ndSetLen.Child)) {
                NestedVisitEnsureStack(ndSetLen.Child);
                this.helper.CallCacheCount(this.iterNested.Storage.ItemStorageType);
            }
            else {
                // length = 0;
                this.helper.Emit(OpCodes.Ldc_I4_0);

                StartNestedIterator(ndSetLen.Child, lblOnEnd);

                // foreach (item in expr) {
                Visit(ndSetLen.Child);

                // Pop values of SetLength expression from the stack if necessary
                this.iterCurr.EnsureNoCache();
                this.iterCurr.DiscardStack();

                // length++;
                this.helper.Emit(OpCodes.Ldc_I4_1);
                this.helper.Emit(OpCodes.Add);

                if (patt.MatchesPattern(OptimizerPatternName.MaxPosition)) {
                    // Short-circuit rest of loop if max position has been exceeded
                    this.helper.Emit(OpCodes.Dup);
                    this.helper.LoadInteger((int) patt.GetArgument(OptimizerPatternArgument.MaxPosition));
                    this.helper.Emit(OpCodes.Bgt, lblOnEnd);
                }

                // }
                this.iterCurr.LoopToEnd(lblOnEnd);

                EndNestedIterator(ndSetLen.Child);
            }

            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(int), false);

            return ndSetLen;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.Sequence.
        /// </summary>
        protected override QilNode VisitSequence(QilList ndSeq) {
            if (XmlILConstructInfo.Read(ndSeq).ConstructMethod == XmlILConstructMethod.Writer) {
                // Push each item in the list to output
                foreach (QilNode nd in ndSeq)
                    NestedVisit(nd);
            }
            else {
                // Empty sequence is special case
                if (ndSeq.Count == 0)
                    VisitEmpty(ndSeq);
                else
                    Sequence(ndSeq);
            }

            return ndSeq;
        }

        /// <summary>
        /// Generate code for the empty sequence.
        /// </summary>
        private void VisitEmpty(QilNode nd) {
            Debug.Assert(XmlILConstructInfo.Read(nd).PullFromIteratorFirst, "VisitEmpty should only be called if items are iterated");

            // IL's rules prevent OpCodes.Br here
            // Empty sequence
            this.helper.EmitUnconditionalBranch(OpCodes.Brtrue, this.iterCurr.GetLabelNext());

            // Push dummy value so that Location is not None and IL rules are met
            this.helper.Emit(OpCodes.Ldnull);
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XPathItem), false);
        }

        /// <summary>
        /// Generate code for QilNodeType.Sequence, when sort-merging to retain document order is not necessary.
        /// </summary>
        private void Sequence(QilList ndSeq) {
            LocalBuilder locIdx, locList;
            Label lblStart, lblNext, lblOnEnd = new Label();
            Label[] arrSwitchLabels;
            int i;
            Type itemStorageType = GetItemStorageType(ndSeq);
            Debug.Assert(XmlILConstructInfo.Read(ndSeq).ConstructMethod == XmlILConstructMethod.Iterator, "This method should only be called if items in list are pulled from a code iterator.");

            // Singleton list is a special case if in addition to the singleton there are warnings or errors which should be executed
            if (ndSeq.XmlType.IsSingleton) {
                foreach (QilNode nd in ndSeq) {
                    // Generate nested iterator's code
                    if (nd.XmlType.IsSingleton) {
                        NestedVisitEnsureStack(nd);
                    }
                    else {
                        lblOnEnd = this.helper.DefineLabel();
                        NestedVisit(nd, lblOnEnd);
                        this.iterCurr.DiscardStack();
                        this.helper.MarkLabel(lblOnEnd);
                    }
                }
                this.iterCurr.Storage = StorageDescriptor.Stack(itemStorageType, false);
            }
            else {
                // Type itemList;
                // int idxList;
                locList = this.helper.DeclareLocal("$$$itemList", itemStorageType);
                locIdx = this.helper.DeclareLocal("$$$idxList", typeof(int));

                arrSwitchLabels = new Label[ndSeq.Count];
                lblStart = this.helper.DefineLabel();

                for (i = 0; i < ndSeq.Count; i++) {
                    // LabelOnEnd[i - 1]:
                    // When previous nested iterator is exhausted, it should jump to this (the next) iterator
                    if (i != 0)
                        this.helper.MarkLabel(lblOnEnd);

                    // Create new LabelOnEnd for all but the last iterator, which jumps back to parent iterator when exhausted
                    if (i == ndSeq.Count - 1)
                        lblOnEnd = this.iterCurr.GetLabelNext();
                    else
                        lblOnEnd = this.helper.DefineLabel();

                    // idxList = [i];
                    this.helper.LoadInteger(i);
                    this.helper.Emit(OpCodes.Stloc, locIdx);

                    // Generate nested iterator's code
                    NestedVisit(ndSeq[i], lblOnEnd);

                    // Result of list should be saved to a common type and location
                    this.iterCurr.EnsureItemStorageType(ndSeq[i].XmlType, itemStorageType);
                    this.iterCurr.EnsureLocalNoCache(locList);

                    // Switch statement will jump to nested iterator's LabelNext
                    arrSwitchLabels[i] = this.iterNested.GetLabelNext();

                    // IL's rules prevent OpCodes.Br here
                    // goto LabelStart;
                    this.helper.EmitUnconditionalBranch(OpCodes.Brtrue, lblStart);
                }

                // LabelNext:
                lblNext = this.helper.DefineLabel();
                this.helper.MarkLabel(lblNext);

                // switch (idxList)
                //   case 0: goto LabelNext1;
                //   ...
                //   case N-1: goto LabelNext[N];
                this.helper.Emit(OpCodes.Ldloc, locIdx);
                this.helper.Emit(OpCodes.Switch, arrSwitchLabels);

                // LabelStart:
                this.helper.MarkLabel(lblStart);

                this.iterCurr.SetIterator(lblNext, StorageDescriptor.Local(locList, itemStorageType, false));
            }
        }

        /// <summary>
        /// Generate code for QilNodeType.Union.
        /// </summary>
        protected override QilNode VisitUnion(QilBinary ndUnion) {
            return CreateSetIterator(ndUnion, "$$$iterUnion", typeof(UnionIterator), XmlILMethods.UnionCreate, XmlILMethods.UnionNext);
        }

        /// <summary>
        /// Generate code for QilNodeType.Intersection.
        /// </summary>
        protected override QilNode VisitIntersection(QilBinary ndInter) {
            return CreateSetIterator(ndInter, "$$$iterInter", typeof(IntersectIterator), XmlILMethods.InterCreate, XmlILMethods.InterNext);
        }

        /// <summary>
        /// Generate code for QilNodeType.Difference.
        /// </summary>
        protected override QilNode VisitDifference(QilBinary ndDiff) {
            return CreateSetIterator(ndDiff, "$$$iterDiff", typeof(DifferenceIterator), XmlILMethods.DiffCreate, XmlILMethods.DiffNext);
        }

        /// <summary>
        /// Generate code to combine nodes from two nested iterators using Union, Intersection, or Difference semantics.
        /// </summary>
        private QilNode CreateSetIterator(QilBinary ndSet, string iterName, Type iterType, MethodInfo methCreate, MethodInfo methNext) {
            LocalBuilder locIter, locNav;
            Label lblNext, lblCall, lblNextLeft, lblNextRight, lblInitRight;

            // SetIterator iterSet;
            // XPathNavigator navSet;
            locIter = this.helper.DeclareLocal(iterName, iterType);
            locNav = this.helper.DeclareLocal("$$$navSet", typeof(XPathNavigator));

            // iterSet.Create(runtime);
            this.helper.Emit(OpCodes.Ldloca, locIter);
            this.helper.LoadQueryRuntime();
            this.helper.Call(methCreate);

            // Define labels that will be used
            lblNext = this.helper.DefineLabel();
            lblCall = this.helper.DefineLabel();
            lblInitRight = this.helper.DefineLabel();

            // Generate left nested iterator.  When it is empty, it will branch to lblNext.
            // goto LabelCall;
            NestedVisit(ndSet.Left, lblNext);
            lblNextLeft = this.iterNested.GetLabelNext();
            this.iterCurr.EnsureLocal(locNav);
            this.helper.EmitUnconditionalBranch(OpCodes.Brtrue, lblCall);

            // Generate right nested iterator.  When it is empty, it will branch to lblNext.
            // LabelInitRight:
            // goto LabelCall;
            this.helper.MarkLabel(lblInitRight);
            NestedVisit(ndSet.Right, lblNext);
            lblNextRight = this.iterNested.GetLabelNext();
            this.iterCurr.EnsureLocal(locNav);
            this.helper.EmitUnconditionalBranch(OpCodes.Brtrue, lblCall);

            // LabelNext:
            this.helper.MarkLabel(lblNext);
            this.helper.Emit(OpCodes.Ldnull);
            this.helper.Emit(OpCodes.Stloc, locNav);

            // LabelCall:
            // switch (iterSet.MoveNext(nestedNested)) {
            //      case SetIteratorResult.NoMoreNodes: goto LabelNextCtxt;
            //      case SetIteratorResult.InitRightIterator: goto LabelInitRight;
            //      case SetIteratorResult.NeedLeftNode: goto LabelNextLeft;
            //      case SetIteratorResult.NeedRightNode: goto LabelNextRight;
            // }
            this.helper.MarkLabel(lblCall);
            this.helper.Emit(OpCodes.Ldloca, locIter);
            this.helper.Emit(OpCodes.Ldloc, locNav);
            this.helper.Call(methNext);

            // If this iterator always returns a single node, then NoMoreNodes will never be returned
            // Don't expose Next label if this iterator always returns a single node
            if (ndSet.XmlType.IsSingleton) {
                this.helper.Emit(OpCodes.Switch, new Label[] {lblInitRight, lblNextLeft, lblNextRight});
                this.iterCurr.Storage = StorageDescriptor.Current(locIter, typeof(XPathNavigator));
            }
            else {
                this.helper.Emit(OpCodes.Switch, new Label[] {this.iterCurr.GetLabelNext(), lblInitRight, lblNextLeft, lblNextRight});
                this.iterCurr.SetIterator(lblNext, StorageDescriptor.Current(locIter, typeof(XPathNavigator)));
            }

            return ndSet;
        }

        /// <summary>
        /// Generate code for QilNodeType.Average.
        /// </summary>
        protected override QilNode VisitAverage(QilUnary ndAvg) {
            XmlILStorageMethods meths = XmlILMethods.StorageMethods[GetItemStorageType(ndAvg)];
            return CreateAggregator(ndAvg, "$$$aggAvg", meths, meths.AggAvg, meths.AggAvgResult);
        }

        /// <summary>
        /// Generate code for QilNodeType.Sum.
        /// </summary>
        protected override QilNode VisitSum(QilUnary ndSum) {
            XmlILStorageMethods meths = XmlILMethods.StorageMethods[GetItemStorageType(ndSum)];
            return CreateAggregator(ndSum, "$$$aggSum", meths, meths.AggSum, meths.AggSumResult);
        }

        /// <summary>
        /// Generate code for QilNodeType.Minimum.
        /// </summary>
        protected override QilNode VisitMinimum(QilUnary ndMin) {
            XmlILStorageMethods meths = XmlILMethods.StorageMethods[GetItemStorageType(ndMin)];
            return CreateAggregator(ndMin, "$$$aggMin", meths, meths.AggMin, meths.AggMinResult);
        }

        /// <summary>
        /// Generate code for QilNodeType.Maximum.
        /// </summary>
        protected override QilNode VisitMaximum(QilUnary ndMax) {
            XmlILStorageMethods meths = XmlILMethods.StorageMethods[GetItemStorageType(ndMax)];
            return CreateAggregator(ndMax, "$$$aggMax", meths, meths.AggMax, meths.AggMaxResult);
        }

        /// <summary>
        /// Generate code for QilNodeType.Sum, QilNodeType.Average, QilNodeType.Minimum, and QilNodeType.Maximum.
        /// </summary>
        private QilNode CreateAggregator(QilUnary ndAgg, string aggName, XmlILStorageMethods methods, MethodInfo methAgg, MethodInfo methResult) {
            Label lblOnEnd = this.helper.DefineLabel();
            Type typAgg = methAgg.DeclaringType;
            LocalBuilder locAgg;

            // Aggregate agg;
            // agg.Create();
            locAgg = this.helper.DeclareLocal(aggName, typAgg);
            this.helper.Emit(OpCodes.Ldloca, locAgg);
            this.helper.Call(methods.AggCreate);

            // foreach (num in expr) {
            StartNestedIterator(ndAgg.Child, lblOnEnd);
            this.helper.Emit(OpCodes.Ldloca, locAgg);
            Visit(ndAgg.Child);

            //   agg.Aggregate(num);
            this.iterCurr.EnsureStackNoCache();
            this.iterCurr.EnsureItemStorageType(ndAgg.XmlType, GetItemStorageType(ndAgg));
            this.helper.Call(methAgg);
            this.helper.Emit(OpCodes.Ldloca, locAgg);

            // }
            this.iterCurr.LoopToEnd(lblOnEnd);

            // End nested iterator
            EndNestedIterator(ndAgg.Child);

            // If aggregate might be empty sequence, then generate code to handle this possibility
            if (ndAgg.XmlType.MaybeEmpty) {
                // if (agg.IsEmpty) goto LabelNextCtxt;
                this.helper.Call(methods.AggIsEmpty);
                this.helper.Emit(OpCodes.Brtrue, this.iterCurr.GetLabelNext());
                this.helper.Emit(OpCodes.Ldloca, locAgg);
            }

            // result = agg.Result;
            this.helper.Call(methResult);
            this.iterCurr.Storage = StorageDescriptor.Stack(GetItemStorageType(ndAgg), false);

            return ndAgg;
        }

        /// <summary>
        /// Generate code for QilNodeType.Negate.
        /// </summary>
        protected override QilNode VisitNegate(QilUnary ndNeg) {
            NestedVisitEnsureStack(ndNeg.Child);
            this.helper.CallArithmeticOp(QilNodeType.Negate, ndNeg.XmlType.TypeCode);
            this.iterCurr.Storage = StorageDescriptor.Stack(GetItemStorageType(ndNeg), false);
            return ndNeg;
        }

        /// <summary>
        /// Generate code for QilNodeType.Add.
        /// </summary>
        protected override QilNode VisitAdd(QilBinary ndPlus) {
            return ArithmeticOp(ndPlus);
        }

        /// <summary>
        /// Generate code for QilNodeType.Subtract.
        /// </summary>
        protected override QilNode VisitSubtract(QilBinary ndMinus) {
            return ArithmeticOp(ndMinus);
        }

        /// <summary>
        /// Generate code for QilNodeType.Multiply.
        /// </summary>
        protected override QilNode VisitMultiply(QilBinary ndMul) {
            return ArithmeticOp(ndMul);
        }

        /// <summary>
        /// Generate code for QilNodeType.Divide.
        /// </summary>
        protected override QilNode VisitDivide(QilBinary ndDiv) {
            return ArithmeticOp(ndDiv);
        }

        /// <summary>
        /// Generate code for QilNodeType.Modulo.
        /// </summary>
        protected override QilNode VisitModulo(QilBinary ndMod) {
            return ArithmeticOp(ndMod);
        }

        /// <summary>
        /// Generate code for two-argument arithmetic operations.
        /// </summary>
        private QilNode ArithmeticOp(QilBinary ndOp) {
            NestedVisitEnsureStack(ndOp.Left, ndOp.Right);
            this.helper.CallArithmeticOp(ndOp.NodeType, ndOp.XmlType.TypeCode);
            this.iterCurr.Storage = StorageDescriptor.Stack(GetItemStorageType(ndOp), false);
            return ndOp;
        }

        /// <summary>
        /// Generate code for QilNodeType.StrLength.
        /// </summary>
        protected override QilNode VisitStrLength(QilUnary ndLen) {
            NestedVisitEnsureStack(ndLen.Child);
            this.helper.Call(XmlILMethods.StrLen);
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(int), false);
            return ndLen;
        }

        /// <summary>
        /// Generate code for QilNodeType.StrConcat.
        /// </summary>
        protected override QilNode VisitStrConcat(QilStrConcat ndStrConcat) {
            LocalBuilder locStringConcat;
            bool fasterConcat;
            QilNode delimiter;
            QilNode listStrings;
            Debug.Assert(!ndStrConcat.Values.XmlType.IsSingleton, "Optimizer should have folded StrConcat of a singleton value");

            // Get delimiter (assuming it's not the empty string)
            delimiter = ndStrConcat.Delimiter;
            if (delimiter.NodeType == QilNodeType.LiteralString && ((string) (QilLiteral) delimiter).Length == 0) {
                delimiter = null;
            }

            listStrings = ndStrConcat.Values;
            if (listStrings.NodeType == QilNodeType.Sequence && listStrings.Count < 5) {
                // Faster concat possible only if cardinality can be guaranteed at compile-time and there's no delimiter
                fasterConcat = true;
                foreach (QilNode ndStr in listStrings) {
                    if (!ndStr.XmlType.IsSingleton)
                        fasterConcat = false;
                }
            }
            else {
                // If more than 4 strings, array will need to be built
                fasterConcat = false;
            }

            if (fasterConcat) {
                foreach (QilNode ndStr in listStrings)
                    NestedVisitEnsureStack(ndStr);

                this.helper.CallConcatStrings(listStrings.Count);
            }
            else {
                // Create StringConcat helper internal class
                locStringConcat = this.helper.DeclareLocal("$$$strcat", typeof(StringConcat));
                this.helper.Emit(OpCodes.Ldloca, locStringConcat);
                this.helper.Call(XmlILMethods.StrCatClear);

                // Set delimiter, if it's not empty string
                if (delimiter != null) {
                    this.helper.Emit(OpCodes.Ldloca, locStringConcat);
                    NestedVisitEnsureStack(delimiter);
                    this.helper.Call(XmlILMethods.StrCatDelim);
                }

                this.helper.Emit(OpCodes.Ldloca, locStringConcat);

                if (listStrings.NodeType == QilNodeType.Sequence) {
                    foreach (QilNode ndStr in listStrings)
                        GenerateConcat(ndStr, locStringConcat);
                }
                else {
                    GenerateConcat(listStrings, locStringConcat);
                }

                // Push resulting string onto stack
                this.helper.Call(XmlILMethods.StrCatResult);
            }

            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(string), false);

            return ndStrConcat;
        }

        /// <summary>
        /// Generate code to concatenate string values returned by expression "ndStr" using the StringConcat helper class.
        /// </summary>
        private void GenerateConcat(QilNode ndStr, LocalBuilder locStringConcat) {
            Label lblOnEnd;

            // str = each string;
            lblOnEnd = this.helper.DefineLabel();
            StartNestedIterator(ndStr, lblOnEnd);
            Visit(ndStr);

            // strcat.Concat(str);
            this.iterCurr.EnsureStackNoCache();
            this.iterCurr.EnsureItemStorageType(ndStr.XmlType, typeof(string));
            this.helper.Call(XmlILMethods.StrCatCat);
            this.helper.Emit(OpCodes.Ldloca, locStringConcat);

            // Get next string
            // goto LabelNext;
            // LabelOnEnd:
            this.iterCurr.LoopToEnd(lblOnEnd);

            // End nested iterator
            EndNestedIterator(ndStr);
        }

        /// <summary>
        /// Generate code for QilNodeType.StrParseQName.
        /// </summary>
        protected override QilNode VisitStrParseQName(QilBinary ndParsedTagName) {
            VisitStrParseQName(ndParsedTagName, false);
            return ndParsedTagName;
        }

        /// <summary>
        /// Generate code for QilNodeType.StrParseQName.
        /// </summary>
        private void VisitStrParseQName(QilBinary ndParsedTagName, bool preservePrefix) {
            // If QName prefix should be preserved, then don't create an XmlQualifiedName, which discards the prefix
            if (!preservePrefix)
                this.helper.LoadQueryRuntime();

            // Push (possibly computed) tag name onto the stack
            NestedVisitEnsureStack(ndParsedTagName.Left);

            // If type of second parameter is string,
            if (ndParsedTagName.Right.XmlType.TypeCode == XmlTypeCode.String) {
                // Then push (possibly computed) namespace onto the stack
                Debug.Assert(ndParsedTagName.Right.XmlType.IsSingleton);
                NestedVisitEnsureStack(ndParsedTagName.Right);

                if (!preservePrefix)
                    this.helper.CallParseTagName(GenerateNameType.TagNameAndNamespace);
            }
            else {
                // Else push index of set of prefix mappings to use in resolving the prefix
                if (ndParsedTagName.Right.NodeType == QilNodeType.Sequence)
                    this.helper.LoadInteger(this.helper.StaticData.DeclarePrefixMappings(ndParsedTagName.Right));
                else
                    this.helper.LoadInteger(this.helper.StaticData.DeclarePrefixMappings(new QilNode[] {ndParsedTagName.Right}));

                // If QName prefix should be preserved, then don't create an XmlQualifiedName, which discards the prefix
                if (!preservePrefix)
                    this.helper.CallParseTagName(GenerateNameType.TagNameAndMappings);
            }

            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XmlQualifiedName), false);
        }

        /// <summary>
        /// Generate code for QilNodeType.Ne.
        /// </summary>
        protected override QilNode VisitNe(QilBinary ndNe) {
            Compare(ndNe);
            return ndNe;
        }

        /// <summary>
        /// Generate code for QilNodeType.Eq.
        /// </summary>
        protected override QilNode VisitEq(QilBinary ndEq) {
            Compare(ndEq);
            return ndEq;
        }

        /// <summary>
        /// Generate code for QilNodeType.Gt.
        /// </summary>
        protected override QilNode VisitGt(QilBinary ndGt) {
            Compare(ndGt);
            return ndGt;
        }

        /// <summary>
        /// Generate code for QilNodeType.Ne.
        /// </summary>
        protected override QilNode VisitGe(QilBinary ndGe) {
            Compare(ndGe);
            return ndGe;
        }

        /// <summary>
        /// Generate code for QilNodeType.Lt.
        /// </summary>
        protected override QilNode VisitLt(QilBinary ndLt) {
            Compare(ndLt);
            return ndLt;
        }

        /// <summary>
        /// Generate code for QilNodeType.Le.
        /// </summary>
        protected override QilNode VisitLe(QilBinary ndLe) {
            Compare(ndLe);
            return ndLe;
        }

        /// <summary>
        /// Generate code for comparison operations.
        /// </summary>
        private void Compare(QilBinary ndComp) {
            QilNodeType relOp = ndComp.NodeType;
            XmlTypeCode code;
            Debug.Assert(ndComp.Left.XmlType.IsAtomicValue && ndComp.Right.XmlType.IsAtomicValue, "Operands to compare must be atomic values.");
            Debug.Assert(ndComp.Left.XmlType.IsSingleton && ndComp.Right.XmlType.IsSingleton, "Operands to compare must be cardinality one.");
            Debug.Assert(ndComp.Left.XmlType == ndComp.Right.XmlType, "Operands to compare may not be heterogenous.");

            if (relOp == QilNodeType.Eq || relOp == QilNodeType.Ne) {
                // Generate better code for certain special cases
                if (TryZeroCompare(relOp, ndComp.Left, ndComp.Right))
                    return;

                if (TryZeroCompare(relOp, ndComp.Right, ndComp.Left))
                    return;

                if (TryNameCompare(relOp, ndComp.Left, ndComp.Right))
                    return;

                if (TryNameCompare(relOp, ndComp.Right, ndComp.Left))
                    return;
            }

            // Push two operands onto the stack
            NestedVisitEnsureStack(ndComp.Left, ndComp.Right);

            // Perform comparison
            code = ndComp.Left.XmlType.TypeCode;
            switch (code) {
                case XmlTypeCode.String:
                case XmlTypeCode.Decimal:
                case XmlTypeCode.QName:
                    if (relOp == QilNodeType.Eq || relOp == QilNodeType.Ne) {
                        this.helper.CallCompareEquals(code);

                        // If relOp is Eq, then branch to true label or push "true" if Equals function returns true (non-zero)
                        // If relOp is Ne, then branch to true label or push "true" if Equals function returns false (zero)
                        ZeroCompare((relOp == QilNodeType.Eq) ? QilNodeType.Ne : QilNodeType.Eq, true);
                    }
                    else {
                        Debug.Assert(code != XmlTypeCode.QName, "QName values do not support the " + relOp + " operation");

                        // Push -1, 0, or 1 onto the stack depending upon the result of the comparison
                        this.helper.CallCompare(code);

                        // Compare result to 0 (e.g. Ge is >= 0)
                        this.helper.Emit(OpCodes.Ldc_I4_0);
                        ClrCompare(relOp, code);
                    }
                    break;

                case XmlTypeCode.Integer:
                case XmlTypeCode.Int:
                case XmlTypeCode.Boolean:
                case XmlTypeCode.Double:
                    ClrCompare(relOp, code);
                    break;

                default:
                    Debug.Fail("Comparisons for datatype " + code + " are invalid.");
                    break;
            }
        }

        /// <summary>
        /// Generate code for QilNodeType.VisitIs.
        /// </summary>
        protected override QilNode VisitIs(QilBinary ndIs) {
            // Generate code to push arguments onto stack
            NestedVisitEnsureStack(ndIs.Left, ndIs.Right);
            this.helper.Call(XmlILMethods.NavSamePos);

            // navThis.IsSamePosition(navThat);
            ZeroCompare(QilNodeType.Ne, true);
            return ndIs;
        }

        /// <summary>
        /// Generate code for QilNodeType.VisitBefore.
        /// </summary>
        protected override QilNode VisitBefore(QilBinary ndBefore) {
            ComparePosition(ndBefore);
            return ndBefore;
        }

        /// <summary>
        /// Generate code for QilNodeType.VisitAfter.
        /// </summary>
        protected override QilNode VisitAfter(QilBinary ndAfter) {
            ComparePosition(ndAfter);
            return ndAfter;
        }

        /// <summary>
        /// Generate code for QilNodeType.VisitBefore and QilNodeType.VisitAfter.
        /// </summary>
        private void ComparePosition(QilBinary ndComp) {
            // Generate code to push arguments onto stack
            this.helper.LoadQueryRuntime();
            NestedVisitEnsureStack(ndComp.Left, ndComp.Right);
            this.helper.Call(XmlILMethods.CompPos);

            // XmlQueryRuntime.ComparePosition(navThis, navThat) < 0;
            this.helper.LoadInteger(0);
            ClrCompare(ndComp.NodeType == QilNodeType.Before ? QilNodeType.Lt : QilNodeType.Gt, XmlTypeCode.String);
        }

        /// <summary>
        /// Generate code for a QilNodeType.For.
        /// </summary>
        protected override QilNode VisitFor(QilIterator ndFor) {
            IteratorDescriptor iterInfo;

            // Reference saved location
            iterInfo = XmlILAnnotation.Write(ndFor).CachedIteratorDescriptor;
            this.iterCurr.Storage = iterInfo.Storage;

            // If the iterator is a reference to a global variable or parameter,
            if (this.iterCurr.Storage.Location == ItemLocation.Global) {
                // Then compute global value and push it onto the stack
                this.iterCurr.EnsureStack();
            }

            return ndFor;
        }

        /// <summary>
        /// Generate code for a QilNodeType.Let.
        /// </summary>
        protected override QilNode VisitLet(QilIterator ndLet) {
            // Same as For
            return VisitFor(ndLet);
        }

        /// <summary>
        /// Generate code for a QilNodeType.Parameter.
        /// </summary>
        protected override QilNode VisitParameter(QilParameter ndParameter) {
            // Same as For
            return VisitFor(ndParameter);
        }

        /// <summary>
        /// Generate code for a QilNodeType.Loop.
        /// </summary>
        protected override QilNode VisitLoop(QilLoop ndLoop) {
            bool hasOnEnd;
            Label lblOnEnd;

            StartWriterLoop(ndLoop, out hasOnEnd, out lblOnEnd);

            StartBinding(ndLoop.Variable);

            // Unnest loop body as part of the current iterator
            Visit(ndLoop.Body);

            EndBinding(ndLoop.Variable);

            EndWriterLoop(ndLoop, hasOnEnd, lblOnEnd);

            return ndLoop;
        }

        /// <summary>
        /// Generate code for a QilNodeType.Filter.
        /// </summary>
        protected override QilNode VisitFilter(QilLoop ndFilter) {
            // Handle any special-case patterns that are rooted at Filter
            if (HandleFilterPatterns(ndFilter))
                return ndFilter;

            StartBinding(ndFilter.Variable);

            // Result of filter is the sequence bound to the iterator
            this.iterCurr.SetIterator(this.iterNested);

            // If filter is false, skip the current item
            StartNestedIterator(ndFilter.Body);
            this.iterCurr.SetBranching(BranchingContext.OnFalse, this.iterCurr.ParentIterator.GetLabelNext());
            Visit(ndFilter.Body);
            EndNestedIterator(ndFilter.Body);

            EndBinding(ndFilter.Variable);

            return ndFilter;
        }

        /// <summary>
        /// There are a number of path patterns that can be rooted at Filter nodes.  Determine whether one of these patterns
        /// has been previously matched on "ndFilter".  If so, generate code for the pattern and return true.  Otherwise, just
        /// return false.
        /// </summary>
        private bool HandleFilterPatterns(QilLoop ndFilter) {
            OptimizerPatterns patt = OptimizerPatterns.Read(ndFilter);
            LocalBuilder locIter;
            XmlNodeKindFlags kinds;
            QilName name;
            QilNode input, step;
            bool isFilterElements;

            // Handle FilterElements and FilterContentKind patterns
            isFilterElements = patt.MatchesPattern(OptimizerPatternName.FilterElements);
            if (isFilterElements || patt.MatchesPattern(OptimizerPatternName.FilterContentKind)) {
                if (isFilterElements) {
                    // FilterElements pattern, so Kind = Element and Name = Argument
                    kinds = XmlNodeKindFlags.Element;
                    name = (QilName) patt.GetArgument(OptimizerPatternArgument.ElementQName);
                }
                else {
                    // FilterKindTest pattern, so Kind = Argument and Name = null
                    kinds = ((XmlQueryType) patt.GetArgument(OptimizerPatternArgument.KindTestType)).NodeKinds;
                    name = null;
                }

                step = (QilNode) patt.GetArgument(OptimizerPatternArgument.StepNode);
                input = (QilNode) patt.GetArgument(OptimizerPatternArgument.StepInput);
                switch (step.NodeType) {
                    case QilNodeType.Content:
                        if (isFilterElements) {
                            // Iterator iter;
                            locIter = this.helper.DeclareLocal("$$$iterElemContent", typeof(ElementContentIterator));

                            // iter.Create(navCtxt, locName, ns);
                            this.helper.Emit(OpCodes.Ldloca, locIter);
                            NestedVisitEnsureStack(input);
                            this.helper.CallGetAtomizedName(this.helper.StaticData.DeclareName(name.LocalName));
                            this.helper.CallGetAtomizedName(this.helper.StaticData.DeclareName(name.NamespaceUri));
                            this.helper.Call(XmlILMethods.ElemContentCreate);

                            GenerateSimpleIterator(typeof(XPathNavigator), locIter, XmlILMethods.ElemContentNext);
                        }
                        else {
                            if (kinds == XmlNodeKindFlags.Content) {
                                CreateSimpleIterator(input, "$$$iterContent", typeof(ContentIterator), XmlILMethods.ContentCreate, XmlILMethods.ContentNext);
                            }
                            else {
                                // Iterator iter;
                                locIter = this.helper.DeclareLocal("$$$iterContent", typeof(NodeKindContentIterator));

                                // iter.Create(navCtxt, nodeType);
                                this.helper.Emit(OpCodes.Ldloca, locIter);
                                NestedVisitEnsureStack(input);
                                this.helper.LoadInteger((int) QilXmlToXPathNodeType(kinds));
                                this.helper.Call(XmlILMethods.KindContentCreate);

                                GenerateSimpleIterator(typeof(XPathNavigator), locIter, XmlILMethods.KindContentNext);
                            }
                        }
                        return true;

                    case QilNodeType.Parent:
                        CreateFilteredIterator(input, "$$$iterPar", typeof(ParentIterator), XmlILMethods.ParentCreate, XmlILMethods.ParentNext,
                                               kinds, name, TriState.Unknown, null);
                        return true;

                    case QilNodeType.Ancestor:
                    case QilNodeType.AncestorOrSelf:
                        CreateFilteredIterator(input, "$$$iterAnc", typeof(AncestorIterator), XmlILMethods.AncCreate, XmlILMethods.AncNext,
                                               kinds, name, (step.NodeType == QilNodeType.Ancestor) ? TriState.False : TriState.True, null);
                        return true;

                    case QilNodeType.Descendant:
                    case QilNodeType.DescendantOrSelf:
                        CreateFilteredIterator(input, "$$$iterDesc", typeof(DescendantIterator), XmlILMethods.DescCreate, XmlILMethods.DescNext,
                                               kinds, name, (step.NodeType == QilNodeType.Descendant) ? TriState.False : TriState.True, null);
                        return true;

                    case QilNodeType.Preceding:
                        CreateFilteredIterator(input, "$$$iterPrec", typeof(PrecedingIterator), XmlILMethods.PrecCreate, XmlILMethods.PrecNext,
                                               kinds, name, TriState.Unknown, null);
                        return true;

                    case QilNodeType.FollowingSibling:
                        CreateFilteredIterator(input, "$$$iterFollSib", typeof(FollowingSiblingIterator), XmlILMethods.FollSibCreate, XmlILMethods.FollSibNext,
                                               kinds, name, TriState.Unknown, null);
                        return true;

                    case QilNodeType.PrecedingSibling:
                        CreateFilteredIterator(input, "$$$iterPreSib", typeof(PrecedingSiblingIterator), XmlILMethods.PreSibCreate, XmlILMethods.PreSibNext,
                                               kinds, name, TriState.Unknown, null);
                        return true;

                    case QilNodeType.NodeRange:
                        CreateFilteredIterator(input, "$$$iterRange", typeof(NodeRangeIterator), XmlILMethods.NodeRangeCreate, XmlILMethods.NodeRangeNext,
                                               kinds, name, TriState.Unknown, ((QilBinary) step).Right);
                        return true;

                    case QilNodeType.XPathFollowing:
                        CreateFilteredIterator(input, "$$$iterFoll", typeof(XPathFollowingIterator), XmlILMethods.XPFollCreate, XmlILMethods.XPFollNext,
                                               kinds, name, TriState.Unknown, null);
                        return true;

                    case QilNodeType.XPathPreceding:
                        CreateFilteredIterator(input, "$$$iterPrec", typeof(XPathPrecedingIterator), XmlILMethods.XPPrecCreate, XmlILMethods.XPPrecNext,
                                               kinds, name, TriState.Unknown, null);
                        return true;

                    default:
                        Debug.Assert(false, "Pattern " + step.NodeType + " should have been handled.");
                        break;
                }
            }
            else if (patt.MatchesPattern(OptimizerPatternName.FilterAttributeKind)) {
                // Handle FilterAttributeKind pattern
                input = (QilNode) patt.GetArgument(OptimizerPatternArgument.StepInput);
                CreateSimpleIterator(input, "$$$iterAttr", typeof(AttributeIterator), XmlILMethods.AttrCreate, XmlILMethods.AttrNext);
                return true;
            }
            else if (patt.MatchesPattern(OptimizerPatternName.EqualityIndex)) {
                // Handle EqualityIndex pattern
                Label lblOnEnd = this.helper.DefineLabel();
                Label lblLookup = this.helper.DefineLabel();
                QilIterator nodes = (QilIterator) patt.GetArgument(OptimizerPatternArgument.IndexedNodes);
                QilNode keys = (QilNode) patt.GetArgument(OptimizerPatternArgument.KeyExpression);

                // XmlILIndex index;
                // if (runtime.FindIndex(navCtxt, indexId, out index)) goto LabelLookup;
                LocalBuilder locIndex = this.helper.DeclareLocal("$$$index", typeof(XmlILIndex));
                this.helper.LoadQueryRuntime();
                this.helper.Emit(OpCodes.Ldarg_1);
                this.helper.LoadInteger(this.indexId);
                this.helper.Emit(OpCodes.Ldloca, locIndex);
                this.helper.Call(XmlILMethods.FindIndex);
                this.helper.Emit(OpCodes.Brtrue, lblLookup);

                // runtime.AddNewIndex(navCtxt, indexId, [build index]);
                this.helper.LoadQueryRuntime();
                this.helper.Emit(OpCodes.Ldarg_1);
                this.helper.LoadInteger(this.indexId);
                this.helper.Emit(OpCodes.Ldloc, locIndex);

                // Generate code to iterate over the the nodes which are being indexed ($iterNodes in the pattern)
                StartNestedIterator(nodes, lblOnEnd);
                StartBinding(nodes);

                // Generate code to iterate over the keys for each node ($bindingKeys in the pattern)
                Visit(keys);

                // index.Add(key, value);
                this.iterCurr.EnsureStackNoCache();
                VisitFor(nodes);
                this.iterCurr.EnsureStackNoCache();
                this.iterCurr.EnsureItemStorageType(nodes.XmlType, typeof(XPathNavigator));
                this.helper.Call(XmlILMethods.IndexAdd);
                this.helper.Emit(OpCodes.Ldloc, locIndex);

                // LabelOnEnd:
                this.iterCurr.LoopToEnd(lblOnEnd);
                EndBinding(nodes);
                EndNestedIterator(nodes);

                // runtime.AddNewIndex(navCtxt, indexId, [build index]);
                this.helper.Call(XmlILMethods.AddNewIndex);

                // LabelLookup:
                // results = index.Lookup(keyValue);
                this.helper.MarkLabel(lblLookup);
                this.helper.Emit(OpCodes.Ldloc, locIndex);
                this.helper.Emit(OpCodes.Ldarg_2);
                this.helper.Call(XmlILMethods.IndexLookup);
                this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XPathNavigator), true);

                this.indexId++;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Generate code for a Let, For, or Parameter iterator.  Bind iterated value to a variable.
        /// </summary>
        private void StartBinding(QilIterator ndIter) {
            OptimizerPatterns patt = OptimizerPatterns.Read(ndIter);
            Debug.Assert(ndIter != null);

            // DebugInfo: Sequence point just before generating code for the bound expression
            if (this.qil.IsDebug && ndIter.SourceLine != null)
                this.helper.DebugSequencePoint(ndIter.SourceLine);

            // Treat cardinality one Let iterators as if they were For iterators (no nesting necessary)
            if (ndIter.NodeType == QilNodeType.For || ndIter.XmlType.IsSingleton) {
                StartForBinding(ndIter, patt);
            }
            else {
                Debug.Assert(ndIter.NodeType == QilNodeType.Let || ndIter.NodeType == QilNodeType.Parameter);
                Debug.Assert(!patt.MatchesPattern(OptimizerPatternName.IsPositional));

                // Bind Let values (nested iterator) to variable
                StartLetBinding(ndIter);
            }

            // Attach IteratorDescriptor to the iterator
            XmlILAnnotation.Write(ndIter).CachedIteratorDescriptor = this.iterNested;
        }

        /// <summary>
        /// Bind values produced by the "ndFor" expression to a non-stack location that can later
        /// be referenced.
        /// </summary>
        private void StartForBinding(QilIterator ndFor, OptimizerPatterns patt) {
            LocalBuilder locPos = null;
            Debug.Assert(ndFor.XmlType.IsSingleton);

            // For expression iterator will be unnested as part of parent iterator
            if (this.iterCurr.HasLabelNext)
                StartNestedIterator(ndFor.Binding, this.iterCurr.GetLabelNext());
            else
                StartNestedIterator(ndFor.Binding);

            if (patt.MatchesPattern(OptimizerPatternName.IsPositional)) {
                // Need to track loop index so initialize it to 0 before starting loop
                locPos = this.helper.DeclareLocal("$$$pos", typeof(int));
                this.helper.Emit(OpCodes.Ldc_I4_0);
                this.helper.Emit(OpCodes.Stloc, locPos);
            }

            // Allow base internal class to dispatch based on QilExpression node type
            Visit(ndFor.Binding);

            // DebugInfo: Open variable scope
            // DebugInfo: Ensure that for variable is stored in a local and tag it with the user-defined name
            if (this.qil.IsDebug && ndFor.DebugName != null) {
                this.helper.DebugStartScope();

                // Ensure that values are stored in a local variable with a user-defined name
                this.iterCurr.EnsureLocalNoCache("$$$for");
                this.iterCurr.Storage.LocalLocation.SetLocalSymInfo(ndFor.DebugName);
            }
            else {
                // Ensure that values are not stored on the stack
                this.iterCurr.EnsureNoStackNoCache("$$$for");
            }

            if (patt.MatchesPattern(OptimizerPatternName.IsPositional)) {
                // Increment position
                this.helper.Emit(OpCodes.Ldloc, locPos);
                this.helper.Emit(OpCodes.Ldc_I4_1);
                this.helper.Emit(OpCodes.Add);
                this.helper.Emit(OpCodes.Stloc, locPos);

                if (patt.MatchesPattern(OptimizerPatternName.MaxPosition)) {
                    // Short-circuit rest of loop if max position has already been reached
                    this.helper.Emit(OpCodes.Ldloc, locPos);
                    this.helper.LoadInteger((int) patt.GetArgument(OptimizerPatternArgument.MaxPosition));
                    this.helper.Emit(OpCodes.Bgt, this.iterCurr.ParentIterator.GetLabelNext());
                }

                this.iterCurr.LocalPosition = locPos;
            }

            EndNestedIterator(ndFor.Binding);
            this.iterCurr.SetIterator(this.iterNested);
        }

        /// <summary>
        /// Bind values in the "ndLet" expression to a non-stack location that can later be referenced.
        /// </summary>
        public void StartLetBinding(QilIterator ndLet) {
            Debug.Assert(!ndLet.XmlType.IsSingleton);

            // Construct nested iterator
            StartNestedIterator(ndLet);

            // Allow base internal class to dispatch based on QilExpression node type
            NestedVisit(ndLet.Binding, GetItemStorageType(ndLet), !ndLet.XmlType.IsSingleton);

            // DebugInfo: Open variable scope
            // DebugInfo: Ensure that for variable is stored in a local and tag it with the user-defined name
            if (this.qil.IsDebug && ndLet.DebugName != null) {
                this.helper.DebugStartScope();

                // Ensure that cache is stored in a local variable with a user-defined name
                this.iterCurr.EnsureLocal("$$$cache");
                this.iterCurr.Storage.LocalLocation.SetLocalSymInfo(ndLet.DebugName);
            }
            else {
                // Ensure that cache is not stored on the stack
                this.iterCurr.EnsureNoStack("$$$cache");
            }

            EndNestedIterator(ndLet);
        }

        /// <summary>
        /// Mark iterator variables as out-of-scope.
        /// </summary>
        private void EndBinding(QilIterator ndIter) {
            Debug.Assert(ndIter != null);

            // Variables go out of scope here
            if (this.qil.IsDebug && ndIter.DebugName != null)
                this.helper.DebugEndScope();
        }

        /// <summary>
        /// Generate code for QilNodeType.PositionOf.
        /// </summary>
        protected override QilNode VisitPositionOf(QilUnary ndPos) {
            QilIterator ndIter = ndPos.Child as QilIterator;
            LocalBuilder locPos;
            Debug.Assert(ndIter.NodeType == QilNodeType.For);

            locPos = XmlILAnnotation.Write(ndIter).CachedIteratorDescriptor.LocalPosition;
            Debug.Assert(locPos != null);
            this.iterCurr.Storage = StorageDescriptor.Local(locPos, typeof(int), false);

            return ndPos;
        }

        /// <summary>
        /// Generate code for QilNodeType.Sort.
        /// </summary>
        protected override QilNode VisitSort(QilLoop ndSort) {
            Type itemStorageType = GetItemStorageType(ndSort);
            LocalBuilder locCache, locKeys;
            Label lblOnEndSort = this.helper.DefineLabel();
            Debug.Assert(ndSort.Variable.NodeType == QilNodeType.For);

            // XmlQuerySequence<T> cache;
            // cache = XmlQuerySequence.CreateOrReuse(cache);
            XmlILStorageMethods methods = XmlILMethods.StorageMethods[itemStorageType];
            locCache = this.helper.DeclareLocal("$$$cache", methods.SeqType);
            this.helper.Emit(OpCodes.Ldloc, locCache);
            this.helper.CallToken(methods.SeqReuse);
            this.helper.Emit(OpCodes.Stloc, locCache);
            this.helper.Emit(OpCodes.Ldloc, locCache);
  
            // XmlSortKeyAccumulator keys;
            // keys.Create(runtime);
            locKeys = this.helper.DeclareLocal("$$$keys", typeof(XmlSortKeyAccumulator));
            this.helper.Emit(OpCodes.Ldloca, locKeys);
            this.helper.Call(XmlILMethods.SortKeyCreate);

            // Construct nested iterator
            // foreach (item in sort-expr) {
            StartNestedIterator(ndSort.Variable, lblOnEndSort);
            StartBinding(ndSort.Variable);
            Debug.Assert(!this.iterNested.Storage.IsCached);

            // cache.Add(item);
            this.iterCurr.EnsureStackNoCache();
            this.iterCurr.EnsureItemStorageType(ndSort.Variable.XmlType, GetItemStorageType(ndSort.Variable));
            this.helper.Call(methods.SeqAdd);

            this.helper.Emit(OpCodes.Ldloca, locKeys);

            // Add keys to accumulator (there may be several keys)
            foreach (QilSortKey ndKey in ndSort.Body)
                VisitSortKey(ndKey, locKeys);

            // keys.FinishSortKeys();
            this.helper.Call(XmlILMethods.SortKeyFinish);

            // }
            this.helper.Emit(OpCodes.Ldloc, locCache);
            this.iterCurr.LoopToEnd(lblOnEndSort);

            // Remove cache reference from stack
            this.helper.Emit(OpCodes.Pop);

            // cache.SortByKeys(keys.Keys);
            this.helper.Emit(OpCodes.Ldloc, locCache);
            this.helper.Emit(OpCodes.Ldloca, locKeys);
            this.helper.Call(XmlILMethods.SortKeyKeys);
            this.helper.Call(methods.SeqSortByKeys);

            // End nested iterator
            this.iterCurr.Storage = StorageDescriptor.Local(locCache, itemStorageType, true);
            EndBinding(ndSort.Variable);
            EndNestedIterator(ndSort.Variable);
            this.iterCurr.SetIterator(this.iterNested);

            return ndSort;
        }

        /// <summary>
        /// Generate code to add a (value, collation) sort key to the XmlSortKeyAccumulator.
        /// </summary>
        private void VisitSortKey(QilSortKey ndKey, LocalBuilder locKeys) {
            Label lblOnEndKey;
            Debug.Assert(ndKey.Key.XmlType.IsAtomicValue, "Sort key must be an atomic value.");

            // Push collation onto the stack
            this.helper.Emit(OpCodes.Ldloca, locKeys);
            if (ndKey.Collation.NodeType == QilNodeType.LiteralString) {
                // collation = runtime.GetCollation(idx);
                this.helper.CallGetCollation(this.helper.StaticData.DeclareCollation((string) (QilLiteral) ndKey.Collation));
            }
            else {
                // collation = runtime.CreateCollation(str);
                this.helper.LoadQueryRuntime();
                NestedVisitEnsureStack(ndKey.Collation);
                this.helper.Call(XmlILMethods.CreateCollation);
            }

            if (ndKey.XmlType.IsSingleton) {
                NestedVisitEnsureStack(ndKey.Key);

                // keys.AddSortKey(collation, value);
                this.helper.AddSortKey(ndKey.Key.XmlType);
            }
            else {
                lblOnEndKey = this.helper.DefineLabel();
                StartNestedIterator(ndKey.Key, lblOnEndKey);
                Visit(ndKey.Key);
                this.iterCurr.EnsureStackNoCache();
                this.iterCurr.EnsureItemStorageType(ndKey.Key.XmlType, GetItemStorageType(ndKey.Key));

                // Non-empty sort key
                // keys.AddSortKey(collation, value);
                this.helper.AddSortKey(ndKey.Key.XmlType);

                // goto LabelDone;
                // LabelOnEnd:
                Label lblDone = this.helper.DefineLabel();
                this.helper.EmitUnconditionalBranch(OpCodes.Br_S, lblDone);
                this.helper.MarkLabel(lblOnEndKey);

                // Empty sequence key
                // keys.AddSortKey(collation);
                this.helper.AddSortKey(null);

                this.helper.MarkLabel(lblDone);

                EndNestedIterator(ndKey.Key);
            }
        }

        /// <summary>
        /// Generate code for for QilNodeType.DocOrderDistinct.
        /// </summary>
        protected override QilNode VisitDocOrderDistinct(QilUnary ndDod) {
            // DocOrderDistinct applied to a singleton is a no-op
            if (ndDod.XmlType.IsSingleton)
                return Visit(ndDod.Child);

            // Handle any special-case patterns that are rooted at DocOrderDistinct
            if (HandleDodPatterns(ndDod))
                return ndDod;

            // Sort results of child expression by document order and remove duplicate nodes
            // cache = runtime.DocOrderDistinct(cache);
            this.helper.LoadQueryRuntime();
            NestedVisitEnsureCache(ndDod.Child, typeof(XPathNavigator));
            this.iterCurr.EnsureStack();
            this.helper.Call(XmlILMethods.DocOrder);
            return ndDod;
        }

        /// <summary>
        /// There are a number of path patterns that can be rooted at DocOrderDistinct nodes.  Determine whether one of these
        /// patterns has been previously matched on "ndDod".  If so, generate code for the pattern and return true.  Otherwise,
        /// just return false.
        /// </summary>
        private bool HandleDodPatterns(QilUnary ndDod) {
            OptimizerPatterns pattDod = OptimizerPatterns.Read(ndDod);
            XmlNodeKindFlags kinds;
            QilName name;
            QilNode input, step;
            bool isJoinAndDod;

            // Handle JoinAndDod and DodReverse patterns
            isJoinAndDod = pattDod.MatchesPattern(OptimizerPatternName.JoinAndDod);
            if (isJoinAndDod || pattDod.MatchesPattern(OptimizerPatternName.DodReverse)) {
                OptimizerPatterns pattStep = OptimizerPatterns.Read((QilNode) pattDod.GetArgument(OptimizerPatternArgument.DodStep));

                if (pattStep.MatchesPattern(OptimizerPatternName.FilterElements)) {
                    // FilterElements pattern, so Kind = Element and Name = Argument
                    kinds = XmlNodeKindFlags.Element;
                    name = (QilName) pattStep.GetArgument(OptimizerPatternArgument.ElementQName);
                }
                else if (pattStep.MatchesPattern(OptimizerPatternName.FilterContentKind)) {
                    // FilterKindTest pattern, so Kind = Argument and Name = null
                    kinds = ((XmlQueryType) pattStep.GetArgument(OptimizerPatternArgument.KindTestType)).NodeKinds;
                    name = null;
                }
                else {
                    Debug.Assert(pattStep.MatchesPattern(OptimizerPatternName.Axis), "Dod patterns should only match if step is FilterElements or FilterKindTest or Axis");
                    kinds = ((ndDod.XmlType.NodeKinds & XmlNodeKindFlags.Attribute) != 0) ? XmlNodeKindFlags.Any : XmlNodeKindFlags.Content;
                    name = null;
                }

                step = (QilNode) pattStep.GetArgument(OptimizerPatternArgument.StepNode);
                if (isJoinAndDod) {
                    switch (step.NodeType) {
                        case QilNodeType.Content:
                            CreateContainerIterator(ndDod, "$$$iterContent", typeof(ContentMergeIterator), XmlILMethods.ContentMergeCreate, XmlILMethods.ContentMergeNext,
                                                    kinds, name, TriState.Unknown);
                            return true;

                        case QilNodeType.Descendant:
                        case QilNodeType.DescendantOrSelf:
                            CreateContainerIterator(ndDod, "$$$iterDesc", typeof(DescendantMergeIterator), XmlILMethods.DescMergeCreate, XmlILMethods.DescMergeNext,
                                                    kinds, name, (step.NodeType == QilNodeType.Descendant) ? TriState.False : TriState.True);
                            return true;

                        case QilNodeType.XPathFollowing:
                            CreateContainerIterator(ndDod, "$$$iterFoll", typeof(XPathFollowingMergeIterator), XmlILMethods.XPFollMergeCreate, XmlILMethods.XPFollMergeNext,
                                                    kinds, name, TriState.Unknown);
                            return true;

                        case QilNodeType.FollowingSibling:
                            CreateContainerIterator(ndDod, "$$$iterFollSib", typeof(FollowingSiblingMergeIterator), XmlILMethods.FollSibMergeCreate, XmlILMethods.FollSibMergeNext,
                                                    kinds, name, TriState.Unknown);
                            return true;

                        case QilNodeType.XPathPreceding:
                            CreateContainerIterator(ndDod, "$$$iterPrec", typeof(XPathPrecedingMergeIterator), XmlILMethods.XPPrecMergeCreate, XmlILMethods.XPPrecMergeNext,
                                                    kinds, name, TriState.Unknown);
                            return true;

                        default:
                            Debug.Assert(false, "Pattern " + step.NodeType + " should have been handled.");
                            break;
                    }
                }
                else {
                    input = (QilNode) pattStep.GetArgument(OptimizerPatternArgument.StepInput);
                    switch (step.NodeType) {
                        case QilNodeType.Ancestor:
                        case QilNodeType.AncestorOrSelf:
                            CreateFilteredIterator(input, "$$$iterAnc", typeof(AncestorDocOrderIterator), XmlILMethods.AncDOCreate, XmlILMethods.AncDONext,
                                                   kinds, name, (step.NodeType == QilNodeType.Ancestor) ? TriState.False : TriState.True, null);
                            return true;

                        case QilNodeType.PrecedingSibling:
                            CreateFilteredIterator(input, "$$$iterPreSib", typeof(PrecedingSiblingDocOrderIterator), XmlILMethods.PreSibDOCreate, XmlILMethods.PreSibDONext,
                                                   kinds, name, TriState.Unknown, null);
                            return true;

                        case QilNodeType.XPathPreceding:
                            CreateFilteredIterator(input, "$$$iterPrec", typeof(XPathPrecedingDocOrderIterator), XmlILMethods.XPPrecDOCreate, XmlILMethods.XPPrecDONext,
                                                   kinds, name, TriState.Unknown, null);
                            return true;

                        default:
                            Debug.Assert(false, "Pattern " + step.NodeType + " should have been handled.");
                            break;
                    }
                }
            }
            else if (pattDod.MatchesPattern(OptimizerPatternName.DodMerge)) {
                // DodSequenceMerge dodMerge;
                LocalBuilder locMerge = this.helper.DeclareLocal("$$$dodMerge", typeof(DodSequenceMerge));
                Label lblOnEnd = this.helper.DefineLabel();

                // dodMerge.Create(runtime);
                this.helper.Emit(OpCodes.Ldloca, locMerge);
                this.helper.LoadQueryRuntime();
                this.helper.Call(XmlILMethods.DodMergeCreate);
                this.helper.Emit(OpCodes.Ldloca, locMerge);

                StartNestedIterator(ndDod.Child, lblOnEnd);

                // foreach (seq in expr) {
                Visit(ndDod.Child);

                // dodMerge.AddSequence(seq);
                Debug.Assert(this.iterCurr.Storage.IsCached, "DodMerge pattern should only be matched when cached sequences are returned from loop");
                this.iterCurr.EnsureStack();
                this.helper.Call(XmlILMethods.DodMergeAdd);
                this.helper.Emit(OpCodes.Ldloca, locMerge);

                // }
                this.iterCurr.LoopToEnd(lblOnEnd);

                EndNestedIterator(ndDod.Child);

                // mergedSequence = dodMerge.MergeSequences();
                this.helper.Call(XmlILMethods.DodMergeSeq);

                this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XPathNavigator), true);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Generate code for for QilNodeType.Invoke.
        /// </summary>
        protected override QilNode VisitInvoke(QilInvoke ndInvoke) {
            QilFunction ndFunc = ndInvoke.Function;
            MethodInfo methInfo = XmlILAnnotation.Write(ndFunc).FunctionBinding;
            bool useWriter = (XmlILConstructInfo.Read(ndFunc).ConstructMethod == XmlILConstructMethod.Writer);
            Debug.Assert(!XmlILConstructInfo.Read(ndInvoke).PushToWriterFirst || useWriter);

            // Push XmlQueryRuntime onto the stack as the first parameter
            this.helper.LoadQueryRuntime();

            // Generate code to push each Invoke argument onto the stack
            for (int iArg = 0; iArg < ndInvoke.Arguments.Count; iArg++) {
                QilNode ndActualArg = ndInvoke.Arguments[iArg];
                QilNode ndFormalArg = ndInvoke.Function.Arguments[iArg];
                NestedVisitEnsureStack(ndActualArg, GetItemStorageType(ndFormalArg), !ndFormalArg.XmlType.IsSingleton);
            }

            // Check whether this call should compiled using the .tailcall instruction
            if (OptimizerPatterns.Read(ndInvoke).MatchesPattern(OptimizerPatternName.TailCall))
                this.helper.TailCall(methInfo);
            else
                this.helper.Call(methInfo);

            // If function's results are not pushed to Writer,
            if (!useWriter) {
                // Return value is on the stack; ensure it has the correct storage type
                this.iterCurr.Storage = StorageDescriptor.Stack(GetItemStorageType(ndInvoke), !ndInvoke.XmlType.IsSingleton);
            }
            else {
                this.iterCurr.Storage = StorageDescriptor.None();
            }

            return ndInvoke;
        }

        /// <summary>
        /// Generate code for for QilNodeType.Content.
        /// </summary>
        protected override QilNode VisitContent(QilUnary ndContent) {
            CreateSimpleIterator(ndContent.Child, "$$$iterAttrContent", typeof(AttributeContentIterator), XmlILMethods.AttrContentCreate, XmlILMethods.AttrContentNext);
            return ndContent;
        }

        /// <summary>
        /// Generate code for for QilNodeType.Attribute.
        /// </summary>
        protected override QilNode VisitAttribute(QilBinary ndAttr) {
            QilName ndName = ndAttr.Right as QilName;
            Debug.Assert(ndName != null, "Attribute node must have a literal QName as its second argument");

            // XPathNavigator navAttr;
            LocalBuilder locNav = this.helper.DeclareLocal("$$$navAttr", typeof(XPathNavigator));

            // navAttr = SyncToNavigator(navAttr, navCtxt);
            SyncToNavigator(locNav, ndAttr.Left);

            // if (!navAttr.MoveToAttribute(localName, namespaceUri)) goto LabelNextCtxt;
            this.helper.Emit(OpCodes.Ldloc, locNav);
            this.helper.CallGetAtomizedName(this.helper.StaticData.DeclareName(ndName.LocalName));
            this.helper.CallGetAtomizedName(this.helper.StaticData.DeclareName(ndName.NamespaceUri));
            this.helper.Call(XmlILMethods.NavMoveAttr);
            this.helper.Emit(OpCodes.Brfalse, this.iterCurr.GetLabelNext());

            this.iterCurr.Storage = StorageDescriptor.Local(locNav, typeof(XPathNavigator), false);
            return ndAttr;
        }

        /// <summary>
        /// Generate code for for QilNodeType.Parent.
        /// </summary>
        protected override QilNode VisitParent(QilUnary ndParent) {
            // XPathNavigator navParent;
            LocalBuilder locNav = this.helper.DeclareLocal("$$$navParent", typeof(XPathNavigator));

            // navParent = SyncToNavigator(navParent, navCtxt);
            SyncToNavigator(locNav, ndParent.Child);

            // if (!navParent.MoveToParent()) goto LabelNextCtxt;
            this.helper.Emit(OpCodes.Ldloc, locNav);
            this.helper.Call(XmlILMethods.NavMoveParent);
            this.helper.Emit(OpCodes.Brfalse, this.iterCurr.GetLabelNext());

            this.iterCurr.Storage = StorageDescriptor.Local(locNav, typeof(XPathNavigator), false);
            return ndParent;
        }

        /// <summary>
        /// Generate code for for QilNodeType.Root.
        /// </summary>
        protected override QilNode VisitRoot(QilUnary ndRoot) {
            // XPathNavigator navRoot;
            LocalBuilder locNav = this.helper.DeclareLocal("$$$navRoot", typeof(XPathNavigator));

            // navRoot = SyncToNavigator(navRoot, navCtxt);
            SyncToNavigator(locNav, ndRoot.Child);

            // navRoot.MoveToRoot();
            this.helper.Emit(OpCodes.Ldloc, locNav);
            this.helper.Call(XmlILMethods.NavMoveRoot);

            this.iterCurr.Storage = StorageDescriptor.Local(locNav, typeof(XPathNavigator), false);
            return ndRoot;
        }

        /// <summary>
        /// Generate code for QilNodeType.XmlContext.
        /// </summary>
        /// <remarks>
        /// Generates code to retrieve the default document using the XmlResolver.
        /// </remarks>
        protected override QilNode VisitXmlContext(QilNode ndCtxt) {
            // runtime.ExternalContext.DefaultDataSource
            this.helper.LoadQueryContext();
            this.helper.Call(XmlILMethods.GetDefaultDataSource);
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XPathNavigator), false);
            return ndCtxt;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.Descendant.
        /// </summary>
        protected override QilNode VisitDescendant(QilUnary ndDesc) {
            CreateFilteredIterator(ndDesc.Child, "$$$iterDesc", typeof(DescendantIterator), XmlILMethods.DescCreate, XmlILMethods.DescNext,
                                   XmlNodeKindFlags.Any, null, TriState.False, null);
            return ndDesc;
        }

        /// <summary>
        /// Generate code for for QilNodeType.DescendantOrSelf.
        /// </summary>
        protected override QilNode VisitDescendantOrSelf(QilUnary ndDesc) {
            CreateFilteredIterator(ndDesc.Child, "$$$iterDesc", typeof(DescendantIterator), XmlILMethods.DescCreate, XmlILMethods.DescNext,
                                   XmlNodeKindFlags.Any, null, TriState.True, null);
            return ndDesc;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.Ancestor.
        /// </summary>
        protected override QilNode VisitAncestor(QilUnary ndAnc) {
            CreateFilteredIterator(ndAnc.Child, "$$$iterAnc", typeof(AncestorIterator), XmlILMethods.AncCreate, XmlILMethods.AncNext,
                                   XmlNodeKindFlags.Any, null, TriState.False, null);
            return ndAnc;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.AncestorOrSelf.
        /// </summary>
        protected override QilNode VisitAncestorOrSelf(QilUnary ndAnc) {
            CreateFilteredIterator(ndAnc.Child, "$$$iterAnc", typeof(AncestorIterator), XmlILMethods.AncCreate, XmlILMethods.AncNext,
                                   XmlNodeKindFlags.Any, null, TriState.True, null);
            return ndAnc;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.Preceding.
        /// </summary>
        protected override QilNode VisitPreceding(QilUnary ndPrec) {
            CreateFilteredIterator(ndPrec.Child, "$$$iterPrec", typeof(PrecedingIterator), XmlILMethods.PrecCreate, XmlILMethods.PrecNext,
                                   XmlNodeKindFlags.Any, null, TriState.Unknown, null);
            return ndPrec;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.FollowingSibling.
        /// </summary>
        protected override QilNode VisitFollowingSibling(QilUnary ndFollSib) {
            CreateFilteredIterator(ndFollSib.Child, "$$$iterFollSib", typeof(FollowingSiblingIterator), XmlILMethods.FollSibCreate, XmlILMethods.FollSibNext,
                                   XmlNodeKindFlags.Any, null, TriState.Unknown, null);
            return ndFollSib;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.PrecedingSibling.
        /// </summary>
        protected override QilNode VisitPrecedingSibling(QilUnary ndPreSib) {
            CreateFilteredIterator(ndPreSib.Child, "$$$iterPreSib", typeof(PrecedingSiblingIterator), XmlILMethods.PreSibCreate, XmlILMethods.PreSibNext,
                                   XmlNodeKindFlags.Any, null, TriState.Unknown, null);
            return ndPreSib;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.NodeRange.
        /// </summary>
        protected override QilNode VisitNodeRange(QilBinary ndRange) {
            CreateFilteredIterator(ndRange.Left, "$$$iterRange", typeof(NodeRangeIterator), XmlILMethods.NodeRangeCreate, XmlILMethods.NodeRangeNext,
                                   XmlNodeKindFlags.Any, null, TriState.Unknown, ndRange.Right);
            return ndRange;
        }

        /// <summary>
        /// Generate code for for QilNodeType.Deref.
        /// </summary>
        protected override QilNode VisitDeref(QilBinary ndDeref) {
            // IdIterator iterId;
            LocalBuilder locIter = this.helper.DeclareLocal("$$$iterId", typeof(IdIterator));

            // iterId.Create(navCtxt, value);
            this.helper.Emit(OpCodes.Ldloca, locIter);
            NestedVisitEnsureStack(ndDeref.Left);
            NestedVisitEnsureStack(ndDeref.Right);
            this.helper.Call(XmlILMethods.IdCreate);

            GenerateSimpleIterator(typeof(XPathNavigator), locIter, XmlILMethods.IdNext);

            return ndDeref;
        }

        /// <summary>
        /// Generate code for QilNodeType.ElementCtor.
        /// </summary>
        protected override QilNode VisitElementCtor(QilBinary ndElem) {
            XmlILConstructInfo info = XmlILConstructInfo.Read(ndElem);
            bool callChk;
            GenerateNameType nameType;
            Debug.Assert(XmlILConstructInfo.Read(ndElem).PushToWriterFirst, "Element contruction should always be pushed to writer.");

            // Runtime checks must be made in the following cases:
            //   1. Xml state is not known at compile-time, or is illegal
            //   2. Element's namespace must be declared
            //   3. Element's attributes might be duplicates of one another, or namespaces might follow attributes
            callChk = CheckWithinContent(info) || !info.IsNamespaceInScope || ElementCachesAttributes(info);

            // If it is not known whether element content was output, then make this check at run-time
            if (XmlILConstructInfo.Read(ndElem.Right).FinalStates == PossibleXmlStates.Any)
                callChk = true;

            // If runtime state after EndElement is called is not known, then call XmlQueryOutput.WriteEndElementChk
            if (info.FinalStates == PossibleXmlStates.Any)
                callChk = true;

            // If WriteStartElementChk will *not* be called, then code must be generated to ensure valid state transitions
            if (!callChk)
                BeforeStartChecks(ndElem);

            // Generate call to WriteStartElement
            nameType = LoadNameAndType(XPathNodeType.Element, ndElem.Left, true, callChk);
            this.helper.CallWriteStartElement(nameType, callChk);

            // Recursively construct content
            NestedVisit(ndElem.Right);

            // If runtime state is guaranteed to be EnumAttrs, and an element is being constructed, call XmlQueryOutput.StartElementContent
            if (XmlILConstructInfo.Read(ndElem.Right).FinalStates == PossibleXmlStates.EnumAttrs && !callChk)
                this.helper.CallStartElementContent();

            // Generate call to WriteEndElement
            nameType = LoadNameAndType(XPathNodeType.Element, ndElem.Left, false, callChk);
            this.helper.CallWriteEndElement(nameType, callChk);

            if (!callChk)
                AfterEndChecks(ndElem);

            this.iterCurr.Storage = StorageDescriptor.None();
            return ndElem;
        }

        /// <summary>
        /// Generate code for QilNodeType.AttributeCtor.
        /// </summary>
        protected override QilNode VisitAttributeCtor(QilBinary ndAttr) {
            XmlILConstructInfo info = XmlILConstructInfo.Read(ndAttr);
            bool callChk;
            GenerateNameType nameType;
            Debug.Assert(XmlILConstructInfo.Read(ndAttr).PushToWriterFirst, "Attribute construction should always be pushed to writer.");

            // Runtime checks must be made in the following cases:
            //   1. Xml state is not known at compile-time, or is illegal
            //   2. Attribute's namespace must be declared
            callChk = CheckEnumAttrs(info) || !info.IsNamespaceInScope;

            // If WriteStartAttributeChk will *not* be called, then code must be generated to ensure well-formedness
            // and track namespace scope.
            if (!callChk)
                BeforeStartChecks(ndAttr);

            // Generate call to WriteStartAttribute
            nameType = LoadNameAndType(XPathNodeType.Attribute, ndAttr.Left, true, callChk);
            this.helper.CallWriteStartAttribute(nameType, callChk);

            // Recursively construct content
            NestedVisit(ndAttr.Right);

            // Generate call to WriteEndAttribute
            this.helper.CallWriteEndAttribute(callChk);

            if (!callChk)
                AfterEndChecks(ndAttr);

            this.iterCurr.Storage = StorageDescriptor.None();
            return ndAttr;
        }

        /// <summary>
        /// Generate code for QilNodeType.CommentCtor.
        /// </summary>
        protected override QilNode VisitCommentCtor(QilUnary ndComment) {
            Debug.Assert(XmlILConstructInfo.Read(ndComment).PushToWriterFirst, "Comment construction should always be pushed to writer.");

            // Always call XmlQueryOutput.WriteStartComment
            this.helper.CallWriteStartComment();

            // Recursively construct content
            NestedVisit(ndComment.Child);

            // Always call XmlQueryOutput.WriteEndComment
            this.helper.CallWriteEndComment();

            this.iterCurr.Storage = StorageDescriptor.None();
            return ndComment;
        }

        /// <summary>
        /// Generate code for QilNodeType.PICtor.
        /// </summary>
        protected override QilNode VisitPICtor(QilBinary ndPI) {
            Debug.Assert(XmlILConstructInfo.Read(ndPI).PushToWriterFirst, "PI construction should always be pushed to writer.");

            // Always call XmlQueryOutput.WriteStartPI
            this.helper.LoadQueryOutput();
            NestedVisitEnsureStack(ndPI.Left);
            this.helper.CallWriteStartPI();

            // Recursively construct content
            NestedVisit(ndPI.Right);

            // Always call XmlQueryOutput.WriteEndPI
            this.helper.CallWriteEndPI();

            this.iterCurr.Storage = StorageDescriptor.None();
            return ndPI;
        }

        /// <summary>
        /// Generate code for QilNodeType.TextCtor.
        /// </summary>
        protected override QilNode VisitTextCtor(QilUnary ndText) {
            return VisitTextCtor(ndText, false);
        }

        /// <summary>
        /// Generate code for QilNodeType.RawTextCtor.
        /// </summary>
        protected override QilNode VisitRawTextCtor(QilUnary ndText) {
            return VisitTextCtor(ndText, true);
        }

        /// <summary>
        /// Generate code for QilNodeType.TextCtor and QilNodeType.RawTextCtor.
        /// </summary>
        private QilNode VisitTextCtor(QilUnary ndText, bool disableOutputEscaping) {
            XmlILConstructInfo info = XmlILConstructInfo.Read(ndText);
            bool callChk;
            Debug.Assert(info.PushToWriterFirst, "Text construction should always be pushed to writer.");

            // Write out text in different contexts (within attribute, within element, within comment, etc.)
            switch (info.InitialStates) {
                case PossibleXmlStates.WithinAttr:
                case PossibleXmlStates.WithinComment:
                case PossibleXmlStates.WithinPI:
                    callChk = false;
                    break;

                default:
                    callChk = CheckWithinContent(info);
                    break;
            }

            if (!callChk)
                BeforeStartChecks(ndText);

            this.helper.LoadQueryOutput();

            // Push string value of text onto IL stack
            NestedVisitEnsureStack(ndText.Child);

            // Write out text in different contexts (within attribute, within element, within comment, etc.)
            switch (info.InitialStates) {
                case PossibleXmlStates.WithinAttr:
                    // Ignore hints when writing out attribute text
                    this.helper.CallWriteString(false, callChk);
                    break;

                case PossibleXmlStates.WithinComment:
                    // Call XmlQueryOutput.WriteCommentString
                    this.helper.Call(XmlILMethods.CommentText);
                    break;

                case PossibleXmlStates.WithinPI:
                    // Call XmlQueryOutput.WriteProcessingInstructionString
                    this.helper.Call(XmlILMethods.PIText);
                    break;

                default:
                    // Call XmlQueryOutput.WriteTextBlockChk, XmlQueryOutput.WriteTextBlockNoEntities, or XmlQueryOutput.WriteTextBlock
                    this.helper.CallWriteString(disableOutputEscaping, callChk);
                    break;
            }

            if (!callChk)
                AfterEndChecks(ndText);

            this.iterCurr.Storage = StorageDescriptor.None();
            return ndText;
        }

        /// <summary>
        /// Generate code for QilNodeType.DocumentCtor.
        /// </summary>
        protected override QilNode VisitDocumentCtor(QilUnary ndDoc) {
            Debug.Assert(XmlILConstructInfo.Read(ndDoc).PushToWriterFirst, "Document root construction should always be pushed to writer.");

            // Generate call to XmlQueryOutput.WriteStartRootChk
            this.helper.CallWriteStartRoot();

            // Recursively construct content
            NestedVisit(ndDoc.Child);

            // Generate call to XmlQueryOutput.WriteEndRootChk
            this.helper.CallWriteEndRoot();

            this.iterCurr.Storage = StorageDescriptor.None();

            return ndDoc;
        }

        /// <summary>
        /// Generate code for QilNodeType.NamespaceDecl.
        /// </summary>
        protected override QilNode VisitNamespaceDecl(QilBinary ndNmsp) {
            XmlILConstructInfo info = XmlILConstructInfo.Read(ndNmsp);
            bool callChk;
            Debug.Assert(info.PushToWriterFirst, "Namespace construction should always be pushed to writer.");

            // Runtime checks must be made in the following cases:
            //   1. Xml state is not known at compile-time, or is illegal
            //   2. Namespaces might be added to element after attributes have already been added
            callChk = CheckEnumAttrs(info) || MightHaveNamespacesAfterAttributes(info);

            // If WriteNamespaceDeclarationChk will *not* be called, then code must be generated to ensure well-formedness
            // and track namespace scope.
            if (!callChk)
                BeforeStartChecks(ndNmsp);

            this.helper.LoadQueryOutput();

            // Recursively construct prefix and ns
            NestedVisitEnsureStack(ndNmsp.Left);
            NestedVisitEnsureStack(ndNmsp.Right);

            // Generate call to WriteNamespaceDecl
            this.helper.CallWriteNamespaceDecl(callChk);

            if (!callChk)
                AfterEndChecks(ndNmsp);

            this.iterCurr.Storage = StorageDescriptor.None();
            return ndNmsp;
        }

        /// <summary>
        /// Generate code for for QilNodeType.RtfCtor.
        /// </summary>
        protected override QilNode VisitRtfCtor(QilBinary ndRtf) {
            OptimizerPatterns patt = OptimizerPatterns.Read(ndRtf);
            string baseUri = (string) (QilLiteral) ndRtf.Right;

            if (patt.MatchesPattern(OptimizerPatternName.SingleTextRtf)) {
                // Special-case Rtf containing a root node and a single text node child
                this.helper.LoadQueryRuntime();
                NestedVisitEnsureStack((QilNode) patt.GetArgument(OptimizerPatternArgument.RtfText));
                this.helper.Emit(OpCodes.Ldstr, baseUri);
                this.helper.Call(XmlILMethods.RtfConstr);
            }
            else {
                // Start nested construction of an Rtf
                this.helper.CallStartRtfConstruction(baseUri);

                // Write content of Rtf to writer
                NestedVisit(ndRtf.Left);

                // Get the result Rtf
                this.helper.CallEndRtfConstruction();
            }

            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XPathNavigator), false);
            return ndRtf;
        }

        /// <summary>
        /// Generate code for QilNodeType.NameOf.
        /// </summary>
        protected override QilNode VisitNameOf(QilUnary ndName) {
            return VisitNodeProperty(ndName);
        }

        /// <summary>
        /// Generate code for QilNodeType.LocalNameOf.
        /// </summary>
        protected override QilNode VisitLocalNameOf(QilUnary ndName) {
            return VisitNodeProperty(ndName);
        }

        /// <summary>
        /// Generate code for QilNodeType.NamespaceUriOf.
        /// </summary>
        protected override QilNode VisitNamespaceUriOf(QilUnary ndName) {
            return VisitNodeProperty(ndName);
        }

        /// <summary>
        /// Generate code for QilNodeType.PrefixOf.
        /// </summary>
        protected override QilNode VisitPrefixOf(QilUnary ndName) {
            return VisitNodeProperty(ndName);
        }

        /// <summary>
        /// Generate code to push the local name, namespace uri, or qname of the context navigator.
        /// </summary>
        private QilNode VisitNodeProperty(QilUnary ndProp) {
            // Generate code to push argument onto stack
            NestedVisitEnsureStack(ndProp.Child);

            switch (ndProp.NodeType) {
                case QilNodeType.NameOf:
                    // push new XmlQualifiedName(navigator.LocalName, navigator.NamespaceURI);
                    this.helper.Emit(OpCodes.Dup);
                    this.helper.Call(XmlILMethods.NavLocalName);
                    this.helper.Call(XmlILMethods.NavNmsp);
                    this.helper.Construct(XmlILConstructors.QName);
                    this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XmlQualifiedName), false);
                    break;

                case QilNodeType.LocalNameOf:
                    // push navigator.Name;
                    this.helper.Call(XmlILMethods.NavLocalName);
                    this.iterCurr.Storage = StorageDescriptor.Stack(typeof(string), false);
                    break;

                case QilNodeType.NamespaceUriOf:
                    // push navigator.NamespaceURI;
                    this.helper.Call(XmlILMethods.NavNmsp);
                    this.iterCurr.Storage = StorageDescriptor.Stack(typeof(string), false);
                    break;

                case QilNodeType.PrefixOf:
                    // push navigator.Prefix;
                    this.helper.Call(XmlILMethods.NavPrefix);
                    this.iterCurr.Storage = StorageDescriptor.Stack(typeof(string), false);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            return ndProp;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.TypeAssert.
        /// </summary>
        protected override QilNode VisitTypeAssert(QilTargetType ndTypeAssert) {
            if (!ndTypeAssert.Source.XmlType.IsSingleton && ndTypeAssert.XmlType.IsSingleton && !this.iterCurr.HasLabelNext) {
                // This case occurs when a non-singleton expression is treated as cardinality One.
                // The trouble is that the expression will branch to an end label when it's done iterating, so
                // an end label must be provided.  But there is no next label in the current iteration context,
                // so we've got to create a dummy label instead (IL requires it).  This creates an infinite loop,
                // but since it's known statically that the expression is cardinality One, this branch will never
                // be taken.
                Label lblDummy = this.helper.DefineLabel();
                this.helper.MarkLabel(lblDummy);
                NestedVisit(ndTypeAssert.Source, lblDummy);
            }
            else {
                // Generate code for child expression
                Visit(ndTypeAssert.Source);
            }

            this.iterCurr.EnsureItemStorageType(ndTypeAssert.Source.XmlType, GetItemStorageType(ndTypeAssert));
            return ndTypeAssert;
        }

        /// <summary>
        /// Generate code for QilNodeType.IsType.
        /// </summary>
        protected override QilNode VisitIsType(QilTargetType ndIsType) {
            XmlQueryType typDerived, typBase;
            XmlTypeCode codeBase;

            typDerived = ndIsType.Source.XmlType;
            typBase = ndIsType.TargetType;
            Debug.Assert(!typDerived.NeverSubtypeOf(typBase), "Normalizer should have eliminated IsType where source can never be a subtype of destination type.");

            // Special Case: Test whether singleton item is a Node
            if (typDerived.IsSingleton && (object) typBase == (object) TypeFactory.Node) {
                NestedVisitEnsureStack(ndIsType.Source);
                Debug.Assert(this.iterCurr.Storage.ItemStorageType == typeof(XPathItem), "If !IsNode, then storage type should be Item");

                // if (item.IsNode op true) goto LabelBranch;
                this.helper.Call(XmlILMethods.ItemIsNode);
                ZeroCompare(QilNodeType.Ne, true);

                return ndIsType;
            }

            // Special Case: Source value is a singleton Node, and we're testing whether it is an Element, Attribute, PI, etc.
            if (MatchesNodeKinds(ndIsType, typDerived, typBase))
                return ndIsType;

            // Special Case: XmlTypeCode is sufficient to describe destination type
            if ((object) typBase == (object) TypeFactory.Double) codeBase = XmlTypeCode.Double;
            else if ((object) typBase == (object) TypeFactory.String) codeBase = XmlTypeCode.String;
            else if ((object) typBase == (object) TypeFactory.Boolean) codeBase = XmlTypeCode.Boolean;
            else if ((object) typBase == (object) TypeFactory.Node) codeBase = XmlTypeCode.Node;
            else codeBase = XmlTypeCode.None;

            if (codeBase != XmlTypeCode.None) {
                // if (runtime.MatchesXmlType(value, code) op true) goto LabelBranch;
                this.helper.LoadQueryRuntime();
                NestedVisitEnsureStack(ndIsType.Source, typeof(XPathItem), !typDerived.IsSingleton);
                this.helper.LoadInteger((int) codeBase);
                this.helper.Call(typDerived.IsSingleton ? XmlILMethods.ItemMatchesCode : XmlILMethods.SeqMatchesCode);
                ZeroCompare(QilNodeType.Ne, true);

                return ndIsType;
            }

            // if (runtime.MatchesXmlType(value, idxType) op true) goto LabelBranch;
            this.helper.LoadQueryRuntime();
            NestedVisitEnsureStack(ndIsType.Source, typeof(XPathItem), !typDerived.IsSingleton);
            this.helper.LoadInteger(this.helper.StaticData.DeclareXmlType(typBase));
            this.helper.Call(typDerived.IsSingleton ? XmlILMethods.ItemMatchesType : XmlILMethods.SeqMatchesType);
            ZeroCompare(QilNodeType.Ne, true);

            return ndIsType;
        }

        /// <summary>
        /// Faster code can be generated if type test is just a node kind test.  If this special case is detected, then generate code and return true.
        /// Otherwise, return false, and a call to MatchesXmlType will be generated instead.
        /// </summary>
        private bool MatchesNodeKinds(QilTargetType ndIsType, XmlQueryType typDerived, XmlQueryType typBase) {
            XmlNodeKindFlags kinds;
            bool allowKinds = true;
            XPathNodeType kindsRuntime;
            int kindsUnion;

            // If not checking whether typDerived is some kind of singleton node, then fallback to MatchesXmlType
            if (!typBase.IsNode || !typBase.IsSingleton)
                return false;

            // If typDerived is not statically guaranteed to be a singleton node (and not an rtf), then fallback to MatchesXmlType
            if (!typDerived.IsNode || !typDerived.IsSingleton || !typDerived.IsNotRtf)
                return false;

            // Now we are guaranteed that typDerived is a node, and typBase is a node, so check node kinds
            // Ensure that typBase is only composed of kind-test prime types (no name-test, no schema-test, etc.)
            kinds = XmlNodeKindFlags.None;
            foreach (XmlQueryType typItem in typBase) {
                if ((object) typItem == (object) TypeFactory.Element) kinds |= XmlNodeKindFlags.Element;
                else if ((object) typItem == (object) TypeFactory.Attribute) kinds |= XmlNodeKindFlags.Attribute;
                else if ((object) typItem == (object) TypeFactory.Text) kinds |= XmlNodeKindFlags.Text;
                else if ((object) typItem == (object) TypeFactory.Document) kinds |= XmlNodeKindFlags.Document;
                else if ((object) typItem == (object) TypeFactory.Comment) kinds |= XmlNodeKindFlags.Comment;
                else if ((object) typItem == (object) TypeFactory.PI) kinds |= XmlNodeKindFlags.PI;
                else if ((object) typItem == (object) TypeFactory.Namespace) kinds |= XmlNodeKindFlags.Namespace;
                else return false;
            }

            Debug.Assert((typDerived.NodeKinds & kinds) != XmlNodeKindFlags.None, "Normalizer should have taken care of case where node kinds are disjoint.");

            kinds = typDerived.NodeKinds & kinds;

            // Attempt to allow or disallow exactly one kind
            if (!Bits.ExactlyOne((uint) kinds)) {
                // Not possible to allow one kind, so try to disallow one kind
                kinds = ~kinds & XmlNodeKindFlags.Any;
                allowKinds = !allowKinds;
            }

            switch (kinds) {
                case XmlNodeKindFlags.Element: kindsRuntime = XPathNodeType.Element; break;
                case XmlNodeKindFlags.Attribute: kindsRuntime = XPathNodeType.Attribute; break;
                case XmlNodeKindFlags.Namespace: kindsRuntime = XPathNodeType.Namespace; break;
                case XmlNodeKindFlags.PI: kindsRuntime = XPathNodeType.ProcessingInstruction; break;
                case XmlNodeKindFlags.Comment: kindsRuntime = XPathNodeType.Comment; break;
                case XmlNodeKindFlags.Document: kindsRuntime = XPathNodeType.Root; break;

                default:
                    // Union of several types (when testing for Text, we need to test for Whitespace as well)

                    // if (((1 << navigator.NodeType) & nodesDisallow) op 0) goto LabelBranch;
                    this.helper.Emit(OpCodes.Ldc_I4_1);
                    kindsRuntime = XPathNodeType.All;
                    break;
            }

            // Push navigator.NodeType onto the stack
            NestedVisitEnsureStack(ndIsType.Source);
            this.helper.Call(XmlILMethods.NavType);

            if (kindsRuntime == XPathNodeType.All) {
                // if (((1 << navigator.NodeType) & kindsUnion) op 0) goto LabelBranch;
                this.helper.Emit(OpCodes.Shl);

                kindsUnion = 0;
                if ((kinds & XmlNodeKindFlags.Document) != 0) kindsUnion |= (1 << (int) XPathNodeType.Root);
                if ((kinds & XmlNodeKindFlags.Element) != 0) kindsUnion |= (1 << (int) XPathNodeType.Element);
                if ((kinds & XmlNodeKindFlags.Attribute) != 0) kindsUnion |= (1 << (int) XPathNodeType.Attribute);
                if ((kinds & XmlNodeKindFlags.Text) != 0) kindsUnion |= (1 << (int) (int) XPathNodeType.Text) |
                                                                      (1 << (int) (int) XPathNodeType.SignificantWhitespace) |
                                                                      (1 << (int) (int) XPathNodeType.Whitespace);
                if ((kinds & XmlNodeKindFlags.Comment) != 0) kindsUnion |= (1 << (int) XPathNodeType.Comment);
                if ((kinds & XmlNodeKindFlags.PI) != 0) kindsUnion |= (1 << (int) XPathNodeType.ProcessingInstruction);
                if ((kinds & XmlNodeKindFlags.Namespace) != 0) kindsUnion |= (1 << (int) XPathNodeType.Namespace);
                
                this.helper.LoadInteger(kindsUnion);
                this.helper.Emit(OpCodes.And);
                ZeroCompare(allowKinds ? QilNodeType.Ne : QilNodeType.Eq, false);
            }
            else {
                // if (navigator.NodeType op runtimeItem) goto LabelBranch;
                this.helper.LoadInteger((int) kindsRuntime);
                ClrCompare(allowKinds ? QilNodeType.Eq : QilNodeType.Ne, XmlTypeCode.Int);
            }

            return true;
        }

        /// <summary>
        /// Generate code for QilNodeType.IsEmpty.
        /// </summary>
        /// <remarks>
        /// BranchingContext.OnFalse context: is-empty(expr)
        /// ==> foreach (item in expr)
        ///         goto LabelBranch;
        ///
        /// BranchingContext.OnTrue context: is-empty(expr)
        /// ==> foreach (item in expr)
        ///         break;
        ///     ...
        ///     LabelOnEnd: (called if foreach is empty)
        ///     goto LabelBranch;
        ///
        /// BranchingContext.None context: is-empty(expr)
        /// ==> foreach (item in expr)
        ///         break;
        ///     push true();
        ///     ...
        ///     LabelOnEnd: (called if foreach is empty)
        ///     push false();
        /// </remarks>
        protected override QilNode VisitIsEmpty(QilUnary ndIsEmpty) {
            Label lblTrue;

            // If the child expression returns a cached result,
            if (CachesResult(ndIsEmpty.Child)) {
                // Then get the count directly from the cache
                NestedVisitEnsureStack(ndIsEmpty.Child);
                this.helper.CallCacheCount(this.iterNested.Storage.ItemStorageType);

                switch (this.iterCurr.CurrentBranchingContext) {
                    case BranchingContext.OnFalse:
                        // Take false path if count != 0
                        this.helper.TestAndBranch(0, this.iterCurr.LabelBranch, OpCodes.Bne_Un);
                        break;

                    case BranchingContext.OnTrue:
                        // Take true path if count == 0
                        this.helper.TestAndBranch(0, this.iterCurr.LabelBranch, OpCodes.Beq);
                        break;

                    default:
                        Debug.Assert(this.iterCurr.CurrentBranchingContext == BranchingContext.None);

                        // if (count == 0) goto LabelTrue;
                        lblTrue = this.helper.DefineLabel();
                        this.helper.Emit(OpCodes.Brfalse_S, lblTrue);

                        // Convert branch targets into push of true/false
                        this.helper.ConvBranchToBool(lblTrue, true);
                        break;
                }
            }
            else {
                Label lblOnEnd = this.helper.DefineLabel();
                IteratorDescriptor iterParent = this.iterCurr;

                // Forward any LabelOnEnd jumps to LabelBranch if BranchingContext.OnTrue
                if (iterParent.CurrentBranchingContext == BranchingContext.OnTrue)
                    StartNestedIterator(ndIsEmpty.Child, this.iterCurr.LabelBranch);
                else
                    StartNestedIterator(ndIsEmpty.Child, lblOnEnd);

                Visit(ndIsEmpty.Child);

                // Pop value of IsEmpty expression from the stack if necessary
                this.iterCurr.EnsureNoCache();
                this.iterCurr.DiscardStack();

                switch (iterParent.CurrentBranchingContext) {
                    case BranchingContext.OnFalse:
                        // Reverse polarity of iterator
                        this.helper.EmitUnconditionalBranch(OpCodes.Br, iterParent.LabelBranch);
                        this.helper.MarkLabel(lblOnEnd);
                        break;

                    case BranchingContext.OnTrue:
                        // Nothing to do
                        break;

                    case BranchingContext.None:
                        // Convert branch targets into push of true/false
                        this.helper.ConvBranchToBool(lblOnEnd, true);
                        break;
                }

                // End nested iterator
                EndNestedIterator(ndIsEmpty.Child);
            }

            if (this.iterCurr.IsBranching)
                this.iterCurr.Storage = StorageDescriptor.None();
            else
                this.iterCurr.Storage = StorageDescriptor.Stack(typeof(bool), false);

            return ndIsEmpty;
        }

        /// <summary>
        /// Generate code for QilNodeType.XPathNodeValue.
        /// </summary>
        protected override QilNode VisitXPathNodeValue(QilUnary ndVal) {
            Label lblOnEnd, lblDone;
            Debug.Assert(ndVal.Child.XmlType.IsNode, "XPathNodeValue node may only be applied to a sequence of Nodes.");

            // If the expression is a singleton,
            if (ndVal.Child.XmlType.IsSingleton) {
                // Then generate code to push expresion result onto the stack
                NestedVisitEnsureStack(ndVal.Child, typeof(XPathNavigator), false);

                // navigator.Value;
                this.helper.Call(XmlILMethods.Value);
            }
            else {
                lblOnEnd = this.helper.DefineLabel();

                // Construct nested iterator and iterate over results
                StartNestedIterator(ndVal.Child, lblOnEnd);
                Visit(ndVal.Child);
                this.iterCurr.EnsureStackNoCache();

                // navigator.Value;
                this.helper.Call(XmlILMethods.Value);

                // Handle empty sequence by pushing empty string onto the stack
                lblDone = this.helper.DefineLabel();
                this.helper.EmitUnconditionalBranch(OpCodes.Br, lblDone);
                this.helper.MarkLabel(lblOnEnd);
                this.helper.Emit(OpCodes.Ldstr, "");
                this.helper.MarkLabel(lblDone);

                // End nested iterator
                EndNestedIterator(ndVal.Child);
            }

            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(string), false);

            return ndVal;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.XPathFollowing.
        /// </summary>
        protected override QilNode VisitXPathFollowing(QilUnary ndFoll) {
            CreateFilteredIterator(ndFoll.Child, "$$$iterFoll", typeof(XPathFollowingIterator), XmlILMethods.XPFollCreate, XmlILMethods.XPFollNext,
                                   XmlNodeKindFlags.Any, null, TriState.Unknown, null);
            return ndFoll;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.XPathPreceding.
        /// </summary>
        protected override QilNode VisitXPathPreceding(QilUnary ndPrec) {
            CreateFilteredIterator(ndPrec.Child, "$$$iterPrec", typeof(XPathPrecedingIterator), XmlILMethods.XPPrecCreate, XmlILMethods.XPPrecNext,
                                   XmlNodeKindFlags.Any, null, TriState.Unknown, null);
            return ndPrec;
        }

        /// <summary>
        /// Find physical query plan for QilNodeType.XPathNamespace.
        /// </summary>
        protected override QilNode VisitXPathNamespace(QilUnary ndNmsp) {
            CreateSimpleIterator(ndNmsp.Child, "$$$iterNmsp", typeof(NamespaceIterator), XmlILMethods.NmspCreate, XmlILMethods.NmspNext);
            return ndNmsp;
        }

        /// <summary>
        /// Generate code for QilNodeType.XsltGenerateId.
        /// </summary>
        protected override QilNode VisitXsltGenerateId(QilUnary ndGenId) {
            Label lblOnEnd, lblDone;

            this.helper.LoadQueryRuntime();

            // If the expression is a singleton,
            if (ndGenId.Child.XmlType.IsSingleton) {
                // Then generate code to push expresion result onto the stack
                NestedVisitEnsureStack(ndGenId.Child, typeof(XPathNavigator), false);

                // runtime.GenerateId(value);
                this.helper.Call(XmlILMethods.GenId);
            }
            else {
                lblOnEnd = this.helper.DefineLabel();

                // Construct nested iterator and iterate over results
                StartNestedIterator(ndGenId.Child, lblOnEnd);
                Visit(ndGenId.Child);
                this.iterCurr.EnsureStackNoCache();
                this.iterCurr.EnsureItemStorageType(ndGenId.Child.XmlType, typeof(XPathNavigator));

                // runtime.GenerateId(value);
                this.helper.Call(XmlILMethods.GenId);

                // Handle empty sequence by pushing empty string onto the stack
                lblDone = this.helper.DefineLabel();
                this.helper.EmitUnconditionalBranch(OpCodes.Br, lblDone);
                this.helper.MarkLabel(lblOnEnd);
                this.helper.Emit(OpCodes.Pop);
                this.helper.Emit(OpCodes.Ldstr, "");
                this.helper.MarkLabel(lblDone);

                // End nested iterator
                EndNestedIterator(ndGenId.Child);
            }

            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(string), false);

            return ndGenId;
        }

        /// <summary>
        /// Generate code for for QilNodeType.XsltInvokeLateBound.
        /// </summary>
        protected override QilNode VisitXsltInvokeLateBound(QilInvokeLateBound ndInvoke) {
            LocalBuilder locArgs = this.helper.DeclareLocal("$$$args", typeof(IList<XPathItem>[]));
            QilName ndName = (QilName) ndInvoke.Name;
            Debug.Assert(XmlILConstructInfo.Read(ndInvoke).ConstructMethod != XmlILConstructMethod.Writer);

            // runtime.ExternalContext.InvokeXsltLateBoundFunction(name, ns, args);
            this.helper.LoadQueryContext();
            this.helper.Emit(OpCodes.Ldstr, ndName.LocalName);
            this.helper.Emit(OpCodes.Ldstr, ndName.NamespaceUri);

            // args = new IList<XPathItem>[argCount];
            this.helper.LoadInteger(ndInvoke.Arguments.Count);
            this.helper.Emit(OpCodes.Newarr, typeof(IList<XPathItem>));
            this.helper.Emit(OpCodes.Stloc, locArgs);

            for (int iArg = 0; iArg < ndInvoke.Arguments.Count; iArg++) {
                QilNode ndArg = ndInvoke.Arguments[iArg];

                // args[0] = arg0;
                // ...
                // args[N] = argN;
                this.helper.Emit(OpCodes.Ldloc, locArgs);
                this.helper.LoadInteger(iArg);
                this.helper.Emit(OpCodes.Ldelema, typeof(IList<XPathItem>));

                NestedVisitEnsureCache(ndArg, typeof(XPathItem));
                this.iterCurr.EnsureStack();

                this.helper.Emit(OpCodes.Stobj, typeof(IList<XPathItem>));
            }

            this.helper.Emit(OpCodes.Ldloc, locArgs);

            this.helper.Call(XmlILMethods.InvokeXsltLate);

            // Returned item sequence is on the stack
            this.iterCurr.Storage = StorageDescriptor.Stack(typeof(XPathItem), true);

            return ndInvoke;
        }

        /// <summary>
        /// Generate code for for QilNodeType.XsltInvokeEarlyBound.
        /// </summary>
        protected override QilNode VisitXsltInvokeEarlyBound(QilInvokeEarlyBound ndInvoke) {
            QilName ndName = ndInvoke.Name;
            XmlExtensionFunction extFunc;
            Type clrTypeRetSrc, clrTypeRetDst;

            // Retrieve metadata from the extension function
            extFunc = new XmlExtensionFunction(ndName.LocalName, ndName.NamespaceUri, ndInvoke.ClrMethod);
            clrTypeRetSrc = extFunc.ClrReturnType;
            clrTypeRetDst = GetStorageType(ndInvoke);

            // Prepare to call runtime.ChangeTypeXsltResult
            if (clrTypeRetSrc != clrTypeRetDst && !ndInvoke.XmlType.IsEmpty) {
                this.helper.LoadQueryRuntime();
                this.helper.LoadInteger(this.helper.StaticData.DeclareXmlType(ndInvoke.XmlType));
            }

            // If this is not a static method, then get the instance object
            if (!extFunc.Method.IsStatic) {
                // Special-case the XsltLibrary object
                if (ndName.NamespaceUri.Length == 0)
                    this.helper.LoadXsltLibrary();
                else
                    this.helper.CallGetEarlyBoundObject(this.helper.StaticData.DeclareEarlyBound(ndName.NamespaceUri, extFunc.Method.DeclaringType), extFunc.Method.DeclaringType);
            }

            // Generate code to push each Invoke argument onto the stack
            for (int iArg = 0; iArg < ndInvoke.Arguments.Count; iArg++) {
                QilNode ndActualArg;
                XmlQueryType xmlTypeFormalArg;
                Type clrTypeActualArg, clrTypeFormalArg;

                ndActualArg = ndInvoke.Arguments[iArg];

                // Infer Xml type and Clr type of formal argument
                xmlTypeFormalArg = extFunc.GetXmlArgumentType(iArg);
                clrTypeFormalArg = extFunc.GetClrArgumentType(iArg);

                Debug.Assert(ndActualArg.XmlType.IsSubtypeOf(xmlTypeFormalArg), "Xml type of actual arg must be a subtype of the Xml type of the formal arg");

                // Use different conversion rules for internal Xslt libraries.  If the actual argument is
                // stored using Clr type T, then library must use type T, XPathItem, IList<T>, or IList<XPathItem>.
                // If the actual argument is stored using Clr type IList<T>, then library must use type
                // IList<T> or IList<XPathItem>.  This is to ensure that there will not be unnecessary
                // conversions that take place when calling into an internal library.
                if (ndName.NamespaceUri.Length == 0) {
                    Type itemType = GetItemStorageType(ndActualArg);

                    if (clrTypeFormalArg == XmlILMethods.StorageMethods[itemType].IListType) {
                        // Formal type is IList<T>
                        NestedVisitEnsureStack(ndActualArg, itemType, true);
                    }
                    else if (clrTypeFormalArg == XmlILMethods.StorageMethods[typeof(XPathItem)].IListType) {
                        // Formal type is IList<XPathItem>
                        NestedVisitEnsureStack(ndActualArg, typeof(XPathItem), true);
                    }
                    else if ((ndActualArg.XmlType.IsSingleton && clrTypeFormalArg == itemType) || ndActualArg.XmlType.TypeCode == XmlTypeCode.None) {
                        // Formal type is T
                        NestedVisitEnsureStack(ndActualArg, clrTypeFormalArg, false);
                    }
                    else if (ndActualArg.XmlType.IsSingleton && clrTypeFormalArg == typeof(XPathItem)) {
                        // Formal type is XPathItem
                        NestedVisitEnsureStack(ndActualArg, typeof(XPathItem), false);
                    }
                    else
                        Debug.Fail("Internal Xslt library may not use parameters of type " + clrTypeFormalArg);
                }
                else {
                    // There is an implicit upcast to the Xml type of the formal argument.  This can change the Clr storage type.
                    clrTypeActualArg = GetStorageType(xmlTypeFormalArg);

                    // If the formal Clr type is typeof(object) or if it is not a supertype of the actual Clr type, then call ChangeTypeXsltArgument
                    if (xmlTypeFormalArg.TypeCode == XmlTypeCode.Item || !clrTypeFormalArg.IsAssignableFrom(clrTypeActualArg)) {
                        // (clrTypeFormalArg) runtime.ChangeTypeXsltArgument(xmlTypeFormalArg, (object) value, clrTypeFormalArg);
                        this.helper.LoadQueryRuntime();
                        this.helper.LoadInteger(this.helper.StaticData.DeclareXmlType(xmlTypeFormalArg));
                        NestedVisitEnsureStack(ndActualArg, GetItemStorageType(xmlTypeFormalArg), !xmlTypeFormalArg.IsSingleton);
                        this.helper.TreatAs(clrTypeActualArg, typeof(object));
                        this.helper.LoadType(clrTypeFormalArg);
                        this.helper.Call(XmlILMethods.ChangeTypeXsltArg);
                        this.helper.TreatAs(typeof(object), clrTypeFormalArg);
                    }
                    else {
                        NestedVisitEnsureStack(ndActualArg, GetItemStorageType(xmlTypeFormalArg), !xmlTypeFormalArg.IsSingleton);
                    }
                }
            }

            // Invoke the target method
            this.helper.Call(extFunc.Method);

            // Return value is on the stack; convert it to canonical ILGen storage type
            if (ndInvoke.XmlType.IsEmpty) {
                this.helper.Emit(OpCodes.Ldsfld, XmlILMethods.StorageMethods[typeof(XPathItem)].SeqEmpty);
            }
            else if (clrTypeRetSrc != clrTypeRetDst) {
                // (T) runtime.ChangeTypeXsltResult(idxType, (object) value);
                this.helper.TreatAs(clrTypeRetSrc, typeof(object));
                this.helper.Call(XmlILMethods.ChangeTypeXsltResult);
                this.helper.TreatAs(typeof(object), clrTypeRetDst);
            }
            else if (ndName.NamespaceUri.Length != 0 && !clrTypeRetSrc.IsValueType){
                // Check for null if a user-defined extension function returns a reference type
                Label lblSkip = this.helper.DefineLabel();
                this.helper.Emit(OpCodes.Dup);
                this.helper.Emit(OpCodes.Brtrue, lblSkip);
                this.helper.LoadQueryRuntime();
                this.helper.Emit(OpCodes.Ldstr, Res.GetString(Res.Xslt_ItemNull));
                this.helper.Call(XmlILMethods.ThrowException);
                this.helper.MarkLabel(lblSkip);
            }

            this.iterCurr.Storage = StorageDescriptor.Stack(GetItemStorageType(ndInvoke), !ndInvoke.XmlType.IsSingleton);

            return ndInvoke;
        }

        /// <summary>
        /// Generate code for QilNodeType.XsltCopy.
        /// </summary>
        protected override QilNode VisitXsltCopy(QilBinary ndCopy) {
            Label lblSkipContent = this.helper.DefineLabel();
            Debug.Assert(XmlILConstructInfo.Read(ndCopy).PushToWriterFirst);

            // if (!xwrtChk.StartCopyChk(navCopy)) goto LabelSkipContent;
            this.helper.LoadQueryOutput();

            NestedVisitEnsureStack(ndCopy.Left);
            Debug.Assert(ndCopy.Left.XmlType.IsNode);

            this.helper.Call(XmlILMethods.StartCopy);
            this.helper.Emit(OpCodes.Brfalse, lblSkipContent);

            // Recursively construct content
            NestedVisit(ndCopy.Right);

            // xwrtChk.EndCopyChk(navCopy);
            this.helper.LoadQueryOutput();

            NestedVisitEnsureStack(ndCopy.Left);
            Debug.Assert(ndCopy.Left.XmlType.IsNode);

            this.helper.Call(XmlILMethods.EndCopy);

            // LabelSkipContent:
            this.helper.MarkLabel(lblSkipContent);

            this.iterCurr.Storage = StorageDescriptor.None();
            return ndCopy;
        }

        /// <summary>
        /// Generate code for QilNodeType.XsltCopyOf.
        /// </summary>
        protected override QilNode VisitXsltCopyOf(QilUnary ndCopyOf) {
            Debug.Assert(XmlILConstructInfo.Read(ndCopyOf).PushToWriterFirst, "XsltCopyOf should always be pushed to writer.");

            this.helper.LoadQueryOutput();

            // XmlQueryOutput.XsltCopyOf(navigator);
            NestedVisitEnsureStack(ndCopyOf.Child);
            this.helper.Call(XmlILMethods.CopyOf);

            this.iterCurr.Storage = StorageDescriptor.None();
            return ndCopyOf;
        }

        /// <summary>
        /// Generate code for QilNodeType.XsltConvert.
        /// </summary>
        protected override QilNode VisitXsltConvert(QilTargetType ndConv) {
            XmlQueryType typSrc, typDst;
            MethodInfo meth;

            typSrc = ndConv.Source.XmlType;
            typDst = ndConv.TargetType;

            if (GetXsltConvertMethod(typSrc, typDst, out meth)) {
                NestedVisitEnsureStack(ndConv.Source);
            }
            else {
                // If a conversion could not be found, then convert the source expression to item or item* and try again
                NestedVisitEnsureStack(ndConv.Source, typeof(XPathItem), !typSrc.IsSingleton);
                if (!GetXsltConvertMethod(typSrc.IsSingleton ? TypeFactory.Item : TypeFactory.ItemS, typDst, out meth))
                    Debug.Fail("Conversion from " + ndConv.Source.XmlType + " to " + ndConv.TargetType + " is not supported.");
            }

            // XsltConvert.XXXToYYY(value);
            if (meth != null)
                this.helper.Call(meth);

            this.iterCurr.Storage = StorageDescriptor.Stack(GetItemStorageType(typDst), !typDst.IsSingleton);
            return ndConv;
        }

        /// <summary>
        /// Get the XsltConvert method that converts from "typSrc" to "typDst".  Return false if no
        /// such method exists.  This conversion matrix should match the one in XsltConvert.ExternalValueToExternalValue.
        /// </summary>
        private bool GetXsltConvertMethod(XmlQueryType typSrc, XmlQueryType typDst, out MethodInfo meth) {
            meth = null;

            // Note, Ref.Equals is OK to use here, since we will always fall back to Item or Item* in the
            // case where the source or destination types do not match the static types exposed on the
            // XmlQueryTypeFactory.  This is bad for perf if it accidentally occurs, but the results
            // should still be correct.

            // => xs:boolean
            if ((object) typDst == (object) TypeFactory.BooleanX) {
                if ((object) typSrc == (object) TypeFactory.Item)               meth = XmlILMethods.ItemToBool;
                else if ((object) typSrc == (object) TypeFactory.ItemS)         meth = XmlILMethods.ItemsToBool;
            }
            // => xs:dateTime
            else if ((object) typDst == (object) TypeFactory.DateTimeX) {
                if ((object) typSrc == (object) TypeFactory.StringX)            meth = XmlILMethods.StrToDT;
            }
            // => xs:decimal
            else if ((object) typDst == (object) TypeFactory.DecimalX) {
                if ((object) typSrc == (object) TypeFactory.DoubleX)            meth = XmlILMethods.DblToDec;
            }
            // => xs:double
            else if ((object) typDst == (object) TypeFactory.DoubleX) {
                if ((object) typSrc == (object) TypeFactory.DecimalX)           meth = XmlILMethods.DecToDbl;
                else if ((object) typSrc == (object) TypeFactory.IntX)          meth = XmlILMethods.IntToDbl;
                else if ((object) typSrc == (object) TypeFactory.Item)          meth = XmlILMethods.ItemToDbl;
                else if ((object) typSrc == (object) TypeFactory.ItemS)         meth = XmlILMethods.ItemsToDbl;
                else if ((object) typSrc == (object) TypeFactory.LongX)         meth = XmlILMethods.LngToDbl;
                else if ((object) typSrc == (object) TypeFactory.StringX)       meth = XmlILMethods.StrToDbl;
            }
            // => xs:int
            else if ((object) typDst == (object) TypeFactory.IntX) {
                if ((object) typSrc == (object) TypeFactory.DoubleX)            meth = XmlILMethods.DblToInt;
            }
            // => xs:long
            else if ((object) typDst == (object) TypeFactory.LongX) {
                if ((object) typSrc == (object) TypeFactory.DoubleX)            meth = XmlILMethods.DblToLng;
            }
            // => node
            else if ((object) typDst == (object) TypeFactory.NodeNotRtf) {
                if ((object) typSrc == (object) TypeFactory.Item)               meth = XmlILMethods.ItemToNode;
                else if ((object) typSrc == (object) TypeFactory.ItemS)         meth = XmlILMethods.ItemsToNode;
            }
            // => node*
            else if ((object) typDst == (object) TypeFactory.NodeSDod ||
                     (object) typDst == (object) TypeFactory.NodeNotRtfS) {
                if ((object) typSrc == (object) TypeFactory.Item)               meth = XmlILMethods.ItemToNodes;
                else if ((object) typSrc == (object) TypeFactory.ItemS)         meth = XmlILMethods.ItemsToNodes;
            }
            // => xs:string
            else if ((object) typDst == (object) TypeFactory.StringX) {
                if ((object) typSrc == (object) TypeFactory.DateTimeX)          meth = XmlILMethods.DTToStr;
                else if ((object) typSrc == (object) TypeFactory.DoubleX)       meth = XmlILMethods.DblToStr;
                else if ((object) typSrc == (object) TypeFactory.Item)          meth = XmlILMethods.ItemToStr;
                else if ((object) typSrc == (object) TypeFactory.ItemS)         meth = XmlILMethods.ItemsToStr;
            }

            return meth != null;
        }


        //-----------------------------------------------
        // Helper methods
        //-----------------------------------------------

        /// <summary>
        /// Ensure that the "locNav" navigator is positioned to the context node "ndCtxt".
        /// </summary>
        private void SyncToNavigator(LocalBuilder locNav, QilNode ndCtxt) {
            this.helper.Emit(OpCodes.Ldloc, locNav);
            NestedVisitEnsureStack(ndCtxt);
            this.helper.CallSyncToNavigator();
            this.helper.Emit(OpCodes.Stloc, locNav);
        }

        /// <summary>
        /// Generate boiler-plate code to create a simple Xml iterator.
        /// </summary>
        /// <remarks>
        ///     Iterator iter;
        ///     iter.Create(navCtxt);
        /// LabelNext:
        ///     if (!iter.MoveNext())
        ///         goto LabelNextCtxt;
        /// </remarks>
        private void CreateSimpleIterator(QilNode ndCtxt, string iterName, Type iterType, MethodInfo methCreate, MethodInfo methNext) {
            // Iterator iter;
            LocalBuilder locIter = this.helper.DeclareLocal(iterName, iterType);

            // iter.Create(navCtxt);
            this.helper.Emit(OpCodes.Ldloca, locIter);
            NestedVisitEnsureStack(ndCtxt);
            this.helper.Call(methCreate);

            GenerateSimpleIterator(typeof(XPathNavigator), locIter, methNext);
        }

        /// <summary>
        /// Generate boiler-plate code to create an Xml iterator that uses an XmlNavigatorFilter to filter items.
        /// </summary>
        /// <remarks>
        ///     Iterator iter;
        ///     iter.Create(navCtxt, filter [, orSelf] [, navEnd]);
        /// LabelNext:
        ///     if (!iter.MoveNext())
        ///         goto LabelNextCtxt;
        /// </remarks>
        private void CreateFilteredIterator(QilNode ndCtxt, string iterName, Type iterType, MethodInfo methCreate, MethodInfo methNext,
                                                XmlNodeKindFlags kinds, QilName ndName, TriState orSelf, QilNode ndEnd) {
            // Iterator iter;
            LocalBuilder locIter = this.helper.DeclareLocal(iterName, iterType);

            // iter.Create(navCtxt, filter [, orSelf], [, navEnd]);
            this.helper.Emit(OpCodes.Ldloca, locIter);
            NestedVisitEnsureStack(ndCtxt);
            LoadSelectFilter(kinds, ndName);
            if (orSelf != TriState.Unknown)
                this.helper.LoadBoolean(orSelf == TriState.True);
            if (ndEnd != null)
                NestedVisitEnsureStack(ndEnd);
            this.helper.Call(methCreate);

            GenerateSimpleIterator(typeof(XPathNavigator), locIter, methNext);
        }

        /// <summary>
        /// Generate boiler-plate code to create an Xml iterator that controls a nested iterator.
        /// </summary>
        /// <remarks>
        ///     Iterator iter;
        ///     iter.Create(filter [, orSelf]);
        ///         ...nested iterator...
        ///     navInput = nestedNested;
        ///     goto LabelCall;
        /// LabelNext:
        ///     navInput = null;
        /// LabelCall:
        ///     switch (iter.MoveNext(navInput)) {
        ///         case IteratorState.NoMoreNodes: goto LabelNextCtxt;
        ///         case IteratorState.NextInputNode: goto LabelNextNested;
        ///     }
        /// </remarks>
        private void CreateContainerIterator(QilUnary ndDod, string iterName, Type iterType, MethodInfo methCreate, MethodInfo methNext,
                                                   XmlNodeKindFlags kinds, QilName ndName, TriState orSelf) {
            // Iterator iter;
            LocalBuilder locIter = this.helper.DeclareLocal(iterName, iterType);
            Label lblOnEndNested;
            QilLoop ndLoop = (QilLoop) ndDod.Child;
            Debug.Assert(ndDod.NodeType == QilNodeType.DocOrderDistinct && ndLoop != null);

            // iter.Create(filter [, orSelf]);
            this.helper.Emit(OpCodes.Ldloca, locIter);
            LoadSelectFilter(kinds, ndName);
            if (orSelf != TriState.Unknown)
                this.helper.LoadBoolean(orSelf == TriState.True);
            this.helper.Call(methCreate);

            // Generate nested iterator (branch to lblOnEndNested when iteration is complete)
            lblOnEndNested = this.helper.DefineLabel();
            StartNestedIterator(ndLoop, lblOnEndNested);
            StartBinding(ndLoop.Variable);
            EndBinding(ndLoop.Variable);
            EndNestedIterator(ndLoop.Variable);
            this.iterCurr.Storage = this.iterNested.Storage;

            GenerateContainerIterator(ndDod, locIter, lblOnEndNested, methNext, typeof(XPathNavigator));
        }

        /// <summary>
        /// Generate boiler-plate code that calls MoveNext on a simple Xml iterator.  Iterator should have already been
        /// created by calling code.
        /// </summary>
        /// <remarks>
        ///     ...
        /// LabelNext:
        ///     if (!iter.MoveNext())
        ///         goto LabelNextCtxt;
        /// </remarks>
        private void GenerateSimpleIterator(Type itemStorageType, LocalBuilder locIter, MethodInfo methNext) {
            Label lblNext;

            // LabelNext:
            lblNext = this.helper.DefineLabel();
            this.helper.MarkLabel(lblNext);

            // if (!iter.MoveNext()) goto LabelNextCtxt;
            this.helper.Emit(OpCodes.Ldloca, locIter);
            this.helper.Call(methNext);
            this.helper.Emit(OpCodes.Brfalse, this.iterCurr.GetLabelNext());

            this.iterCurr.SetIterator(lblNext, StorageDescriptor.Current(locIter, itemStorageType));
        }

        /// <summary>
        /// Generate boiler-plate code that calls MoveNext on an Xml iterator that controls a nested iterator.  Iterator should
        /// have already been created by calling code.
        /// </summary>
        /// <remarks>
        ///     ...
        ///     goto LabelCall;
        /// LabelNext:
        ///     navCtxt = null;
        /// LabelCall:
        ///     switch (iter.MoveNext(navCtxt)) {
        ///         case IteratorState.NoMoreNodes: goto LabelNextCtxt;
        ///         case IteratorState.NextInputNode: goto LabelNextNested;
        ///     }
        /// </remarks>
        private void GenerateContainerIterator(QilNode nd, LocalBuilder locIter, Label lblOnEndNested,
                                                       MethodInfo methNext, Type itemStorageType) {
            Label lblCall;

            // Define labels that will be used
            lblCall = this.helper.DefineLabel();

            // iter.MoveNext(input);
            // goto LabelCall;
            this.iterCurr.EnsureNoStackNoCache(nd.XmlType.IsNode ? "$$$navInput" : "$$$itemInput");
            this.helper.Emit(OpCodes.Ldloca, locIter);
            this.iterCurr.PushValue();
            this.helper.EmitUnconditionalBranch(OpCodes.Br, lblCall);

            // LabelNext:
            // iterSet.MoveNext(null);
            this.helper.MarkLabel(lblOnEndNested);
            this.helper.Emit(OpCodes.Ldloca, locIter);
            this.helper.Emit(OpCodes.Ldnull);

            // LabelCall:
            // result = iter.MoveNext(input);
            this.helper.MarkLabel(lblCall);
            this.helper.Call(methNext);

            // If this iterator always returns a single node, then NoMoreNodes will never be returned
            if (nd.XmlType.IsSingleton) {
                // if (result == IteratorResult.NeedInputNode) goto LabelNextInput;
                this.helper.LoadInteger((int) IteratorResult.NeedInputNode);
                this.helper.Emit(OpCodes.Beq, this.iterNested.GetLabelNext());

                this.iterCurr.Storage = StorageDescriptor.Current(locIter, itemStorageType);
            }
            else {
                // switch (iter.MoveNext(input)) {
                //      case IteratorResult.NoMoreNodes: goto LabelNextCtxt;
                //      case IteratorResult.NeedInputNode: goto LabelNextInput;
                // }
                this.helper.Emit(OpCodes.Switch, new Label[] {this.iterCurr.GetLabelNext(), this.iterNested.GetLabelNext()});

                this.iterCurr.SetIterator(lblOnEndNested, StorageDescriptor.Current(locIter, itemStorageType));
            }
        }

        /// <summary>
        /// Load XmlQueryOutput, load a name (computed or literal) and load an index to an Xml schema type.
        /// Return an enumeration that specifies what kind of name was loaded.
        /// </summary>
        private GenerateNameType LoadNameAndType(XPathNodeType nodeType, QilNode ndName, bool isStart, bool callChk) {
            QilName ndLiteralName;
            string prefix, localName, ns;
            GenerateNameType nameType;
            Debug.Assert(ndName.XmlType.TypeCode == XmlTypeCode.QName, "Element or attribute name must have QName type.");

            this.helper.LoadQueryOutput();

            // 0. Default is to pop names off stack
            nameType = GenerateNameType.StackName;

            // 1. Literal names
            if (ndName.NodeType == QilNodeType.LiteralQName) {
                // If checks need to be made on End construction, then always pop names from stack
                if (isStart || !callChk) {
                    ndLiteralName = ndName as QilName;
                    prefix = ndLiteralName.Prefix;
                    localName = ndLiteralName.LocalName;
                    ns = ndLiteralName.NamespaceUri;

                    // Check local name, namespace parts in debug code
                    Debug.Assert(ValidateNames.ValidateName(prefix, localName, ns, nodeType, ValidateNames.Flags.AllExceptPrefixMapping));

                    // If the namespace is empty,
                    if (ndLiteralName.NamespaceUri.Length == 0) {
                        // Then always call method on XmlQueryOutput
                        this.helper.Emit(OpCodes.Ldstr, ndLiteralName.LocalName);
                        return GenerateNameType.LiteralLocalName;
                    }

                    // If prefix is not valid for the node type,
                    if (!ValidateNames.ValidateName(prefix, localName, ns, nodeType, ValidateNames.Flags.CheckPrefixMapping)) {
                        // Then construct a new prefix at run-time
                        if (isStart) {
                            this.helper.Emit(OpCodes.Ldstr, localName);
                            this.helper.Emit(OpCodes.Ldstr, ns);
                            this.helper.Construct(XmlILConstructors.QName);

                            nameType = GenerateNameType.QName;
                        }
                    }
                    else {
                        // Push string parts
                        this.helper.Emit(OpCodes.Ldstr, prefix);
                        this.helper.Emit(OpCodes.Ldstr, localName);
                        this.helper.Emit(OpCodes.Ldstr, ns);

                        nameType = GenerateNameType.LiteralName;
                    }
                }
            }
            else {
                if (isStart) {
                    // 2. Copied names
                    if (ndName.NodeType == QilNodeType.NameOf) {
                        // Preserve prefix of source node, so just push navigator onto stack
                        NestedVisitEnsureStack((ndName as QilUnary).Child);
                        nameType = GenerateNameType.CopiedName;
                    }
                    // 3. Parsed tag names (foo:bar)
                    else if (ndName.NodeType == QilNodeType.StrParseQName) {
                        // Preserve prefix from parsed tag name
                        VisitStrParseQName(ndName as QilBinary, true);

                        // Type of name depends upon data-type of name argument
                        if ((ndName as QilBinary).Right.XmlType.TypeCode == XmlTypeCode.String)
                            nameType = GenerateNameType.TagNameAndNamespace;
                        else
                            nameType = GenerateNameType.TagNameAndMappings;
                    }
                    // 4. Other computed qnames
                    else {
                        // Push XmlQualifiedName onto the stack
                        NestedVisitEnsureStack(ndName);
                        nameType = GenerateNameType.QName;
                    }
                }
            }

            return nameType;
        }

        /// <summary>
        /// If the first argument is a constant value that evaluates to zero, then a more optimal instruction sequence
        /// can be generated that does not have to push the zero onto the stack.  Instead, a Brfalse or Brtrue instruction
        /// can be used.
        /// </summary>
        private bool TryZeroCompare(QilNodeType relOp, QilNode ndFirst, QilNode ndSecond) {
            Debug.Assert(relOp == QilNodeType.Eq || relOp == QilNodeType.Ne);

            switch (ndFirst.NodeType) {
                case QilNodeType.LiteralInt64:
                    if ((int) (QilLiteral) ndFirst != 0) return false;
                    break;

                case QilNodeType.LiteralInt32:
                    if ((int) (QilLiteral) ndFirst != 0) return false;
                    break;

                case QilNodeType.False:
                    break;

                case QilNodeType.True:
                    // Inverse of QilNodeType.False
                    relOp = (relOp == QilNodeType.Eq) ? QilNodeType.Ne : QilNodeType.Eq;
                    break;

                default:
                    return false;
            }

            // Generate code to push second argument on stack
            NestedVisitEnsureStack(ndSecond);

            // Generate comparison code -- op == 0 or op != 0
            ZeroCompare(relOp, ndSecond.XmlType.TypeCode == XmlTypeCode.Boolean);

            return true;
        }

        /// <summary>
        /// If the comparison involves a qname, then perform comparison using atoms and return true.
        /// Otherwise, return false (caller will perform comparison).
        /// </summary>
        private bool TryNameCompare(QilNodeType relOp, QilNode ndFirst, QilNode ndSecond) {
            Debug.Assert(relOp == QilNodeType.Eq || relOp == QilNodeType.Ne);

            if (ndFirst.NodeType == QilNodeType.NameOf) {
                switch (ndSecond.NodeType) {
                    case QilNodeType.NameOf:
                    case QilNodeType.LiteralQName: {
                        this.helper.LoadQueryRuntime();

                        // Push left navigator onto the stack
                        NestedVisitEnsureStack((ndFirst as QilUnary).Child);

                        // Push the local name and namespace uri of the right argument onto the stack
                        if (ndSecond.NodeType == QilNodeType.LiteralQName) {
                            QilName ndName = ndSecond as QilName;
                            this.helper.LoadInteger(this.helper.StaticData.DeclareName(ndName.LocalName));
                            this.helper.LoadInteger(this.helper.StaticData.DeclareName(ndName.NamespaceUri));

                            // push runtime.IsQNameEqual(navigator, localName, namespaceUri)
                            this.helper.Call(XmlILMethods.QNameEqualLit);
                        }
                        else {
                            // Generate code to locate the navigator argument of NameOf operator
                            Debug.Assert(ndSecond.NodeType == QilNodeType.NameOf);
                            NestedVisitEnsureStack(ndSecond);

                            // push runtime.IsQNameEqual(nav1, nav2)
                            this.helper.Call(XmlILMethods.QNameEqualNav);
                        }

                        // Branch based on boolean result or push boolean value
                        ZeroCompare((relOp == QilNodeType.Eq) ? QilNodeType.Ne : QilNodeType.Eq, true);
                        return true;
                    }
                }
            }

            // Caller must perform comparison
            return false;
        }

        /// <summary>
        /// For QilExpression types that map directly to CLR primitive types, the built-in CLR comparison operators can
        /// be used to perform the specified relational operation.
        /// </summary>
        private void ClrCompare(QilNodeType relOp, XmlTypeCode code) {
            OpCode opcode;
            Label lblTrue;

            switch (this.iterCurr.CurrentBranchingContext) {
                case BranchingContext.OnFalse:
                    // Reverse the comparison operator
                    // Use Bxx_Un OpCodes to handle NaN case for double and single types
                    if (code == XmlTypeCode.Double || code == XmlTypeCode.Float) {
                        switch (relOp) {
                            case QilNodeType.Gt: opcode = OpCodes.Ble_Un; break;
                            case QilNodeType.Ge: opcode = OpCodes.Blt_Un; break;
                            case QilNodeType.Lt: opcode = OpCodes.Bge_Un; break;
                            case QilNodeType.Le: opcode = OpCodes.Bgt_Un; break;
                            case QilNodeType.Eq: opcode = OpCodes.Bne_Un; break;
                            case QilNodeType.Ne: opcode = OpCodes.Beq; break;
                            default: Debug.Assert(false); opcode = OpCodes.Nop; break;
                        }
                    }
                    else {
                        switch (relOp)
                        {
                            case QilNodeType.Gt: opcode = OpCodes.Ble; break;
                            case QilNodeType.Ge: opcode = OpCodes.Blt; break;
                            case QilNodeType.Lt: opcode = OpCodes.Bge; break;
                            case QilNodeType.Le: opcode = OpCodes.Bgt; break;
                            case QilNodeType.Eq: opcode = OpCodes.Bne_Un; break;
                            case QilNodeType.Ne: opcode = OpCodes.Beq; break;
                            default: Debug.Assert(false); opcode = OpCodes.Nop; break;
                        }
                    }
                    this.helper.Emit(opcode, this.iterCurr.LabelBranch);
                    this.iterCurr.Storage = StorageDescriptor.None();
                    break;

                case BranchingContext.OnTrue:
                    switch (relOp) {
                        case QilNodeType.Gt: opcode = OpCodes.Bgt; break;
                        case QilNodeType.Ge: opcode = OpCodes.Bge; break;
                        case QilNodeType.Lt: opcode = OpCodes.Blt; break;
                        case QilNodeType.Le: opcode = OpCodes.Ble; break;
                        case QilNodeType.Eq: opcode = OpCodes.Beq; break;
                        case QilNodeType.Ne: opcode = OpCodes.Bne_Un; break;
                        default: Debug.Assert(false); opcode = OpCodes.Nop; break;
                    }
                    this.helper.Emit(opcode, this.iterCurr.LabelBranch);
                    this.iterCurr.Storage = StorageDescriptor.None();
                    break;

                default:
                    Debug.Assert(this.iterCurr.CurrentBranchingContext == BranchingContext.None);
                    switch (relOp) {
                        case QilNodeType.Gt: this.helper.Emit(OpCodes.Cgt); break;
                        case QilNodeType.Lt: this.helper.Emit(OpCodes.Clt); break;
                        case QilNodeType.Eq: this.helper.Emit(OpCodes.Ceq); break;
                        default:
                            switch (relOp) {
                                case QilNodeType.Ge: opcode = OpCodes.Bge_S; break;
                                case QilNodeType.Le: opcode = OpCodes.Ble_S; break;
                                case QilNodeType.Ne: opcode = OpCodes.Bne_Un_S; break;
                                default: Debug.Assert(false); opcode = OpCodes.Nop; break;
                            }

                            // Push "true" if comparison succeeds, "false" otherwise
                            lblTrue = this.helper.DefineLabel();
                            this.helper.Emit(opcode, lblTrue);
                            this.helper.ConvBranchToBool(lblTrue, true);
                            break;
                    }
                    this.iterCurr.Storage = StorageDescriptor.Stack(typeof(bool), false);
                    break;
            }
        }

        /// <summary>
        /// Generate code to compare the top stack value to 0 by using the Brfalse or Brtrue instructions,
        /// which avoid pushing zero onto the stack.  Both of these instructions test for null/zero/false.
        /// </summary>
        private void ZeroCompare(QilNodeType relOp, bool isBoolVal) {
            Label lblTrue;
            Debug.Assert(relOp == QilNodeType.Eq || relOp == QilNodeType.Ne);

            // Test to determine if top stack value is zero (if relOp is Eq) or is not zero (if relOp is Ne)
            switch (this.iterCurr.CurrentBranchingContext) {
                case BranchingContext.OnTrue:
                    // If relOp is Eq, jump to true label if top value is zero (Brfalse)
                    // If relOp is Ne, jump to true label if top value is non-zero (Brtrue)
                    this.helper.Emit((relOp == QilNodeType.Eq) ? OpCodes.Brfalse : OpCodes.Brtrue, this.iterCurr.LabelBranch);
                    this.iterCurr.Storage = StorageDescriptor.None();
                    break;

                case BranchingContext.OnFalse:
                    // If relOp is Eq, jump to false label if top value is non-zero (Brtrue)
                    // If relOp is Ne, jump to false label if top value is zero (Brfalse)
                    this.helper.Emit((relOp == QilNodeType.Eq) ? OpCodes.Brtrue : OpCodes.Brfalse, this.iterCurr.LabelBranch);
                    this.iterCurr.Storage = StorageDescriptor.None();
                    break;

                default:
                    Debug.Assert(this.iterCurr.CurrentBranchingContext == BranchingContext.None);

                    // Since (boolval != 0) = boolval, value on top of the stack is already correct
                    if (!isBoolVal || relOp == QilNodeType.Eq) {
                        // If relOp is Eq, push "true" if top value is zero, "false" otherwise
                        // If relOp is Ne, push "true" if top value is non-zero, "false" otherwise
                        lblTrue = this.helper.DefineLabel();
                        this.helper.Emit((relOp == QilNodeType.Eq) ? OpCodes.Brfalse : OpCodes.Brtrue, lblTrue);
                        this.helper.ConvBranchToBool(lblTrue, true);
                    }

                    this.iterCurr.Storage = StorageDescriptor.Stack(typeof(bool), false);
                    break;
            }
        }

        /// <summary>
        /// Construction within a loop is starting.  If transition from non-Any to Any state occurs, then ensure
        /// that runtime state will be set.
        /// </summary>
        private void StartWriterLoop(QilNode nd, out bool hasOnEnd, out Label lblOnEnd) {
            XmlILConstructInfo info = XmlILConstructInfo.Read(nd);

            // By default, do not create a new iteration label
            hasOnEnd = false;
            lblOnEnd = new Label();

            // If loop is not involved in Xml construction, or if loop returns exactly one value, then do nothing
            if (!info.PushToWriterLast || nd.XmlType.IsSingleton)
                return;

            if (!this.iterCurr.HasLabelNext) {
                // Iterate until all items are constructed
                hasOnEnd = true;
                lblOnEnd = this.helper.DefineLabel();
                this.iterCurr.SetIterator(lblOnEnd, StorageDescriptor.None());
            }
        }

        /// <summary>
        /// Construction within a loop is ending.  If transition from non-Any to Any state occurs, then ensure that
        /// runtime state will be set.
        /// </summary>
        private void EndWriterLoop(QilNode nd, bool hasOnEnd, Label lblOnEnd) {
            XmlILConstructInfo info = XmlILConstructInfo.Read(nd);

            // If loop is not involved in Xml construction, then do nothing
            if (!info.PushToWriterLast)
                return;

            // Since results of construction were pushed to writer, there are no values to return
            this.iterCurr.Storage = StorageDescriptor.None();

            // If loop returns exactly one value, then do nothing further
            if (nd.XmlType.IsSingleton)
                return;

            if (hasOnEnd) {
                // Loop over all items in the list, sending each to the output writer
                this.iterCurr.LoopToEnd(lblOnEnd);
            }
        }

        /// <summary>
        /// Returns true if the specified node's owner element might have local namespaces added to it
        /// after attributes have already been added.
        /// </summary>
        private bool MightHaveNamespacesAfterAttributes(XmlILConstructInfo info) {
            // Get parent element
            if (info != null)
                info = info.ParentElementInfo;

            // If a parent element has not been statically identified, then assume that the runtime
            // element will have namespaces added after attributes.
            if (info == null)
                return true;

            return info.MightHaveNamespacesAfterAttributes;
        }

        /// <summary>
        /// Returns true if the specified element should cache attributes.
        /// </summary>
        private bool ElementCachesAttributes(XmlILConstructInfo info) {
            // Attributes will be cached if namespaces might be constructed after the attributes
            return info.MightHaveDuplicateAttributes || info.MightHaveNamespacesAfterAttributes;
        }

        /// <summary>
        /// This method is called before calling any WriteEnd??? method.  It generates code to perform runtime
        /// construction checks separately.  This should only be called if the XmlQueryOutput::StartElementChk
        /// method will *not* be called.
        /// </summary>
        private void BeforeStartChecks(QilNode ndCtor) {
            switch (XmlILConstructInfo.Read(ndCtor).InitialStates) {
                case PossibleXmlStates.WithinSequence:
                    // If runtime state is guaranteed to be WithinSequence, then call XmlQueryOutput.StartTree
                    this.helper.CallStartTree(QilConstructorToNodeType(ndCtor.NodeType));
                    break;

                case PossibleXmlStates.EnumAttrs:
                    switch (ndCtor.NodeType) {
                        case QilNodeType.ElementCtor:
                        case QilNodeType.TextCtor:
                        case QilNodeType.RawTextCtor:
                        case QilNodeType.PICtor:
                        case QilNodeType.CommentCtor:
                            // If runtime state is guaranteed to be EnumAttrs, and content is being constructed, call
                            // XmlQueryOutput.StartElementContent
                            this.helper.CallStartElementContent();
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// This method is called after calling any WriteEnd??? method.  It generates code to perform runtime
        /// construction checks separately.  This should only be called if the XmlQueryOutput::EndElementChk
        /// method will *not* be called.
        /// </summary>
        private void AfterEndChecks(QilNode ndCtor) {
            if (XmlILConstructInfo.Read(ndCtor).FinalStates == PossibleXmlStates.WithinSequence) {
                // If final runtime state is guaranteed to be WithinSequence, then call XmlQueryOutput.StartTree
                this.helper.CallEndTree();
            }
        }

        /// <summary>
        /// Return true if a runtime check needs to be made in order to transition into the WithinContent state.
        /// </summary>
        private bool CheckWithinContent(XmlILConstructInfo info) {
            switch (info.InitialStates) {
                case PossibleXmlStates.WithinSequence:
                case PossibleXmlStates.EnumAttrs:
                case PossibleXmlStates.WithinContent:
                    // Transition to WithinContent can be ensured at compile-time
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Return true if a runtime check needs to be made in order to transition into the EnumAttrs state.
        /// </summary>
        private bool CheckEnumAttrs(XmlILConstructInfo info) {
            switch (info.InitialStates) {
                case PossibleXmlStates.WithinSequence:
                case PossibleXmlStates.EnumAttrs:
                    // Transition to EnumAttrs can be ensured at compile-time
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Map the XmlNodeKindFlags enumeration into the XPathNodeType enumeration.
        /// </summary>
        private XPathNodeType QilXmlToXPathNodeType(XmlNodeKindFlags xmlTypes) {
            switch (xmlTypes) {
                case XmlNodeKindFlags.Element: return XPathNodeType.Element;
                case XmlNodeKindFlags.Attribute: return XPathNodeType.Attribute;
                case XmlNodeKindFlags.Text: return XPathNodeType.Text;
                case XmlNodeKindFlags.Comment: return XPathNodeType.Comment;
            }
            Debug.Assert(xmlTypes == XmlNodeKindFlags.PI);
            return XPathNodeType.ProcessingInstruction;
        }

        /// <summary>
        /// Map a QilExpression constructor type into the XPathNodeType enumeration.
        /// </summary>
        private XPathNodeType QilConstructorToNodeType(QilNodeType typ) {
            switch (typ) {
                case QilNodeType.DocumentCtor: return XPathNodeType.Root;
                case QilNodeType.ElementCtor: return XPathNodeType.Element;
                case QilNodeType.TextCtor: return XPathNodeType.Text;
                case QilNodeType.RawTextCtor: return XPathNodeType.Text;
                case QilNodeType.PICtor: return XPathNodeType.ProcessingInstruction;
                case QilNodeType.CommentCtor: return XPathNodeType.Comment;
                case QilNodeType.AttributeCtor: return XPathNodeType.Attribute;
                case QilNodeType.NamespaceDecl: return XPathNodeType.Namespace;
            }

            Debug.Assert(false, "Cannot map QilNodeType " + typ + " to an XPathNodeType");
            return XPathNodeType.All;
        }

        /// <summary>
        /// Load an XmlNavigatorFilter that matches only the specified name and types onto the stack.
        /// </summary>
        private void LoadSelectFilter(XmlNodeKindFlags xmlTypes, QilName ndName) {
            if (ndName != null) {
                // Push NameFilter
                Debug.Assert(xmlTypes == XmlNodeKindFlags.Element);
                this.helper.CallGetNameFilter(this.helper.StaticData.DeclareNameFilter(ndName.LocalName, ndName.NamespaceUri));
            }
            else {
                // Either type cannot be a union, or else it must be >= union of all Content types
                bool isXmlTypeUnion = IsNodeTypeUnion(xmlTypes);
                Debug.Assert(!isXmlTypeUnion || (xmlTypes & XmlNodeKindFlags.Content) == XmlNodeKindFlags.Content);

                if (isXmlTypeUnion) {
                    if ((xmlTypes & XmlNodeKindFlags.Attribute) != 0) {
                        // Union includes attributes, so allow all node kinds
                        this.helper.CallGetTypeFilter(XPathNodeType.All);
                    }
                    else {
                        // Filter attributes
                        this.helper.CallGetTypeFilter(XPathNodeType.Attribute);
                    }
                }
                else {
                    // Filter nodes of all but one type
                    this.helper.CallGetTypeFilter(QilXmlToXPathNodeType(xmlTypes));
                }
            }
        }

        /// <summary>
        /// Return true if more than one node type is set.
        /// </summary>
        private static bool IsNodeTypeUnion(XmlNodeKindFlags xmlTypes) {
            return ((int) xmlTypes & ((int) xmlTypes - 1)) != 0;
        }

        /// <summary>
        /// Start construction of a new nested iterator.  If this.iterCurr == null, then the new iterator
        /// is a top-level, or root iterator.  Otherwise, the new iterator will be nested within the
        /// current iterator.
        /// </summary>
        private void StartNestedIterator(QilNode nd) {
            IteratorDescriptor iterParent = this.iterCurr;

            // Create a new, nested iterator
            if (iterParent == null) {
                // Create a "root" iterator info that has no parernt
                this.iterCurr = new IteratorDescriptor(this.helper);
            }
            else {
                // Create a nested iterator
                this.iterCurr = new IteratorDescriptor(iterParent);
            }

            this.iterNested = null;
        }

        /// <summary>
        /// Calls StartNestedIterator(nd) and also sets up the nested iterator to branch to "lblOnEnd" when iteration
        /// is complete.
        /// </summary>
        private void StartNestedIterator(QilNode nd, Label lblOnEnd) {
            StartNestedIterator(nd);
            this.iterCurr.SetIterator(lblOnEnd, StorageDescriptor.None());
        }

        /// <summary>
        /// End construction of the current iterator.
        /// </summary>
        private void EndNestedIterator(QilNode nd) {
            Debug.Assert(this.iterCurr.Storage.Location == ItemLocation.None ||
                         this.iterCurr.Storage.ItemStorageType == GetItemStorageType(nd) ||
                         this.iterCurr.Storage.ItemStorageType == typeof(XPathItem) ||
                         nd.XmlType.TypeCode == XmlTypeCode.None,
                         "QilNodeType " + nd.NodeType + " cannot be stored using type " + this.iterCurr.Storage.ItemStorageType + ".");

            // If the nested iterator was constructed in branching mode,
            if (this.iterCurr.IsBranching) {
                // Then if branching hasn't already taken place, do so now
                if (this.iterCurr.Storage.Location != ItemLocation.None) {
                    this.iterCurr.EnsureItemStorageType(nd.XmlType, typeof(bool));
                    this.iterCurr.EnsureStackNoCache();

                    if (this.iterCurr.CurrentBranchingContext == BranchingContext.OnTrue)
                        this.helper.Emit(OpCodes.Brtrue, this.iterCurr.LabelBranch);
                    else
                        this.helper.Emit(OpCodes.Brfalse, this.iterCurr.LabelBranch);

                    this.iterCurr.Storage = StorageDescriptor.None();
                }
            }

            // Save current iterator as nested iterator
            this.iterNested = this.iterCurr;

            // Update current iterator to be parent iterator
            this.iterCurr = this.iterCurr.ParentIterator;
        }

        /// <summary>
        /// Recursively generate code to iterate over the results of the "nd" expression.  If "nd" is pushed
        /// to the writer, then there are no results.  If "nd" is a singleton expression and isCached is false,
        /// then generate code to construct the singleton.  Otherwise, cache the sequence in an XmlQuerySequence
        /// object.  Ensure that all items are converted to the specified "itemStorageType".
        /// </summary>
        private void NestedVisit(QilNode nd, Type itemStorageType, bool isCached) {
            if (XmlILConstructInfo.Read(nd).PushToWriterLast) {
                // Push results to output, so nothing is left to store
                StartNestedIterator(nd);
                Visit(nd);
                EndNestedIterator(nd);
                this.iterCurr.Storage = StorageDescriptor.None();
            }
            else if (!isCached && nd.XmlType.IsSingleton) {
                // Storage of result will be a non-cached singleton
                StartNestedIterator(nd);
                Visit(nd);
                this.iterCurr.EnsureNoCache();
                this.iterCurr.EnsureItemStorageType(nd.XmlType, itemStorageType);
                EndNestedIterator(nd);
                this.iterCurr.Storage = this.iterNested.Storage;
            }
            else {
                NestedVisitEnsureCache(nd, itemStorageType);
            }
        }

        /// <summary>
        /// Calls NestedVisit(QilNode, Type, bool), storing result in the default storage type for "nd".
        /// </summary>
        private void NestedVisit(QilNode nd) {
            NestedVisit(nd, GetItemStorageType(nd), !nd.XmlType.IsSingleton);
        }

        /// <summary>
        /// Recursively generate code to iterate over the results of the "nd" expression.  When the expression
        /// has been fully iterated, it will jump to "lblOnEnd".
        /// </summary>
        private void NestedVisit(QilNode nd, Label lblOnEnd) {
            Debug.Assert(!XmlILConstructInfo.Read(nd).PushToWriterLast);
            StartNestedIterator(nd, lblOnEnd);
            Visit(nd);
            this.iterCurr.EnsureNoCache();
            this.iterCurr.EnsureItemStorageType(nd.XmlType, GetItemStorageType(nd));
            EndNestedIterator(nd);
            this.iterCurr.Storage = this.iterNested.Storage;
        }

        /// <summary>
        /// Call NestedVisit(QilNode) and ensure that result is pushed onto the IL stack.
        /// </summary>
        private void NestedVisitEnsureStack(QilNode nd) {
            Debug.Assert(!XmlILConstructInfo.Read(nd).PushToWriterLast);
            NestedVisit(nd);
            this.iterCurr.EnsureStack();
        }

        /// <summary>
        /// Generate code for both QilExpression nodes and ensure that each result is pushed onto the IL stack.
        /// </summary>
        private void NestedVisitEnsureStack(QilNode ndLeft, QilNode ndRight) {
            NestedVisitEnsureStack(ndLeft);
            NestedVisitEnsureStack(ndRight);
        }

        /// <summary>
        /// Call NestedVisit(QilNode, Type, bool) and ensure that result is pushed onto the IL stack.
        /// </summary>
        private void NestedVisitEnsureStack(QilNode nd, Type itemStorageType, bool isCached) {
            Debug.Assert(!XmlILConstructInfo.Read(nd).PushToWriterLast);
            NestedVisit(nd, itemStorageType, isCached);
            this.iterCurr.EnsureStack();
        }

        /// <summary>
        /// Call NestedVisit(QilNode) and ensure that result is stored in local variable "loc".
        /// </summary>
        private void NestedVisitEnsureLocal(QilNode nd, LocalBuilder loc) {
            Debug.Assert(!XmlILConstructInfo.Read(nd).PushToWriterLast);
            NestedVisit(nd);
            this.iterCurr.EnsureLocal(loc);
        }

        /// <summary>
        /// Start a nested iterator in a branching context and recursively generate code for the specified QilExpression node.
        /// </summary>
        private void NestedVisitWithBranch(QilNode nd, BranchingContext brctxt, Label lblBranch) {
            Debug.Assert(nd.XmlType.IsSingleton && !XmlILConstructInfo.Read(nd).PushToWriterLast);
            StartNestedIterator(nd);
            this.iterCurr.SetBranching(brctxt, lblBranch);
            Visit(nd);
            EndNestedIterator(nd);
            this.iterCurr.Storage = StorageDescriptor.None();
        }

        /// <summary>
        /// Generate code for the QilExpression node and ensure that results are fully cached as an XmlQuerySequence.  All results
        /// should be converted to "itemStorageType" before being added to the cache.
        /// </summary>
        private void NestedVisitEnsureCache(QilNode nd, Type itemStorageType) {
            Debug.Assert(!XmlILConstructInfo.Read(nd).PushToWriterLast);
            bool cachesResult = CachesResult(nd);
            LocalBuilder locCache;
            Label lblOnEnd = this.helper.DefineLabel();
            Type cacheType;
            XmlILStorageMethods methods;

            // If bound expression will already be cached correctly, then don't create an XmlQuerySequence
            if (cachesResult) {
                StartNestedIterator(nd);
                Visit(nd);
                EndNestedIterator(nd);
                this.iterCurr.Storage = this.iterNested.Storage;
                Debug.Assert(this.iterCurr.Storage.IsCached, "Expression result should be cached.  CachesResult() might have a bug in it.");

                // If type of items in the cache matches "itemStorageType", then done
                if (this.iterCurr.Storage.ItemStorageType == itemStorageType)
                    return;

                // If the cache has navigators in it, or if converting to a cache of navigators, then EnsureItemStorageType
                // can directly convert without needing to create a new cache.
                if (this.iterCurr.Storage.ItemStorageType == typeof(XPathNavigator) || itemStorageType == typeof(XPathNavigator)) {
                    this.iterCurr.EnsureItemStorageType(nd.XmlType, itemStorageType);
                    return;
                }

                this.iterCurr.EnsureNoStack("$$$cacheResult");
            }

            // Always store navigators in XmlQueryNodeSequence (which implements IList<XPathItem>)
            cacheType = (GetItemStorageType(nd) == typeof(XPathNavigator)) ? typeof(XPathNavigator) : itemStorageType;

            // XmlQuerySequence<T> cache;
            methods = XmlILMethods.StorageMethods[cacheType];
            locCache = this.helper.DeclareLocal("$$$cache", methods.SeqType);
            this.helper.Emit(OpCodes.Ldloc, locCache);

            // Special case non-navigator singletons to use overload of CreateOrReuse
            if (nd.XmlType.IsSingleton) {
                // cache = XmlQuerySequence.CreateOrReuse(cache, item);
                NestedVisitEnsureStack(nd, cacheType, false);
                this.helper.CallToken(methods.SeqReuseSgl);
                this.helper.Emit(OpCodes.Stloc, locCache);
            }
            else {
                // XmlQuerySequence<T> cache;
                // cache = XmlQuerySequence.CreateOrReuse(cache);
                this.helper.CallToken(methods.SeqReuse);
                this.helper.Emit(OpCodes.Stloc, locCache);
                this.helper.Emit(OpCodes.Ldloc, locCache);

                StartNestedIterator(nd, lblOnEnd);

                if (cachesResult)
                    this.iterCurr.Storage = this.iterCurr.ParentIterator.Storage;
                else
                    Visit(nd);

                // cache.Add(item);
                this.iterCurr.EnsureItemStorageType(nd.XmlType, cacheType);
                this.iterCurr.EnsureStackNoCache();
                this.helper.Call(methods.SeqAdd);
                this.helper.Emit(OpCodes.Ldloc, locCache);

                // }
                this.iterCurr.LoopToEnd(lblOnEnd);

                EndNestedIterator(nd);

                // Remove cache reference from stack
                this.helper.Emit(OpCodes.Pop);
            }

            this.iterCurr.Storage = StorageDescriptor.Local(locCache, itemStorageType, true);
        }

        /// <summary>
        /// Returns true if the specified QilExpression node type is *guaranteed* to cache its results in an XmlQuerySequence,
        /// where items in the cache are stored using the default storage type.
        /// </summary>
        private bool CachesResult(QilNode nd) {
            OptimizerPatterns patt;

            switch (nd.NodeType) {
                case QilNodeType.Let:
                case QilNodeType.Parameter:
                case QilNodeType.Invoke:
                case QilNodeType.XsltInvokeLateBound:
                case QilNodeType.XsltInvokeEarlyBound:
                    return !nd.XmlType.IsSingleton;

                case QilNodeType.Filter:
                    // EqualityIndex pattern caches results
                    patt = OptimizerPatterns.Read(nd);
                    return patt.MatchesPattern(OptimizerPatternName.EqualityIndex);

                case QilNodeType.DocOrderDistinct:
                    if (nd.XmlType.IsSingleton)
                        return false;

                    // JoinAndDod and DodReverse patterns don't cache results
                    patt = OptimizerPatterns.Read(nd);
                    return !patt.MatchesPattern(OptimizerPatternName.JoinAndDod) && !patt.MatchesPattern(OptimizerPatternName.DodReverse);

                case QilNodeType.TypeAssert:
                    QilTargetType ndTypeAssert = (QilTargetType) nd;
                    // Check if TypeAssert would be no-op
                    return CachesResult(ndTypeAssert.Source) && GetItemStorageType(ndTypeAssert.Source) == GetItemStorageType(ndTypeAssert);
            }

            return false;
        }

        /// <summary>
        /// Shortcut call to XmlILTypeHelper.GetStorageType.
        /// </summary>
        private Type GetStorageType(QilNode nd) {
            return XmlILTypeHelper.GetStorageType(nd.XmlType);
        }

        /// <summary>
        /// Shortcut call to XmlILTypeHelper.GetStorageType.
        /// </summary>
        private Type GetStorageType(XmlQueryType typ) {
            return XmlILTypeHelper.GetStorageType(typ);
        }

        /// <summary>
        /// Shortcut call to XmlILTypeHelper.GetStorageType, using an expression's prime type.
        /// </summary>
        private Type GetItemStorageType(QilNode nd) {
            return XmlILTypeHelper.GetStorageType(nd.XmlType.Prime);
        }

        /// <summary>
        /// Shortcut call to XmlILTypeHelper.GetStorageType, using the prime type.
        /// </summary>
        private Type GetItemStorageType(XmlQueryType typ) {
            return XmlILTypeHelper.GetStorageType(typ.Prime);
        }
    }
}
