REM LineNo: 9
REM ExpectedError: BC30548 
REM ErrorMessage: Attribute 'FlagsAttribute' cannot be applied to an assembly. <assembly: Flags>

Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices

<assembly: Flags> 

Module Test
Sub Main()
 	
	dim asm as System.Reflection.Assembly
	dim i as integer
	asm = System.Reflection.Assembly.GetAssembly (GetType (Test))
	dim att as object () = asm.GetCustomAttributes (false)
	for i=0 to att.Length - 1
		Console.WriteLine ("Arribute = {1}", i, att (i).ToString ())
	next i
End Sub
End Module

