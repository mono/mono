'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices

<assembly: AssemblyCompany ("Novell, Inc.")>

Module Test
Sub Main()
 	
	dim asm as System.Reflection.Assembly
	dim i as integer
	asm = System.Reflection.Assembly.GetAssembly (GetType (Test))
	dim att as object () = asm.GetCustomAttributes (false)
	for i=0 to att.Length - 1
	If att (i).ToString () <> "System.Reflection.AssemblyCompanyAttribute" Then 
	    Throw New System.Exception ("AssemblyCompany Attribute was not set properly expected SystemAssemblyCompany but got", att (i))
	End If
	next i
End Sub
End Module

