<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmLayer
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmLayer))
        Me.MenuBar = New System.Windows.Forms.MenuStrip
        Me.MenuFile = New System.Windows.Forms.ToolStripMenuItem
        Me.MenuOpen = New System.Windows.Forms.ToolStripMenuItem
        Me.MenuSave = New System.Windows.Forms.ToolStripMenuItem
        Me.SaveAsMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator
        Me.MenuExit = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.BackgroundToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.LoadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.MoveToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.FlipHorizontallyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.FlipVerticallyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.LayersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.Toolbar = New System.Windows.Forms.ToolStrip
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator
        Me.ToolStripSeparator4 = New System.Windows.Forms.ToolStripSeparator
        Me.ToolStripSeparator5 = New System.Windows.Forms.ToolStripSeparator
        Me.MenuRightClickObject = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.Menu_SelectConnectedPads = New System.Windows.Forms.ToolStripMenuItem
        Me.HighlightConnectedPadsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator
        Me.Menu_Properties = New System.Windows.Forms.ToolStripMenuItem
        Me.Menu_deleteObject = New System.Windows.Forms.ToolStripMenuItem
        Me.MoveObjectToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.OpenFileDialog = New System.Windows.Forms.OpenFileDialog
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip
        Me.StatusBar = New System.Windows.Forms.ToolStripStatusLabel
        Me.DoubleBuffer = New unPCB.DoubleBuffer
        Me.ToolStripButtonPointer = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButtonMove = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButtonResize = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButtonText = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButtonUndo = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButtonRedo = New System.Windows.Forms.ToolStripButton
        Me.ToolStripSplitButtonDrawPad = New System.Windows.Forms.ToolStripSplitButton
        Me.SMDPadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ThroughHolePadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ViaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.RouteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.RouteJunctionToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.SplitRouteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripButtonConnect = New System.Windows.Forms.ToolStripButton
        Me.ToolStripDisconnect = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButtonUnconnect = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButtonConnectionFinished = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButtonAutoRoute = New System.Windows.Forms.ToolStripButton
        Me.MenuBar.SuspendLayout()
        Me.Toolbar.SuspendLayout()
        Me.MenuRightClickObject.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'MenuBar
        '
        Me.MenuBar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuFile, Me.ToolsToolStripMenuItem, Me.LayersToolStripMenuItem})
        Me.MenuBar.Location = New System.Drawing.Point(0, 0)
        Me.MenuBar.Name = "MenuBar"
        Me.MenuBar.Size = New System.Drawing.Size(727, 24)
        Me.MenuBar.TabIndex = 0
        Me.MenuBar.Text = "MenuStrip1"
        '
        'MenuFile
        '
        Me.MenuFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuOpen, Me.MenuSave, Me.SaveAsMenuItem, Me.ToolStripSeparator2, Me.MenuExit})
        Me.MenuFile.Name = "MenuFile"
        Me.MenuFile.Size = New System.Drawing.Size(35, 20)
        Me.MenuFile.Text = "File"
        '
        'MenuOpen
        '
        Me.MenuOpen.Name = "MenuOpen"
        Me.MenuOpen.Size = New System.Drawing.Size(114, 22)
        Me.MenuOpen.Text = "Open"
        '
        'MenuSave
        '
        Me.MenuSave.Name = "MenuSave"
        Me.MenuSave.Size = New System.Drawing.Size(114, 22)
        Me.MenuSave.Text = "Save"
        '
        'SaveAsMenuItem
        '
        Me.SaveAsMenuItem.Name = "SaveAsMenuItem"
        Me.SaveAsMenuItem.Size = New System.Drawing.Size(114, 22)
        Me.SaveAsMenuItem.Text = "Save As"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(111, 6)
        '
        'MenuExit
        '
        Me.MenuExit.Name = "MenuExit"
        Me.MenuExit.Size = New System.Drawing.Size(114, 22)
        Me.MenuExit.Text = "Exit"
        '
        'ToolsToolStripMenuItem
        '
        Me.ToolsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.BackgroundToolStripMenuItem})
        Me.ToolsToolStripMenuItem.Name = "ToolsToolStripMenuItem"
        Me.ToolsToolStripMenuItem.Size = New System.Drawing.Size(45, 20)
        Me.ToolsToolStripMenuItem.Text = "Tools"
        '
        'BackgroundToolStripMenuItem
        '
        Me.BackgroundToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LoadToolStripMenuItem, Me.MoveToolStripMenuItem, Me.FlipHorizontallyToolStripMenuItem, Me.FlipVerticallyToolStripMenuItem})
        Me.BackgroundToolStripMenuItem.Name = "BackgroundToolStripMenuItem"
        Me.BackgroundToolStripMenuItem.Size = New System.Drawing.Size(156, 22)
        Me.BackgroundToolStripMenuItem.Text = "PCB Background"
        '
        'LoadToolStripMenuItem
        '
        Me.LoadToolStripMenuItem.Name = "LoadToolStripMenuItem"
        Me.LoadToolStripMenuItem.Size = New System.Drawing.Size(147, 22)
        Me.LoadToolStripMenuItem.Text = "Load"
        '
        'MoveToolStripMenuItem
        '
        Me.MoveToolStripMenuItem.Name = "MoveToolStripMenuItem"
        Me.MoveToolStripMenuItem.Size = New System.Drawing.Size(147, 22)
        Me.MoveToolStripMenuItem.Text = "Move"
        '
        'FlipHorizontallyToolStripMenuItem
        '
        Me.FlipHorizontallyToolStripMenuItem.Name = "FlipHorizontallyToolStripMenuItem"
        Me.FlipHorizontallyToolStripMenuItem.Size = New System.Drawing.Size(147, 22)
        Me.FlipHorizontallyToolStripMenuItem.Text = "Flip Horizontally"
        '
        'FlipVerticallyToolStripMenuItem
        '
        Me.FlipVerticallyToolStripMenuItem.Name = "FlipVerticallyToolStripMenuItem"
        Me.FlipVerticallyToolStripMenuItem.Size = New System.Drawing.Size(147, 22)
        Me.FlipVerticallyToolStripMenuItem.Text = "Flip Vertically"
        '
        'LayersToolStripMenuItem
        '
        Me.LayersToolStripMenuItem.Name = "LayersToolStripMenuItem"
        Me.LayersToolStripMenuItem.Size = New System.Drawing.Size(50, 20)
        Me.LayersToolStripMenuItem.Text = "Layers"
        '
        'Toolbar
        '
        Me.Toolbar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButtonPointer, Me.ToolStripButtonMove, Me.ToolStripButtonResize, Me.ToolStripButtonText, Me.ToolStripSeparator3, Me.ToolStripButtonUndo, Me.ToolStripButtonRedo, Me.ToolStripSeparator4, Me.ToolStripSplitButtonDrawPad, Me.ToolStripSeparator5, Me.ToolStripButtonConnect, Me.ToolStripDisconnect, Me.ToolStripButtonUnconnect, Me.ToolStripButtonConnectionFinished, Me.ToolStripButtonAutoRoute})
        Me.Toolbar.Location = New System.Drawing.Point(0, 24)
        Me.Toolbar.Name = "Toolbar"
        Me.Toolbar.Size = New System.Drawing.Size(727, 25)
        Me.Toolbar.TabIndex = 1
        Me.Toolbar.Text = "Toolbar"
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        Me.ToolStripSeparator3.Size = New System.Drawing.Size(6, 25)
        '
        'ToolStripSeparator4
        '
        Me.ToolStripSeparator4.Name = "ToolStripSeparator4"
        Me.ToolStripSeparator4.Size = New System.Drawing.Size(6, 25)
        '
        'ToolStripSeparator5
        '
        Me.ToolStripSeparator5.Name = "ToolStripSeparator5"
        Me.ToolStripSeparator5.Size = New System.Drawing.Size(6, 25)
        '
        'MenuRightClickObject
        '
        Me.MenuRightClickObject.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Menu_SelectConnectedPads, Me.HighlightConnectedPadsToolStripMenuItem, Me.ToolStripSeparator1, Me.Menu_Properties, Me.Menu_deleteObject, Me.MoveObjectToolStripMenuItem})
        Me.MenuRightClickObject.Name = "MenuRightClickObject"
        Me.MenuRightClickObject.Size = New System.Drawing.Size(198, 120)
        '
        'Menu_SelectConnectedPads
        '
        Me.Menu_SelectConnectedPads.Enabled = False
        Me.Menu_SelectConnectedPads.Name = "Menu_SelectConnectedPads"
        Me.Menu_SelectConnectedPads.Size = New System.Drawing.Size(197, 22)
        Me.Menu_SelectConnectedPads.Text = "Select Connected Pads"
        '
        'HighlightConnectedPadsToolStripMenuItem
        '
        Me.HighlightConnectedPadsToolStripMenuItem.Enabled = False
        Me.HighlightConnectedPadsToolStripMenuItem.Name = "HighlightConnectedPadsToolStripMenuItem"
        Me.HighlightConnectedPadsToolStripMenuItem.Size = New System.Drawing.Size(197, 22)
        Me.HighlightConnectedPadsToolStripMenuItem.Text = "Highlight Connected Pads"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(194, 6)
        '
        'Menu_Properties
        '
        Me.Menu_Properties.Name = "Menu_Properties"
        Me.Menu_Properties.Size = New System.Drawing.Size(197, 22)
        Me.Menu_Properties.Text = "Properties"
        '
        'Menu_deleteObject
        '
        Me.Menu_deleteObject.Name = "Menu_deleteObject"
        Me.Menu_deleteObject.Size = New System.Drawing.Size(197, 22)
        Me.Menu_deleteObject.Text = "Delete"
        '
        'MoveObjectToolStripMenuItem
        '
        Me.MoveObjectToolStripMenuItem.Name = "MoveObjectToolStripMenuItem"
        Me.MoveObjectToolStripMenuItem.Size = New System.Drawing.Size(197, 22)
        Me.MoveObjectToolStripMenuItem.Text = "Move"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StatusBar})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 422)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(727, 22)
        Me.StatusStrip1.TabIndex = 3
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'StatusBar
        '
        Me.StatusBar.Name = "StatusBar"
        Me.StatusBar.Size = New System.Drawing.Size(38, 17)
        Me.StatusBar.Text = "Ready"
        '
        'DoubleBuffer
        '
        Me.DoubleBuffer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DoubleBuffer.Location = New System.Drawing.Point(0, 49)
        Me.DoubleBuffer.Name = "DoubleBuffer"
        Me.DoubleBuffer.Size = New System.Drawing.Size(727, 395)
        Me.DoubleBuffer.TabIndex = 2
        '
        'ToolStripButtonPointer
        '
        Me.ToolStripButtonPointer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonPointer.Image = Global.unPCB.My.Resources.Resources.ToolPointer2
        Me.ToolStripButtonPointer.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonPointer.Name = "ToolStripButtonPointer"
        Me.ToolStripButtonPointer.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonPointer.Text = "Select objects"
        '
        'ToolStripButtonMove
        '
        Me.ToolStripButtonMove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonMove.Image = CType(resources.GetObject("ToolStripButtonMove.Image"), System.Drawing.Image)
        Me.ToolStripButtonMove.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonMove.Name = "ToolStripButtonMove"
        Me.ToolStripButtonMove.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonMove.Text = "Move"
        '
        'ToolStripButtonResize
        '
        Me.ToolStripButtonResize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonResize.Image = CType(resources.GetObject("ToolStripButtonResize.Image"), System.Drawing.Image)
        Me.ToolStripButtonResize.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonResize.Name = "ToolStripButtonResize"
        Me.ToolStripButtonResize.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonResize.Text = "Resize"
        '
        'ToolStripButtonText
        '
        Me.ToolStripButtonText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonText.Image = Global.unPCB.My.Resources.Resources.Text
        Me.ToolStripButtonText.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonText.Name = "ToolStripButtonText"
        Me.ToolStripButtonText.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonText.ToolTipText = "Add Text"
        '
        'ToolStripButtonUndo
        '
        Me.ToolStripButtonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonUndo.Image = Global.unPCB.My.Resources.Resources.undo
        Me.ToolStripButtonUndo.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonUndo.Name = "ToolStripButtonUndo"
        Me.ToolStripButtonUndo.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonUndo.Text = "Undo"
        Me.ToolStripButtonUndo.ToolTipText = "Undo"
        '
        'ToolStripButtonRedo
        '
        Me.ToolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonRedo.Image = Global.unPCB.My.Resources.Resources.redo
        Me.ToolStripButtonRedo.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonRedo.Name = "ToolStripButtonRedo"
        Me.ToolStripButtonRedo.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonRedo.Text = "Redo"
        Me.ToolStripButtonRedo.ToolTipText = "Redo"
        '
        'ToolStripSplitButtonDrawPad
        '
        Me.ToolStripSplitButtonDrawPad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripSplitButtonDrawPad.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SMDPadToolStripMenuItem, Me.ThroughHolePadToolStripMenuItem, Me.ViaToolStripMenuItem, Me.RouteToolStripMenuItem, Me.RouteJunctionToolStripMenuItem, Me.SplitRouteToolStripMenuItem})
        Me.ToolStripSplitButtonDrawPad.Image = Global.unPCB.My.Resources.Resources.smd_pad
        Me.ToolStripSplitButtonDrawPad.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripSplitButtonDrawPad.Name = "ToolStripSplitButtonDrawPad"
        Me.ToolStripSplitButtonDrawPad.Size = New System.Drawing.Size(32, 22)
        Me.ToolStripSplitButtonDrawPad.Text = "Draw a new unconnected pad."
        '
        'SMDPadToolStripMenuItem
        '
        Me.SMDPadToolStripMenuItem.Image = Global.unPCB.My.Resources.Resources.smd_pad
        Me.SMDPadToolStripMenuItem.Name = "SMDPadToolStripMenuItem"
        Me.SMDPadToolStripMenuItem.Size = New System.Drawing.Size(161, 22)
        Me.SMDPadToolStripMenuItem.Text = "SMD Pad"
        '
        'ThroughHolePadToolStripMenuItem
        '
        Me.ThroughHolePadToolStripMenuItem.Image = Global.unPCB.My.Resources.Resources.throughhole_pad
        Me.ThroughHolePadToolStripMenuItem.Name = "ThroughHolePadToolStripMenuItem"
        Me.ThroughHolePadToolStripMenuItem.Size = New System.Drawing.Size(161, 22)
        Me.ThroughHolePadToolStripMenuItem.Text = "Through Hole Pad"
        '
        'ViaToolStripMenuItem
        '
        Me.ViaToolStripMenuItem.Image = CType(resources.GetObject("ViaToolStripMenuItem.Image"), System.Drawing.Image)
        Me.ViaToolStripMenuItem.Name = "ViaToolStripMenuItem"
        Me.ViaToolStripMenuItem.Size = New System.Drawing.Size(161, 22)
        Me.ViaToolStripMenuItem.Text = "Via"
        '
        'RouteToolStripMenuItem
        '
        Me.RouteToolStripMenuItem.Image = Global.unPCB.My.Resources.Resources.route
        Me.RouteToolStripMenuItem.Name = "RouteToolStripMenuItem"
        Me.RouteToolStripMenuItem.Size = New System.Drawing.Size(161, 22)
        Me.RouteToolStripMenuItem.Text = "Route"
        Me.RouteToolStripMenuItem.ToolTipText = "Connects pads and vias by folowing the route on the PCB."
        '
        'RouteJunctionToolStripMenuItem
        '
        Me.RouteJunctionToolStripMenuItem.Image = Global.unPCB.My.Resources.Resources.junction
        Me.RouteJunctionToolStripMenuItem.Name = "RouteJunctionToolStripMenuItem"
        Me.RouteJunctionToolStripMenuItem.Size = New System.Drawing.Size(161, 22)
        Me.RouteJunctionToolStripMenuItem.Text = "Route Junction"
        '
        'SplitRouteToolStripMenuItem
        '
        Me.SplitRouteToolStripMenuItem.Image = Global.unPCB.My.Resources.Resources.split
        Me.SplitRouteToolStripMenuItem.Name = "SplitRouteToolStripMenuItem"
        Me.SplitRouteToolStripMenuItem.Size = New System.Drawing.Size(161, 22)
        Me.SplitRouteToolStripMenuItem.Text = "Split Route"
        '
        'ToolStripButtonConnect
        '
        Me.ToolStripButtonConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonConnect.Image = Global.unPCB.My.Resources.Resources.connect
        Me.ToolStripButtonConnect.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonConnect.Name = "ToolStripButtonConnect"
        Me.ToolStripButtonConnect.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonConnect.Text = "ToolStripButton1"
        Me.ToolStripButtonConnect.ToolTipText = "Connect the selected pads"
        '
        'ToolStripDisconnect
        '
        Me.ToolStripDisconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripDisconnect.Image = Global.unPCB.My.Resources.Resources.disconnect
        Me.ToolStripDisconnect.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripDisconnect.Name = "ToolStripDisconnect"
        Me.ToolStripDisconnect.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripDisconnect.ToolTipText = "Disconnect the selected pads."
        '
        'ToolStripButtonUnconnect
        '
        Me.ToolStripButtonUnconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonUnconnect.Image = Global.unPCB.My.Resources.Resources.unconnect
        Me.ToolStripButtonUnconnect.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonUnconnect.Name = "ToolStripButtonUnconnect"
        Me.ToolStripButtonUnconnect.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonUnconnect.ToolTipText = "Selected Pads are never connected."
        '
        'ToolStripButtonConnectionFinished
        '
        Me.ToolStripButtonConnectionFinished.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonConnectionFinished.Image = Global.unPCB.My.Resources.Resources.connection_finished
        Me.ToolStripButtonConnectionFinished.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonConnectionFinished.Name = "ToolStripButtonConnectionFinished"
        Me.ToolStripButtonConnectionFinished.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonConnectionFinished.ToolTipText = "Connect the selected pads with each other and pads connected to them then NOT con" & _
            "nect to the deselected."
        '
        'ToolStripButtonAutoRoute
        '
        Me.ToolStripButtonAutoRoute.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonAutoRoute.Image = Global.unPCB.My.Resources.Resources.auto_router
        Me.ToolStripButtonAutoRoute.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonAutoRoute.Name = "ToolStripButtonAutoRoute"
        Me.ToolStripButtonAutoRoute.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButtonAutoRoute.Text = "Manual Router"
        '
        'FrmLayer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(727, 444)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.DoubleBuffer)
        Me.Controls.Add(Me.Toolbar)
        Me.Controls.Add(Me.MenuBar)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuBar
        Me.Name = "FrmLayer"
        Me.Text = "Layer"
        Me.MenuBar.ResumeLayout(False)
        Me.MenuBar.PerformLayout()
        Me.Toolbar.ResumeLayout(False)
        Me.Toolbar.PerformLayout()
        Me.MenuRightClickObject.ResumeLayout(False)
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents MenuBar As System.Windows.Forms.MenuStrip
    Friend WithEvents MenuFile As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MenuExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Toolbar As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripSplitButtonDrawPad As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents SMDPadToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ThroughHolePadToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DoubleBuffer As unPCB.DoubleBuffer
    Friend WithEvents ToolStripButtonPointer As System.Windows.Forms.ToolStripButton
    Friend WithEvents MenuOpen As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MenuSave As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripButtonConnect As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripDisconnect As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButtonUnconnect As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButtonAutoRoute As System.Windows.Forms.ToolStripButton
    Friend WithEvents MenuRightClickObject As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents Menu_SelectConnectedPads As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents Menu_Properties As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Menu_deleteObject As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripButtonUndo As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButtonRedo As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents BackgroundToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MoveToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FlipHorizontallyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FlipVerticallyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LoadToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenFileDialog As System.Windows.Forms.OpenFileDialog
    Friend WithEvents MoveObjectToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents SaveAsMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripButtonConnectionFinished As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButtonMove As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripSeparator4 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripSeparator5 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripButtonResize As System.Windows.Forms.ToolStripButton
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents StatusBar As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ToolStripButtonText As System.Windows.Forms.ToolStripButton
    Friend WithEvents HighlightConnectedPadsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ViaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LayersToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RouteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RouteJunctionToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SplitRouteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
