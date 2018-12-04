// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using XAML3 = System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Markup;
using System.Diagnostics;
using System.Xaml;
using MS.Internal.Xaml.Runtime;

namespace MS.Internal.Xaml.Context
{
    internal enum FixupType
    {
        MarkupExtensionFirstRun,   // An ME that can't be run because it has pending fixups on its properties
        MarkupExtensionRerun,      // An ME that was run and returned a FixupToken
        PropertyValue,             // A TC on a property that returned a FixupToken
        ObjectInitializationValue, // A TC on an object that returned a FixupToken
        UnresolvedChildren,        // An object that can't be EndInited because it has pending fixups on its properties
    };

    internal class FixupTargetKeyHolder
    {
        public FixupTargetKeyHolder(Object key)
        {
            Key = key;
        }

        public Object Key { get; set; }
    }

    internal class FixupTarget : IAddLineInfo
    {
        /// <summary>
        /// The Property the value will be assigned into.
        /// In the case of FixupType.Property, this property will be set directly
        /// when the Name is known.
        /// </summary>
        public XamlMember Property { get; set; }

        /// <summary>
        /// The Instance the Property is on.
        /// </summary>
        public Object Instance { get; set; }

        /// <summary>
        /// The x:Name, if any, of Instance. May not be set yet if InstanceIsOnTheStack is still true.
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// The XamlType of the Instance (taken from the Frame, not exactly the type of the instance)
        /// </summary>
        public XamlType InstanceType { get; set; }

        /// <summary>
        /// If Property is XamlLanguage.Items, then all the items being added to the target collection
        /// are stored in a temporary holding collection. This is the index of the location in the collection
        /// where this token is stored.
        /// </summary>
        public int TemporaryCollectionIndex { get; set; }

        /// <summary>
        /// The LineNumber to use when calling EndInit on the Target Instance
        /// </summary>
        public int EndInstanceLineNumber { get; set; }

        /// <summary>
        /// The LinePosition to use when calling EndInit on the Target Instance
        /// </summary>
        public int EndInstanceLinePosition { get; set; }

        /// <summary>
        /// The ObjectWriterFrame and the FixupTarget need to both be updated when the Key is changed.
        /// We create a KeyHolder to accomplish this
        /// </summary>
        public FixupTargetKeyHolder KeyHolder { get; set; }

        /// <summary>
        /// Whether Instance is still on the live builder stack. We will use this to determine
        /// whether to call EndInit on it when all fixups are resolved.
        /// </summary>
        public bool InstanceIsOnTheStack { get; set; }

        /// <summary>
        /// Whether Instance was retrieved from a property (i.e. was a GO). If so, we don't call EndInit on it.
        /// </summary>
        public bool InstanceWasGotten { get; set; }

        XamlException IAddLineInfo.WithLineInfo(XamlException ex)
        {
            if (EndInstanceLineNumber > 0)
            {
                ex.SetLineInfo(EndInstanceLineNumber, EndInstanceLinePosition);
            }
            return ex;
        }
    }

    internal class NameFixupToken : IAddLineInfo
    {
        List<string> _names;
        List<INameScopeDictionary> _nameScopeDictionaryList;

        public NameFixupToken()
        {
            _names = new List<string>();
            _nameScopeDictionaryList = new List<INameScopeDictionary>();
            Target = new FixupTarget();
            Target.TemporaryCollectionIndex = -1;
            Target.InstanceIsOnTheStack = true;
        }

        public bool CanAssignDirectly { get; set; }
        public FixupType FixupType { get; set; }

        public int LineNumber { get; set; }
        public int LinePosition { get; set; }

        public FixupTarget Target { get; set; }

        private XamlRuntime _runtime;
        public XamlRuntime Runtime
        {
            get { return _runtime; }
            set
            {
                Debug.Assert(_runtime == null);
                _runtime = value;
            }
        }

        private ObjectWriterContext _targetContext;
        public ObjectWriterContext TargetContext
        {
            get
            {
                if (_targetContext == null)
                {
                    _targetContext = new ObjectWriterContext(SavedContext, null, null, Runtime);
                }
                return _targetContext;
            }
        }

        /// <summary>
        /// Saved state for the reparse option.
        /// </summary>
        public XamlSavedContext SavedContext { get; set; }

        /// <summary>
        /// Saved List of Name Scopes.   With simple fixups we don't have a full context stack.
        /// </summary>
        public List<INameScopeDictionary> NameScopeDictionaryList
        {
            get { return _nameScopeDictionaryList; }
        }
        
        public List<String> NeededNames
        {
            get { return _names; }
        }

        // For simple fixups (CanAssignDirectly), this property is the referenced object when the
        // name is finally resolved.
        // For UnresolvedChildren fixups, this is the object that has
        // unresolved children (which is in turn blocking its parent, Target.Instance, from being
        // fully initialized).
        // For other fixup types, this property is null.
        public object ReferencedObject { get; set; }

        internal object ResolveName(string name)
        {
            object namedObject = null;
            if (CanAssignDirectly)
            {
                foreach (INameScopeDictionary nameScope in NameScopeDictionaryList)
                {
                    namedObject = nameScope.FindName(name);
                    if (namedObject != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                TargetContext.IsInitializedCallback = null;
                bool isFullyInitialized;
                namedObject = TargetContext.ResolveName(name, out isFullyInitialized);
            }
            return namedObject;
        }

        XamlException IAddLineInfo.WithLineInfo(XamlException ex)
        {
            if (LineNumber > 0)
            {
                ex.SetLineInfo(LineNumber, LinePosition);
            }
            return ex;
        }
    }
}
