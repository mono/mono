/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// #GUID heap
	/// </summary>
	/// <remarks>
	/// 23.1.5
	/// </remarks>
	public class GUIDHeap : MDHeap {

		private byte [] data;

		internal GUIDHeap(MDStream stream) : base(stream)
		{
		}

		unsafe override public void FromRawData(byte [] rawData)
		{
			data = rawData;
		}

		public Guid this [int index] {
			get {
				if (index + 16 > data.Length)
					throw new IndexOutOfRangeException();
				byte [] buff = new byte [16];
				Buffer.BlockCopy(data, index, buff, 0, 16);
				return new Guid(buff);
			}
		}

	}

}