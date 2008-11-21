// Mono.Util.CorCompare.MissingField
//
// Author(s):
//   Piers Haken (piersh@friskit.com)
//
// (C) 2002 Piers Haken

using System;
using Mono.Cecil;

namespace Mono.Util.CorCompare
{

	/// <summary>
	/// 	Represents an interface implemented on a class
	/// </summary>
	/// <remarks>
	/// 	created by - Piers
	/// 	created on - 10:34 AM 3/12/2002
	/// </remarks>
	class MissingInterface : MissingBase
	{
		protected TypeReference ifaceMono;
		protected TypeReference ifaceMS;

		// e.g. <method name="Equals" status="missing"/>
		public MissingInterface (TypeReference _ifaceMono, TypeReference _ifaceMS)
		{
			ifaceMono = _ifaceMono;
			ifaceMS = _ifaceMS;
			m_nodeStatus = new NodeStatus (ifaceMono, ifaceMS);
		}

		public override string Type
		{
			get { return "interface"; }
		}
		public override string Name
		{
			get { return Interface.FullName; }
		}
		protected TypeReference Interface
		{
			get { return (ifaceMono != null) ? ifaceMono : ifaceMS; }
		}
		public override NodeStatus Analyze ()
		{
			return m_nodeStatus;
		}
	}
}
