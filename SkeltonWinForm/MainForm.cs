using BRY;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SkeltonWinForm
{


	public partial class MainForm : Form
	{
		private string m_FileName = "";
		public static bool _execution = true;
		// ********************************************************************
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		// ********************************************************************
		public MainForm()
		{
			InitializeComponent();
		}

		// ********************************************************************
		private void Form1_Load(object sender, EventArgs e)
		{
			PrefFile pf = new PrefFile();
			this.Text = pf.AppName;
			if (pf.Load() == true)
			{
				bool ok = false;
				Rectangle r = pf.GetRect("Bound", out ok);
				if ((ok) && (PrefFile.ScreenIn(r) == true))
				{
					this.Bounds = r;
				}
				else
				{
					ToCenter();
				}
			}
			//
			Command(Environment.GetCommandLineArgs().Skip(1).ToArray(), PIPECALL.StartupExec);
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
		public void ToCenter()
		{
			Rectangle rct = Screen.PrimaryScreen.Bounds;
			Point p = new Point((rct.Width - this.Width) / 2, (rct.Height - this.Height) / 2);
			this.Location = p;
			ForegroundWindow();
		}
		// ********************************************************************
		public bool Export(string p)
		{
			bool ret = false;
			ForegroundWindow();
			try
			{
				string s = textBox1.Text;
				File.WriteAllText(p, s, Encoding.GetEncoding("utf-8"));
				m_FileName = p;
				this.Text = Path.GetFileName(p);
				ret = true;
			}
			catch
			{
				ret = false;
			}

			return ret;
		}
		// ********************************************************************
		public bool Import(string p)
		{
			bool ret = false;
			ForegroundWindow();
			if (File.Exists(p) == true)
			{
				try
				{
					if (File.Exists(p) == true)
					{
						string str = File.ReadAllText(p, Encoding.GetEncoding("utf-8"));
						textBox1.Text = str;
						textBox1.Select(0, 0);
						m_FileName = p;
						this.Text = Path.GetFileName(p);
						ret = true;
					}
				}
				catch
				{
					ret = false;
				}
			}

			return ret;
		}
		// ********************************************************************
		public void ForegroundWindow()
		{
			SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
		}
		// ********************************************************************
		public void Command(string[] args, PIPECALL IsPipe = PIPECALL.StartupExec)
		{
			bool QuitFlag = false;			bool err = true;

			Args args1 = new Args(args);
			if (args1.OptionCount > 0)
			{
				for (int i = 0; i < args1.OptionCount; i++)
				{
					if (err == false) break;
					Param p = args1.Option(i);
					switch (p.OptionStr.ToLower())
					{
						case "tocenter":
						case "center":
							ToCenter();
							break;
						case "foregroundWindow":
						case "foreground":
							ForegroundWindow();
							break;
						case "load":
						case "ld":
							int idx = p.Index + 1;
							if (idx < args1.ParamsCount)
							{
								if (args1.Params[idx].IsOption == false)
								{
									err = Import(args1.Params[idx].Arg);
								}
							}
							break;
						case "save":
						case "sv":
							int idx2 = p.Index + 1;
							if (idx2 < args1.ParamsCount)
							{
								if (args1.Params[idx2].IsOption == false)
								{
									err = Export(args1.Params[idx2].Arg);
								}
							}
							break;
						case "exit":
						case "quit":
							if ((args1.ParamsCount == 1) && ((IsPipe == PIPECALL.DoubleExec)|| (IsPipe == PIPECALL.PipeExec)))
							{
								QuitFlag = true;
							}
							break;
					}
				}
			}
			else
			{
				if (args1.ParamsCount > 0)
				{
					if (args1.ParamsCount == 1)
					{
						err = Import(args1.Params[0].Arg);
					}
					else
					{
						this.Invoke((Action)(() => {
							textBox1.Lines = args1.ParamStrings;
							textBox1.Select(0, 0);
						}));
					}
				}
			}
			if( IsPipe == PIPECALL.PipeExec)
			{
				PipeData pd = new PipeData(args, IsPipe);
				CallExe.PipeClient("SkeltonWinFormCall",pd.ToJson()).Wait();
			}
			if(QuitFlag) Application.Exit();
			if (IsPipe == PIPECALL.PipeExec)
			{
				//this.Text += "Pi";
			}
		}
		// *******************************************************************************
		// *******************************************************************************
		static public void ArgumentPipeServer(string pipeName)
		{
			Task.Run(() =>
			{ //Taskを使ってクライアント待ち
				while (_execution)
				{
					//複数作ることもできるが、今回はwhileで1つずつ処理する
					using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1))
					{
						// クライアントの接続待ち
						pipeServer.WaitForConnection();

						StreamString ssSv = new StreamString(pipeServer);

						while (true)
						{ //データがなくなるまで                       
							string read = ssSv.ReadString(); //クライアントの引数を受信 
							if (string.IsNullOrEmpty(read))
								break;

							//引数が受信できたら、Applicationに登録されているだろうForm1に引数を送る
							FormCollection apcl = Application.OpenForms;

							if (apcl.Count > 0)
							{
								PipeData pd = new PipeData(read);
								((MainForm)apcl[0]).Command(pd.GetArgs(), pd.GetPIPECALL()); //取得した引数を送る
							}

							if (!_execution)
								break; //起動停止？
						}
#pragma warning disable CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
						ssSv = null;
#pragma warning restore CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
					}
				}
			});
		}
		// ******************************************************************************
		/*
		public static Task ArgumentPipeClient(string pipeName, string js)
		{
			return Task.Run(() =>
			{ //Taskを使ってサーバに送信waitで処理が終わるまで待つ
				using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation))
				{
					StreamString ssCl;
					string writeData;
					pipeClient.Connect();

					ssCl = new StreamString(pipeClient);
					writeData = js; //送信する引数
					ssCl.WriteString(writeData);
#pragma warning disable CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
					ssCl = null;
#pragma warning restore CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
				}
			});
		}
		*/

		private void button1_Click(object sender, EventArgs e)
		{
			/*
			PIPECALL a = PIPECALL.PipeExec;

			PipeData pd = new PipeData(b, a);
			string js = pd.ToJson();
			PipeData pd2 = new PipeData(js);
			string[] b2 = pd2.GetArgs();
			*/
			string[] a = new string[] { "aaa", "sssss", "sfsda" };
			int[] b = new int[] { 12, -2, 6 };

			PrefFile p = new PrefFile();
			p.SetValue("a", a);
			p.SetValue("b", b);

			bool ok = true;
			string[] a2 = p.GetValueStringArray("a", out ok);
			int[] b2 = p.GetValueIntArray("b", out ok);

			textBox1.Text = string.Format("{0};{1}",a2[0], b2[0]);


		}
	}
	// ******************************************************************************

	// *******************************************************************************


	// ********************************************************************
}