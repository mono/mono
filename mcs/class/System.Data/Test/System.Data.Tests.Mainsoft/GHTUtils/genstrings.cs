// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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
using System.IO;
using System.Globalization;
using System.Text;
using System.Collections;
using Microsoft.VisualBasic;

namespace GenStrings
{
	public class IntlStrings
	{
		//Random generator used to generate the random numbers.
		private Random rand;
		//*-------------------------------------------------------------------------------------------------
		//    Name           : IntlStrings ( constructor )
		//    Purpose        : Creates a random genrator and loads the default resources.
		//*-------------------------------------------------------------------------------------------------
		public IntlStrings()
		{
			//Create a random object.
			rand = new Random(10);//'cint(DateTime.Now.Ticks));
		}

		public IntlStrings(long lSeed):this(){}

		//*--------------------------------------------------------------------------------------------------
		//    Name           : GetRandString
		//    Purpose        : Generates a string composed of valid characters for the current locale ID. 
		//                     String is retrieved from TEXT block in the resouce file.
		//    Inputs         : iMaxChar -- int maximum number of unicode character to be generated.
		//                   : bAbsolute -- if set true, exact number (iMaxChar) will be generated,
		//                        if set false, number of generated chars will be random.
		//                   : bValidate -- boolean, if true, verify generated characters are valid
		//                        if false, does not verify generated characters
		//    Outputs        : Random generated string
		//*--------------------------------------------------------------------------------------------------        
		public string GetString(int iMaxChar, bool bAbsolute, bool bValidate, bool bNoLeadNum)
		{
			string strTemp ;
			if ( iMaxChar <= 0 )  // If the string length is zero, return an empty string
			{
				return String.Empty;
			}
            
			//'If (Not bAbsolute ) Then
			//'    iMaxChar = rand.Next( 1  , iMaxChar )
			//'End If

			strTemp = GetString(iMaxChar, true, true);
            
			//Include all the intestring characters.
			//rafi strTemp = InsertInterestingChars( strTemp );
			return strTemp;

		}

		public string GetString(int iMaxChar, bool bAbsolute, bool bValidate)
		{
			int idx;
			char[] chr_arr = new char[iMaxChar] ;

			for (idx = 0 ;idx < iMaxChar ;idx++) //'GetString must return exact size of iMaxChar
			{  //genstring must be random, otherwise ,if generated strings will be repeated, tests will fail.
				chr_arr[idx] = System.Convert.ToChar(64 + (System.DateTime.Now.Ticks + idx) % 60) ;
			}
			//'return strBuffer
			return new string(chr_arr);

		}
        

	}
                      
}