Imports System
'Testing 'Inherited' in attributes
<AttributeUsage(AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)> _
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


<Author("Robin Cook"),Author("Authur Haily")> _
Public Class C1
	
	Public Sub S1()
	End Sub

End Class

Public Class C2
     Inherits C1
		
End Class

Module Test
	Sub Main()

	End Sub
End Module
