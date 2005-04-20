REM LineNo: 9
REM ExpectedError: BC30129 
REM ErrorMessage: Assembly attribute 'System.Reflection.AssemblyCultureAttribute
'is not valid: Executables cannot be localized, Culture should always be empty


Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices

<assembly: AssemblyCulture("de")> 

Module Test
Sub Main()
 	
	dim asm as System.Reflection.Assembly
	dim i as integer
	asm = System.Reflection.Assembly.GetAssembly (GetType (Test))
	dim att as object () = asm.GetCustomAttributes (true)
	for i=0 to att.Length - 1
		Console.WriteLine ("Arribute = {1}", i, att (i).ToString ())
	next i
End Sub
End Module
