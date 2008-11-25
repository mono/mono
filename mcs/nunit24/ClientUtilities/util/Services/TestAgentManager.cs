using System;
using System.Threading;
using System.Collections.Specialized;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Services;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for TestAgentManager.
	/// </summary>
	public class TestAgentManager : ServerBase, IService
	{
		private ListDictionary agents = new ListDictionary();

		public TestAgentManager( string uri, int port ) : base( uri, port ) { }

		public void Register( object obj, int id )
		{
			agents[id] = obj;
		}

		public object GetTestRunner( int id )
		{
			return agents[id];
		}

		#region IService Members

		public void UnloadService()
		{
			// TODO:  Add TestAgentManager.UnloadService implementation
		}

		public void InitializeService()
		{
			this.Start();
		}

		#endregion
	}
}
