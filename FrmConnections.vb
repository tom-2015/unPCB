Public Class FrmConnections

    Public WithEvents ConnectionMatrix As ConnectionMatrix
    Public WithEvents PCB As PCB


    Public Sub New(ByVal PCB As PCB)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.PCB = PCB
        Me.ConnectionMatrix = PCB.ConnectionMatrix
        RefreshData()
    End Sub

    Private Sub ConnectionMatrix_ConnectionsChanged(ByVal Sender As ConnectionMatrix, ByVal Connections As System.Collections.Generic.Dictionary(Of Pad, System.Collections.Generic.Dictionary(Of Pad, ConnectionMatrix.ConnectionTypes))) Handles ConnectionMatrix.ConnectionsChanged
        RefreshData()
    End Sub

    Public Sub RefreshData()
        Dim Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionMatrix.ConnectionTypes))
        Dim Col As KeyValuePair(Of Pad, ConnectionMatrix.ConnectionTypes)
        Dim Table As New DataTable
        Dim HeaderCellValues As New Collection

        DataGridView.ReadOnly = True
        'Table.Columns.Add(" ", GetType(String))
        For Each Row In ConnectionMatrix.Connections
            Table.Columns.Add(Row.Key.Name, GetType(String))
            HeaderCellValues.Add(Row.Key.Name)
        Next

        For Each Row In ConnectionMatrix.Connections
            Dim DataRow As DataRow = Table.NewRow()
            For Each Col In Row.Value
                If Col.Value = unPCB.ConnectionMatrix.ConnectionTypes.ConnectionTypeUnknown Then
                    DataRow(Col.Key.Name) = "?"
                Else
                    DataRow(Col.Key.Name) = CInt(Col.Value)
                End If
            Next
            Table.Rows.Add(DataRow)
        Next

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
End Class