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
            if (!verifySGBDConnection())
                return;

            string email = textBox2.Text.Trim();
            string password = textBox1.Text.Trim();

            if (email == "" || password == "")
            {
                MessageBox.Show("Por favor, preencha todos os campos.");
                return;
            }

            string nome = "MANELCORSAS";  // FAZER DEPOIS UMA PAGINA QUE PEÇA O NOME NÉ                TODO
            string id_utilizador = GenerateUniqueID(); // método que gera um ID único (U001, U002...)

            string hashedPassword = ComputeSha256Hash(password);

            try
            {
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO projeto.utilizador (id_utilizador, nome, email, password) " +
                    "VALUES (@Id, @Nome, @Email, @Password)", cn);

                cmd.Parameters.AddWithValue("@Id", id_utilizador);
                cmd.Parameters.AddWithValue("@Nome", nome);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                    MessageBox.Show("Conta criada com sucesso!");
                else
                    MessageBox.Show("Erro ao criar conta.");
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Erro SQL: " + ex.Message);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!verifySGBDConnection())
                return;

            string email = textBox2.Text.Trim();
            string password = textBox1.Text.Trim();

            if (email == "" || password == "")
            {
                MessageBox.Show("Por favor, preencha todos os campos.");
                return;
            }

            // Hash da password em SHA256 (hexadecimal, tal como na base de dados)
            string hashedPassword = ComputeSha256Hash(password);

            SqlCommand cmd = new SqlCommand(
                "SELECT * FROM projeto.utilizador WHERE email = @Email AND password = @Password", cn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Password", hashedPassword);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string nome = reader["nome"].ToString();
                MessageBox.Show($"Bem-vindo, {nome}");
            }
            else
            {
                MessageBox.Show("Email ou password incorretos.");
            }

            reader.Close();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

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
