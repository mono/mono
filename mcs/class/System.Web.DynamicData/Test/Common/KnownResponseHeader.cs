using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
    class KnownResponseHeader
    {
        private int index;
        private string value;

        public KnownResponseHeader (int index, string value)
        {
            this.index = index;
            this.value = value;
        }

        public int Index
        {
            get { return index; }
        }

        public string Value
        {
            get { return value; }
        }
    }
}
