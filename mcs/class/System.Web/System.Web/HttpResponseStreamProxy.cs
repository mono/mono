// 
// System.Web.HttpResponseStreamProxy
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.IO;

namespace System.Web
{
	class HttpResponseStreamProxy : HttpResponseStream
	{
		bool filteringActive;

		internal HttpResponseStreamProxy (HttpWriter writer) : base (writer)
		{
		}

		internal void CheckFilteringState ()
		{
			if (!filteringActive)
				throw new HttpException ("Invalid response filter state");
		}

		internal bool Active {
			get { return filteringActive; }
			set { filteringActive = value; }
		}

		public override void Flush ()
		{
		}

		public override void Close ()
		{
		}

		public override void Write (byte [] buffer, int offset, int length)
		{
			CheckFilteringState ();
			base.Write (buffer, offset, length);
		}
	}
}

