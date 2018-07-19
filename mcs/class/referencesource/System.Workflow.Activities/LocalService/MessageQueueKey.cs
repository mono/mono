#pragma warning disable 1634, 1691
//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Text;
using System.Workflow.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.Runtime;

namespace System.Workflow.Activities
{
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class EventQueueName : IComparable
    {
        Type interfaceType;
        string operation;
        CorrelationProperty[] correlationValues;
        string activityId;

        [NonSerialized]
        string assemblyQualifiedName;

        [NonSerialized]
        String stringifiedKey;

        private string AssemblyQualifiedName
        {
            get
            {
                if (assemblyQualifiedName == null)
                {
                    assemblyQualifiedName = this.interfaceType.AssemblyQualifiedName;
                }
                return assemblyQualifiedName;
            }
        }


        public EventQueueName(Type interfaceType, string operation)
        {
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");
            if (operation == null)
                throw new ArgumentNullException("operation");

            this.interfaceType = interfaceType;
            this.operation = operation;
        }

        public EventQueueName(Type interfaceType, string operation, ICollection<CorrelationProperty> propertyValues)
            : this(interfaceType, operation)
        {
            if (propertyValues != null)
            {
                this.correlationValues = new CorrelationProperty[propertyValues.Count];
                propertyValues.CopyTo(this.correlationValues, 0);
            }
        }

        internal EventQueueName(Type interfaceType, string operation, string activityId)
            : this(interfaceType, operation)
        {
            this.activityId = activityId;
        }

        // properties
        public Type InterfaceType
        {
            get
            {
                return this.interfaceType;
            }
        }

        public string MethodName
        {
            get
            {
                return this.operation;
            }
        }

        public CorrelationProperty[] GetCorrelationValues()
        {
            return this.correlationValues;
        }

        public int CompareTo(Object toCompare)
        {
            if (toCompare is EventQueueName)
                return this.CompareTo(toCompare as EventQueueName);
            else
                return -1;
        }

        // IComparable implementation
        public int CompareTo(EventQueueName eventQueueName)
        {
            if (eventQueueName == null)
                return -1;

            // compare operation
            int compared = StringComparer.Ordinal.Compare(this.MethodName, eventQueueName.MethodName);

            if (compared == 0)
            {
                // compare type names
#pragma warning disable 56506
                compared = StringComparer.Ordinal.Compare(AssemblyQualifiedName, eventQueueName.AssemblyQualifiedName);
#pragma warning restore 56506

                if (compared == 0)
                {
                    if (this.correlationValues != null)
                    {
                        compared = (eventQueueName.correlationValues != null) ? (this.correlationValues.Length - eventQueueName.correlationValues.Length) : -1;

                        if (compared == 0)
                        {
                            // compare correlation values
                            for (int i = 0; i < this.correlationValues.Length; i++)
                            {
                                if (this.correlationValues[i] == null || eventQueueName.correlationValues[i] == null)
                                {
                                    compared = -1;
                                    break; // match failed
                                }

                                object leftValue = this.correlationValues[i].Value;
                                object rightValue = FindCorrelationValue(this.correlationValues[i].Name, eventQueueName.correlationValues);

#pragma warning suppress 56506
                                if (leftValue == null && rightValue == null)
                                {
                                    continue;
                                }

                                // do the explicit equals check
                                if (leftValue != null)
                                {
                                    IComparable comparable = leftValue as IComparable;
                                    if (comparable != null)
                                    {
                                        compared = comparable.CompareTo(rightValue);
                                        if (compared != 0)
                                        {
                                            break; // match failed
                                        }
                                    }
                                    else if ((!leftValue.Equals(rightValue)))
                                    {
                                        compared = -1;
                                        break; // match failed
                                    }
                                }
                                else
                                {
                                    compared = -1;
                                    break; // match failed
                                }
                            }
                        }
                    }
                }
            }
            return compared;
        }

        public override bool Equals(object obj)
        {
            EventQueueName k = obj as EventQueueName;
            //Without the cast we end up in op_Equality below
            if ((object)k == null)
                return false;

            return 0 == CompareTo(k);
        }

        public static bool operator ==(EventQueueName queueKey1, EventQueueName queueKey2)
        {
            bool equality = false;

            if ((object)queueKey1 != null)
            {
                if ((object)queueKey2 != null)
                {
                    equality = (0 == queueKey1.CompareTo(queueKey2));
                }
            }
            else if ((object)queueKey2 == null)
            {
                equality = true;
            }

            return equality;
        }

        public static bool operator !=(EventQueueName queueKey1, EventQueueName queueKey2)
        {
            return !(queueKey1 == queueKey2);
        }

        public static bool operator <(EventQueueName queueKey1, EventQueueName queueKey2)
        {
            if (queueKey1 == null)
                throw new ArgumentNullException("queueKey1");

            if (queueKey2 == null)
                throw new ArgumentNullException("queueKey2");

            return (queueKey1.CompareTo(queueKey2) < 0);
        }

        public static bool operator >(EventQueueName queueKey1, EventQueueName queueKey2)
        {
            if (queueKey1 == null)
                throw new ArgumentNullException("queueKey1");

            if (queueKey2 == null)
                throw new ArgumentNullException("queueKey2");

            return (queueKey1.CompareTo(queueKey2) > 0);
        }

        public override int GetHashCode()
        {
            if (String.IsNullOrEmpty(this.activityId))
                return (AssemblyQualifiedName.GetHashCode() ^ this.operation.GetHashCode());

            return (AssemblyQualifiedName.GetHashCode() ^ this.operation.GetHashCode() ^ this.activityId.GetHashCode());
        }

        public override string ToString()
        {
            if (stringifiedKey == null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("Message Properties");
                stringBuilder.AppendLine("Interface Type:" + this.interfaceType.ToString());
                stringBuilder.AppendLine("Method Name:" + this.operation.ToString());
                stringBuilder.AppendLine("CorrelationValues:");

                if (correlationValues != null)
                {
                    foreach (CorrelationProperty pred in correlationValues)
                    {
                        if (pred != null && pred.Value != null)
                            stringBuilder.AppendLine(pred.Value.ToString());
                    }
                }

                stringifiedKey = stringBuilder.ToString();
            }

            return stringifiedKey;
        }

        object FindCorrelationValue(string name, CorrelationProperty[] correlationValues)
        {
            object value = null;
            foreach (CorrelationProperty property in correlationValues)
            {
                if (property != null && property.Name == name)
                {
                    value = property.Value;
                    break;
                }
            }

            return value;
        }
    }
}
