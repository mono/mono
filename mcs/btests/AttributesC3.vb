REM LineNo: 26
REM ExpectedError: BC30644
REM ErrorMessage: Attribute cannot be used on 'C1'.

REM LineNo: 26
REM ExpectedError: BC31506
REM ErrorMessage: 'AuthorAttribute' cannot be used as an attribute because it is declared 'MustInherit'.

REM LineNo: 28
REM ExpectedError: BC30644
REM ErrorMessage: Attribute cannot be used on 'S1'.

'Using MustInherit on a attribute class
Imports System

<AttributeUsage(AttributeTargets.All)> _
Public MustInherit Class AuthorAttribute 
     Inherits Attribute
	Public Name
	Public Sub New(ByVal Name As String)
		Me.Name=Name
	End Sub	
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
