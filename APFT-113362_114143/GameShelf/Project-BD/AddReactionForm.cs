using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Project_BD
{
    public partial class AddReactionForm : Form
    {
        private string userId;
        private string reviewId;
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public AddReactionForm(string userId, string reviewId)
        {
            InitializeComponent();
            this.userId = userId;
            this.reviewId = reviewId;
        }

        private SqlConnection getSGBDConnection()
        {
            return bdconnect.getSGBDConnection();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReaction.Text))
            {
                MessageBox.Show("Please enter your reaction");
                return;
            }

            try
            {
                cn = getSGBDConnection();
                if (cn.State != ConnectionState.Open)
                    cn.Open();

                string query = @"INSERT INTO projeto.reage_a 
                       (id_utilizador, id_review, reacao_texto, reacao_data)
                       VALUES (@userId, @reviewId, @reaction, GETDATE())";

                SqlCommand command = new SqlCommand(query, cn);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@reviewId", reviewId);
                command.Parameters.AddWithValue("@reaction", txtReaction.Text);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Reaction added successfully!");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding reaction: " + ex.Message);
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
        }
    }
}