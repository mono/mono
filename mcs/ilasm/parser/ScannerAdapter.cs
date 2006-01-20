// ScannerAdapter.cs
// (C) Sergey Chaban (serge@wildwestsoftware.com)

using System;

namespace Mono.ILASM {

	/// <summary>
	/// </summary>
	public class ScannerAdapter : yyParser.yyInput {

		private ITokenStream tokens;

		/// <summary>
		/// </summary>
		/// <param name="tokens"></param>
		public ScannerAdapter (ITokenStream tokens)
		{
			this.tokens = tokens;
		}


		/// <summary>
		/// </summary>
		public ITokenStream BaseStream {
			get {
				return tokens;
			}
		}

		//
		// yyParser.yyInput interface
		//

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public bool advance ()
		{
			return (tokens.NextToken != ILToken.EOF);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public int token ()
		{
			return tokens.LastToken.TokenId;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public object value ()
		{
			return tokens.LastToken.Value;
		}
	}
}

