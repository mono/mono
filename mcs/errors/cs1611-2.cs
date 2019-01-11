// CS1611: The params parameter cannot be declared as ref, out or in
// Line: 6
// Compiler options: -langversion:latest

class Test
{
    public static void Error (params in int args) {}
}