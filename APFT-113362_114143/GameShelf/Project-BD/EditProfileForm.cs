using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Project_BD
{
    public partial class EditProfileForm : Form
    {
        private string userId;
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public string UpdatedName { get; private set; }
        public string UpdatedBio { get; private set; }

        public EditProfileForm(string userId, string currentName, string currentBio)
        {
            InitializeComponent();
            this.userId = userId;

            textBoxName.Text = currentName;
            textBoxBio.Text = currentBio;
        }

        private SqlConnection getSGBDConnection()
        {
            return bdconnect.getSGBDConnection();
        }

        private bool verifySGBDConnection()
        {
            if (cn == null)
                cn = getSGBDConnection();
            if (cn.State != System.Data.ConnectionState.Open)
                cn.Open();
            return cn.State == System.Data.ConnectionState.Open;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string name = textBoxName.Text.Trim();
            string bio = textBoxBio.Text.Trim();

            try
            {
                cn = getSGBDConnection();
                if (!verifySGBDConnection())
                    return;

                // Update user name
                SqlCommand cmd = new SqlCommand("UPDATE projeto.utilizador SET nome = @name WHERE id_utilizador = @userId", cn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();

                // Update or insert bio
                string checkProfile = "IF EXISTS (SELECT 1 FROM projeto.perfil WHERE utilizador = @userId) " +
                                      "UPDATE projeto.perfil SET bio = @bio WHERE utilizador = @userId " +
                                      "ELSE INSERT INTO projeto.perfil (bio, utilizador) VALUES (@bio, @userId)";

                SqlCommand cmd2 = new SqlCommand(checkProfile, cn);
                cmd2.Parameters.AddWithValue("@bio", bio);
                cmd2.Parameters.AddWithValue("@userId", userId);
                cmd2.ExecuteNonQuery();

                // Set updated values and close with OK result
                UpdatedName = name;
                UpdatedBio = bio;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating profile: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }
    }
}
