// NumberHelper.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Globalization;

namespace Mono.ILASM {

	/// <summary>
	/// </summary>
	internal class NumberHelper : StringHelperBase {

		private ILToken result;

		/// <summary>
		/// </summary>
		/// <param name="host"></param>
		public NumberHelper (ILTokenizer host) : base (host)
		{
			Reset ();
		}


		private void Reset ()
		{
			result = ILToken.Invalid.Clone() as ILToken;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override bool Start (char ch)
		{
			bool res = (Char.IsDigit (ch) || ch == '-' || (ch == '.' && Char.IsDigit ((char) host.Reader.Peek ())));
			Reset ();
			return res;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string Build ()
		{
			ILReader reader = host.Reader;
			reader.MarkLocation ();
			string num = reader.ReadToWhitespace ();

			NumberStyles nstyles = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint;

			try {
				if (num.IndexOf ('.') != -1) {
					double d = Double.Parse (num, nstyles, NumberFormatInfo.InvariantInfo);
					result.token = Token.FLOAT64;
					result.val = d;
				} else {
					long i = Int64.Parse (num, nstyles);
					if (i < Int32.MinValue || i > Int32.MaxValue) {
						result.token = Token.INT64;
						result.val = i;
					} else {
						result.token = Token.INT32;
						result.val = (int) i;
					}
				}
			} catch {
				reader.Unread (num.ToCharArray ());
				reader.RestoreLocation ();
				num = String.Empty;
				Reset ();
				throw new ILSyntaxError ("Bad number format!");
			}
			return num;
		}


		/// <summary>
		/// </summary>
		public ILToken ResultToken {
			get {
				return result;
			}
		}


	}

}
