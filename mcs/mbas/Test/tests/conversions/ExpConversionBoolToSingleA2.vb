Module ExpConversionofBoolToSingle
        Sub Main()
                Dim a as Boolean = True
                Dim b as Single = CSng(a)
                if b <> -1 then
                        Throw New System.Exception("Explicit Conversion of Bool(False) to Single has Failed. Expected -1, but got " & b)
                End if
        End Sub
End Module
