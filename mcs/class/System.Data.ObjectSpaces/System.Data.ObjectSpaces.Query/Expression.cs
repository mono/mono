//
// System.Data.ObjectSpaces.Query.Expression
//
//
// Author:
//	Richard Thombs (stony@stony.org)
//

#if NET_1_2

using System;
using System.Xml;

namespace System.Data.ObjectSpaces.Query
{
	[MonoTODO()]
	public abstract class Expression : ICloneable
	{
		public Expression parent;

		[MonoTODO()]
		protected Expression()
		{
			throw new NotImplementedException();
		}

		public abstract object Clone();

		[MonoTODO()]
		public static void EnumNodes(Expression root,EnumNodesCallBack callback)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static void EnumNodes(Expression root,EnumNodesCallBack callback,object[] oParams)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public object GetAnnotation(AnnotationType annotationType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public virtual bool IsArithmetic()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public virtual bool IsBoolean()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public virtual bool IsFilter()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static void Replace(Expression oldNode,Expression newNode)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public void SetAnnotation(AnnotationType annotationType,object o)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public string ToXmlString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public virtual void WriteXml(XmlWriter xmlw)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public virtual bool IsConst
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public virtual NodeType NodeType
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public Expression Owner
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public virtual Type ValueType
		{
			get { throw new NotImplementedException(); }
		}

	}
}

#endif
