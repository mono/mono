using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace System.Runtime.Serialization.Json
{
	internal partial class JsonFormatWriterGenerator
	{
		partial class CriticalHelper
		{
			internal JsonFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
			{
				throw new NotImplementedException ();
			}
			internal JsonFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
			{
				throw new NotImplementedException ();
			}
		}
	}
}

