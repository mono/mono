﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using System.ComponentModel;
using DbLinq.Schema;

namespace DbMetal.Generator.EntityInterface.Implementation
{
#if !MONO_STRICT
    public
#endif
    class INotifyPropertyChangingImplementation : InterfaceImplementation
    {
        public override string InterfaceName
        {
            get { return typeof(INotifyPropertyChanging).Name; }
        }

        private const string sendPropertyChangingMethod = "SendPropertyChanging";

        /// <summary>
        /// Registers the required namespace
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="context"></param>
        public override void WriteHeader(CodeWriter writer, GenerationContext context)
        {
            writer.WriteUsingNamespace(typeof(INotifyPropertyChanging).Namespace);
        }

        public override void WriteClassHeader(CodeWriter writer, DbLinq.Schema.Dbml.Table table, GenerationContext context)
        {
            using (writer.WriteRegion(string.Format("{0} handling", typeof(INotifyPropertyChanging).Name)))
            {
                const string eventName = "PropertyChanging"; // do not change, part of INotifyPropertyChanging
                const string emptyArgs = "emptyChangingEventArgs";

                // event
                writer.WriteEvent(SpecificationDefinition.Public, eventName, typeof(PropertyChangingEventHandler).Name);
                writer.WriteLine();
                // empty event arg
                writer.WriteField(SpecificationDefinition.Private | SpecificationDefinition.Static,
                                  writer.GetAssignmentExpression(emptyArgs, writer.GetNewExpression(
                                                                                writer.GetMethodCallExpression(typeof(PropertyChangingEventArgs).Name, "\"\""))),
                                  typeof(PropertyChangingEventArgs).Name);
                // method
                using (writer.WriteMethod(SpecificationDefinition.Protected | SpecificationDefinition.Virtual,
                                          sendPropertyChangingMethod, null))
                {
                    using (writer.WriteIf(writer.GetDifferentExpression(eventName, writer.GetNullExpression())))
                    {
                        writer.WriteLine(writer.GetStatement(writer.GetMethodCallExpression(eventName,
                                                                                            writer.GetThisExpression(), emptyArgs)));
                    }
                }
            }
        }

        public override void WritePropertyBeforeSet(CodeWriter writer, DbLinq.Schema.Dbml.Column property, GenerationContext context)
        {
            writer.WriteLine(writer.GetStatement(writer.GetMethodCallExpression("On" + property.Member + "Changing", "value")));
            writer.WriteLine(writer.GetStatement(writer.GetMethodCallExpression(sendPropertyChangingMethod)));
        }
    }
}
