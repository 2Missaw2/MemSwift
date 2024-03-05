using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MemSwift
{
    public partial class Form1 : Form
    {
        Timer myTimer = new Timer();
        Timer myTimer2 = new Timer();
        Timer myTimer3 = new Timer();

        class GlobalHotkeys
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        }

        private int MY_HOTKEY_ID = 696969;
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CTRL = 0x0002;        

        public Form1()
        {
            InitializeComponent();
            GetUsageInit();
            FormClosing += (s, e) => { GlobalHotkeys.UnregisterHotKey(Handle, MY_HOTKEY_ID); };
            string hotkey = Settings1.Default.hotkey;
            if (hotkey.StartsWith("Ctrl+")) registerNewHotKey(ConvertStringToKey(hotkey.Substring(5)), MOD_CTRL);
            if (hotkey.StartsWith("Alt+")) registerNewHotKey(ConvertStringToKey(hotkey.Substring(4)), MOD_ALT);
            else registerNewHotKey(ConvertStringToKey(hotkey), MOD_NONE);

            loadOnSystemStartupToolStripMenuItem.Checked = Settings1.Default.startup;
            startMinimisedToolStripMenuItem.Checked = Settings1.Default.minimized;
            label1.Text = (GetTotalMemoryInBytes() / (1024 * 1024)).ToString();
            GetTotalMemory();

            myTimer.Interval = 1000;
            myTimer2.Interval = 1;            
            myTimer.Tick += new EventHandler(GetAvailableMemory);
            myTimer.Tick += new EventHandler(GetUsage);
            myTimer2.Tick += new EventHandler(refresh_Settings);
            myTimer2.Tick += new EventHandler(refresh_Event);
            myTimer3.Tick += new EventHandler(interval_Event);
            myTimer.Start();
            myTimer2.Start();
        }

        private void interval_Event(object sender, EventArgs e)
        {
            Program.EmptyWorkingSetFunc();
        }

        public static void RemoveFromStartup()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);

                key.DeleteValue("MemSwift");

                key.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Operation Failed");
            }
        }

        public static void AddToStartup()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                string executablePath = Application.ExecutablePath;
                key.SetValue("MemSwift", executablePath);

                key.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Operation Failed");
            }
        }

        protected override void WndProc(ref Message m)
		{
            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();
                if (id == MY_HOTKEY_ID)
                {
                    Program.EmptyWorkingSetFunc();
                }
            }
			base.WndProc(ref m);
		}

		private void button1_Click(object sender, EventArgs e)
        {
            Program.EmptyWorkingSetFunc();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Settings1.Default.minimized) WindowState = FormWindowState.Minimized;
        }

        private void refresh_Event(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void GetAvailableMemory(object sender, EventArgs e) {
            label1.Text = (GetAvailableMemoryInBytes() / (1024 * 1024)).ToString();
        }

        private void GetTotalMemory()
        {
            label5.Text = (GetTotalMemoryInBytes() / (1024 * 1024)).ToString();
        }

        static ulong x;
        static ulong y;

        private void GetUsage(object sender, EventArgs e)
        {
            x = (GetAvailableMemoryInBytes() / (1024 * 1024));
            y = (GetTotalMemoryInBytes() / (1024 * 1024));
            int a = Convert.ToInt32(x);
            int b = Convert.ToInt32(y);
            double res = (double)(b - a) / b;
            res = (double) res * 100;
            label8.Text = res.ToString("0");
        }

        private void GetUsageInit()
        {
            x = (GetAvailableMemoryInBytes() / (1024 * 1024));
            y = (GetTotalMemoryInBytes() / (1024 * 1024));
            int a = Convert.ToInt32(x);
            int b = Convert.ToInt32(y);
            double res = (double)(b - a) / b;
            res = (double)res * 100;
            label8.Text = res.ToString("0");
        }

        static ulong GetAvailableMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory;
        }

        static ulong GetTotalMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        }

        public class Program
        {
            [DllImport("psapi.dll")]
            static extern int EmptyWorkingSet(IntPtr hwProc);

            public static void EmptyWorkingSetFunc()
            {
                Process[] allProc = Process.GetProcesses();
                for (int i = 0; i < allProc.Length; i++)
                {
                    Process p = new Process();
                    p = allProc[i];
                    try
                    {
                        EmptyWorkingSet(p.Handle);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        
        private void settingsToolStripMenuItem2_Click(object sender, EventArgs e)
		{
            Form2 frm = new Form2();
            frm.check1 = Settings1.Default.startup;
            frm.check2 = Settings1.Default.minimized;
            frm.hotky = Settings1.Default.hotkey;
            frm.interv = Settings1.Default.interval;
            frm.check4 = Settings1.Default.interval_check;
            frm.thresh = Settings1.Default.threshold;
            frm.check3 = Settings1.Default.threshold_check;
            frm.language = Settings1.Default.lang;
            frm.ShowDialog();
        }

		private void loadOnSystemStartupToolStripMenuItem_Click(object sender, EventArgs e)
		{
            if (loadOnSystemStartupToolStripMenuItem.Checked == true)
            {
                Settings1.Default.startup = true;
                Settings1.Default.Save();
            }
            if (loadOnSystemStartupToolStripMenuItem.Checked == false)
            {
                Settings1.Default.startup = false;
                Settings1.Default.Save();
            }
            if (Settings1.Default.startup) AddToStartup();
            else RemoveFromStartup();
        }

		private void startMinimisedToolStripMenuItem_Click(object sender, EventArgs e)
		{
            if (startMinimisedToolStripMenuItem.Checked == true)
            {
                Settings1.Default.minimized = true;
                Settings1.Default.Save();
            }
            if (startMinimisedToolStripMenuItem.Checked == false)
            {
                Settings1.Default.minimized = false;
                Settings1.Default.Save();
            }
        }

        private void refresh_Settings(object sender, EventArgs e)
        {
            loadOnSystemStartupToolStripMenuItem.Checked = Settings1.Default.startup;
            startMinimisedToolStripMenuItem.Checked = Settings1.Default.minimized;
            string hotkey = Settings1.Default.hotkey;
            if (hotkey.StartsWith("Ctrl+")) registerNewHotKey(ConvertStringToKey(hotkey.Substring(5)), MOD_CTRL);
            if (hotkey.StartsWith("Alt+")) registerNewHotKey(ConvertStringToKey(hotkey.Substring(4)), MOD_ALT);
            else registerNewHotKey(ConvertStringToKey(hotkey), MOD_NONE);
            myTimer3.Interval = Settings1.Default.interval * 60000;
            if (!Settings1.Default.interval_check) myTimer3.Stop();
            else myTimer3.Start();
            if (Settings1.Default.threshold_check)
            {
                int res = Int32.Parse(label8.Text);
                if (res > Settings1.Default.threshold) Program.EmptyWorkingSetFunc();
            }
            if (Settings1.Default.lang == "French")
            {
                settingsToolStripMenuItem.Text = "Paramétres";
                settingsToolStripMenuItem1.Text = "Paramétres";
                settingsToolStripMenuItem2.Text = "Paramétres";
                helpToolStripMenuItem.Text = "Aide";
                fileToolStripMenuItem.Text = "Fichier";
                exitToolStripMenuItem.Text = "Quiter";
                exitToolStripMenuItem1.Text = "Quiter";
                loadOnSystemStartupToolStripMenuItem.Text = "Charger au démarrage";
                startMinimisedToolStripMenuItem.Text = "Commencer minimisé";
                aboutToolStripMenuItem.Text = "À propos";
                label2.Text = "Mémoire disponible :";
                label4.Text = "Mémoire totale :";
                label9.Text = "Utilisation :";
                button1.Text = "Optimiser";
            }
            if (Settings1.Default.lang == "English")
            {
                settingsToolStripMenuItem.Text = "Settings";
                settingsToolStripMenuItem1.Text = "Settings";
                settingsToolStripMenuItem2.Text = "Settings";
                helpToolStripMenuItem.Text = "Help";
                fileToolStripMenuItem.Text = "File";
                exitToolStripMenuItem.Text = "Exit";
                exitToolStripMenuItem1.Text = "Exit";
                loadOnSystemStartupToolStripMenuItem.Text = "Load on startup";
                startMinimisedToolStripMenuItem.Text = "Start minimized";
                aboutToolStripMenuItem.Text = "About";
                label2.Text = "Available Memory :";
                label4.Text = "Total Memory :";
                label9.Text = "Usage :";
                button1.Text = "Optimise";
            }
        }

		private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
		{
            Close();
		}

        private void registerNewHotKey(Keys key, uint mod)
        {
            GlobalHotkeys.UnregisterHotKey(Handle, MY_HOTKEY_ID);
            GlobalHotkeys.RegisterHotKey(Handle, MY_HOTKEY_ID, mod, (uint)key);
        }

        private static Keys ConvertStringToKey(string keyString)
        {
            try
            {
                Keys convertedKey = (Keys)Enum.Parse(typeof(Keys), keyString);

                return convertedKey;
            }
            catch (ArgumentException)
            {
                return Keys.None;
            }
        }

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start("http://www.github.com/2Missaw2/");
        }
	}
}
