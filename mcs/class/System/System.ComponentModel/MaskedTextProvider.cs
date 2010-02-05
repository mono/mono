// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Rolf Bjarne Kvinge  <RKvinge@novell.com>
//
//

/*

Mask language:

0	Digit, required. This element will accept any single digit between 0 and 9.
9	Digit or space, optional. 
#	Digit or space, optional. If this position is blank in the mask, it will be rendered as a space in the Text property. Plus (+) and minus (-) signs are allowed.
L	Letter, required. Restricts input to the ASCII letters a-z and A-Z. This mask element is equivalent to [a-zA-Z] in regular expressions. 
?	Letter, optional. Restricts input to the ASCII letters a-z and A-Z. This mask element is equivalent to [a-zA-Z]? in regular expressions. 
&	Character, required. If the AsciiOnly property is set to true, this element behaves like the "L" element. 
C	Character, optional. Any non-control character. If the AsciiOnly property is set to true, this element behaves like the "?" element.
 * LAMESPEC: A is REQUIRED, not optional.
A	Alphanumeric, optional. If the AsciiOnly property is set to true, the only characters it will accept are the ASCII letters a-z and A-Z.
a	Alphanumeric, optional. If the AsciiOnly property is set to true, the only characters it will accept are the ASCII letters a-z and A-Z.
.	Decimal placeholder. The actual display character used will be the decimal symbol appropriate to the format provider, as determined by the control's FormatProvider property.
,	Thousands placeholder. The actual display character used will be the thousands placeholder appropriate to the format provider, as determined by the control's FormatProvider property.
:	Time separator. The actual display character used will be the time symbol appropriate to the format provider, as determined by the control's FormatProvider property.
/	Date separator. The actual display character used will be the date symbol appropriate to the format provider, as determined by the control's FormatProvider property.
$	Currency symbol. The actual character displayed will be the currency symbol appropriate to the format provider, as determined by the control's FormatProvider property.
<	Shift down. Converts all characters that follow to lowercase. 
> 	Shift up. Converts all characters that follow to uppercase.
|	Disable a previous shift up or shift down.
\	Escape. Escapes a mask character, turning it into a literal. "\\" is the escape sequence for a backslash.

 * */
 
#if NET_2_0
using System.Globalization;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace System.ComponentModel {
	public class MaskedTextProvider : ICloneable
	{
#region Private fields
		private bool allow_prompt_as_input;
		private bool ascii_only;
		private CultureInfo culture; 
		private bool include_literals;
		private bool include_prompt;
		private bool is_password;
		private string mask;
		private char password_char;
		private char prompt_char;
		private bool reset_on_prompt;
		private bool reset_on_space;
		private bool skip_literals;
		
		private EditPosition [] edit_positions;
		
		private static char default_prompt_char = '_';
		private static char default_password_char = char.MinValue;
		
#endregion

#region Private classes
		private enum EditState {
			None,
			UpperCase,
			LowerCase
		}
		private enum EditType {
			DigitRequired, // 0
			DigitOrSpaceOptional, // 9
			DigitOrSpaceOptional_Blank, // #
			LetterRequired, // L
			LetterOptional, // ?
			CharacterRequired, // &
			CharacterOptional, // C
			AlphanumericRequired, // A
			AlphanumericOptional, // a
			DecimalPlaceholder, // .
			ThousandsPlaceholder, // ,
			TimeSeparator, // :
			DateSeparator, // /
			CurrencySymbol, // $ 
			Literal
		}

		private class EditPosition {
			public MaskedTextProvider Parent;
			public EditType Type;
			public EditState State;
			public char MaskCharacter;
			public char input;
			
			public void Reset ()
			{
				input = char.MinValue;
			}
			
			internal EditPosition Clone () {
				EditPosition result = new EditPosition ();
				result.Parent = Parent;
				result.Type = Type;
				result.State = State;
				result.MaskCharacter = MaskCharacter;
				result.input = input;
				return result;
			}
			
			public char Input {
				get {
					return input;
				}
				set {
					switch (State) {
					case EditState.LowerCase:
						input = char.ToLower (value, Parent.Culture);
						break;
					case EditState.UpperCase:
						input = char.ToUpper (value, Parent.Culture);
						break;
					default:// case EditState.None:
						input = value;
						break;
					}
				}
			}
			
			public bool IsAscii (char c)
			{
				return ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
			}
			
			public bool Match (char c, out MaskedTextResultHint resultHint, bool only_test)
			{
				if (!MaskedTextProvider.IsValidInputChar (c)) {
					resultHint = MaskedTextResultHint.InvalidInput;
					return false;
				}

				if (Parent.ResetOnSpace && c == ' ' && Editable) {
					resultHint = MaskedTextResultHint.CharacterEscaped;
					if (FilledIn) {
						resultHint = MaskedTextResultHint.Success;
						if (!only_test && input != ' ') {
							switch (Type) {
							case EditType.AlphanumericOptional:
							case EditType.AlphanumericRequired:
							case EditType.CharacterOptional:
							case EditType.CharacterRequired:
								Input = c;
								break;
							default:
								Input = char.MinValue;
								break;
							}
						}
					}
					return true;
				}
				
				if (Type == EditType.Literal && MaskCharacter == c && Parent.SkipLiterals) {
					resultHint = MaskedTextResultHint.Success;
					return true;
				}
				
				if (!Editable) {
					resultHint = MaskedTextResultHint.NonEditPosition;
					return false;
				}

				switch (Type) {
					case EditType.AlphanumericOptional:
					case EditType.AlphanumericRequired:
						if (char.IsLetterOrDigit (c)) {
							if (Parent.AsciiOnly && !IsAscii (c)) {
								resultHint = MaskedTextResultHint.AsciiCharacterExpected;
								return false;
							} else {
								if (!only_test) {
									Input = c;
								}
								resultHint = MaskedTextResultHint.Success;
								return true;
							}
						} else {
							resultHint = MaskedTextResultHint.AlphanumericCharacterExpected;
							return false;
						}
					case EditType.CharacterOptional:
					case EditType.CharacterRequired:
						if (Parent.AsciiOnly && !IsAscii (c)) {
							resultHint = MaskedTextResultHint.LetterExpected;
							return false;
						} else if (!char.IsControl (c)) {
							if (!only_test) {
								Input = c;
							}
							resultHint = MaskedTextResultHint.Success;
							return true;
						} else {
							resultHint = MaskedTextResultHint.LetterExpected;
							return false;
						}
					case EditType.DigitOrSpaceOptional:
					case EditType.DigitOrSpaceOptional_Blank:
						if (char.IsDigit (c) || c == ' ') {
							if (!only_test) {
								Input = c;
							}
							resultHint = MaskedTextResultHint.Success;
							return true;
						} else {
							resultHint = MaskedTextResultHint.DigitExpected;
							return false;
						}
					case EditType.DigitRequired:
						if (char.IsDigit (c)) {
							if (!only_test) {
								Input = c;
							}
							resultHint = MaskedTextResultHint.Success;
							return true;
						} else {
							resultHint = MaskedTextResultHint.DigitExpected;
							return false;
						}
					case EditType.LetterOptional:
					case EditType.LetterRequired:
						if (!char.IsLetter (c)) {
							resultHint = MaskedTextResultHint.LetterExpected;
							return false;
						} else if (Parent.AsciiOnly && !IsAscii (c)) {
							resultHint = MaskedTextResultHint.LetterExpected;
							return false;
						} else {
							if (!only_test) {
								Input = c;
							}
							resultHint = MaskedTextResultHint.Success;
							return true;
						}
					default:
						resultHint = MaskedTextResultHint.Unknown;
						return false;
				}
			}
			
			public bool FilledIn {
				get {
					return Input != char.MinValue;
				}
			}
			
			public bool Required {
				get {
					switch (MaskCharacter) {
					case '0':
					case 'L':
					case '&':
					case 'A':
						return true;
					default:
						return false;
					}
				}
			}
			
			public bool Editable {
				get {
					switch (MaskCharacter) {
					case '0':
					case '9':
					case '#':
					case 'L':
					case '?':
					case '&':
					case 'C':
					case 'A':
					case 'a':
						return true;
					default:
						return false;
					}
				}
			}	
			
			public bool Visible {
				get {
					switch (MaskCharacter) {
					case '|':
					case '<':
					case '>':
						return false;
					default:
						return true;
					}
				}
			}
			public string Text {
				get {
					if (Type == EditType.Literal) {
						return MaskCharacter.ToString ();
					}
					switch (MaskCharacter) {
					case '.': return Parent.Culture.NumberFormat.NumberDecimalSeparator;
					case ',': return Parent.Culture.NumberFormat.NumberGroupSeparator;
					case ':': return Parent.Culture.DateTimeFormat.TimeSeparator;
					case '/': return Parent.Culture.DateTimeFormat.DateSeparator;
					case '$': return Parent.Culture.NumberFormat.CurrencySymbol;
					default:
						return FilledIn ? Input.ToString () : Parent.PromptChar.ToString ();
					}
				}
			}

			private EditPosition ()
			{
			}

			public EditPosition (MaskedTextProvider Parent, EditType Type, EditState State, char MaskCharacter)
			{
				this.Type = Type;
				this.Parent = Parent;
				this.State = State;
				this.MaskCharacter = MaskCharacter;
				
			}
		}
#endregion

#region Private methods & properties
		private void SetMask (string mask)
		{
			if (mask == null || mask == string.Empty)
				throw new ArgumentException ("The Mask value cannot be null or empty.\r\nParameter name: mask");

			this.mask = mask;
			
			System.Collections.Generic.List <EditPosition> list = new System.Collections.Generic.List<EditPosition> (mask.Length);
			
			EditState State = EditState.None;
			bool escaped = false;
			for (int i = 0; i < mask.Length; i++) {
				if (escaped) {
					list.Add (new EditPosition (this, EditType.Literal, State, mask [i]));
					escaped = false;
					continue;
				}
				
				switch (mask [i]) {
				case '\\':
					escaped = true; 
					break;
				case '0':
					list.Add (new EditPosition (this, EditType.DigitRequired, State, mask [i])); 
					break;
				case '9':
					list.Add (new EditPosition (this, EditType.DigitOrSpaceOptional, State, mask [i]));
					break;
				case '#':
					list.Add (new EditPosition (this, EditType.DigitOrSpaceOptional_Blank, State, mask [i]));
					break;
				case 'L':
					list.Add (new EditPosition (this, EditType.LetterRequired, State, mask [i]));
					break;
				case '?':
					list.Add (new EditPosition (this, EditType.LetterOptional, State, mask [i]));
					break;
				case '&':
					list.Add (new EditPosition (this, EditType.CharacterRequired, State, mask [i]));
					break;
				case 'C':
					list.Add (new EditPosition (this, EditType.CharacterOptional, State, mask [i]));
					break;
				case 'A':
					list.Add (new EditPosition (this, EditType.AlphanumericRequired, State, mask [i]));
					break;
				case 'a':
					list.Add (new EditPosition (this, EditType.AlphanumericOptional, State, mask [i]));
					break;
				case '.':
					list.Add (new EditPosition (this, EditType.DecimalPlaceholder, State, mask [i]));
					break;
				case ',':
					list.Add (new EditPosition (this, EditType.ThousandsPlaceholder, State, mask [i]));
					break;
				case ':':
					list.Add (new EditPosition (this, EditType.TimeSeparator, State, mask [i]));
					break;
				case '/':
					list.Add (new EditPosition (this, EditType.DateSeparator, State, mask [i]));
					break;
				case '$':
					list.Add (new EditPosition (this, EditType.CurrencySymbol, State, mask [i]));
					break;
				case '<':
					State = EditState.LowerCase;
					break;
				case '>':
					State = EditState.UpperCase;
					break;
				case '|':
					State = EditState.None;
					break;
				default:
					list.Add (new EditPosition (this, EditType.Literal, State, mask [i]));
					break;
				}
			}
			edit_positions = list.ToArray ();
		}
		
		private EditPosition [] ClonePositions ()
		{
			EditPosition [] result = new EditPosition [edit_positions.Length];
			for (int i = 0; i < result.Length; i++) {
				result [i] = edit_positions [i].Clone ();
			}
			return result;
		}

		private bool AddInternal (string str_input, out int testPosition, out MaskedTextResultHint resultHint, bool only_test)
		{
			EditPosition [] edit_positions;
			
			if (only_test) {
				edit_positions = ClonePositions ();
			} else {
				edit_positions = this.edit_positions;
			}

			if (str_input == null)
				throw new ArgumentNullException ("input");

			if (str_input.Length == 0) {
				resultHint = MaskedTextResultHint.NoEffect;
				testPosition = LastAssignedPosition + 1;
				return true;
			}

			resultHint = MaskedTextResultHint.Unknown;
			testPosition = 0;

			int next_position = LastAssignedPosition;
			MaskedTextResultHint tmpResult = MaskedTextResultHint.Unknown;

			if (next_position >= edit_positions.Length) {
				testPosition = next_position;
				resultHint = MaskedTextResultHint.UnavailableEditPosition;
				return false;
			}

			for (int i = 0; i < str_input.Length; i++) {
				char input = str_input [i];
				next_position++;
				testPosition = next_position;
				
				if (tmpResult > resultHint) {
					resultHint = tmpResult;
				}

				if (VerifyEscapeChar (input, next_position)) {
					tmpResult = MaskedTextResultHint.CharacterEscaped;
					continue;
				}

				next_position = FindEditPositionFrom (next_position, true);
				testPosition = next_position;

				if (next_position == InvalidIndex) {
					testPosition = edit_positions.Length;
					resultHint = MaskedTextResultHint.UnavailableEditPosition;
					return false;
				}

				if (!IsValidInputChar (input)) {
					testPosition = next_position;
					resultHint = MaskedTextResultHint.InvalidInput;
					return false;
				}
				
				if (!edit_positions [next_position].Match (input, out tmpResult, false)) {
					testPosition = next_position;
					resultHint = tmpResult;
					return false;
				}		
			}

			if (tmpResult > resultHint) {
				resultHint = tmpResult;
			}

			return true;
		}
		
		private bool AddInternal (char input, out int testPosition, out MaskedTextResultHint resultHint, bool check_available_positions_first, bool check_escape_char_first)
		{
			/*
			 * check_available_positions_first: when adding a char, MS first seems to check if there are any available edit positions, then if there are any
			 * they will try to add the char (meaning that if you add a char matching a literal char in the mask with SkipLiterals and there are no
			 * more available edit positions, it will fail).
			 * 
			 * check_escape_char_first: When adding a char, MS doesn't check for escape char, they directly search for the first edit position.
			 * When adding a string, they check first for escape char, then if not successful they find the first edit position.
			 */
			 
			int new_position;
			testPosition = 0;
			
			new_position = LastAssignedPosition + 1;

			if (check_available_positions_first) {
				int tmp = new_position;
				bool any_available = false;
				while (tmp < edit_positions.Length) {
					if (edit_positions [tmp].Editable) {
						any_available = true;
						break;
					}
					tmp++;
				}
				if (!any_available) {
					testPosition = tmp;
					resultHint = MaskedTextResultHint.UnavailableEditPosition;
					return GetOperationResultFromHint (resultHint);
				}
			}

			if (check_escape_char_first) {
				if (VerifyEscapeChar (input, new_position)) {
					testPosition = new_position;
					resultHint = MaskedTextResultHint.CharacterEscaped;
					return true;
				}
			}

			new_position = FindEditPositionFrom (new_position, true);

			if (new_position > edit_positions.Length - 1 ||new_position == InvalidIndex) {
				testPosition = new_position;
				resultHint = MaskedTextResultHint.UnavailableEditPosition;
				return GetOperationResultFromHint (resultHint);
			}

			if (!IsValidInputChar (input)) {
				testPosition = new_position;
				resultHint = MaskedTextResultHint.InvalidInput;
				return GetOperationResultFromHint (resultHint);
			}

			if (!edit_positions [new_position].Match (input, out resultHint, false)) {
				testPosition = new_position;
				return GetOperationResultFromHint (resultHint);
			}

			testPosition = new_position;

			return GetOperationResultFromHint (resultHint);
		}
		
		private bool VerifyStringInternal (string input, out int testPosition, out MaskedTextResultHint resultHint, int startIndex, bool only_test)
		{
			int previous_position = startIndex;
			int current_position;
			resultHint = MaskedTextResultHint.Unknown;
			
			// Replace existing characters
			for (int i = 0; i < input.Length; i++) {
				MaskedTextResultHint tmpResult;
				current_position = FindEditPositionFrom (previous_position, true);
				if (current_position == InvalidIndex) {
					testPosition = edit_positions.Length;
					resultHint = MaskedTextResultHint.UnavailableEditPosition;
					return false;
				}
				
				if (!VerifyCharInternal (input [i], current_position, out tmpResult, only_test)) {
					testPosition = current_position;
					resultHint = tmpResult;
					return false;
				}
				if (tmpResult > resultHint) {
					resultHint = tmpResult;
				}
				previous_position = current_position + 1;
				
			}
			// Remove characters not in the input.
			if (!only_test) {
				previous_position = FindEditPositionFrom (previous_position, true);
				while (previous_position != InvalidIndex) {
					if (edit_positions [previous_position].FilledIn) {
						edit_positions [previous_position].Reset ();
						if (resultHint != MaskedTextResultHint.NoEffect) {
							resultHint = MaskedTextResultHint.Success;
						}
					}
					previous_position = FindEditPositionFrom (previous_position + 1, true);
				}
			}
			if (input.Length > 0) {
				testPosition = startIndex + input.Length - 1;
			} else {
				testPosition = startIndex;
				if (resultHint < MaskedTextResultHint.NoEffect) {
					resultHint = MaskedTextResultHint.NoEffect;
				}
			}
			
			return true;
		}

		private bool VerifyCharInternal (char input, int position, out MaskedTextResultHint hint, bool only_test)
		{
			hint = MaskedTextResultHint.Unknown;

			if (position < 0 || position >= edit_positions.Length) {
				hint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}

			if (!IsValidInputChar (input)) {
				hint = MaskedTextResultHint.InvalidInput;
				return false;
			}

			if (input == ' ' && ResetOnSpace && edit_positions [position].Editable && edit_positions [position].FilledIn) {
				if (!only_test) {
					edit_positions [position].Reset ();
				}
				hint = MaskedTextResultHint.SideEffect;
				return true;
			}
			
			if (edit_positions [position].Editable &&  edit_positions [position].FilledIn && edit_positions [position].input == input) {
				hint = MaskedTextResultHint.NoEffect;
				return true;
			}

			if (SkipLiterals && !edit_positions [position].Editable && edit_positions [position].Text == input.ToString ()) {
				hint = MaskedTextResultHint.CharacterEscaped;
				return true;
			}

			return edit_positions [position].Match (input, out hint, only_test);
		}
		
		// Test to see if the input string can be inserted at the specified position.
		// Does not try to move characters.
		private bool IsInsertableString (string str_input, int position, out int testPosition, out MaskedTextResultHint resultHint)
		{
			int current_position = position;
			int test_position;
			
			resultHint = MaskedTextResultHint.UnavailableEditPosition;
			testPosition = InvalidIndex;
			
			for (int i = 0; i < str_input.Length; i++) {
				char ch = str_input [i];

				test_position = FindEditPositionFrom (current_position, true);
				
				if (test_position != InvalidIndex && VerifyEscapeChar (ch, test_position)) {
					current_position = test_position + 1;
					continue;
				}
				
				if (VerifyEscapeChar (ch, current_position)) {
					current_position = current_position + 1;
					continue;
				}
				
				
				if (test_position == InvalidIndex) {
					resultHint = MaskedTextResultHint.UnavailableEditPosition;
					testPosition = edit_positions.Length;
					return false;
				}

				testPosition = test_position;
				
				if (!edit_positions [test_position].Match (ch, out resultHint, true)) {
					return false;
				}
				
				current_position = test_position + 1;
			}
			resultHint = MaskedTextResultHint.Success;
			
			return true;
		}
		
		private bool ShiftPositionsRight (EditPosition [] edit_positions, int start, out int testPosition, out MaskedTextResultHint resultHint)
		{
			int index = start;
			int last_assigned_index = FindAssignedEditPositionFrom (edit_positions.Length, false);
			int endindex = FindUnassignedEditPositionFrom (last_assigned_index, true); // Find the first non-assigned edit position
			
			testPosition = start;
			resultHint = MaskedTextResultHint.Unknown;

			if (endindex == InvalidIndex) {
				// No more free edit positions.
				testPosition = edit_positions.Length;
				resultHint = MaskedTextResultHint.UnavailableEditPosition;
				return false;
			}
		
			while (endindex > index) {
				char char_to_assign;
				int index_to_move;
				
				index_to_move = FindEditPositionFrom (endindex - 1, false);	
				char_to_assign = edit_positions [index_to_move].input;

				if (char_to_assign == char.MinValue) {
					edit_positions [endindex].input = char_to_assign;
				} else {
					if (!edit_positions [endindex].Match (char_to_assign, out resultHint, false)) {
						testPosition = endindex;
						return false;
					}
				}
				endindex = index_to_move;
			}
			
			if (endindex != InvalidIndex) {
				edit_positions [endindex].Reset ();
			}
			
			return true;
		}
		
		private bool ReplaceInternal (string input, int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint, bool only_test, bool dont_remove_at_end)
		{
			EditPosition [] edit_positions;
			resultHint = MaskedTextResultHint.Unknown;
			
			if (only_test) {
				edit_positions = ClonePositions ();
			} else {
				edit_positions = this.edit_positions;
			}
			
			if (input == null)
				throw new ArgumentNullException ("input");

			if (endPosition >= edit_positions.Length) {
				testPosition = endPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			if (startPosition < 0) {
				testPosition = startPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			if (startPosition >= edit_positions.Length) {
				testPosition = startPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			if (startPosition > endPosition) {
				testPosition = startPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}

			
			if (input.Length == 0) {
				return RemoveAtInternal (startPosition, endPosition, out testPosition, out resultHint, only_test);
			}

			int previous_position = startPosition;
			int current_position = previous_position;
			MaskedTextResultHint tmpResult = MaskedTextResultHint.Unknown;
			testPosition = InvalidIndex;
			
			for (int i = 0; i < input.Length; i++) {
				char current_input = input [i];
				
				current_position = previous_position;

				if (VerifyEscapeChar (current_input, current_position)) {
					if (edit_positions [current_position].FilledIn &&
						edit_positions [current_position].Editable &&
						(current_input == ' ' && ResetOnSpace) || (current_input == PromptChar && ResetOnPrompt)) {
						edit_positions [current_position].Reset ();
						tmpResult = MaskedTextResultHint.SideEffect;
					} else {
						tmpResult = MaskedTextResultHint.CharacterEscaped;
					}
				} else if (current_position < edit_positions.Length && !edit_positions [current_position].Editable && FindAssignedEditPositionInRange (current_position, endPosition, true) == InvalidIndex) {
					// If replacing over a literal, the replacement character is INSERTED at the next
					// available edit position. Weird, huh?
					current_position = FindEditPositionFrom (current_position, true);
					if (current_position == InvalidIndex) {
						resultHint = MaskedTextResultHint.UnavailableEditPosition;
						testPosition = edit_positions.Length;
						return false;
					}
					if (!InsertAtInternal (current_input.ToString (), current_position, out testPosition, out tmpResult, only_test)) {
						resultHint = tmpResult;
						return false;
					}
				} else { 
				
					current_position = FindEditPositionFrom (current_position, true);

					if (current_position == InvalidIndex) {
						testPosition = edit_positions.Length;
						resultHint = MaskedTextResultHint.UnavailableEditPosition;
						return false;
					}

					if (!IsValidInputChar (current_input)) {
						testPosition = current_position;
						resultHint = MaskedTextResultHint.InvalidInput;
						return false;
					}

					if (!ReplaceInternal (edit_positions, current_input, current_position, out testPosition, out tmpResult, false)) {
						resultHint = tmpResult;
						return false;
					}
				}
				
				if (tmpResult > resultHint) {
					resultHint = tmpResult;
				}
				
				previous_position = current_position + 1;
			}

			testPosition = current_position;
			
			int tmpPosition;
			if (!dont_remove_at_end && previous_position <= endPosition) {
				if (!RemoveAtInternal (previous_position, endPosition, out tmpPosition, out tmpResult, only_test)) {
					testPosition = tmpPosition;
					resultHint = tmpResult;
					return false;
				}
			}
			if (tmpResult == MaskedTextResultHint.Success && resultHint < MaskedTextResultHint.SideEffect) {
				resultHint = MaskedTextResultHint.SideEffect;
			}
				
			return true;
		}
		
		private bool ReplaceInternal (EditPosition [] edit_positions, char input, int position, out int testPosition, out MaskedTextResultHint resultHint, bool only_test)
		{
			testPosition = position;

			if (!IsValidInputChar (input)) {
				resultHint = MaskedTextResultHint.InvalidInput;
				return false;
			}

			if (VerifyEscapeChar (input, position)) {
				if (edit_positions [position].FilledIn && edit_positions [position].Editable && (input == ' ' && ResetOnSpace) || (input == PromptChar && ResetOnPrompt)) {
					edit_positions [position].Reset ();
					resultHint = MaskedTextResultHint.SideEffect;
				} else {
					resultHint = MaskedTextResultHint.CharacterEscaped;
				}
				testPosition = position;
				return true;
			}

			if (!edit_positions [position].Editable) {
				resultHint = MaskedTextResultHint.NonEditPosition;
				return false;
			}
			
			bool is_filled = edit_positions [position].FilledIn;
			if (is_filled && edit_positions [position].input == input) {
				if (VerifyEscapeChar (input, position)) {
					resultHint = MaskedTextResultHint.CharacterEscaped;
				} else {
					resultHint = MaskedTextResultHint.NoEffect;
				}
			} else if (input == ' ' && this.ResetOnSpace) {
				if (is_filled) {
					resultHint = MaskedTextResultHint.SideEffect;
					edit_positions [position].Reset ();
				} else {
					resultHint = MaskedTextResultHint.CharacterEscaped;
				}
				return true;
			} else if (VerifyEscapeChar (input, position)) {
				resultHint = MaskedTextResultHint.SideEffect;
			} else {
				resultHint = MaskedTextResultHint.Success;
			}
			MaskedTextResultHint tmpResult;
			if (!edit_positions [position].Match (input, out tmpResult, false)) {
				resultHint = tmpResult;
				return false;
			}

			return true;
		}

		private bool RemoveAtInternal (int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint, bool only_testing)
		{
			EditPosition [] edit_positions;
			testPosition = -1;
			resultHint = MaskedTextResultHint.Unknown;

			if (only_testing) {
				edit_positions = ClonePositions ();
			} else {
				edit_positions = this.edit_positions;
			}

			if (endPosition < 0 || endPosition >= edit_positions.Length) {
				testPosition = endPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			if (startPosition < 0 || startPosition >= edit_positions.Length) {
				testPosition = startPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			
			if (startPosition > endPosition) {
				testPosition = startPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			int edit_positions_in_range = 0;
			for (int i = startPosition; i <= endPosition; i++) {
				if (edit_positions [i].Editable) {
					edit_positions_in_range++;
				}
			}
			
			if (edit_positions_in_range == 0) {
				testPosition = startPosition;
				resultHint = MaskedTextResultHint.NoEffect;
				return true;
			}
			
			int current_edit_position = FindEditPositionFrom (startPosition, true);
			while (current_edit_position != InvalidIndex) {
				// Find the edit position that will reach the current position.
				int next_index = FindEditPositionFrom (current_edit_position + 1, true);
				for (int j = 1; j < edit_positions_in_range && next_index != InvalidIndex; j++) {
					next_index = FindEditPositionFrom (next_index + 1, true);
				}
				
				if (next_index == InvalidIndex) {
					if (edit_positions [current_edit_position].FilledIn) {
						edit_positions [current_edit_position].Reset ();
						resultHint = MaskedTextResultHint.Success;	
					} else {
						if (resultHint < MaskedTextResultHint.NoEffect) {
							resultHint = MaskedTextResultHint.NoEffect;
						}
					}
				} else {
					if (!edit_positions [next_index].FilledIn) {
						if (edit_positions [current_edit_position].FilledIn) {
							edit_positions [current_edit_position].Reset ();
							resultHint = MaskedTextResultHint.Success;	
						} else {
							if (resultHint < MaskedTextResultHint.NoEffect) {
								resultHint = MaskedTextResultHint.NoEffect;
							}
						}
					} else {
						MaskedTextResultHint tmpResult = MaskedTextResultHint.Unknown;
						if (edit_positions [current_edit_position].FilledIn) {
							resultHint = MaskedTextResultHint.Success;
						} else if (resultHint < MaskedTextResultHint.SideEffect) {
							resultHint = MaskedTextResultHint.SideEffect;
						}
						if (!edit_positions [current_edit_position].Match (edit_positions [next_index].input, out tmpResult, false)) {
							resultHint = tmpResult;
							testPosition = current_edit_position;
							return false;
						}
					}	
					edit_positions [next_index].Reset ();
				}
				current_edit_position = FindEditPositionFrom (current_edit_position + 1, true);
			}
			
			if (resultHint == MaskedTextResultHint.Unknown) {
				resultHint = MaskedTextResultHint.NoEffect;
			}
			
			testPosition = startPosition;
			
			return true;
		}
		
		private bool InsertAtInternal (string str_input, int position, out int testPosition, out MaskedTextResultHint resultHint, bool only_testing)
		{
			EditPosition [] edit_positions;
			testPosition = -1;
			resultHint = MaskedTextResultHint.Unknown;

			if (only_testing) {
				edit_positions = ClonePositions ();
			} else {
				edit_positions = this.edit_positions;
			}

			if (position < 0 || position >= edit_positions.Length) {
				testPosition = 0;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			if (!IsInsertableString (str_input, position, out testPosition, out resultHint)) {
				return false;
			}
			
			resultHint = MaskedTextResultHint.Unknown;
			
			int next_position = position;
			for (int i = 0; i < str_input.Length; i++) {
				char input = str_input [i];
				int index = FindEditPositionFrom (next_position, true); // Find the first edit position (here the input will go)
				int endindex = FindUnassignedEditPositionFrom (next_position, true); // Find the first non-assigned edit position
				bool escaped = false;

				if (VerifyEscapeChar (input, next_position)) {
					escaped = true;

					if (input.ToString () == edit_positions [next_position].Text) {
						if (FindAssignedEditPositionInRange (0, next_position - 1, true) != InvalidIndex && endindex == InvalidIndex) {
							resultHint = MaskedTextResultHint.UnavailableEditPosition;
							testPosition = edit_positions.Length;
							return false;
						}
						resultHint = MaskedTextResultHint.CharacterEscaped;
						testPosition = next_position;
						next_position++;
						continue;						
					}
				}
				
				if (!escaped && index == InvalidIndex) {
					// No edit positions left at all in the string
					testPosition = edit_positions.Length;
					resultHint = MaskedTextResultHint.UnavailableEditPosition;
					return false;
				}
				
				if (index == InvalidIndex) {
					index = next_position;
				}
				
				bool was_filled = edit_positions [index].FilledIn;
				bool shift = was_filled;
				if (shift) {
					if (!ShiftPositionsRight (edit_positions, index, out testPosition, out resultHint)) {
						return false;
					}
				}

				testPosition = index;
				if (escaped) {
					if (was_filled) {
						resultHint = MaskedTextResultHint.Success;
					} else if (!edit_positions [index].Editable && input.ToString () == edit_positions [index].Text) {
						resultHint = MaskedTextResultHint.CharacterEscaped;
						testPosition = next_position;
					} else {
						int first_edit_position = FindEditPositionFrom (index, true);
						if (first_edit_position == InvalidIndex) {
							resultHint = MaskedTextResultHint.UnavailableEditPosition;
							testPosition = edit_positions.Length;
							return false;
						}
			
						resultHint = MaskedTextResultHint.CharacterEscaped;
						if (input.ToString () == edit_positions [next_position].Text) {
							testPosition = next_position;
						}
					}
				} else {
					MaskedTextResultHint tmpResult;
					if (!edit_positions [index].Match (input, out tmpResult, false)) {
						resultHint = tmpResult;
						return false;
					}
					if (resultHint < tmpResult) {
						resultHint = tmpResult;
					}
				}
				next_position = index + 1;
				
			}
			
			return true;
		}
#endregion
		#region Public constructors
		static MaskedTextProvider()
		{
		}

		public MaskedTextProvider(string mask) 
			: this (mask, null, true, default_prompt_char, default_password_char, false)
		{
		}

		public MaskedTextProvider (string mask, bool restrictToAscii) 
			: this (mask, null, true, default_prompt_char, default_password_char, restrictToAscii)
		{
		}

		public MaskedTextProvider (string mask, CultureInfo culture) 
			: this (mask, culture, true, default_prompt_char, default_password_char, false)
		{
		}

		public MaskedTextProvider (string mask, char passwordChar, bool allowPromptAsInput) 
			: this (mask, null, allowPromptAsInput, default_prompt_char, passwordChar, false)
		{
		}

		public MaskedTextProvider (string mask, CultureInfo culture, bool restrictToAscii) 
			: this (mask, culture, true, default_prompt_char, default_password_char, restrictToAscii)
		{
		}
		
		public MaskedTextProvider(string mask, CultureInfo culture, char passwordChar, bool allowPromptAsInput) 
			: this (mask, culture, allowPromptAsInput, default_prompt_char, passwordChar, false)
		{
		}
		
		public MaskedTextProvider(string mask, CultureInfo culture, bool allowPromptAsInput, char promptChar, char passwordChar, bool restrictToAscii)
		{
			SetMask (mask);
			
			if (culture == null)
				this.culture = Threading.Thread.CurrentThread.CurrentCulture;
			else
				this.culture = culture;
				
			this.allow_prompt_as_input = allowPromptAsInput;
			
			this.PromptChar = promptChar;
			this.PasswordChar = passwordChar;
			this.ascii_only = restrictToAscii;
			
			include_literals = true;
			reset_on_prompt = true;
			reset_on_space = true;
			skip_literals = true;
		}
#endregion

#region Public methods
		public bool Add(char input)
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return Add (input, out testPosition, out resultHint);
		}

		public bool Add (string input)
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return Add (input, out testPosition, out resultHint);
		}

		public bool Add (char input, out int testPosition, out MaskedTextResultHint resultHint)
		{
			return AddInternal (input, out testPosition, out resultHint, true, false);
		}

		public bool Add (string input, out int testPosition, out MaskedTextResultHint resultHint)
		{
			bool result;
			
			result = AddInternal (input, out testPosition, out resultHint, true);
			
			if (result) {
				result = AddInternal (input, out testPosition, out resultHint, false);
			}
			
			return result;
		}

		public void Clear ()
		{
			MaskedTextResultHint resultHint;
			Clear (out resultHint);
		}

		public void Clear (out MaskedTextResultHint resultHint)
		{
			resultHint = MaskedTextResultHint.NoEffect;
			for (int i = 0; i < edit_positions.Length; i++) {
				if (edit_positions [i].Editable && edit_positions [i].FilledIn) {
					edit_positions [i].Reset ();
					resultHint = MaskedTextResultHint.Success;
				}
			}
		}

		public object Clone ()
		{
			MaskedTextProvider result = new MaskedTextProvider (mask);
			
			result.allow_prompt_as_input = allow_prompt_as_input;
			result.ascii_only = ascii_only;
			result.culture = culture;
			result.edit_positions = ClonePositions ();
			result.include_literals = include_literals;
			result.include_prompt = include_prompt;
			result.is_password = is_password;
			result.mask = mask;
			result.password_char = password_char;
			result.prompt_char = prompt_char;
			result.reset_on_prompt = reset_on_prompt;
			result.reset_on_space = reset_on_space;
			result.skip_literals = skip_literals;
						
			return result;
		}

		public int FindAssignedEditPositionFrom (int position, bool direction)
		{
			if (direction) {
				return FindAssignedEditPositionInRange (position, edit_positions.Length - 1, direction);
			} else {
				return FindAssignedEditPositionInRange (0, position, direction);			
			}
		}

		public int FindAssignedEditPositionInRange (int startPosition, int endPosition, bool direction)
		{
			int step;
			int start, end;
			
			if (startPosition < 0)
				startPosition = 0;
			if (endPosition >= edit_positions.Length)
				endPosition = edit_positions.Length - 1;

			if (startPosition > endPosition)
				return InvalidIndex;
				
			step = direction ? 1 : -1;
			start = direction ? startPosition : endPosition;
			end = (direction ? endPosition: startPosition) + step;

			for (int i = start; i != end; i+=step) {
				if (edit_positions [i].Editable && edit_positions [i].FilledIn)
					return i;
			}
			return InvalidIndex;
		}

		public int FindEditPositionFrom (int position, bool direction)
		{
			if (direction) {
				return FindEditPositionInRange (position, edit_positions.Length - 1, direction);
			} else {
				return FindEditPositionInRange (0, position, direction);
			}
		}

		public int FindEditPositionInRange (int startPosition, int endPosition, bool direction)
		{
			int step;
			int start, end;

			if (startPosition < 0)
				startPosition = 0;
			if (endPosition >= edit_positions.Length)
				endPosition = edit_positions.Length - 1;

			if (startPosition > endPosition)
				return InvalidIndex;

			step = direction ? 1 : -1;
			start = direction ? startPosition : endPosition;
			end = (direction ? endPosition : startPosition) + step;

			for (int i = start; i != end; i += step) {
				if (edit_positions [i].Editable)
					return i;
			}
			return InvalidIndex;
		}

		public int FindNonEditPositionFrom (int position, bool direction)
		{
			if (direction) {
				return FindNonEditPositionInRange (position, edit_positions.Length - 1, direction);
			} else {
				return FindNonEditPositionInRange (0, position, direction);
			}
		}

		public int FindNonEditPositionInRange (int startPosition, int endPosition, bool direction)
		{		
			int step;
			int start, end;

			if (startPosition < 0)
				startPosition = 0;
			if (endPosition >= edit_positions.Length)
				endPosition = edit_positions.Length - 1;

			if (startPosition > endPosition)
				return InvalidIndex;

			step = direction ? 1 : -1;
			start = direction ? startPosition : endPosition;
			end = (direction ? endPosition : startPosition) + step;

			for (int i = start; i != end; i += step) {
				if (!edit_positions [i].Editable)
					return i;
			}
			return InvalidIndex;
		}

		public int FindUnassignedEditPositionFrom (int position, bool direction)
		{
			if (direction) {
				return FindUnassignedEditPositionInRange (position, edit_positions.Length - 1, direction);
			} else {
				return FindUnassignedEditPositionInRange (0, position, direction);
			}
		}

		public int FindUnassignedEditPositionInRange (int startPosition, int endPosition, bool direction)
		{
			int step;
			int start, end;

			if (startPosition < 0)
				startPosition = 0;
			if (endPosition >= edit_positions.Length)
				endPosition = edit_positions.Length - 1;

			if (startPosition > endPosition)
				return InvalidIndex;

			step = direction ? 1 : -1;
			start = direction ? startPosition : endPosition;
			end = (direction ? endPosition : startPosition) + step;

			for (int i = start; i != end; i += step) {
				if (edit_positions [i].Editable && !edit_positions [i].FilledIn)
					return i;
			}
			return InvalidIndex;
		}

		public static bool GetOperationResultFromHint (MaskedTextResultHint hint)
		{
			return (hint == MaskedTextResultHint.CharacterEscaped || 
				hint == MaskedTextResultHint.NoEffect || 
				hint == MaskedTextResultHint.SideEffect || 
				hint == MaskedTextResultHint.Success);
		}

		public bool InsertAt (char input, int position)
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return InsertAt (input, position, out testPosition, out resultHint);
		}

		public bool InsertAt (string input, int position)
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return InsertAt (input, position, out testPosition, out resultHint);
		}

		public bool InsertAt (char input, int position, out int testPosition, out MaskedTextResultHint resultHint)
		{
			return InsertAt (input.ToString (), position, out testPosition, out resultHint);		
		}

		public bool InsertAt (string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
			
			if (position >= edit_positions.Length) {
				testPosition = position;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			if (input == string.Empty) {
				testPosition = position;
				resultHint = MaskedTextResultHint.NoEffect;
				return true;
			}
			
			bool result;

			result = InsertAtInternal (input, position, out testPosition, out resultHint, true);

			if (result) {	
				result = InsertAtInternal (input, position, out testPosition, out resultHint, false);
			}

			return result;	
		}

		public bool IsAvailablePosition (int position)
		{
			if (position < 0 || position >= edit_positions.Length)
				return false;
				
			return edit_positions [position].Editable && !edit_positions [position].FilledIn;
		}

		public bool IsEditPosition (int position)
		{
			if (position < 0 || position >= edit_positions.Length)
				return false;
				
			return edit_positions [position].Editable;
		}

		public static bool IsValidInputChar (char c)
		{
			return char.IsLetterOrDigit (c) || char.IsPunctuation (c) || char.IsSymbol (c) || c == ' ';
		}

		public static bool IsValidMaskChar (char c)
		{
			return char.IsLetterOrDigit (c) || char.IsPunctuation (c) || char.IsSymbol (c) || c == ' ';
		}

		public static bool IsValidPasswordChar (char c)
		{
			return char.IsLetterOrDigit (c) || char.IsPunctuation (c) || char.IsSymbol (c) || c == ' ' || c == char.MinValue;
		}

		public bool Remove ()
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return Remove (out testPosition, out resultHint);
		}

		public bool Remove (out int testPosition, out MaskedTextResultHint resultHint)
		{
			if (LastAssignedPosition == InvalidIndex) {
				resultHint = MaskedTextResultHint.NoEffect;
				testPosition = 0;
				return true;
			}
			
			testPosition = LastAssignedPosition;
			resultHint = MaskedTextResultHint.Success;
			edit_positions [LastAssignedPosition].input = char.MinValue;
			
			return true;
		}

		public bool RemoveAt (int position)
		{
			return RemoveAt (position, position);
		}

		public bool RemoveAt (int startPosition, int endPosition)
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return RemoveAt (startPosition, endPosition, out testPosition, out resultHint);
		}

		public bool RemoveAt (int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
		{	
			bool result;
			
			result = RemoveAtInternal (startPosition, endPosition, out testPosition, out resultHint, true);
			
			if (result) {
				result = RemoveAtInternal (startPosition, endPosition, out testPosition, out resultHint, false);
			}
		
			return result;
		}

		public bool Replace (char input, int position)
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return Replace (input, position, out testPosition, out resultHint);
		}

		public bool Replace (string input, int position)
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return Replace (input, position, out testPosition, out resultHint);
		}

		public bool Replace (char input, int position, out int testPosition, out MaskedTextResultHint resultHint)
		{
			if (position < 0 || position >= edit_positions.Length) {
				testPosition = position;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}

			if (VerifyEscapeChar (input, position)) {
				if (edit_positions [position].FilledIn && edit_positions [position].Editable && (input == ' ' && ResetOnSpace) || (input == PromptChar && ResetOnPrompt)) {
					edit_positions [position].Reset ();
					resultHint = MaskedTextResultHint.SideEffect;
				} else {
					resultHint = MaskedTextResultHint.CharacterEscaped;
				}
				testPosition = position;
				return true;
			}
			
			int current_edit_position;
			current_edit_position = FindEditPositionFrom (position, true);

			if (current_edit_position == InvalidIndex) {
				testPosition = position;
				resultHint = MaskedTextResultHint.UnavailableEditPosition;
				return false;
			}

			if (!IsValidInputChar (input)) {
				testPosition = current_edit_position;
				resultHint = MaskedTextResultHint.InvalidInput;
				return false;
			}
			
			return ReplaceInternal (edit_positions, input, current_edit_position, out testPosition, out resultHint, false);
		}

		public bool Replace (string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			if (position < 0 || position >= edit_positions.Length) {
				testPosition = position;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			if (input.Length == 0) {
				return RemoveAt (position, position, out testPosition, out resultHint);
			}
			
			bool result;
			
			result = ReplaceInternal (input, position, edit_positions.Length - 1, out testPosition, out resultHint, true, true);
			
			if (result) {
				result = ReplaceInternal (input, position, edit_positions.Length - 1, out testPosition, out resultHint, false, true);	
			}

			return result;
		}

		public bool Replace (char input, int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
		{

			if (endPosition >= edit_positions.Length) {
				testPosition = endPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}

			if (startPosition < 0) {
				testPosition = startPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			if (startPosition > endPosition) {
				testPosition = startPosition;
				resultHint = MaskedTextResultHint.PositionOutOfRange;
				return false;
			}
			
			if (startPosition == endPosition) {
				return ReplaceInternal (edit_positions, input, startPosition, out testPosition, out resultHint, false);
			}

			return Replace (input.ToString (), startPosition, endPosition, out testPosition, out resultHint);
		}

		public bool Replace (string input, int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
		{
			bool result;
			
			result = ReplaceInternal (input, startPosition, endPosition, out testPosition, out resultHint, true, false);
			
			if (result) {
				result = ReplaceInternal (input, startPosition, endPosition, out testPosition, out resultHint, false, false);
			}
			return result;
		}

		public bool Set (string input)
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return Set (input, out testPosition, out resultHint);
		}

		public bool Set (string input, out int testPosition, out MaskedTextResultHint resultHint)
		{
			bool result;

			if (input == null) {
				throw new ArgumentNullException ("input");
			}

			result = VerifyStringInternal (input, out testPosition, out resultHint, 0, true);
			
			if (result) {
				result = VerifyStringInternal (input, out testPosition, out resultHint, 0, false);
			}
			
			return result;
		}

		public string ToDisplayString ()
		{
			return ToString (false, true, true, 0, Length);
		}

		public override string ToString ()
		{
			return ToString (true, IncludePrompt, IncludeLiterals, 0, Length);
		}

		public string ToString (bool ignorePasswordChar)
		{
			return ToString (ignorePasswordChar, IncludePrompt, IncludeLiterals, 0, Length);
		}

		public string ToString (bool includePrompt, bool includeLiterals)
		{
			return ToString (true, includePrompt, includeLiterals, 0, Length);
		}

		public string ToString (int startPosition, int length)
		{
			return ToString (true, IncludePrompt, IncludeLiterals, startPosition, length);
		}

		public string ToString (bool ignorePasswordChar, int startPosition, int length)
		{
			return ToString (ignorePasswordChar, IncludePrompt, IncludeLiterals, startPosition, length);
		}

		public string ToString (bool includePrompt, bool includeLiterals, int startPosition, int length)
		{
			return ToString (true, includePrompt, includeLiterals, startPosition, length);
		}

		public string ToString (bool ignorePasswordChar, bool includePrompt, bool includeLiterals, int startPosition, int length)
		{
			if (startPosition < 0)
				startPosition = 0;
			
			if (length <= 0)
				return string.Empty;
			
			StringBuilder result = new StringBuilder ();
			int start = startPosition;
			int end = startPosition + length - 1;
			
			if (end >= edit_positions.Length) {
				end = edit_positions.Length - 1;
			}
			int last_assigned_position = FindAssignedEditPositionInRange (start, end, false);
			
			// Find the last position in the mask to check for
			if (!includePrompt) {
				int last_non_edit_position;
				
				last_non_edit_position = FindNonEditPositionInRange (start, end, false);
				
				if (includeLiterals) {
					end = last_assigned_position > last_non_edit_position ? last_assigned_position : last_non_edit_position;
				} else {
					end = last_assigned_position;
				}
			}
			
			for (int i = start; i <= end; i++) {
				EditPosition ed = edit_positions [i];
				
				if (ed.Type == EditType.Literal) {
					if (includeLiterals) {
						result.Append (ed.Text);
					}				
				} else if (ed.Editable) {
					if (IsPassword) {
						if (!ed.FilledIn) { // Nothing to hide or show.
							if (includePrompt)
								result.Append (PromptChar);
							else
								result.Append (" ");
						} else if (ignorePasswordChar)
							result.Append (ed.Input);
						else
							result.Append (PasswordChar);
					} else if (!ed.FilledIn) {
						if (includePrompt) {
							result.Append (PromptChar);
						} else if (includeLiterals) {
							result.Append (" ");
						} else if (last_assigned_position != InvalidIndex && last_assigned_position > i) {
							result.Append (" ");
						}
					} else {
						result.Append (ed.Text);
					}
				} else {
					if (includeLiterals)
						result.Append (ed.Text);
				}
			}
			
			return result.ToString ();
		}

		public bool VerifyChar (char input, int position, out MaskedTextResultHint hint)
		{
			return VerifyCharInternal (input, position, out hint, true);
		}

		public bool VerifyEscapeChar (char input, int position)
		{
			if (position >= edit_positions.Length || position < 0) {
				return false;
			}
			
			if (!edit_positions [position].Editable) {
				if (SkipLiterals) {
					return input.ToString () == edit_positions [position].Text;
				} else {
					return false;
				}
			}
			
			if (ResetOnSpace && input == ' ') {
				return true;
			} else if (ResetOnPrompt && input == PromptChar) {
				return true;
			} else {
				return false;
			}
		}

		public bool VerifyString (string input)
		{
			int testPosition;
			MaskedTextResultHint resultHint;
			return VerifyString (input, out testPosition, out resultHint);
		}

		public bool VerifyString (string input, out int testPosition, out MaskedTextResultHint resultHint)
		{
			if (input == null || input.Length == 0) {			
				testPosition = 0;
				resultHint = MaskedTextResultHint.NoEffect;
				return true;
			}
			
			return VerifyStringInternal (input, out testPosition, out resultHint, 0, true);
		}
#endregion

#region Public properties
		public bool AllowPromptAsInput {
			get {
				return allow_prompt_as_input;
			}
		}
		
		public bool AsciiOnly {
			get {
				return ascii_only;
			}
		}
		
		public int AssignedEditPositionCount {
			get {
				int result = 0;
				for (int i = 0; i < edit_positions.Length; i++) {
					if (edit_positions [i].FilledIn) {
						result++;
					}
				}
				return result;
			}
		}
		
		public int AvailableEditPositionCount {
			get {
				int result = 0;
				foreach (EditPosition edit in edit_positions) {
					if (!edit.FilledIn && edit.Editable) {
						result++;
					}
				}
				return result;
			}
		}
		
		public CultureInfo Culture {
			get {
				return culture;
			}
		}
		
		public static char DefaultPasswordChar {
			get {
				return '*';
			}
		}
		
		public int EditPositionCount {
			get {
				int result = 0;
				foreach (EditPosition edit in edit_positions) {
					if (edit.Editable) {
						result++;
					}
				}
						
				return result;
			}
		}
		
		public IEnumerator EditPositions {
			get {
				System.Collections.Generic.List <int> result = new System.Collections.Generic.List<int> ();
				for (int i = 0; i < edit_positions.Length; i++) {
					if (edit_positions [i].Editable) {
						result.Add (i);
					}
				}
				return result.GetEnumerator ();
			}
		}
		
		public bool IncludeLiterals {
			get {
				return include_literals;
			}
			set {
				include_literals = value;
			}
		}
		
		public bool IncludePrompt {
			get {
				return include_prompt;
			}
			set {
				include_prompt = value;
			}
		}
		
		public static int InvalidIndex {
			get {
				return -1;
			}
		}
		
		public bool IsPassword {
			get {
				return password_char != char.MinValue;
			}
			set {
				password_char = value ? DefaultPasswordChar : char.MinValue;
			}
		}
		
		public char this [int index] {
			get {
				if (index < 0 || index >= Length) {
					throw new IndexOutOfRangeException (index.ToString ());
				}
				
				return ToString (true, true, true, 0, edit_positions.Length) [index];
			}
		}
		
		public int LastAssignedPosition {
			get {
				return FindAssignedEditPositionFrom (edit_positions.Length - 1, false);
			}
		}
		
		public int Length {
			get {
				int result = 0;
				for (int i = 0; i < edit_positions.Length; i++) {
					if (edit_positions [i].Visible)
						result++;
				}
				return result;
			}
		}
		
		public string Mask {
			get {
				return mask;
			}
		}
		
		public bool MaskCompleted {
			get {
				for (int i = 0; i < edit_positions.Length; i++)
					if (edit_positions [i].Required && !edit_positions [i].FilledIn)
						return false;
				return true;
			}
		}
		
		public bool MaskFull {
			get {
				for (int i = 0; i < edit_positions.Length; i++)
					if (edit_positions [i].Editable && !edit_positions [i].FilledIn)
						return false;
				return true;
			}
		}
		
		public char PasswordChar {
			get {
				return password_char;
			}
			set {
				password_char = value;
			}
		}
		
		public char PromptChar {
			get {
				return prompt_char;
			}
			set {
				prompt_char = value;
			}
		}
		
		public bool ResetOnPrompt {
			get {
				return reset_on_prompt;
			}
			set {
				reset_on_prompt = value;
			}
		}
		
		public bool ResetOnSpace {
			get {
				return reset_on_space;
			}
			set {
				reset_on_space = value;
			}
		}
		
		public bool SkipLiterals {
			get {
				return skip_literals;
			}
			set {
				skip_literals = value;
			}
		}
#endregion

	}
}
#endif
