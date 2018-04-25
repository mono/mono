//------------------------------------------------------------------------------
// <copyright file="TextBoxBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;

    /// <devdoc>
    ///     Provides functionality for formatting a test string against a mask string.
    ///     MaskedTextProvider is stateful, it keeps information about the input characters so
    ///     multiple call to Add/Remove will work in the same buffer.
    ///     Most of the operations are performed on a virtual string containing the input characters as opposed 
    ///     to the test string itself, since mask literals cannot be modified (i.e: replacing on a literal position
    ///     will actually replace on the nearest edit position forward).
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class MaskedTextProvider : ICloneable
    {
        ///
        ///  Some concept definitions:
        ///  
        ///  'mask'             : A string representing the mask associated with an instance of this class.
        ///  'test string'      : A string representing the user's text formatted as specified by the mask.
        ///  'virtual text'     : The characters entered by the user to be converted into the 'test string'.
        ///                       no buffer exists to hold them since they're stored in the test string but
        ///                       we keep an array with their position in the test string for fast access.
        ///  'text indexer'     : An array which values point to 'edit char' positions in the test string and 
        ///                       indexes correspond to the position in the user's text.
        ///  'char descriptor'  : A structure describing a char constraints as specified in the mask plus some
        ///                       other info.
        ///  'string descriptor': An array of char descriptor objects describing the chars in the 'test string',
        ///                       the indexes of this array represent the position of the chars in the string.
        
        
        /// <devdoc>
        ///     Char case conversion type used when '>' (subsequent chars to upper case) or '<' (subsequent chars to lower case)
        ///     are specified in the mask.
        /// </devdoc>
        private enum CaseConversion
        {
            None,
            ToLower,
            ToUpper
        }


        /// <devdoc>
        ///     Type of the characters in the test string according to the mask language.
        /// </devdoc>
        [Flags]
        private enum CharType
        {
            EditOptional = 0x01, // editable char  ('#', '9', 'A', 'a', etc) optional.
            EditRequired = 0x02, // editable char  ('#', '9', 'A', 'a', etc) required.
            Separator    = 0x04, // separator char ('.', ',', ':', '$').
            Literal      = 0x08, // literal char   ('\\', '-', etc)
            Modifier     = 0x10  // char modifier  ('>', '<')
        }

        /// <devdoc>
        ///    This structure describes some constraints and properties of a character in the test string, as specified 
        ///    in the mask.
        /// </devdoc>
        private class CharDescriptor
        {
            // The position the character holds in the mask string. Required for testing the character against the mask.
            public int MaskPosition;   
            
            // The char case conversion specified in the mask.  Required for formatting the string when requested.
            public CaseConversion CaseConversion; 

            // The char type according to the mask language indentifiers. (Separator, Editable char...).
            // Required for validating the input char.
            public CharType CharType;

            // Specifies whether the editable char has been assigned a value.  Meaningful to edit chars only.
            public bool IsAssigned;

            // constructors.
            public CharDescriptor(int maskPos, CharType charType )
            {
                this.MaskPosition = maskPos;
                this.CharType     = charType;
            }

            public override string ToString()
            {
                return String.Format( 
                                        CultureInfo.InvariantCulture,
                                        "MaskPosition[{0}] <CaseConversion.{1}><CharType.{2}><IsAssigned: {3}", 
                                        this.MaskPosition, 
                                        this.CaseConversion, 
                                        this.CharType,
                                        this.IsAssigned
                                     ); 
            }
        }

        //// class data.
        
        const char spaceChar           = ' ';
        const char defaultPromptChar   = '_';
        const char nullPasswordChar    = '\0';
        const bool defaultAllowPrompt  = true;
        const int  invalidIndex        = -1;
        const byte editAny             = 0;
        const byte editUnassigned      = 1;
        const byte editAssigned        = 2;
        const bool forward             = true;
        const bool backward            = false;

        // Bit masks for bool properties.
        static int ASCII_ONLY            = BitVector32.CreateMask();
        static int ALLOW_PROMPT_AS_INPUT = BitVector32.CreateMask(ASCII_ONLY);
        static int INCLUDE_PROMPT        = BitVector32.CreateMask(ALLOW_PROMPT_AS_INPUT);
        static int INCLUDE_LITERALS      = BitVector32.CreateMask(INCLUDE_PROMPT);
        static int RESET_ON_PROMPT       = BitVector32.CreateMask(INCLUDE_LITERALS);
        static int RESET_ON_LITERALS     = BitVector32.CreateMask(RESET_ON_PROMPT);
        static int SKIP_SPACE            = BitVector32.CreateMask(RESET_ON_LITERALS);

        // Type cached to speed up cloning of this object.
        static Type maskTextProviderType = typeof(MaskedTextProvider);

        //// Instance data.
        
        // Bit vector to represent bool variables.
        private BitVector32 flagState;
        
        // Used to obtained localized placeholder chars (date separator for instance).
        private CultureInfo culture;

        // the formatted string.
        private StringBuilder testString;

        // the number of assigned edit chars.
        private int assignedCharCount;

        // the number of assigned required edit chars.
        private int requiredCharCount;

        // the number of required edit positions in the test string.
        private int requiredEditChars;

        // the number of optional edit positions in the test string.
        private int optionalEditChars;

        // Properties backend fields (see corresponding property for info).
        private string mask;
        private char   passwordChar;
        private char   promptChar;

        // We maintain an array (string descriptor table) of CharDescriptor elements describing the characters in the
        // test string, as specified in the mask.  It allows us to access character information in constant time since
        // we don't have to traverse the mask or test string whenever we need that information.
        private List<CharDescriptor> stringDescriptor;

        ////// Construction API

        /// <devdoc>
        ///     Creates a MaskedTextProvider object from the specified mask.
        /// </devdoc>
        public MaskedTextProvider( string mask ) 
            : this( mask , null, defaultAllowPrompt, defaultPromptChar, nullPasswordChar, false )
        {
        }

        /// <devdoc>
        ///     Creates a MaskedTextProvider object from the specified mask.
        ///     'restrictToAscii' specifies whether the input characters should be restricted to ASCII characters only.
        /// </devdoc>
        public MaskedTextProvider(string mask, bool restrictToAscii) 
            : this( mask , null, defaultAllowPrompt, defaultPromptChar, nullPasswordChar, restrictToAscii )
        {
        }

        /// <devdoc>
        ///     Creates a MaskedTextProvider object from the specified mask.
        ///     'culture' is used to set the separator characters to the correspondig locale character; if null, the current
        ///               culture is used.
        /// </devdoc>
        public MaskedTextProvider( string mask, CultureInfo culture ) 
            : this( mask , culture, defaultAllowPrompt, defaultPromptChar, nullPasswordChar, false )
        {
        }

        /// <devdoc>
        ///     Creates a MaskedTextProvider object from the specified mask.
        ///     'culture' is used to set the separator characters to the correspondig locale character; if null, the current
        ///               culture is used.
        ///     'restrictToAscii' specifies whether the input characters should be restricted to ASCII characters only.
        /// </devdoc>
        public MaskedTextProvider(string mask, CultureInfo culture, bool restrictToAscii)
            : this( mask , culture, defaultAllowPrompt, defaultPromptChar, nullPasswordChar, restrictToAscii )
        {
        }

        /// <devdoc>
        ///     Creates a MaskedTextProvider object from the specified mask .  
        ///     'passwordChar' specifies the character to be used in the password string.
        ///     'allowPromptAsInput' specifies whether the prompt character should be accepted as a valid input or not.
        /// </devdoc>
        public MaskedTextProvider( string mask, char passwordChar, bool allowPromptAsInput ) 
            : this( mask , null, allowPromptAsInput, defaultPromptChar, passwordChar, false )
        {
        }

        /// <devdoc>
        ///     Creates a MaskedTextProvider object from the specified mask .  
        ///     'passwordChar' specifies the character to be used in the password string.
        ///     'allowPromptAsInput' specifies whether the prompt character should be accepted as a valid input or not.
        /// </devdoc>
        public MaskedTextProvider(string mask, CultureInfo culture, char passwordChar, bool allowPromptAsInput) 
            : this( mask , culture, allowPromptAsInput, defaultPromptChar, passwordChar, false )
        {
        }

        /// <devdoc>
        ///     Creates a MaskedTextProvider object from the specified mask.
        ///     'culture' is used to set the separator characters to the correspondig locale character; if null, the current
        ///               culture is used.
        ///     'allowPromptAsInput' specifies whether the prompt character should be accepted as a valid input or not.
        ///     'promptChar' specifies the character to be used for the prompt.
        ///     'passwordChar' specifies the character to be used in the password string.
        ///     'restrictToAscii' specifies whether the input characters should be restricted to ASCII characters only.
        /// </devdoc>
        public MaskedTextProvider( string mask, CultureInfo culture, bool allowPromptAsInput, char promptChar,  char passwordChar, bool restrictToAscii ) 
        {
            if( string.IsNullOrEmpty(mask) )
            {
                throw new ArgumentException( SR.GetString( SR.MaskedTextProviderMaskNullOrEmpty), "mask" );
            }

            foreach( char c in mask )
            {
                if( !IsPrintableChar( c ) )
                {
                    throw new ArgumentException( SR.GetString( SR.MaskedTextProviderMaskInvalidChar ) );
                }
            }

            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }

            this.flagState = new BitVector32();
            
            // read only property-backend fields.

            this.mask               = mask;
            this.promptChar         = promptChar;
            this.passwordChar       = passwordChar;

            //Neutral cultures cannot be queried for culture-specific information.
            if (culture.IsNeutralCulture)
            {
                // find the first specific (non-neutral) culture that contains ----/region specific info.
                foreach (CultureInfo tempCulture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    if (culture.Equals(tempCulture.Parent))
                    {
                        this.culture = tempCulture;
                        break;
                    }
                }

                // Last resort use invariant culture.
                if (this.culture == null)
                {
                    this.culture = CultureInfo.InvariantCulture;
                }
            }
            else
            {
                this.culture = culture;
            }

            if (!this.culture.IsReadOnly)
            {
                this.culture = CultureInfo.ReadOnly(this.culture);
            }

            this.flagState[ALLOW_PROMPT_AS_INPUT] = allowPromptAsInput;
            this.flagState[ASCII_ONLY           ] = restrictToAscii;

            // set default values for read/write properties.

            this.flagState[INCLUDE_PROMPT   ] = false;
            this.flagState[INCLUDE_LITERALS ] = true;
            this.flagState[RESET_ON_PROMPT  ] = true;
            this.flagState[SKIP_SPACE       ] = true;
            this.flagState[RESET_ON_LITERALS] = true;

            Initialize();
        }

        /// <devdoc>
        ///     Initializes the test string according to the mask and populates the character descirptor table
        ///     (stringDescriptor).
        /// </devdoc>
        private void Initialize()
        {
            this.testString = new StringBuilder();
            this.stringDescriptor = new List<CharDescriptor>();

            CaseConversion caseConversion = CaseConversion.None; // The conversion specified in the mask.
            bool escapedChar  = false;            // indicates the current char is to be escaped.
            int  testPosition = 0;                // the position of the char in the test string.
            CharType charType = CharType.Literal; // the mask language char type.
            char ch;                              // the char under test.
            string locSymbol  = string.Empty;     // the locale symbol corresponding to a separator in the mask.
                                                  // in some cultures a symbol is represented with more than one
                                                  // char, for instance '$' for en-JA is '$J'.

            //
            // Traverse the mask to generate the test string and the string descriptor table so we don't have
            // to traverse those strings anymore.
            //
            for (int maskPos = 0; maskPos < this.mask.Length; maskPos++)
            {
                ch = this.mask[maskPos];
                if (!escapedChar)   // if false treat the char as literal.
                {
                    switch (ch)
                    {
                        //
                        // Mask language placeholders.
                        // set the corresponding localized char to be added to the test string.
                        //
                        case '.':   // decimal separator.
                            locSymbol = this.culture.NumberFormat.NumberDecimalSeparator;
                            charType  = CharType.Separator;
                            break;

                        case ',':   // thousands separator.
                            locSymbol = this.culture.NumberFormat.NumberGroupSeparator;
                            charType  = CharType.Separator;
                            break;

                        case ':':   // time separator.
                            locSymbol = this.culture.DateTimeFormat.TimeSeparator;
                            charType  = CharType.Separator;
                            break;

                        case '/':   // date separator.
                            locSymbol = this.culture.DateTimeFormat.DateSeparator;
                            charType  = CharType.Separator;
                            break;

                        case '$':   // currency symbol.
                            locSymbol = this.culture.NumberFormat.CurrencySymbol;
                            charType  = CharType.Separator;
                            break;

                        //
                        // Mask language modifiers.
                        // StringDescriptor won't have an entry for modifiers, the modified character
                        // descriptor contains an entry for case conversion that is set accordingly.
                        // Just set the appropriate flag for the characters that follow and continue.
                        //
                        case '<':   // convert all chars that follow to lowercase.
                            caseConversion = CaseConversion.ToLower;
                            continue;

                        case '>':   // convert all chars that follow to uppercase.
                            caseConversion = CaseConversion.ToUpper;
                            continue;

                        case '|':   // no convertion performed on the chars that follow.
                            caseConversion = CaseConversion.None;
                            continue;

                        case '\\':   // escape char - next will be a literal.
                            escapedChar = true;
                            charType = CharType.Literal;
                            continue;

                        //
                        // Mask language edit identifiers (#, 9, &, C, A, a, ?).
                        // Populate a CharDescriptor structure desrcribing the editable char corresponding to this
                        // identifier.
                        //
                        case '0':   // digit required.
                        case 'L':   // letter required.
                        case '&':   // any character required.
                        case 'A':   // alphanumeric (letter or digit) required.
                            this.requiredEditChars++;
                            ch = this.promptChar;                     // replace edit identifier with prompt.
                            charType = CharType.EditRequired;         // set char as editable.
                            break;

                        case '?':   // letter optional (space OK).
                        case '9':   // digit optional (space OK).
                        case '#':   // digit or plus/minus sign optional (space OK).
                        case 'C':   // any character optional (space OK).
                        case 'a':   // alphanumeric (letter or digit) optional.
                            this.optionalEditChars++;
                            ch = this.promptChar;                     // replace edit identifier with prompt.
                            charType = CharType.EditOptional;         // set char as editable.
                            break;

                        //
                        // Literals just break so they're added to the test string.
                        //
                        default:
                            charType = CharType.Literal;
                            break;
                    }
                }
                else
                {
                    escapedChar = false; // reset flag since the escaped char is now going to be added to the test string.
                }

                // Populate a character descriptor for the current character (or loc symbol).
                CharDescriptor chDex = new CharDescriptor(maskPos, charType);

                if (IsEditPosition(chDex))
                {
                    chDex.CaseConversion = caseConversion;
                }

                // Now let's add the character to the string description table.
                // For code clarity we treat all characters as localizable symbols (can have multi-char representation).
                
                if (charType != CharType.Separator)
                {
                    locSymbol = ch.ToString();
                }

                foreach (char chVal in locSymbol)
                {
                    this.testString.Append(chVal);
                    this.stringDescriptor.Add(chDex);
                    testPosition++;
                }
            }

            //
            // Trim test string to needed size.
            //
            this.testString.Capacity = this.testString.Length;
        }

            
        ////// Properties

         
        /// <devdoc>
        ///     Specifies whether the prompt character should be treated as a valid input character or not.
        /// </devdoc>
        public bool AllowPromptAsInput
        {
            get
            {
                return this.flagState[ALLOW_PROMPT_AS_INPUT];
            }
        }

        /// <devdoc>
        ///     Retreives the number of editable characters that have been set.
        /// </devdoc>
        public int AssignedEditPositionCount
        {
            get
            {
                return this.assignedCharCount;
            }
        }

        /// <devdoc>
        ///     Retreives the number of editable characters that have been set.
        /// </devdoc>
        public int AvailableEditPositionCount
        {
            get
            {
                return this.EditPositionCount - this.assignedCharCount;
            }
        }

        /// <devdoc>
        ///     Creates a 'clean' (no text assigned) MaskedTextProvider instance with the same property values as the 
        ///     current instance.
        ///     Derived classes can override this method and call base.Clone to get proper cloning semantics but must
        ///     implement the full-paramter contructor (passing parameters to the base constructor as well).
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2113:SecureLateBindingMethods")]
        public object Clone()
        {
            MaskedTextProvider clonedProvider;
            Type providerType = this.GetType();

            if (providerType == maskTextProviderType)
            {
                clonedProvider = new MaskedTextProvider(
                                                        this.Mask,
                                                        this.Culture,
                                                        this.AllowPromptAsInput,
                                                        this.PromptChar,
                                                        this.PasswordChar,
                                                        this.AsciiOnly);
            }
            else // A derived Type instance used.
            {
                object[] parameters = new object[] 
                {
                    this.Mask,
                    this.Culture,
                    this.AllowPromptAsInput,
                    this.PromptChar,
                    this.PasswordChar,
                    this.AsciiOnly
                };

                clonedProvider = SecurityUtils.SecureCreateInstance(providerType, parameters) as MaskedTextProvider;
            }

            clonedProvider.ResetOnPrompt = false; 
            clonedProvider.ResetOnSpace  = false;
            clonedProvider.SkipLiterals  = false;

            for (int position = 0; position < this.testString.Length; position++)
            {
                CharDescriptor chDex = this.stringDescriptor[position];

                if (IsEditPosition(chDex) && chDex.IsAssigned)
                {
                    clonedProvider.Replace(this.testString[position], position);
                }
            }

            clonedProvider.ResetOnPrompt    = this.ResetOnPrompt;
            clonedProvider.ResetOnSpace     = this.ResetOnSpace;
            clonedProvider.SkipLiterals     = this.SkipLiterals;
            clonedProvider.IncludeLiterals  = this.IncludeLiterals;
            clonedProvider.IncludePrompt    = this.IncludePrompt;

            return clonedProvider;
        }

        /// <devdoc>
        ///     The culture that determines the value of the localizable mask language separators and placeholders.
        /// </devdoc>
        public CultureInfo Culture
        {
            get
            {
                return this.culture;
            }
        }

        /// <devdoc>
        ///       The system password char.
        /// </devdoc>
        public static char DefaultPasswordChar
        {
            get
            {
                // ComCtl32.dll V6 (WindowsXP) provides a nice black circle but we don't want to attempt to simulate it 
                // here to avoid hard coding values.  MaskedTextBox picks up the right value at run time from comctl32.
                return '*';
            }
        }

        /// <devdoc>
        ///       The number of editable positions in the test string.
        /// </devdoc>
        public int EditPositionCount
        {
            get
            {
                return this.optionalEditChars + this.requiredEditChars;
            }
        }

        /// <devdoc>
        ///       Returns a new IEnumerator object containing the editable positions in the test string.
        /// </devdoc>
        public System.Collections.IEnumerator EditPositions
        {
            get
            {
                List<int> editPositions = new List<int>();
                int position = 0;

                foreach( CharDescriptor chDex in this.stringDescriptor )
                {
                    if( IsEditPosition(chDex) )
                    {
                        editPositions.Add(position);
                    }

                    position++;
                }

                return ((System.Collections.IList) editPositions).GetEnumerator();
            }
        }

        /// <devdoc>
        ///     Specifies whether the formatted string should include literals.
        /// </devdoc>
        public bool IncludeLiterals
        {
            get
            {
                return this.flagState[INCLUDE_LITERALS];
            }
            set
            {
                this.flagState[INCLUDE_LITERALS] = value;
            }
        }

        /// <devdoc>
        ///     Specifies whether or not the prompt character should be included in the formatted text when there are
        ///     character slots available in the mask.
        /// </devdoc>
        public bool IncludePrompt
        {
            get
            {
                return this.flagState[INCLUDE_PROMPT];
            }
            set
            {
                this.flagState[INCLUDE_PROMPT] = value;
            }
        }

        /// <devdoc>
        ///     Specifies whether only ASCII characters are accepted as valid input.
        /// </devdoc>
        public bool AsciiOnly
        {
            get
            {
                return this.flagState[ASCII_ONLY];
            }
        }

        /// <devdoc>
        ///     Specifies whether the user text is to be rendered as password characters.
        /// </devdoc>
        public bool IsPassword
        {
            get
            {
                return this.passwordChar != '\0';
            }

            set
            {
                if( this.IsPassword != value )
                {
                    this.passwordChar = value ? DefaultPasswordChar : nullPasswordChar;
                }
            }
        }

        /// <devdoc>
        ///     A negative value representing an index outside the test string.
        /// </devdoc>
        public static int InvalidIndex
        {
            get
            {
                return invalidIndex;
            }
        }

        /// <devdoc>
        ///     The last edit position (relative to the origin not to time) in the test string where 
        ///     an input character has been placed.  If no position has been assigned, InvalidIndex is returned.
        /// </devdoc>
        public int LastAssignedPosition
        {
            get
            {
                return FindAssignedEditPositionFrom( this.testString.Length - 1, backward );
            }
        }

        /// <devdoc>
        ///     Specifies the length of the test string.
        /// </devdoc>
        public int Length
        {
            get
            {
                return this.testString.Length;
            }
        }

        /// <devdoc>
        ///     The mask to be applied to the test string.
        /// </devdoc>
        public string Mask
        {
            get
            { 
                return this.mask; 
            }
        }

        /// <devdoc>
        ///     Specifies whether all required inputs have been provided into the mask successfully.
        /// </devdoc>
        public bool MaskCompleted
        {
            get
            {
                Debug.Assert( this.assignedCharCount >= 0, "Invalid count of assigned chars." );
                return this.requiredCharCount == this.requiredEditChars;
            }
        }

        /// <devdoc>
        ///     Specifies whether all inputs (required and optional) have been provided into the mask successfully.
        /// </devdoc>
        public bool MaskFull
        {
            get
            {
                Debug.Assert(this.assignedCharCount >= 0, "Invalid count of assigned chars.");
                return this.assignedCharCount == this.EditPositionCount;
            }
        }

        /// <devdoc>
        ///     Specifies the character to be used in the formatted string in place of editable characters.
        ///     Use the null character '\0' to reset this property.
        /// </devdoc>
        public char PasswordChar
        {
            get
            {
                return this.passwordChar;
            }
        
            set
            {
                if( value == this.promptChar )
                {
                    // Prompt and password chars must be different.
                    throw new InvalidOperationException( SR.GetString(SR.MaskedTextProviderPasswordAndPromptCharError) );
                }

                if( !IsValidPasswordChar(value) && (value != nullPasswordChar))
                {
                    // Same message as in SR.MaskedTextBoxInvalidCharError.
                    throw new ArgumentException( SR.GetString(SR.MaskedTextProviderInvalidCharError) );
                }

                if (value != this.passwordChar)
                {
                    this.passwordChar = value;
                }
            }
        } 

        /// <devdoc>
        ///     Specifies the prompt character to be used in the formatted string for unsupplied characters.
        /// </devdoc>
        public char PromptChar
        {
            get
            {
                return this.promptChar;
            }
        
            set
            {
                if( value == this.passwordChar )
                {
                    // Prompt and password chars must be different.
                    throw new InvalidOperationException( SR.GetString(SR.MaskedTextProviderPasswordAndPromptCharError) );
                }

                if (!IsPrintableChar(value))
                {
                    // Same message as in SR.MaskedTextBoxInvalidCharError.
                    throw new ArgumentException( SR.GetString(SR.MaskedTextProviderInvalidCharError) );
                }
                
                if (value != this.promptChar)
                {
                    this.promptChar = value;

                    for(int position = 0; position < this.testString.Length; position++ )
                    {
                        CharDescriptor chDex = this.stringDescriptor[position];

                        if (IsEditPosition(position) && !chDex.IsAssigned)
                        {
                            this.testString[position] = this.promptChar;
                        }   
                    }
                }
            }
        }

        
        
        /// <devdoc>
        ///     Specifies whether to reset and skip the current position if editable, when the input character has 
        ///     the same value as the prompt.
        ///     
        ///     This is useful when assigning text that was saved including the prompt; in this case
        ///     we don't want to take the prompt character as valid input but don't want to fail the test either. 
        /// </devdoc>
        public bool ResetOnPrompt
        {
            get
            {
                return this.flagState[RESET_ON_PROMPT];
            }
            set
            {
                this.flagState[RESET_ON_PROMPT] = value;
            }
        }

        /// <devdoc>
        ///     Specifies whether to reset and skip the current position if editable, when the input is the space character.
        ///
        ///     This is useful when assigning text that was saved excluding the prompt (prompt replaced with spaces); 
        ///     in this case we don't want to take the space but instead, reset the postion (or just skip it) so the 
        ///     next input character gets positioned correctly.
        /// </devdoc>
        public bool ResetOnSpace
        {
            get
            {
                return this.flagState[SKIP_SPACE];
            }
            set
            {
                this.flagState[SKIP_SPACE] = value;
            }
        }

        
        /// <devdoc>
        ///     Specifies whether to skip the current position if non-editable and the input character has the same 
        ///     value as the literal at that position.
        /// 
        ///     This is useful for round-tripping the text when saved with literals; when assigned back we don't want
        ///     to treat literals as input.
        /// </devdoc>
        public bool SkipLiterals
        {
            get
            {
                return this.flagState[RESET_ON_LITERALS];
            }
            set
            {
                this.flagState[RESET_ON_LITERALS] = value;
            }
        }

        /// <devdoc>
        ///     Indexer.
        /// </devdoc>
        public char this[int index] 
        {
            get
            {
                if( index < 0 || index >= this.testString.Length )
                {
                    throw new IndexOutOfRangeException(index.ToString(CultureInfo.CurrentCulture));
                }

                return this.testString[index];
            }
        }

        ////// Methods
        
        /// <devdoc>
        ///     Attempts to add the specified charactert to the last unoccupied positions in the test string (append text to 
        ///     the virtual string).
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Add( char input )
        {
            int dummyVar;
            MaskedTextResultHint dummyVar2;
            return Add( input, out dummyVar, out dummyVar2);
        }
        
        /// <devdoc>
        ///     Attempts to add the specified charactert to the last unoccupied positions in the test string (append text to 
        ///     the virtual string).
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful,
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives a hint about the operation result reason.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Add( char input, out int testPosition, out MaskedTextResultHint resultHint )
        {
            int lastAssignedPos = this.LastAssignedPosition;

            if( lastAssignedPos == this.testString.Length - 1 )    // at the last edit char position.
            {
                testPosition = this.testString.Length;
                resultHint = MaskedTextResultHint.UnavailableEditPosition;
                return false;
            }

            // Get position after last assigned position.
            testPosition = lastAssignedPos + 1;
            testPosition = FindEditPositionFrom( testPosition, forward );

            if( testPosition == invalidIndex )
            {
                resultHint = MaskedTextResultHint.UnavailableEditPosition;
                testPosition = this.testString.Length;
                return false;
            }

            if( !TestSetChar( input, testPosition, out resultHint ) )
            {
                return false;
            }

            return true;
        }

        /// <devdoc>
        ///     Attempts to add the characters in the specified string to the last unoccupied positions in the test string
        ///     (append text to the virtual string).
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Add( string input )
        {
            int dummyVar;
            MaskedTextResultHint dummyVar2;
            return Add( input, out dummyVar, out dummyVar2 );
        }

        /// <devdoc>
        ///     Attempts to add the characters in the specified string to the last unoccupied positions in the test string
        ///     (append text to the virtual string).
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives a hint about the operation result reason.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Add( string input, out int testPosition, out MaskedTextResultHint resultHint )
        {
            if( input == null )
            {
                throw new ArgumentNullException( "input" );
            }

            testPosition = this.LastAssignedPosition + 1;
            
            if( input.Length == 0 ) // nothing to add.
            {
                // Get position where the test would be performed.
                resultHint = MaskedTextResultHint.NoEffect;
                return true;    
            }

            return TestSetString(input, testPosition, out testPosition, out resultHint);
        }

        /// <devdoc>
        ///     Resets the state of the test string edit chars. (Remove all characters from the virtual string).
        /// </devdoc>
        public void Clear()
        {
            MaskedTextResultHint dummyHint;
            Clear( out dummyHint );
        }

        /// <devdoc>
        ///     Resets the state of the test string edit chars. (Remove all characters from the virtual string).
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        /// </devdoc>
        public void Clear( out MaskedTextResultHint resultHint )
        {
            if (this.assignedCharCount == 0)
            {
                resultHint = MaskedTextResultHint.NoEffect;
                return;
            }

            resultHint = MaskedTextResultHint.Success;

            for( int position = 0; position < this.testString.Length; position++ )
            {
                ResetChar( position );
            }
        }

        /// <devdoc>
        ///     Gets the position of the first edit char in the test string, the search starts from the specified 
        ///     position included.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        public int FindAssignedEditPositionFrom(int position, bool direction)
        {
            if (this.assignedCharCount == 0)
            {
                return invalidIndex;
            }

            int startPosition;
            int endPosition;

            if( direction == forward )
            {
                startPosition = position;
                endPosition   = this.testString.Length - 1;
            }
            else
            {
                startPosition = 0;
                endPosition   = position;
            }

            return FindAssignedEditPositionInRange(startPosition, endPosition, direction); 
        }

        /// <devdoc>
        ///     Gets the position of the first edit char in the test string in the specified range, the search starts from 
        ///     the specified  position included.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        public int FindAssignedEditPositionInRange(int startPosition, int endPosition, bool direction)
        {
            if (this.assignedCharCount == 0)
            {
                return invalidIndex;
            } 
            
            return FindEditPositionInRange(startPosition, endPosition, direction, editAssigned);
        }

        /// <devdoc>
        ///     Gets the position of the first assigned edit char in the test string, the search starts from the specified
        ///     position included and in the direction specified (true == forward).  The positions are relative to the test
        ///     string.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        public int FindEditPositionFrom(int position, bool direction)
        {
            int startPosition;
            int endPosition;

            if( direction == forward )
            {
                startPosition = position;
                endPosition   = this.testString.Length - 1;
            }
            else
            {
                startPosition = 0;
                endPosition   = position;
            }

            return FindEditPositionInRange(startPosition, endPosition, direction);
        }

        /// <devdoc>
        ///     Gets the position of the first assigned edit char in the test string; the search is performed in the specified
        ///     positions range and in the specified direction.
        ///     The positions are relative to the test string.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        public int FindEditPositionInRange(int startPosition, int endPosition, bool direction)
        {
            CharType editCharFlags = CharType.EditOptional | CharType.EditRequired;
            return FindPositionInRange(startPosition, endPosition, direction, editCharFlags );
        }

        /// <devdoc>
        ///     Gets the position of the first edit char in the test string in the specified range, according to the 
        ///     assignedRequired parameter; if true, it gets the first assigned position otherwise the first unassigned one.
        ///     The search starts from the specified position included.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        private int FindEditPositionInRange(int startPosition, int endPosition, bool direction, byte assignedStatus)
        {
            // out of range position is handled in FindEditPositionFrom method.
            int testPosition;

            do
            {
                testPosition = FindEditPositionInRange(startPosition, endPosition, direction);

                if (testPosition == invalidIndex)  // didn't find any.
                {
                    break;
                }

                CharDescriptor chDex = this.stringDescriptor[testPosition];

                switch( assignedStatus )
                {
                    case editUnassigned:
                        if( !chDex.IsAssigned )
                        {
                            return testPosition;
                        }
                        break;

                    case editAssigned:
                        if( chDex.IsAssigned )
                        {
                            return testPosition;
                        }
                        break;

                    default: // don't care
                        return testPosition;
                }

                if( direction == forward )
                {
                    startPosition++;
                }
                else
                {
                    endPosition--;
                }
            }
            while( startPosition <= endPosition );

            return invalidIndex;
        }

        /// <devdoc>
        ///     Gets the position of the first non edit position in the test string; the search is performed from the specified
        ///     position and in the specified direction.
        ///     The positions are relative to the test string.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        public int FindNonEditPositionFrom(int position, bool direction)
        {
            int startPosition;
            int endPosition;

            if (direction == forward)
            {
                startPosition = position;
                endPosition = this.testString.Length - 1;
            }
            else
            {
                startPosition = 0;
                endPosition = position;
            } 

            return FindNonEditPositionInRange(startPosition, endPosition, direction);
        }

        /// <devdoc>
        ///     Gets the position of the first non edit position in the test string; the search is performed in the specified
        ///     positions range and in the specified direction.
        ///     The positions are relative to the test string.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        public int FindNonEditPositionInRange(int startPosition, int endPosition, bool direction)
        {
            CharType literalCharFlags = CharType.Literal | CharType.Separator;
            return FindPositionInRange(startPosition, endPosition, direction, literalCharFlags );
        }

        /// <devdoc>
        ///     Finds a position in the test string according to the needed position type (needEditPos).
        ///     The positions are relative to the test string.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        private int FindPositionInRange(int startPosition, int endPosition, bool direction, CharType charTypeFlags)
        {
            if (startPosition < 0)
            {
                startPosition = 0;
            }

            if (endPosition >= this.testString.Length)
            {
                endPosition = this.testString.Length - 1;
            }

            if (startPosition > endPosition)
            {
                return invalidIndex;
            }

            // Iterate through the test string until we find an edit char position.
            int testPosition;

            while (startPosition <= endPosition)
            {
                testPosition = (direction == forward) ? startPosition++ : endPosition--;

                CharDescriptor chDex = this.stringDescriptor[testPosition];

                if ((chDex.CharType & charTypeFlags) == chDex.CharType)
                {
                    return testPosition;
                }
            }

            return invalidIndex;
        }

        /// <devdoc>
        ///     Gets the position of the first edit char in the test string, the search starts from the specified 
        ///     position included.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        public int FindUnassignedEditPositionFrom(int position, bool direction)
        {
            int startPosition;
            int endPosition;

            if (direction == forward)
            {
                startPosition = position;
                endPosition   = this.testString.Length - 1;
            }
            else
            {
                startPosition = 0;
                endPosition   = position;
            } 
            
            return FindEditPositionInRange(startPosition, endPosition, direction, editUnassigned);
        }

        /// <devdoc>
        ///     Gets the position of the first edit char in the test string in the specified range; the search starts
        ///     from the specified position included.
        ///     Returns InvalidIndex if it doesn't find one.
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
        public int FindUnassignedEditPositionInRange(int startPosition, int endPosition, bool direction)
        {
            int position;

            while( true )
            {
                position = FindEditPositionInRange(startPosition, endPosition, direction, editAny );

                if( position == invalidIndex )
                {
                    return invalidIndex;
                }
                
                CharDescriptor chDex = this.stringDescriptor[position];

                if( !chDex.IsAssigned )
                {
                    return position;
                }

                if( direction == forward )
                {
                    startPosition++;
                }
                else
                {
                    endPosition--;
                }
            }
        }

        /// <devdoc>
        ///     Specifies whether the specified MaskedTextResultHint denotes success or not.
        /// </devdoc>
        public static bool GetOperationResultFromHint( MaskedTextResultHint hint )
        {
            return ((int)hint) > 0;
        }

        /// <devdoc>
        ///     Attempts to insert the specified character at the specified position in the test string. 
        ///     (Insert character in the virtual string).
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool InsertAt(char input, int position)
        {
            if (position < 0 || position >= this.testString.Length)
            {
                return false;
                //throw new ArgumentOutOfRangeException("position");
            }

            return InsertAt(input.ToString(), position);
        }

        /// <devdoc>
        ///     Attempts to insert the specified character at the specified position in the test string, shifting characters
        ///     at upper positions (if any) to make room for the input.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool InsertAt(char input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            return InsertAt(input.ToString(), position, out testPosition, out resultHint);
        }

        /// <devdoc>
        ///     Attempts to insert the characters in the specified string in at the specified position in the test string.
        ///     (Insert characters in the virtual string).
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool InsertAt(string input, int position)
        {
            int dummyVar;
            MaskedTextResultHint dummyVar2;
            return InsertAt(input, position, out dummyVar, out dummyVar2);
        }

        /// <devdoc>
        ///     Attempts to insert the characters in the specified string in at the specified position in the test string,
        ///     shifting characters at upper positions (if any) to make room for the input.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool InsertAt(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (position < 0 || position >= this.testString.Length)
            {
                testPosition = position;
                resultHint   = MaskedTextResultHint.PositionOutOfRange;
                return false;
                //throw new ArgumentOutOfRangeException("position");
            }

            return InsertAtInt(input, position, out testPosition, out resultHint, false);
        }

        /// <devdoc>
        ///     Attempts to insert the characters in the specified string in at the specified position in the test string,
        ///     shifting characters at upper positions (if any) to make room for the input.
        ///     It performs the insertion if the testOnly parameter is false and the test passes.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        private bool InsertAtInt(string input, int position, out int testPosition, out MaskedTextResultHint resultHint, bool testOnly)
        {
            Debug.Assert(input != null && position >= 0 && position < this.testString.Length, "input param out of range.");

            if (input.Length == 0) // nothing to insert.
            {
                testPosition = position;
                resultHint   = MaskedTextResultHint.NoEffect;
                return true;
            }

            // Test input string first.  testPosition will containt the position of the last inserting character from the input.
            if (!TestString(input, position, out testPosition, out resultHint))
            {
                return false;
            }

            // Now check if we need to open room for the input characters (shift characters right) and if so test the shifting characters.

            int  srcPos          = FindEditPositionFrom(position, forward);               // source position.
            bool shiftNeeded     = FindAssignedEditPositionInRange(srcPos, testPosition, forward) != invalidIndex;
            int  lastAssignedPos = this.LastAssignedPosition;

            if( shiftNeeded && (testPosition == this.testString.Length - 1)) // no room for shifting.
            {
                resultHint   = MaskedTextResultHint.UnavailableEditPosition;
                testPosition = this.testString.Length;
                return false;
            }
            
            int dstPos = FindEditPositionFrom(testPosition + 1, forward);  // destination position.

            if (shiftNeeded)
            {
                // Temp hint used not to overwrite the primary operation result hint (from TestString).
                MaskedTextResultHint tempHint = MaskedTextResultHint.Unknown;

                // Test shifting characters.
                while (true)
                {
                    if (dstPos == invalidIndex)
                    {
                        resultHint   = MaskedTextResultHint.UnavailableEditPosition;
                        testPosition = this.testString.Length;
                        return false;
                    }

                    CharDescriptor chDex = this.stringDescriptor[srcPos];

                    if (chDex.IsAssigned) // only test assigned positions.
                    {
                        if (!TestChar(this.testString[srcPos], dstPos, out tempHint))
                        {
                            resultHint   = tempHint;
                            testPosition = dstPos;
                            return false;
                        }
                    }

                    if (srcPos == lastAssignedPos) // all shifting positions tested?
                    {
                        break;
                    }

                    srcPos = FindEditPositionFrom(srcPos + 1, forward);
                    dstPos = FindEditPositionFrom(dstPos + 1, forward);
                }

                if (tempHint > resultHint)
                {
                    resultHint = tempHint;
                }
            }

            if (testOnly)
            {
                return true; // test done!
            }

            // Tests passed so we can go ahead and shift the existing characters (if needed) and insert the new ones.

            if (shiftNeeded)
            {
                while (srcPos >= position)
                {
                    CharDescriptor chDex = this.stringDescriptor[srcPos];

                    if (chDex.IsAssigned)
                    {
                        SetChar(this.testString[srcPos], dstPos);
                    }
                    else
                    {
                        ResetChar(dstPos);
                    }

                    dstPos = FindEditPositionFrom(dstPos - 1, backward);
                    srcPos = FindEditPositionFrom(srcPos - 1, backward); 
                }
            }

            // Finally set the input characters.
            SetString(input, position);

            return true;
        }

        /// <devdoc>
        ///     Helper function for testing char in ascii mode.
        /// </devdoc>
        private static bool IsAscii(char c)
        {
            //ASCII non-control chars ['!'-'/', '0'-'9', ':'-'@', 'A'-'Z', '['-'''', 'a'-'z', '{'-'~'] all consecutive.
            return (c >= '!' && c <= '~');
        }

        /// <devdoc>
        ///     Helper function for alphanumeric char in ascii mode.
        /// </devdoc>
        private static bool IsAciiAlphanumeric(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        /// <devdoc>
        ///     Helper function for testing mask language alphanumeric identifiers.
        /// </devdoc>
        private static bool IsAlphanumeric(char c)
        {
            return char.IsLetter(c) || char.IsDigit(c);
        }

        /// <devdoc>
        ///     Helper function for testing letter char in ascii mode.
        /// </devdoc>
        private static bool IsAsciiLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        /// <devdoc>
        ///     Checks wheteher the specified position is available for assignment.  Returns false if it is assigned
        ///     or it is not editable, true otherwise.
        /// </devdoc>
        public bool IsAvailablePosition(int position)
        {
            if (position < 0 || position >= this.testString.Length)
            {
                return false;
                //throw new ArgumentOutOfRangeException("position");
            }

            CharDescriptor chDex = this.stringDescriptor[position];
            return IsEditPosition(chDex) && !chDex.IsAssigned;
        }

        /// <devdoc>
        ///     Checks wheteher the specified position in the test string is editable.
        /// </devdoc>
        public bool IsEditPosition(int position)
        {
            if (position < 0 || position >= this.testString.Length)
            {
                return false;
                //throw new ArgumentOutOfRangeException("position");
            }

            CharDescriptor chDex = this.stringDescriptor[position];
            return IsEditPosition( chDex );
        }

        private static bool IsEditPosition( CharDescriptor charDescriptor )
        {
            return (charDescriptor.CharType == CharType.EditRequired || charDescriptor.CharType == CharType.EditOptional);
        }

        /// <devdoc>
        ///     Checks wheteher the character in the specified position is a literal and the same as the specified character.
        /// </devdoc>
        private static bool IsLiteralPosition( CharDescriptor charDescriptor )
        {
            return (charDescriptor.CharType == CharType.Literal) || (charDescriptor.CharType == CharType.Separator);
        }

        /// <devdoc>
        ///     Checks wheteher the specified character is valid as part of a mask or an input string.  
        /// </devdoc>
        private static bool IsPrintableChar( char c )
        {
            return char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || (c == spaceChar);
        }

        /// <devdoc>
        ///     Checks wheteher the specified character is a valid input char.  
        /// </devdoc>
        public static bool IsValidInputChar( char c )
        {
            return IsPrintableChar( c );
        }

        /// <devdoc>
        ///     Checks wheteher the specified character is a valid input char.  
        /// </devdoc>
        public static bool IsValidMaskChar( char c )
        {
            return IsPrintableChar( c );
        }

        /// <devdoc>
        ///     Checks wheteher the specified character is a valid password char.
        /// </devdoc>
        public static bool IsValidPasswordChar( char c )
        {
            return IsPrintableChar(c) || (c == '\0');  // null character means password reset.
        }

        /// <devdoc>
        ///     Removes the last character from the formatted string. (Remove last character in virtual string).
        /// </devdoc>
        public bool Remove()
        {
            int dummyVar;
            MaskedTextResultHint dummyVar2;
            return Remove(out dummyVar, out dummyVar2);
        }

        /// <devdoc>
        ///     Removes the last character from the formatted string. (Remove last character in virtual string).
        ///     On exit the out param contains the position where the operation was actually performed.
        ///     This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Remove(out int testPosition, out MaskedTextResultHint resultHint)
        {
            int lastAssignedPos = this.LastAssignedPosition;

            if (lastAssignedPos == invalidIndex)
            {
                testPosition = 0;
                resultHint = MaskedTextResultHint.NoEffect;
                return true; // nothing to remove.
            }

            ResetChar(lastAssignedPos);

            testPosition = lastAssignedPos;
            resultHint   = MaskedTextResultHint.Success;

            return true;
        }

        /// <devdoc>
        ///     Removes the character from the formatted string at the specified position and shifts characters
        ///     left.
        ///     True if character shifting is successful.  
        /// </devdoc>
        public bool RemoveAt(int position)
        {
            return RemoveAt(position, position);
        }

        /// <devdoc>
        ///     Removes all characters in edit position from in the test string at the specified start and end positions 
        ///     and shifts any remaining characters left.  (Remove characters from the virtual string).
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool RemoveAt(int startPosition, int endPosition)
        {
            int dummyVar;
            MaskedTextResultHint dummyVar2;
            return RemoveAt(startPosition, endPosition, out dummyVar, out dummyVar2);
        }

        /// <devdoc>
        ///     Removes all characters in edit position from in the test string at the specified start and end positions 
        ///     and shifts any remaining characters left.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool RemoveAt(int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (endPosition >= this.testString.Length)
            {
                testPosition = endPosition;
                resultHint   = MaskedTextResultHint.PositionOutOfRange;
                return false;
                //throw new ArgumentOutOfRangeException("endPosition");
            }

            if (startPosition < 0 || startPosition > endPosition)
            {
                testPosition = startPosition;
                resultHint   = MaskedTextResultHint.PositionOutOfRange;
                return false;
                //throw new ArgumentOutOfRangeException("startPosition");
            }

            return RemoveAtInt(startPosition, endPosition, out testPosition, out resultHint, /*testOnly*/ false);
        }

        /// <devdoc>
        ///     Removes all characters in edit position from in the test string at the specified start and end positions 
        ///     and shifts any remaining characters left.
        ///     If testOnly parameter is set to false and the test passes it performs the operations on the characters.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        private bool RemoveAtInt(int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint, bool testOnly)
        {
            Debug.Assert(startPosition >= 0 && startPosition <= endPosition && endPosition < this.testString.Length, "Out of range input value.");

            // Check if we need to shift characters left to occupied the positions left by the characters being removed.
            int lastAssignedPos = this.LastAssignedPosition;
            int dstPos          = FindEditPositionInRange(startPosition, endPosition, forward); // first edit position in range.

            resultHint = MaskedTextResultHint.NoEffect;

            if (dstPos == invalidIndex || dstPos > lastAssignedPos) // nothing to remove.
            {
                testPosition = startPosition;
                return true;
            }

            testPosition = startPosition;    // On remove range, testPosition remains the same as startPosition.

            bool shiftNeeded = endPosition < lastAssignedPos; // last assigned position is upper.

            // if there are assigned characters to be removed (could be that the range doesn't have one, in such case we may be just 
            // be shifting chars), the result hint is success, let's check.
            if (FindAssignedEditPositionInRange(startPosition, endPosition, forward) != invalidIndex)
            {
                resultHint = MaskedTextResultHint.Success;
            }

            if (shiftNeeded)
            {
                // Test shifting characters.

                int srcPos = FindEditPositionFrom(endPosition + 1, forward);  // first position to shift left.
                int shiftStart = srcPos; // cache it here so we don't have to search for it later if needed.
                MaskedTextResultHint testHint;

                startPosition = dstPos; // actual start position.

                while(true)
                {
                    char srcCh = this.testString[srcPos];
                    CharDescriptor chDex = this.stringDescriptor[srcPos];
                    
                    // if the shifting character is the prompt and it is at an unassigned position we don't need to test it.
                    if (srcCh != this.PromptChar || chDex.IsAssigned)
                    {
                        if (!TestChar(srcCh, dstPos, out testHint))
                        {
                            resultHint   = testHint;
                            testPosition = dstPos; // failed position.
                            return false;
                        }
                    }

                    if (srcPos == lastAssignedPos)
                    {
                        break;
                    }

                    srcPos = FindEditPositionFrom( srcPos + 1, forward);
                    dstPos = FindEditPositionFrom( dstPos + 1, forward);
                }

                // shifting characters is a resultHint == sideEffect, update hint if no characters removed (which would be hint == success).
                if( MaskedTextResultHint.SideEffect > resultHint )
                {
                    resultHint = MaskedTextResultHint.SideEffect;
                }

                if (testOnly)
                {
                    return true; // test completed.
                }

                // test passed so shift characters.
                srcPos = shiftStart;
                dstPos = startPosition;

                while(true)
                {
                    char srcCh = this.testString[srcPos];
                    CharDescriptor chDex = this.stringDescriptor[srcPos];
                    
                    // if the shifting character is the prompt and it is at an unassigned position we just reset the destination position.
                    if (srcCh == this.PromptChar && !chDex.IsAssigned)
                    {
                        ResetChar(dstPos);
                    }
                    else
                    {
                        SetChar(srcCh, dstPos);
                        ResetChar( srcPos );
                    }

                    if (srcPos == lastAssignedPos)
                    {
                        break;
                    }

                    srcPos = FindEditPositionFrom( srcPos + 1, forward );
                    dstPos = FindEditPositionFrom( dstPos + 1, forward );
                }

                // If shifting character are less than characters to remove in the range, we need to remove the remaining ones in the range; 
                // update startPosition and ResetString belwo will take care of that.
                startPosition = dstPos + 1;
            }

            if( startPosition <= endPosition )
            {
                ResetString(startPosition, endPosition);
            }

            return true;
        }

        /// <devdoc>
        ///     Replaces the first editable character in the test string from the specified position, with the specified 
        ///     character (Replace is performed in the virtual string), unless the character at the specified position 
        ///     is to be escaped.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Replace(char input, int position)
        {
            int dummyVar;
            MaskedTextResultHint dummyVar2;
            return Replace(input, position, out dummyVar, out dummyVar2);
        }

        /// <devdoc>
        ///     Replaces the first editable character in the test string from the specified position, with the specified 
        ///     character, unless the character at the specified position is to be escaped.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Replace(char input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (position < 0 || position >= this.testString.Length)
            {
                testPosition = position;
                resultHint   = MaskedTextResultHint.PositionOutOfRange;
                return false;
                //throw new ArgumentOutOfRangeException("position");
            }

            testPosition = position;

            // If character is not to be escaped, we need to find the first edit position to test it in.
            if (!TestEscapeChar( input, testPosition ))
            {
                testPosition = FindEditPositionFrom( testPosition, forward );
            }

            if (testPosition == invalidIndex)
            {
                resultHint   = MaskedTextResultHint.UnavailableEditPosition;
                testPosition = position;
                return false;
            }

            if (!TestSetChar(input, testPosition, out resultHint))
            {
                return false;
            }

            return true;
        }


        /// <devdoc>
        ///     Replaces the first editable character in the test string from the specified position, with the specified 
        ///     character and removes any remaining characters in the range unless the character at the specified position 
        ///     is to be escaped.
        ///     If specified range covers more than one assigned edit character, shift-left is performed after replacing
        ///     the first character.  This is useful when in a edit box the user selects text and types a character to replace it.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Replace(char input, int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (endPosition >= this.testString.Length)
            {
                testPosition = endPosition;
                resultHint   = MaskedTextResultHint.PositionOutOfRange;
                return false;
                //throw new ArgumentOutOfRangeException("endPosition");
            }

            if (startPosition < 0 || startPosition > endPosition)
            {
                testPosition = startPosition;
                resultHint   = MaskedTextResultHint.PositionOutOfRange;
                return false;
                //throw new ArgumentOutOfRangeException("startPosition");
            }

            if (startPosition == endPosition)
            {
                testPosition = startPosition;
                return TestSetChar(input, startPosition, out resultHint);
            }

            return Replace( input.ToString(), startPosition, endPosition, out testPosition, out resultHint );
        }

        /// <devdoc>
        ///     Replaces the character at the first edit position from the one specified with the first character in the input;
        ///     the rest of the characters in the input will be placed in the test string according to the InsertMode (insert/replace).
        ///     (Replace is performed in the virtual text).
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Replace(string input, int position)
        {
            int dummyVar;
            MaskedTextResultHint dummyVar2;
            return Replace(input, position, out dummyVar, out dummyVar2);
        }

        /// <devdoc>
        ///     Replaces the character at the first edit position from the one specified with the first character in the input;
        ///     the rest of the characters in the input will be placed in the test string according to the InsertMode (insert/replace),
        ///     shifting characters at upper positions (if any) to make room for the entire input.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Replace(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (position < 0 || position >= this.testString.Length)
            {
                testPosition = position;
                resultHint   = MaskedTextResultHint.PositionOutOfRange;
                return false;
                //throw new ArgumentOutOfRangeException("position");
            }

            if (input.Length == 0) // remove the character at position.
            {
                return RemoveAt(position, position, out testPosition, out resultHint );
            }

            // At this point, we are replacing characters with the ones in the input.

            if (!TestSetString(input, position, out testPosition, out resultHint))
            {
                return false;
            }

            return true;
        }

        /// <devdoc>
        ///     Replaces the characters in the specified range with the characters in the input string and shifts 
        ///     characters appropriately (removing or inserting characters according to whether the input string is
        ///     shorter or larger than the specified range.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool Replace(string input, int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (endPosition >= this.testString.Length)
            {
                testPosition = endPosition;
                resultHint   = MaskedTextResultHint.PositionOutOfRange;
                return false;
                //throw new ArgumentOutOfRangeException("endPosition");
            }

            if (startPosition < 0 || startPosition > endPosition)
            {
                testPosition = startPosition;
                resultHint   = MaskedTextResultHint.PositionOutOfRange;
                return false;
                //throw new ArgumentOutOfRangeException("startPosition");
            }

            if (input.Length == 0) // remove character at position.
            {
                return RemoveAt( startPosition, endPosition, out testPosition, out resultHint );
            }
        
            // If replacing the entire text with a same-lenght text, we are just setting (not replacing) the test string to the new value;
            // in this case we just call SetString.
            // If the text length is different than the specified range we would need to remove or insert characters; there are three possible
            // cases as follows:
            // 1. The text length is the same as edit positions in the range (or no assigned chars): just replace the text, no additional operations needed.
            // 2. The text is shorter: replace the text in the text string and remove (range - text.Length) characters.
            // 3. The text is larger: replace range count characters and insert (range - text.Length) characters.
            
            // Test input string first and get the last test position to determine what to do.
            if( !TestString( input, startPosition, out testPosition, out resultHint ))
            {
                return false;
            }

            if (this.assignedCharCount > 0)
            {
                // cache out params to preserve the ones from the primary operation (in case of success).
                int tempPos;
                MaskedTextResultHint tempHint;

                if (testPosition < endPosition) // Case 2. Replace + Remove.
                {
                    // Test removing remaining characters.
                    if (!RemoveAtInt(testPosition + 1, endPosition, out tempPos, out tempHint, /*testOnly*/ false))
                    {
                        testPosition = tempPos;
                        resultHint = tempHint;
                        return false;
                    }

                    // If current result hint is not success (no effect), and character shifting is actually performed, hint = side effect.
                    if (tempHint == MaskedTextResultHint.Success && resultHint != tempHint)
                    {
                        resultHint = MaskedTextResultHint.SideEffect;
                    }
                }
                else if (testPosition > endPosition) // Case 3. Replace + Insert.
                {
                    // Test shifting existing characters to make room for inserting part of the input.
                    int lastAssignedPos = this.LastAssignedPosition;
                    int dstPos = testPosition + 1;
                    int srcPos = endPosition + 1;

                    while (true)
                    {
                        srcPos = FindEditPositionFrom(srcPos, forward);
                        dstPos = FindEditPositionFrom(dstPos, forward);

                        if (dstPos == invalidIndex)
                        {
                            testPosition = this.testString.Length;
                            resultHint = MaskedTextResultHint.UnavailableEditPosition;
                            return false;
                        }

                        if (!TestChar(this.testString[srcPos], dstPos, out tempHint))
                        {
                            testPosition = dstPos;
                            resultHint = tempHint;
                            return false;
                        }

                        // If current result hint is not success (no effect), and character shifting is actually performed, hint = success effect.
                        if (tempHint == MaskedTextResultHint.Success && resultHint != tempHint)
                        {
                            resultHint = MaskedTextResultHint.Success;
                        }

                        if (srcPos == lastAssignedPos)
                        {
                            break;
                        }

                        srcPos++;
                        dstPos++;
                    }

                    // shift test passed, now do it.

                    while (dstPos > testPosition)
                    {
                        SetChar(this.testString[srcPos], dstPos);

                        srcPos = FindEditPositionFrom(srcPos - 1, backward);
                        dstPos = FindEditPositionFrom(dstPos - 1, backward);
                    }
                }
                // else endPosition == testPosition, this means replacing the entire text which is the same as Set().
            }

            // in all cases we need to replace the input.
            SetString( input, startPosition );
            return true;
        }

        /// <devdoc>
        ///     Resets the test string character at the specified position.
        /// </devdoc>
        private void ResetChar(int testPosition)
        {
            CharDescriptor chDex = this.stringDescriptor[testPosition];

            if (IsEditPosition(testPosition) && chDex.IsAssigned)
            {
                chDex.IsAssigned = false;
                this.testString[testPosition] = this.promptChar;
                this.assignedCharCount--;

                if (chDex.CharType == CharType.EditRequired)
                {
                    this.requiredCharCount--;
                }

                Debug.Assert(this.assignedCharCount >= 0, "Invalid count of assigned chars.");
            }
        }

        /// <devdoc>
        ///     Resets characters in the test string in the range defined by the specified positions.
        ///     Position is relative to the test string and count is the number of edit characters to reset.
        /// </devdoc>
        private void ResetString(int startPosition, int endPosition)
        {
            Debug.Assert( startPosition >= 0 && endPosition >= 0 && endPosition >= startPosition && endPosition < this.testString.Length, "position out of range."  );

            startPosition = FindAssignedEditPositionFrom( startPosition, forward );

            if( startPosition != invalidIndex )
            {
                endPosition = FindAssignedEditPositionFrom( endPosition, backward );

                while (startPosition <= endPosition)
                {
                    startPosition = FindAssignedEditPositionFrom( startPosition, forward );
                    ResetChar(startPosition);
                    startPosition++;
                }
            }
        }

        /// <devdoc>
        ///     Sets the edit characters in the test string to the ones specified in the input string if all characters
        ///     are valid.
        ///     If passwordChar is assigned, it is rendered in the output string instead of the user-supplied values.
        /// </devdoc>
        public bool Set(string input)
        {
            int dummyVar;
            MaskedTextResultHint dummyVar2;

            return Set(input, out dummyVar, out dummyVar2);
        }

        /// <devdoc>
        ///     Sets the edit characters in the test string to the ones specified in the input string if all characters
        ///     are valid.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     If passwordChar is assigned, it is rendered in the output string instead of the user-supplied values.
        /// </devdoc>
        public bool Set(string input, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            resultHint = MaskedTextResultHint.Unknown;
            testPosition = 0;

            if (input.Length == 0) // Clearing the input text.
            {
                Clear(out resultHint);
                return true;
            }

            if (!TestSetString(input, testPosition, out testPosition, out resultHint))
            {
                return false;
            }

            // Reset remaining characters (if any).
            int resetPos = FindAssignedEditPositionFrom(testPosition + 1, forward);

            if (resetPos != invalidIndex)
            {
                ResetString(resetPos, this.testString.Length - 1);
            }

            return true;
        }

        /// <devdoc>
        ///     Sets the character at the specified position in the test string to the specified value.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        private void SetChar(char input, int position)
        {
            Debug.Assert(position >= 0 && position < this.testString.Length, "Position out of range.");

            CharDescriptor chDex = this.stringDescriptor[position];
            SetChar(input, position, chDex);
        }

        /// <devdoc>
        ///     Sets the character at the specified position in the test string to the specified value.
        ///     SetChar increments the number of assigned characters in the test string.
        /// </devdoc>
        private void SetChar(char input, int position, CharDescriptor charDescriptor)
        {
            Debug.Assert(position >= 0 && position < this.testString.Length, "Position out of range.");
            Debug.Assert(charDescriptor != null, "Null character descriptor.");

            // Get the character info from the char descriptor table.
            CharDescriptor charDex = this.stringDescriptor[position];

            // If input is space or prompt and is to be escaped, we are actually resetting the position if assigned,
            // this doesn't affect literal positions.
            if (TestEscapeChar(input, position, charDescriptor))
            {
                ResetChar(position);
                return;
            }

            Debug.Assert( !IsLiteralPosition( charDex ), "Setting char in literal position." );

            if (Char.IsLetter(input))
            {
                if (Char.IsUpper(input))
                {
                    if (charDescriptor.CaseConversion == CaseConversion.ToLower)
                    {
                        input = this.culture.TextInfo.ToLower(input);
                    }
                }
                else // Char.IsLower( input )
                {
                    if (charDescriptor.CaseConversion == CaseConversion.ToUpper)
                    {
                        input = this.culture.TextInfo.ToUpper(input);
                    }
                }
            }

            this.testString[position] = input;

            if (!charDescriptor.IsAssigned) // if position not counted for already (replace case) we do it (add case).
            {
                charDescriptor.IsAssigned = true;
                this.assignedCharCount++;

                if (charDescriptor.CharType == CharType.EditRequired)
                {
                    this.requiredCharCount++;
                }
            }

            Debug.Assert(this.assignedCharCount <= this.EditPositionCount, "Invalid count of assigned chars.");
        }

        /// <devdoc>
        ///     Sets the characters in the test string starting from the specified position, to the ones in the input 
        ///     string. It assumes there's enough edit positions to hold the characters in the input string (so call
        ///     TestString before calling SetString).
        ///     The position is relative to the test string.
        /// </devdoc>
        private void SetString(string input, int testPosition)
        {
            foreach (char ch in input)
            {
                // If character is not to be escaped, we need to find the first edit position to test it in.
                if (!TestEscapeChar( ch, testPosition ))
                {
                    testPosition = FindEditPositionFrom(testPosition, forward);
                }

                SetChar(ch, testPosition);
                testPosition++;
            }
        }

#if OUT 
        // HERE FOR REF - 
        // VSW#482024 (Consider adding a method to synchroniza input processing options with output formatting options to guarantee text round-tripping.

        /// <devdoc>
        ///     Upadate the input processing options according to the output formatting options to guarantee
        ///     text round-tripping, this is: the Text property does not change when setting it to the value 
        ///     obtain from it; for a control: when copying the text to the clipboard and then pasting it back 
        ///     while selecting the entire text.
        /// </devdoc>
        private void SynchronizeInputOptions(MaskedTextProvider mtp, bool includePrompt, bool includeLiterals)
        {
            // Input options are processed in the following order:
            // 1. Literals
            // 2. Prompts
            // 3. Spaces.

            // If literals not included in the output, it should not attempt to skip literals.
            mtp.SkipLiterals = includeLiterals;

            // MaskedTextProvider processes space as follows:
            // If it is an input character, it would be processed as such (no scaping).
            // If it is a literal, it would be processed first since literals are processed first.
            // If it is the same as the prompt, the value of IncludePrompt does not matter because the output
            // will be the same; this case should be treated as if IncludePrompt was true.  Observe that 
            // AllowPromptAsInput would not be affected because ResetOnPrompt has higher precedence.
            if (mtp.PromptChar == ' ')
            {
                includePrompt = true;
            }

            // If prompts are not present in the output, spaces will replace the prompts and will be process
            // by ResetOnSpace.  Literals characters same as the prompt will be processed as literals first.
            // If prompts present positions will be rest.
            // Exception: PromptChar == space.
            mtp.ResetOnPrompt = includePrompt;

            // If no prompts in the output, the input may contain spaces replacing the prompt, reset on space 
            // should be enabled.  If prompts included in the output, spaces will be processed as literals.
            // Exception: PromptChar == space.
            mtp.ResetOnSpace = !includePrompt;
        }
#endif

        /// <devdoc>
        ///     Tests whether the character at the specified position in the test string can be set to the specified
        ///     value.
        ///     The position specified is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        private bool TestChar(char input, int position, out MaskedTextResultHint resultHint)
        {
            // boundary checks are performed in the public methods.
            Debug.Assert(position >= 0 && position < this.testString.Length, "Position out of range.");

            if (!IsPrintableChar(input))
            {
                resultHint = MaskedTextResultHint.InvalidInput;
                return false;
            }

            // Get the character info from the char descriptor table.
            CharDescriptor charDex = this.stringDescriptor[position];

            // Test if character should be escaped.
            // Test literals first - See VSW#454461.  See commented-out method SynchronizeInputOptions()

            if (IsLiteralPosition(charDex))
            {
                if (this.SkipLiterals && (input == this.testString[position]))
                {
                    resultHint = MaskedTextResultHint.CharacterEscaped;
                    return true;
                }

                resultHint = MaskedTextResultHint.NonEditPosition;
                return false;
            }

            if (input == this.promptChar )
            {
                if( this.ResetOnPrompt )
                {
                    if (IsEditPosition(charDex) && charDex.IsAssigned) // Position would be reset.
                    {
                        resultHint = MaskedTextResultHint.SideEffect;
                    }
                    else
                    {
                        resultHint = MaskedTextResultHint.CharacterEscaped;
                    }
                    return true; // test does not fail for prompt when it is to be scaped.
                }

                // Escaping precedes AllowPromptAsInput. Now test for it.
                if( !this.AllowPromptAsInput )
                {
                    resultHint = MaskedTextResultHint.PromptCharNotAllowed;
                    return false;
                }
            }

            if( input == spaceChar && this.ResetOnSpace )
            {
                if( IsEditPosition(charDex) && charDex.IsAssigned ) // Position would be reset.
                {
                    resultHint = MaskedTextResultHint.SideEffect;
                }
                else
                {
                    resultHint = MaskedTextResultHint.CharacterEscaped;
                }
                return true;
            }


            // Character was not escaped, now test it against the mask.

            // Test the character against the mask constraints.  The switch tests false conditions.
            // Space char succeeds the test if the char type is optional.
            switch (this.mask[charDex.MaskPosition])
            {
                case '#':   // digit or plus/minus sign optional.
                    if (!Char.IsDigit(input) && (input != '-') && (input != '+') && input != spaceChar)
                    {
                        resultHint = MaskedTextResultHint.DigitExpected;
                        return false;
                    }
                    break;

                case '0':   // digit required.
                    if (!Char.IsDigit(input))
                    {
                        resultHint = MaskedTextResultHint.DigitExpected;
                        return false;
                    }
                    break;

                case '9':   // digit optional.
                    if (!Char.IsDigit(input) && input != spaceChar)
                    {
                        resultHint = MaskedTextResultHint.DigitExpected;
                        return false;
                    }
                    break;

                case 'L':   // letter required.
                    if (!Char.IsLetter(input))
                    {
                        resultHint = MaskedTextResultHint.LetterExpected;
                        return false;
                    }
                    if (!IsAsciiLetter(input) && this.AsciiOnly)
                    {
                        resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                        return false;
                    }
                    break;

                case '?':   // letter optional.
                    if (!Char.IsLetter(input) && input != spaceChar)
                    {
                        resultHint = MaskedTextResultHint.LetterExpected;
                        return false;
                    }
                    if (!IsAsciiLetter(input) && this.AsciiOnly)
                    {
                        resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                        return false;
                    }
                    break;

                case '&':   // any character required.
                    if (!IsAscii(input) && this.AsciiOnly)
                    {
                        resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                        return false;
                    }
                    break;

                case 'C':   // any character optional.
                    if ((!IsAscii(input) && this.AsciiOnly) && input != spaceChar)
                    {
                        resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                        return false;
                    }
                    break;

                case 'A':   // Alphanumeric required.
                    if (!IsAlphanumeric(input))
                    {
                        resultHint = MaskedTextResultHint.AlphanumericCharacterExpected;
                        return false;
                    }
                    if (!IsAciiAlphanumeric(input) && this.AsciiOnly)
                    {
                        resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                        return false;
                    }
                    break;

                case 'a':   // Alphanumeric optional.
                    if (!IsAlphanumeric(input) && input != spaceChar)
                    {
                        resultHint = MaskedTextResultHint.AlphanumericCharacterExpected;
                        return false;
                    }
                    if (!IsAciiAlphanumeric(input) && this.AsciiOnly)
                    {
                        resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                        return false;
                    }
                    break;

                default:
                    Debug.Fail("Invalid mask language character.");
                    break;
            }

            // Test passed.

            if (input == this.testString[position] && charDex.IsAssigned)  // setting char would not make any difference
            {
                resultHint = MaskedTextResultHint.NoEffect;
            }
            else
            {
                resultHint = MaskedTextResultHint.Success;
            }

            return true;
        }

        /// <devdoc>
        ///     Tests if the character at the specified position in the test string is to be escaped.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        private bool TestEscapeChar( char input, int position )
        {
            CharDescriptor chDex = this.stringDescriptor[position];
            return TestEscapeChar(input, position, chDex);
    
        }
        private bool TestEscapeChar( char input, int position, CharDescriptor charDex )
        {
            // Test literals first.  See VSW#454461.
            // If the position holds a literal, it is escaped only if the input is the same as the literal independently on
            // the input value (space, prompt,...).
            if (IsLiteralPosition(charDex)) 
            {
                return this.SkipLiterals && input == this.testString[position];
            }
            
            if ((this.ResetOnPrompt && (input == this.promptChar)) || (this.ResetOnSpace && (input == spaceChar)))
            {
                return true;
            }

            return false;
        }

        /// <devdoc>
        ///     Tests if the character at the specified position in the test string can be set to the value specified,
        ///     and sets the character to that value if the test is successful.
        ///     The position specified is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        private bool TestSetChar(char input, int position, out MaskedTextResultHint resultHint)
        {
            if (TestChar(input, position, out resultHint))
            {
                if( resultHint == MaskedTextResultHint.Success || resultHint == MaskedTextResultHint.SideEffect ) // the character is not to be escaped.
                {
                    SetChar(input, position);
                }

                return true;
            }

            return false;
        }

        /// <devdoc>
        ///     Test the characters in the specified string agaist the test string, starting from the specified position.
        ///     If the test is successful, the characters in the test string are set appropriately.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        private bool TestSetString(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (TestString(input, position, out testPosition, out resultHint))
            {
                SetString(input, position);
                return true;
            }

            return false;
        }

        /// <devdoc>
        ///     Test the characters in the specified string agaist the test string, starting from the specified position.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The successCount out param contains the number of characters that would be actually set (not escaped).
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        private bool TestString(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            Debug.Assert(input != null, "null input.");
            Debug.Assert( position >= 0, "Position out of range." );

            resultHint = MaskedTextResultHint.Unknown;
            testPosition = position;

            if( input.Length == 0 )
            {
                return true;
            }

            // If any char is actually accepted, then the hint is success, otherwise whatever the last character result is.
            // Need a temp variable for this.
            MaskedTextResultHint tempHint = resultHint;
            
            foreach( char ch in input )
            {
                if( testPosition >= this.testString.Length )
                {
                    resultHint = MaskedTextResultHint.UnavailableEditPosition;
                    return false;
                }
                
                // If character is not to be escaped, we need to find an edit position to test it in.
                if( !TestEscapeChar(ch, testPosition) )
                {
                    testPosition = FindEditPositionFrom( testPosition, forward );
                    
                    if( testPosition == invalidIndex )
                    {
                        testPosition = this.testString.Length;
                        resultHint   = MaskedTextResultHint.UnavailableEditPosition;
                        return false;
                    }
                }

                // Test/Set char will scape prompt, space and literals if needed.
                if( !TestChar( ch, testPosition, out tempHint ) )
                {
                    resultHint = tempHint;
                    return false;
                }

                // Result precedence: Success, SideEffect, NoEffect, CharacterEscaped.
                if( tempHint > resultHint )
                {
                    resultHint = tempHint;
                }

                testPosition++;
            }

            testPosition--;

            return true;
        }

        /// <devdoc>
        ///     Returns a formatted string based on the mask, honoring only the PasswordChar property.  prompt character 
        ///     and literals are always included.  This is the text to be shown in a control when it has the focus.
        /// </devdoc>
        public string ToDisplayString()
        {
            if (!this.IsPassword || this.assignedCharCount == 0) // just return the testString since it contains the formatted text.
            {
                return this.testString.ToString();
            }
            
            // Copy test string and replace edit chars with password.
            StringBuilder st = new StringBuilder(this.testString.Length);

            for (int position = 0; position < this.testString.Length; position++)
            {
                CharDescriptor chDex = this.stringDescriptor[position];
                st.Append(IsEditPosition(chDex) && chDex.IsAssigned ? this.passwordChar : this.testString[position]);
            }

            return st.ToString();
        }

        /// <devdoc>
        ///     Returns a formatted string based on the mask, honoring  IncludePrompt and IncludeLiterals but ignoring
        ///     PasswordChar.
        /// </devdoc>
        public override string ToString()
        {
            return ToString(/*ignorePwdChar*/ true, this.IncludePrompt, this.IncludeLiterals, 0, this.testString.Length);
        }
        
        /// <devdoc>
        ///     Returns a formatted string based on the mask, honoring the IncludePrompt and IncludeLiterals properties,
        ///     and PasswordChar depending on the value of the 'ignorePasswordChar' parameter.
        /// </devdoc>
        public string ToString(bool ignorePasswordChar)
        {
            return ToString(ignorePasswordChar, this.IncludePrompt, this.IncludeLiterals, 0, this.testString.Length);
        }

        /// <devdoc>
        ///     Returns a formatted string starting at the specified position and for the specified number of character,
        ///     based on the mask, honoring IncludePrompt and IncludeLiterals but ignoring PasswordChar.
        ///     Parameters are relative to the test string.
        /// </devdoc>
        public string ToString(int startPosition, int length)
        {
            return ToString(/*ignorePwdChar*/ true, this.IncludePrompt, this.IncludeLiterals, startPosition, length);
        }

        /// <devdoc>
        ///     Returns a formatted string starting at the specified position and for the specified number of character,
        ///     based on the mask, honoring the IncludePrompt, IncludeLiterals properties and PasswordChar depending on
        ///     the 'ignorePasswordChar' parameter.
        ///     Parameters are relative to the test string.
        /// </devdoc>
        public string ToString(bool ignorePasswordChar, int startPosition, int length)
        {
            return ToString( ignorePasswordChar, this.IncludePrompt, this.IncludeLiterals, startPosition, length );
        }

        /// <devdoc>
        ///     Returns a formatted string based on the mask, ignoring the PasswordChar and according to the includePrompt 
        ///     and includeLiterals parameters.
        /// </devdoc>
        public string ToString(bool includePrompt, bool includeLiterals)
        {
            return ToString( /*ignorePwdChar*/ true, includePrompt, includeLiterals, 0, this.testString.Length );
        }

        /// <devdoc>
        ///     Returns a formatted string starting at the specified position and for the specified number of character,
        ///     based on the mask, according to the ignorePasswordChar, includePrompt and includeLiterals parameters.
        ///     Parameters are relative to the test string.
        /// </devdoc>
        public string ToString(bool includePrompt, bool includeLiterals, int startPosition, int length)
        {
            return ToString( /*ignorePwdChar*/ true, includePrompt, includeLiterals, startPosition, length);
        }

        /// <devdoc>
        ///     Returns a formatted string starting at the specified position and for the specified number of character,
        ///     based on the mask, according to the ignorePasswordChar, includePrompt and includeLiterals parameters.
        ///     Parameters are relative to the test string.
        /// </devdoc>
        public string ToString(bool ignorePasswordChar, bool includePrompt, bool includeLiterals, int startPosition, int length)
        {
            if (length <= 0)
            {
                return string.Empty;
            }

            if (startPosition < 0 )
            {
                startPosition = 0;
                //throw new ArgumentOutOfRangeException("startPosition");
            }

            if (startPosition >= this.testString.Length)
            {
                return string.Empty;
                //throw new ArgumentOutOfRangeException("startPosition");
            }

            int maxLength = this.testString.Length - startPosition;

            if (length > maxLength)
            {
                length = maxLength;
                //throw new ArgumentOutOfRangeException("length");
            }

            if (!this.IsPassword || ignorePasswordChar) // we may not need to format the text...
            {
                if (includePrompt && includeLiterals)
                {
                    return this.testString.ToString(startPosition, length); // testString contains just what the user is asking for.
                }
            }

            // Build the formatted string ...
            
            StringBuilder st = new StringBuilder();
            int lastPosition = startPosition + length - 1;

            if( !includePrompt )
            {
                // If prompt is not to be included we need to replace it with a space, but only for unassigned postions below
                // the last assigned position or last literal position if including literals, whichever is higher; upper unassigned
                // positions are not included in the resulting string.

                int lastLiteralPos = includeLiterals ? FindNonEditPositionInRange( startPosition, lastPosition, backward ) : InvalidIndex;
                int lastAssignedPos = FindAssignedEditPositionInRange( lastLiteralPos == InvalidIndex ? startPosition : lastLiteralPos, lastPosition, backward );
               
                // If lastLiteralPos is in the range and lastAssignedPos is not InvalidIndex, the lastAssignedPos is the upper limit
                // we are looking for since it is searched in the range from lastLiteralPos and lastPosition.  In any other case
                // lastLiteral would contain the upper position we are looking for or InvalidIndex, meaning all characters in the
                // range are to be ignored, in this case a null string should be returned.

                lastPosition = lastAssignedPos != InvalidIndex ? lastAssignedPos : lastLiteralPos;

                if( lastPosition == InvalidIndex )
                {
                    return string.Empty;
                }
            }

            for (int index = startPosition; index <= lastPosition; index++)
            {
                char ch = this.testString[index];
                CharDescriptor chDex = this.stringDescriptor[index];

                switch (chDex.CharType)
                {
                    case CharType.EditOptional:
                    case CharType.EditRequired:
                        if (chDex.IsAssigned)
                        {
                            if (this.IsPassword && !ignorePasswordChar)
                            {
                                st.Append(this.passwordChar); // replace edit char with pwd char.
                                break;
                            }
                        }
                        else
                        {
                            if (!includePrompt)
                            {
                                st.Append(spaceChar); // replace prompt with space.
                                break;
                            }
                        }

                        goto default;

                    case CharType.Separator:
                    case CharType.Literal:
                        if (!includeLiterals)
                        {
                            break;  // exclude literals.
                        }
                        goto default;

                    default:
                        st.Append(ch);
                        break;
                }
            }

            return st.ToString();
        }

        /// <devdoc>
        ///     Tests whether the specified character would be set successfully at the specified position.
        /// </devdoc>
        public bool VerifyChar(char input, int position, out MaskedTextResultHint hint)
        {
            hint = MaskedTextResultHint.NoEffect;

            if (position < 0 || position >= this.testString.Length)
            {
                hint = MaskedTextResultHint.PositionOutOfRange;
                return false;
            }

            return TestChar(input, position, out hint);
        }

        /// <devdoc>
        ///     Tests whether the specified character would be escaped at the specified position.
        /// </devdoc>
        public bool VerifyEscapeChar(char input, int position)
        {
            if (position < 0 || position >= this.testString.Length)
            {
                return false;
            }

            return TestEscapeChar(input, position);
        }

        /// <devdoc>
        ///     Verifies the test string against the mask.
        /// </devdoc>
        public bool VerifyString(string input)
        {
            int dummyVar;
            MaskedTextResultHint dummyVar2;
            return VerifyString(input, out dummyVar, out dummyVar2);
        }

        /// <devdoc>
        ///     Verifies the test string against the mask.
        ///     On exit the testPosition contains last position where the primary operation was actually performed if successful, 
        ///     otherwise the first position that made the test fail. This position is relative to the test string.
        ///     The MaskedTextResultHint out param gives more information about the operation result.
        ///     Returns true on success, false otherwise.
        /// </devdoc>
        public bool VerifyString(string input, out int testPosition, out MaskedTextResultHint resultHint)
        {
            testPosition = 0;

            if( input == null || input.Length == 0 ) // nothing to verify.
            {
                resultHint = MaskedTextResultHint.NoEffect;
                return true;  
            }

            return TestString(input, 0, out testPosition, out resultHint);
        }
    }
}
