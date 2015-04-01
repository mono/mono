using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace System.Runtime.Serialization
{
	internal partial class XmlFormatWriterGenerator
	{
		partial class CriticalHelper
		{
			internal XmlFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
			{
				throw new NotImplementedException ();
			}
			internal XmlFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
			{
				throw new NotImplementedException ();
			}
		}
	}
}

