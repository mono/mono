// created on 21.02.2002 at 17:06
//
// FrameDimension.cs
//
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Dennis Hayes (dennish@raytek.com)
// Sanjay Gupta <gsanjay@novell.com>
// Jordi Mas i Hernanez (jordi@ximian.com)

namespace System.Drawing.Imaging {

using System;

public sealed class FrameDimension {

	internal Guid guid;
	
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

	//destructor
	~FrameDimension() 
	{
	
	}
}
}
