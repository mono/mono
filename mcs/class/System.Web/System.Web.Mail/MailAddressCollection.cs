//
// System.Web.Mail.MailAddressCollection.cs
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
using System.Collections;

namespace System.Web.Mail {

    // represents a collection of MailAddress objects
    internal class MailAddressCollection : IEnumerable {
	
	protected ArrayList data = new ArrayList();
	
	public MailAddress this[ int index ] {
	    get { return this.Get( index ); }
	}

	public int Count { get { return data.Count; } }
	
	public void Add( MailAddress addr ) { data.Add( addr ); }
	public MailAddress Get( int index ) { return (MailAddress)data[ index ]; }

	public IEnumerator GetEnumerator() {
	    return data.GetEnumerator();
	}
    
     
	public override string ToString() {
	    
	    StringBuilder builder = new StringBuilder();
	    for( int i = 0; i <data.Count ; i++ ) {
		MailAddress addr = this.Get( i );
		
		builder.Append( addr );
		
		if( i != ( data.Count - 1 ) ) builder.Append( ",\r\n  " );
	    }

	    return builder.ToString(); 
	}

	public static MailAddressCollection Parse( string str ) {
	    
	    if( str == null ) throw new ArgumentNullException("Null is not allowed as an address string");
	    
	    MailAddressCollection list = new MailAddressCollection();
	    
	    string[] parts = str.Split( new char[] { ',' , ';' } );
	    
	    foreach( string part in parts ) {
	    	MailAddress add = MailAddress.Parse (part);
		if (add == null)
			continue;

		list.Add (add);
	    }
	
	    return list;
	}
	
    }

}
