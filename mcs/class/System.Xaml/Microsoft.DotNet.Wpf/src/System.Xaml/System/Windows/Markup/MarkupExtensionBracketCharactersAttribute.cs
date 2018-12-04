// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Text;

namespace System.Windows.Markup
{
    /// <summary>
    /// Attribute to declare that this associated property will have special parsing rules
    /// for any text that is enclosed between these special characters. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class MarkupExtensionBracketCharactersAttribute : Attribute
    {
        /// <summary>
        /// Constructor for an MarkupExtensionBracketCharactersAttribute
        /// </summary>
        /// <param name="openingBracket">Opening character for the Bracket characters. For example, '(' , '[' </param>
        /// /// <param name="closingBracket">Closing character for the Bracket characters. For example, ')' , ']' </param>
        public MarkupExtensionBracketCharactersAttribute(char openingBracket, char closingBracket)
        {
            OpeningBracket = openingBracket;
            ClosingBracket = closingBracket;
        }

        public char OpeningBracket { get; }
        public char ClosingBracket { get; }
    }
}
