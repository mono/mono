// 
// System.Web.Services.Protocols.UrlEncodedParameterWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Text;
using System.Web.Services;
using System.Web;
using System.Reflection;

namespace System.Web.Services.Protocols {
	public abstract class UrlEncodedParameterWriter : MimeParameterWriter {

		Encoding requestEncoding;
		ParameterInfo[] parameters;
		
		#region Constructors

		protected UrlEncodedParameterWriter () 
		{
		}
		
		#endregion // Constructors

		#region Properties 

		public override Encoding RequestEncoding {
			get { return requestEncoding; }
			set { requestEncoding = value; }
		}

		#endregion // Properties

		#region Methods

		protected void Encode (TextWriter writer, object[] values)
		{
			for (int n=0; n<values.Length; n++)
			{
				if (n>0) writer.Write ("&");
				Encode (writer, parameters[n].Name, values[n]);
			}
		}

		protected void Encode (TextWriter writer, string name, object value)
		{
			if (requestEncoding != null)
			{
				writer.Write (HttpUtility.UrlEncode (name, requestEncoding));
				writer.Write ("=");
				writer.Write (HttpUtility.UrlEncode (value.ToString(), requestEncoding));
			}
			else
			{
				writer.Write (HttpUtility.UrlEncode (name));
				writer.Write ("=");
				writer.Write (HttpUtility.UrlEncode (value.ToString()));
			}
				
		}

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			if (methodInfo.OutParameters.Length > 0) return null;
			else return methodInfo.Parameters;
		}

		public override void Initialize (object initializer)
		{
			parameters = (ParameterInfo[]) initializer;
		}

		#endregion // Methods
	}
}
