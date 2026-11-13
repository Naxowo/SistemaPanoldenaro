using Microsoft.Data.SqlClient;

namespace SistemaPanol;

public class MainForm : Form
{
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly DataGridView toolsGrid = Grid();
    private readonly DataGridView studentsGrid = Grid();
    private readonly DataGridView loansGrid = Grid();
    private readonly DataGridView reportsGrid = Grid();

    public MainForm()
    {
        Text = "Gestión de Pañol Escolar";
        Width = 1050;
        Height = 680;
        StartPosition = FormStartPosition.CenterScreen;

        tabs.TabPages.Add(BuildToolsTab());
        tabs.TabPages.Add(BuildStudentsTab());
        tabs.TabPages.Add(BuildLoansTab());
        tabs.TabPages.Add(BuildReportsTab());
        Controls.Add(tabs);

        Load += (_, _) => RefreshAll();
    }

    private static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false
    };

    private TabPage BuildToolsTab()
    {
        var page = new TabPage("Herramientas e inventario");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
        var name = Input("Herramienta");
        var stock = Input("Stock");
        var state = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        state.Items.AddRange(["Disponible", "En reparación", "Dañada"]);
        state.SelectedIndex = 0;

        panel.Controls.AddRange([
            new Label { Text = "Nombre:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, name,
            new Label { Text = "Cantidad:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, stock,
            new Label { Text = "Estado:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, state,
            Button("Agregar herramienta", (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(name.Text) || !int.TryParse(stock.Text, out int quantity) || quantity < 0)
                { MessageBox.Show("Ingresá un nombre y una cantidad válida."); return; }
                Run(() => Database.Execute(
                    "INSERT INTO Herramientas(Nombre, Stock, Estado) VALUES(@nombre,@stock,@estado)",
                    Database.Param("@nombre", name.Text.Trim()), Database.Param("@stock", quantity),
                    Database.Param("@estado", state.Text)));
                name.Clear(); stock.Clear(); RefreshAll();
            })
        ]);
        page.Controls.Add(toolsGrid);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage BuildStudentsTab()
    {
        var page = new TabPage("Alumnos");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
        var name = Input("Apellido y nombre");
        var course = Input("Curso");
        panel.Controls.AddRange([
            new Label { Text = "Alumno:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, name,
            new Label { Text = "Curso:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, course,
            Button("Registrar alumno", (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(course.Text))
                { MessageBox.Show("Completá el nombre y el curso."); return; }
                Run(() => Database.Execute(
                    "INSERT INTO Alumnos(Nombre, Curso) VALUES(@nombre,@curso)",
                    Database.Param("@nombre", name.Text.Trim()), Database.Param("@curso", course.Text.Trim())));
                name.Clear(); course.Clear(); RefreshAll();
            })
        ]);
        page.Controls.Add(studentsGrid);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage BuildLoansTab()
    {
        var page = new TabPage("Préstamos y devoluciones");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(8) };
        var studentId = Input("ID alumno");
        var toolId = Input("ID herramienta");
        var quantity = Input("Cantidad");
        panel.Controls.AddRange([
            new Label { Text = "ID alumno:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, studentId,
            new Label { Text = "ID herramienta:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, toolId,
            new Label { Text = "Cantidad:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, quantity,
            Button("Registrar préstamo", (_, _) =>
            {
                if (!int.TryParse(studentId.Text, out int sid) ||
                    !int.TryParse(toolId.Text, out int tid) ||
                    !int.TryParse(quantity.Text, out int qty) || qty <= 0)
                { MessageBox.Show("Ingresá IDs y cantidad válidos."); return; }
                Run(() => CreateLoan(sid, tid, qty));
                RefreshAll();
            }),
            Button("Devolver préstamo seleccionado", (_, _) =>
            {
                if (loansGrid.CurrentRow?.Cells["PrestamoID"].Value is not object id) return;
                Run(() => ReturnLoan(Convert.ToInt32(id)));
                RefreshAll();
            })
        ]);
        page.Controls.Add(loansGrid);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage BuildReportsTab()
    {
        var page = new TabPage("Daños e historial");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8) };
        var loanId = Input("ID préstamo");
        var detail = Input("Detalle del daño");
        panel.Controls.AddRange([
            new Label { Text = "ID préstamo:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, loanId,
            new Label { Text = "Detalle:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, detail,
            Button("Informar daño", (_, _) =>
            {
                if (!int.TryParse(loanId.Text, out int id) || string.IsNullOrWhiteSpace(detail.Text))
                { MessageBox.Show("Ingresá el ID del préstamo y el detalle."); return; }
                Run(() => ReportDamage(id, detail.Text.Trim()));
                RefreshAll();
            }),
            Button("Actualizar", (_, _) => RefreshAll())
        ]);
        page.Controls.Add(reportsGrid);
        page.Controls.Add(panel);
        return page;
    }

    private static TextBox Input(string placeholder) => new()
    {
        Width = 130,
        PlaceholderText = placeholder
    };

    private static Button Button(string text, EventHandler action)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += action;
        return button;
    }

    private void CreateLoan(int studentId, int toolId, int quantity)
    {
        using var connection = Database.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var check = new SqlCommand(
                "SELECT Stock FROM Herramientas WHERE HerramientaID=@id AND Estado='Disponible'", connection, transaction);
            check.Parameters.AddWithValue("@id", toolId);
            var result = check.ExecuteScalar();
            if (result is null) throw new Exception("La herramienta no existe o no está disponible.");
            if (Convert.ToInt32(result) < quantity) throw new Exception("Stock insuficiente.");

            using var student = new SqlCommand("SELECT COUNT(*) FROM Alumnos WHERE AlumnoID=@id", connection, transaction);
            student.Parameters.AddWithValue("@id", studentId);
            if (Convert.ToInt32(student.ExecuteScalar()) == 0) throw new Exception("El alumno no existe.");

            using var update = new SqlCommand(
                "UPDATE Herramientas SET Stock=Stock-@cantidad WHERE HerramientaID=@id", connection, transaction);
            update.Parameters.AddWithValue("@cantidad", quantity);
            update.Parameters.AddWithValue("@id", toolId);
            update.ExecuteNonQuery();

            using var insert = new SqlCommand(
                "INSERT INTO Prestamos(AlumnoID,HerramientaID,Cantidad,FechaPrestamo,Estado) " +
                "VALUES(@alumno,@herramienta,@cantidad,GETDATE(),'Pendiente')", connection, transaction);
            insert.Parameters.AddWithValue("@alumno", studentId);
            insert.Parameters.AddWithValue("@herramienta", toolId);
            insert.Parameters.AddWithValue("@cantidad", quantity);
            insert.ExecuteNonQuery();
            transaction.Commit();
            MessageBox.Show("Préstamo registrado correctamente.");
        }
        catch { transaction.Rollback(); throw; }
    }

    private void ReturnLoan(int loanId)
    {
        using var connection = Database.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var get = new SqlCommand(
                "SELECT HerramientaID,Cantidad,Estado FROM Prestamos WHERE PrestamoID=@id", connection, transaction);
            get.Parameters.AddWithValue("@id", loanId);
            using var reader = get.ExecuteReader();
            if (!reader.Read()) throw new Exception("No se encontró el préstamo.");
            int toolId = reader.GetInt32(0);
            int quantity = reader.GetInt32(1);
            string status = reader.GetString(2);
            reader.Close();
            if (status == "Devuelto") throw new Exception("Ese préstamo ya fue devuelto.");

            using var updateLoan = new SqlCommand(
                "UPDATE Prestamos SET Estado='Devuelto',FechaDevolucion=GETDATE() WHERE PrestamoID=@id", connection, transaction);
            updateLoan.Parameters.AddWithValue("@id", loanId);
            updateLoan.ExecuteNonQuery();

            using var updateTool = new SqlCommand(
                "UPDATE Herramientas SET Stock=Stock+@cantidad WHERE HerramientaID=@id", connection, transaction);
            updateTool.Parameters.AddWithValue("@cantidad", quantity);
            updateTool.Parameters.AddWithValue("@id", toolId);
            updateTool.ExecuteNonQuery();

            transaction.Commit();
            MessageBox.Show("Devolución registrada. El stock fue actualizado.");
        }
        catch { transaction.Rollback(); throw; }
    }

    private void ReportDamage(int loanId, string detail)
    {
        Database.Execute(
            "INSERT INTO Danios(PrestamoID,Detalle,Fecha) VALUES(@id,@detalle,GETDATE())",
            Database.Param("@id", loanId), Database.Param("@detalle", detail));
        MessageBox.Show("Daño informado correctamente.");
    }

    private void RefreshAll()
    {
        Run(() =>
        {
            toolsGrid.DataSource = Database.Query(
                "SELECT HerramientaID,Nombre,Stock,Estado FROM Herramientas ORDER BY Nombre");
            studentsGrid.DataSource = Database.Query(
                "SELECT AlumnoID,Nombre,Curso FROM Alumnos ORDER BY Nombre");
            loansGrid.DataSource = Database.Query(
                "SELECT p.PrestamoID,a.Nombre AS Alumno,h.Nombre AS Herramienta,p.Cantidad," +
                "p.FechaPrestamo,p.FechaDevolucion,p.Estado FROM Prestamos p " +
                "JOIN Alumnos a ON a.AlumnoID=p.AlumnoID JOIN Herramientas h ON h.HerramientaID=p.HerramientaID " +
                "ORDER BY p.PrestamoID DESC");
            reportsGrid.DataSource = Database.Query(
                "SELECT d.DanioID,d.PrestamoID,d.Detalle,d.Fecha FROM Danios d ORDER BY d.Fecha DESC");
        });
    }

    private static void Run(Action action)
    {
        try { action(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
}
