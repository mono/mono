//
// System.Data.ObjectSpaces.ValueRecordMergeEventArgs.cs : The argument passed when a ValueRecord's merge event occurs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public class ValueRecordMergeEventArgs
        {
                private ValueRecord currentRecord;        //The current value record
                private ValueRecord originalRecord;       //The original value record
                private ValueRecord persistentRecord;     //The persistent value record
                
                
                //Simple constructors are ideal to code on Monday afternoons
                public ValueRecordMergeEventArgs (ValueRecord currentRecord,
                        ValueRecord originalRecord, ValueRecord persistentRecord)
                {
                        this.currentRecord = currentRecord;        
                        this.originalRecord = originalRecord;
                        this.persistentRecord = persistentRecord;
                }


                //Properties
                public ValueRecord CurrentValueRecord { 
                        get{ return currentRecord; }
                } 

                public ValueRecord OriginalValueRecord { 
                        get{ return originalRecord; }
                } 

                public ValueRecord PersistentValueRecord { 
                        get{ return persistentRecord; }
                } 
        }
}

#endif