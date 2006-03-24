// StringHelperBase.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Text;

namespace Mono.ILASM {

	/// <summary>
	/// </summary>
	internal abstract class StringHelperBase {

		protected ILTokenizer host;
		protected int mode;

		/// <summary>
		/// </summary>
		/// <param name="host"></param>
		public StringHelperBase (ILTokenizer host) {
			this.host = host;
			mode = Token.UNKNOWN;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public abstract bool Start (char ch);


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public bool Start (int ch)
		{
			return Start ((char)ch);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public bool Start ()
		{
			return Start (host.Reader.Peek ());
		}


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public abstract string Build ();


		/// <summary>
		/// </summary>
		public int TokenId {
			get {
				return mode;
			}
		}

	}

}