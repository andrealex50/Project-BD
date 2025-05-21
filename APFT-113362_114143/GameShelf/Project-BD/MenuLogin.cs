using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_BD
{
    public partial class MenuLogin : Form

    {
        private SqlConnection cn;
        public static ConnectionBD bdconnect = new ConnectionBD();

        public MenuLogin()
        {
            InitializeComponent();
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Menu_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (RegisterForm registerForm = new RegisterForm())
            {
                if (registerForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Account created successfully! Please log in.");
                }
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!verifySGBDConnection())
                {
                    MessageBox.Show("Erro na conexão com a base de dados.");
                    return;
                }

                string email = textBox2.Text.Trim();
                string password = textBox1.Text.Trim();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Por favor, preencha todos os campos.");
                    return;
                }

                // Hash da password em SHA256 (hexadecimal, tal como na base de dados)
                string hashedPassword = ComputeSha256Hash(password);

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT id_utilizador, nome FROM projeto.utilizador WHERE email = @Email AND password = @Password", cn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string userId = reader["id_utilizador"].ToString();
                            string nome = reader["nome"].ToString();

                            // Fecha o reader antes de abrir o novo formulário
                            reader.Close();

                            // Esconde o formulário de login
                            this.Hide();

                            // Cria e mostra a MainPage
                            MainPage mainPage = new MainPage(userId);
                            mainPage.Closed += (s, args) => this.Close(); // Fecha a aplicação quando a MainPage fechar
                            mainPage.Show();
                        }
                        else
                        {
                            MessageBox.Show("Email ou password incorretos.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro durante o login: {ex.Message}");
            }
            finally
            {
                cn.Close();
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("X2")); // hexadecimal em maiúsculas
                }
                return builder.ToString();
            }
        }

        // Gera algo tipo "U001", "U002"
        private string GenerateUniqueID()
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM projeto.utilizador", cn);
            int count = (int)cmd.ExecuteScalar();
            return "U" + (count + 1).ToString("D3");
        }
    }
}
