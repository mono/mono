// created on 21.02.2002 at 17:06
//
// FrameDimension.cs
//
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Dennis Hayes (dennish@raytek.com)
//

namespace System.Drawing.Imaging {

using System;

public sealed class FrameDimension {

	// constructor
	public FrameDimension (Guid guid) {}

	//properties
	public Guid Guid {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public static FrameDimension Page {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public static FrameDimension Resolution {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public static FrameDimension Time {
		get {
			throw new NotImplementedException ();
		}
	}
	
	//methods
	public override bool Equals (object o) {
		throw new NotImplementedException ();
	}
	
	public override int GetHashCode () {
		throw new NotImplementedException ();
	}
	
	public override string ToString() {
		throw new NotImplementedException ();
	}

	//destructor
	~FrameDimension () {}
}
}
