Module ExpConversionofBoolToShort
        Sub Main()
                Dim a as Boolean = True
                Dim b as Short = CShort(a)
                if b <> -1 then
                        Throw New System.Exception("Explicit Conversion of Bool(False) to Short has Failed. Expected -1, but got " & b)
                End if
        End Sub
End Module
