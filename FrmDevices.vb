Public Class FrmDevices

    Public WithEvents PCB As PCB

    'these dictionaries hold the treenodes for each device / devicepin or pad
    Dim DeviceTreeNodes As Dictionary(Of Device, TreeNode)
    Dim DevicePinTreeNodes As Dictionary(Of DevicePin, TreeNode)
    Dim PadTreeNodes As Dictionary(Of Pad, TreeNode)

    Private Enum DeviceTreeNodeType As Integer
        DeviceTreeNodeRoot = 0
        DeviceTreeNodeDevice = 1
        DeviceTreeNodePin = 2
        DeviceTreeNodePad = 4
    End Enum

    Private Class DeviceTreeNode
        Public Type As DeviceTreeNodeType
        Public NodeObject As Object
        Public Device As Device
        Public Pad As Pad
        Public Pin As DevicePin

        Public Sub New()
            Type = DeviceTreeNodeType.DeviceTreeNodeRoot
        End Sub

        Public Sub New(ByVal Type As DeviceTreeNodeType, ByVal NodeObject As Object)
            Me.Type = Type
            Me.NodeObject = NodeObject
            Select Case Type
                Case DeviceTreeNodeType.DeviceTreeNodeDevice
                    Device = CType(NodeObject, Device)
                Case DeviceTreeNodeType.DeviceTreeNodePad
                    Pad = CType(NodeObject, Pad)
                Case DeviceTreeNodeType.DeviceTreeNodePin
                    Pin = CType(NodeObject, DevicePin)
            End Select
        End Sub

        Public Sub New(ByVal Pin As DevicePin)
            Type = DeviceTreeNodeType.DeviceTreeNodePin
            Me.Pin = Pin
            NodeObject = Pin
        End Sub

        Public Sub New(ByVal Pad As Pad)
            Type = DeviceTreeNodeType.DeviceTreeNodePad
            Me.Pad = Pad
            NodeObject = Pad
        End Sub

        Public Sub New(ByVal Device As Device)
            Type = DeviceTreeNodeType.DeviceTreeNodeDevice
            Me.Device = Device
            NodeObject = Device
        End Sub

    End Class


    Public Sub New(ByVal PCB As PCB)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.PCB = PCB
        '
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)
        UpdateDeviceList()
        AddEventHandlers()
    End Sub


    Private Sub ToolStripButtonAddDevice_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButtonAddDevice.Click
        Dim AddDevice As New FrmAddDevice(PCB)
        AddDevice.ShowDialog(Me)
    End Sub

    Private Shared Function GetDeviceTreeNodeName(ByVal Device As Device) As String
        Dim res As String = Device.Name
        Dim PinCount As Integer = Device.Pins.Count
        If Device.Value <> "" Then res &= " (" & Device.Value & ")"
        res &= " [" & (PinCount - Device.UnconnectedPinCount) & "/" & PinCount & "]"
        Return res
    End Function

    Private Shared Function GetPinTreeNodeName(ByVal Pin As DevicePin) As String
        Return Pin.Name
    End Function

    Private Sub GetExpandedNodes(ByVal Nodes As TreeNode, ByVal Prefix As String, ByVal ExpandedNodes As List(Of String))
        For Each Node As TreeNode In Nodes.Nodes
            If Node.IsExpanded Then
                ExpandedNodes.Add(Prefix & "/" & Node.Name)
            End If
            GetExpandedNodes(Node, Prefix & "/" & Node.Name, ExpandedNodes)
        Next
        If Nodes.IsExpanded Then ExpandedNodes.Add(Prefix & "/" & Nodes.Name)
    End Sub

    Private Sub AddEventHandlers()
        For Each Device As KeyValuePair(Of String, Device) In PCB.Devices
            AddEventHandler(Device.Value)
        Next
    End Sub

    Private Sub AddEventHandler(ByVal Device As Device)
        AddHandler Device.NameChanged, AddressOf DeviceNameChanged
        AddHandler Device.ValueChanged, AddressOf DeviceValueChanged
        For Each Pin As DevicePin In Device.Pins()
            AddHandler Pin.PadAdded, AddressOf DevicePinPadAdded
            AddHandler Pin.PadRemoved, AddressOf DevicePinPadRemoved
        Next
    End Sub

    Private Sub RemoveEventHandler(ByVal Device As Device)
        RemoveHandler Device.NameChanged, AddressOf DeviceNameChanged
        RemoveHandler Device.ValueChanged, AddressOf DeviceValueChanged
        For Each pin As DevicePin In Device.Pins()
            RemoveHandler pin.PadAdded, AddressOf DevicePinPadAdded
            RemoveHandler pin.PadRemoved, AddressOf DevicePinPadRemoved
        Next
    End Sub

    Private Sub UpdateDeviceList()
        Dim Devices As ReadOnlyDictionary(Of String, Device) = PCB.Devices
        Dim ExpandedNodes As New List(Of String)
        Dim Root As SortableTreeNode = New SortableTreeNode(PCB.Name())

        DeviceTreeNodes = New Dictionary(Of Device, TreeNode)
        DevicePinTreeNodes = New Dictionary(Of DevicePin, TreeNode)
        PadTreeNodes = New Dictionary(Of Pad, TreeNode)

        Root.Name = PCB.Name()

        If TrvDevices.Nodes.Count > 0 Then GetExpandedNodes(TrvDevices.Nodes(0), PCB.Name, ExpandedNodes)

        For Each Device As KeyValuePair(Of String, Device) In Devices
            Dim DeviceNode As TreeNode = Root.Nodes.Add(Device.Value.Name, GetDeviceTreeNodeName(Device.Value))
            DeviceNode.Tag = New DeviceTreeNode(Device.Value)
            DeviceNode.ContextMenuStrip = MenuConnectDevice
            DeviceTreeNodes.Add(Device.Value, DeviceNode)
            If ExpandedNodes.Contains(PCB.Name & "/" & Device.Value.Name) Then
                DeviceNode.Expand()
            End If
            For Each DevicePin As DevicePin In Device.Value.Pins
                Dim PinNode As TreeNode = DeviceNode.Nodes.Add(DevicePin.Name, GetPinTreeNodeName(DevicePin))
                PinNode.Tag = New DeviceTreeNode(DevicePin)
                PinNode.ContextMenuStrip = MenuConnectPin
                DevicePinTreeNodes.Add(DevicePin, PinNode)
                If ExpandedNodes.Contains(PCB.Name & "/" & Device.Value.Name & "/" & DevicePin.Name) Then
                    PinNode.Expand()
                End If
                For Each Pad As Pad In DevicePin.Pads
                    Dim PadNode As TreeNode = PinNode.Nodes.Add(Pad.Name, Pad.Name)
                    PadNode.Tag = New DeviceTreeNode(Pad)
                    PadNode.ContextMenuStrip = MenuPad
                    If ExpandedNodes.Contains(PCB.Name & "/" & Device.Value.Name & "/" & DevicePin.Name & "/" & Pad.Name) Then
                        PinNode.Expand()
                    End If
                    PadTreeNodes.Add(Pad, PadNode)
                Next
            Next
        Next

        Root.sort()
        Root.Expand()

        TrvDevices.Nodes.Clear()
        TrvDevices.Nodes.Add(Root)
    End Sub

    Private Sub TrvDevices_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles TrvDevices.AfterSelect
        Dim DeviceTreeNode As DeviceTreeNode = CType(e.Node.Tag, DeviceTreeNode)
        If DeviceTreeNode IsNot Nothing Then
            Select Case DeviceTreeNode.Type
                Case DeviceTreeNodeType.DeviceTreeNodeDevice

                Case DeviceTreeNodeType.DeviceTreeNodePad
                    'PCB.DeselectAllObjects()
                    'PCB.SelectObject(DeviceTreeNode.Pad)
                Case DeviceTreeNodeType.DeviceTreeNodePin
                    'PCB.DeselectAllObjects()
                    'For Each Pad As Pad In DeviceTreeNode.Pin.Pads
                    '    PCB.SelectObject(Pad)
                    'Next
                Case DeviceTreeNodeType.DeviceTreeNodeRoot

            End Select
        End If
    End Sub

    Private Sub PadNameChanged(ByVal Sender As LayerObject, ByVal OldName As String, ByRef NewName As String)
        If PadTreeNodes.ContainsKey(CType(Sender, Pad)) Then
            PadTreeNodes(CType(Sender, Pad)).Text = NewName
        Else
            UpdateDeviceList()
        End If
    End Sub

    Private Sub DevicePinPadAdded(ByVal Sender As DevicePin, ByVal Pad As Pad)
        UpdateDeviceList()
        AddHandler Pad.NameChanged, AddressOf PadNameChanged
    End Sub

    Private Sub DevicePinPadRemoved(ByVal Sender As DevicePin, ByVal Pad As Pad)
        UpdateDeviceList()
        RemoveHandler Pad.NameChanged, AddressOf PadNameChanged
    End Sub

    Private Sub DeviceNameChanged(ByVal Sender As Device, ByVal OldName As String, ByRef NewName As String)
        DeviceTreeNodes(Sender).Text = GetDeviceTreeNodeName(Sender)
    End Sub

    Private Sub DeviceValueChanged(ByVal Sender As Device, ByVal OldValue As String, ByRef NewName As String)
        DeviceTreeNodes(Sender).Text = GetDeviceTreeNodeName(Sender)
    End Sub

    Private Sub FrmDevices_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub MenuConnectPinToSelectedPad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuConnectPinToSelectedPad.Click
        Dim SelectedNode As TreeNode = TrvDevices.SelectedNode
        Dim SelectedDevicePin As DevicePin

        If SelectedNode.Tag IsNot Nothing AndAlso CType(SelectedNode.Tag, DeviceTreeNode).Type = DeviceTreeNodeType.DeviceTreeNodePin Then
            SelectedDevicePin = CType(SelectedNode.Tag.nodeobject, DevicePin)

            Dim SelectedObjects As List(Of SelectableLayerObject) = PCB.GetSelectedLayerObjects(GetType(Pad))
            If SelectedObjects.Count <> 0 Then
                For Each SelectedObject As SelectableLayerObject In SelectedObjects
                    Dim SelectedPad As Pad = CType(SelectedObject, Pad)
                    If SelectedPad.DevicePin IsNot Nothing Then
                        If MsgBox("The pad " & SelectedPad.Name & " is already connected to device " & SelectedPad.DevicePin.Device.Name & " pin " & SelectedPad.DevicePin.Name & " (" & Join(SelectedPad.DevicePin.PadNames, ", ") & ") do you want to disconnect it?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                            PCB.AddUndoItem(New UndoRedoDisconnectPin(PCB, SelectedDevicePin, SelectedPad))
                            SelectedPad.DevicePin.RemovePad(SelectedPad)
                            SelectedPad.DevicePin = Nothing
                        End If
                    End If
                    If SelectedPad.DevicePin Is Nothing Then
                        PCB.AddUndoItem(New UndoRedoConnectPin(PCB, SelectedDevicePin, SelectedPad))
                        Dim EaglePadName As String
                        Dim EaglePadNames As List(Of String) = SelectedDevicePin.GetUnconnectedPadNames()
                        If EaglePadNames.Count > 1 Then
                            MsgBox("More padnames possible for this pin, taking " & EaglePadNames(0))
                        End If
                        If EaglePadNames.Count = 0 Then
                            MsgBox("This pin has the maximum connected pads!" & vbCrLf & "Disconnect one of the pads first before adding more.", MsgBoxStyle.Critical)
                            Exit Sub
                        End If
                        EaglePadName = EaglePadNames(0)
                        SelectedPad.Name = SelectedDevicePin.GetUniquePadName(EaglePadName)
                        SelectedDevicePin.AddPad(SelectedPad, EaglePadName)
                    End If
                Next
                UpdateDeviceList()
            Else
                MsgBox("Select pads to connect with first!", MsgBoxStyle.Critical)
            End If
        End If
    End Sub

    Private Sub PlaceAPadForEachPinToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PlaceAPadForEachPinToolStripMenuItem.Click
        Dim SelectedNode As TreeNode = TrvDevices.SelectedNode
        If SelectedNode IsNot Nothing AndAlso SelectedNode.Tag IsNot Nothing AndAlso CType(SelectedNode.Tag, DeviceTreeNode).Type = DeviceTreeNodeType.DeviceTreeNodeDevice Then
            Dim SelectedDevice As Device = CType(SelectedNode.Tag.nodeobject, Device)

            FrmLayer.DrawDevicePins(SelectedDevice)
        End If
    End Sub

    Private Sub DeleteDeviceToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeleteDeviceToolStripMenuItem.Click
        Dim SelectedNode As TreeNode = TrvDevices.SelectedNode
        If SelectedNode IsNot Nothing AndAlso SelectedNode.Tag IsNot Nothing AndAlso CType(SelectedNode.Tag, DeviceTreeNode).Type = DeviceTreeNodeType.DeviceTreeNodeDevice Then
            Dim SelectedDevice As Device = CType(SelectedNode.Tag.nodeobject, Device)
            If MsgBox("Are you sure you want to delete this device and it's pins/pads?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                If FrmLayer.CurrentDrawState = FrmLayer.DrawStates.DrawDevicePins Then FrmLayer.ResetDrawState()
                PCB.UnHighlightAllObjects()
                PCB.DeselectAllObjects()
                PCB.AddUndoItem(New UndoRedoDeleteDevice(PCB, SelectedDevice))
                PCB.RemoveDevice(SelectedDevice)
            End If
        End If
    End Sub


    Private Sub SelectAllPadsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectAllPadsToolStripMenuItem.Click
        Dim SelectedNode As TreeNode = TrvDevices.SelectedNode
        If SelectedNode IsNot Nothing AndAlso SelectedNode.Tag IsNot Nothing AndAlso CType(SelectedNode.Tag, DeviceTreeNode).Type = DeviceTreeNodeType.DeviceTreeNodeDevice Then
            Dim DeviceTreeNode As DeviceTreeNode = CType(SelectedNode.Tag, DeviceTreeNode)
            PCB.DeselectAllObjects()
            For Each Pin As DevicePin In DeviceTreeNode.Device.Pins
                For Each Pad As Pad In Pin.Pads
                    PCB.SelectObject(Pad)
                Next
            Next
        End If
    End Sub

    Private Sub PCB_DeviceAdded(ByVal Sender As PCB, ByVal Device As Device) Handles PCB.DeviceAdded
        UpdateDeviceList()
        AddEventHandler(Device)
    End Sub

    Private Sub PCB_DeviceRemoved(ByVal Sender As PCB, ByVal Device As Device) Handles PCB.DeviceRemoved
        UpdateDeviceList()
        RemoveEventHandler(Device)
    End Sub

    Private Sub PCB_NameChanged(ByVal Sender As PCB, ByVal Name As String) Handles PCB.NameChanged
        If TrvDevices.Nodes.Count > 0 Then
            TrvDevices.Nodes(0).Text = Name
        Else
            UpdateDeviceList()
        End If
    End Sub

    Private Sub PCB_ProjectLoaded(ByVal Sender As PCB, ByVal ZipFile As Ionic.Zip.ZipFile) Handles PCB.ProjectLoaded
        AddEventHandlers()
    End Sub

    Private Sub RenameDeviceToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RenameDeviceToolStripMenuItem.Click
        Dim SelectedNode As TreeNode = TrvDevices.SelectedNode
        If SelectedNode IsNot Nothing AndAlso SelectedNode.Tag IsNot Nothing AndAlso CType(SelectedNode.Tag, DeviceTreeNode).Type = DeviceTreeNodeType.DeviceTreeNodeDevice Then
            Dim DeviceTreeNode As DeviceTreeNode = CType(SelectedNode.Tag, DeviceTreeNode)
            Dim OldName As String = DeviceTreeNode.Device.Name
            Dim NewName As String = InputBox("Enter new name.", "Rename device", OldName)
            If NewName <> "" Then
                If NewName <> DeviceTreeNode.Device.Name Then
                    If Not PCB.Devices.ContainsKey(NewName) Then
                        DeviceTreeNode.Device.Name = NewName
                        PCB.AddUndoItem(New UndoRedoDeviceNameChange(PCB, DeviceTreeNode.Device, OldName, NewName))
                        UpdateDeviceList()
                    Else
                        MsgBox("This name already exists.", MsgBoxStyle.Critical)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub ChangeDeviceValueToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChangeDeviceValueToolStripMenuItem.Click
        Dim SelectedNode As TreeNode = TrvDevices.SelectedNode
        If SelectedNode IsNot Nothing AndAlso SelectedNode.Tag IsNot Nothing AndAlso CType(SelectedNode.Tag, DeviceTreeNode).Type = DeviceTreeNodeType.DeviceTreeNodeDevice Then
            Dim DeviceTreeNode As DeviceTreeNode = CType(SelectedNode.Tag, DeviceTreeNode)
            Dim OldValue As String = DeviceTreeNode.Device.Value
            Dim NewValue As String = InputBox("Enter new value.", "Change device value", OldValue)
            If NewValue <> DeviceTreeNode.Device.Value Then
                PCB.AddUndoItem(New UndoRedoDeviceValueChange(PCB, DeviceTreeNode.Device, OldValue, NewValue))
                DeviceTreeNode.Device.Value = NewValue
                UpdateDeviceList()
            End If
        End If
    End Sub

    Private Sub SelectAllPadsToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectAllPadsToolStripMenuItem1.Click
        Dim SelectedNode As TreeNode = TrvDevices.SelectedNode
        If SelectedNode IsNot Nothing AndAlso SelectedNode.Tag IsNot Nothing AndAlso CType(SelectedNode.Tag, DeviceTreeNode).Type = DeviceTreeNodeType.DeviceTreeNodePin Then
            Dim DeviceTreeNode As DeviceTreeNode = CType(SelectedNode.Tag, DeviceTreeNode)
            PCB.DeselectAllObjects()
            For Each Pad As Pad In DeviceTreeNode.Pin.Pads
                PCB.SelectObject(Pad)
            Next
        End If
    End Sub

    Private Sub SelectPadToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectPadToolStripMenuItem.Click
        Dim SelectedNode As TreeNode = TrvDevices.SelectedNode
        If SelectedNode IsNot Nothing AndAlso SelectedNode.Tag IsNot Nothing AndAlso CType(SelectedNode.Tag, DeviceTreeNode).Type = DeviceTreeNodeType.DeviceTreeNodePad Then
            Dim DeviceTreeNode As DeviceTreeNode = CType(SelectedNode.Tag, DeviceTreeNode)
            PCB.DeselectAllObjects()
            PCB.SelectObject(DeviceTreeNode.Pad)
        End If
    End Sub

    Private Sub PCB_UndoRedoAction(ByVal Sender As PCB, ByVal UndoRedoItem As UndoRedoItem, ByVal Undo As Boolean) Handles PCB.UndoRedoAction
        UpdateDeviceList()
    End Sub

    Private Sub MenuPad_Opening(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MenuPad.Opening

    End Sub
End Class