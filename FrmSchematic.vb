Public Class FrmSchematic

    Protected WithEvents m_PCB As PCB
    Protected WithEvents m_Schematic As PCBSchematic

    Public Enum SchematicDrawStates As Integer
        DrawStateSelectObjects
        DrawStateWindowSelectObjects
        DrawStateMoveObjects
    End Enum

    Private Structure ClickPoint
        Dim Location As PointF
    End Structure

    Dim MovePoint As ClickPoint 'last mouse move position
    Dim LeftClickPoint As ClickPoint 'the last point left clicked = mouse down event
    Dim RightClickPoint As ClickPoint 'the last point right clicked
    Dim MiddleClickPoint As ClickPoint 'the last point middle clicked 
    Dim m_TranslateX As Single
    Dim m_TranslateY As Single
    Dim m_Scale As Double
    Dim m_StartLocation As PointF
    Dim m_DrawState As SchematicDrawStates = SchematicDrawStates.DrawStateSelectObjects
    Dim m_MainWindow As FrmMain

    Dim TranslateStartX As Single, TranslateStartY As Single 'used for moving the pcb on the screen with middle mouse button

    Public Sub UpdateGraphics()
        DoubleBuffer.Refresh()
    End Sub

    Private Sub DoubleBuffer_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles DoubleBuffer.KeyUp
        If e.KeyCode = Keys.Escape Then
            Select Case m_DrawState
                Case SchematicDrawStates.DrawStateMoveObjects
                    For Each SelectedItem As SchematicInstance In m_Schematic.SelectedInstances
                        SelectedItem.Location = SelectedItem.StartLocation
                    Next
                    m_Schematic.DeselectAllInstances()
                    m_DrawState = SchematicDrawStates.DrawStateSelectObjects
            End Select
        End If
    End Sub

    Private Sub DoubleBuffer_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DoubleBuffer.MouseDown
        Select Case e.Button
            Case MouseButtons.Left
                LeftClickPoint.Location = e.Location
            Case MouseButtons.Middle
                TranslateStartX = m_TranslateX
                TranslateStartY = m_TranslateY
                MiddleClickPoint.Location = e.Location
            Case MouseButtons.Right
                RightClickPoint.Location = e.Location
        End Select
    End Sub


    Private Sub DoubleBuffer_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DoubleBuffer.MouseMove
        MovePoint.Location = e.Location

        If e.Button = MouseButtons.Middle Then
            m_TranslateX = TranslateStartX + ConvertX(e.X) - ConvertX(MiddleClickPoint.Location.X)
            m_TranslateY = TranslateStartY + -(ConvertY(e.Y) - ConvertY(MiddleClickPoint.Location.Y))
            UpdateGraphics()
        End If

        Select Case m_DrawState
            Case SchematicDrawStates.DrawStateMoveObjects
                If m_Schematic.SelectedInstances.Count > 0 Then
                    Dim Move As PointF = New PointF(ConvertX(e.X) - m_StartLocation.X, ConvertY(e.Y) - m_StartLocation.Y)
                    For Each Instance As SchematicInstance In m_Schematic.SelectedInstances
                        Instance.Location = m_Schematic.Grid.RoundLocation(New PointF(Instance.StartLocation.X + Move.X, Instance.StartLocation.Y + Move.Y))
                    Next 'm_Schematic.Grid.RoundLocation(ConvertLocation(e.Location)) '
                Else
                    m_DrawState = SchematicDrawStates.DrawStateSelectObjects
                End If
                UpdateGraphics()
            Case SchematicDrawStates.DrawStateSelectObjects
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    If Distance(LeftClickPoint.Location, e.Location) > 10 Then 'move than 10 pixels, we start to draw a window
                        m_DrawState = SchematicDrawStates.DrawStateWindowSelectObjects
                    End If
                End If
            Case SchematicDrawStates.DrawStateWindowSelectObjects
                UpdateGraphics()
        End Select

        StatusBar.Items(0).Text = ConvertLocation(e.Location).ToString
    End Sub

    Private Sub DoubleBuffer_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DoubleBuffer.MouseUp
        If e.Button = MouseButtons.Middle Then
            m_TranslateX = TranslateStartX + ConvertX(e.X) - ConvertX(MiddleClickPoint.Location.X)
            m_TranslateY = TranslateStartY + -(ConvertY(e.Y) - ConvertY(MiddleClickPoint.Location.Y))
        End If
        Select Case m_DrawState
            Case SchematicDrawStates.DrawStateMoveObjects
                If e.Button = Windows.Forms.MouseButtons.Right Then
                    For Each SelectedItem As SchematicInstance In m_Schematic.SelectedInstances
                        SelectedItem.Rotation = (SelectedItem.Rotation + 90) Mod 360
                    Next
                Else
                    For Each SelectedItem As SchematicInstance In m_Schematic.SelectedInstances
                        m_PCB.AddUndoItem(New UndoRedoMoveSchematicInstance(m_PCB, SelectedItem))
                    Next
                    m_DrawState = SchematicDrawStates.DrawStateSelectObjects
                    m_Schematic.DeselectAllInstances()
                End If
            Case SchematicDrawStates.DrawStateSelectObjects
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    Dim ClosestInstance As SchematicInstance = m_Schematic.GetClosestItem(ConvertLocation(e.Location))
                    If ClosestInstance IsNot Nothing Then
                        m_Schematic.SelectInstance(ClosestInstance)
                        ClosestInstance.StartLocation = ClosestInstance.Location
                        ClosestInstance.StartRotation = ClosestInstance.Rotation
                        If Not My.Computer.Keyboard.CtrlKeyDown Then
                            m_StartLocation = ConvertLocation(e.Location)
                            m_DrawState = SchematicDrawStates.DrawStateMoveObjects
                        End If
                    End If
                End If
            Case SchematicDrawStates.DrawStateWindowSelectObjects
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    'rect is a rectangle on the schematic, check which items are in this rectangle and select them
                    Dim Rect As New Rectangle(Math.Min(ConvertX(LeftClickPoint.Location.X), ConvertX(MovePoint.Location.X)), Math.Min(ConvertY(LeftClickPoint.Location.Y), ConvertY(MovePoint.Location.Y)), Math.Abs(ConvertX(LeftClickPoint.Location.X) - ConvertX(MovePoint.Location.X)), Math.Abs(ConvertY(LeftClickPoint.Location.Y) - ConvertY(MovePoint.Location.Y)))
                    If Not My.Computer.Keyboard.CtrlKeyDown Then
                        m_Schematic.DeselectAllInstances()
                    End If
                    For Each SchematicInstance As SchematicInstance In m_Schematic.Instances
                        If SchematicInstance.Location.X >= Rect.X AndAlso SchematicInstance.Location.X <= Rect.X + Rect.Width Then
                            If SchematicInstance.Location.Y >= Rect.Y AndAlso SchematicInstance.Location.Y <= Rect.Y + Rect.Height Then
                                m_Schematic.SelectInstance(SchematicInstance)
                                SchematicInstance.StartLocation = SchematicInstance.Location
                                SchematicInstance.StartRotation = SchematicInstance.Rotation
                            End If
                        End If
                    Next
                    If Not My.Computer.Keyboard.CtrlKeyDown Then
                        If m_Schematic.SelectedInstances.Count > 0 Then
                            m_StartLocation = ConvertLocation(e.Location)
                            m_DrawState = SchematicDrawStates.DrawStateMoveObjects
                        Else
                            m_DrawState = SchematicDrawStates.DrawStateSelectObjects
                        End If
                    End If
                End If
        End Select

        UpdateGraphics()
    End Sub

    Private Sub DoubleBuffer_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles DoubleBuffer.Paint
        Dim Graphics As Graphics = e.Graphics
        Graphics.Clear(Color.White)
        Graphics.ResetTransform()
        Graphics.ScaleTransform(m_Scale, m_Scale)
        Graphics.TranslateTransform(m_TranslateX, m_TranslateY)
        Dim Pen As New Pen(Color.Gray, 1 / m_Scale)
        Graphics.DrawLine(Pen, -10, 0, 10, 0)
        Graphics.DrawLine(Pen, 0, 10, 0, -10)
        For Each SchematicInstance As SchematicInstance In m_Schematic.Instances
            Dim Container As Drawing2D.GraphicsContainer = Graphics.BeginContainer()
            Graphics.TranslateTransform(SchematicInstance.Location.X, -SchematicInstance.Location.Y)
            Graphics.ScaleTransform(1, -1)
            SchematicInstance.Render(Graphics)
            'Gate.GetSymbol().Render(e.Graphics)
            Graphics.EndContainer(Container)
        Next

        Graphics.ResetTransform()
        Select Case m_DrawState
            Case SchematicDrawStates.DrawStateWindowSelectObjects
                Dim Rect As New Rectangle(Math.Min(LeftClickPoint.Location.X, MovePoint.Location.X), Math.Min(LeftClickPoint.Location.Y, MovePoint.Location.Y), Math.Abs(LeftClickPoint.Location.X - MovePoint.Location.X), Math.Abs(LeftClickPoint.Location.Y - MovePoint.Location.Y))
                Graphics.DrawRectangle(New Pen(Color.Gray, 1), Rect)
                Graphics.FillRectangle(New SolidBrush(Color.FromArgb(&HB4808080)), Rect)
        End Select

    End Sub

    Public Sub New(ByVal PCB As PCB, ByVal MainWindow As FrmMain)

        m_PCB = PCB
        m_Schematic = PCB.Schematic
        m_Scale = 1
        m_MainWindow = MainWindow
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Private Sub FrmSchematic_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        With DoubleBuffer.CreateGraphics
            m_TranslateX = .VisibleClipBounds.Width / 2
            m_TranslateY = .VisibleClipBounds.Height / 2
        End With
    End Sub

    Private Sub DoubleBuffer_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DoubleBuffer.MouseWheel
        Dim xBefore As Integer = ConvertX(e.X)
        Dim yBefore As Integer = ConvertY(e.Y)
        Dim xAfter As Integer
        Dim yAfter As Integer

        If e.Delta > 0 Then
            m_Scale = m_Scale / 0.85
        ElseIf e.Delta < 0 Then
            m_Scale = m_Scale * 0.85
        End If

        xAfter = ConvertX(e.X)
        yAfter = ConvertY(e.Y)

        m_TranslateX += -(xBefore - xAfter)
        m_TranslateY += (yBefore - yAfter)

        UpdateGraphics()
    End Sub

    ''' <summary>
    ''' Converts location X on the screen (window pixels) to X location on the schematic
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvertX(ByVal x As Single) As Single
        'If PCB.VerticalMirror(WindowType) Then
        '    Return x / -m_Scale + m_Width - m_TranslateX
        'Else
        Return (x / m_Scale - m_TranslateX)
        'End If
    End Function

    ''' <summary>
    ''' Converts location Y on the screen (window pixels) to Y location on the schematic
    ''' </summary>
    ''' <param name="y"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvertY(ByVal y As Single) As Single
        'If PCB.HorizontalMirror(WindowType) Then
        'Return y / -m_Scale - m_TranslateY
        'Else
        Return -(y / m_Scale - m_TranslateY)
        'End If
    End Function

    ''' <summary>
    ''' Converts a point on the screen to location on the schematic
    ''' </summary>
    ''' <param name="Location"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvertLocation(ByVal Location As PointF) As PointF
        Return New PointF(ConvertX(Location.X), ConvertY(Location.Y))
    End Function

    Private Sub m_PCB_DeviceAdded(ByVal Sender As PCB, ByVal Device As Device) Handles m_PCB.DeviceAdded
        UpdateGraphics()
    End Sub

    Private Sub m_PCB_UndoRedoAction(ByVal Sender As PCB, ByVal UndoRedoItem As UndoRedoItem, ByVal Undo As Boolean) Handles m_PCB.UndoRedoAction
        UpdateGraphics()
    End Sub

    Private Sub EagleSchematicToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EagleSchematicToolStripMenuItem.Click
        m_MainWindow.ExportEagleSchematic()
    End Sub

    Private Sub DoubleBuffer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DoubleBuffer.Load

    End Sub
End Class