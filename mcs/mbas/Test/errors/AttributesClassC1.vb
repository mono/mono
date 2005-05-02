REM LineNo: 20
REM ExpectedError: BC30663
REM ErrorMessage: Attribute 'AuthorAttribute' cannot be applied multiple times.

Imports System

<AttributeUsage(AttributeTargets.Class, AllowMultiple := False)> _
Public Class AuthorAttribute
	Inherits System.Attribute

	Public Sub New(ByVal Value As String)
	End Sub

	Public ReadOnly Property Value() As String
        	Get
	        End Get
	End Property
End Class

<Author("A"), Author("B")> _
Public Class Class1
	shared Sub Main()
		Dim type As Type = GetType(Class1)
		Dim arr() As Object = _
                type.GetCustomAttributes(GetType(AuthorAttribute), True)
	        If arr.Length <> 2 Then
            		Throw New Exception ("Class1 should get NewAttribute. Lenght of the array should not be 0 but got " & arr.Length)	
                End If
	End Sub
End Class
