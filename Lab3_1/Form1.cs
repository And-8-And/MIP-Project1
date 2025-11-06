using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab3_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            string logFile = "log.txt";

            if (System.IO.File.Exists(logFile))
            {
                System.IO.File.WriteAllText(logFile, string.Empty);
            }

            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate("https://duckduckgo.com");

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
            Trace.AutoFlush = true;

            foreach (var word in blockedKeywords)
            {
                toolStripComboBox1.Items.Add(word);
            }
        }

        private List<string> blockedKeywords = new List<string>
        {
            "facebook",
            "instagram",
            "twitter"
        };

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate("https://duckduckgo.com");

            LogEventAsync("Back to home page");
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoBack)
            {
                webBrowser1.GoBack();
            }

            LogEventAsync("Navigate back");
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoForward)
            {
                webBrowser1.GoForward();
            }

            LogEventAsync("Navigate forward");
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate("https://duckduckgo.com");

            LogEventAsync("Home button clicked - Navigated to homepage");
        }


        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                webBrowser1.Navigate(toolStripTextBox1.Text);
            }

            LogEventAsync("Navigate from textbox: " + toolStripTextBox1.Text);
        }

        private async void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.ToString().ToLower();

            string found = await Task.Run<string>(delegate ()
            {
                var query =

                from word in blockedKeywords
                where url.Contains(word)
                select word;

                if (query.Any())
                {
                    return query.First();
                }
                else
                {
                    return null;
                }
            });

            if (found != null)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    e.Cancel = true;
                    MessageBox.Show("You don't have acces to this website", "Blocking...", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    LogEventAsync("Blocked website: " + url);
                });
            }
            else
            {
                LogEventAsync("Navigating to: " + url);
            }
        }

        private async void LogEventAsync(string message)
        {
            await Task.Run(delegate()
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                Trace.WriteLine(time + " - " + message);
            });
        }

        private void toolStripTextBox2_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            string keyword = toolStripTextBox2.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("No keyword typed", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            bool exists = 
            (
            from word in blockedKeywords
            where word == keyword
            select word
            ).Any();

            if (exists)
            {
                MessageBox.Show("This word is already in the list", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                blockedKeywords.Add(keyword);

                toolStripComboBox1.Items.Add(keyword);
                toolStripTextBox2.Clear();

                MessageBox.Show("Word added to blocked keywords", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            string keyword = toolStripTextBox2.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("No keyword typed", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            bool exists =
            (
            from word in blockedKeywords
            where word == keyword
            select word
            ).Any();

            if (!exists)
            {
                MessageBox.Show("This word is not in the list", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                blockedKeywords.Remove(keyword);

                toolStripComboBox1.Items.Remove(keyword);
                toolStripTextBox2.Clear();

                MessageBox.Show("Word removed from blocked keywords", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private SQLiteHandler dbHandler = new SQLiteHandler();

        private void connectToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                dbHandler.ConnectToDb();

                MessageBox.Show("Database connected", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection failed: " + ex.Message);
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dbHandler.DisconnectFromDb();

                MessageBox.Show("Database disconnected", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database disconnection failed: " + ex.Message);
            }
        }

        private void addKeywordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeywordInput kwdForm = new KeywordInput();

            if (kwdForm.ShowDialog() == DialogResult.OK)
            {
                string kwd = kwdForm.getInputText();

                if (!string.IsNullOrWhiteSpace(kwd))
                {
                    dbHandler.InsertKeyword(kwd);

                    MessageBox.Show("Keyword added to database: " + kwd, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No keyword entered", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

        }

        private void viewKeywordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> words = dbHandler.GetAllKeywords();

            if (words.Count == 0)
            {
                MessageBox.Show("No keywords found in database", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string all = string.Join("\n", words);

                MessageBox.Show("Keywords:\n" + all, "Database contents", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
