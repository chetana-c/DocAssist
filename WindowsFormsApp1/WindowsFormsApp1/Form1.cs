using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class frmMain : Form
    {
        DataTable dt1 = new DataTable();
        DataTable dt2 = new DataTable();
        DataTable dt3 = new DataTable();
        DataTable dt4 = new DataTable();
        DataTable dt5 = new DataTable();
        DataTable dt6 = new DataTable();
        String text1;
        DataTable responseObj;
        int i = 0;
        int record = 0;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'sYMPTOMS.distinct_symtoms' table. You can move, or remove it, as needed.
            String Query = "SELECT code_sym, symptom_desc FROM     [Affordability_Sandbox].[distinct_symtoms](NOLOCK) order by 2;";
            dt1 = GetData(Query).Tables[0];

            DataRow row = dt1.NewRow();
            row[0] = "";
            dt1.Rows.InsertAt(row, 0);

            dt2 = dt1;
            dt3 = dt1;
            dt4 = dt1;
            dt5 = dt1;
            dt6 = dt1;

            cmbSymptom1.BindingContext = new BindingContext();
            cmbSymptom1.DataSource = dt1;
            cmbSymptom1.DisplayMember = "symptom_desc";
            cmbSymptom1.ValueMember = "code_sym";
            cmbSymptom1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbSymptom1.AutoCompleteSource = AutoCompleteSource.ListItems;

            cmbSymptom2.BindingContext = new BindingContext();
            cmbSymptom2.DataSource = dt2;
            cmbSymptom2.DisplayMember = "symptom_desc";
            cmbSymptom2.ValueMember = "code_Sym";
            cmbSymptom2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbSymptom2.AutoCompleteSource = AutoCompleteSource.ListItems;

            cmbSymptom3.BindingContext = new BindingContext();
            cmbSymptom3.DataSource = dt3;
            cmbSymptom3.DisplayMember = "symptom_desc";
            cmbSymptom3.ValueMember = "code_Sym";
            cmbSymptom3.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbSymptom3.AutoCompleteSource = AutoCompleteSource.ListItems;

            cmbSymptom4.BindingContext = new BindingContext();
            cmbSymptom4.DataSource = dt4;
            cmbSymptom4.DisplayMember = "symptom_desc";
            cmbSymptom4.ValueMember = "code_Sym";
            cmbSymptom4.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbSymptom4.AutoCompleteSource = AutoCompleteSource.ListItems;

            cmbSymptom5.BindingContext = new BindingContext();
            cmbSymptom5.DataSource = dt5;
            cmbSymptom5.DisplayMember = "symptom_desc";
            cmbSymptom5.ValueMember = "code_Sym";
            cmbSymptom5.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbSymptom5.AutoCompleteSource = AutoCompleteSource.ListItems;

            cmbSymptom6.BindingContext = new BindingContext();
            cmbSymptom6.DataSource = dt6;
            cmbSymptom6.DisplayMember = "symptom_desc";
            cmbSymptom6.ValueMember = "code_Sym";
            cmbSymptom6.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbSymptom6.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        private DataSet GetData(String Query)
        {
            DataSet Result = new DataSet();
            try
            {
                Result = DAL.DAL.GetData(Query);
                return Result;
            }
            catch (Exception ex)
            {
                this.FindForm().Enabled = true;
                Application.UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
                MessageBox.Show("An error occured while getting the dropdown options. " + ex.Message);
                this.Refresh();
                Query = null;
                Result.Dispose();
                return null;
            }
        }

        private void btnStartRecording_Click(object sender, EventArgs e)
        {
            record = 1;
            label9.Text = "Recording started...";
            
                SpeechSynthesizer sythesizer = new SpeechSynthesizer();
                sythesizer.Speak("Recording started");
                SpeechRecognizer sre = new SpeechRecognizer();
                //GrammarBuilder gb = new GrammarBuilder("hello computer");
                //Grammar gr = new Grammar(gb);
                sre.LoadGrammar(new DictationGrammar());
                sre.SpeechRecognized += sr_SpeechRecognized;
                
                

        }
        void sr_SpeechRecognized(object sender1, SpeechRecognizedEventArgs e1)
        {
            txtAuto.Text = txtAuto.Text + " " + e1.Result.Text;
            if (record == 0)
            {
                label9.Text = "Recording stopped...";
                return;
            }
        }

        private void btnDiagnose_Click(object sender, EventArgs e)
        {
            SpeechSynthesizer sythesizer = new SpeechSynthesizer();
            if (tabControl1.SelectedIndex==2)
            {
                sythesizer.Speak("Recording stopped");
            }            
            label9.Text = "Recording stopped...";
            this.FindForm().Enabled = false;
            Application.UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;
            this.Refresh();
            getAPIResponseAsync();
            Thread.Sleep(2000);
            DataTable dt = new DataTable();
            dataGridView3.DataSource = null;
            dataGridView3.AutoGenerateColumns = false;
            dataGridView3.Columns.Clear();
            dataGridView3.Refresh();
            dataGridView2.DataSource = null;
            dataGridView2.AutoGenerateColumns = false;
            dataGridView2.Columns.Clear();
            dataGridView2.Refresh();
            dt.Clear();
            dt.Columns.Clear();
            using (StreamReader sr = new StreamReader("C:\\Users\\srai20\\Documents\\Python Scripts\\GetDiseaseFromSym.csv"))
            {
                string header = sr.ReadLine();
                if (header == "code_disease,disease_desc,disease_sym_occur_count,sym_occur_count,prob_x,prob_y,score")
                {
                    dt.Columns.Add("code_disease");
                    dt.Columns.Add("disease_desc");

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        string[] fields = line.Split(',');
                        DataRow dr = dt.NewRow();
                            dr[0] = fields[0];
                            dr[1] = fields[1];
                        dt.Rows.Add(dr);
                    }
                    if (dt.Rows.Count<1)
                    {
                        MessageBox.Show("Could not find related disease.");
                    }
                    DataGridViewTextBoxColumn Diseases = new DataGridViewTextBoxColumn()
                    {
                        DataPropertyName = "disease_desc",
                        HeaderText = "Diseases",
                        ReadOnly = true,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                    };
                    DataGridViewTextBoxColumn Code = new DataGridViewTextBoxColumn()
                    {
                        DataPropertyName = "code_disease",
                        HeaderText = "Code",
                        ReadOnly = true,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                    };
                    dataGridView3.Columns.Add(Diseases);
                    dataGridView3.Columns.Add(Code);
                    dataGridView3.DataSource = dt;
                    dataGridView3.Refresh();
                    this.FindForm().Enabled = true;
                    Application.UseWaitCursor = false;
                    Cursor.Current = Cursors.Default;
                    this.Refresh();
                }
            }
                
        }

        private  async Task getAPIResponseAsync()
        {            
            keys k1 = new keys();
            if (cmbSymptom1.SelectedValue != "")
            {
                k1.sym1 = cmbSymptom1.SelectedValue.ToString();
            }
            if (cmbSymptom2.SelectedValue != "")
            {
                k1.sym2 = cmbSymptom2.SelectedValue.ToString();
            }
            if (cmbSymptom3.SelectedValue != "")
            {
                k1.sym3 = cmbSymptom3.SelectedValue.ToString();
            }
            if (cmbSymptom4.SelectedValue != "")
            {
                k1.sym4 = cmbSymptom4.SelectedValue.ToString();
            }
            if (cmbSymptom5.SelectedValue != "")
            {
                k1.sym5 = cmbSymptom5.SelectedValue.ToString();
            }
            if (cmbSymptom6.SelectedValue != "")
            {
                k1.sym6 = cmbSymptom6.SelectedValue.ToString();
            }

            var stringPayload = JsonConvert.SerializeObject(k1);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var httpClient = new HttpClient();
            var httpResponse = await httpClient.PostAsync("http://10.73.43.99:105/GetDiseaseFromSym", httpContent).ConfigureAwait(false);
            text1 = httpResponse.ToString();            
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            this.FindForm().Enabled = false;
            Application.UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;
            this.Refresh();
            DataTable dt = new DataTable();
            dataGridView2.DataSource = null;
            dataGridView2.AutoGenerateColumns = false;
            dataGridView2.Columns.Clear();
            dataGridView2.Refresh();
            dt.Clear();
            dt.Columns.Clear();
            i = e.RowIndex;
            using (StreamReader sr = new StreamReader("C:\\Users\\srai20\\Documents\\Python Scripts\\GetSymFromDisease.csv"))
            {
                string header = sr.ReadLine();
                if (header == "code_sym,symptom_desc,disease_sym_occur_count,disease_occur_count,prob,code_disease")
                {
                    dt.Columns.Add("code_sym");
                    dt.Columns.Add("symptom_desc");

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        string[] fields = line.Split(',');
                        if (dataGridView3.Rows[e.RowIndex].Cells[1].Value.ToString()== fields[5])
                        {
                            DataRow dr = dt.NewRow();
                            dr[0] = fields[0];
                            dr[1] = fields[1];
                            dt.Rows.Add(dr);
                        }                        
                    }
                    if (dt.Rows.Count < 1)
                    {
                        MessageBox.Show("Could not find related symptoms.");
                    }
                    DataGridViewTextBoxColumn code = new DataGridViewTextBoxColumn()
                    {
                        DataPropertyName = "code_sym",
                        HeaderText = "Code",
                        ReadOnly = true,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                    };
                    DataGridViewTextBoxColumn symptom = new DataGridViewTextBoxColumn()
                    {
                        DataPropertyName = "symptom_desc",
                        HeaderText = "Symptom",
                        ReadOnly = true,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                    };
                    dataGridView2.Columns.Add(code);
                    dataGridView2.Columns.Add(symptom);
                    dataGridView2.DataSource = dt;
                    dataGridView2.Refresh();
                }
            }
            this.FindForm().Enabled = true;
            Application.UseWaitCursor = false;
            Cursor.Current = Cursors.Default;
            this.Refresh();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            record = 0;
        }
    }

    public class keys
    {
        public string sym1 { get; set; }
        public string sym2 { get; set; }
        public string sym3 { get; set; }
        public string sym4 { get; set; }
        public string sym5 { get; set; }
        public string sym6 { get; set; }
    }
}
