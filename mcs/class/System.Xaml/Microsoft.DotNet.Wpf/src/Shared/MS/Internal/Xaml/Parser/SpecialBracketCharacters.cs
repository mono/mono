// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
//  Description: Data model for the Bracket characters specified on a Markup Extension property.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MS.Internal.Xaml.Parser
{
    /// <summary>
    /// Class that provides helper functions for the parser/Xaml Reader 
    /// to process Bracket Characters specified on a Markup Extension Property
    /// </summary>
    internal class SpecialBracketCharacters : ISupportInitialize
    {
        private string _startChars;
        private string _endChars;
        private readonly static ISet<char> _restrictedCharSet = new SortedSet<char>((new char[] { '=', ',', '\'', '"', '{', '}', '\\' }));
        private bool _initializing;
        private StringBuilder _startCharactersStringBuilder;
        private StringBuilder _endCharactersStringBuilder;
        
        internal SpecialBracketCharacters()
        {
            BeginInit();
        }

        internal SpecialBracketCharacters(IReadOnlyDictionary<char,char> attributeList)
        {
            BeginInit();
            if (attributeList != null && attributeList.Count > 0)
            {
                Tokenize(attributeList);
            }
        }

        internal void AddBracketCharacters(char openingBracket, char closingBracket)
        {
            if (_initializing)
            {
                _startCharactersStringBuilder.Append(openingBracket);
                _endCharactersStringBuilder.Append(closingBracket);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private void Tokenize(IReadOnlyDictionary<char,char> attributeList)
        {
            if (_initializing)
            {
                foreach (char openingBracket in attributeList.Keys)
                {
                    char closingBracket = attributeList[openingBracket];
                    string errorMessage = string.Empty; 
                    if (IsValidBracketCharacter(openingBracket, closingBracket))
                    {
                        _startCharactersStringBuilder.Append(openingBracket);
                        _endCharactersStringBuilder.Append(closingBracket);
                    }
                }
            }
        }

        private bool IsValidBracketCharacter(char openingBracket, char closingBracket)
        {
            if (openingBracket == closingBracket)
            {
                throw new InvalidOperationException("Opening bracket character cannot be the same as closing bracket character.");
            }
            else if (char.IsLetterOrDigit(openingBracket) || char.IsLetterOrDigit(closingBracket) || char.IsWhiteSpace(openingBracket) || char.IsWhiteSpace(closingBracket))
            {
                throw new InvalidOperationException("Bracket characters cannot be alpha-numeric or whitespace.");
            }
            else if (_restrictedCharSet.Contains(openingBracket) || _restrictedCharSet.Contains(closingBracket))
            {
                throw new InvalidOperationException("Bracket characters cannot be one of the following: '=' , ',', '\'', '\"', '{ ', ' }', '\\'");
            }
            else
            {
                return true;
            }
        }

        internal bool IsSpecialCharacter(char ch)
        {
            return _startChars.Contains(ch.ToString()) || _endChars.Contains(ch.ToString());
        }

        internal bool StartsEscapeSequence(char ch)
        {
            return _startChars.Contains(ch.ToString());
        }

        internal bool EndsEscapeSequence(char ch)
        {
            return _endChars.Contains(ch.ToString());
        }

        internal bool Match(char start, char end)
        {
            return _endChars.IndexOf(end.ToString()) == _startChars.IndexOf(start.ToString());
        }

        internal string StartBracketCharacters
        {
            get { return _startChars; }
        }

        internal string EndBracketCharacters
        {
            get { return _endChars; }
        }

        public void BeginInit()
        {
            _initializing = true;
            _startCharactersStringBuilder = new StringBuilder();
            _endCharactersStringBuilder = new StringBuilder();
        }

        public void EndInit()
        {
            _startChars = _startCharactersStringBuilder.ToString();
            _endChars = _endCharactersStringBuilder.ToString();
            _initializing = false;
        }
    }
}
