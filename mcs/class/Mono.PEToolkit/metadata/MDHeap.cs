/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// Base class for all metadata heaps.
	/// </summary>
	public abstract class MDHeap {


		protected MDStream stream;

		protected MDHeap(MDStream stream)
		{
			this.stream = stream;
			if (stream.RawData != null) {
				FromRawData(stream.RawData);
			}
		}

		public MDStream Stream {
			get {
				return stream;
			}
		}

		public abstract void FromRawData(byte [] rawData);


		/// <summary>
		/// Heap factory.
		/// </summary>
		/// <param name="stream">Base stream.</param>
		/// <returns></returns>
		public static MDHeap Create(MDStream stream)
		{
			MDHeap res = null;

			switch (stream.Name) {
				case "#~" :
				case "#-" :
					res = new TablesHeap(stream);
					break;
				case "#Strings" :
					res = new StringsHeap(stream);
					break;
				case "#GUID" :
					res = new GUIDHeap(stream);
					break;
			}

			return res;
		}

	}
}
