//
// System.Data.Mapping.FieldMap
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class FieldMap  
        {
		#region Properties
	
		[MonoTODO]
		public string NullValue {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Map OwnerMap {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string SourceConstant {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Field SourceField {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingParameter SourceParameter {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingArgumentType SourceType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string TargetConstant {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IDomainField TargetDomainField {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingParameter TargetParameter {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingArgumentType TargetType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool UseForConcurrency {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingAccess UseNull {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_2_0
