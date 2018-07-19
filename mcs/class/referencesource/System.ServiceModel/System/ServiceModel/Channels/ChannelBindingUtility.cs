//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Threading;
    using System.Net;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Authentication.ExtendedProtection.Configuration;

    static class ChannelBindingUtility
    {
        static ExtendedProtectionPolicy disabledPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
        static ExtendedProtectionPolicy defaultPolicy = disabledPolicy;

        public static ExtendedProtectionPolicy DisabledPolicy
        {
            get
            {
                return disabledPolicy;
            }
        }

        public static ExtendedProtectionPolicy DefaultPolicy
        {
            get
            {
                return defaultPolicy;
            }
        }

        public static bool IsDefaultPolicy(ExtendedProtectionPolicy policy)
        {
            return Object.ReferenceEquals(policy, defaultPolicy);
        }

        public static void CopyFrom(ExtendedProtectionPolicyElement source, ExtendedProtectionPolicyElement destination)
        {
            destination.PolicyEnforcement = source.PolicyEnforcement;
            destination.ProtectionScenario = source.ProtectionScenario;
            destination.CustomServiceNames.Clear();
            foreach (ServiceNameElement sourceEntry in source.CustomServiceNames)
            {
                ServiceNameElement entry = new ServiceNameElement();
                entry.Name = sourceEntry.Name;
                destination.CustomServiceNames.Add(entry);
            }
        }

        public static void InitializeFrom(ExtendedProtectionPolicy source, ExtendedProtectionPolicyElement destination)
        {
            if (!IsDefaultPolicy(source))
            {
                destination.PolicyEnforcement = source.PolicyEnforcement;
                destination.ProtectionScenario = source.ProtectionScenario;
                destination.CustomServiceNames.Clear();

                if (source.CustomServiceNames != null)
                {
                    foreach (string name in source.CustomServiceNames)
                    {
                        ServiceNameElement entry = new ServiceNameElement();
                        entry.Name = name;
                        destination.CustomServiceNames.Add(entry);
                    }
                }
            }
        }

        public static ExtendedProtectionPolicy BuildPolicy(ExtendedProtectionPolicyElement configurationPolicy)
        {
            //using this pattern allows us to have a different default policy
            //than the NCL team chooses.
            if (configurationPolicy.ElementInformation.IsPresent)
            {
                return configurationPolicy.BuildPolicy();
            }
            else
            {
                return ChannelBindingUtility.DefaultPolicy;
            }
        }

        public static ChannelBinding GetToken(SslStream stream)
        {
            return GetToken(stream.TransportContext);
        }

        public static ChannelBinding GetToken(TransportContext context)
        {
            ChannelBinding token = null;
            if (context != null)
            {
                token = context.GetChannelBinding(ChannelBindingKind.Endpoint);
            }
            return token;
        }

        public static ChannelBinding DuplicateToken(ChannelBinding source)
        {
            if (source == null)
            {
                return null;
            }

            return DuplicatedChannelBinding.CreateCopy(source);
        }

        public static void TryAddToMessage(ChannelBinding channelBindingToken, Message message, bool messagePropertyOwnsCleanup)
        {
            if (channelBindingToken != null)
            {
                ChannelBindingMessageProperty property = new ChannelBindingMessageProperty(channelBindingToken, messagePropertyOwnsCleanup);
                property.AddTo(message);
                property.Dispose(); //message.Properties.Add() creates a copy...
            }
        }

        //does not validate the ExtendedProtectionPolicy.CustomServiceNames collections on the policies
        public static bool AreEqual(ExtendedProtectionPolicy policy1, ExtendedProtectionPolicy policy2)
        {
            Fx.Assert(policy1 != null, "policy1 param cannot be null");
            Fx.Assert(policy2 != null, "policy2 param cannot be null");

            if (policy1.PolicyEnforcement == PolicyEnforcement.Never && policy2.PolicyEnforcement == PolicyEnforcement.Never)
            {
                return true;
            }

            if (policy1.PolicyEnforcement != policy2.PolicyEnforcement)
            {
                return false;
            }

            if (policy1.ProtectionScenario != policy2.ProtectionScenario)
            {
                return false;
            }

            if (policy1.CustomChannelBinding != policy2.CustomChannelBinding)
            {
                return false;
            }

            return true;
        }

        public static bool IsSubset(ServiceNameCollection primaryList, ServiceNameCollection subset)
        {
            bool result = false;
            if (subset == null || subset.Count == 0)
            {
                result = true;
            }
            else if (primaryList == null || primaryList.Count < subset.Count)
            {
                result = false;
            }
            else
            {
                ServiceNameCollection merged = primaryList.Merge(subset);

                //The merge routine only adds an entry if it is unique.
                result = (merged.Count == primaryList.Count);
            }

            return result;
        }

        public static void Dispose(ref ChannelBinding channelBinding)
        {
            // Explicitly cast to IDisposable to avoid the SecurityException.
            IDisposable disposable = (IDisposable)channelBinding;
            channelBinding = null;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        class DuplicatedChannelBinding : ChannelBinding
        {
            [Fx.Tag.SecurityNote(Critical = "Used when referencing raw native memory")]
            [SecurityCritical]
            int size;

            DuplicatedChannelBinding()
            {

            }

            public override int Size
            {
                [Fx.Tag.SecurityNote(Critical = "Used when referencing raw native memory",
                    Safe = "All inputs are validated during initization")]
                [SecuritySafeCritical]
                get { return this.size; }
            }

            [Fx.Tag.SecurityNote(Critical = "Invokes unsafe code.",
                Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
            [SecuritySafeCritical]
            internal static ChannelBinding CreateCopy(ChannelBinding source)
            {
                Fx.Assert(source != null, "source ChannelBinding should have been checked for null previously");

                if (source.IsInvalid || source.IsClosed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(source.GetType().FullName));
                }

                if (source.Size <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("source.Size", source.Size,
                        SR.GetString(SR.ValueMustBePositive)));
                }

                //Instantiate the SafeHandle before trying to allocate the native memory
                DuplicatedChannelBinding duplicate = new DuplicatedChannelBinding();

                //allocate the native memory and make a deep copy of the original.
                duplicate.Initialize(source);

                return duplicate;
            }

            [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.")]
            [SecurityCritical]
            unsafe void Initialize(ChannelBinding source)
            {
                //allocates the memory pointed to by this.handle
                //and sets this.size after allocation succeeds.
                AllocateMemory(source.Size);

                byte* sourceBuffer = (byte*)source.DangerousGetHandle().ToPointer();
                byte* destinationBuffer = (byte*)this.handle.ToPointer();

                for (int i = 0; i < source.Size; i++)
                {
                    destinationBuffer[i] = sourceBuffer[i];
                }

                this.size = source.Size;
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            void AllocateMemory(int bytesToAllocate)
            {
                Fx.Assert(bytesToAllocate > 0, "bytesToAllocate must be positive");

                //this protects us from problems like an appdomain shutdown occuring 
                //after allocating the native memory but before the handle gets set (which would result in a memory leak)
                RuntimeHelpers.PrepareConstrainedRegions();
                try { }
                finally
                {
                    base.SetHandle(Marshal.AllocHGlobal(bytesToAllocate));
                }
            }

            protected override bool ReleaseHandle()
            {
                Marshal.FreeHGlobal(this.handle);
                base.SetHandle(IntPtr.Zero);
                return true;
            }
        }
    }
}
