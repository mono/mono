Imports System

Module ExceptionHandlingB
    Dim i As Integer
    Sub Main()

        Try

            Try
                i = 2 / i
                i = 3
                Console.WriteLine(i)
            Catch e As Exception
                Console.WriteLine(e.Message)
                ' Try statement wil not handle any exceptions thrown in Catch block
                Throw New Exception("FF")
            End Try ' inner try

            ' Catch exception thrown by inner Try statement 
        Catch e As Exception When e.Message = "FF"
            Console.WriteLine("OK")
        End Try  ' outer try
    End Sub

End Module

