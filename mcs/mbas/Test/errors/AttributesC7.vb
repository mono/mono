REM LineNo: 7
REM ExpectedError: BC32035
REM ErrorMessage: Attribute specifier is not a complete statement. Use a line continuation to apply the attribute to the following statement.

Imports System

<AttributeUsage(AttributeTargets.All)> 
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
