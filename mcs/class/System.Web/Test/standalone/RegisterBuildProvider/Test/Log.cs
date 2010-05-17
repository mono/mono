using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RegisterBuildProvider.Test
{
	public static class Log
	{
		public static List<string> Data { get; private set; }

		static Log ()
		{
			Data = new List<string> ();
		}
	}
}