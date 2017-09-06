Public Class Net

    Public Name As String
    Public Pads As New List(Of Pad)

    Public Sub New()
    End Sub


    ''' <summary>
    ''' Returns true if this net has multiple pads and these pads have different devicepins
    ''' This can be used to determine if wires have to be generated in the schematic
    ''' </summary>
    ''' <returns>false if this net has only 1 pad or multiple pads connected to the same devicepin</returns>
    ''' <remarks></remarks>
    Public Function HasMultipleDevicePins() As Boolean
        If Pads.Count > 0 Then
            Dim DevicePin As DevicePin = Pads(0).DevicePin
            Dim i As Integer
            While DevicePin Is Nothing AndAlso i < Pads.Count
                DevicePin = Pads(i).DevicePin
                i = i + 1
            End While
            If DevicePin Is Nothing Then Return False
            For Each Pad As Pad In Pads
                If Pad.DevicePin IsNot Nothing AndAlso Not Pad.DevicePin.Equals(DevicePin) Then Return True
            Next
        End If
        Return False
    End Function

    Public Sub join(ByVal Net As Net)
        Dim Pad As Pad
        For Each Pad In Net.Pads
            Pads.Add(Pad)
        Next
    End Sub

    Public Shared Function join(ByVal Net1 As Net, ByVal net2 As Net) As Net
        Dim Net As New Net
        Dim Pad As Pad
        For Each Pad In Net1.Pads
            Net.Pads.Add(Pad)
        Next
        For Each Pad In net2.Pads
            Net.Pads.Add(Pad)
        Next
        Return Net
    End Function

    Public Shared Operator +(ByVal Net1 As Net, ByVal Net2 As Net) As Net
        Return join(Net1, Net2)
    End Operator

End Class
