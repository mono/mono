//
// System.Drawing.Imaging.MetaHeader.cs
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

	public sealed class MetaHeader {

		short headerSize;
		int maxRecord;
		short noObjects;
		short noParameters;
		int size;
		short type;
		short version;

		// constructors
		public MetaHeader()
		{
		}

		// properties
		public short HeaderSize {
			get { return headerSize; }
			set { headerSize = value; }
		}
		
		public int MaxRecord {
			get { return maxRecord; }
			set { maxRecord = value; }
		}
		
		public short NoObjects {
			get { return noObjects; }
			set { noObjects = value; }
		}
		
		public short NoParameters {
			get { return noParameters; }
			set { noParameters = value; }
		}
		
		public int Size {
			get { return size; }
			set { size = value; }
		}

		public short Type {
			get { return type; }
			set { type = value; }
		}

		public short Version {
			get { return version; }
			set { version = value; }
		}
		
	}

}
