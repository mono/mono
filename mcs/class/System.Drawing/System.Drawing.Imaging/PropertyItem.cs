//
// System.Drawing.Imaging.PropertyItem.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Drawing.Imaging {

	public sealed class PropertyItem {

		int id;
		int len;
		short type;
		byte[] value;

		// properties
		public int Id {
			get { return id; }
			set { id = value; }
		}

		public int Len {
			get { return len; }
			set { len = value; }
		}

		public short Type {
			get { return type; }
			set { type = value; }
		}

		public byte[] Value {
			get { return this.value; }
			set { this.value = value; }
		}

	}

}
