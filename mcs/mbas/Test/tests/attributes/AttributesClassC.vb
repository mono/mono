'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell Inc. (http://www.novell.com)
'this checks for the AllowMultiple, if true <Author("A"), Author("B")> should not throw any error

Imports System

<AttributeUsage(AttributeTargets.Class, AllowMultiple := True)> _
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
            		Throw New Exception ("Class1 should get the Author Attributes. Lenght of the array should be 2 but got " & arr.Length)	
		End If
	End Sub
End Class
