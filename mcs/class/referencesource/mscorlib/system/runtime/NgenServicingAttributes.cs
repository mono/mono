using System;

namespace System.Runtime
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class AssemblyTargetedPatchBandAttribute : Attribute
    {
        private String m_targetedPatchBand;

        public AssemblyTargetedPatchBandAttribute(String targetedPatchBand)
        {
            m_targetedPatchBand = targetedPatchBand;
        }

        public String TargetedPatchBand
        {
            get { return m_targetedPatchBand; }
        }
    }

// This attribute seems particularly prone to accidental inclusion in bcl.small
// We would only want to do so intentionally (if targeted patching were enabled there)
#if !FEATURE_CORECLR

    //============================================================================================================
    // [TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")] - 
    // Sacrifices cheap servicing of a method body in order to allow unrestricted inlining.  Certain types of
    // trivial methods (e.g. simple property getters) are automatically attributed by ILCA.EXE during the build.
    // For other performance critical methods, it should be added manually.
    //============================================================================================================

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class TargetedPatchingOptOutAttribute : Attribute
    {
        private String m_reason;

        public TargetedPatchingOptOutAttribute(String reason) 
        { 
            m_reason = reason;
        }

        public String Reason
        {
            get { return m_reason; }
        }

        private TargetedPatchingOptOutAttribute() { }
    }

#endif

    //============================================================================================================
    // [ForceTokenStabilization] - Using this CA forces ILCA.EXE to stabilize the attached type, method or field.
    // We use this to identify private helper methods invoked by IL stubs.
    //
    // NOTE: Attaching this to a type is NOT equivalent to attaching it to all of its methods!
    //============================================================================================================
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Delegate |
                    AttributeTargets.Method | AttributeTargets.Constructor |
                    AttributeTargets.Field
                   , AllowMultiple = false, Inherited = false)]
    sealed class ForceTokenStabilizationAttribute : Attribute
    {
        public ForceTokenStabilizationAttribute() { }
    }
}
