REM LineNo: 27
REM ExpectedError: BC30390
REM ErrorMessage: 'AuthorAttribute.Private Sub New(Name As String)' is not accessible in this context because it is 'Private'.

REM LineNo: 29
REM ExpectedError: BC30390
REM ErrorMessage: 'AuthorAttribute.Private Sub New(Name As String)' is not accessible in this context because it is 'Private'.

'Using private constructor in attributes
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
	<Author("John")> _
	Public Sub S1()
	End Sub
End Class

Module Test
	Sub Main()

	End Sub
End Module
