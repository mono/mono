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
	class MissingType
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

		public string Name 
		{
			get 
			{
				return theType.Name;
			}
		}

		public string NameSpace 
		{
			get 
			{
				return theType.Namespace;
			}
		}

		public virtual string Status 
		{
			get 
			{
				return "missing";
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
		public virtual XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltClass;
			if (theType.IsEnum)
				eltClass = doc.CreateElement ("enum");
			else if (theType.IsInterface)
				eltClass = doc.CreateElement ("interface");
			else if (IsDelegate)
				eltClass = doc.CreateElement ("delegate");
			else if (theType.IsValueType)
				eltClass = doc.CreateElement ("struct");
			else
				eltClass = doc.CreateElement ("class");

			eltClass.SetAttribute ("name", Name);
			eltClass.SetAttribute ("status", Status);

			return eltClass;
		}
	}
}
