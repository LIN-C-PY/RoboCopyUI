using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace RoboCopyUI
{
    public partial class MainWindow : Window
    {
        private static readonly string[] Modes =
        {
            "标准复制",
            "复制子目录（不含空目录）",
            "复制子目录（含空目录）",
            "镜像同步（目标与源完全一致）",
            "移动文件（复制后删除源文件）",
            "移动全部（复制后删除源内容）",
            "清理多余文件（删除目标中源没有的内容）"
        };

        private static readonly Brush BNormal = MakeBrush(0xF3, 0xF3, 0xF3);
        private static readonly Brush BMuted = MakeBrush(0x9D, 0x9D, 0x9D);
        private static readonly Brush BAccent = MakeBrush(0x00, 0x78, 0xD4);
        private static readonly Brush BError = MakeBrush(0xF2, 0x6D, 0x5B);
        private static readonly Brush BOk = MakeBrush(0x4C, 0xAF, 0x50);

        private readonly ObservableCollection<LogLine> _logs = new ObservableCollection<LogLine>();
        private Process _proc;
        private bool _running;
        private bool _userStop;

        public MainWindow()
        {
            InitializeComponent();
            cmbMode.ItemsSource = Modes;
            cmbMode.SelectedIndex = 2;
            logList.ItemsSource = _logs;
            NavList.SelectedIndex = 0;
        }

        private static Brush MakeBrush(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        /* ================= 窗口控制 ================= */

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                OnMaximizeRestore(null, null);
                return;
            }
            if (WindowState == WindowState.Normal)
                DragMove();
        }

        private void OnMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeRestore(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            bool max = WindowState == WindowState.Maximized;
            WindowFrame.Margin = max ? new Thickness(0) : new Thickness(22);
            WindowFrame.CornerRadius = max ? new CornerRadius(0) : new CornerRadius(14);
        }




        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnHelpLink(object sender, RequestNavigateEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true }); }
            catch { }
            e.Handled = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_proc != null && !_proc.HasExited)
            {
                if (MessageBox.Show("复制任务仍在执行，退出将终止任务。确定退出？", "确认退出", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                try { _proc.Kill(); }
                catch { }
            }
            base.OnClosing(e);
        }

        /* ================= 导航 ================= */

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageCopy == null) return;
            var item = NavList.SelectedItem as ListBoxItem;
            string tag = item != null ? (item.Tag as string ?? "copy") : "copy";
            PageCopy.Visibility = tag == "copy" ? Visibility.Visible : Visibility.Collapsed;
            PageAdvanced.Visibility = tag == "advanced" ? Visibility.Visible : Visibility.Collapsed;
            PageHelp.Visibility = tag == "help" ? Visibility.Visible : Visibility.Collapsed;
        }

        /* ================= 浏览 ================= */

        private void BrowseSrc(object sender, RoutedEventArgs e) { Browse(txtSrc); }
        private void BrowseDst(object sender, RoutedEventArgs e) { Browse(txtDst); }

        private void Browse(TextBox target)
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog();
            dlg.Title = "请选择目录";
            dlg.Multiselect = false;
            if (dlg.ShowDialog(this) == true)
                target.Text = dlg.FolderName;
        }

        /* ================= 预设 ================= */

        private void OnPreset(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string key = "dry";
            if (btn == btnPresetBackup) key = "backup";
            else if (btn == btnPresetMirror) key = "mirror";
            else if (btn == btnPresetMigrate) key = "migrate";
            ApplyPreset(key);
        }

        private void ApplyPreset(string key)
        {
            if (key == "backup")
            {
                cmbMode.SelectedIndex = 2;
                chkZ.IsChecked = true; chkXJ.IsChecked = true;
                chkMT.IsChecked = true; txtMT.Text = "16";
                chkR.IsChecked = true; txtR.Text = "3";
                chkW.IsChecked = true; txtW.Text = "5";
                chkIPG.IsChecked = false;
                chkB.IsChecked = false; chkZB.IsChecked = false;
                chkFFT.IsChecked = false; chkSEC.IsChecked = false; chkCopyAll.IsChecked = false;
                chkDcopyT.IsChecked = false; chkTimfix.IsChecked = false; chkL.IsChecked = false;
                txtLog.Text = ".\\robocopy.log";
                Status("已应用预设：常规备份");
            }
            else if (key == "mirror")
            {
                cmbMode.SelectedIndex = 3;
                chkZ.IsChecked = false; chkXJ.IsChecked = true;
                chkMT.IsChecked = true; txtMT.Text = "32";
                chkR.IsChecked = true; txtR.Text = "3";
                chkW.IsChecked = true; txtW.Text = "5";
                chkIPG.IsChecked = false;
                chkB.IsChecked = false; chkZB.IsChecked = false;
                chkFFT.IsChecked = false;
                chkSEC.IsChecked = true; chkCopyAll.IsChecked = true;
                chkDcopyT.IsChecked = true; chkTimfix.IsChecked = true; chkL.IsChecked = false;
                txtLog.Text = ".\\mirror.log";
                Status("已应用预设：完整镜像（危险，请先试运行确认）");
            }
            else if (key == "migrate")
            {
                cmbMode.SelectedIndex = 5;
                chkZ.IsChecked = true; chkXJ.IsChecked = true;
                chkMT.IsChecked = true; txtMT.Text = "16";
                chkR.IsChecked = true; txtR.Text = "3";
                chkW.IsChecked = true; txtW.Text = "5";
                chkIPG.IsChecked = false;
                chkB.IsChecked = false; chkZB.IsChecked = false;
                chkFFT.IsChecked = false; chkSEC.IsChecked = false; chkCopyAll.IsChecked = false;
                chkDcopyT.IsChecked = false; chkTimfix.IsChecked = false; chkL.IsChecked = false;
                txtLog.Text = ".\\migrate.log";
                Status("已应用预设：数据迁移（会删除源文件，请确认）");
            }
            else
            {
                cmbMode.SelectedIndex = 2;
                chkZ.IsChecked = false; chkXJ.IsChecked = true;
                chkMT.IsChecked = false;
                chkR.IsChecked = true; txtR.Text = "3";
                chkW.IsChecked = true; txtW.Text = "5";
                chkIPG.IsChecked = false;
                chkB.IsChecked = false; chkZB.IsChecked = false;
                chkFFT.IsChecked = false; chkSEC.IsChecked = false; chkCopyAll.IsChecked = false;
                chkDcopyT.IsChecked = false; chkTimfix.IsChecked = false;
                chkL.IsChecked = true;
                txtLog.Text = "";
                Status("已应用预设：试运行（只预览，不会实际复制）");
            }
            NavList.SelectedIndex = 0;
        }

        private void Status(string text)
        {
            lblStatus.Text = text;
        }

        /* ================= 命令构建 ================= */

        private static string Quote(string p) { return "\"" + p + "\""; }

        private int GetInt(TextBox tb, int def)
        {
            int v;
            return int.TryParse(tb.Text.Trim(), out v) ? v : def;
        }

        private string BuildArgs()
        {
            var a = new System.Collections.Generic.List<string>();
            int mi = cmbMode.SelectedIndex;
            switch (mi)
            {
                case 1: a.Add("/S"); break;
                case 2: a.Add("/E"); break;
                case 3: a.Add("/MIR"); break;
                case 4: a.Add("/MOV"); break;
                case 5: a.Add("/MOVE"); break;
                case 6: a.Add("/E"); a.Add("/PURGE"); break;
            }
            if (chkZ.IsChecked == true) a.Add("/Z");
            if (chkB.IsChecked == true) a.Add("/B");
            if (chkZB.IsChecked == true) a.Add("/ZB");
            if (chkMT.IsChecked == true) a.Add("/MT:" + Math.Max(1, Math.Min(128, GetInt(txtMT, 16))));
            if (chkR.IsChecked == true) a.Add("/R:" + Math.Max(0, GetInt(txtR, 3)));
            if (chkW.IsChecked == true) a.Add("/W:" + Math.Max(0, GetInt(txtW, 5)));
            if (chkXJ.IsChecked == true) a.Add("/XJ");
            if (chkFFT.IsChecked == true) a.Add("/FFT");
            if (chkSEC.IsChecked == true) a.Add("/SEC");
            if (chkCopyAll.IsChecked == true) a.Add("/COPYALL");
            if (chkDcopyT.IsChecked == true) a.Add("/DCOPY:T");
            if (chkTimfix.IsChecked == true) a.Add("/TIMFIX");
            if (chkIPG.IsChecked == true) a.Add("/IPG:" + Math.Max(0, GetInt(txtIPG, 100)));

            string xf = txtXf.Text.Trim();
            if (xf.Length > 0) { a.Add("/XF"); a.Add(xf); }
            string xd = txtXd.Text.Trim();
            if (xd.Length > 0) { a.Add("/XD"); a.Add(xd); }
            string log = txtLog.Text.Trim();
            if (log.Length > 0) a.Add("/LOG:" + log);
            if (chkL.IsChecked == true) a.Add("/L");

            return string.Join(" ", a.ToArray());
        }

        private string BuildCommand()
        {
            string cmd = "robocopy " + Quote(txtSrc.Text.Trim()) + " " + Quote(txtDst.Text.Trim());
            string inc = txtInc.Text.Trim();
            if (inc.Length > 0 && inc != "*.*") cmd += " " + Quote(inc);
            string args = BuildArgs();
            if (args.Length > 0) cmd += " " + args;
            return cmd;
        }

        /* ================= 执行 ================= */

        private void OnStart(object sender, RoutedEventArgs e)
        {
            if (_running) return;
            string src = txtSrc.Text.Trim();
            string dst = txtDst.Text.Trim();
            if (src.Length == 0 || dst.Length == 0)
            {
                MessageBox.Show("请先填写源目录和目标目录。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (chkMT.IsChecked == true && chkIPG.IsChecked == true)
            {
                MessageBox.Show("多线程与限速不能同时使用，请取消其中一项。", "参数冲突", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (chkZ.IsChecked == true && (chkB.IsChecked == true || chkZB.IsChecked == true))
            {
                MessageBox.Show("可重启与备份模式不能同时使用，请只保留其中一种。", "参数冲突", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int mi = cmbMode.SelectedIndex;
            if (mi == 3 || mi == 5 || mi == 6)
            {
                string msg = "当前模式为「" + Modes[mi] + "」，该模式会删除目标目录中源没有的文件/目录！\n\n确定要继续吗？建议先勾选「仅试运行」预览。";
                if (MessageBox.Show(msg, "危险操作确认", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;
            }

            string args = BuildArgs();
            string inc = txtInc.Text.Trim();
            if (inc.Length > 0 && inc != "*.*") args = Quote(inc) + (args.Length > 0 ? " " + args : "");
            string arguments = Quote(src) + " " + Quote(dst) + (args.Length > 0 ? " " + args : "");

            AppendLog(">>> 开始复制任务…", BAccent);
            prog.Value = 0;
            prog.IsIndeterminate = true;
            Status("正在复制…");
            _userStop = false;
            SetRunning(true);

            _proc = new Process();
            _proc.StartInfo.FileName = "robocopy.exe";
            _proc.StartInfo.Arguments = arguments;
            _proc.StartInfo.UseShellExecute = false;
            _proc.StartInfo.RedirectStandardOutput = true;
            _proc.StartInfo.RedirectStandardError = true;
            _proc.StartInfo.CreateNoWindow = true;
            _proc.EnableRaisingEvents = true;
            _proc.OutputDataReceived += ProcOut;
            _proc.ErrorDataReceived += ProcErr;
            _proc.Exited += ProcExited;

            try
            {
                if (!_proc.Start())
                {
                    AppendLog(">>> 无法启动 robocopy。", BError);
                    SetRunning(false);
                    return;
                }
            }
            catch (Exception ex)
            {
                AppendLog(">>> 启动失败: " + ex.Message, BError);
                SetRunning(false);
                return;
            }
            _proc.BeginOutputReadLine();
            _proc.BeginErrorReadLine();
        }

        private void OnStop(object sender, RoutedEventArgs e)
        {
            if (_proc != null && !_proc.HasExited)
            {
                _userStop = true;
                try
                {
                    _proc.Kill();
                    AppendLog(">>> 已请求停止复制。", BError);
                    Status("正在停止…");
                }
                catch (Exception ex)
                {
                    AppendLog(">>> 停止失败: " + ex.Message, BError);
                }
            }
        }

        private void OnCopyCommand(object sender, RoutedEventArgs e)
        {
            string src = txtSrc.Text.Trim();
            string dst = txtDst.Text.Trim();
            if (src.Length == 0 || dst.Length == 0)
            {
                MessageBox.Show("请先填写源目录和目标目录。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string cmd = BuildCommand();
            try
            {
                Clipboard.SetText(cmd);
                Status("命令已复制到剪贴板");
                AppendLog(">>> 命令已复制到剪贴板。", BAccent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("复制到剪贴板失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnClearLog(object sender, RoutedEventArgs e)
        {
            _logs.Clear();
        }

        private void SetRunning(bool r)
        {
            _running = r;
            btnStart.IsEnabled = !r;
            btnStop.IsEnabled = r;
        }

        private void ProcOut(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            HandleLine(e.Data, BNormal);
        }

        private void ProcErr(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            HandleLine(e.Data, BError);
        }

        private void HandleLine(string line, Brush color)
        {
            if (line.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) >= 0)
                color = BError;

            var m = System.Text.RegularExpressions.Regex.Match(line, @"(\d{1,3}(?:\.\d+)?)\s*%");
            if (m.Success)
            {
                double p;
                if (double.TryParse(m.Groups[1].Value, out p))
                {
                    int v = (int)Math.Min(100, Math.Max(0, p));
                    try
                    {
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            prog.IsIndeterminate = false;
                            prog.Value = v;
                        }));
                    }
                    catch { }
                }
            }

            try
            {
                Dispatcher.BeginInvoke(new Action(delegate { AppendLog(line, color); }));
            }
            catch { }
        }

        private void ProcExited(object sender, EventArgs e)
        {
            int code = -1;
            try { code = _proc.ExitCode; }
            catch { }
            _proc = null;

            bool stopped = _userStop;
            _userStop = false;

            try
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    SetRunning(false);
                    prog.IsIndeterminate = false;
                    bool ok = code >= 0 && code <= 7;
                    prog.Value = ok ? 100 : 0;
                    if (stopped)
                    {
                        Status("已停止");
                        AppendLog(">>> 任务已停止。", BError);
                    }
                    else
                    {
                        Status("退出码 " + code + (ok ? "（成功）" : "（出错）"));
                        AppendLog(">>> robocopy 结束，退出码 " + code + "（0-7 成功，8+ 出错）", ok ? BOk : BError);
                        if (!ok)
                            MessageBox.Show("复制过程中出现错误，退出码 " + code + "，请查看上方日志。", "ROBOCOPY", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }));
            }
            catch { }
        }

        private void AppendLog(string text, Brush color)
        {
            var line = new LogLine
            {
                Stamp = DateTime.Now.ToString("HH:mm:ss"),
                Text = text,
                Color = color
            };
            _logs.Add(line);
            if (_logs.Count > 3000) _logs.RemoveAt(0);
            logList.ScrollIntoView(line);
        }
    }

    public class LogLine
    {
        public string Stamp { get; set; }
        public string Text { get; set; }
        public Brush Color { get; set; }
    }
}


