using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.Json;

namespace Wiz_LAB3
{
    [Serializable]
    public class Osoba
    {
        public int ID { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public int Wiek { get; set; }
        public string Stanowisko { get; set; }
    }

    public partial class Form1 : Form
    {
        private DataGridView dataGridView1;
        private Button btnDodaj, btnUsun, btnZapisz, btnWczytaj, btnZapiszXML, btnZapiszJSON;
        private DataTable dataTable;
        private int currentId = 1;

        public Form1()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            this.Text = "Arkusz Pracowników";
            this.Size = new Size(800, 460);
            this.StartPosition = FormStartPosition.CenterScreen;

            dataGridView1 = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(600, 320),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnDodaj = new Button { Text = "Dodaj", Location = new Point(640, 20), Size = new Size(120, 35), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnUsun = new Button { Text = "Usuń", Location = new Point(640, 65), Size = new Size(120, 35), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            
            btnZapisz = new Button { Text = "Zapis do .csv", Location = new Point(20, 360), Size = new Size(110, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnWczytaj = new Button { Text = "odczyt z .csv", Location = new Point(140, 360), Size = new Size(110, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            btnZapiszJSON = new Button { Text = "Zapis do JSON", Location = new Point(380, 360), Size = new Size(110, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            
            this.Controls.AddRange(new Control[] { dataGridView1, btnDodaj, btnUsun, btnZapisz, btnWczytaj, btnZapiszXML, btnZapiszJSON });

            dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Imię", typeof(string));
            dataTable.Columns.Add("Nazwisko", typeof(string));
            dataTable.Columns.Add("Wiek", typeof(int));
            dataTable.Columns.Add("Stanowisko", typeof(string));

            dataGridView1.DataSource = dataTable;

            btnDodaj.Click += BtnDodaj_Click;
            btnUsun.Click += BtnUsun_Click;
            btnZapisz.Click += BtnZapisz_Click;
            btnWczytaj.Click += BtnWczytaj_Click;        
        }

        private List<Osoba> PobierzDaneZTabeli()
        {
            var listaOsob = new List<Osoba>();
            foreach (DataRow row in dataTable.Rows)
            {
                listaOsob.Add(new Osoba
                {
                    ID = Convert.ToInt32(row["ID"]),
                    Imie = row["Imię"].ToString(),
                    Nazwisko = row["Nazwisko"].ToString(),
                    Wiek = Convert.ToInt32(row["Wiek"]),
                    Stanowisko = row["Stanowisko"].ToString()
                });
            }
            return listaOsob;
        }

        private void BtnZapiszJSON_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Pliki JSON (*.json)|*.json", Title = "Zapisz jako JSON" })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    List<Osoba> dane = PobierzDaneZTabeli();

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(dane, options);

                    File.WriteAllText(saveFileDialog.FileName, jsonString, Encoding.UTF8);
                    MessageBox.Show("Dane zserializowano pomyślnie do pliku JSON.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnDodaj_Click(object sender, EventArgs e)
        {
            using (var form2 = new Form2())
            {
                if (form2.ShowDialog() == DialogResult.OK)
                {
                    dataTable.Rows.Add(currentId, form2.Imie, form2.Nazwisko, form2.Wiek, form2.Stanowisko);
                    currentId++;
                }
            }
        }

        private void BtnUsun_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    dataGridView1.Rows.Remove(row);
                }
            }
        }

        private void BtnZapisz_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog() { Filter = "Pliki CSV (*.csv)|*.csv", Title = "Wybierz lokalizację zapisu pliku CSV" })
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string header = string.Join(",", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                    var lines = dataTable.AsEnumerable().Select(row => string.Join(",", row.ItemArray));
                    File.WriteAllLines(saveFileDialog1.FileName, new string[] { header }.Concat(lines), Encoding.GetEncoding("Windows-1250"));
                }
            }
        }

        private void BtnWczytaj_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog() { Filter = "Pliki CSV (*.csv)|*.csv", Title = "Wybierz plik CSV do wczytania" })
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(openFileDialog1.FileName, Encoding.GetEncoding("Windows-1250"));
                    if (lines.Length > 0)
                    {
                        dataTable.Clear();
                        for (int i = 1; i < lines.Length; i++)
                        {
                            string[] values = lines[i].Split(',');
                            dataTable.Rows.Add(values[0], values[1], values[2], values[3], values[4]);
                        }

                        if (dataTable.Rows.Count > 0)
                        {
                            currentId = Convert.ToInt32(dataTable.Rows[dataTable.Rows.Count - 1]["ID"]) + 1;
                        }
                        else
                        {
                            currentId = 1;
                        }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }

    public class Form2 : Form
    {
        public string Imie { get; private set; }
        public string Nazwisko { get; private set; }
        public int Wiek { get; private set; }
        public string Stanowisko { get; private set; }

        private TextBox txtImie, txtNazwisko;
        private NumericUpDown numWiek;
        private ComboBox cbStanowisko;
        private Button btnZatwierdz, btnAnuluj;

        public Form2()
        {
            this.Text = "Dodaj pracownika";
            this.Size = new Size(350, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            txtImie = new TextBox { Location = new Point(20, 30), Width = 150 };
            Label lblImie = new Label { Text = "Imię", Location = new Point(190, 33), AutoSize = true };

            txtNazwisko = new TextBox { Location = new Point(20, 70), Width = 150 };
            Label lblNazwisko = new Label { Text = "Nazwisko", Location = new Point(190, 73), AutoSize = true };

            numWiek = new NumericUpDown { Location = new Point(20, 110), Width = 150, Minimum = 18, Maximum = 100 };
            Label lblWiek = new Label { Text = "Wiek", Location = new Point(190, 113), AutoSize = true };

            cbStanowisko = new ComboBox { Location = new Point(20, 150), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStanowisko.Items.AddRange(new string[] { "Programista", "Kierownik", "Księgowy" });
            cbStanowisko.SelectedIndex = 0;
            Label lblStanowisko = new Label { Text = "Stanowisko", Location = new Point(190, 153), AutoSize = true };

            btnZatwierdz = new Button { Text = "Zatwierdź", Location = new Point(20, 210), Size = new Size(100, 35) };
            btnZatwierdz.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtImie.Text) || string.IsNullOrWhiteSpace(txtNazwisko.Text))
                {
                    MessageBox.Show("Pola Imię i Nazwisko nie mogą być puste!", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Imie = txtImie.Text;
                Nazwisko = txtNazwisko.Text;
                Wiek = (int)numWiek.Value;
                Stanowisko = cbStanowisko.SelectedItem.ToString();

                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnAnuluj = new Button { Text = "Anuluj", Location = new Point(140, 210), Size = new Size(100, 35), DialogResult = DialogResult.Cancel };
            btnAnuluj.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                txtImie, lblImie,
                txtNazwisko, lblNazwisko,
                numWiek, lblWiek,
                cbStanowisko, lblStanowisko,
                btnZatwierdz, btnAnuluj
            });
        }
    }
}