//
// System.Web.Mail.MailAddressCollection.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
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
		list.Add( MailAddress.Parse( part ) );
	    }
	
	    return list;
	}
	
    }

}
