#region MIT license
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
    class INotifyPropertyChangedImplementation : InterfaceImplementation
    {
        public override string InterfaceName
        {
            get { return typeof(INotifyPropertyChanged).Name; }
        }

        private const string sendPropertyChangedMethod = "SendPropertyChanged";

        /// <summary>
        /// Registers the required namespace
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="context"></param>
        public override void WriteHeader(CodeWriter writer, GenerationContext context)
        {
            writer.WriteUsingNamespace(typeof(INotifyPropertyChanged).Namespace);
        }

        public override void WriteClassHeader(CodeWriter writer, DbLinq.Schema.Dbml.Table table, GenerationContext context)
        {
            using (writer.WriteRegion(string.Format("{0} handling", typeof(INotifyPropertyChanged).Name)))
            {
                const string eventName = "PropertyChanged"; // do not change, part of INotifyPropertyChanged
                const string propertyNameName = "propertyName";

                // event
                writer.WriteEvent(SpecificationDefinition.Public, eventName, typeof(PropertyChangedEventHandler).Name);
                writer.WriteLine();
                // method
                using (writer.WriteMethod(SpecificationDefinition.Protected | SpecificationDefinition.Virtual,
                                          sendPropertyChangedMethod, null, new ParameterDefinition { Name = propertyNameName, Type = typeof(string) }))
                {
                    using (writer.WriteIf(writer.GetDifferentExpression(eventName, writer.GetNullExpression())))
                    {
                        writer.WriteLine(writer.GetStatement(writer.GetMethodCallExpression(eventName,
                                                                                            writer.GetThisExpression(),
                                                                                            writer.GetNewExpression(writer.GetMethodCallExpression(typeof(PropertyChangedEventArgs).Name,
                                                                                                                                                   propertyNameName)))));
                    }
                }
            }
        }

        public override void WritePropertyAfterSet(CodeWriter writer, DbLinq.Schema.Dbml.Column property, GenerationContext context)
        {
            writer.WriteLine(writer.GetStatement(writer.GetMethodCallExpression(sendPropertyChangedMethod,
                                                                                writer.GetLiteralValue(property.Member))));
            writer.WriteLine(writer.GetStatement(writer.GetMethodCallExpression("On" + property.Name + "Changed")));
        }
    }
}
