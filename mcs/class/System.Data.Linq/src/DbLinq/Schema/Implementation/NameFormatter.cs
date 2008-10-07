#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DbLinq.Factory;
using DbLinq.Language;
using DbLinq.Schema;

namespace DbLinq.Schema.Implementation
{
    internal class NameFormatter : INameFormatter
    {
        internal enum Singularization
        {
            DontChange,
            Singular,
            Plural,
        }

        [Flags]
        protected enum Position
        {
            First = 0x01,
            Last = 0x02,
        }

        private IDictionary<string, ILanguageWords> languageWords = new Dictionary<string, ILanguageWords>();

        protected virtual ILanguageWords GetLanguageWords(CultureInfo cultureInfo)
        {
            lock (languageWords)
            {
                ILanguageWords words;
                if (!languageWords.TryGetValue(cultureInfo.Name, out words))
                {
                    var languages = ObjectFactory.Get<ILanguages>();
                    words = languages.Load(cultureInfo);
                    languageWords[cultureInfo.Name] = words;
                }
                return words;
            }
        }

        public virtual string Format(ILanguageWords words, string oldName, Case newCase, Singularization singularization)
        {
            var parts = words.GetWords(oldName);
            return Format(words, parts, newCase, singularization);
        }

        private string Format(ILanguageWords words, IList<string> parts, Case newCase, Singularization singularization)
        {
            var result = new StringBuilder();
            for (int partIndex = 0; partIndex < parts.Count; partIndex++)
            {
                Position position = 0;
                if (partIndex == 0)
                    position |= Position.First;
                if (partIndex == parts.Count - 1)
                    position |= Position.Last;
                result.Append(AdjustPart(words, parts[partIndex], position, newCase, singularization));
            }
            return result.ToString();
        }

        public string ToCamelCase(string part)
        {
            return part.ToLower();
        }

        public string ToPascalCase(string part)
        {
            // we have a very special case here, for "ID" that goes to full uppercase even in PascalCase mode
            if (string.Compare(part, "id", true) == 0)
                return "ID";
            part = part.Substring(0, 1).ToUpper() + part.Substring(1).ToLower();
            return part;
        }

        public string ToNetCase(string part)
        {
            if (part.Length <= 2)
                part = part.ToUpper();
            else
                part = ToPascalCase(part);
            return part;
        }

        protected virtual string AdjustPart(ILanguageWords words, string part, Position position, Case newCase, Singularization singularization)
        {
            if (singularization != Singularization.DontChange && (position & Position.Last) != 0)
            {
                if (singularization == Singularization.Singular)
                    part = words.Singularize(part);
                else
                    part = words.Pluralize(part);
            }
            Case applyCase = newCase;
            if (applyCase == Case.camelCase && (position & Position.First) == 0)
                applyCase = Case.PascalCase;
            switch (applyCase)
            {
            case Case.Leave:
                break;
            case Case.camelCase:
                part = ToCamelCase(part);
                break;
            case Case.PascalCase:
                part = ToPascalCase(part);
                break;
            case Case.NetCase:
                part = ToNetCase(part);
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
            return part;
        }

        private void PushWord(IList<string> words, StringBuilder currentWord)
        {
            if (currentWord.Length > 0)
            {
                words.Add(currentWord.ToString());
                currentWord.Remove(0, currentWord.Length);
            }
        }

        /// <summary>
        /// Extracts words from uppercase and _
        /// A word can also be composed of several uppercase letters
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual IList<string> ExtractWordsFromCase(string name)
        {
            List<string> words = new List<string>();
            bool currentLowerCase = true;
            StringBuilder currentWord = new StringBuilder();
            for (int charIndex = 0; charIndex < name.Length; charIndex++)
            {
                char currentChar = name[charIndex];
                bool isLower = char.IsLower(currentChar);
                // we switched to uppercase
                if (!isLower && currentLowerCase)
                {
                    PushWord(words, currentWord);
                }
                else if (isLower && !currentLowerCase)
                {
                    // if the current word has several uppercase letters, it is one unique word
                    if (currentWord.Length > 1)
                        PushWord(words, currentWord);
                }
                if (char.IsLetterOrDigit(currentChar))
                    currentWord.Append(currentChar);
                currentLowerCase = isLower;
            }
            PushWord(words, currentWord);

            return words;
        }

        protected virtual IList<string> ExtractWords(ILanguageWords words, string dbName, WordsExtraction extraction)
        {
            switch (extraction)
            {
            case WordsExtraction.None:
                return new[] { dbName };
            case WordsExtraction.FromCase:
                return ExtractWordsFromCase(dbName);
            case WordsExtraction.FromDictionary:
                return words.GetWords(dbName);
            default:
                throw new ArgumentOutOfRangeException("extraction");
            }
        }

        protected virtual Singularization GetSingularization(Singularization singularization, NameFormat nameFormat)
        {
            if (!nameFormat.Pluralize)
                return Singularization.DontChange;
            return singularization;
        }

        public string Format(string words, Case newCase)
        {
            return Format(null, ExtractWordsFromCase(words), newCase, Singularization.DontChange);
        }

        public SchemaName GetSchemaName(string dbName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var schemaName = new SchemaName { DbName = dbName };
            schemaName.NameWords = ExtractWords(words, dbName, extraction);
            schemaName.ClassName = Format(words, schemaName.NameWords, nameFormat.Case, Singularization.DontChange);
            return schemaName;
        }

        public ProcedureName GetProcedureName(string dbName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var procedureName = new ProcedureName { DbName = dbName };
            procedureName.NameWords = ExtractWords(words, dbName, extraction);
            procedureName.MethodName = Format(words, procedureName.NameWords, nameFormat.Case, Singularization.DontChange);
            return procedureName;
        }

        public ParameterName GetParameterName(string dbName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var parameterName = new ParameterName { DbName = dbName };
            parameterName.NameWords = ExtractWords(words, dbName, extraction);
            parameterName.CallName = Format(words, parameterName.NameWords, Case.camelCase, Singularization.DontChange);
            return parameterName;
        }

        public TableName GetTableName(string dbName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var tableName = new TableName { DbName = dbName };
            tableName.NameWords = ExtractWords(words, dbName, extraction);
            // if no extraction (preset name, just copy it)
            if (extraction == WordsExtraction.None)
                tableName.ClassName = tableName.DbName;
            else
                tableName.ClassName = Format(words, tableName.NameWords, nameFormat.Case, GetSingularization(Singularization.Singular, nameFormat));
            tableName.MemberName = Format(words, tableName.NameWords, nameFormat.Case, GetSingularization(Singularization.Plural, nameFormat));
            return tableName;
        }

        public ColumnName GetColumnName(string dbName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var columnName = new ColumnName { DbName = dbName };
            columnName.NameWords = ExtractWords(words, dbName, extraction);
            // if no extraction (preset name, just copy it)
            if (extraction == WordsExtraction.None)
                columnName.PropertyName = dbName;
            else
                columnName.PropertyName = Format(words, columnName.NameWords, nameFormat.Case, Singularization.DontChange);
            return columnName;
        }

        public AssociationName GetAssociationName(string dbManyName, string dbOneName, string dbConstraintName, 
            string foreignKeyName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var associationName = new AssociationName { DbName = dbManyName };
            associationName.NameWords = ExtractWords(words, dbManyName, extraction);
            associationName.ManyToOneMemberName = Format(words, dbOneName, nameFormat.Case, GetSingularization(Singularization.Singular, nameFormat));
            // TODO: this works only for PascalCase
            if (dbManyName == dbOneName)
                associationName.ManyToOneMemberName = foreignKeyName + associationName.ManyToOneMemberName;
            // TODO: support new extraction
            associationName.OneToManyMemberName = Format(words, dbManyName, nameFormat.Case, GetSingularization(Singularization.Plural, nameFormat));
            return associationName;
        }
    }
}
