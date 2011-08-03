//
// System.ActivationContext class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System {

	[Serializable]
	[ComVisible (false)]
	public sealed class ActivationContext : IDisposable, ISerializable {

		public enum ContextForm {
			Loose,
			StoreBounded
		}

		private ApplicationIdentity _appid;

		// FIXME:
        #pragma warning disable 649
		private ContextForm _form;
        #pragma warning restore 649
		private bool _disposed;

		private ActivationContext (ApplicationIdentity identity)
		{
			_appid = identity;
		}

		~ActivationContext ()
		{
			Dispose (false);
		}

		public ContextForm Form {
			get { return _form; }
		}

		public ApplicationIdentity Identity {
			get { return _appid; }
		}

		[MonoTODO ("Missing validation")]
		static public ActivationContext CreatePartialActivationContext (ApplicationIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");

			// TODO - throw new ArgumentException
			// - for invalid ApplicationIdentity

			return new ActivationContext (identity);
		}

		[MonoTODO("Missing validation")]
		static public ActivationContext CreatePartialActivationContext (ApplicationIdentity identity, string[] manifestPaths)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			if (manifestPaths == null)
				throw new ArgumentNullException ("manifestPaths");

			// TODO - throw new ArgumentException
			// - for invalid ApplicationIdentity
			// - not matching manifests
			// - number components != # manifest paths

			return new ActivationContext (identity);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			if (_disposed) {
				if (disposing) {
				}
				_disposed = true;
			}
		}

		[MonoTODO("Missing serialization support")]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
		}
	}
}

