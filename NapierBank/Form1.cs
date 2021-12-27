using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace NapierBank
{
    
    public partial class Form1 : Form
    {
        List<Data> allData = new List<Data>();
        public List<Abbreviations> abrv = new List<Abbreviations>();
        public List<Trends> trending = new List<Trends>();
        public List<Trends> mentions = new List<Trends>();
        public List<Sirs> sirList = new List<Sirs>();
        
        //Declaration of type abbreviations
        public class Abbreviations
        {
            [Name("Abbreviations")]
            public string shorty { get; set; }
            [Name("Full word")]
            public string longy { get; set; }
        }

        //Deserialize JSON file
        static List<Data> FetchDataList ()
        {
            string fileName = @"D:\Napier Bank Soft Project\NapierBank\NapierBank\data.json";

            if (File.Exists(fileName))
            {
                var _dataList = JsonConvert.DeserializeObject<List<Data>>(File.ReadAllText(fileName));
                return _dataList;
            }
            return null;
        }

        public Form1()
        {
            InitializeComponent();
            //Fetch abbreviations from Excel sheet
            using (var streamReader = new StreamReader(@"D:\Napier Bank Soft Project\NapierBank\NapierBank\textwords.csv"))
            {
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    abrv = csvReader.GetRecords<Abbreviations>().ToList();
                }
            }

            //deserialize
            var tempdata = FetchDataList();
            if (tempdata != null)
            { 
            allData = (FetchDataList());


                if (allData != null)
                {
                    foreach (var listBoxEntries in allData)
                    {
                        listBox1.Items.Add(listBoxEntries.mHeader);
                    }

                    setTrends();
                    setMentions();
                    setSirs();
                }
            }
        }

        private void RefreshData()
        {
            allData = (FetchDataList());

            foreach (var listBoxEntries in allData)
            {
                listBox1.Items.Add(listBoxEntries.mHeader);
            }

            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();

            foreach (var listBoxEntries in allData)
            {
                listBox1.Items.Add(listBoxEntries.mHeader);
            }
            setTrends();
            setMentions();
            setSirs();
        }


        //method to determine type of method /T/E/S
        private string typeDetermineID(string id)
        {
            long numb = 0;
            Random rand1 = new Random();
            Random rand2 = new Random();
            Random rand3 = new Random();


            if ((long.TryParse(id, out numb) == true) || (id.StartsWith("+") == true))
            {
                numb = rand1.Next(100000000, 999999999);
                id = numb.ToString();
                id = "S" + id;
                return id;
            }

            if (id.StartsWith("@"))
            {
                numb = rand2.Next(100000000, 999999999);
                id = numb.ToString();
                id = "T" + id;
                return id;
            }

            if (id.Contains("@"))
            {
                numb = rand3.Next(100000000, 999999999);
                id = numb.ToString();
                id = "E" + id;
                return id;
            }
            else id = "Unknown";

            return id;
        }

        //method to determine body
        private string typeDetermineBody(string body, Data _data)
        {
            string elongate = "Sender: " + textBox1.Text + "\n";
            string newWord;

            if (_data.mHeader.StartsWith("S"))
            {
                foreach (string word in body.Split(' '))
                {
                    newWord = word;
                    if (word.Length <= 6)
                    {
                        foreach (var shorties in abrv)
                        {
                            if (shorties.shorty.Equals(word) || (shorties.shorty.Equals(word.Remove(word.Length- 1))))
                            {
                                elongate = elongate + word + " ";
                                newWord = "<" + shorties.longy + ">";
                                break;
                            }
                        }
                    }
                    elongate = elongate + newWord + " ";
                }
                return elongate.Remove(elongate.Length - 1);
            }

            if (_data.mHeader.StartsWith("E"))
            {
                elongate = elongate + "Subject: ";

                using (var reader = new StringReader(body))
                {
                    string first = reader.ReadLine();
                    string second = reader.ReadLine();
                    if (first[2].Equals('-') == true)
                    {
                        if (first[5].Equals('-') == true)
                        {
                            elongate = "Sort Code: ";
                            Sirs tempsirs = new Sirs();
                            tempsirs.sortCode = first;
                            tempsirs.nature = second;
                            sirList.Add(tempsirs);
                            elongate = elongate + first + "\nNature of incident: " + second.Remove(second.Length - 1);
                            body = body.Remove(0, first.Length);
                            body = body.Remove(0, second.Length);
                            listBox4.Items.Add(elongate);
                        }
                    }
                    else
                    {
                        elongate = elongate + first;
                        body = body.Remove(0, first.Length);
                    }
                }

                foreach (string word in body.Split(' '))
                {
                    if ((word.Contains("http") == true && word.Contains(".")) || (word.Contains("www") == true && word.Contains(".") == true))
                    {
                        elongate = elongate + "<URL QUARANTINED> ";
                        continue;
                    }
                    elongate = elongate + word + " ";
                }
                return elongate.Remove(elongate.Length - 1);
            }

            if (_data.mHeader.StartsWith("T"))
            {
                Trends trendMentions = new Trends();
                foreach (string word in body.Split(' '))
                {
                    newWord = word;
                    if (word.Length <= 6)
                    {
                        foreach (var shorties in abrv)
                        {
                            if (shorties.shorty.Equals(word) || (shorties.shorty.Equals(word.Remove(word.Length - 1))))
                            {
                                elongate = elongate + word + " ";
                                newWord = "<" + shorties.longy + ">";
                                break;
                            }
                        }
                    }
                    elongate = elongate + newWord + " ";

                    if (word.StartsWith("#"))
                    {
                        trendMentions.text = word;
                        mentions.Add(trendMentions);
                        listBox2.Items.Add(word);
                    }

                    if (word.StartsWith("@"))
                    {
                        trendMentions.text = word;
                        trending.Add(trendMentions);
                        listBox3.Items.Add(word);
                    }
                }
                return elongate.Remove(elongate.Length - 1);
            }
            return body;
        }

        //method to set sirs
        private void setSirs()
        {
            Sirs tempsir = new Sirs();
            foreach (var temp in allData)
            {
                if (temp.mHeader.StartsWith("E") && temp.body.StartsWith("Sort Code: "))
                {
                    using (var reader = new StringReader(temp.body))
                    {
                        tempsir.sortCode = reader.ReadLine();
                        tempsir.nature = reader.ReadLine();
                        sirList.Add(tempsir);
                        listBox4.Items.Add(tempsir.sortCode + " " +  tempsir.nature);
                    }
                }
            }
        }

        //method to set mentions
        private void setMentions()
        {
            Trends tempMentions = new Trends();
            {
                foreach(var temp in allData)
                {
                    if (temp.mHeader.StartsWith("T"))
                    {
                        foreach (string word in temp.body.Split(' '))
                        {
                            if (word.StartsWith("@"))
                            {
                                tempMentions.text = word;
                                mentions.Add(tempMentions);
                                listBox3.Items.Add(word);
                            }
                        }
                    }
                }
            }
        }

        //method to set trends
        private void setTrends()
        {
            Trends trendMentions = new Trends();
            {
                foreach (var temp in allData)
                {
                    if (temp.mHeader.StartsWith("T"))
                    {
                        foreach (string word in temp.body.Split(' '))
                        {
                            if (word.StartsWith("#"))
                            {
                                trendMentions.text = word;
                                trending.Add(trendMentions);
                                listBox2.Items.Add(word);
                            }
                        }
                    }
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Data _data = new Data();
            if (typeDetermineID(textBox1.Text).Equals("Unknown"))
            {
                MessageBox.Show("Please use correct format(EX: +34323245, you@home.com, @HomeAllan)");
            }
            else
            {

                _data.mHeader = typeDetermineID(textBox1.Text);
                _data.body = typeDetermineBody(richTextBox1.Text, _data);

                allData.Add(_data);

                listBox1.Items.Add(_data.mHeader);

                string jsonFile = @"D:\Napier Bank Soft Project\NapierBank\NapierBank\data.json";
                string json = JsonConvert.SerializeObject(allData, Formatting.Indented);
                File.WriteAllText(jsonFile, json);
            }




        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.StartsWith("@"))
            {
                textBox1.MaxLength = 16;
            }
            else
                textBox1.MaxLength = 30;


           
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        //fetch selected data
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                foreach (var temp in allData)
                {
                    if (temp.mHeader.Equals(listBox1.SelectedItem.ToString()))
                    {
                        MessageBox.Show(temp.body);
                    }
                }
            }
            else MessageBox.Show("Please select entry first.");
        }

        //read from text file
        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string fileName = openFileDialog1.FileName;
            string readFile = File.ReadAllText(fileName);
            if (fileName.EndsWith("txt"))
            {

                Data _data = new Data();

                using (var reader = new StringReader(readFile))
                {
                    string first = reader.ReadLine();
                    _data.mHeader = typeDetermineID(first);
                    readFile = readFile.Remove(0, first.Length);
                }
                _data.body = typeDetermineBody(readFile, _data);
                allData.Add(_data);
                listBox1.Items.Add(_data.mHeader);
             }

            else { MessageBox.Show("Please select appropriate file"); }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            RefreshData();
        }
    }
}
