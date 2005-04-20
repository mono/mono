'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices

<assembly: AssemblyDescription("Mono VB Compiler")> 

Module Test
Sub Main()
 	
	dim asm as System.Reflection.Assembly
	dim i as integer
	asm = System.Reflection.Assembly.GetAssembly (GetType (Test))
	dim att as object () = asm.GetCustomAttributes (false)
	for i=0 to att.Length - 1
		if att (i).ToString () <> "System.Reflection.AssemblyDescriptionAttribute" Then
			Throw New exception ("Expected System.Reflection.AssemblyDescriptionAttribute")
		End If
	next i
End Sub
End Module

