REM LineNo: 8
REM ExpectedError: BC32015
REM ErrorMessage: Assembly or Module expected.

Imports System
Imports System.Reflection

<Assembly:AssemblyVersionAttribute("1.0"), AssemblyCulture("")>

<AttributeUsage(AttributeTargets.All)> _
Public Class AuthorAttribute 
     Inherits Attribute
	Public Name
	Public Sub New(ByVal Name As String)
		Me.Name=Name
	End Sub	
End Class

Module Test
	Sub Main()

	End Sub
End Module
