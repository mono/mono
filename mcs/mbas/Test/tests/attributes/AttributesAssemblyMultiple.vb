'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Imports System.Reflection

<Assembly:AssemblyVersionAttribute("1.0"), Assembly: AssemblyCulture("")>

<AttributeUsage(AttributeTargets.All)> _
Public Class AuthorAttribute 
     Inherits Attribute
	Public Name
	Public Sub New1(ByVal Name As String)
		Me.Name=Name
		If Me.Name <> "a" then 
			Throw New Exception ("Expected Me.Name to be a but got, ", Me.Name)
		End if	
	End Sub	
End Class

Module Test
	Sub Main()
		Dim a as AuthorAttribute = New AuthorAttribute ()
		a.New1 ("a")
	End Sub
End Module
