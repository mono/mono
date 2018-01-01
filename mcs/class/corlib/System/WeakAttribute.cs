using System;

#if MOBILE && !UNITY
namespace System {

[AttributeUsage(AttributeTargets.Field)]
public sealed class WeakAttribute : Attribute
{
}

}
#endif
