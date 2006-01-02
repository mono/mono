// created on 21.02.2002 at 17:06
//
// FrameDimension.cs
//
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Dennis Hayes (dennish@raytek.com)
// Sanjay Gupta <gsanjay@novell.com>
// Jordi Mas i Hernanez (jordi@ximian.com)

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Drawing.Imaging {

using System;

public sealed class FrameDimension {

	readonly Guid guid;
	
	// constructor
	public FrameDimension(Guid guid) 
	{
		this.guid = guid;
	}

	//properties
	public Guid Guid {
		get {
			return guid;
		}
	}
	
	public static FrameDimension Page {
		get {
			return new FrameDimension (new Guid ("7462dc86-6180-4c7e-8e3f-ee7333a7a483"));
		}
	}
	
	public static FrameDimension Resolution {
		get {
			return new FrameDimension (new Guid ("84236f7b-3bd3-428f-8dab-4ea1439ca315" ));			
		}
	}
	
	public static FrameDimension Time {
		get {
			return new FrameDimension (new Guid ("6aedbd6d-3fb5-418a-83a6-7f45229dc872" ));			
		}
	}

	//methods
	public override bool Equals(object o) 
	{
		if (!(o is FrameDimension))
			return false;		
		
		return (guid == ((FrameDimension)o).guid);			
	}
	
	public override int GetHashCode() 
	{
		return guid.GetHashCode ();
	}
	
	public override string ToString() 
	{
		return "FrameDimension :" + guid;
	}

}
}
