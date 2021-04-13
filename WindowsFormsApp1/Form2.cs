using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;//設定button1為OK
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;//設定button為Cancel

            comboBox1.Items.Add("Anonymous");
            comboBox1.Items.Add("User/Password");
            comboBox1.SelectedIndex = 0;

        }
        public String url;
        public String user;
        public String password;

        public int loginMethod = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            url = textBox1.Text;
            user = textBox2.Text;
            password = textBox3.Text;
        }

        public void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
         
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "User/Password") 
            {
                loginMethod = 1;
                tableLayoutPanel7.Visible = true;
            }

            else if (comboBox1.SelectedItem.ToString() == "Anonymous")  
            {
                loginMethod = 0;
                tableLayoutPanel7.Visible = false;

            }
            else
            {
                tableLayoutPanel7.Visible = false;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
