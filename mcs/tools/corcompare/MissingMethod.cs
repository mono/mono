// Mono.Util.CorCompare.MissingMethod
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;
using System.Text;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class method that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingMethod : MissingMember 
	{
		// e.g. <method name="Equals" status="missing"/>
		public MissingMethod (MemberInfo infoMono, MemberInfo infoMS) : base (infoMono, infoMS) {}

		public override string Name {
			get {
				string s = Info.ToString();
				int index = s.IndexOf(' ');
				return s.Substring(index + 1);
			}
		}

		public override string Type {
			get {
				return "method";
			}
		}

		public override NodeStatus Analyze ()
		{
			m_nodeStatus = base.Analyze ();

			if (mInfoMono != null && mInfoMS != null)
			{
				MethodBase miMono = (MethodBase) mInfoMono;
				MethodBase miMS   = (MethodBase) mInfoMS;

				AddFlagWarning (miMono.IsAbstract, miMS.IsAbstract, "abstract");
				AddFlagWarning (miMono.IsStatic, miMS.IsStatic, "static");
				//AddFlagWarning (miMono.IsVirtual && !miMono.IsFinal, miMS.IsVirtual && !miMS.IsFinal, "virtual");
				AddFlagWarning (miMono.IsVirtual, miMS.IsVirtual, "virtual");
				AddFlagWarning (miMono.IsFinal, miMS.IsFinal, "final");
				AddFlagWarning (miMono.IsConstructor, miMS.IsConstructor, "a constructor");
				//AddFlagWarning (miMono.IsFinal, miMS.IsFinal, "sealed");
			}
			return m_nodeStatus;
		}
	}
}
