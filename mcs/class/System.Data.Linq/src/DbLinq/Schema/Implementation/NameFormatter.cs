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

namespace DbLinq.Schema.Implementation
{
    /// <summary>
    /// INameFormatter default implementation
    /// </summary>
    internal class NameFormatter : INameFormatter
    {
        /// <summary>
        /// Singularization type
        /// </summary>
        internal enum Singularization
        {
            /// <summary>
            /// The word plural doesn't change
            /// </summary>
            DontChange,
            /// <summary>
            /// Singularize the word
            /// </summary>
            Singular,
            /// <summary>
            /// Pluralize the word
            /// </summary>
            Plural,
        }

        /// <summary>
        /// Indicates the word position. Internally used for capitalization
        /// </summary>
        [Flags]
        protected enum Position
        {
            /// <summary>
            /// Word is first in sentence
            /// </summary>
            First = 0x01,
            /// <summary>
            /// Word is last in sentence
            /// </summary>
            Last = 0x02,
        }

        /// <summary>
        /// ILanguageWords by culture info name
        /// </summary>
        private readonly IDictionary<string, ILanguageWords> languageWords = new Dictionary<string, ILanguageWords>();

        /// <summary>
        /// Substitution char for invalid characters
        /// </summary>
        private const char SubstitutionChar = '_';

        /// <summary>
        /// Gets the ILanguageWords by CultureInfo.
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Formats the specified words.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <param name="oldName">The old name.</param>
        /// <param name="newCase">The new case.</param>
        /// <param name="singularization">The singularization.</param>
        /// <returns></returns>
        public virtual string Format(ILanguageWords words, string oldName, Case newCase, Singularization singularization)
        {
            var parts = ExtractWordsFromCaseAndLanguage(words, oldName);
            return Format(words, parts, newCase, singularization);
        }

        /// <summary>
        /// Formats the specified words.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <param name="parts">The parts.</param>
        /// <param name="newCase">The new case.</param>
        /// <param name="singularization">The singularization.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Toes the camel case.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <returns></returns>
        public string ToCamelCase(string part)
        {
            return part.ToLower();
        }

        /// <summary>
        /// Toes the pascal case.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <returns></returns>
        public string ToPascalCase(string part)
        {
            // we have a very special case here, for "ID" that goes to full uppercase even in PascalCase mode
            if (string.Compare(part, "id", true) == 0)
                return "ID";
            part = part.Substring(0, 1).ToUpper() + part.Substring(1).ToLower();
            return part;
        }

        /// <summary>
        /// Toes the net case.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <returns></returns>
        public string ToNetCase(string part)
        {
            if (part.Length <= 2)
                part = part.ToUpper();
            else
                part = ToPascalCase(part);
            return part;
        }

        /// <summary>
        /// Adjusts the part.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <param name="part">The part.</param>
        /// <param name="position">The position.</param>
        /// <param name="newCase">The new case.</param>
        /// <param name="singularization">The singularization.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Pushes the word on a collection
        /// </summary>
        /// <param name="words">The words.</param>
        /// <param name="currentWord">The current word.</param>
        private static void PushWord(ICollection<string> words, StringBuilder currentWord)
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
        /// <param name="substitutionChar"></param>
        /// <returns></returns>
        protected virtual IList<string> ExtractWordsFromCase(string name, char substitutionChar)
        {
            var words = new List<string>();
            bool currentLowerCase = true;
            var currentWord = new StringBuilder();
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
                // only letters or digits are allowed
                if (char.IsLetterOrDigit(currentChar))
                    currentWord.Append(currentChar);
                // _ is the separator character, but all other characters will be kept
                else if (currentChar != '_' && !char.IsSeparator(currentChar))
                    currentWord.Append(substitutionChar);
                currentLowerCase = isLower;
            }
            PushWord(words, currentWord);

            return words;
        }

        /// <summary>
        /// Extracts the words from case and language.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <returns></returns>
        protected virtual IList<string> ExtractWordsFromCaseAndLanguage(ILanguageWords words, string dbName)
        {
            var extractedWords = new List<string>();
            foreach (var wordsMagma in ExtractWordsFromCase(dbName, SubstitutionChar))
            {
                extractedWords.AddRange(words.GetWords(wordsMagma));
            }
            return extractedWords;
        }

        /// <summary>
        /// Extracts the words from given text.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction type (case or language identification).</param>
        /// <returns></returns>
        protected virtual IList<string> ExtractWords(ILanguageWords words, string dbName, WordsExtraction extraction)
        {
            switch (extraction)
            {
            case WordsExtraction.None:
                return new[] { dbName };
            case WordsExtraction.FromCase:
                return ExtractWordsFromCase(dbName, SubstitutionChar);
            case WordsExtraction.FromDictionary:
                return ExtractWordsFromCaseAndLanguage(words, dbName);
            default:
                throw new ArgumentOutOfRangeException("extraction");
            }
        }

        /// <summary>
        /// Gets the singularization.
        /// </summary>
        /// <param name="singularization">The singularization.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        protected virtual Singularization GetSingularization(Singularization singularization, NameFormat nameFormat)
        {
            if (!nameFormat.Pluralize)
                return Singularization.DontChange;
            return singularization;
        }

        /// <summary>
        /// Reformats a name by adjusting its case.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <param name="newCase">The new case.</param>
        /// <returns></returns>
        public string Format(string words, Case newCase)
        {
            return Format(null, ExtractWordsFromCase(words, SubstitutionChar), newCase, Singularization.DontChange);
        }

        /// <summary>
        /// Gets the name of the schema.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        public SchemaName GetSchemaName(string dbName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var schemaName = new SchemaName { DbName = dbName };
            schemaName.NameWords = ExtractWords(words, dbName, extraction);
            schemaName.ClassName = Format(words, schemaName.NameWords, nameFormat.Case, Singularization.DontChange);
            return schemaName;
        }

        /// <summary>
        /// Gets the name of the procedure.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        public ProcedureName GetProcedureName(string dbName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var procedureName = new ProcedureName { DbName = dbName };
            procedureName.NameWords = ExtractWords(words, dbName, extraction);
            procedureName.MethodName = Format(words, procedureName.NameWords, nameFormat.Case, Singularization.DontChange);
            return procedureName;
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        public ParameterName GetParameterName(string dbName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var parameterName = new ParameterName { DbName = dbName };
            parameterName.NameWords = ExtractWords(words, dbName, extraction);
            parameterName.CallName = Format(words, parameterName.NameWords, Case.camelCase, Singularization.DontChange);
            return parameterName;
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the name of the association.
        /// </summary>
        /// <param name="dbManyName">Name of the db many.</param>
        /// <param name="dbOneName">Name of the db one.</param>
        /// <param name="dbConstraintName">Name of the db constraint.</param>
        /// <param name="foreignKeyName">Name of the foreign key.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        public AssociationName GetAssociationName(string dbManyName, string dbOneName, string dbConstraintName,
            string foreignKeyName, WordsExtraction extraction, NameFormat nameFormat)
        {
            var words = GetLanguageWords(nameFormat.Culture);
            var associationName = new AssociationName { DbName = dbManyName };
            associationName.NameWords = ExtractWords(words, dbManyName, extraction);
            associationName.ManyToOneMemberName = Format(words, dbOneName, nameFormat.Case, GetSingularization(Singularization.Singular, nameFormat));
            // TODO: this works only for PascalCase
            if (dbManyName == dbOneName)
                associationName.ManyToOneMemberName = foreignKeyName.Replace(',', '_') + associationName.ManyToOneMemberName;
            // TODO: support new extraction
            associationName.OneToManyMemberName = Format(words, dbManyName, nameFormat.Case, GetSingularization(Singularization.Plural, nameFormat));
            return associationName;
        }
    }
}
