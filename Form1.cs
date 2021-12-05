using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace CSV_to_Aiken
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Some obvious stuff - get file name and load its' content to CsvReader
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            ofd.Multiselect = false;
            DialogResult dr=ofd.ShowDialog();
            if (dr != DialogResult.OK) return;
            CsvImporter csv = new CsvImporter(File.ReadAllText(ofd.FileName));
            
            csv.next();     //we need to skip the header
            string aiken = "";
            int questions = 0;
            while (csv.next())
            {
                if (csv.GetColumnCount() < 4) continue;     //question must contain question, min.2 answers, correct answer
                int answerColumn = csv.GetColumnCount()-1;  //there might be a column with comemnts
                while (answerColumn > 0 && csv.GetColumnValue(answerColumn).Length!=1)  //so we look for a column with one letter
                    answerColumn--;
                if (answerColumn <= 0) continue;

                //Now we now that the row is a question
                questions++;
                aiken += csv.GetColumnValue(0) + '\n';
                char optionLetter = 'A';
                for (int i = 1; i < answerColumn; i++)
                {
                    aiken += optionLetter++;
                    aiken += ". " + csv.GetColumnValue(i)+'\n';
                }
                aiken += "ANSWER: " + csv.GetColumnValue(answerColumn) + "\n\n";
            }
            aikenTextBox.Text = aiken;
            infoBox.Text += "No. of questions:\n" + questions.ToString()+"\n";
        }
    }
    public class CsvImporter
    {
        int dataPtr = -1;
        string[] data = null;
        string[] content = null;
        public CsvImporter(string csv)
        {
            //csv.Split()
            data = csv.Split('\n');
            //data = csv.Split("," + "(?=([^\"]*\"[^\"]*\")*[^\"]*$)");
        }
        const string pattern = @"""\s*,\s*""";
        public bool next()
        {
            dataPtr++;
            if(dataPtr>=data.Length)
                return false;

            content = SplitCsvLine(data[dataPtr]).ToArray();
            return true;
        }
        public int GetColumnCount()
        {
            return content.Length;
        }
        public string GetColumnValue(int column)
        {
            return content[column];
        }
        public List<string> SplitCsvLine(string s)
        {
            int i;
            int a = 0;
            int count = 0;
            List<string> str = new List<string>();
            for (i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case ',':
                        if ((count & 1) == 0)
                        {
                            str.Add(s.Substring(a, i - a));
                            a = i + 1;
                        }
                        break;
                    case '"':
                    /*case '\'':*/ count++; break;
                }
            }
            str.Add(s.Substring(a));
            str.RemoveAt(str.Count() - 1);

            List<string> str2 = new List<string>();
            foreach (string sx in str)
                str2.Add(removeRedundantParenthasies(sx));
            return str2;
        }
        string removeRedundantParenthasies(string source)
        {
            if (source.Contains(','))
                return source.Substring(1, source.Length - 2);
            return source;
        }
    }
}
