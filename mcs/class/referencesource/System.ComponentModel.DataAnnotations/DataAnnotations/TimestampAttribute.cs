
namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// This attribute is used to mark a Timestamp member of a Type. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class TimestampAttribute : Attribute {
    }
}
