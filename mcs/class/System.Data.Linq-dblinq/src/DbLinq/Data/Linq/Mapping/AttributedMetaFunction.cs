#region MIT license
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

using System;
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Reflection;

#if MONO_STRICT
namespace System.Data.Linq.Mapping
#else
namespace DbLinq.Data.Linq.Mapping
#endif
{
    internal class AttributedMetaFunction : MetaFunction
    {
        public AttributedMetaFunction(MethodInfo method, FunctionAttribute attribute)
        {
            functionAttribute = attribute;
            methodInfo = method;
        }

        private MethodInfo methodInfo;
        private readonly FunctionAttribute functionAttribute;

        //private bool hasMultipleResults;
        public override bool HasMultipleResults
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsComposable
        {
            get { return functionAttribute.IsComposable; }
        }

        public override string MappedName
        {
            get { return functionAttribute.Name; }
        }

        public override MethodInfo Method
        {
            get { return methodInfo; }
        }

        public override MetaModel Model
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { return methodInfo.Name; }
        }

        public override ReadOnlyCollection<MetaParameter> Parameters
        {
            get { throw new NotImplementedException(); }
        }

        public override ReadOnlyCollection<MetaType> ResultRowTypes
        {
            get { throw new NotImplementedException(); }
        }

        public override MetaParameter ReturnParameter
        {
            get { throw new NotImplementedException(); }
        }
    }
}