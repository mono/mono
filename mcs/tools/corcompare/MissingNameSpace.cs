// Mono.Util.CorCompare.MissingNameSpace
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a namespace that has missing and/or MonoTODO classes.
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingNameSpace : MissingBase
	{
		// e.g. <namespace name="System" missing="267" todo="453" complete="21">
		public string name;
		protected Type [] rgTypesMS;

		public MissingNameSpace(string nameSpace, Type [] _rgTypesMS)
		{
			name = nameSpace;
			rgTypesMS = _rgTypesMS;
		}

		public virtual string [] MissingTypeNames (bool f)
		{
			return null;
		}

		public virtual ArrayList ToDoTypeNames
		{
			get { return null; }
		}
		public override string Name 
		{
			get { return name; }
		}
		public override string Type
		{
			get { return "namespace"; }
		}
	}
}

