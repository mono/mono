// CS3002: Return type of `I.Error()' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

[assembly:System.CLSCompliant(true)]

public interface I 
{
       ulong Error();
}
