//
// System.Web.Mail.MailUtil.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
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
