// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xaml;
using System.Xaml.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace MS.Internal.Xaml.Parser
{
    class GenericTypeNameParser
    {
        [Serializable]
        class TypeNameParserException : Exception
        {
            public TypeNameParserException(string message)
                : base(message)
            {
            }

            protected TypeNameParserException(SerializationInfo si, StreamingContext sc) : base(si, sc)
            {
            }
        }

        private GenericTypeNameScanner _scanner;
        private string _inputText;
        private Func<string, string> _prefixResolver;
        Stack<TypeNameFrame> _stack;

        public GenericTypeNameParser(Func<string, string> prefixResolver)
        {
            _prefixResolver = prefixResolver;
        }

        public static XamlTypeName ParseIfTrivalName(string text, Func<string, string> prefixResolver, out string error)
        {
            int parenIdx = text.IndexOf('(');
            int bracketIdx = text.IndexOf('[');
            if (parenIdx != -1 || bracketIdx != -1)
            {
                error = String.Empty;
                return null;
            }

            string prefix;
            string simpleName;

            error = String.Empty;
            if (!XamlQualifiedName.Parse(text, out prefix, out simpleName))
            {
                error = SR.Get(SRID.InvalidTypeString, text);
                return null;
            }

            string ns = prefixResolver(prefix);
            if (String.IsNullOrEmpty(ns))
            {
                error = SR.Get(SRID.PrefixNotFound, prefix);
                return null;
            }
            XamlTypeName xamlTypeName = new XamlTypeName(ns, simpleName);
            return xamlTypeName;
        }

        public XamlTypeName ParseName(string text, out string error)
        {
            error = String.Empty;
            _scanner = new GenericTypeNameScanner(text);
            _inputText = text;

            StartStack();

            try
            {
                _scanner.Read();
                P_XamlTypeName();
                if (_scanner.Token != GenericTypeNameScannerToken.NONE)
                {
                    ThrowOnBadInput();
                }
            }
            catch (TypeNameParserException ex)
            {
                error = ex.Message;
            }

            XamlTypeName typeName = null;
            if (String.IsNullOrEmpty(error))
            {
                typeName = CollectNameFromStack();
            }
            return typeName;
        }

        public IList<XamlTypeName> ParseList(string text, out string error)
        {
            _scanner = new GenericTypeNameScanner(text);
            _inputText = text;
            StartStack();

            error = String.Empty;
            try
            {
                _scanner.Read();
                P_XamlTypeNameList();
                if (_scanner.Token != GenericTypeNameScannerToken.NONE)
                {
                    ThrowOnBadInput();
                }
            }
            catch (TypeNameParserException ex)
            {
                error = ex.Message;
            }

            IList<XamlTypeName> typeNameList = null;
            if (String.IsNullOrEmpty(error))
            {
                typeNameList = CollectNameListFromStack();
            }
            return typeNameList;
        }

        // XamlTypeName     ::= SimpleTypeName TypeParameters? Subscript*
        // SimpleTypeName   ::= (Prefix ‘:’)? TypeName
        // TypeParameters   ::= ‘(‘ XamlTypeNameList ‘)’
        // XamlTypeNameList ::= XamlTypeName NameListExt*
        // NameListExt      ::= ‘,’ XamlTypeName
        // Subscript        ::= ‘[’ ‘,’* ‘]’

        // XamlTypeName     ::= SimpleTypeName TypeParameters? Subscript*
        //
        private void P_XamlTypeName()
        {
            // Required
            if (_scanner.Token != GenericTypeNameScannerToken.NAME)
            {
                ThrowOnBadInput();
            }
            P_SimpleTypeName();

            // Optional
            if (_scanner.Token == GenericTypeNameScannerToken.OPEN)
            {
                P_TypeParameters();
            }

            // Optional
            if (_scanner.Token == GenericTypeNameScannerToken.SUBSCRIPT)
            {
                P_RepeatingSubscript();
            }
            
            Callout_EndOfType();
        }


        // SimpleTypeName   ::= (Prefix ‘:’)? TypeName
        //
        private void P_SimpleTypeName()
        {
            // caller checks this.
            Debug.Assert(_scanner.Token == GenericTypeNameScannerToken.NAME);

            string prefix = String.Empty;
            string name = _scanner.MultiCharTokenText;
            _scanner.Read();

            // Colon is optional.
            if (_scanner.Token == GenericTypeNameScannerToken.COLON)
            {
                prefix = name;
                _scanner.Read();

                // IF there was a colon then there must be a name following.
                if (_scanner.Token != GenericTypeNameScannerToken.NAME)
                {
                    ThrowOnBadInput();
                }
                name = _scanner.MultiCharTokenText;
                _scanner.Read();

            }
            Callout_FoundName(prefix, name);
        }

        // TypeParameters   ::= ‘(‘ XamlTypeNameList ‘)’
        //
        private void P_TypeParameters()
        {
            // Required
            // caller checks this.
            Debug.Assert(_scanner.Token == GenericTypeNameScannerToken.OPEN);
            _scanner.Read();

            P_XamlTypeNameList();

            // Required
            if (_scanner.Token != GenericTypeNameScannerToken.CLOSE)
            {
                ThrowOnBadInput();
            }
            _scanner.Read();
        }

        // XamlTypeNameList ::= XamlTypeName NameListExt*
        //
        private void P_XamlTypeNameList()
        {
            P_XamlTypeName();

            // optional zero or more.
            while (_scanner.Token == GenericTypeNameScannerToken.COMMA)
            {
                P_NameListExt();
            }
        }

        // NameListExt      ::= ‘,’ XamlTypeName
        //
        private void P_NameListExt()
        {
            // Caller checked this.
            Debug.Assert(_scanner.Token == GenericTypeNameScannerToken.COMMA);
            _scanner.Read();

            P_XamlTypeName();
        }

        // Subscript        ::= ‘[’ ‘,’* ‘]’
        //
        private void P_RepeatingSubscript()
        {
            // caller checks this.
            Debug.Assert(_scanner.Token == GenericTypeNameScannerToken.SUBSCRIPT);

            do
            {
                Callout_Subscript(_scanner.MultiCharTokenText);
                _scanner.Read();
            }
            while (_scanner.Token == GenericTypeNameScannerToken.SUBSCRIPT);
        }

        private void ThrowOnBadInput()
        {
            throw new TypeNameParserException(SR.Get(SRID.InvalidCharInTypeName, _scanner.ErrorCurrentChar, _inputText));
        }

        private void StartStack()
        {
            _stack = new Stack<TypeNameFrame>();
            TypeNameFrame frame;
            frame = new TypeNameFrame();
            _stack.Push(frame);
        }

        void Callout_FoundName(string prefix, string name)
        {
            TypeNameFrame frame = new TypeNameFrame();
            frame.Name = name;
            string ns = _prefixResolver(prefix);
            if (ns == null)
            {
                throw new TypeNameParserException(SR.Get(SRID.PrefixNotFound, prefix));
            }
            frame.Namespace = ns;
            _stack.Push(frame);
        }

        void Callout_EndOfType()
        {
            TypeNameFrame frame = _stack.Pop();
            XamlTypeName typeName = new XamlTypeName(frame.Namespace, frame.Name, frame.TypeArgs);

            frame = _stack.Peek();
            if (frame.TypeArgs == null)
            {
                frame.AllocateTypeArgs();
            }
            frame.TypeArgs.Add(typeName);
        }

        void Callout_Subscript(string subscript)
        {
            TypeNameFrame frame = _stack.Peek();
            frame.Name += subscript;
        }

        private XamlTypeName CollectNameFromStack()
        {
            if (_stack.Count != 1)
            {
                throw new TypeNameParserException(SR.Get(SRID.InvalidTypeString, _inputText));
            }

            TypeNameFrame frame = _stack.Peek();
            if (frame.TypeArgs.Count != 1)
            {
                throw new TypeNameParserException(SR.Get(SRID.InvalidTypeString, _inputText));
            }

            XamlTypeName xamlTypeName = frame.TypeArgs[0];
            return xamlTypeName;
        }

        private IList<XamlTypeName> CollectNameListFromStack()
        {
            if (_stack.Count != 1)
            {
                throw new TypeNameParserException(SR.Get(SRID.InvalidTypeString, _inputText));
            }

            TypeNameFrame frame = _stack.Peek();

            List<XamlTypeName> xamlTypeNameList = frame.TypeArgs;
            return xamlTypeNameList;
        }
    }

    class TypeNameFrame
    {
        List<XamlTypeName> _typeArgs;

        public string Namespace { get; set; }
        public string Name { get; set; }
        public List<XamlTypeName> TypeArgs { get { return _typeArgs; } }

        public void AllocateTypeArgs()
        {
            _typeArgs = new List<XamlTypeName>();
        }
    }
}
