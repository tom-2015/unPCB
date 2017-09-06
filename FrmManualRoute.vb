Public Class FrmManualRoute

    Public WithEvents PCB As PCB

    Dim PadToCheck As Pad
    Dim CheckWithPad As Pad
    Dim Skipped As New List(Of Pad)

    Private Sub FrmAutoRoute_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        PCB.UnHighlightAllObjects()
    End Sub

    Public Sub New(ByVal PCB As PCB)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.PCB = PCB
    End Sub

    ''' <summary>
    ''' Goes to the next check, takes selected pad as to check with or if none selected, takes the first pad found
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub NextCheck()
        Dim SelectedPads As List(Of SelectableLayerObject)

        If CheckWithPad IsNot Nothing Then
            PCB.UnHighlightObject(CheckWithPad)
        End If

        If PadToCheck Is Nothing Then
            SelectedPads = PCB.GetSelectedLayerObjects(GetType(Pad))
            PCB.DeselectAllObjects()
            If SelectedPads.Count > 0 Then
                PadToCheck = CType(SelectedPads(0), Pad)
            Else
                PadToCheck = PCB.ConnectionMatrix.GetFirstUnconnectedPad()
            End If
        End If

        If PadToCheck IsNot Nothing Then
            CheckWithPad = PCB.ConnectionMatrix.GetNextToCheckPad(PadToCheck, True, Skipped)

            If CheckWithPad IsNot Nothing Then
                PCB.HighlightObject(CheckWithPad)
                PCB.HighlightObject(PadToCheck)
                lblCurrentPads.Text = PadToCheck.Name & " with " & CheckWithPad.Name
            Else
                PCB.UnHighlightObject(PadToCheck)
                PadToCheck = Nothing
                MsgBox("All connections tested for this pad, switching main pad!", MsgBoxStyle.Information)
                NextCheck() 'go to next padtocheck
            End If
        Else
            If Skipped.Count > 0 Then
                MsgBox("All connections are tested except for the skipped items, will now test the skipped items", MsgBoxStyle.Information)
                Skipped.Clear()
                NextCheck()
            Else
                PCB.UnHighlightAllObjects()
                MsgBox("Everything is connected!", MsgBoxStyle.Information)
            End If
        End If

        PgbCompleted.Minimum = 0
        PgbCompleted.Maximum = PCB.ConnectionMatrix.GetTotalConnections()
        PgbCompleted.Value = PgbCompleted.Maximum - PCB.ConnectionMatrix.GetUnknownConnections()

    End Sub

    Private Sub CmdConnected_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdConnected.Click
        If PadToCheck IsNot Nothing AndAlso CheckWithPad IsNot Nothing Then
            PCB.AddUndoItem(New UndoRedoConnect(PCB, True))
            PCB.ConnectionMatrix.ConnectPads(PadToCheck, CheckWithPad)
            NextCheck()
        Else
            MsgBox("Error no pads to check found", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub CmdNotConnected_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CmdNotConnected.Click
        If PadToCheck IsNot Nothing AndAlso CheckWithPad IsNot Nothing Then
            PCB.AddUndoItem(New UndoRedoConnect(PCB, True))
            PCB.ConnectionMatrix.NotConnectPads(PadToCheck, CheckWithPad)
            NextCheck()
        Else
            MsgBox("Error no pads to check found", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub CmdNotConnectedToAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdNotConnectedToAll.Click
        If PadToCheck IsNot Nothing Then
            PCB.AddUndoItem(New UndoRedoConnect(PCB, True))
            PCB.ConnectionMatrix.NotConnectToAllPads(PadToCheck)
            NextCheck()
        End If
    End Sub

    Private Sub CmdSkip_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CmdSkip.Click
        Skipped.Add(CheckWithPad)
        NextCheck()
    End Sub

    Private Sub CmdNext_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdNext.Click
        If FrmLayer.GetDrawstate() <> FrmLayer.DrawStates.AutoRouter Then
            FrmLayer.ResetDrawState()
            FrmLayer.SetDrawState(FrmLayer.DrawStates.AutoRouter) 'we go to autorouter mode
        End If
        NextCheck()
        Me.CmdConnected.Enabled = True
        Me.CmdNotConnectedToAll.Enabled = True
        Me.CmdNotConnected.Enabled = True
        Me.CmdSkip.Enabled = True
    End Sub

    Private Sub FrmAutoRoute_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ToolTip.SetToolTip(CmdNext, "Search and highlight pads to test.")
        ToolTip.SetToolTip(CmdNotConnected, "The highlighted pads are NOT connected.")
        ToolTip.SetToolTip(CmdSkip, "Skip this pad for now and go to the next one.")
        ToolTip.SetToolTip(CmdConnected, "The highlighted pads are connected.")
    End Sub

    Public Sub MustSearchNext()
        Me.CmdConnected.Enabled = False 'Objects were dehighlighted! we must search next first
        Me.CmdNotConnected.Enabled = False
        Me.CmdNotConnectedToAll.Enabled = False
        Me.CmdSkip.Enabled = False
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Sender"></param>
    ''' <param name="Objects"></param>
    ''' <remarks></remarks>
    Private Sub PCB_ObjectsDeHighlighted(ByVal Sender As PCB, ByVal Objects As System.Collections.Generic.List(Of SelectableLayerObject)) Handles PCB.ObjectsDeHighlighted
        If FrmLayer.GetDrawstate() <> FrmLayer.DrawStates.AutoRouter Then
            MustSearchNext()
        End If
    End Sub
End Class