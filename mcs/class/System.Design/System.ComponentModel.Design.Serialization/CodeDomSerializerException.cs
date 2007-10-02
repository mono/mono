//
// System.ComponentModel.Design.Serialization.CodeDomSerializerException.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2004-2005 Novell (http://www.novell.com)
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

using System.CodeDom;
using System.Runtime.Serialization;

namespace System.ComponentModel.Design.Serialization {

#if NET_2_0
[Serializable]
#endif
public class CodeDomSerializerException : SystemException
{
	private CodeLinePragma linePragma;

	public CodeDomSerializerException (Exception ex, CodeLinePragma linePragma)
		: base (String.Empty, ex) {

		this.linePragma = linePragma;
	}

	public CodeDomSerializerException (String message, CodeLinePragma linePragma)
		: base (message) {

		this.linePragma = linePragma;
	}

	[MonoTODO]
	protected CodeDomSerializerException (SerializationInfo info, StreamingContext context) {
		throw new NotImplementedException ();
	}

#if NET_2_0
	[MonoTODO]
	public CodeDomSerializerException (string message, IDesignerSerializationManager manager)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public CodeDomSerializerException (Exception ex, IDesignerSerializationManager manager)
	{
		throw new NotImplementedException ();
	}
#endif

	[MonoTODO]
	public override void GetObjectData (SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}

	public CodeLinePragma LinePragma {
		get {
			return linePragma;
		}
	}
}
}
