// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Xaml;
using XAML3 = System.Windows.Markup;
using System.Xaml.Schema;
using MS.Internal.Xaml.Parser;
using System.Xaml.MS.Impl;
using System.Diagnostics;

namespace MS.Internal.Xaml.Context
{
    internal class XamlParserContext : XamlContext
    {
        private XamlContextStack<XamlParserFrame> _stack;
        private Dictionary<string, string> _prescopeNamespaces;

        public bool AllowProtectedMembersOnRoot { get; set; }

        public Func<string, string> XmlNamespaceResolver { get; set; }

        public XamlParserContext(XamlSchemaContext schemaContext, Assembly localAssembly)
            :base(schemaContext)
        {
            _stack = new XamlContextStack<XamlParserFrame>(()=>new XamlParserFrame());
            _prescopeNamespaces = new Dictionary<string, string>();
            base._localAssembly = localAssembly;
        }

        // -----  abstracts overriden from XamlContext.

        public override void AddNamespacePrefix(String prefix, string xamlNS)
        {
            _prescopeNamespaces.Add(prefix, xamlNS);
        }

        public string FindNamespaceByPrefixInParseStack(String prefix)
        {
            string xamlNs;

            if (null != _prescopeNamespaces)
            {
                if (_prescopeNamespaces.TryGetValue(prefix, out xamlNs))
                {
                    return xamlNs;
                }
            }

            XamlParserFrame frame = _stack.CurrentFrame;
            while (frame.Depth > 0)
            {
                if (frame.TryGetNamespaceByPrefix(prefix, out xamlNs))
                {
                    return xamlNs;
                }
                frame = (XamlParserFrame)frame.Previous;
            }
            return null;
        }

        public override string FindNamespaceByPrefix(String prefix)
        {
            // For proper operation of some corner senarios the XmlNamespaceResolver
            // must be set.   But if it isn't we fall back to a look in our own stack.
            // Senarios that REQUIRE an XmlNamespaceResolver
            //  1) Prefix defs that ONLY exist in the XmlNsManager are only found this way
            //  2) Prefix defs on MarkupCompat Tags have effect but don't appear
            //     in the Xml node stream the XAML parser sees.
            // But for normal XAML the XmlNamespaceResolver does not need to be used.

            if (XmlNamespaceResolver != null)
            {
                return XmlNamespaceResolver(prefix);
            }
            return FindNamespaceByPrefixInParseStack(prefix);
        }

        public override IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            XamlParserFrame frame = _stack.CurrentFrame;
            Dictionary<string, string> keys = new Dictionary<string, string>();
            while (frame.Depth > 0)
            {
                if (frame._namespaces != null)
                {
                    foreach (NamespaceDeclaration namespaceDeclaration in frame.GetNamespacePrefixes())
                    {
                        if (!keys.ContainsKey(namespaceDeclaration.Prefix))
                        {
                            keys.Add(namespaceDeclaration.Prefix, null);
                            yield return namespaceDeclaration;
                        }
                    }
                }
                frame = (XamlParserFrame)frame.Previous;
            }

            if (_prescopeNamespaces != null)
            {
                foreach (KeyValuePair<string, string> kvp in _prescopeNamespaces)
                {
                    if (!keys.ContainsKey(kvp.Key))
                    {
                        keys.Add(kvp.Key, null);
                        yield return new NamespaceDeclaration(kvp.Value, kvp.Key);
                    }
                }
            }
        }

        // Only pass rootObjectType if the member is being looked up on the root object
        internal override bool IsVisible(XamlMember member, XamlType rootObjectType)
        {
            if (member == null)
            {
                return false;
            }

            Type allowProtectedForType = null;
            if (AllowProtectedMembersOnRoot && rootObjectType != null)
            {
                allowProtectedForType = rootObjectType.UnderlyingType;
            }

            // First check if the property setter is visible
            if (member.IsWriteVisibleTo(LocalAssembly, allowProtectedForType))
            {
                return true;
            }
            
            // If the property setter is not visible, but the property getter is, treat the property
            // as if it were read-only
            if (member.IsReadOnly || (member.Type != null && member.Type.IsUsableAsReadOnly))
            {
                return member.IsReadVisibleTo(LocalAssembly, allowProtectedForType);
            }

            return false;
        }

        // ----- new public methods.

        public void PushScope()
        {
            _stack.PushScope();
            if (_prescopeNamespaces.Count > 0)
            {
                _stack.CurrentFrame.SetNamespaces(_prescopeNamespaces);
                _prescopeNamespaces = new Dictionary<string, string>();
            }
        }

        public void PopScope()
        {
            _stack.PopScope();
        }

        internal void InitBracketCharacterCacheForType(XamlType extensionType)
        {
            CurrentEscapeCharacterMapForMarkupExtension = SchemaContext.InitBracketCharacterCacheForType(extensionType);
        }

        /// <summary>
        /// Finds the list of parameters of the constructor with the most number
        /// of arguments.
        /// </summary>
        internal void InitLongestConstructor(XamlType xamlType)
        {
            IEnumerable<ConstructorInfo> constructors = xamlType.GetConstructors();
            ParameterInfo[] constructorParameters = null;
            int parameterCount = 0;
            foreach (ConstructorInfo ctr in constructors)
            {
                ParameterInfo[] parInfo = ctr.GetParameters();
                if (parInfo.Length >= parameterCount)
                {
                    parameterCount = parInfo.Length;
                    constructorParameters = parInfo;
                }
            }

            CurrentLongestConstructorOfMarkupExtension = constructorParameters;
        }

        // FxCop says this is never called
        //public int Depth
        //{
        //    get { return _stack.Depth; }
        //}

        public XamlType CurrentType
        {
            get { return _stack.CurrentFrame.XamlType; }
            set { _stack.CurrentFrame.XamlType = value; }
        }

        internal BracketModeParseParameters CurrentBracketModeParseParameters
        {
            get { return _stack.CurrentFrame.BracketModeParseParameters; }
            set { _stack.CurrentFrame.BracketModeParseParameters = value; }
        }

        internal ParameterInfo[] CurrentLongestConstructorOfMarkupExtension
        {
            get { return _stack.CurrentFrame.LongestConstructorOfCurrentMarkupExtensionType; }
            set { _stack.CurrentFrame.LongestConstructorOfCurrentMarkupExtensionType = value; }
        }

        internal Dictionary<string, SpecialBracketCharacters> CurrentEscapeCharacterMapForMarkupExtension
        {
            get { return _stack.CurrentFrame.EscapeCharacterMapForMarkupExtension; }
            set { _stack.CurrentFrame.EscapeCharacterMapForMarkupExtension = value; }
        }

        // This property refers to a namespace specified explicitly in the XAML document.
        // If this is an implicit type (e.g. GO, x:Data, implicit array) just leave it as null.
        public string CurrentTypeNamespace
        {
            get { return _stack.CurrentFrame.TypeNamespace; }
            set { _stack.CurrentFrame.TypeNamespace = value; }
        }

        public bool CurrentInContainerDirective
        {
            get { return _stack.CurrentFrame.InContainerDirective; }
            set { _stack.CurrentFrame.InContainerDirective = value; }
        }

        // FxCop says this is not called
        //public XamlType ParentType
        //{
        //    get { return _stack.PreviousFrame.XamlType; }
        //}

        public XamlMember CurrentMember
        {
            get { return _stack.CurrentFrame.Member; }
            set { _stack.CurrentFrame.Member = value; }
        }

        // FxCop says this is not called
        //public XamlProperty MemberProperty
        //{
        //    get { return _stack.PreviousFrame.Member; }
        //}

        public int CurrentArgCount
        {
            get { return _stack.CurrentFrame.CtorArgCount; }
            set { _stack.CurrentFrame.CtorArgCount = value; }
        }

        public bool CurrentForcedToUseConstructor
        {
            get { return _stack.CurrentFrame.ForcedToUseConstructor; }
            set { _stack.CurrentFrame.ForcedToUseConstructor = value; }
        }

        public bool CurrentInItemsProperty
        {
            get { return _stack.CurrentFrame.Member == XamlLanguage.Items; }
        }

        public bool CurrentInInitProperty
        {
            get { return _stack.CurrentFrame.Member == XamlLanguage.Initialization; }
        }

        public bool CurrentInUnknownContent
        {
            get { return _stack.CurrentFrame.Member == XamlLanguage.UnknownContent; }
        }

        public bool CurrentInImplicitArray
        {
            get { return _stack.CurrentFrame.InImplicitArray; }
            set { _stack.CurrentFrame.InImplicitArray = value; }
        }

        public bool CurrentInCollectionFromMember
        {
            get { return _stack.CurrentFrame.InCollectionFromMember; }
            set { _stack.CurrentFrame.InCollectionFromMember = value; }
        }

        public XamlType CurrentPreviousChildType
        {
            get { return _stack.CurrentFrame.PreviousChildType; }
            set { _stack.CurrentFrame.PreviousChildType = value; }
        }

        public bool CurrentMemberIsWriteVisible()
        {
            Type allowProtectedForType = null;
            if (AllowProtectedMembersOnRoot && _stack.Depth == 1)
            {
                allowProtectedForType = CurrentType.UnderlyingType;
            }
            return CurrentMember.IsWriteVisibleTo(LocalAssembly, allowProtectedForType);
        }

        public bool CurrentTypeIsRoot
        {
            get { return _stack.CurrentFrame.XamlType != null && _stack.Depth == 1; }
        }
    }
}
