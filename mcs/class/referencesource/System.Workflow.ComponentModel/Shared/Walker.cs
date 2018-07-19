// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 


/*********************************************************************
 * NOTE: A copy of this file exists at: WF\Activities\Common
 * The two files must be kept in sync.  Any change made here must also
 * be made to WF\Activities\Common\Walker.cs
*********************************************************************/
namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    #endregion

    // Returns true to continue the walk, false to stop.
    internal delegate void WalkerEventHandler(Walker walker, WalkerEventArgs eventArgs);

    internal enum WalkerAction
    {
        Continue = 0,
        Skip = 1,
        Abort = 2
    }
    #region Class WalkerEventArgs

    internal sealed class WalkerEventArgs : EventArgs
    {
        private Activity currentActivity = null;
        private object currentPropertyOwner = null;
        private PropertyInfo currentProperty = null;
        private object currentValue = null;
        private WalkerAction action = WalkerAction.Continue;

        internal WalkerEventArgs(Activity currentActivity)
        {
            this.currentActivity = currentActivity;
            this.currentPropertyOwner = null;
            this.currentProperty = null;
            this.currentValue = null;
        }

        internal WalkerEventArgs(Activity currentActivity, object currentValue, PropertyInfo currentProperty, object currentPropertyOwner)
            : this(currentActivity)
        {
            this.currentPropertyOwner = currentPropertyOwner;
            this.currentProperty = currentProperty;
            this.currentValue = currentValue;
        }

        public WalkerAction Action
        {
            get
            {
                return this.action;
            }
            set
            {
                this.action = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public PropertyInfo CurrentProperty
        {
            get
            {
                return this.currentProperty;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public object CurrentPropertyOwner
        {
            get
            {
                return this.currentPropertyOwner;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public object CurrentValue
        {
            get
            {
                return this.currentValue;
            }
        }

        public Activity CurrentActivity
        {
            get
            {
                return this.currentActivity;
            }
        }
    }

    #endregion

    internal sealed class Walker
    {
        #region Members

        internal event WalkerEventHandler FoundActivity;
        internal event WalkerEventHandler FoundProperty;
        private bool useEnabledActivities = false;

        #endregion

        #region Methods

        public Walker()
            : this(false)
        {
        }

        public Walker(bool useEnabledActivities)
        {
            this.useEnabledActivities = useEnabledActivities;
        }

        public void Walk(Activity seedActivity)
        {
            Walk(seedActivity, true);
        }

        public void Walk(Activity seedActivity, bool walkChildren)
        {
            Queue queue = new Queue();

            queue.Enqueue(seedActivity);
            while (queue.Count > 0)
            {
                Activity activity = queue.Dequeue() as Activity;

                if (FoundActivity != null)
                {
                    WalkerEventArgs args = new WalkerEventArgs(activity);
                    FoundActivity(this, args);
                    if (args.Action == WalkerAction.Abort)
                        return;
                    if (args.Action == WalkerAction.Skip)
                        continue;
                }

                if (FoundProperty != null)
                {
                    if (!WalkProperties(activity))
                        return;
                }

                if (walkChildren && activity is CompositeActivity)
                {
                    if (useEnabledActivities)
                    {
                        foreach (Activity activity2 in Design.Helpers.GetAllEnabledActivities((CompositeActivity)activity))
                            queue.Enqueue(activity2);
                    }
                    else
                    {
                        foreach (Activity activity2 in ((CompositeActivity)activity).Activities)
                            queue.Enqueue(activity2);
                    }
                }
            }
        }

        private bool WalkProperties(Activity seedActivity)
        {
            return WalkProperties(seedActivity as Activity, seedActivity);
        }

        public bool WalkProperties(Activity activity, object obj)
        {
            Activity currentActivity = obj as Activity;

            PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props)
            {
                // !!Work around: no indexer property walking
                if (prop.GetIndexParameters() != null && prop.GetIndexParameters().Length > 0)
                    continue;

                DesignerSerializationVisibility visibility = GetSerializationVisibility(prop);
                if (visibility == DesignerSerializationVisibility.Hidden)
                    continue;

                //Try to see if we have dynamic property associated with the object on the same object
                //if so then we should compare if the dynamic property values match with the property type
                //if not we bail out
                object propValue = null;
                DependencyProperty dependencyProperty = DependencyProperty.FromName(prop.Name, obj.GetType());
                if (dependencyProperty != null && currentActivity != null)
                {
                    if (currentActivity.IsBindingSet(dependencyProperty))
                        propValue = currentActivity.GetBinding(dependencyProperty);
                    else
                        propValue = currentActivity.GetValue(dependencyProperty);
                }
                else
                {
                    try
                    {
                        propValue = prop.CanRead ? prop.GetValue(obj, null) : null;
                    }
                    catch
                    {
                        // Eat exceptions that occur while invoking the getter.
                    }
                }

                if (FoundProperty != null)
                {
                    WalkerEventArgs args = new WalkerEventArgs(activity, propValue, prop, obj);
                    FoundProperty(this, args);
                    if (args.Action == WalkerAction.Skip)
                        continue;
                    else if (args.Action == WalkerAction.Abort)
                        return false;
                }

                if (propValue is IList)
                {
                    //We do not need to reflect on the properties of the list
                    foreach (object childObj in (IList)propValue)
                    {
                        if (FoundProperty != null)
                        {
                            WalkerEventArgs args = new WalkerEventArgs(activity, childObj, null, propValue);
                            FoundProperty(this, args);
                            if (args.Action == WalkerAction.Skip)
                                continue;
                            else if (args.Action == WalkerAction.Abort)
                                return false;
                        }
                        if (childObj != null && IsBrowsableType(childObj.GetType()))
                        {
                            if (!WalkProperties(activity, childObj))
                                return false;
                        }
                    }
                }
                else if (propValue != null && IsBrowsableType(propValue.GetType()))
                {
                    if (!WalkProperties(activity, propValue))
                        return false;
                }
            }
            return true;
        }

        private static DesignerSerializationVisibility GetSerializationVisibility(PropertyInfo prop)
        {
            // work around!!! for Activities collection
            if (prop.DeclaringType == typeof(CompositeActivity) && string.Equals(prop.Name, "Activities", StringComparison.Ordinal))
                return DesignerSerializationVisibility.Hidden;

            DesignerSerializationVisibility visibility = DesignerSerializationVisibility.Visible;
            DesignerSerializationVisibilityAttribute[] visibilityAttrs = (DesignerSerializationVisibilityAttribute[])prop.GetCustomAttributes(typeof(DesignerSerializationVisibilityAttribute), true);
            if (visibilityAttrs.Length > 0)
                visibility = visibilityAttrs[0].Visibility;

            return visibility;
        }

        private static bool IsBrowsableType(Type type)
        {
            bool browsable = false;
            BrowsableAttribute[] browsableAttrs = (BrowsableAttribute[])type.GetCustomAttributes(typeof(BrowsableAttribute), true);
            if (browsableAttrs.Length > 0)
                browsable = browsableAttrs[0].Browsable;
            return browsable;
        }
        #endregion
    }
}
