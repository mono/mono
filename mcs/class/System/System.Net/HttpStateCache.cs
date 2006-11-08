using System;
using System.Collections;

using mainsoft.apache.commons.httpclient;

namespace System.Net
{
	
	class HttpStateCache
	{
		private static readonly int MAX_SIZE = 30;

		private Stack _states;
		private int _currentSize;

		internal HttpStateCache()
		{
			_states = new Stack(20);
		}

		internal HttpState GetHttpState()
		{
			lock(this)
			{
				if(_states.Count > 0)
					return (HttpState) _states.Pop();
			}
			return new HttpState();
		}

		internal void ReleaseHttpState(HttpState state)
		{
			lock(this)
			{
				if(_states.Count < MAX_SIZE)
				{
					state.clear();
					_states.Push(state);
				}
			}
		}
	}
}
