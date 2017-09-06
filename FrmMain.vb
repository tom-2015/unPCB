Public Class FrmMain

    Public Shared ProgramSettings As Xml.XmlDocument

    Dim WithEvents PCB As PCB
    Dim WithEvents DevicesWindow As FrmDevices
    Dim WithEvents ConnectionsWindow As FrmConnections
    Dim WithEvents TopWindow As FrmLayer 'top
    Dim WithEvents BottomWindow As FrmLayer 'bottom
    Dim WithEvents SchematicWindow As FrmSchematic


    Dim NextAutoSaveTime As Integer

    Public Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        EndProgram()
    End Sub

    Private Sub FrmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If CloseProject() Then
            SaveSettings()
            End
        Else
            e.Cancel = True
        End If
    End Sub

    Private Sub FrmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Show()
        If Command() <> "" Then
            Dim File As String = Replace(Command(), """", "")
            If System.IO.File.Exists(File) Then
                PCB = New PCB()
                PCB.Open(File)
                UpdateAutoSaveTime()
            End If
        Else
            FrmNew.ShowDialog(Me)
        End If
    End Sub

    Private Sub OpenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripMenuItem.Click
        OpenProject()
    End Sub

    Private Sub SaveToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveToolStripMenuItem.Click
        SaveProject()
    End Sub

    Private Sub ToolStripMenuNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuNew.Click
        NewProject()
    End Sub


    ''' <summary>
    ''' Shows dialog to create a new project
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub NewProject()
        If CloseProject() Then
            FrmNew.ShowDialog()
        End If
    End Sub

    ''' <summary>
    ''' Asks to save the changes and ends the program
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub EndProgram()
        If CloseProject() Then
            End
        End If
    End Sub

    ''' <summary>
    ''' Closes the current project and all windows, asks to save any new changes
    ''' </summary>
    ''' <returns>True if the project was closed</returns>
    ''' <remarks></remarks>
    Public Function CloseProject() As Boolean
        If PCB IsNot Nothing AndAlso PCB.IsChanged Then
            Select Case MsgBox("Do you want to save the changes?", MsgBoxStyle.Question Or MsgBoxStyle.YesNoCancel)
                Case MsgBoxResult.Yes
                    If PCB.FileName = "" Then
                        Me.SaveToolStripMenuItem_Click(Me, New System.EventArgs())
                    End If
                Case MsgBoxResult.Cancel
                    Return False
            End Select
        End If
        If ConnectionsWindow IsNot Nothing Then ConnectionsWindow.Close()
        If DevicesWindow IsNot Nothing Then DevicesWindow.Close()
        If TopWindow IsNot Nothing Then TopWindow.Close()
        If BottomWindow IsNot Nothing Then BottomWindow.Close()
        PCB = Nothing
        Return True
    End Function

    ''' <summary>
    ''' Saves the project, asks for filename if not yet saved
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SaveProject()

        If Not PCB Is Nothing Then
            If PCB.FileName = "" Then
                SaveProjectAs()
            Else
                'Try
                PCB.Save(PCB.FileName)
                'Catch Ex As Exception
                'MsgBox("Error saving " & Ex.Message, MsgBoxStyle.Critical)
                'End Try
            End If
        End If

    End Sub

    ''' <summary>
    ''' Saves project as
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SaveProjectAs()
        SaveFileDialog.Filter = "unPCB files (*.upcb)|*.upcb|All files (*.*)|*.*"
        SaveFileDialog.OverwritePrompt = True
        If SaveFileDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Try
                PCB.Save(SaveFileDialog.FileName)
            Catch Ex As Exception
                MsgBox("Error saving " & Ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Closes the current project, Shows dialog and Opens a project
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub OpenProject()
        CloseProject()
        OpenFileDialog.Filter = "unPCB files (*.upcb)|*.upcb|All files (*.*)|*.*"
        OpenFileDialog.Multiselect = False
        If OpenFileDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
            PCB = New PCB()
            PCB.Open(OpenFileDialog.FileName)
            UpdateAutoSaveTime()
        End If
    End Sub

    ''' <summary>
    ''' Automatically save a backup copy of the project
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub AutoSaveProject()
        Try
            If PCB IsNot Nothing Then
                Dim File As String = Application.StartupPath & System.IO.Path.DirectorySeparatorChar & PCB.Name & " - " & Year(Now()) & "-" & Month(Now()) & "-" & Now().Day() & " " & Hour(Now()) & "-" & Minute(Now()) & "-" & Second(Now()) & ".upcbb"
                If PCB.FileName <> "" Then
                    File = PCB.FileName & " - " & Year(Now()) & "-" & Month(Now()) & "-" & Now().Day() & " " & Hour(Now()) & "-" & Minute(Now()) & "-" & Second(Now()) & ".upcbb"
                End If
                PCB.Save(File, True)
            End If
        Catch Ex As Exception
            MsgBox("Error in auto saving " & Ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    ''' <summary>
    ''' Creates a new unpcb project
    ''' </summary>
    ''' <param name="Name">Poject Name</param>
    ''' <param name="TopLayer">Top layer Image location</param>
    ''' <param name="BottomLayer">Bottom layer image location</param>
    ''' <remarks></remarks>
    Public Sub CreateNewProject(ByVal Name As String, ByVal TopLayer As String, ByVal BottomLayer As String, ByVal MirrorTopLayerHorizontally As Boolean, ByVal MirrorTopLayerVertically As Boolean, ByVal MirrorBottomLayerHorizontally As Boolean, ByVal MirrorBottomLayerVertically As Boolean)
        PCB = New PCB()

        If System.IO.File.Exists(TopLayer) Then
            Dim Top As New PCBImage(New Point(0, 0), TopLayer)
            PCB.PlaceObject(Top, unPCB.PCB.WindowTypes.WindowTypeTop, New Point(0, 0))
            PCB.Width = Top.Width
            PCB.Height = Top.Height
        End If

        If System.IO.File.Exists(BottomLayer) Then
            Dim Bottom As New PCBImage(New Point(0, 0), BottomLayer)
            PCB.PlaceObject(Bottom, unPCB.PCB.WindowTypes.WindowTypeBottom, New Point(0, 0))
            PCB.Width = Math.Max(PCB.Width, Bottom.Width)
            PCB.Height = Math.Max(PCB.Height, Bottom.Height)
        End If


        'PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeBottomImage).LayerObjects.Add(Bottom)
        'PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeTopImage).LayerObjects.Add(Top)

        'PCB.Width = Math.Max(Top.Width, Bottom.Width)
        'PCB.Height = Math.Max(Bottom.Height, Top.Height)
        PCB.HorizontalMirror(unPCB.PCB.WindowTypes.WindowTypeTop) = MirrorTopLayerHorizontally
        PCB.VerticalMirror(unPCB.PCB.WindowTypes.WindowTypeTop) = MirrorTopLayerVertically
        PCB.HorizontalMirror(unPCB.PCB.WindowTypes.WindowTypeBottom) = MirrorBottomLayerHorizontally
        PCB.VerticalMirror(unPCB.PCB.WindowTypes.WindowTypeBottom) = MirrorBottomLayerVertically
        PCB.Name = Name

        Dim LibraryNodes As Xml.XmlNodeList = ProgramSettings.SelectNodes("unPCB/libraries/library")
        For Each LibraryNode As Xml.XmlNode In LibraryNodes
            PCB.LoadEagleLibrary(LibraryNode.InnerText)
        Next

        ShowTopLayerWindow()
        ShowBottomLayerWindow()

        UpdateAutoSaveTime()
    End Sub

    Private Sub ConnectionsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConnectionsToolStripMenuItem.Click
        ShowConnectionsWindow()
    End Sub

    Public Sub ShowConnectionsWindow()
        If PCB IsNot Nothing Then
            If ConnectionsWindow Is Nothing Then ConnectionsWindow = New FrmConnections(PCB)
            ConnectionsWindow.MdiParent = Me
            ConnectionsWindow.Show()
            ConnectionsToolStripMenuItem.Checked = True
        End If
    End Sub

    Private Sub DevicesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DevicesToolStripMenuItem.Click
        ShowDevicesWindow()
    End Sub

    Public Sub ShowDevicesWindow()
        If PCB IsNot Nothing Then
            If DevicesWindow Is Nothing Then DevicesWindow = New FrmDevices(PCB)
            DevicesWindow.MdiParent = Me
            DevicesWindow.Show()
            DevicesToolStripMenuItem.Checked = True
        End If
    End Sub

    Private Sub TopLayerWindowToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TopLayerWindowToolStripMenuItem.Click
        ShowTopLayerWindow()
    End Sub

    Public Sub ShowTopLayerWindow()
        If PCB IsNot Nothing Then
            If TopWindow Is Nothing Then TopWindow = New FrmLayer(unPCB.PCB.WindowTypes.WindowTypeTop, PCB)
            TopWindow.Show()
            TopLayerWindowToolStripMenuItem.Checked = True
        End If
    End Sub

    Private Sub BottomLayerWindowToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BottomLayerWindowToolStripMenuItem.Click
        ShowBottomLayerWindow()
    End Sub

    Public Sub ShowBottomLayerWindow()
        If PCB IsNot Nothing Then
            If BottomWindow Is Nothing Then BottomWindow = New FrmLayer(unPCB.PCB.WindowTypes.WindowTypeBottom, PCB)
            BottomWindow.Show()
            BottomLayerWindowToolStripMenuItem.Checked = True
        End If
    End Sub

    Private Sub TopWindow_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles TopWindow.Disposed
        TopLayerWindowToolStripMenuItem.Checked = False
        TopWindow = Nothing
    End Sub

    Private Sub BottomWindow_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles BottomWindow.Disposed
        BottomLayerWindowToolStripMenuItem.Checked = False
        BottomWindow = Nothing
    End Sub

    Private Sub DevicesWindow_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles DevicesWindow.Disposed
        DevicesToolStripMenuItem.Checked = False
        DevicesWindow = Nothing
    End Sub

    Private Sub ConnectionsWindow_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles ConnectionsWindow.Disposed
        ConnectionsToolStripMenuItem.Checked = False
        ConnectionsWindow = Nothing
    End Sub

    ''' <summary>
    ''' Exports the project to eagle schematic
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ExportEagleSchematic()
        Dim FileName As String
        Dim Project As New Eagle.Project
        Dim Drawing As Eagle.Drawing = Project.CreateDrawing()
        Dim Schematic As Eagle.Schematic = Drawing.CreateSchematic()
        Dim Sheet As Eagle.Sheet = Schematic.AddSheet()

        SaveFileDialog.Filter = "Eagle Schematic (*.sch)|*.sch"
        SaveFileDialog.OverwritePrompt = True
        SaveFileDialog.CheckPathExists = True

        Dim G As Graphics = Me.CreateGraphics()

        If SaveFileDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
            FileName = SaveFileDialog.FileName
            PCB.Schematic.ExportEagleSchematic(FileName)
        End If
        'If SaveFileDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
        '    FileName = SaveFileDialog.FileName
        '    'save all devices and parts
        '    Dim j As Integer = 0
        '    Dim x As Single = 0, y As Single = 0, MaxX As Single = 0

        '    For Each Device As KeyValuePair(Of String, Device) In PCB.Devices
        '        With Device.Value
        '            If .EagleDevice IsNot Nothing Then
        '                Dim LibraryName As String = .EagleDevice.DeviceSet.Library.Name
        '                'add library to the schematic
        '                If Not Schematic.LibraryNodes.ContainsKey(LibraryName) Then
        '                    Schematic.AddXMLLibraryNode(.EagleDevice.DeviceSet.Library.Name, .EagleDevice.DeviceSet.Library.Node)
        '                    Schematic.AddLibrary(.EagleDevice.DeviceSet.Library) 'adds a library object, required if we want to calculate some things like pin locations
        '                End If

        '                'add a part then create an instance
        '                Dim Part As Eagle.Part = Schematic.AddPart(.EagleDevice, .EagleDeviceTechnology, .Name, .Value) 'Schematic.AddPart(.Name, .EagleDevice.DeviceSet.Library.Name, .EagleDevice.DeviceSet.Name, .EagleDevice.Name, .EagleDeviceTechnoly.Name, .Value)

        '                'instantiate each gate on a schematic location
        '                For Each GateInstance As GateInstance In .GateInstances
        '                    'Dim Bounds As RectangleF = Gate.DeviceSet.Library.GetSymbol(Gate.SymbolName).GetRegion().GetBounds(G)
        '                    'Dim Gate_location As New PointF(x - Bounds.Left, y - Bounds.Top) 'New PointF(.Location.X + Gate.Location.X, .Location.Y + Gate.Location.Y)
        '                    Dim Instance As Eagle.Instance = Sheet.AddInstance(Part, GateInstance.Gate, GateInstance.Location) 'Drawing.Grid.RoundLocation(Gate_location))
        '                    'j += 1
        '                    'y += Bounds.Height * 1.25
        '                    'MaxX = Math.Max(x, x + Bounds.Width * 1.25)
        '                    'If j = 10 Then 'after 10 devices added, we move to the next virtual 'column' in the schematic, this makes all device appear in some kind of table
        '                    '    j = 0
        '                    '    y = 0
        '                    '    x = MaxX
        '                    'End If
        '                Next
        '            End If
        '        End With
        '    Next

        '    'get all nets, connect all device pins,...

        '    Dim Nets As List(Of Net) = PCB.ConnectionMatrix.GetNets()
        '    Dim i As Integer = 0
        '    For Each Net As Net In Nets
        '        If Net.Name = "" Then Net.Name = "N$" & i
        '        Dim Segment As Eagle.Segment = Sheet.AddNet(Net.Name).AddSegment()
        '        Dim PPad As Pad = Nothing
        '        For Each Pad As Pad In Net.Pads
        '            If Pad.DevicePin IsNot Nothing Then
        '                Segment.AddPinRef(Pad.DevicePin.Device.Name, Pad.DevicePin.GateName, Pad.DevicePin.PinConnection.PinName)
        '                If PPad IsNot Nothing Then 'spawn wires
        '                    Dim Inst1 As Eagle.Instance = Sheet.GetInstance(PPad.DevicePin.Device.Name, PPad.DevicePin.GateName)
        '                    Dim Inst2 As Eagle.Instance = Sheet.GetInstance(Pad.DevicePin.Device.Name, Pad.DevicePin.GateName)
        '                    Segment.AddWire(Inst1.GetPinLocation(PPad.DevicePin.PinConnection.PinName), Inst2.GetPinLocation(Pad.DevicePin.PinConnection.PinName), Schematic.DefaultWireWidth, Schematic.DefaultWireLayer)
        '                End If
        '            Else
        '                'TO DO: spawn warning about unconnected pin
        '            End If
        '            PPad = Pad
        '        Next
        '        i += 1
        '    Next

        '    Project.SaveSchematic(FileName)
        'End If
    End Sub

    Private Sub ExportEagleToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExportEagleToolStripMenuItem.Click
        ExportEagleSchematic()
    End Sub

    Private Sub SaveAsMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveAsMenuItem.Click
        SaveProjectAs()
    End Sub

    Private Sub UpdateWindowTitle()
        Me.Text = "unPCB - " & PCB.Name & " " & IIf(PCB.IsChanged, "*", "")
    End Sub

    Private Sub PCB_NameChanged(ByVal Sender As PCB, ByVal Name As String) Handles PCB.NameChanged
        UpdateWindowTitle()
    End Sub

    Private Sub PCB_ProjectChanged(ByVal Sender As PCB) Handles PCB.ProjectChanged
        UpdateWindowTitle()
        If ProgramSettings.SelectSingleNode("unPCB/auto_save").InnerText = True Then
            TmrAutoSave.Enabled = True
        Else
            TmrAutoSave.Enabled = False
        End If
    End Sub

    Private Sub PCB_ProjectLoaded(ByVal Sender As PCB, ByVal ZipFile As Ionic.Zip.ZipFile) Handles PCB.ProjectLoaded
        UpdateWindowTitle()
    End Sub

    Private Sub PCB_ProjectSaved(ByVal Sender As PCB, ByVal ZipFile As Ionic.Zip.ZipFile) Handles PCB.ProjectSaved
        UpdateWindowTitle()
    End Sub

    Private Sub ToolStripButtonNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonNew.Click
        NewProject()
    End Sub

    Private Sub ToolStripButtonOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonOpen.Click
        OpenProject()
    End Sub

    Private Sub ToolStripButtonSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonSave.Click
        SaveProject()
    End Sub

    Private Sub ToolsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolsToolStripMenuItem.Click

    End Sub

    Private Sub PCB_UndoRedoStackUpdate(ByVal Sender As PCB, ByVal UndoStack As System.Collections.Generic.LinkedList(Of UndoRedoItem), ByVal RedoStack As System.Collections.Generic.Stack(Of UndoRedoItem)) Handles PCB.UndoRedoStackUpdate
        ToolStripButtonUndo.Enabled = UndoStack.Count > 0
        ToolStripButtonRedo.Enabled = RedoStack.Count > 0
        If UndoStack.Count > 0 Then ToolStripButtonUndo.ToolTipText = "Undo " & UndoStack.First.Value.Description
        If RedoStack.Count > 0 Then ToolStripButtonRedo.ToolTipText = "Redo " & RedoStack.Peek().Description
    End Sub

    Private Sub PropertiesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PropertiesToolStripMenuItem.Click
        Dim Frm As New FrmProjectProperties(PCB)
        Frm.ShowDialog(Me)
    End Sub

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        LoadSettings()

    End Sub

    Public Sub LoadSettings()
        ProgramSettings = New Xml.XmlDocument()
        If System.IO.File.Exists(Application.StartupPath & System.IO.Path.DirectorySeparatorChar & "settings.xml") Then
            ProgramSettings.XmlResolver = Nothing
            ProgramSettings.Load(Application.StartupPath & System.IO.Path.DirectorySeparatorChar & "settings.xml")
        Else
            ProgramSettings.AppendChild(ProgramSettings.CreateXmlDeclaration("1.0", "UTF-8", ""))
        End If
        Dim RootNode As Xml.XmlNode = ProgramSettings.SelectSingleNode("unPCB")
        If RootNode Is Nothing Then
            RootNode = ProgramSettings.AppendChild(ProgramSettings.CreateElement("unPCB"))
        End If
        If RootNode.SelectSingleNode("libraries") Is Nothing Then RootNode.AppendChild(ProgramSettings.CreateElement("libraries"))
        If RootNode.SelectSingleNode("auto_save") Is Nothing Then RootNode.AppendChild(ProgramSettings.CreateElement("auto_save")).InnerText = "False"
        If RootNode.SelectSingleNode("auto_save_interval") Is Nothing Then RootNode.AppendChild(ProgramSettings.CreateElement("auto_save_interval")).InnerText = 300

    End Sub

    Public Sub SaveSettings()
        ProgramSettings.Save(Application.StartupPath & System.IO.Path.DirectorySeparatorChar & "settings.xml")
    End Sub

    Private Sub OptionsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OptionsToolStripMenuItem.Click
        FrmSettings.ShowDialog()
        If ProgramSettings.SelectSingleNode("unPCB/auto_save").InnerText = True Then
            TmrAutoSave.Enabled = True
        Else
            TmrAutoSave.Enabled = False
        End If
    End Sub

    Private Sub UpdateAutoSaveTime()
        NextAutoSaveTime = CInt(ProgramSettings.SelectSingleNode("unPCB/auto_save_interval").InnerText)
    End Sub

    Private Sub TmrAutoSave_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TmrAutoSave.Tick
        NextAutoSaveTime -= 1
        If NextAutoSaveTime <= 0 Then
            AutoSaveProject()
            UpdateAutoSaveTime()
        End If
    End Sub

    Private Sub ToolStripButtonUndo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonUndo.Click
        FrmLayer.ResetDrawState()
        PCB.Undo()
    End Sub

    Private Sub ToolStripButtonRedo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonRedo.Click
        FrmLayer.ResetDrawState()
        PCB.Redo()
    End Sub

    Private Sub SchematicToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SchematicToolStripMenuItem.Click
        If SchematicWindow Is Nothing Then
            SchematicWindow = New FrmSchematic(PCB, Me)
        End If
        SchematicToolStripMenuItem.Checked = True
        SchematicWindow.Show()
    End Sub

    Private Sub SchematicWindow_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles SchematicWindow.Disposed
        SchematicWindow = Nothing
        SchematicToolStripMenuItem.Checked = False
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        FrmAbout.ShowDialog(Me)
    End Sub
End Class
