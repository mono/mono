REM LineNo: 26
REM ExpectedError: BC30002
REM ErrorMessage: Type 'C2' is not defined.

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
Public Class C1
	
End Class

Module Test
	Sub Main()

		Dim type As Type=GetType(C2)
		Dim arr As Object()=type.GetCustomAttributes(GetType(AuthorAttribute),True)
		If arr.Length=0 Then
			Console.WriteLine("Interface has no attributes")
		Else
			Throw New Exception("AttributesC:Failed-Interfaces should not inherit attributes")
		End If
	End Sub
End Module
