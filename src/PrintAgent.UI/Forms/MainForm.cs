using PrintAgent.UI.Models;
using PrintAgent.UI.Services;

namespace PrintAgent.UI.Forms;

public partial class MainForm : Form
{
    private readonly PrintAgentClient _client;
    private List<PrinterInfo> _printers = new();
    private System.Windows.Forms.Timer _statusTimer;

    public MainForm()
    {
        InitializeComponent();
        _client = new PrintAgentClient();
        _statusTimer = new System.Windows.Forms.Timer();
        _statusTimer.Interval = 5000; // Check every 5 seconds
        _statusTimer.Tick += StatusTimer_Tick;
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        await RefreshStatus();
        await RefreshPrinters();
        _statusTimer.Start();
    }

    private async void StatusTimer_Tick(object? sender, EventArgs e)
    {
        await RefreshStatus();
    }

    private async Task RefreshStatus()
    {
        var health = await _client.GetHealthAsync();
        if (health != null)
        {
            statusLabel.Text = $"Servicio activo - v{health.Version}";
            statusLabel.ForeColor = Color.Green;
            notifyIcon.Text = "PrintAgent - Activo";
            notifyIcon.Icon = SystemIcons.Application;
        }
        else
        {
            statusLabel.Text = "Servicio no disponible";
            statusLabel.ForeColor = Color.Red;
            notifyIcon.Text = "PrintAgent - Inactivo";
        }
    }

    private async Task RefreshPrinters()
    {
        _printers = await _client.GetPrintersAsync();

        listViewPrinters.Items.Clear();
        foreach (var printer in _printers)
        {
            var item = new ListViewItem(printer.Name);
            item.SubItems.Add(printer.SystemName);
            item.SubItems.Add(printer.PaperWidth.ToString());
            item.SubItems.Add(printer.Type);
            item.SubItems.Add(printer.IsDefault ? "Sí" : "No");
            item.SubItems.Add(printer.IsOnline ? "En línea" : "Desconectada");
            item.Tag = printer;

            if (!printer.IsOnline)
            {
                item.ForeColor = Color.Gray;
            }

            listViewPrinters.Items.Add(item);
        }

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        bool hasSelection = listViewPrinters.SelectedItems.Count > 0;
        btnEdit.Enabled = hasSelection;
        btnDelete.Enabled = hasSelection;
        btnTest.Enabled = hasSelection;
    }

    private async void btnAdd_Click(object sender, EventArgs e)
    {
        var systemPrinters = await _client.GetSystemPrintersAsync();
        using var form = new PrinterConfigForm(systemPrinters);

        if (form.ShowDialog() == DialogResult.OK && form.PrinterConfig != null)
        {
            var result = await _client.AddPrinterAsync(form.PrinterConfig);
            if (result.Success)
            {
                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshPrinters();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void btnEdit_Click(object sender, EventArgs e)
    {
        if (listViewPrinters.SelectedItems.Count == 0) return;

        if (listViewPrinters.SelectedItems[0].Tag is not PrinterInfo selectedPrinter) return;
        var systemPrinters = await _client.GetSystemPrintersAsync();

        using var form = new PrinterConfigForm(systemPrinters, selectedPrinter);

        if (form.ShowDialog() == DialogResult.OK && form.PrinterConfig != null)
        {
            var result = await _client.UpdatePrinterAsync(selectedPrinter.Name, form.PrinterConfig);
            if (result.Success)
            {
                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshPrinters();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void btnDelete_Click(object sender, EventArgs e)
    {
        if (listViewPrinters.SelectedItems.Count == 0) return;

        if (listViewPrinters.SelectedItems[0].Tag is not PrinterInfo selectedPrinter) return;

        var confirm = MessageBox.Show(
            $"¿Estás seguro de eliminar la impresora '{selectedPrinter.Name}'?",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            var result = await _client.DeletePrinterAsync(selectedPrinter.Name);
            if (result.Success)
            {
                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshPrinters();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void btnTest_Click(object sender, EventArgs e)
    {
        if (listViewPrinters.SelectedItems.Count == 0) return;

        if (listViewPrinters.SelectedItems[0].Tag is not PrinterInfo selectedPrinter) return;

        btnTest.Enabled = false;
        btnTest.Text = "Imprimiendo...";

        try
        {
            var result = await _client.TestPrintAsync(selectedPrinter.Name);
            if (result.Success)
            {
                MessageBox.Show("Página de prueba enviada correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            btnTest.Text = "Probar";
            UpdateButtonStates();
        }
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        await RefreshStatus();
        await RefreshPrinters();
    }

    private void listViewPrinters_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateButtonStates();
    }

    private void listViewPrinters_DoubleClick(object sender, EventArgs e)
    {
        if (listViewPrinters.SelectedItems.Count > 0)
        {
            btnEdit_Click(sender, e);
        }
    }

    // Minimize to tray
    private void MainForm_Resize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
            notifyIcon.Visible = true;
        }
    }

    private void notifyIcon_DoubleClick(object sender, EventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
        notifyIcon.Visible = true;
    }

    private void menuItemShow_Click(object sender, EventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
    }

    private void menuItemExit_Click(object sender, EventArgs e)
    {
        notifyIcon.Visible = false;
        Application.Exit();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(2000, "PrintAgent", "La aplicación sigue ejecutándose en segundo plano", ToolTipIcon.Info);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _statusTimer?.Dispose();
            _client?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}
