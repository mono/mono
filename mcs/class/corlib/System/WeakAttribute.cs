using System;

#if MOBILE
namespace System {

[AttributeUsage(AttributeTargets.Field)]
public sealed class WeakAttribute : Attribute
{
}

}
#endif
