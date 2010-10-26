using Mono.Cecil;

public static class CecilRocks
{
    public static bool IsVisible(this MethodReference self)
    {
        if (self == null)
            return false;

        MethodDefinition method = self.Resolve();
        if ((method == null) || method.IsPrivate || method.IsAssembly)
            return false;
        return self.DeclaringType.Resolve().IsVisible();
    }
    public static bool IsVisible(this TypeDefinition self)
    {
        if (self == null)
            return false;

        TypeDefinition type = self.Resolve();
        if (type == null)
            return true; // it's probably visible since we have a reference to it

        while (type.IsNested)
        {
            if (type.IsNestedPrivate || type.IsNestedAssembly)
                return false;
            // Nested classes are always inside the same assembly, so the cast is ok
            type = type.DeclaringType.Resolve();
        }
        return type.IsPublic;
    }
}