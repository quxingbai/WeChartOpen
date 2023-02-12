using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WeChartOpen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        public Drogs DrogState
        {
            get { return (Drogs)GetValue(DrogStateProperty); }
            set { SetValue(DrogStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrogState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrogStateProperty =
            DependencyProperty.Register("DrogState", typeof(Drogs), typeof(MainWindow), new PropertyMetadata(Drogs.None));



        public object ToolTipText
        {
            get { return (object)GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToolTipText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToolTipTextProperty =
            DependencyProperty.Register("ToolTipText", typeof(object), typeof(MainWindow), new PropertyMetadata("拖入微信"));




        public static int OpenWechartCount = 5;

        public enum Drogs
        {
            Drop, Over, Leave, Error, None
        }
        public MainWindow()
        {
            InitializeComponent();
            //格式为 app.1.exe
            try
            {
                string apppath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                FileInfo f = new FileInfo(apppath);
                var names = f.Name.Split('.');
                if (names.Length == 3)
                {
                    int val = int.Parse(names[1]);
                    OpenWechartCount = val;
                }
            }
            catch
            {
                MessageBox.Show("文件名格式错误");
            }
            ToolTipText += "x" + OpenWechartCount;

        }

        private void BD_DragOver(object sender, DragEventArgs e)
        {
            SetDrogState(Drogs.Over, e);
        }

        private void BD_DragLeave(object sender, DragEventArgs e)
        {
            SetDrogState(Drogs.Leave, e);
        }

        private void BD_Drop(object sender, DragEventArgs e)
        {
            SetDrogState(Drogs.Drop, e);
        }
        private void SetDrogState(Drogs d, DragEventArgs e)
        {
            if (DrogState == d) return;
            this.DrogState = d;
            var data = e.Data.GetData(DataFormats.FileDrop);
            string first = data==null?null:((string[])data)[0];
            if (first == null)
            {
                DrogState = d== Drogs.Over? Drogs.Error:Drogs.None;
                return;
            }
            try
            {

                if (d == Drogs.Drop)
                {
                    try
                    {
                        Open(first);
                    }
                    catch (Exception error)
                    {
                        DrogState = Drogs.None;
                        MessageBox.Show("打开失败\n" + error.Message);
                    }
                }
                else if (d == Drogs.Over)
                {
                    FileInfo f = new FileInfo(first);
                    if (f.Name != "微信" && f.Name != "微信.lnk")
                    {
                        this.DrogState = Drogs.Error;
                    }
                }
            }
            catch(Exception er)
            {
                MessageBox.Show("此数据无法解析","打开", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        private void Open(String path)
        {
            var old = DrogState;
            FileInfo f = new FileInfo(path);
            if (f.Name != "微信")
            {
                if (f.Name == "微信.lnk")
                {
                    IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                    WshShortcut link = shell.CreateShortcut(path);
                    path = link.TargetPath;
                }
                else
                {
                    this.DrogState = Drogs.Error;
                    if (MessageBox.Show("此文件可能不是微信 是否打开" + f.Name, "打开", MessageBoxButton.YesNo, MessageBoxImage.Warning,MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.No)
                    {
                        DrogState = Drogs.None;
                        return;
                    }
                    Show();
                }
            }
            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo(path)
            };
            for (int f1 = 0; f1 < OpenWechartCount; f1++)
            {
                p.Start();
            }
            DrogState = Drogs.None;
            Close();
        }
    }
}
