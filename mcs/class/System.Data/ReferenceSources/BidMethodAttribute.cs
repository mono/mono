using System;

/// <summary>
/// This attribute is used by FxCopBid rule to mark methods that accept format string and list of arguments that match it
/// FxCopBid rule uses this attribute to check if the method needs to be included in checks and to read type mappings
/// between the argument type to printf Type spec.
/// 
/// If you need to rename/remove the attribute or change its properties, make sure to update the FxCopBid rule!
/// </summary>
[System.Diagnostics.ConditionalAttribute("CODE_ANALYSIS")]
[System.AttributeUsage(AttributeTargets.Method)]
internal sealed class BidMethodAttribute : Attribute
{
    private bool m_enabled;

    /// <summary>
    /// enabled by default
    /// </summary>
    internal BidMethodAttribute()
    {
        m_enabled = true;
    }

    /// <summary>
    /// if Enabled is true, FxCopBid rule will validate all calls to this method and require that it will have string argument;
    /// otherwise, this method is ignored.
    /// </summary>
    public bool Enabled {
        get
        {
            return m_enabled;
        }
        set
        {
            m_enabled = value;
        }
    }
}