// Mono.Util.CorCompare.MissingType
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Xml;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class method that missing.
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingType : MissingBase
	{
		// e.g. <class name="System.Byte" status="missing"/>
		protected Type theType;
		public MissingType(Type t) 
		{
			theType = t;
		}

		public override bool Equals(object o)
		{
			if (o is MissingType) 
			{
				return o.GetHashCode() == this.GetHashCode();
			}
			return false;
		}

		public override int GetHashCode() 
		{
			return theType.GetHashCode();
		}

		public override string Name 
		{
			get { return theType.Name; }
		}

		public override string Type
		{
			get
			{
				if (theType.IsEnum)
					return "enum";
				else if (theType.IsInterface)
					return "interface";
				else if (IsDelegate)
					return "delegate";
				else if (theType.IsValueType)
					return "struct";
				else
					return "class";
			}
		}

		public bool IsDelegate
		{
			get
			{
				if (theType.IsEnum || theType.IsInterface || theType.IsValueType)
					return false;
				Type type = theType.BaseType;
				while (type != null)
				{
					if (type.FullName == "System.Delegate")
						return true;
					type = type.BaseType;
				}
				return false;
			}
		}

		public virtual CompletionInfo Analyze ()
		{
			return new CompletionInfo ();
		}
	}
}
