using BRY;
namespace SkeltonWinForm
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		// ********************************************************************
		private void Form1_Load(object sender, EventArgs e)
		{
			PrefFile pf = new PrefFile();
			this.Text = pf.AppName;
			if (pf.Load()==true)
			{
				bool ok = false;
				Rectangle r = pf.GetRect("Bound", out ok);
				if (ok)
				{
					if (PrefFile.ScreenIn(r) == true)
					{
						this.Bounds = r;
					}
				}
			}
			//
			Command(Environment.GetCommandLineArgs().Skip(1).ToArray(),true);
		}
		// ********************************************************************
		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			PrefFile pf = new PrefFile();
			pf.SetRect("Bound", this.Bounds);
			pf.Save();
		}
		// ********************************************************************
		private void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
		// ********************************************************************
		public void Command(string[] args,bool IsFirst = true)
		{
			string r = "";
			if (args == null || args.Length == 0)
			{
			}
			else
			{
				foreach (string ehArg in args)
					r += ehArg + "\r\n"; //引数を表示
			}
			textBox1.Text = r;
			textBox1.Select(0,0);
		}
	}
}