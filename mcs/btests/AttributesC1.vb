REM LineNo: 22
REM ExpectedError: BC30390
REM ErrorMessage: 'AuthorAttribute.Private Sub New(Name As String)' is not accessible in this context because it is 'Private'.

Imports System

<AttributeUsage(AttributeTargets.All)> _
Public Class AuthorAttribute 
     Inherits Attribute
	Public Name
	Private Sub New(ByVal Name As String)
		Me.Name=Name
	End Sub	
	Public ReadOnly Property NameP() As String
		Get
			Return Name
		End Get
	End Property
End Class


<Author("Robin Cook")> _
Public Class C1
	Public Sub S1()
	End Sub
End Class

Module Test
	Sub Main()

	End Sub
End Module
