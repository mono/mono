//---------------------------------------------------------------------------
//
// <copyright file="NotifyCollectionChangedEventArgs.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description: NotifyCollectionChanged event arguments
//
// Specs:       http://avalon/connecteddata/Specs/INotifyCollectionChanged.mht
//
//---------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace System.Collections.Specialized
{
    /// <summary>
    /// This enum describes the action that caused a CollectionChanged event.
    /// </summary>
#if !MOBILE
    [TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
    public enum NotifyCollectionChangedAction
    {
        /// <summary> One or more items were added to the collection. </summary>
        Add,
        /// <summary> One or more items were removed from the collection. </summary>
        Remove,
        /// <summary> One or more items were replaced in the collection. </summary>
        Replace,
        /// <summary> One or more items were moved within the collection. </summary>
        Move,
        /// <summary> The contents of the collection changed dramatically. </summary>
        Reset,
    }

    /// <summary>
    /// Arguments for the CollectionChanged event.
    /// A collection that supports INotifyCollectionChangedThis raises this event
    /// whenever an item is added or removed, or when the contents of the collection
    /// changes dramatically.
    /// </summary>
#if !MOBILE
    [TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
    public class NotifyCollectionChangedEventArgs : EventArgs
    {
        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a reset change.
        /// </summary>
        /// <param name="action">The action that caused the event (must be Reset).</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            if (action != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException(SR.GetString(SR.WrongActionForCtor, NotifyCollectionChangedAction.Reset), "action");

            InitializeAdd(action, null, -1);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event; can only be Reset, Add or Remove action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem)
        {
            if ((action != NotifyCollectionChangedAction.Add) && (action != NotifyCollectionChangedAction.Remove)
                    && (action != NotifyCollectionChangedAction.Reset))
                throw new ArgumentException(SR.GetString(SR.MustBeResetAddOrRemoveActionForCtor), "action");

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                    throw new ArgumentException(SR.GetString(SR.ResetActionRequiresNullItem), "action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                InitializeAddOrRemove(action, new object[]{changedItem}, -1);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The index where the change occurred.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index)
        {
            if ((action != NotifyCollectionChangedAction.Add) && (action != NotifyCollectionChangedAction.Remove)
                    && (action != NotifyCollectionChangedAction.Reset))
                throw new ArgumentException(SR.GetString(SR.MustBeResetAddOrRemoveActionForCtor), "action");

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                    throw new ArgumentException(SR.GetString(SR.ResetActionRequiresNullItem), "action");
                if (index != -1)
                    throw new ArgumentException(SR.GetString(SR.ResetActionRequiresIndexMinus1), "action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                InitializeAddOrRemove(action, new object[]{changedItem}, index);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems)
        {
            if ((action != NotifyCollectionChangedAction.Add) && (action != NotifyCollectionChangedAction.Remove)
                    && (action != NotifyCollectionChangedAction.Reset))
                throw new ArgumentException(SR.GetString(SR.MustBeResetAddOrRemoveActionForCtor), "action");

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                    throw new ArgumentException(SR.GetString(SR.ResetActionRequiresNullItem), "action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                    throw new ArgumentNullException("changedItems");

                InitializeAddOrRemove(action, changedItems, -1);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change (or a reset).
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="startingIndex">The index where the change occurred.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if ((action != NotifyCollectionChangedAction.Add) && (action != NotifyCollectionChangedAction.Remove)
                    && (action != NotifyCollectionChangedAction.Reset))
                throw new ArgumentException(SR.GetString(SR.MustBeResetAddOrRemoveActionForCtor), "action");

            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                    throw new ArgumentException(SR.GetString(SR.ResetActionRequiresNullItem), "action");
                if (startingIndex != -1)
                    throw new ArgumentException(SR.GetString(SR.ResetActionRequiresIndexMinus1), "action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                    throw new ArgumentNullException("changedItems");
                if (startingIndex < -1)
                    throw new ArgumentException(SR.GetString(SR.IndexCannotBeNegative), "startingIndex");

                InitializeAddOrRemove(action, changedItems, startingIndex);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException(SR.GetString(SR.WrongActionForCtor, NotifyCollectionChangedAction.Replace), "action");

            InitializeMoveOrReplace(action, new object[]{newItem}, new object[]{oldItem}, -1, -1);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        /// <param name="index">The index of the item being replaced.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException(SR.GetString(SR.WrongActionForCtor, NotifyCollectionChangedAction.Replace), "action");

            int oldStartingIndex = index;

#if FEATURE_LEGACYNETCF
            if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8)
            {
                // Dev11 444113 quirk:
                // This is a "Replace" so the old and new index should both be set to the index passed in however
                // NetCF on Mango incorrectly leaves OldStartingIndex at -1 and Mango apps depend on this behavior.
                oldStartingIndex = -1;
            }
#endif
            InitializeMoveOrReplace(action, new object[]{newItem}, new object[]{oldItem}, index, oldStartingIndex);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException(SR.GetString(SR.WrongActionForCtor, NotifyCollectionChangedAction.Replace), "action");
            if (newItems == null)
                throw new ArgumentNullException("newItems");
            if (oldItems == null)
                throw new ArgumentNullException("oldItems");

            InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        /// <param name="startingIndex">The starting index of the items being replaced.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException(SR.GetString(SR.WrongActionForCtor, NotifyCollectionChangedAction.Replace), "action");
            if (newItems == null)
                throw new ArgumentNullException("newItems");
            if (oldItems == null)
                throw new ArgumentNullException("oldItems");

            InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Move event.
        /// </summary>
        /// <param name="action">Can only be a Move action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The new index for the changed item.</param>
        /// <param name="oldIndex">The old index for the changed item.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
        {
            if (action != NotifyCollectionChangedAction.Move)
                throw new ArgumentException(SR.GetString(SR.WrongActionForCtor, NotifyCollectionChangedAction.Move), "action");
            if (index < 0)
                throw new ArgumentException(SR.GetString(SR.IndexCannotBeNegative), "index");

            object[] changedItems= new object[] {changedItem};
            InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Move event.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="index">The new index for the changed items.</param>
        /// <param name="oldIndex">The old index for the changed items.</param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex)
        {
            if (action != NotifyCollectionChangedAction.Move)
                throw new ArgumentException(SR.GetString(SR.WrongActionForCtor, NotifyCollectionChangedAction.Move), "action");
            if (index < 0)
                throw new ArgumentException(SR.GetString(SR.IndexCannotBeNegative), "index");

            InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs with given fields (no validation). Used by WinRT marshaling.
        /// </summary>
        internal NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int newIndex, int oldIndex)
        {
            _action = action;
#if FEATURE_LEGACYNETCF
            if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8)
            {
                _newItems = newItems;
            }
            else
#endif
            {
                _newItems = (newItems == null) ? null : ArrayList.ReadOnly(newItems);
            }
            _oldItems = (oldItems == null) ? null : ArrayList.ReadOnly(oldItems);
            _newStartingIndex = newIndex;
            _oldStartingIndex = oldIndex;
        }

        private void InitializeAddOrRemove(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if (action == NotifyCollectionChangedAction.Add)
                InitializeAdd(action, changedItems, startingIndex);
            else if (action == NotifyCollectionChangedAction.Remove)
                InitializeRemove(action, changedItems, startingIndex);
            else
                Contract.Assert(false, String.Format("Unsupported action: {0}", action.ToString()));
        }

        private void InitializeAdd(NotifyCollectionChangedAction action, IList newItems, int newStartingIndex)
        {
            _action = action;
#if FEATURE_LEGACYNETCF
            if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8)
            {
                _newItems = newItems;
            }
            else
#endif // !FEATURE_LEGACYNETCF
            {
                _newItems = (newItems == null) ? null : ArrayList.ReadOnly(newItems);
            }
            _newStartingIndex = newStartingIndex;
        }

        private void InitializeRemove(NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex)
        {
            _action = action;
            _oldItems = (oldItems == null) ? null : ArrayList.ReadOnly(oldItems);
            _oldStartingIndex= oldStartingIndex;
        }

        private void InitializeMoveOrReplace(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex, int oldStartingIndex)
        {
            InitializeAdd(action, newItems, startingIndex);
            InitializeRemove(action, oldItems, oldStartingIndex);
        }

        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        /// <summary>
        /// The action that caused the event.
        /// </summary>
        public NotifyCollectionChangedAction Action
        {
            get { return _action; }
        }

        /// <summary>
        /// The items affected by the change.
        /// </summary>
        public IList NewItems
        {
            get { return _newItems; }
        }

        /// <summary>
        /// The old items affected by the change (for Replace events).
        /// </summary>
        public IList OldItems
        {
            get { return _oldItems; }
        }

        /// <summary>
        /// The index where the change occurred.
        /// </summary>
        public int NewStartingIndex
        {
            get { return _newStartingIndex; }
        }

        /// <summary>
        /// The old index where the change occurred (for Move events).
        /// </summary>
        public int OldStartingIndex
        {
            get { return _oldStartingIndex; }
        }

        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        private NotifyCollectionChangedAction _action;
        private IList _newItems, _oldItems;
        private int _newStartingIndex = -1;
        private int _oldStartingIndex = -1;
    }

    /// <summary>
    ///     The delegate to use for handlers that receive the CollectionChanged event.
    /// </summary>
#if !MOBILE
    [TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
    public delegate void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e);

}


