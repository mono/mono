using System;

namespace NUnit.Util
{
	/// <summary>
	/// TestAgent provides a local representation
	/// for a RemoteTestAgent allowing the lifetime
	/// of the remote object to be independent of
	/// its own.
	/// </summary>
	public class TestAgent
	{
		#region Fields
		/// <summary>
		/// Reference to the TestAgency that controls this agent
		/// </summary>
		private TestAgency agency;

		/// <summary>
		/// This agent's assigned id
		/// </summary>
		private int agentId;

		/// <summary>
		/// Reference to the remote agent
		/// </summary>
		private RemoteTestAgent remoteAgent;
		#endregion

		#region Constructor
		public TestAgent( TestAgency agency, int agentId, RemoteTestAgent remoteAgent )
		{
			this.agency = agency;
			this.agentId = agentId;
			this.remoteAgent = remoteAgent;
		}
		#endregion

		#region Properties
		public TestAgency Agency
		{
			get { return agency; }
		}

		public int Id
		{
			get { return agentId; }
		}
		#endregion

		#region Public Methods
		public NUnit.Core.TestRunner CreateRunner(int runnerId)
		{
			return remoteAgent.CreateRunner( runnerId );
		}
		#endregion
	}
}
