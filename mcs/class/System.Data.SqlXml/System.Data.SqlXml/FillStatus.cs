//
// System.Data.SqlXml.FillStatus
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

namespace System.Data.SqlXml {
        public enum FillStatus
        {
		Continue,
		ErrorsOccurred,
		SkipCurrentRow
        }
}

#endif // NET_2_0
