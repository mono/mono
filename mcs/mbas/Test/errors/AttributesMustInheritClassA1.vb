REM LineNo: 25
REM ExpectedError: BC31506
REM ErrorMessage: 'NewAttribute' cannot be used as an attribute because it is declared 'MustInherit'.

Imports System

<AttributeUsage(AttributeTargets.All)> _
MustInherit Class NewAttribute
	Inherits Attribute

	Public Sub New(ByVal A As String)
		Me.A = A
	End Sub

	Public B As String
	Private A As String

	Public ReadOnly Property A1() As String
		Get
			Return A
		End Get
	End Property
End Class

<NewAttribute("hello")> _
public Class Class1
	shared Sub Main()
		Dim type As Type = GetType(Class1)
		Dim arr() As Object = _
                type.GetCustomAttributes(GetType(NewAttribute), True)
                If arr.Length <> 1 Then
        	    	Throw New Exception ("Class1 should get NewAttribute. Lenght of the array should not be 0 but got " & arr.Length)	
                End If
	End Sub
End Class
