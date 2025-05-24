using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;

namespace Project_BD
{
    public partial class CreateListForm : Form
    {
        private string currentUserId;
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();
        private CheckBox chkUsePositions;

        public CreateListForm(string userId)
        {
            InitializeComponent();
            currentUserId = userId;
            this.chkUsePositions = new CheckBox();
            this.chkUsePositions.AutoSize = true;
            this.chkUsePositions.Location = new System.Drawing.Point(15, 200);
            this.chkUsePositions.Text = "Use positions in this list";
            this.chkUsePositions.Checked = true;
            this.Controls.Add(this.chkUsePositions);
        }

        private SqlConnection getSGBDConnection()
        {
            return bdconnect.getSGBDConnection();
        }

        private bool verifySGBDConnection()
        {
            if (cn == null)
                cn = getSGBDConnection();

            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cn.State == ConnectionState.Open;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please enter a title for the list.");
                return;
            }

            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                string listId = GenerateListId();

                string query = @"INSERT INTO projeto.lista 
                (id_lista, titulo_lista, descricao_lista, visibilidade_lista, id_utilizador, usa_posicoes)
                VALUES 
                (@listId, @title, @description, @visibility, @userId, @usePositions)";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@listId", listId);
                command.Parameters.AddWithValue("@title", txtTitle.Text);
                command.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(txtDescription.Text) ? DBNull.Value : (object)txtDescription.Text);
                command.Parameters.AddWithValue("@visibility", rbPublic.Checked ? "Publica" : "Privada");
                command.Parameters.AddWithValue("@userId", currentUserId);
                command.Parameters.AddWithValue("@usePositions", chkUsePositions.Checked);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("List created successfully!");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to create list.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating list: " + ex.Message);
            }
            finally
            {
                if (cn != null && cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private string GenerateListId()
        {
            try
            {
                using (var tempCn = getSGBDConnection())
                {
                    tempCn.Open();

                    string query = "SELECT MAX(id_lista) FROM projeto.lista";
                    SqlCommand cmd = new SqlCommand(query, tempCn);
                    object result = cmd.ExecuteScalar();

                    if (result == DBNull.Value || result == null)
                    {
                        return "L001"; // First list
                    }

                    string maxId = result.ToString();
                    if (maxId.StartsWith("L") && int.TryParse(maxId.Substring(1), out int number))
                    {
                        return $"L{(number + 1):D3}"; // Increment and format
                    }

                    // Fallback for unexpected format
                    return "L" + Guid.NewGuid().ToString("N").Substring(0, 3);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating list ID: " + ex.Message);
                return "L" + DateTime.Now.Ticks.ToString().Substring(0, 10); // Fallback
            }
        }

    }
}