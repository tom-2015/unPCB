Public Class FrmConnections

    Public WithEvents ConnectionMatrix As ConnectionMatrix
    Public WithEvents PCB As PCB

    Dim SkipUpdateLargeData As Boolean

    Public Sub New(ByVal PCB As PCB)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.PCB = PCB
        Me.ConnectionMatrix = PCB.ConnectionMatrix
        RefreshData()
    End Sub

    Private Sub ConnectionMatrix_ConnectionsChanged(ByVal Sender As ConnectionMatrix) Handles ConnectionMatrix.ConnectionsChanged
        RefreshData()
    End Sub

    Public Sub RefreshData()
        'Dim Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionMatrix.ConnectionTypes))
        'Dim Col As KeyValuePair(Of Pad, ConnectionMatrix.ConnectionTypes)
        Dim Table As New DataTable
        Dim HeaderCellValues As New Collection

        DataGridView.ReadOnly = True

        If SkipUpdateLargeData Then Exit Sub

        If ConnectionMatrix.ConnectionCount > 1000 Then
            If MsgBox("There are more than 10000 connections in this table, loading this will slow down you computer! Do you want to continue?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                SkipUpdateLargeData = True
                Exit Sub
            End If
        End If

        Dim TotalRows As Integer = ConnectionMatrix.ConnectionCount

        For Row As Integer = 0 To TotalRows - 1
            If ConnectionMatrix.GetIndexUsed(Row) Then
                Dim Pad As Pad = ConnectionMatrix.GetPad(Row)
                Table.Columns.Add(Pad.Name, GetType(String))
                HeaderCellValues.Add(Pad.Name)
            End If
        Next

        For Row As Integer = 0 To TotalRows - 1
            If ConnectionMatrix.GetIndexUsed(Row) Then
                Dim DataRow As DataRow = Table.NewRow()
                Dim ColIndex As Integer = 0
                For Col As Integer = 0 To TotalRows - 1
                    If ConnectionMatrix.GetIndexUsed(Col) Then
                        Dim Connection As ConnectionState = ConnectionMatrix.Connection(Row, Col)
                        If Connection = unPCB.ConnectionMatrix.ConnectionTypes.ConnectionTypeUnknown Then
                            DataRow(ColIndex) = "?"
                        Else
                            DataRow(ColIndex) = CInt(Connection)
                        End If
                        ColIndex += 1
                    End If
                Next
                Table.Rows.Add(DataRow)
            End If
        Next


        'Table.Columns.Add(" ", GetType(String))
        'For Each Row In ConnectionMatrix.Connections
        '    Table.Columns.Add(Row.Key.Name, GetType(String))
        '    HeaderCellValues.Add(Row.Key.Name)
        'Next

        'For Each Row In ConnectionMatrix.Connections
        '    Dim DataRow As DataRow = Table.NewRow()
        '    For Each Col In Row.Value
        '        If Col.Value = unPCB.ConnectionMatrix.ConnectionTypes.ConnectionTypeUnknown Then
        '            DataRow(Col.Key.Name) = "?"
        '        Else
        '            DataRow(Col.Key.Name) = CInt(Col.Value)
        '        End If
        '    Next
        '    Table.Rows.Add(DataRow)
        'Next

        DataGridView.DataSource = Table
        For i As Integer = 0 To Table.Rows.Count - 1
            DataGridView.Rows(i).HeaderCell.Value = HeaderCellValues(i + 1)
        Next

    End Sub

    Private Sub FrmConnections_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        ConnectionMatrix = Nothing
        PCB = Nothing
    End Sub

    Private Sub FrmConnections_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        RefreshData()
    End Sub

    Private Sub PCB_DeviceNameChangedEvent(ByVal Sender As PCB, ByVal Device As Device, ByVal OldName As String, ByVal NewName As String) Handles PCB.DeviceNameChangedEvent
        RefreshData()
    End Sub

    Private Sub PCB_ObjectNameChangedEvent(ByVal Sender As PCB, ByVal LayerObject As LayerObject, ByVal OldName As String, ByVal NewName As String) Handles PCB.ObjectNameChangedEvent
        If TypeOf (LayerObject) Is Pad Then
            RefreshData()
        End If
    End Sub
End Class