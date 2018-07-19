//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Text;

    class UniqueCodeIdentifierScope
    {
        const int MaxIdentifierLength = 511;
        SortedList<string, string> names;

        // assumes identifier is valid
        protected virtual void AddIdentifier(string identifier)
        {
            if (names == null)
                names = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);

            names.Add(identifier, identifier);
        }

        // assumes identifier is valid
        public void AddReserved(string identifier)
        {
            Fx.Assert(IsUnique(identifier), "");

            AddIdentifier(identifier);
        }

        // validates name before trying to add
        public string AddUnique(string name, string defaultName)
        {
            string validIdentifier = MakeValid(name, defaultName);

            string uniqueIdentifier = validIdentifier;
            int i = 1;

            while (!IsUnique(uniqueIdentifier))
            {
                uniqueIdentifier = validIdentifier + (i++).ToString(CultureInfo.InvariantCulture);
            }

            AddIdentifier(uniqueIdentifier);

            return uniqueIdentifier;
        }

        // assumes identifier is valid
        public virtual bool IsUnique(string identifier)
        {
            return names == null || !names.ContainsKey(identifier);
        }

        static bool IsValidStart(char c)
        {
            return (Char.GetUnicodeCategory(c) != UnicodeCategory.DecimalDigitNumber);
        }

        static bool IsValid(char c)
        {
            UnicodeCategory uc = Char.GetUnicodeCategory(c);

            // each char must be Lu, Ll, Lt, Lm, Lo, Nd, Mn, Mc, Pc

            switch (uc)
            {
                case UnicodeCategory.UppercaseLetter:        // Lu
                case UnicodeCategory.LowercaseLetter:        // Ll
                case UnicodeCategory.TitlecaseLetter:        // Lt
                case UnicodeCategory.ModifierLetter:         // Lm
                case UnicodeCategory.OtherLetter:            // Lo
                case UnicodeCategory.DecimalDigitNumber:     // Nd
                case UnicodeCategory.NonSpacingMark:         // Mn
                case UnicodeCategory.SpacingCombiningMark:   // Mc
                case UnicodeCategory.ConnectorPunctuation:   // Pc
                    return true;
                default:
                    return false;
            }
        }

        public static string MakeValid(string identifier, string defaultIdentifier)
        {
            if (String.IsNullOrEmpty(identifier))
                return defaultIdentifier;

            if (identifier.Length <= MaxIdentifierLength && System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(identifier))
                return identifier;

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < identifier.Length && builder.Length < MaxIdentifierLength; i++)
            {
                char c = identifier[i];
                if (IsValid(c))
                {
                    if (builder.Length == 0)
                    {
                        // check for valid start char
                        if (!IsValidStart(c))
                            builder.Append('_');
                    }
                    builder.Append(c);
                }
            }
            if (builder.Length == 0)
                return defaultIdentifier;

            return builder.ToString();
        }
    }

    class UniqueCodeNamespaceScope : UniqueCodeIdentifierScope
    {
        CodeNamespace codeNamespace;

        // possible direction: add an option to cache for multi-use cases
        public UniqueCodeNamespaceScope(CodeNamespace codeNamespace)
        {
            this.codeNamespace = codeNamespace;
        }

        public CodeNamespace CodeNamespace
        {
            get { return this.codeNamespace; }
        }

        protected override void AddIdentifier(string identifier)
        {
        }

        public CodeTypeReference AddUnique(CodeTypeDeclaration codeType, string name, string defaultName)
        {
            codeType.Name = base.AddUnique(name, defaultName);
            codeNamespace.Types.Add(codeType);
            return ServiceContractGenerator.NamespaceHelper.GetCodeTypeReference(this.codeNamespace, codeType);
        }

        public override bool IsUnique(string identifier)
        {
            return !NamespaceContainsType(identifier);
        }

        bool NamespaceContainsType(string typeName)
        {
            foreach (CodeTypeDeclaration codeType in codeNamespace.Types)
            {
                if (String.Compare(codeType.Name, typeName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
