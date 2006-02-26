//
// System.Data.OracleClient.Regex.cs
//
// Authors:
//   	Konstantin Triger (kostat@mainsoft.com)
//
// (c) 2006 Mainsoft corp. (http://www.mainsoft.com)
//

//
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

using System;
using System.Data.ProviderBase;

namespace System.Data.OracleClient
{
	/// <summary>
	/// Summary description for Regex.
	/// </summary>
	internal class OracleParamsRegex : SimpleRegex {
		//static readonly char[] charsEnd = new char[] {' ', '\f', '\t', '\v', '\r', '\n',  ',', ';', '(', ')', '[', ']','='};
		protected override SimpleMatch Match(string input, int beginning, int length) {

			int actualLen = length-1; //there must be something after @
			for (int i = beginning; i < actualLen; i++) {
				char ch = input[i];
				switch(ch) {
					case '\'': {
						int end = input.IndexOf('\'', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '"': {
						int end = input.IndexOf('"', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case '[': {
						int end = input.IndexOf(']', i+1);
						if (end < 0)
							break;

						i = end;
						break;
					}
					case ':': {
						int start = i;

						do {
							i++;
						}while (i < length && input[i] == ':');

						if (i - start > 1)
							break;

						while (i < length && IsWordChar(input[i]))
							i++;
	
						return new SimpleMatch(this, length, true, start, i-start, input);
					}
				}
			}

			return new SimpleMatch(this, length, false, length, 0, input);
		}
	}
}
