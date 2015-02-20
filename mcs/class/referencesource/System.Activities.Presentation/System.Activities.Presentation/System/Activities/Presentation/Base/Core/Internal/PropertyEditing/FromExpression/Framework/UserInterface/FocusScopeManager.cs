// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\UserInterface
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.UserInterface
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Controls.Primitives;
    using System.Windows.Threading;
    using System.Windows.Media;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;

    internal delegate void ReturnFocusCallback();

    internal class FocusScopeManager
    {
        public static readonly DependencyProperty FocusScopePriorityProperty;
        public static readonly DependencyProperty AllowedFocusProperty = DependencyProperty.RegisterAttached("AllowedFocus", typeof(bool), typeof(FocusScopeManager), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(FocusScopeManager.AllowedFocusChanged)));

        private static int DefaultFocusScopePriority;

        private static FocusScopeManager instance;

        private bool listContainsDeadReferences = false;
        private List<WeakReference> scopes = new List<WeakReference>();
        // Has a value when a managed focus scope has keyboard focus within it. Null otherwise
        private WeakReference activeManagedFocusScope = null;

        private bool denyNextFocusChange = false;
        // Only set to true if we are handling a mouse event while we are denyNextFocusChange is true
        private bool handlingPointerButtonEvent = false;

        ReturnFocusCallback returnFocusCallback;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static FocusScopeManager()
        {
            // Must do this explicitly because the default value of the FocusScopePriorityProperty depends on DefaultFocusScopePriority being properly initialized.
            FocusScopeManager.DefaultFocusScopePriority = Int32.MaxValue;
            FocusScopeManager.FocusScopePriorityProperty = DependencyProperty.RegisterAttached("FocusScopePriority", typeof(int), typeof(FocusScopeManager), new FrameworkPropertyMetadata(FocusScopeManager.DefaultFocusScopePriority, new PropertyChangedCallback(FocusScopeManager.FocusScopePriorityChanged)));
            FocusManager.FocusedElementProperty.OverrideMetadata(typeof(FrameworkElement), new PropertyMetadata(null, null, new CoerceValueCallback(FocusScopeManager.FocusManager_CoerceFocusedElement)));
        }

        private FocusScopeManager()
        {
        }

        public static FocusScopeManager Instance
        {
            get
            {
                if (FocusScopeManager.instance == null)
                {
                    FocusScopeManager.instance = new FocusScopeManager();
                    EventManager.RegisterClassHandler(typeof(Window), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(FocusScopeManager.HandleGotKeyboardFocusEvent), true);
                    EventManager.RegisterClassHandler(typeof(Popup), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(FocusScopeManager.HandleGotKeyboardFocusEvent), true);

                    EventManager.RegisterClassHandler(typeof(Window), Keyboard.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(FocusScopeManager.HandlePreviewGotKeyboardFocus), true);
                    EventManager.RegisterClassHandler(typeof(Popup), Keyboard.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(FocusScopeManager.HandlePreviewGotKeyboardFocus), true);
                }
                return FocusScopeManager.instance;
            }
        }

        public static bool HasInstance
        {
            get
            {
                return FocusScopeManager.instance != null;
            }
        }

        // <summary>
        // Sets the callback that will be called when the FocusScopeManager
        // wants to return focus from a control that should not have it.
        // Clients can use this to override the default behavior here which
        // is to set keyboard focus to null.
        // </summary>
        public ReturnFocusCallback ReturnFocusCallback
        {
            get { return this.returnFocusCallback; }
            set
            {
                Fx.Assert(this.returnFocusCallback == null, "Cannot set the ReturnFocusCallback more than once per Process");
                if (this.returnFocusCallback == null)
                {
                    this.returnFocusCallback = value;
                }
            }
        }

        private UIElement ActiveManagedFocusScope
        {
            get
            {
                if (this.activeManagedFocusScope != null && this.activeManagedFocusScope.IsAlive)
                {
                    return this.activeManagedFocusScope.Target as UIElement;
                }
                return null;
            }
            set
            {
                if (this.activeManagedFocusScope == null)
                {
                    this.activeManagedFocusScope = new WeakReference(value);
                }
                else
                {
                    this.activeManagedFocusScope.Target = value;
                }
            }
        }

        private bool ShouldDenyFocusChange
        {
            get
            {
                return this.denyNextFocusChange && !this.handlingPointerButtonEvent;
            }
        }

        private static void HandleGotKeyboardFocusEvent(object sender, KeyboardFocusChangedEventArgs e)
        {
            SetFocusToFocusScope(e.NewFocus);
        }

        private static void HandlePreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (FocusScopeManager.Instance.ShouldDenyFocusChange)
            {
                e.Handled = true;
                return;
            }

            DependencyObject newFocusObject = e.NewFocus as DependencyObject;
            // if there is a newly focused object and it is not allowed focus
            if (newFocusObject != null && !FocusScopeManager.GetAllowedFocus(newFocusObject))
            {
                // Push focus back to the client's desired focus sink and cancel the change
                // of focus to new focus.
                FocusScopeManager.Instance.ReturnFocus();
                e.Handled = true;
            }
        }

        public static void SetFocusToFocusScope(IInputElement newFocus)
        {
            UIElement newElementFocus = newFocus as UIElement;
            if (newElementFocus != null)
            {
                UIElement focusScope = FocusManager.GetFocusScope(newElementFocus) as UIElement;
                int priority = FocusScopeManager.GetFocusScopePriority(focusScope);
                if (priority != FocusScopeManager.DefaultFocusScopePriority)
                {
                    FocusScopeManager.Instance.OnScopeKeyboardFocusChanged(focusScope, priority);
                }
            }
        }

        public static void DenyNextFocusChange()
        {
            FocusScopeManager.Instance.denyNextFocusChange = true;
            InputManager.Current.PreNotifyInput += new NotifyInputEventHandler(FocusScopeManager.Instance.InputManager_PreNotifyInput);
            InputManager.Current.PostNotifyInput += new NotifyInputEventHandler(FocusScopeManager.Instance.InputManager_PostNotifyInput);

            UIThreadDispatcher.Instance.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate
            {
                // guard against the focus Scope manager going null.
                if (FocusScopeManager.HasInstance)
                {
                    FocusScopeManager.Instance.EndDenyNextFocusChange();
                }
                return null;
            }
                ), null);
        }

        private void EndDenyNextFocusChange()
        {
            this.denyNextFocusChange = false;
            this.handlingPointerButtonEvent = false;
            InputManager.Current.PreNotifyInput -= new NotifyInputEventHandler(this.InputManager_PreNotifyInput);
            InputManager.Current.PostNotifyInput -= new NotifyInputEventHandler(this.InputManager_PostNotifyInput);
        }

        private bool IsPointerButtonEventItem(StagingAreaInputItem stagingItem)
        {
            return stagingItem != null && (stagingItem.Input as MouseButtonEventArgs != null || stagingItem.Input as StylusButtonEventArgs != null);
        }

        private void InputManager_PreNotifyInput(object sender, NotifyInputEventArgs e)
        {
            if (this.IsPointerButtonEventItem(e.StagingItem))
            {
                this.handlingPointerButtonEvent = true;
            }
        }

        private void InputManager_PostNotifyInput(object sender, NotifyInputEventArgs e)
        {
            if (this.IsPointerButtonEventItem(e.StagingItem))
            {
                this.handlingPointerButtonEvent = false;
            }
        }

        public static void SetFocusScopePriority(DependencyObject element, int value)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            if (!FocusManager.GetIsFocusScope(element))
            {
                throw FxTrace.Exception.AsError(new ArgumentException(ExceptionStringTable.CanOnlySetFocusScopePriorityOnAnElementThatIsAFocusScope));
            }
            element.SetValue(FocusScopeManager.FocusScopePriorityProperty, value);
        }

        public static int GetFocusScopePriority(DependencyObject element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            return (int)element.GetValue(FocusScopeManager.FocusScopePriorityProperty);
        }

        public static void SetAllowedFocus(DependencyObject element, bool value)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            element.SetValue(FocusScopeManager.AllowedFocusProperty, value);
        }

        public static bool GetAllowedFocus(DependencyObject element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            return (bool)element.GetValue(FocusScopeManager.AllowedFocusProperty);
        }

        private static void FocusScopePriorityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FocusScopeManager.Instance.UpdateFocusScopePriorityForElement(d as UIElement, (int)e.OldValue, (int)e.NewValue);
        }

        private static object FocusManager_CoerceFocusedElement(DependencyObject d, object value)
        {
            UIElement scope = d as UIElement;
            if (scope != null)
            {
                return FocusScopeManager.Instance.CoerceFocusedElement(value);
            }
            return value;
        }

        private static void AllowedFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                bool newValue = (bool)e.NewValue;
                FocusScopeManager.Instance.OnAllowedFocusChanged(element, newValue);
            }
        }

        private void UpdateFocusScopePriorityForElement(UIElement element, int oldValue, int newValue)
        {
            // If we had an index before
            if (oldValue != FocusScopeManager.DefaultFocusScopePriority)
            {
                this.RemoveScope(element);
            }

            // If we now have an index
            if (newValue != FocusScopeManager.DefaultFocusScopePriority)
            {
                int startIndex = this.FindStartIndexForPriority(newValue);
                // No ordering within priorities
                this.InsertScope(startIndex, element);
            }
        }

        private object CoerceFocusedElement(object newFocus)
        {
            UIElement newFocusElement = newFocus as UIElement;
            if (newFocusElement != null)
            {
                // If we have been told to deny the next focus change then do so
                if (this.ShouldDenyFocusChange)
                {
                    this.EndDenyNextFocusChange();
                    return DependencyProperty.UnsetValue;
                }

                // Don't allow logical focus to go to elements that are not focusable.
                if (newFocus != null && !FocusScopeManager.GetAllowedFocus(newFocusElement))
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            return newFocus;
        }

        private void OnAllowedFocusChanged(FrameworkElement element, bool newValue)
        {
            if (newValue == false && element.IsKeyboardFocused)
            {
                this.ReturnFocus();
            }
        }

        private void OnScopeKeyboardFocusChanged(UIElement focusScope, int priority)
        {
            if (focusScope != this.ActiveManagedFocusScope)
            {
                // Make sure that the passed focus scope is managed

                int startIndex = this.FindStartIndexForPriority(priority);

                if (!this.IsFocusScopeManaged(focusScope, priority, startIndex))
                {
                    return;
                }
                for (int curScopeIndex = startIndex; curScopeIndex < this.scopes.Count; curScopeIndex++)
                {
                    WeakReference curRef = this.scopes[curScopeIndex];
                    if (curRef.IsAlive)
                    {
                        UIElement curScope = (UIElement)curRef.Target;
                        if (curScope != focusScope)
                        {
                            FocusManager.SetFocusedElement(curScope, null);
                        }
                    }
                    else
                    {
                        this.listContainsDeadReferences = true;
                    }
                }

                this.CleanUpDeadReferences();

                this.ActiveManagedFocusScope = focusScope;
            }
        }

        private bool IsFocusScopeManaged(UIElement focusScope, int priority, int priorityStartIndex)
        {
            for (int curScopeIndex = priorityStartIndex; curScopeIndex < this.scopes.Count; curScopeIndex++)
            {
                WeakReference curRef = this.scopes[curScopeIndex];
                if (curRef.IsAlive)
                {
                    UIElement curTarget = (UIElement)curRef.Target;
                    if (FocusScopeManager.GetFocusScopePriority(curTarget) > priority)
                    {
                        return false;
                    }
                    if (curTarget == focusScope)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void CleanUpDeadReferences()
        {
            if (this.listContainsDeadReferences)
            {
                for (int curRefIndex = this.scopes.Count - 1; curRefIndex >= 0; curRefIndex--)
                {
                    if (!this.scopes[curRefIndex].IsAlive)
                    {
                        this.scopes.RemoveAt(curRefIndex);
                    }
                }
                this.listContainsDeadReferences = false;
            }
        }

        private int FindStartIndexForPriority(int priority)
        {
            int startIndex = 0;
            for (; startIndex < this.scopes.Count; startIndex++)
            {
                WeakReference curRef = this.scopes[startIndex];
                if (curRef.IsAlive)
                {
                    if (FocusScopeManager.GetFocusScopePriority((UIElement)curRef.Target) >= priority)
                    {
                        break;
                    }
                }
                else
                {
                    this.listContainsDeadReferences = true;
                }
            }

            return startIndex;
        }

        private void InsertScope(int index, UIElement scope)
        {
            this.scopes.Insert(index, new WeakReference(scope));
        }

        private void RemoveScope(UIElement scope)
        {
            for (int curScopeIndex = this.scopes.Count - 1; curScopeIndex >= 0; curScopeIndex--)
            {
                WeakReference curRef = this.scopes[curScopeIndex];
                if (!curRef.IsAlive || curRef.Target == scope)
                {
                    this.scopes.RemoveAt(curScopeIndex);
                }
            }
        }

        private void ReturnFocus()
        {
            if (this.ReturnFocusCallback != null)
            {
                this.ReturnFocusCallback();
            }
            else
            {
                Keyboard.Focus(null);
            }
        }
    }
}
