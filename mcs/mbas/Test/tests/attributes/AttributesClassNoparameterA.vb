'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell Inc. (http://www.novell.com)
' Passing No parameter for Attributes
Imports System

<AttributeUsage(AttributeTargets.Class Or AttributeTargets.Interface)> _
Public Class SimpleAttribute
	Inherits System.Attribute

	Public Sub New()
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

<Simple> _
Class Class1
End Class

<Simple()> _
public Interface Interface1
End Interface
Module Test
		Sub Main()
		End Sub
End Module
