// 
// System.Web.Services.Protocols.ServerProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	[MonoTODO ("Figure out what this class does.")]
	internal abstract class ServerProtocol {

		HttpContext _context;

		#region Constructors

		internal ServerProtocol (HttpContext context)
		{
			_context = context;
		}

		protected ServerProtocol ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		public HttpContext Context {
			get { return _context; }
		}

		public abstract bool IsOneWay {
			get;
		}

		public abstract LogicalMethodInfo MethodInfo {
			get;
		}

		[MonoTODO]
		public virtual Exception OnewayInitException {
			get { throw new NotImplementedException (); }
		}

		public HttpRequest Request {
			get { return _context.Request; }
		}

		public HttpResponse Response {
			get { return _context.Response; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		protected void AddToCache (Type t1, Type t2, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void CreateServerInstance ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void DisposeServerInstance ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GenerateFaultString (Exception exception)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object GetFromCache (Type t1, Type t2)
		{
			throw new NotImplementedException ();
		}

		public abstract bool Initialize ();
                public abstract object[] ReadParameters ();

		[MonoTODO]
		public virtual bool WriteException (Exception e, Stream outputStream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteOneWayResponse ()
		{
			throw new NotImplementedException ();
		}

		public abstract void WriteReturns (object[] returnValues, Stream outputStream);

		#endregion
	}
}
