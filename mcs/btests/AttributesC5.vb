REM LineNo: 22
REM ExpectedError: BC30663
REM ErrorMessage: Attribute 'AuthorAttribute' cannot be applied multiple times.

Imports System
'Using a single-use attribute multiple times
<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)> _
Public Class AuthorAttribute 
     Inherits Attribute
	Public Name
	Public Sub New(ByVal Name As String)
		Me.Name=Name
	End Sub	
	Public ReadOnly Property NameP() As String
		Get
			Return Name
		End Get
	End Property
End Class


<Author("Robin Cook"),Author("John Gresham")> _
Public Class C1
	
	Public Sub S1()
	End Sub

End Class

Module Test
	Sub Main()

	End Sub
End Module
