using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ToDoList
{
    public partial class ToDoListForm : Form
    {
        private int rowId = 0;
        private int rowIndex = 0;

        SqlConnection cn;
        SqlCommand cmd;
        SqlDataAdapter da;

        public ToDoListForm()
        {
            InitializeComponent();
            this.Load += ToDoListForm_Load;
        }

        private void ToDoListForm_Load(object sender, System.EventArgs e)
        {
            dataGridViewTasks.RowTemplate.ContextMenuStrip = contextMenuStrip1;
            dataGridViewTasks.Select();
            OpenDbConnection();
            GetAllTasksRecords();
            CloseDbConnection();
        }

        private void dataGridViewTasks_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                dataGridViewTasks.Rows[e.RowIndex].Selected = true;
                rowIndex = e.RowIndex;
                dataGridViewTasks.CurrentCell = dataGridViewTasks.Rows[e.RowIndex].Cells[0];
                Int32.TryParse(dataGridViewTasks.CurrentCell.FormattedValue.ToString(), out rowId);
                contextMenuStrip1.Show(dataGridViewTasks, e.Location);
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void deleteTaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.dataGridViewTasks.Rows[this.rowIndex].IsNewRow)
            {
                this.dataGridViewTasks.Rows.RemoveAt(this.rowIndex);

                OpenDbConnection();
                RemoveTaskRecord(rowId);
                CloseDbConnection();
            }
        }

        private void dataGridViewTasks_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            var dataGrid = (DataGridView)sender;
            if (e.Button == MouseButtons.Right && e.RowIndex != -1)
            {
                var row = dataGrid.Rows[e.RowIndex];
                dataGrid.CurrentCell = row.Cells[e.ColumnIndex == -1 ? 1 : e.ColumnIndex];
                row.Selected = true;
                dataGrid.Focus();
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxTaskDescription.Text))
            {
                OpenDbConnection();
                AddTaskRecord();
                GetAllTasksRecords();
                CloseDbConnection();
            }
            else
            {
                MessageBox.Show("Task description can't be empty or whitespace");
                textBoxTaskDescription.Focus();
            }
        }

        private void OpenDbConnection()
        {
            var myDbPathOnDisk = Program.Configuration.GetSection("MainSettings:LocalPathToTheDatabaseFilesFolder").Value;
            cn = new SqlConnection($@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={myDbPathOnDisk}Database.mdf;Integrated Security=True");
            cn.Open();
        }

        private void CloseDbConnection()
        {
            cn.Close();
        }

        private void GetAllTasksRecords()
        {
            cmd = new SqlCommand("Select * from Tasks", cn);
            da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            dataGridViewTasks.DataSource = dt;
            var column = dataGridViewTasks.Columns[1];
            column.Width = 610;
        }

        private void AddTaskRecord()
        {
            cmd = new SqlCommand("Insert into Tasks (TaskDescription) Values (@TaskDescription)", cn);
            cmd.Parameters.AddWithValue("@TaskDescription", textBoxTaskDescription.Text.Trim());
            cmd.ExecuteNonQuery();
            textBoxTaskDescription.Text = string.Empty;
        }

        private void RemoveTaskRecord(int id)
        {
            cmd = new SqlCommand("Delete from Tasks where Id = @Id", cn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            MessageBox.Show($"Task with the Id={id} was removed successfully");
        }
    }
}