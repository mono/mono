Imports System
'Ommiting AttributeUsage attribute

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
