using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;

namespace MemSwift
{
	public partial class Form2 : Form
	{
		public bool check1 { get; set; }
		public bool check2 { get; set; }
		public string hotky { get; set; }
		public int interv { get; set; }
		public int thresh { get; set; }
		public bool check3 { get; set; }
		public bool check4 { get; set; }
		public string language { get; set; }

		class GlobalHotkeys
		{
			[DllImport("user32.dll", SetLastError = true)]
			public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		}

		private int MY_HOTKEY_ID = 69;
		private const uint MOD_NONE = 0x0000;
		private const uint MOD_ALT = 0x0001;
		private const uint MOD_CTRL = 0x0002;

		public Form2()
		{
			InitializeComponent();
		}

		public void textBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Modifiers != 0 && e.KeyCode != Keys.None)
			{
				if (e.Modifiers.ToString() == "Control") textBox1.Text = $"Ctrl+{e.KeyCode}";
				else textBox1.Text = $"{e.Modifiers}+{e.KeyCode}";

				e.SuppressKeyPress = true;
			}
			if (e.Modifiers == 0 && e.KeyCode != Keys.None)
			{
				textBox1.Text = $"{e.KeyCode}";

				e.SuppressKeyPress = true;
			}
		}
		private void registerNewHotKey(Keys key, uint mod)
		{
			GlobalHotkeys.UnregisterHotKey(Handle, MY_HOTKEY_ID);
			GlobalHotkeys.RegisterHotKey(Handle, MY_HOTKEY_ID, mod, (uint)key);
		}

		public void Form2_Load(object sender, EventArgs e)
		{
			checkBox1.Checked = check1;
			checkBox2.Checked = check2;
			textBox1.Text = hotky;
			checkBox3.Checked = check3;
			checkBox4.Checked = check4;
			numericUpDown1.Value = interv;
			numericUpDown2.Value = thresh;
			comboBox1.SelectedItem = language;
			if (comboBox1.SelectedItem == "French")
			{
				checkBox1.Text = "Charger au démarrage";
				checkBox2.Text = "Commencer minimisé";
				label1.Text = "Changer le raccourci clavier :";
				label2.Text = "Optimiser quand on est au-dessus (%)";
				label3.Text = "Optimiser chaque (min)";
				label4.Text = "Choisir la langue :";
				button1.Text = "Appliquer";
				this.Text = "Paramétres";
			}
			if (comboBox1.SelectedItem == "English")
			{
				checkBox1.Text = "Load on startup";
				checkBox2.Text = "Start minimized";
				label1.Text = "Change the hotkey :";
				label2.Text = "Optimise when above (%)";
				label3.Text = "Optimise every (min)";
				label4.Text = "Select Language :";
				button1.Text = "Apply";
				this.Text = "Settings";
			}
		}

		private static Keys ConvertStringToKey(string keyString)
		{
			try
			{
				Keys convertedKey = (Keys)new KeysConverter().ConvertFromString(keyString);

				return convertedKey;
			}
			catch (ArgumentException)
			{
				return Keys.None;
			}
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
			Settings1.Default.startup = checkBox1.Checked;
			Settings1.Default.minimized = checkBox2.Checked;
			Settings1.Default.hotkey = textBox1.Text;
			Settings1.Default.interval = (int)numericUpDown1.Value;
			Settings1.Default.threshold = (int)numericUpDown2.Value;
			Settings1.Default.interval_check = checkBox4.Checked;
			Settings1.Default.threshold_check = checkBox3.Checked;
			Settings1.Default.lang = (string)comboBox1.SelectedItem;
			Settings1.Default.Save();
			string hotkey = Settings1.Default.hotkey;
			if (hotkey.StartsWith("Ctrl+")) registerNewHotKey(ConvertStringToKey(hotkey.Substring(5)), MOD_CTRL);
			if (hotkey.StartsWith("Alt+")) registerNewHotKey(ConvertStringToKey(hotkey.Substring(4)), MOD_ALT);
			else registerNewHotKey(ConvertStringToKey(hotkey), MOD_NONE);
			if (comboBox1.SelectedItem == "French")
			{
				checkBox1.Text = "Charger au démarrage";
				checkBox2.Text = "Commencer minimisé";
				label1.Text = "Changer le raccourci clavier :";
				label2.Text = "Optimiser quand on est au-dessus (%)";
				label3.Text = "Optimiser chaque (min)";
				label4.Text = "Choisir la langue :";
				button1.Text = "Appliquer";
				this.Text = "Paramétres";
			}
			if (comboBox1.SelectedItem == "English")
			{
				checkBox1.Text = "Load on startup";
				checkBox2.Text = "Start minimized";
				label1.Text = "Change the hotkey :";
				label2.Text = "Optimise when above (%)";
				label3.Text = "Optimise every (min)";
				label4.Text = "Select Language :";
				button1.Text = "Apply";
				this.Text = "Settings";
			}
		}
	}
}
