Imports System
Imports TestUtils

Enum MemberType 
            Lions
            Pythons
            Jackals
            Eagles
End Enum

Structure ClubMember
            Public Name  As String
            Public Age   As Integer
            Public Group As MemberType
End Structure

 
Module Test
            Sub Main()
                        Dim a As ClubMember

                        a.Name = "John"
                        a.Age = 13
                        a.Group = MemberType.Eagles
                        
                        Dim b As ClubMember = a 
                        b.Age = 17
                        Dim s As String = String.Format("Member {0} is {1} years old and belongs to the group of {2}", _
                                                              a.Name, a.Age,   a.Group)
                                                              
			Console.WriteLine(TestUtils.GenerateHash (s))
            End Sub
End Module

 
