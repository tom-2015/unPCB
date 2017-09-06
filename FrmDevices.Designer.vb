<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmDevices
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmDevices))
        Me.ToolStripDevices = New System.Windows.Forms.ToolStrip
        Me.ToolStripButtonAddDevice = New System.Windows.Forms.ToolStripButton
        Me.TrvDevices = New System.Windows.Forms.TreeView
        Me.MenuConnectPin = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.MenuConnectPinToSelectedPad = New System.Windows.Forms.ToolStripMenuItem
        Me.SelectAllPadsToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem
        Me.MenuConnectDevice = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.PlaceAPadForEachPinToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.DeleteDeviceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.SelectAllPadsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.RenameDeviceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ChangeDeviceValueToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.MenuPad = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.SelectPadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripDevices.SuspendLayout()
        Me.MenuConnectPin.SuspendLayout()
        Me.MenuConnectDevice.SuspendLayout()
        Me.MenuPad.SuspendLayout()
        Me.SuspendLayout()
        '
        'ToolStripDevices
        '
        Me.ToolStripDevices.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButtonAddDevice})
        Me.ToolStripDevices.Location = New System.Drawing.Point(0, 0)
        Me.ToolStripDevices.Name = "ToolStripDevices"
        Me.ToolStripDevices.Size = New System.Drawing.Size(550, 25)
        Me.ToolStripDevices.TabIndex = 1
        Me.ToolStripDevices.Text = "ToolStrip1"
        '
        'ToolStripButtonAddDevice
        '
        Me.ToolStripButtonAddDevice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonAddDevice.Image = Global.unPCB.My.Resources.Resources.add
        Me.ToolStripButtonAddDevice.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonAddDevice.Name = "ToolStripButtonAddDevice"
        Me.ToolStripButtonAddDevice.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonAddDevice.Text = "Add device"
        '
        'TrvDevices
        '
        Me.TrvDevices.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TrvDevices.Location = New System.Drawing.Point(0, 25)
        Me.TrvDevices.Name = "TrvDevices"
        Me.TrvDevices.Size = New System.Drawing.Size(550, 532)
        Me.TrvDevices.TabIndex = 2
        '
        'MenuConnectPin
        '
        Me.MenuConnectPin.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuConnectPinToSelectedPad, Me.SelectAllPadsToolStripMenuItem1})
        Me.MenuConnectPin.Name = "MenuConnectPin"
        Me.MenuConnectPin.Size = New System.Drawing.Size(228, 48)
        '
        'MenuConnectPinToSelectedPad
        '
        Me.MenuConnectPinToSelectedPad.Name = "MenuConnectPinToSelectedPad"
        Me.MenuConnectPinToSelectedPad.Size = New System.Drawing.Size(227, 22)
        Me.MenuConnectPinToSelectedPad.Text = "Connect Pin To Selected Pads"
        '
        'SelectAllPadsToolStripMenuItem1
        '
        Me.SelectAllPadsToolStripMenuItem1.Name = "SelectAllPadsToolStripMenuItem1"
        Me.SelectAllPadsToolStripMenuItem1.Size = New System.Drawing.Size(227, 22)
        Me.SelectAllPadsToolStripMenuItem1.Text = "Select All Pads"
        '
        'MenuConnectDevice
        '
        Me.MenuConnectDevice.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.PlaceAPadForEachPinToolStripMenuItem, Me.DeleteDeviceToolStripMenuItem, Me.SelectAllPadsToolStripMenuItem, Me.RenameDeviceToolStripMenuItem, Me.ChangeDeviceValueToolStripMenuItem})
        Me.MenuConnectDevice.Name = "MenuConnectDevice"
        Me.MenuConnectDevice.Size = New System.Drawing.Size(201, 114)
        '
        'PlaceAPadForEachPinToolStripMenuItem
        '
        Me.PlaceAPadForEachPinToolStripMenuItem.Name = "PlaceAPadForEachPinToolStripMenuItem"
        Me.PlaceAPadForEachPinToolStripMenuItem.Size = New System.Drawing.Size(200, 22)
        Me.PlaceAPadForEachPinToolStripMenuItem.Text = "Place a pad for each pin"
        Me.PlaceAPadForEachPinToolStripMenuItem.ToolTipText = "Places a pad for each unconnected device pin"
        '
        'DeleteDeviceToolStripMenuItem
        '
        Me.DeleteDeviceToolStripMenuItem.Name = "DeleteDeviceToolStripMenuItem"
        Me.DeleteDeviceToolStripMenuItem.Size = New System.Drawing.Size(200, 22)
        Me.DeleteDeviceToolStripMenuItem.Text = "Delete Device"
        Me.DeleteDeviceToolStripMenuItem.ToolTipText = "Deletes selected device"
        '
        'SelectAllPadsToolStripMenuItem
        '
        Me.SelectAllPadsToolStripMenuItem.Name = "SelectAllPadsToolStripMenuItem"
        Me.SelectAllPadsToolStripMenuItem.Size = New System.Drawing.Size(200, 22)
        Me.SelectAllPadsToolStripMenuItem.Text = "Select all pads"
        '
        'RenameDeviceToolStripMenuItem
        '
        Me.RenameDeviceToolStripMenuItem.Name = "RenameDeviceToolStripMenuItem"
        Me.RenameDeviceToolStripMenuItem.Size = New System.Drawing.Size(200, 22)
        Me.RenameDeviceToolStripMenuItem.Text = "Rename"
        '
        'ChangeDeviceValueToolStripMenuItem
        '
        Me.ChangeDeviceValueToolStripMenuItem.Name = "ChangeDeviceValueToolStripMenuItem"
        Me.ChangeDeviceValueToolStripMenuItem.Size = New System.Drawing.Size(200, 22)
        Me.ChangeDeviceValueToolStripMenuItem.Text = "Change value"
        '
        'MenuPad
        '
        Me.MenuPad.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SelectPadToolStripMenuItem})
        Me.MenuPad.Name = "MenuPad"
        Me.MenuPad.Size = New System.Drawing.Size(136, 26)
        '
        'SelectPadToolStripMenuItem
        '
        Me.SelectPadToolStripMenuItem.Name = "SelectPadToolStripMenuItem"
        Me.SelectPadToolStripMenuItem.Size = New System.Drawing.Size(135, 22)
        Me.SelectPadToolStripMenuItem.Text = "Select Pad"
        '
        'FrmDevices
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(550, 557)
        Me.Controls.Add(Me.TrvDevices)
        Me.Controls.Add(Me.ToolStripDevices)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "FrmDevices"
        Me.Text = "Devices"
        Me.ToolStripDevices.ResumeLayout(False)
        Me.ToolStripDevices.PerformLayout()
        Me.MenuConnectPin.ResumeLayout(False)
        Me.MenuConnectDevice.ResumeLayout(False)
        Me.MenuPad.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ToolStripDevices As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripButtonAddDevice As System.Windows.Forms.ToolStripButton
    Friend WithEvents TrvDevices As System.Windows.Forms.TreeView
    Friend WithEvents MenuConnectPin As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents MenuConnectPinToSelectedPad As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MenuConnectDevice As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents PlaceAPadForEachPinToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DeleteDeviceToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SelectAllPadsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RenameDeviceToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ChangeDeviceValueToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SelectAllPadsToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MenuPad As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents SelectPadToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
