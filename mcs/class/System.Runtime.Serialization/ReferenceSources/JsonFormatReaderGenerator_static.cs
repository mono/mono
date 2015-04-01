using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Runtime.Serialization.Json
{
	internal partial class JsonFormatReaderGenerator
	{
		partial class CriticalHelper
		{
			public JsonFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
			{
				throw new NotImplementedException ();
			}
			public JsonFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
			{
				throw new NotImplementedException ();
			}
			public JsonFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
			{
				throw new NotImplementedException ();
			}
		}
	}
}

