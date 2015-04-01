using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace System.Runtime.Serialization
{
	internal partial class XmlFormatReaderGenerator
	{
		partial class CriticalHelper
		{
			public XmlFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
			{
				throw new NotImplementedException ();
			}
			public XmlFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
			{
				throw new NotImplementedException ();
			}
			public XmlFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
			{
				throw new NotImplementedException ();
			}
		}
	}
}

