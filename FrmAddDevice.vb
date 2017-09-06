Imports system.reflection
Public Class FrmAddDevice


    Public PCB As PCB

    Private Shared SelectedDevicePath As String 'path to selected treeitem

    Dim SelectedEagleDevice As Eagle.Device 'current selected eagle device
    Dim SelectedTechnology As Eagle.Technology
    Dim SelectedDeviceTag As DeviceTag

    Dim DefaultName As String

    Private Structure DeviceTag
        Public Device As Eagle.Device
        Public Technology As Eagle.Technology
        'Dim Node As TreeNode

        Public Sub New(ByVal Device As Eagle.Device)
            Me.Device = Device
        End Sub
        Public Sub New(ByVal Device As Eagle.Device, ByVal Technology As Eagle.Technology)
            Me.Device = Device
            Me.Technology = Technology
        End Sub
    End Structure

    Public Sub New(ByVal PCB As PCB)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.PCB = PCB

    End Sub


    Private Sub CmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub

    Private Sub FrmAddDevice_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Activated
        RefreshDeviceList()
    End Sub

    Private Sub FrmAddDevice_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Public Sub RefreshDeviceList()
        TrvDevices.Nodes.Clear()
        For Each Project As Eagle.Project In PCB.Libraries 'add each library to the treeview
            Dim Root As TreeNode
            If Project.Drawing.DrawingType = Eagle.DrawingType.library Then
                Dim Library As Eagle.Library = Project.Drawing.Library
                Root = TrvDevices.Nodes.Add(Library.Name)

                For Each DeviceSet As Eagle.DeviceSet In Library.DevicesSets
                    Dim DeviceSetNode As TreeNode = Root.Nodes.Add(DeviceSet.Name)

                    If DeviceSet.Devices.Count = 1 AndAlso DeviceSet.Devices(0).Technologies.Count <= 1 Then
                        DeviceSetNode.Tag = New DeviceTag(DeviceSet.Devices(0))
                        If SelectedDevicePath = DeviceSetNode.FullPath Then TrvDevices.SelectedNode = DeviceSetNode
                    Else
                        For Each Device As Eagle.Device In DeviceSet.Devices
                            For Each Technology As Eagle.Technology In Device.Technologies
                                Dim DeviceNode As TreeNode = DeviceSetNode.Nodes.Add(Replace(DeviceSet.Name, "*", Technology.Name) & Device.Name)
                                DeviceNode.Tag = New DeviceTag(Device, Technology)
                                If InStr(SelectedDevicePath, DeviceNode.FullPath) = 1 Then
                                    DeviceNode.Expand()
                                    TrvDevices.SelectedNode = DeviceNode
                                End If
                            Next
                        Next
                    End If
                    If InStr(SelectedDevicePath, DeviceSetNode.FullPath) = 1 Then DeviceSetNode.Expand()
                Next
                If InStr(SelectedDevicePath, Root.FullPath) = 1 Then Root.Expand()
            End If
        Next
        TrvDevices.Focus()
    End Sub

    Private Sub CmdCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdCreate.Click
        If Trim(TxtName.Text) = "" Then
            MsgBox("Enter a name for your device.", MsgBoxStyle.Critical)
            Exit Sub
        End If

        If Not PCB.DeviceNameExists(TxtName.Text) Then
            Dim Device As New Device(PCB, SelectedEagleDevice, SelectedTechnology, Trim(TxtName.Text), Trim(TxtValue.Text))
            PCB.AddUndoItem(New UndoRedoAddDevice(PCB, Device))
            PCB.AddDevice(Device)
            Me.Close()
        Else
            MsgBox("The name for the device already exists, please use a different name!", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub UseLibraryToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UseLibraryToolStripMenuItem.Click
        If OFDialog.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            For Each FileName As String In OFDialog.FileNames
                If Not PCB.LoadEagleLibrary(FileName) Then
                    MsgBox("The file " & FileName & " does not appear to be a valid Eagle XML library file!", MsgBoxStyle.Critical)
                End If
            Next
            PCB.SortEagleLibraries()
            SelectedDevicePath = ""
            RefreshDeviceList()
        End If
    End Sub

    Private Sub TrvDevices_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles TrvDevices.AfterSelect
        If TrvDevices.SelectedNode IsNot Nothing Then
            SelectedDevicePath = TrvDevices.SelectedNode.FullPath
            If TrvDevices.SelectedNode.Tag IsNot Nothing Then
                SelectedEagleDevice = CType(TrvDevices.SelectedNode.Tag.device, Eagle.Device)
                If SelectedEagleDevice IsNot Nothing Then
                    TxtName.Text = PCB.GetUniqueDeviceName(SelectedEagleDevice.DeviceSet.Prefix)
                    TxtDeviceDescription.Text = SelectedEagleDevice.DeviceSet.Description
                    SelectedTechnology = CType(TrvDevices.SelectedNode.Tag, DeviceTag).Technology
                    If SelectedEagleDevice.DeviceSet.UserValue Then
                        TxtValue.Text = ""
                    Else
                        Dim TechnologyName As String = ""
                        If SelectedTechnology IsNot Nothing Then TechnologyName = SelectedTechnology.Name
                        TxtValue.Text = Replace(SelectedEagleDevice.DeviceSet.Name, "*", TechnologyName) & SelectedEagleDevice.Name
                    End If
                End If
            End If
        End If
        PctSymbol.Refresh()
    End Sub

    Private Sub PctSymbol_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PctSymbol.Paint
        Dim Region As Region
        Dim Graphics As Graphics = e.Graphics
        Dim Scale As Single, ScaleX As Single, ScaleY As Single

        Graphics.Clear(Color.White)
        If SelectedEagleDevice IsNot Nothing Then
            Region = SelectedEagleDevice.GetSymbolRegion()
            Dim rect As RectangleF = Region.GetBounds(Graphics)

            ScaleX = Graphics.VisibleClipBounds.Width / (rect.Width * 1.1)
            ScaleY = Graphics.VisibleClipBounds.Height / (rect.Height * 1.1)
            Scale = Math.Min(ScaleX, ScaleY)
            Graphics.TranslateTransform(0, rect.Height * Scale)
            Graphics.ScaleTransform(Scale, -Scale)
            Graphics.TranslateTransform(-rect.X + (Graphics.VisibleClipBounds.Width - rect.Width) / 2, -rect.Y - (Graphics.VisibleClipBounds.Height - rect.Height) / 2)
            'Graphics.DrawRectangle(Pens.Blue, rect.X, rect.Y, rect.Width, rect.Height) 'uncomment if you want to see the region
            SelectedEagleDevice.RenderSymbol(Graphics)
        End If
    End Sub

    Private Sub CmdCancel_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdCancel.Click
        Me.Close()
    End Sub
End Class