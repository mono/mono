Imports System
Imports System.Reflection
Imports TestUtils

Namespace Pippo

<AttributeUsage(AttributeTargets.All, Inherited:=True, AllowMultiple:=True)> _
Public Class Annotation
    Inherits System.Attribute

    Protected strAuthor As String
    Protected strComment As String

    Public Sub New(ByVal Author As String, ByVal Comment As String)
        strAuthor = Author
        strComment = Comment
    End Sub

    Public Property Author() As String
        Get
            Author = strAuthor
        End Get
        
        Set(Value As String)
            strAuthor = CStr(Value)
        End Set
    End Property

    Public Property Comment() As String
        Get
            Return strComment
        End Get
        Set(Value As String)
            strComment = CStr(Value)
        End Set
    End Property

End Class

<Annotation("mr-", "AttributeTest")> _
Public Class TestClass
	Public Sub New(a As integer,b As integer,c As integer)
	
	End Sub
	
	Public Sub SayWhoYouAre()

	End Sub
End Class

Public Class TestClass2
	Public Sub New(a As integer,b As integer,c As integer)
	
	End Sub
	
	Public Sub SayWhoYouAre()

	End Sub
End Class

Module Test

Dim tc As TestClass

Public Sub Main()
	Dim tc_type As Type
	Dim obj(1) As Object
	Dim MyAnnotation As Annotation

	tc = New TestClass(2,3,4)
	tc_type = tc.GetType()
	obj = tc_type.GetCustomAttributes(False)
	MyAnnotation = CType(obj(0), Annotation)
	
	Console.WriteLine(TestUtils.GenerateHash(MyAnnotation.Author & MyAnnotation.Comment))
End Sub

End Module

End Namespace
