// 
// System.Web.Services.Protocols.ServerProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
