/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// Metadata row interface.
	/// </summary>
	public interface Row {

		/// <summary>
		/// Number of colums in a row.
		/// </summary>
		int NumberOfColumns {get;}

		int Size {get;}

		/// <summary>
		/// Returns reference to parent table or null.
		/// </summary>
		MDTable Table {get;}

		void FromRawData(byte [] buff, int offs);

		void Dump(TextWriter writer);

	}


	public sealed class NullRow : Row {
		public static readonly NullRow Instance;

		static NullRow()
		{
			Instance = new NullRow();
		}

		private NullRow()
		{
		}

		public int NumberOfColumns {
			get {
				return 0;
			}
		}

		public int Size {
			get {
				return 0;
			}
		}

		public MDTable Table {
			get {
				return null;
			}
		}

		public void FromRawData(byte [] buff, int offs) 
		{
		}

		public void Dump(TextWriter writer)
		{
			writer.WriteLine("Null row.");
		}
	}

}
