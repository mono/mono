﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace DbLinq.Data.Linq.Mapping
{
    [DebuggerDisplay("MetaTable for {TableName}")]
    internal class AttributedMetaTable : MetaTable
    {
        public AttributedMetaTable(TableAttribute attribute, MetaType type, MetaModel model)
        {
            _tableAttribute = attribute;
            _metaType = type;
        	_containingModel = model;

			//If the attribute doesn't specify a table name the name of the table class is used
			if(attribute.Name != null)
			{
				_tableName = attribute.Name;
			}
			else
			{
				_tableName = type.Name;
			}
        }

        private TableAttribute _tableAttribute;
        private MetaType _metaType;
    	private MetaModel _containingModel;
    	private readonly string _tableName;

        public override MethodInfo DeleteMethod
        {
            get { throw new System.NotImplementedException(); }
        }

        public override MethodInfo InsertMethod
        {
            get { throw new System.NotImplementedException(); }
        }

        public override MetaModel Model
        {
            get { return _containingModel; }
        }

        public override MetaType RowType
        {
            get { return _metaType; }
        }

        public override string TableName
        {
            get { return _tableName; }
        }

        public override MethodInfo UpdateMethod
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}