//
// System.ComponentModel.Design.Serialization.CodeDomSerializerException.cs
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.CodeDom;
using System.Runtime.Serialization;

namespace System.ComponentModel.Design.Serialization
{
	public class CodeDomSerializerException : SystemException
	{
		[MonoTODO]
		public CodeDomSerializerException()
		{
		}

		[MonoTODO]
		public CodeDomSerializerException (Exception ex, 
						   CodeLinePragma code_line_pragma)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected CodeDomSerializerException (SerializationInfo info, 
						      StreamingContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public CodeDomSerializerException (string str, 
						   CodeLinePragma code_line_pragma)
		{
			throw new NotImplementedException();
		}

		public CodeLinePragma LinePragma {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info,
						    StreamingContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~CodeDomSerializerException()
		{
		}
	}
}
