// Compiler options: -t:library

using System;

public static class Factory<BaseType> where BaseType : class
{
    public static BaseType CreateInstance (params object[] args)
    {
        return null;
    }
}
