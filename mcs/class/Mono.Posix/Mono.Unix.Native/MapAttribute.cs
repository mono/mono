//
// MapAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) Novell, Inc.  
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

[AttributeUsage (
		AttributeTargets.Class    |
		AttributeTargets.Delegate |
		AttributeTargets.Enum     |
		AttributeTargets.Field    |
		AttributeTargets.Struct)]
internal class MapAttribute : Attribute {
	private string nativeType;
	private string suppressFlags;

	public MapAttribute ()
	{
	}

	public MapAttribute (string nativeType)
	{
		this.nativeType = nativeType;
	}

	public string NativeType {
		get {return nativeType;}
	}

	public string SuppressFlags {
		get {return suppressFlags;}
		set {suppressFlags = value;}
	}
}

