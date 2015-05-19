#if MOBILE

using System;
using System.Globalization;

namespace Microsoft.CSharp
{
	internal class CodeDomProvider
	{
		public string CreateEscapedIdentifier (string name)
		{
            // Any identifier started with two consecutive underscores are 
            // reserved by CSharp.
            if (IsKeyword(name) || IsPrefixTwoUnderscore(name)) {
                return "@" + name;
            }
            return name;			
		}

		static bool IsKeyword(string value) {
			return false;
		} 

        static bool IsPrefixTwoUnderscore(string value) {
            if( value.Length < 3) {
                return false;
            }
            else {
                return ((value[0] == '_') && (value[1] == '_') && (value[2] != '_'));
            }
        }		
	}

	internal class CSharpCodeProvider : CodeDomProvider
	{
	}

	class CodeGenerator
	{
        public static bool IsValidLanguageIndependentIdentifier(string value)
        {
            return IsValidTypeNameOrIdentifier(value, false);
        }

        private static bool IsValidTypeNameOrIdentifier(string value, bool isTypeName) {
            bool nextMustBeStartChar = true;
            
            if (value.Length == 0) 
                return false;

            // each char must be Lu, Ll, Lt, Lm, Lo, Nd, Mn, Mc, Pc
            // 
            for(int i = 0; i < value.Length; i++) {
                char ch = value[i];
                UnicodeCategory uc = Char.GetUnicodeCategory(ch);
                switch (uc) {
                    case UnicodeCategory.UppercaseLetter:        // Lu
                    case UnicodeCategory.LowercaseLetter:        // Ll
                    case UnicodeCategory.TitlecaseLetter:        // Lt
                    case UnicodeCategory.ModifierLetter:         // Lm
                    case UnicodeCategory.LetterNumber:           // Lm
                    case UnicodeCategory.OtherLetter:            // Lo
                        nextMustBeStartChar = false;
                        break;

                    case UnicodeCategory.NonSpacingMark:         // Mn
                    case UnicodeCategory.SpacingCombiningMark:   // Mc
                    case UnicodeCategory.ConnectorPunctuation:   // Pc
                    case UnicodeCategory.DecimalDigitNumber:     // Nd
                        // Underscore is a valid starting character, even though it is a ConnectorPunctuation.
                        if (nextMustBeStartChar && ch != '_')
                            return false;
                        
                        nextMustBeStartChar = false;
                        break;
                    default:
                        // We only check the special Type chars for type names. 
                        if (isTypeName && IsSpecialTypeChar(ch, ref nextMustBeStartChar)) {
                            break;
                        }

                        return false;
                }
            }

            return true;
        }

        private static bool IsSpecialTypeChar(char ch, ref bool nextMustBeStartChar) {
            switch(ch) {
                case ':':
                case '.':
                case '$':
                case '+':
                case '<':
                case '>':
                case '-':
                case '[':
                case ']':
                case ',':
                case '&':
                case '*':
                    nextMustBeStartChar = true;
                    return true;

                case '`':
                    return true;
            }
            return false;
        }        
	}
}

#endif