Imports System

'Using byref positional parameters

<AttributeUsage(AttributeTargets.All)> _
Public Class AuthorAttribute 
     Inherits Attribute
	Public Name
	Public Sub New(ByRef Name As String)
		Me.Name=Name
	End Sub	
End Class


<Author("Robin Cook")> _
Public Class C1
	
End Class

Module Test
	Sub Main()

	End Sub
End Module
