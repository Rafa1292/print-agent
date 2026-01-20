namespace PrintAgent.UI.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // Main controls
        listViewPrinters = new ListView();
        colName = new ColumnHeader();
        colSystemName = new ColumnHeader();
        colWidth = new ColumnHeader();
        colType = new ColumnHeader();
        colDefault = new ColumnHeader();
        colStatus = new ColumnHeader();

        btnAdd = new Button();
        btnEdit = new Button();
        btnDelete = new Button();
        btnTest = new Button();
        btnRefresh = new Button();

        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();

        notifyIcon = new NotifyIcon(components);
        contextMenuStrip = new ContextMenuStrip(components);
        menuItemShow = new ToolStripMenuItem();
        menuItemSeparator = new ToolStripSeparator();
        menuItemExit = new ToolStripMenuItem();

        labelTitle = new Label();
        panelButtons = new Panel();

        // Suspend layout
        statusStrip.SuspendLayout();
        contextMenuStrip.SuspendLayout();
        panelButtons.SuspendLayout();
        SuspendLayout();

        // labelTitle
        labelTitle.AutoSize = true;
        labelTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        labelTitle.Location = new Point(12, 12);
        labelTitle.Name = "labelTitle";
        labelTitle.Size = new Size(250, 25);
        labelTitle.Text = "Impresoras Configuradas";

        // listViewPrinters
        listViewPrinters.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listViewPrinters.Columns.AddRange(new ColumnHeader[] { colName, colSystemName, colWidth, colType, colDefault, colStatus });
        listViewPrinters.FullRowSelect = true;
        listViewPrinters.GridLines = true;
        listViewPrinters.Location = new Point(12, 50);
        listViewPrinters.MultiSelect = false;
        listViewPrinters.Name = "listViewPrinters";
        listViewPrinters.Size = new Size(560, 280);
        listViewPrinters.TabIndex = 0;
        listViewPrinters.UseCompatibleStateImageBehavior = false;
        listViewPrinters.View = View.Details;
        listViewPrinters.SelectedIndexChanged += listViewPrinters_SelectedIndexChanged;
        listViewPrinters.DoubleClick += listViewPrinters_DoubleClick;

        // Column headers
        colName.Text = "Nombre";
        colName.Width = 100;
        colSystemName.Text = "Impresora Sistema";
        colSystemName.Width = 150;
        colWidth.Text = "Ancho";
        colWidth.Width = 60;
        colType.Text = "Tipo";
        colType.Width = 80;
        colDefault.Text = "Default";
        colDefault.Width = 60;
        colStatus.Text = "Estado";
        colStatus.Width = 90;

        // panelButtons
        panelButtons.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        panelButtons.Controls.Add(btnAdd);
        panelButtons.Controls.Add(btnEdit);
        panelButtons.Controls.Add(btnDelete);
        panelButtons.Controls.Add(btnTest);
        panelButtons.Controls.Add(btnRefresh);
        panelButtons.Location = new Point(578, 50);
        panelButtons.Name = "panelButtons";
        panelButtons.Size = new Size(100, 280);

        // btnAdd
        btnAdd.Location = new Point(0, 0);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(100, 35);
        btnAdd.TabIndex = 1;
        btnAdd.Text = "Agregar";
        btnAdd.UseVisualStyleBackColor = true;
        btnAdd.Click += btnAdd_Click;

        // btnEdit
        btnEdit.Enabled = false;
        btnEdit.Location = new Point(0, 45);
        btnEdit.Name = "btnEdit";
        btnEdit.Size = new Size(100, 35);
        btnEdit.TabIndex = 2;
        btnEdit.Text = "Editar";
        btnEdit.UseVisualStyleBackColor = true;
        btnEdit.Click += btnEdit_Click;

        // btnDelete
        btnDelete.Enabled = false;
        btnDelete.Location = new Point(0, 90);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(100, 35);
        btnDelete.TabIndex = 3;
        btnDelete.Text = "Eliminar";
        btnDelete.UseVisualStyleBackColor = true;
        btnDelete.Click += btnDelete_Click;

        // btnTest
        btnTest.Enabled = false;
        btnTest.Location = new Point(0, 150);
        btnTest.Name = "btnTest";
        btnTest.Size = new Size(100, 35);
        btnTest.TabIndex = 4;
        btnTest.Text = "Probar";
        btnTest.UseVisualStyleBackColor = true;
        btnTest.Click += btnTest_Click;

        // btnRefresh
        btnRefresh.Anchor = AnchorStyles.Bottom;
        btnRefresh.Location = new Point(0, 245);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(100, 35);
        btnRefresh.TabIndex = 5;
        btnRefresh.Text = "Actualizar";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += btnRefresh_Click;

        // statusStrip
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 339);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(690, 22);
        statusStrip.TabIndex = 6;

        // statusLabel
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(150, 17);
        statusLabel.Text = "Verificando servicio...";

        // contextMenuStrip
        contextMenuStrip.Items.AddRange(new ToolStripItem[] { menuItemShow, menuItemSeparator, menuItemExit });
        contextMenuStrip.Name = "contextMenuStrip";
        contextMenuStrip.Size = new Size(120, 54);

        // menuItemShow
        menuItemShow.Name = "menuItemShow";
        menuItemShow.Size = new Size(119, 22);
        menuItemShow.Text = "Mostrar";
        menuItemShow.Click += menuItemShow_Click;

        // menuItemSeparator
        menuItemSeparator.Name = "menuItemSeparator";
        menuItemSeparator.Size = new Size(116, 6);

        // menuItemExit
        menuItemExit.Name = "menuItemExit";
        menuItemExit.Size = new Size(119, 22);
        menuItemExit.Text = "Salir";
        menuItemExit.Click += menuItemExit_Click;

        // notifyIcon
        notifyIcon.ContextMenuStrip = contextMenuStrip;
        notifyIcon.Icon = SystemIcons.Application;
        notifyIcon.Text = "PrintAgent";
        notifyIcon.Visible = true;
        notifyIcon.DoubleClick += notifyIcon_DoubleClick;

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(690, 361);
        Controls.Add(labelTitle);
        Controls.Add(listViewPrinters);
        Controls.Add(panelButtons);
        Controls.Add(statusStrip);
        MinimumSize = new Size(600, 400);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PrintAgent - Configuración";
        FormClosing += MainForm_FormClosing;
        Load += MainForm_Load;
        Resize += MainForm_Resize;

        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        contextMenuStrip.ResumeLayout(false);
        panelButtons.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private ListView listViewPrinters;
    private ColumnHeader colName;
    private ColumnHeader colSystemName;
    private ColumnHeader colWidth;
    private ColumnHeader colType;
    private ColumnHeader colDefault;
    private ColumnHeader colStatus;

    private Button btnAdd;
    private Button btnEdit;
    private Button btnDelete;
    private Button btnTest;
    private Button btnRefresh;

    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;

    private NotifyIcon notifyIcon;
    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem menuItemShow;
    private ToolStripSeparator menuItemSeparator;
    private ToolStripMenuItem menuItemExit;

    private Label labelTitle;
    private Panel panelButtons;
}
