using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;

namespace XpathTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            hd.LoadHtml(textBoxXml.Text);
            Thread th = new Thread(NewMethod);
            th.Start();
        }

        private delegate void Dg(string str);
        Dictionary<string, string> D = new Dictionary<string, string>();

        private void NewMethod()
        {
            Dg dgUIContorol = new Dg(UIContorol);

            List<string> returnList = new List<string>();
            string str = textBoxXml.Text;
            string s = "<script[\\s\\S]*?</script>";
            MatchCollection ms = Regex.Matches(str, s, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            foreach (Match m in ms)
            {
                str = str.Replace(m.Value, "");
            }
            Dictionary<string, int> dic = new Dictionary<string, int>();
            List<string> strList = new List<string>();
            strList.Add(".");

            string strPattern = "<([^<>]*?)>";
            MatchCollection Matches = Regex.Matches(str, strPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            
            foreach (Match NextMatch in Matches)
            {
                if (!NextMatch.Groups[0].Value.EndsWith("/ >") && !NextMatch.Groups[0].Value.EndsWith("/>") && !NextMatch.Groups[0].Value.StartsWith("<!"))
                {
                    if (NextMatch.Groups[0].Value.StartsWith("</"))
                    {
                        if (NextMatch.Groups[0].Value.Replace("</", "<").ToLower() == strList[strList.Count - 1].ToLower())
                        {
                            strList.RemoveAt(strList.Count - 1);
                        }
                    }
                    else
                    {
                        string strOldXpath = XpathRow(strList, dic);
                        string strp = "(<(?<body>[^>]*?) [^>]*?>)|(<(?<body>[^>]*?)>)";
                        string v = Regex.Matches(NextMatch.Groups[0].Value, strp, RegexOptions.IgnoreCase | RegexOptions.Compiled)[0].Groups["body"].Value.ToLower();
                        if (v.ToUpper() != "LINK" && v.ToUpper() != "META" && v.ToUpper() != "SCRIPT" && v.ToUpper() != "IMG" && v.ToUpper() != "INPUT" && v.ToUpper() != "FORM")
                        {

                            AddRowNumber(strOldXpath, "<" + v + ">", dic);
                            strList.Add("<" + v + ">");
                            returnList.Add(XpathRow(strList, dic));
                            //label1.Text = returnList.Last();
                            try
                            {
                                string SelectNodes = hd.DocumentNode.SelectNodes(returnList.Last())[0].InnerHtml;
                                textBoxValue.Invoke(dgUIContorol, new object[] { returnList.Last() });
                                D.Add(returnList.Last(), SelectNodes);
                                //if (D.ContainsKey("./html[1]/body[1]/table[1]/tr[1]/td[1]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/div[1]/div[1]/fieldset[1]/div[1]"))
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                else
                {

                }
            }
            //listBox1.DataSource = returnList;
            //listBox1.Items.Add(returnList.Count);
            if (strList.Count == 1)
            {
                toolStripStatusLabel1.Text = "OK";
            }
            else
            {
                toolStripStatusLabel1.Text = "False";
            }
        }
        private void UIContorol(string str)
        {
            //textBox1.Text = str;
            listBox1.Items.Add(str);
            toolStripStatusLabel1.Text = str;
        }

        private void AddRowNumber(string strOldXpatch, string NewNode, Dictionary<string, int> dic)
        {
            if (strOldXpatch == "")
            {
                if (!dic.ContainsKey("."))
                {
                    dic.Add(".", 0);
                }
                else
                {
                    dic["."] = 0;
                }
                return;
            }
            string strPattern = "<(?<body>[^>]*?)>";
            string v = Regex.Matches(NewNode, strPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)[0].Groups["body"].Value;
            if (dic.ContainsKey(strOldXpatch + "/" + v))
            {
                dic[strOldXpatch + "/" + v]++;
            }
            else
            {
                dic.Add(strOldXpatch + "/" + v, 1);
            }
        }

        private string XpathRow(List<string> strList, Dictionary<string, int> dic)
        {

            StringBuilder sb = new StringBuilder();
            foreach (var str in strList)
            {
                string strPattern = "<(?<body>[^>]*?)>";
                string v = "";
                try
                {
                    v = Regex.Matches(str, strPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)[0].Groups["body"].Value;

                    string temp = sb.ToString() + v;
                    v = v + "[" + dic[temp].ToString() + "]";
                }
                catch
                {
                    v = str;
                }


                sb.Append(v + "/");
            }
            return sb.ToString().TrimEnd('/');
        }

        HtmlAgilityPack.HtmlDocument hd = new HtmlAgilityPack.HtmlDocument();
        private void Form1_Load(object sender, EventArgs e)
        {

        }

       

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            foreach (string str in D.Where(fun => fun.Value.ToLower().Contains(textBox3.Text.ToLower())).Select(fun => fun.Key))
            {
                listBox1.Items.Add(str);
            }
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                textBoxValue.Text = listBox1.SelectedItem.ToString();
                string xpath = textBoxValue.Text;
                button3_Click(null, null);
                Clipboard.SetText(xpath.Remove(0,1));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBoxValue.Text = hd.DocumentNode.SelectNodes(textBoxValue.Text)[0].InnerHtml;
        }

        private void button4_Click(object sender, EventArgs e) {
            textBoxXml.Text = "";
        }

        private void button5_Click(object sender, EventArgs e) {
            textBoxValue.Text = hd.DocumentNode.SelectNodes(txtCustomer.Text)[0].InnerHtml;
        }
    }
}
