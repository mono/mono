//
// System.Data.ObjectSpaces.ValueRecordMergeEventHandler.cs : The delegate for handling ValueRecord's merge events
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public delegate void ValueRecordMergeEventHandler (object sender, ValueRecordMergeEventArgs e);
}

#endif