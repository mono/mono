Imports System

<AttributeUsage(AttributeTargets.All, AllowMultiple:=True, Inherited:=True)> _
Public Class AuthorAttribute 
     Inherits Attribute
	Public Name
	Public Sub New(ByVal Name As String)
		Me.Name=Name
	End Sub	
	
End Class


<Author("Robin Cook"),Author("Authur Haily")> _
Public Interface C1
	
End Interface

Public Interface C2
     Inherits C1
		
End Interface

Module Test
	Sub Main()

	End Sub
End Module
