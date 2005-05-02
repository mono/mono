'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices

<Assembly: AssemblyCulture(""), Assembly: AssemblyVersion("1.2.3.4")>

Module Test
Sub Main()
 	
	dim asm as System.Reflection.AssemblyName
	dim i as integer
	asm = System.Reflection.Assembly.GetCallingAssembly ().GetName ()
	if asm.toString() <> "AttributeAssemblyName, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null"			
		throw new System.Exception("#A1 Atributes not working")
	End If		
End Sub
End Module