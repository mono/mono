//
// System.Data.ObjectSpaces.IObjectHelper.cs - Helps out a bit...
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public interface IObjectHelper
        {
                object this [string name] {
			get;
			set;
                }
                                                                                                                 
        }
}

#endif
