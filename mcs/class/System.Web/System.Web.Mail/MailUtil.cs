//
// System.Web.Mail.MailUtil.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
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
using System.Text;

namespace System.Web.Mail {
    
    // This class contains some utillity functions
    // that doesnt fit in other classes and to keep
    // high cohesion on the other classes.
    internal class MailUtil {
	
	// determines if a string needs to
	// be encoded for transfering over
	// the smtp protocol without risking
	// that it would be changed.
	public static bool NeedEncoding( string str ) {
	    
	    foreach( char chr in str ) {
		
		int ch = (int)chr;
		
		if( ! ( (ch > 61) && (ch < 127) || (ch>31) && (ch<61) ) ) {
		
		    return true;
		}
	    }
	    
	    return false;
	}

	// Encodes a string to base4
	public static string Base64Encode( string str ) {
	    return Convert.ToBase64String( Encoding.Default.GetBytes( str ) );
	}
	
	// Generate a unique boundary
	public static string GenerateBoundary() {
	    StringBuilder  boundary = new StringBuilder("__MONO__Boundary");
	    
	    boundary.Append("__");
	    
	    DateTime now = DateTime.Now;
	    boundary.Append(now.Year);
	    boundary.Append(now.Month);
	    boundary.Append(now.Day);
	    boundary.Append(now.Hour);
	    boundary.Append(now.Minute);
	    boundary.Append(now.Second);
	    boundary.Append(now.Millisecond);
	    	    
	    boundary.Append("__");
	    boundary.Append((new Random()).Next());
	    boundary.Append("__");
	    
	    return boundary.ToString();
	}

    }


}
