using System;

/// <summary>
/// This attribute is used by FxCopBid rule to tell FXCOP the 'real' type sent to the native trace call for this argument. For
/// example, if Bid.Trace accepts enumeration value, but marshals it as string to the native trace method, set this attribute
/// on the argument and set ArgumentType = typeof(string)
/// 
/// It can be applied on a parameter, to let FxCopBid rule know the format spec type used for the argument, or it can be applied on a method,
/// to insert additional format spec arguments at specific location.
/// 
/// If you need to rename/remove the attribute or change its properties, make sure to update the FxCopBid rule!
/// </summary>
[System.Diagnostics.ConditionalAttribute("CODE_ANALYSIS")]
[System.AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple=true)]
internal sealed class BidArgumentTypeAttribute : Attribute
{
    // this overload can be used on the argument itself
    internal BidArgumentTypeAttribute(Type bidArgumentType)
    {
        this.ArgumentType = bidArgumentType;
        this.Index = -1; // if this c-tor is used on methods, default index value is 'last'
    }

    // this overload can be used on the method to add hidden spec arguments
    // set index to -1 to add an argument to the end
    internal BidArgumentTypeAttribute(Type bidArgumentType, int index)
    {
        this.ArgumentType = bidArgumentType;
        this.Index = index;
    }

    public readonly Type ArgumentType;
    // should be used only if attribute is applied on the method
    public readonly int Index;
}