REM LineNo: 21
REM ExpectedError: BC30662
REM ErrorMessage: Attribute 'AuthorAttribute' cannot be applied to 'S1' because the attribute is not valid on this declaration type.

Imports System

'Declaring atrribute target to be class and using attribute on methods

<AttributeUsage(AttributeTargets.Class)> _
Public Class AuthorAttribute 
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
