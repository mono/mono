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
//
// Authors:
//      Antonello Provenzano  <antonello@deveel.com>
//	Marek Safar <marek.safar@gmail.com>
//

namespace System.Data.Linq.Mapping
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ColumnAttribute : DataAttribute
    {

        #region Fields
        private AutoSync autoSync = ~AutoSync.Never;
        private bool canBeNull = true;
        private bool canBeNullSet;
        private UpdateCheck check = UpdateCheck.Always;
        private string dbtype;
        private string expression;
        private bool dbGen;
        private bool discrim;
        private bool pkey;
        private bool version;
        #endregion

        #region Properties
        public AutoSync AutoSync
        {
            get { return autoSync; }
            set { autoSync = value; }
        }

        public bool CanBeNull
        {
            get { return canBeNull; }
            set
            {
                canBeNullSet = true;
                canBeNull = value;
            }
        }

        internal bool CanBeNullSet
        {
            get { return canBeNullSet; }
        }

        public string DbType
        {
            get { return dbtype; }
            set { dbtype = value; }
        }

        public string Expression
        {
            get { return expression; }
            set { expression = value; }
        }

        public bool IsDbGenerated
        {
            get { return dbGen; }
            set { dbGen = value; }
        }

        public bool IsDiscriminator
        {
            get { return discrim; }
            set { discrim = value; }
        }

        public bool IsPrimaryKey
        {
            get { return pkey; }
            set { pkey = value; }
        }

        public bool IsVersion
        {
            get { return version; }
            set { version = value; }
        }

        public UpdateCheck UpdateCheck
        {
            get { return check; }
            set { check = value; }
        }
        #endregion
    }
}
