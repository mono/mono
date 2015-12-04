//------------------------------------------------------------------------------
// <copyright file="IteratorDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Xsl.Runtime;

namespace System.Xml.Xsl.IlGen {

    /// <summary>
    /// Type of location in which iterator items are stored.
    /// </summary>
    internal enum ItemLocation {
        None = 0,
        Stack,                              // Each value is stored as the top value on the IL stack
        Parameter,                          // Each value is stored as a parameter to the current method
        Local,                              // Each value is stored as a local variable in the current method
        Current,                            // Each value is stored as an iterator's Current property
        Global,                             // Each value is stored as a global variable
    };


    /// <summary>
    /// None--Not in a branching context
    /// True--Branch if boolean expression evaluates to true
    /// False--Branch if boolean expression evaluates to false
    /// </summary>
    internal enum BranchingContext {None, OnTrue, OnFalse};

    /// <summary>
    /// Describes the Clr type and location of items returned by an iterator.
    /// This struct is meant to be immutable.
    /// </summary>
    internal struct StorageDescriptor {
        private ItemLocation location;
        private object locationObject;
        private Type itemStorageType;
        private bool isCached;


        //-----------------------------------------------
        // Create Methods
        //-----------------------------------------------

        /// <summary>
        /// Create default, empty StorageDescriptor.
        /// </summary>
        public static StorageDescriptor None() {
            return new StorageDescriptor();
        }

        /// <summary>
        /// Create a StorageDescriptor for an item located on the stack.
        /// </summary>
        public static StorageDescriptor Stack(Type itemStorageType, bool isCached) {
            StorageDescriptor storage = new StorageDescriptor();
            storage.location = ItemLocation.Stack;
            storage.itemStorageType = itemStorageType;
            storage.isCached = isCached;
            return storage;
        }

        /// <summary>
        /// Create a StorageDescriptor for an item which is a parameter to the current method.
        /// </summary>
        public static StorageDescriptor Parameter(int paramIndex, Type itemStorageType, bool isCached) {
            StorageDescriptor storage = new StorageDescriptor();
            storage.location = ItemLocation.Parameter;
            storage.locationObject = paramIndex;
            storage.itemStorageType = itemStorageType;
            storage.isCached = isCached;
            return storage;
        }

        /// <summary>
        /// Create a StorageDescriptor for an item located in a local variable.
        /// </summary>
        public static StorageDescriptor Local(LocalBuilder loc, Type itemStorageType, bool isCached) {
            Debug.Assert(loc.LocalType == itemStorageType ||
                         typeof(IList<>).MakeGenericType(itemStorageType).IsAssignableFrom(loc.LocalType),
                         "Type " + itemStorageType + " does not match the local variable's type");

            StorageDescriptor storage = new StorageDescriptor();
            storage.location = ItemLocation.Local;
            storage.locationObject = loc;
            storage.itemStorageType = itemStorageType;
            storage.isCached = isCached;
            return storage;
        }

        /// <summary>
        /// Create a StorageDescriptor for an item which is the Current item in an iterator.
        /// </summary>
        public static StorageDescriptor Current(LocalBuilder locIter, Type itemStorageType) {
            Debug.Assert(locIter.LocalType.GetMethod("get_Current").ReturnType == itemStorageType,
                         "Type " + itemStorageType + " does not match type of Current property.");

            StorageDescriptor storage = new StorageDescriptor();
            storage.location = ItemLocation.Current;
            storage.locationObject = locIter;
            storage.itemStorageType = itemStorageType;
            return storage;
        }

        /// <summary>
        /// Create a StorageDescriptor for an item located in a global variable.
        /// </summary>
        public static StorageDescriptor Global(MethodInfo methGlobal, Type itemStorageType, bool isCached) {
            Debug.Assert(methGlobal.ReturnType == itemStorageType ||
                         typeof(IList<>).MakeGenericType(itemStorageType).IsAssignableFrom(methGlobal.ReturnType),
                         "Type " + itemStorageType + " does not match the global method's return type");

            StorageDescriptor storage = new StorageDescriptor();
            storage.location = ItemLocation.Global;
            storage.locationObject = methGlobal;
            storage.itemStorageType = itemStorageType;
            storage.isCached = isCached;
            return storage;
        }


        //-----------------------------------------------
        // Accessor Methods
        //-----------------------------------------------

        /// <summary>
        /// Return copy of current descriptor, but change item's location to the stack.
        /// </summary>
        public StorageDescriptor ToStack() {
            return Stack(this.itemStorageType, this.isCached);
        }

        /// <summary>
        /// Create a StorageDescriptor for an item located in a local variable.
        /// </summary>
        public StorageDescriptor ToLocal(LocalBuilder loc) {
            return Local(loc, this.itemStorageType, this.isCached);
        }

        /// <summary>
        /// Create a StorageDescriptor which is the same as this one, except for the item storage type.
        /// </summary>
        public StorageDescriptor ToStorageType(Type itemStorageType) {
            StorageDescriptor storage = this;
            storage.itemStorageType = itemStorageType;
            return storage;
        }

        /// <summary>
        /// Return an enumeration specifying where the value is located.
        /// </summary>
        public ItemLocation Location {
            get { return this.location; }
        }

        /// <summary>
        /// Return the index of the parameter that stores this iterator's values.
        /// </summary>
        public int ParameterLocation {
            get { return (int) this.locationObject; }
        }

        /// <summary>
        /// Return the LocalBuilder that stores this iterator's values.
        /// </summary>
        public LocalBuilder LocalLocation {
            get { return this.locationObject as LocalBuilder; }
        }

        /// <summary>
        /// Return the LocalBuilder that will store this iterator's helper class.  The Current property
        /// on this iterator can be accessed to get the current iteration value.
        /// </summary>
        public LocalBuilder CurrentLocation {
            get { return this.locationObject as LocalBuilder; }
        }

        /// <summary>
        /// Return the MethodInfo for the method that computes this global value.
        /// </summary>
        public MethodInfo GlobalLocation {
            get { return this.locationObject as MethodInfo; }
        }

        /// <summary>
        /// Return true if this iterator's values are cached.
        /// </summary>
        public bool IsCached {
            get { return this.isCached; }
        }

        /// <summary>
        /// Return the Clr type of an individual item in the storage location (never an IList<> type).
        /// </summary>
        public Type ItemStorageType {
            get { return this.itemStorageType; }
        }
    }

    /// <summary>
    /// Iterators are joined together, are nested within each other, and reference each other.  This internal class
    /// contains detailed information about iteration next labels, caching, iterator item location, etc.
    /// </summary>
    internal class IteratorDescriptor {
        private GenerateHelper helper;

        // Related iterators
        private IteratorDescriptor iterParent;

        // Iteration
        private Label lblNext;
        private bool hasNext;
        private LocalBuilder locPos;

        // Branching
        private BranchingContext brctxt;
        private Label lblBranch;

        // Storage
        private StorageDescriptor storage;


        //-----------------------------------------------
        // Initialize
        //-----------------------------------------------

        /// <summary>
        /// Create a "root" IteratorDescriptor which has no parent iterator.
        /// </summary>
        public IteratorDescriptor(GenerateHelper helper) {
            Init(null, helper);
        }

        /// <summary>
        /// Create an IteratorDescriptor that is nested in a parent iterator.
        /// </summary>
        public IteratorDescriptor(IteratorDescriptor iterParent) {
            Init(iterParent, iterParent.helper);
        }

        /// <summary>
        /// Internal helper initializor.
        /// </summary>
        private void Init(IteratorDescriptor iterParent, GenerateHelper helper) {
            this.helper = helper;
            this.iterParent = iterParent;
        }


        //-----------------------------------------------
        // Related Iterators
        //-----------------------------------------------

        /// <summary>
        /// Return the iterator in which this iterator is nested.
        /// </summary>
        public IteratorDescriptor ParentIterator {
            get { return this.iterParent; }
        }


        //-----------------------------------------------
        // Iteration
        //-----------------------------------------------

        /// <summary>
        /// Returns true if LabelNext is currently defined.  If not, then this iterator will return
        /// exactly one result (iterator is cardinality one).
        /// </summary>
        public bool HasLabelNext {
            get { return this.hasNext; }
        }

        /// <summary>
        /// Return the label that is anchored to this code iterator's MoveNext code.
        /// </summary>
        public Label GetLabelNext() {
            Debug.Assert(this.hasNext);
            return this.lblNext;
        }

        /// <summary>
        /// Set this iterator's next label and storage.  This iterator will range over a set of values located in
        /// "storage".  To get the next value, jump to "lblNext".
        /// </summary>
        public void SetIterator(Label lblNext, StorageDescriptor storage) {
            this.lblNext = lblNext;
            this.hasNext = true;
            this.storage = storage;
        }

        /// <summary>
        /// Set this iterator to be the same as the specified iterator.
        /// </summary>
        public void SetIterator(IteratorDescriptor iterInfo) {
            if (iterInfo.HasLabelNext) {
                this.lblNext = iterInfo.GetLabelNext();
                this.hasNext = true;
            }

            this.storage = iterInfo.Storage;
        }

        /// <summary>
        /// Continue iteration until it is complete.  Branch to "lblOnEnd" when iteration is complete.
        /// </summary>
        /// <remarks>
        /// goto LabelNextCtxt;
        /// LabelOnEnd:
        /// </remarks>
        public void LoopToEnd(Label lblOnEnd) {
            if (this.hasNext) {
                this.helper.BranchAndMark(this.lblNext, lblOnEnd);
                this.hasNext = false;
            }

            // After looping is finished, storage is N/A
            this.storage = StorageDescriptor.None();
        }

        /// <summary>
        /// Storage location containing the position of the current item as an integer.
        /// This location is only defined on iterators, and then only if they might be
        /// referenced by a PositionOf operator.
        /// </summary>
        public LocalBuilder LocalPosition {
            get { return this.locPos; }
            set { this.locPos = value; }
        }


        //-----------------------------------------------
        // Caching
        //-----------------------------------------------

        /// <summary>
        /// Push the count of items in the cache onto the stack.
        /// </summary>
        public void CacheCount() {
            Debug.Assert(this.storage.IsCached);
            PushValue();
            this.helper.CallCacheCount(this.storage.ItemStorageType);
        }

        /// <summary>
        /// If the iterator has been fully cached, then iterate the values one-by-one.
        /// </summary>
        public void EnsureNoCache() {
            if (this.storage.IsCached) {
                if (!HasLabelNext) {
                    // If no Next label, this must be a singleton cache
                    EnsureStack();
                    this.helper.LoadInteger(0);
                    this.helper.CallCacheItem(this.storage.ItemStorageType);

                    this.storage = StorageDescriptor.Stack(this.storage.ItemStorageType, false);
                }
                else {
                    // int idx;
                    LocalBuilder locIdx = this.helper.DeclareLocal("$$$idx", typeof(int));
                    Label lblNext;

                    // Make sure cache is not on the stack
                    EnsureNoStack("$$$cache");

                    // idx = -1;
                    this.helper.LoadInteger(-1);
                    this.helper.Emit(OpCodes.Stloc, locIdx);

                    // LabelNext:
                    lblNext = this.helper.DefineLabel();
                    this.helper.MarkLabel(lblNext);

                    // idx++;
                    this.helper.Emit(OpCodes.Ldloc, locIdx);
                    this.helper.LoadInteger(1);
                    this.helper.Emit(OpCodes.Add);
                    this.helper.Emit(OpCodes.Stloc, locIdx);

                    // if (idx >= cache.Count) goto LabelNextCtxt;
                    this.helper.Emit(OpCodes.Ldloc, locIdx);
                    CacheCount();
                    this.helper.Emit(OpCodes.Bge, GetLabelNext());

                    // item = cache[idx];
                    PushValue();
                    this.helper.Emit(OpCodes.Ldloc, locIdx);
                    this.helper.CallCacheItem(this.storage.ItemStorageType);

                    SetIterator(lblNext, StorageDescriptor.Stack(this.storage.ItemStorageType, false));
                }
            }
        }


        //-----------------------------------------------
        // If-then-else branching
        //-----------------------------------------------

        /// <summary>
        /// Setup a branching context.  All nested iterators compiled in this context must evaluate
        /// to a single boolean value.  However, these expressions must not push the result as a boolean
        /// onto the stack.  Instead, if brctxt is BranchType.True, then the expression should
        /// jump to lblBranch if it evaluates to true.  If brctxt is BranchType.False, then the
        /// branch should happen if the evaluation result is false.
        /// </summary>
        public void SetBranching(BranchingContext brctxt, Label lblBranch) {
            Debug.Assert(brctxt != BranchingContext.None);
            this.brctxt = brctxt;
            this.lblBranch = lblBranch;
        }

        /// <summary>
        /// True if within a branching context.
        /// </summary>
        public bool IsBranching {
            get { return this.brctxt != BranchingContext.None; }
        }

        /// <summary>
        /// Returns the label to which conditionals should branch.
        /// </summary>
        public Label LabelBranch {
            get { return this.lblBranch; }
        }

        /// <summary>
        /// If BranchingContext.OnTrue, branch on true.  Otherwise, branch on false.
        /// </summary>
        public BranchingContext CurrentBranchingContext {
            get { return this.brctxt; }
        }


        //-----------------------------------------------
        // Storage
        //-----------------------------------------------

        /// <summary>
        /// Returns information about how and where iterator values are stored.
        /// </summary>
        public StorageDescriptor Storage {
            get { return this.storage; }
            set { this.storage = value; }
        }

        /// <summary>
        /// Push current item onto the stack without affecting Location.
        /// </summary>
        public void PushValue() {
            switch (this.storage.Location) {
                case ItemLocation.Stack:
                    this.helper.Emit(OpCodes.Dup);
                    break;

                case ItemLocation.Parameter:
                    this.helper.LoadParameter(this.storage.ParameterLocation);
                    break;

                case ItemLocation.Local:
                    this.helper.Emit(OpCodes.Ldloc, this.storage.LocalLocation);
                    break;

                case ItemLocation.Current:
                    this.helper.Emit(OpCodes.Ldloca, this.storage.CurrentLocation);
                    this.helper.Call(this.storage.CurrentLocation.LocalType.GetMethod("get_Current"));
                    break;

                default:
                    Debug.Assert(false, "Invalid location: " + this.storage.Location);
                    break;
            }
        }
       
        /// <summary>
        /// Ensure that the current item is pushed onto the stack.
        /// </summary>
        public void EnsureStack() {
            switch (this.storage.Location) {
                case ItemLocation.Stack:
                    // Already on the stack
                    return;

                case ItemLocation.Parameter:
                case ItemLocation.Local:
                case ItemLocation.Current:
                    PushValue();
                    break;

                case ItemLocation.Global:
                    // Call method that computes the value of this global value
                    this.helper.LoadQueryRuntime();
                    this.helper.Call(this.storage.GlobalLocation);
                    break;

                default:
                    Debug.Assert(false, "Invalid location: " + this.storage.Location);
                    break;
            }

            this.storage = this.storage.ToStack();
        }

        /// <summary>
        /// If the current item is on the stack, move it to a local variable.
        /// </summary>
        public void EnsureNoStack(string locName) {
            if (this.storage.Location == ItemLocation.Stack)
                EnsureLocal(locName);
        }

        /// <summary>
        /// If current item is not already in a local variable, then move it to a local variable of the specified name.
        /// </summary>
        public void EnsureLocal(string locName) {
            if (this.storage.Location != ItemLocation.Local) {
                if (this.storage.IsCached)
                    EnsureLocal(this.helper.DeclareLocal(locName, typeof(IList<>).MakeGenericType(this.storage.ItemStorageType)));
                else
                    EnsureLocal(this.helper.DeclareLocal(locName, this.storage.ItemStorageType));
            }
        }

        /// <summary>
        /// Ensure that current item is saved to the specified local variable.
        /// </summary>
        public void EnsureLocal(LocalBuilder bldr) {
            if (this.storage.LocalLocation != bldr) {
                // Push value onto stack and then save to bldr
                EnsureStack();
                this.helper.Emit(OpCodes.Stloc, bldr);
                this.storage = this.storage.ToLocal(bldr);
            }
        }

        /// <summary>
        /// Discard the current item if it is pushed onto the stack.
        /// </summary>
        public void DiscardStack() {
            if (this.storage.Location == ItemLocation.Stack) {
                this.helper.Emit(OpCodes.Pop);
                this.storage = StorageDescriptor.None();
            }
        }
       
        /// <summary>
        /// Ensure that the iterator's items are not cached, and that the current item is pushed onto the stack.
        /// </summary>
        public void EnsureStackNoCache() {
            EnsureNoCache();
            EnsureStack();
        }

        /// <summary>
        /// Ensure that the iterator's items are not cached, and that if the current item is pushed onto the stack,
        /// that it is moved to a local variable.
        /// </summary>
        public void EnsureNoStackNoCache(string locName) {
            EnsureNoCache();
            EnsureNoStack(locName);
        }

        /// <summary>
        /// Ensure that the iterator's items are not cached, and that if the current item is not already in a local,
        /// variable, that it is moved to a local variable of the specified name.
        /// </summary>
        public void EnsureLocalNoCache(string locName) {
            EnsureNoCache();
            EnsureLocal(locName);
        }

        /// <summary>
        /// Ensure that the iterator's items are not cached and that the current item is saved to the specified local variable.
        /// </summary>
        public void EnsureLocalNoCache(LocalBuilder bldr) {
            EnsureNoCache();
            EnsureLocal(bldr);
        }

        /// <summary>
        /// Each XmlQueryType has multiple legal CLR representations.  Ensure that all items returned by this iterator are in
        /// the Clr representation specified by "storageTypeDest".
        /// </summary>
        public void EnsureItemStorageType(XmlQueryType xmlType, Type storageTypeDest) {
            // If source type = destination type, then done
            if (this.storage.ItemStorageType == storageTypeDest)
                goto SetStorageType;

            Debug.Assert(this.storage.ItemStorageType == typeof(XPathItem) || storageTypeDest == typeof(XPathItem),
                         "EnsureItemStorageType must convert to or from Item");

            // If items are cached,
            if (this.storage.IsCached) {
                // Check for special case of IList<XPathNavigator> -> IList<XPathItem>
                if (this.storage.ItemStorageType == typeof(XPathNavigator)) {
                    EnsureStack();
                    this.helper.Call(XmlILMethods.NavsToItems);
                    goto SetStorageType;
                }

                // Check for special case of IList<XPathItem> -> IList<XPathNavigator>
                if (storageTypeDest == typeof(XPathNavigator)) {
                    EnsureStack();
                    this.helper.Call(XmlILMethods.ItemsToNavs);
                    goto SetStorageType;
                }
            }

            // Iterate over each item, and convert each to the destination type
            EnsureStackNoCache();

            // If source type is Item,
            if (this.storage.ItemStorageType == typeof(XPathItem)) {
                // Then downcast to Navigator
                if (storageTypeDest == typeof(XPathNavigator)) {
                    this.helper.Emit(OpCodes.Castclass, typeof(XPathNavigator));
                }
                else {
                    // Call ValueAs methods for atomic types
                    this.helper.CallValueAs(storageTypeDest);
                }
                goto SetStorageType;
            }
            else if (this.storage.ItemStorageType == typeof(XPathNavigator)) {
                // No-op if converting from XPathNavigator to XPathItem
                Debug.Assert(storageTypeDest == typeof(XPathItem), "Must be converting from XPathNavigator to XPathItem");
                goto SetStorageType;
            }

            // Destination type must be item, so generate code to create an XmlAtomicValue
            this.helper.LoadInteger(this.helper.StaticData.DeclareXmlType(xmlType));
            this.helper.LoadQueryRuntime();
            this.helper.Call(XmlILMethods.StorageMethods[this.storage.ItemStorageType].ToAtomicValue);

        SetStorageType:
            this.storage = this.storage.ToStorageType(storageTypeDest);
        }
    }
}
