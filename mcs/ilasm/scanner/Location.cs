// Location.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;

namespace Mono.ILASM {


	/// <summary>
	/// </summary>
	public class Location : ICloneable {
		internal int line;
		internal int column;


		/// <summary>
		/// </summary>
		public static readonly Location Unknown = new Location (-1, -1);

		/// <summary>
		/// </summary>
		public Location () {
			line = 1;
			column = 1;
		}

		/// <summary>
		/// </summary>
		/// <param name="line"></param>
		/// <param name="column"></param>
		public Location (int line, int column)
		{
			this.line = line;
			this.column = column;
		}


		/// <summary>
		/// </summary>
		/// <param name="that"></param>
		public Location (Location that)
		{
			this.line = that.line;
			this.column = that.column;
		}




		/// <summary>
		/// </summary>
		public void NewLine ()
		{
			++line;
			column = 1;
		}


		/// <summary>
		/// </summary>
		public void PreviousLine ()
		{
			--line;
			column = 1;
		}

		/// <summary>
		/// </summary>
		public void NextColumn ()
		{
			++column;
		}

		/// <summary>
		/// </summary>
		public void PreviousColumn ()
		{
			--column;
		}

		/// <summary>
		/// </summary>
		/// <param name="other"></param>
		public void CopyFrom (Location other)
		{
			this.line = other.line;
			this.column = other.column;
		}


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public virtual object Clone () {
			return new Location (this);
		}

		public override string ToString ()
		{
			return "line (" + line + ") column (" + column + ")";
		}
	}
}
