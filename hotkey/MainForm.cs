using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace hotkey
{
    public partial class MainForm : Form
    {
        HotKeyRegister hotKeyToRegister = null;
        Keys registerKey = Keys.None;
        KeyModifiers registerModifiers = KeyModifiers.None;

        string basePath = "";
        FileSystemWatcher watcher = null;
        public MainForm()
        {
            InitializeComponent();

            basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\NBGI";
            if (!Directory.Exists(basePath + @"\bak\Manual Backup"))
            {
                Directory.CreateDirectory(basePath + @"\bak\Manual Backup");
            }

            if (!Directory.Exists(basePath + @"\bak\Auto Backup"))
            {
                Directory.CreateDirectory(basePath + @"\bak\Auto Backup");
            }

            watcher = new FileSystemWatcher();
            string backUpPath = basePath + @"\bak\Auto Backup";
            watcher.Path = basePath + @"\DarkSouls";
            watcher.Filter = "*.json";
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                hotKeyToRegister = new HotKeyRegister(this.Handle, 100, this.registerModifiers, this.registerKey);
                hotKeyToRegister.HotKeyPressed += new EventHandler(this.HotKeyPressed);

                btnRegister.Enabled = false;
                tbHotKey.Enabled = false;
                btnUnregister.Enabled = true;
            }
            catch(ArgumentException argumentException)
            {
                MessageBox.Show(argumentException.Message);
            }
            catch(ApplicationException applicationException)
            {
                MessageBox.Show(applicationException.Message);
            }

        }

        private void btnUnregister_Click(object sender, EventArgs e)
        {
            if(hotKeyToRegister != null)
            {
                hotKeyToRegister.Dispose();
                hotKeyToRegister = null;
            }

            btnRegister.Enabled = true;
            tbHotKey.Enabled = true;
            btnUnregister.Enabled = false;
        }

        private void tbHotKey_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            if(e.Modifiers != Keys.None)
            {
                Keys key = Keys.None;
                KeyModifiers modifiers = HotKeyRegister.GetModifiers(e.KeyData, out key);
                if(key != Keys.None)
                {
                    this.registerKey = key;
                    this.registerModifiers = modifiers;
                    tbHotKey.Text = string.Format("{0}+{1}", this.registerModifiers, this.registerKey);
                    btnRegister.Enabled = true;
                }
            }
        }

        void HotKeyPressed(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;
            string direction = date.ToString("yyyy-MM-dd HH-mm-ss");
            if (!Directory.Exists(basePath + @"\bak\Manual Backup\" + direction))
            {
                Directory.CreateDirectory(basePath + @"\bak\Manual Backup\" + direction);
            }
            string[] files = Directory.GetFiles(basePath + @"\DarkSouls", "*.sl2", SearchOption.TopDirectoryOnly);
            foreach(string file in files)
            {
                string temp = @"bak\Manual Backup\" + direction;
                string newfile = file.Replace("DarkSouls", temp);
                File.Copy(file, newfile, true);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if(hotKeyToRegister != null)
            {
                hotKeyToRegister.Dispose();
                hotKeyToRegister = null;
            }
            watcher.Dispose();

            base.OnClick(e);
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            DateTime date = DateTime.Now;
            string dateString = date.ToString("yyyy-MM-dd HH-mm-ss");

            string newSavePath = e.FullPath.Replace(@"DarkSouls\", @"\bak\Auto Backup/" + dateString + " ");

            File.Copy(e.FullPath, newSavePath, true);           
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            watcher.EnableRaisingEvents = checkBox1.Checked;
        }
    }
}
