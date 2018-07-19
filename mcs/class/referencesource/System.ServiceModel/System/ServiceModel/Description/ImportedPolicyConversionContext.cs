//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Xml;

    public abstract partial class MetadataImporter
    {
        //Consider, Microsoft: make this public
        internal static IEnumerable<PolicyConversionContext> GetPolicyConversionContextEnumerator(ServiceEndpoint endpoint, PolicyAlternatives policyAlternatives)
        {
            return ImportedPolicyConversionContext.GetPolicyConversionContextEnumerator(endpoint, policyAlternatives, MetadataImporterQuotas.Defaults);
        }

        internal static IEnumerable<PolicyConversionContext> GetPolicyConversionContextEnumerator(ServiceEndpoint endpoint, PolicyAlternatives policyAlternatives,
            MetadataImporterQuotas quotas)
        {
            return ImportedPolicyConversionContext.GetPolicyConversionContextEnumerator(endpoint, policyAlternatives, quotas);
        }

        internal sealed class ImportedPolicyConversionContext : PolicyConversionContext
        {
            BindingElementCollection bindingElements = new BindingElementCollection();
            readonly PolicyAssertionCollection endpointAssertions;
            readonly Dictionary<OperationDescription, PolicyAssertionCollection> operationBindingAssertions = new Dictionary<OperationDescription, PolicyAssertionCollection>();
            readonly Dictionary<MessageDescription, PolicyAssertionCollection> messageBindingAssertions = new Dictionary<MessageDescription, PolicyAssertionCollection>();
            readonly Dictionary<FaultDescription, PolicyAssertionCollection> faultBindingAssertions = new Dictionary<FaultDescription, PolicyAssertionCollection>();

            ImportedPolicyConversionContext(ServiceEndpoint endpoint, IEnumerable<XmlElement> endpointAssertions,
                    Dictionary<OperationDescription, IEnumerable<XmlElement>> operationBindingAssertions,
                    Dictionary<MessageDescription, IEnumerable<XmlElement>> messageBindingAssertions,
                    Dictionary<FaultDescription, IEnumerable<XmlElement>> faultBindingAssertions,
                    MetadataImporterQuotas quotas)
                : base(endpoint)
            {
                int remainingAssertionsAllowed = quotas.MaxPolicyAssertions;

                this.endpointAssertions = new PolicyAssertionCollection(new MaxItemsEnumerable<XmlElement>(endpointAssertions, remainingAssertionsAllowed));

                remainingAssertionsAllowed -= this.endpointAssertions.Count;

                foreach (OperationDescription operationDescription in endpoint.Contract.Operations)
                {
                    this.operationBindingAssertions.Add(operationDescription, new PolicyAssertionCollection());

                    foreach (MessageDescription messageDescription in operationDescription.Messages)
                    {
                        this.messageBindingAssertions.Add(messageDescription, new PolicyAssertionCollection());
                    }

                    foreach (FaultDescription faultDescription in operationDescription.Faults)
                    {
                        this.faultBindingAssertions.Add(faultDescription, new PolicyAssertionCollection());
                    }
                }


                foreach (KeyValuePair<OperationDescription, IEnumerable<XmlElement>> entry in operationBindingAssertions)
                {
                    this.operationBindingAssertions[entry.Key].AddRange(new MaxItemsEnumerable<XmlElement>(entry.Value, remainingAssertionsAllowed));
                    remainingAssertionsAllowed -= this.operationBindingAssertions[entry.Key].Count;
                }

                foreach (KeyValuePair<MessageDescription, IEnumerable<XmlElement>> entry in messageBindingAssertions)
                {
                    this.messageBindingAssertions[entry.Key].AddRange(new MaxItemsEnumerable<XmlElement>(entry.Value, remainingAssertionsAllowed));
                    remainingAssertionsAllowed -= this.messageBindingAssertions[entry.Key].Count;
                }

                foreach (KeyValuePair<FaultDescription, IEnumerable<XmlElement>> entry in faultBindingAssertions)
                {
                    this.faultBindingAssertions[entry.Key].AddRange(new MaxItemsEnumerable<XmlElement>(entry.Value, remainingAssertionsAllowed));
                    remainingAssertionsAllowed -= this.faultBindingAssertions[entry.Key].Count;
                }
            }

            //
            // PolicyConversionContext implementation
            //

            public override BindingElementCollection BindingElements { get { return this.bindingElements; } }

            public override PolicyAssertionCollection GetBindingAssertions()
            {
                return this.endpointAssertions;
            }

            public override PolicyAssertionCollection GetOperationBindingAssertions(OperationDescription operation)
            {
                return this.operationBindingAssertions[operation];
            }

            public override PolicyAssertionCollection GetMessageBindingAssertions(MessageDescription message)
            {
                return this.messageBindingAssertions[message];
            }

            public override PolicyAssertionCollection GetFaultBindingAssertions(FaultDescription message)
            {
                return this.faultBindingAssertions[message];
            }

            //
            // Policy Alternative Enumeration code
            //

            public static IEnumerable<PolicyConversionContext> GetPolicyConversionContextEnumerator(ServiceEndpoint endpoint,
                PolicyAlternatives policyAlternatives, MetadataImporterQuotas quotas)
            {
                IEnumerable<Dictionary<MessageDescription, IEnumerable<XmlElement>>> messageAssertionEnumerator;
                IEnumerable<Dictionary<FaultDescription, IEnumerable<XmlElement>>> faultAssertionEnumerator;
                IEnumerable<Dictionary<OperationDescription, IEnumerable<XmlElement>>> operationAssertionEnumerator;
                faultAssertionEnumerator = PolicyIterationHelper.GetCartesianProduct<FaultDescription, IEnumerable<XmlElement>>(policyAlternatives.FaultBindingAlternatives);
                messageAssertionEnumerator = PolicyIterationHelper.GetCartesianProduct<MessageDescription, IEnumerable<XmlElement>>(policyAlternatives.MessageBindingAlternatives);
                operationAssertionEnumerator = PolicyIterationHelper.GetCartesianProduct<OperationDescription, IEnumerable<XmlElement>>(policyAlternatives.OperationBindingAlternatives);

                foreach (Dictionary<FaultDescription, IEnumerable<XmlElement>> faultAssertionsSelection in faultAssertionEnumerator)
                {
                    foreach (Dictionary<MessageDescription, IEnumerable<XmlElement>> messageAssertionsSelection in messageAssertionEnumerator)
                    {
                        foreach (Dictionary<OperationDescription, IEnumerable<XmlElement>> operationAssertionsSelection in operationAssertionEnumerator)
                        {
                            foreach (IEnumerable<XmlElement> endpointAssertionsSelection in policyAlternatives.EndpointAlternatives)
                            {
                                ImportedPolicyConversionContext conversionContext;
                                try
                                {
                                    conversionContext = new ImportedPolicyConversionContext(endpoint, endpointAssertionsSelection,
                                        operationAssertionsSelection, messageAssertionsSelection, faultAssertionsSelection,
                                        quotas);
                                }
                                catch (MaxItemsEnumeratorExceededMaxItemsException) { yield break; }

                                yield return conversionContext;
                            }
                        }
                    }
                }

            }

            internal class MaxItemsEnumerable<T> : IEnumerable<T>
            {
                IEnumerable<T> inner;
                int maxItems;

                public MaxItemsEnumerable(IEnumerable<T> inner, int maxItems)
                {
                    this.inner = inner;
                    this.maxItems = maxItems;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    return new MaxItemsEnumerator<T>(inner.GetEnumerator(), maxItems);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return (IEnumerator)GetEnumerator();
                }
            }

            internal class MaxItemsEnumerator<T> : IEnumerator<T>
            {
                int maxItems;
                int currentItem;
                IEnumerator<T> inner;

                public MaxItemsEnumerator(IEnumerator<T> inner, int maxItems)
                {
                    this.maxItems = maxItems;
                    this.currentItem = 0;
                    this.inner = inner;
                }

                public T Current
                {
                    get { return inner.Current; }
                }

                public void Dispose()
                {
                    inner.Dispose();
                }

                object IEnumerator.Current
                {
                    get { return ((IEnumerator)inner).Current; }
                }

                public bool MoveNext()
                {
                    bool moveNext = inner.MoveNext();
                    if (++currentItem > maxItems)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MaxItemsEnumeratorExceededMaxItemsException());
                    }
                    return moveNext;
                }

                public void Reset()
                {
                    currentItem = 0;
                    inner.Reset();
                }
            }

            internal class MaxItemsEnumeratorExceededMaxItemsException : Exception { }

            static class PolicyIterationHelper
            {
                // This method returns an iterator over the cartesian product of a colleciton of sets.
                //  e.g. If the following 3 sets are provided:
                //    i) { 1, 2, 3 }
                //   ii) { a, b }
                //  iii) { x, y, z }
                //
                // You would get an enumerator that returned the following 18 collections:
                // { 1, a, x}, { 2, a, x}, { 3, a, x}, { 1, b, x}, { 2, b, x}, { 3, b, x},
                // { 1, a, y}, { 2, a, y}, { 3, a, y}, { 1, b, y}, { 2, b, y}, { 3, b, y},
                // { 1, a, z}, { 2, a, z}, { 3, a, z}, { 1, b, z}, { 2, b, z}, { 3, b, z}
                //
                // This method allows us to enumerate over all the possible policy selections in a 
                // dictiaonary of policy alternatives.
                //   e.g. given all the policy alternatives in all the messages in a contract,
                //   we can enumerate over all the possilbe policy selections.
                //
                // Note: A general implementation of this method would differ in that it would probably use a List<T> or an array instead of
                // a dictionary and it would yield clones of the the counterValue. 
                //  - We don't clone because we know that we don't need to based on our useage 
                //  - We use a dictionary because we need to correlate the selections with the alternative source.
                //
                internal static IEnumerable<Dictionary<K, V>> GetCartesianProduct<K, V>(Dictionary<K, IEnumerable<V>> sets)
                {
                    Dictionary<K, V> counterValue = new Dictionary<K, V>(sets.Count);

                    // The iterator is implemented as a counter with each digit being an IEnumerator over one of the sets.
                    KeyValuePair<K, IEnumerator<V>>[] digits = InitializeCounter<K, V>(sets, counterValue);

                    do
                    {
                        yield return (Dictionary<K, V>)counterValue;
                    } while (IncrementCounter<K, V>(digits, sets, counterValue));

                }

                static KeyValuePair<K, IEnumerator<V>>[] InitializeCounter<K, V>(Dictionary<K, IEnumerable<V>> sets, Dictionary<K, V> counterValue)
                {
                    KeyValuePair<K, IEnumerator<V>>[] digits = new KeyValuePair<K, IEnumerator<V>>[sets.Count];

                    // Initialize the digit enumerators and set the counter's current Value.
                    int i = 0;
                    foreach (KeyValuePair<K, IEnumerable<V>> kvp in sets)
                    {
                        digits[i] = new KeyValuePair<K, IEnumerator<V>>(kvp.Key, kvp.Value.GetEnumerator());
                        if (!(digits[i].Value.MoveNext()))
                        {
                            Fx.Assert("each set must have at least one item in it");
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Each set must have at least one item in it")));
                        }
                        counterValue[digits[i].Key] = digits[i].Value.Current;
                        i++;
                    }

                    return digits;
                }

                static bool IncrementCounter<K, V>(KeyValuePair<K, IEnumerator<V>>[] digits, Dictionary<K, IEnumerable<V>> sets, Dictionary<K, V> counterValue)
                {

                    //
                    // Do rollover and carryying for digits.
                    //  - starting at least significant digit, move digits to  next value.
                    //    if digit rolls over, carry to next digit and repeat.
                    //
                    int currentDigit;
                    for (currentDigit = 0; currentDigit < digits.Length && !digits[currentDigit].Value.MoveNext(); currentDigit++)
                    {
                        IEnumerator<V> newDigit = sets[digits[currentDigit].Key].GetEnumerator();
                        digits[currentDigit] = new KeyValuePair<K, IEnumerator<V>>(digits[currentDigit].Key, newDigit);
                        digits[currentDigit].Value.MoveNext();
                    }

                    //
                    // if we just rolled over on the most significant digit, return false
                    //
                    if (currentDigit == digits.Length)
                        return false;

                    //
                    // update countervalue stores for all digits that changed.
                    //
                    for (int i = currentDigit; i >= 0; i--)
                    {
                        counterValue[digits[i].Key] = digits[i].Value.Current;
                    }
                    return true;
                }
            }

        }

        internal class PolicyAlternatives
        {
            public IEnumerable<IEnumerable<XmlElement>> EndpointAlternatives;
            public Dictionary<OperationDescription, IEnumerable<IEnumerable<XmlElement>>> OperationBindingAlternatives;
            public Dictionary<MessageDescription, IEnumerable<IEnumerable<XmlElement>>> MessageBindingAlternatives;
            public Dictionary<FaultDescription, IEnumerable<IEnumerable<XmlElement>>> FaultBindingAlternatives;
        }

    }
}
