//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation.Xaml
{
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xaml;

    internal sealed class XamlObjectReaderWithSequence : XamlObjectReader
    {
        private int sequenceNumber;
        private Dictionary<int, object> sequenceNumberToObjectMap;
        private HashSet<object> visitedObjects;

        private Stack<object> objects = new Stack<object>();
        private XamlMember xamlMember = null;

        public XamlObjectReaderWithSequence(object instance, XamlSchemaContext schemaContext)
            : base(instance, schemaContext)
        {
        }

        public Dictionary<int, object> SequenceNumberToObjectMap
        {
            get
            {
                if (this.sequenceNumberToObjectMap == null)
                {
                    this.sequenceNumberToObjectMap = new Dictionary<int, object>();
                }

                return this.sequenceNumberToObjectMap;
            }
        }

        private HashSet<object> VisitedObjects
        {
            get
            {
                if (this.visitedObjects == null)
                {
                    this.visitedObjects = new HashSet<object>();
                }

                return this.visitedObjects;
            }
        }

        public override bool Read()
        {
            bool readResult = base.Read();

            if (readResult)
            {
                switch (this.NodeType)
                {
                    case XamlNodeType.StartObject:
                        this.objects.Push(this.Instance);
                        this.MapObjectWithSequenceNumber(this.Instance);
                        break;
                    case XamlNodeType.GetObject:
                        this.objects.Push(this.Instance);
                        break;
                    case XamlNodeType.EndObject:
                        this.objects.Pop();
                        break;
                    case XamlNodeType.StartMember:
                        this.xamlMember = this.Member;
                        break;
                    case XamlNodeType.EndMember:
                        this.xamlMember = null;
                        break;
                    case XamlNodeType.Value:
                        this.MapObjectWithSequenceNumber(this.GetRealObject());
                        break;
                }
            }

            return readResult;
        }

        // Current Node contains the value after original object is serialized to a ValueNode, this method
        // try to get the original object before it is serialized.
        private object GetRealObject()
        {
            if (this.Value is string)
            {
                object parent = this.objects.Peek();

                if (parent == null)
                {
                    return null;
                }

                // handle <InArgument x:TypeArguments="...">[expression]</InArgument>
                if (this.xamlMember == XamlLanguage.Initialization)
                {
                    Argument argument = parent as Argument;
                    if (argument != null)
                    {
                        return argument.Expression;
                    }

                    return null;
                }

                if (this.xamlMember == null || !this.xamlMember.IsNameValid || this.xamlMember.IsAttachable || this.xamlMember.IsDirective)
                {
                    return null;
                }

                // handle <x:Array Type="x:Int32"><x:Int32>1</x:Int32>...</x:Array>, type is not limited to x:Int32
                // Here property.DeclaringType would be ArrayExtension, while parent would be a real array (System.Int32[]), 
                // calling property.GetValue(parent, null) would cause a TargetException since the type and object doesn't match.
                // So stop further processing for ArrayExtension.
                if (this.xamlMember.DeclaringType == XamlLanguage.Array)
                {
                    // We can get the element type by parent.GetType().GetElementType(),
                    // but we really don't care about that, so just return null.
                    return null;
                }

                PropertyInfo property = this.xamlMember.UnderlyingMember as PropertyInfo;

                if (property == null || !property.CanRead)
                {
                    return null;
                }

                object realObject = property.GetValue(parent, null);

                // NOTE this is to handle argument containing an IValueSerializable expression
                // this logic should be kept the same as that in System.Activities.Debugger.XamlDebuggerXmlReader.NotifySourceLocationFound.
                Argument argumentObject = realObject as Argument;
                if (argumentObject != null && argumentObject.Expression is IValueSerializableExpression)
                {
                    realObject = argumentObject.Expression;
                }

                return realObject;
            }

            return this.Value;
        }

        private void MapObjectWithSequenceNumber(object mappedObject)
        {
            if (mappedObject == null)
            {
                return;
            }

            if (!this.VisitedObjects.Contains(mappedObject) && !(mappedObject is string))
            {
                this.VisitedObjects.Add(mappedObject);
                this.SequenceNumberToObjectMap.Add(this.sequenceNumber, mappedObject);
                this.sequenceNumber++;
            }
        }
    }
}
