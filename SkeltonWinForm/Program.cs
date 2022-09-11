//using Microsoft.VisualBasic.ApplicationServices;
using BRY;

namespace SkeltonWinForm
{
	/*
	public class MyApp : WindowsFormsApplicationBase
	{
		/// <summary>初期化</summary>
		public MyApp() : base()
		{
			this.EnableVisualStyles = true;  //XP Visualスタイルとかいうやつ
			this.IsSingleInstance = true;    //単一インスタンス            
			this.MainForm = new Form1();     //起動させるフォームを初期化
			this.StartupNextInstance += new StartupNextInstanceEventHandler(MyApp_StartupNextInstance); //二重起動時のメソッドを登録
		}
		/// <summary>Runイベントをキャプチャ</summary>
		protected override void OnRun()
		{
			((Form1)this.MainForm).Command(this.CommandLineArgs.ToArray());
			base.OnRun(); //起動するとここで止まる（フォームを閉じるまで戻ってこない）
		}
		/// <summary>二重起動時インスタンス</summary>
		public void MyApp_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
		{
			((Form1)this.MainForm).Command(e.CommandLine.ToArray());
		}
	}
	*/
	internal static class Program
	{
		private const string ApplicationId = "6E8A81F9-12DC-4954-A353-F5C13DE62BC6"; // GUIDなどユニークなもの
		private static System.Threading.Mutex _mutex = new System.Threading.Mutex(false, ApplicationId);
		[STAThread]
		static void Main(string[] args)
		{
			// 通常の起動
			//ApplicationConfiguration.Initialize();
			//Application.Run(new Form1());

			// WindowsFormsApplicationBaseを使った起動
			//MyApp winAppBase = new MyApp();
			//winAppBase.Run(args); //RunするとOnRunイベントが発生する

			if (_mutex.WaitOne(0, false))
			{//起動していない
				Pipes._execution = true;
				Pipes.ArgumentPipeServer(ApplicationId);
				ApplicationConfiguration.Initialize();
				Form1 f = new Form1();
				f.Command(args);
				Application.Run(f);
				Pipes._execution = false;
			}
			else
			{ //起動している
				//MessageBox.Show("すでに起動しています",
				//				ApplicationId,
				//				MessageBoxButtons.OK, MessageBoxIcon.Hand);

				Pipes.ArgumentPipeClient(ApplicationId, args).Wait();
				return; // プログラム終了
			}
		}
	}
}