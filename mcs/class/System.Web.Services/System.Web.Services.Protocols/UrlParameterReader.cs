// 
// System.Web.Services.Protocols.UrlParameterReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class UrlParameterReader : ValueCollectionParameterReader {

		#region Constructors

		public UrlParameterReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override object[] Read (HttpRequest request)
		{
			return Read (request.QueryString);
		}

		#endregion // Methods
	}
}
