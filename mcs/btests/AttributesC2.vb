REM LineNo: 19
REM ExpectedError: BC32035
REM ErrorMessage: Attribute specifier is not a complete statement. Use a line continuation to apply the attribute to the following statement.

Imports System

<AttributeUsage(AttributeTargets.All)> _
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

End Class

Module Test
	Sub Main()

		Dim type As Type=GetType(C1)
		Dim arr As Object()=type.GetCustomAttributes(GetType(AuthorAttribute),True)
		If arr.Length=0 Then
			Console.WriteLine("Class has no attributes")
		Else
			Dim aa As AuthorAttribute=CType(arr(0),AuthorAttribute)
			Console.WriteLine("Name:" & aa.Name)
		End If
	End Sub
End Module
