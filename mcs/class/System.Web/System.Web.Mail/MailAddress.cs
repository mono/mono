//
// System.Web.Mail.MailAddress.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
//
using System;
using System.Text;

namespace System.Web.Mail {

    // Reperesents a mail address
    internal class MailAddress {
	
	protected string user;
	protected string host;
	protected string name;
	
	public string User {
	    get { return user; }
	    set { user = value; }
	}

	public string Host {
	    get { return host; }
	    set { host = value; }
	}

	public string Name {
	    get { return name; }
	    set { name = value; }
	}

	public string Address {
	    get { return String.Format( "{0}@{1}" , user , host ); }
	    set {
		
		string[] parts = value.Split( new char[] { '@' } );
		
		if( parts.Length != 2 ) 
		    throw new FormatException( "Invalid e-mail address: '" + value + "'.");
		
		user = parts[ 0 ];
		host = parts[ 1 ];
	    }
	}

	public static MailAddress Parse( string str ) {
	    MailAddress addr = new MailAddress();
	    string address = null;
	    string nameString = null;
	    string[] parts = str.Split( new char[] { ' ' } );
	    
	    // find the address: xxx@xx.xxx
	    // and put to gether all the parts
	    // before the address as nameString
	    foreach( string part in parts ) {
		
		if( part.IndexOf( '@' ) > 0 ) {
		    address = part;
		    break;
		}
		
		nameString = nameString + part + " ";
	    }

	    if( address == null ) 
		throw new FormatException( "Invalid e-mail address: '" + str + "'.");
	    
	    address = address.Trim( new char[] { '<' , '>' , '(' , ')' } );
	    
	    addr.Address = address;
	    
	    if( nameString != null ) {
		addr.Name = nameString.Trim();
		addr.Name = ( addr.Name.Length == 0 ? null : addr.Name ); 
	    }
	    
	    
	    return addr;
	} 
    
    
	public override string ToString() {
	    
	    string retString = "";
	
	    if( name == null ) {
		
		retString = String.Format( "<{0}>" , this.Address );
	    
	    } else {
		
		string personName = this.Name;

		if( MailUtil.NeedEncoding( personName ) ) {
		    personName = String.Format( "=?{0}?B?{1}?=",
					        Encoding.Default , 
						MailUtil.Base64Encode( personName ) ) ;
		}

		retString = String.Format( "\"{0}\" <{1}>" , personName , this.Address);
	    
	    }
	    
	    return retString;
	}
    }

}
