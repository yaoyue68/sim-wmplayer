using AxWMPLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mpvplayer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //跨线程
            Control.CheckForIllegalCrossThreadCalls = false;
        }


        //声明歌曲列表字典
        List<string> slist = new List<string>();
        //全局目录，暂时没用到
        string gdir = null;
        //listbox1全局记数
        int i = 0;

        /// <summary>
        /// 播放按钮实现功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            //判断音乐库slist有没有音乐，没有则结束
            if (slist.Count == 0)
            {
                return;
            }

            //listbox1选中非空
            if (listBox1.SelectedItem != null)
            {
                //判断是否是一首歌
                if (Path.GetFileName(axWindowsMediaPlayer1.URL) != listBox1.SelectedItem.ToString())
                {
                    axWindowsMediaPlayer1.URL = slist[listBox1.SelectedIndex];

                    axWindowsMediaPlayer1.Ctlcontrols.play();


                    button1.Text = "暂停";
                }
                else
                {
                    if (button1.Text == "播放")
                    {

                        axWindowsMediaPlayer1.Ctlcontrols.play();
                        button1.Text = "暂停";
                    }
                    else
                    {
                        axWindowsMediaPlayer1.Ctlcontrols.pause();
                        button1.Text = "播放";
                    }

                }
            }

        }

        /// <summary>
        /// 选中歌曲索引改变时间，自动加载歌词，判断是否暂停和继续
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //切歌时清除歌词内容和时间
            dlrc.Clear();
            slrc.Clear();
            //加载新歌词
            checkBox1_CheckedChanged(this, e);


            try        //try防止出错
            {

                if (Path.GetFileName(axWindowsMediaPlayer1.URL) == listBox1.SelectedItem.ToString())
                {
                    button1.Text = "暂停";
                }
                else
                {
                    button1.Text = "播放";
                }
            }
            catch
            {

            }

        }

        /// <summary>
        /// 添加音乐按钮实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Multiselect = true;
            of.InitialDirectory = @"Z:\音乐";
            gdir = of.InitialDirectory;

            //多类型支持写法，用;隔开
            of.Filter = "支持的格式|*.mp3;*.wav;*.flac|mp3格式|*.mp3|wav格式|*.wav|flac格式|*.flac";
            of.ShowDialog();


            if (of.FileNames.Length != 0)
            {
                foreach (var item in of.FileNames)
                {

                    //去重
                    if (!slist.Contains(item))
                    {
                        slist.Add(item);
                        //添加进listbox                   
                        listBox1.Items.Add(Path.GetFileName(item));
                        i++;
                    }
                }
            }
        }
        /// <summary>
        /// 程序启动时暂停自动播放，默认选择顺序播放模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.autoStart = false;
            radioButton3.Checked = true;

            //  label2.Image = Image.FromFile(@"D:\Users\yaoyue\Desktop\1.png");



        }
        /// <summary>
        /// 列表双击播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                axWindowsMediaPlayer1.URL = slist[listBox1.SelectedIndex];
                axWindowsMediaPlayer1.Ctlcontrols.play();
                button1.Text = "暂停";
            }
            catch
            {


            }


        }
        /// <summary>
        /// 随机播放实现，见 ax1状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                
            }
            else
            {

            }

        }

        /// <summary>
        /// 单曲循环模式选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                axWindowsMediaPlayer1.settings.setMode("loop", true);
            }
        }

        //歌词歌曲的list
        List<double> dlrc = new List<double>();
        List<string> slrc = new List<string>();
        /// <summary>
        /// 歌词格式化函数
        /// </summary>
        public void Lrcc()
        {
            try
            {

                string lrcpath = Path.ChangeExtension(axWindowsMediaPlayer1.URL, ".lrc");
                //将歌词文件读取为数组
                string[] lrcalltext = File.ReadAllLines(lrcpath, Encoding.Default);

                double zs = 0;

                foreach (var item in lrcalltext)
                {
                    //只筛选出歌词
                    if (Regex.IsMatch(item, @"[0-9][0-9]:[0-9][0-9].[0-9][0-9]"))
                    {
                        // a.tostring（）为字符串
                        Match a = Regex.Match(item, @"[0-9][0-9]:[0-9][0-9].[0-9][0-9]");
                        Match b = Regex.Match(item, @"[^\d.\[\]:].{0,50}");

                        string time = a.ToString();
                        //分钟的字符形式
                        string minute = time.Split(new char[] { ':' })[0];
                        //秒的字符形式
                        string second = time.Split(new char[] { ':', ']' })[1];

                        //分钟 转成秒
                        double dm = double.Parse(minute) * 60;
                        //秒转换
                        double ds = double.Parse(second);
                        //总秒数
                        zs = dm + ds;
                        dlrc.Add(zs);
                        //歌词
                        string sb = b.ToString();
                        slrc.Add(sb);
                    }
                }

            }
            catch
            {

            }
        }



        /// <summary>
        /// 选中“显示歌词”事件实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked==true)
            {
                Lrcc();
            }
         

        }

      /// <summary>
      /// 上一首
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>      
        private void button2_Click(object sender, EventArgs e)
        {
            int j = 0;
            //因为listbox为多选模式，所以清空所有索引，之前先转存到index
            int index = listBox1.SelectedIndex;
            listBox1.SelectedIndices.Clear();

            if (index > j)
            {

                listBox1.SelectedIndex = index - 1;
            }
            else if (index == j)
            {

                listBox1.SelectedIndex = 0;
            }

            //模拟播放按钮按下
            button1_Click(this, e);


        }
        /// <summary>
        ///  下一首
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {

            int j = slist.Count;
            //清空所有索引之前先转存到index
            int index = listBox1.SelectedIndex;
            listBox1.SelectedIndices.Clear();

            if (index < j - 1)
            {
                listBox1.SelectedIndex = index + 1;
            }
            else if (index == j)
            {
                listBox1.SelectedIndex = j - 1;
            }

            //模拟播放按钮按下
            button1_Click(this, e);
        }


        /// <summary>
        /// 右键删除，从list后面开始顺序不容易乱
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //盛放选中索引
            List<int> sort = new List<int>();

            foreach (var item in listBox1.SelectedIndices)
            {
                sort.Add((int)item);
            }

            //从后面的索引开始删除，排序
            sort.Sort();
            sort.Reverse();

            for (int i = 0; i < sort.Count; i++)
            {

                listBox1.Items.RemoveAt(sort[i]);
                slist.RemoveAt(sort[i]);

            }

        }

        /// <summary>
        /// 右键添加音乐，直接调用添加音乐的btn按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 添加ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            button3_Click(this, e);
        }

     
/// <summary>
/// 音量调节大小，label设置成图片
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
        private void label2_Click(object sender, EventArgs e)
        {
            if (label2.Tag.ToString() == "1")
            {
                axWindowsMediaPlayer1.settings.mute = true;
                label2.Tag = "2";
                //   label2.Image = Image.FromFile(@"D:\Users\yaoyue\Desktop\2.png");
            }
            else
            {
                axWindowsMediaPlayer1.settings.mute = false;
                label2.Tag = "1";
                //   label2.Image = Image.FromFile(@"D:\Users\yaoyue\Desktop\1.png");

            }
        }

        /// <summary>
        /// 列表循环---音乐播放结束时自动播放下一首
        /// </summary>
        public void ListPlayer()
        {
            try
            {

                int i = listBox1.SelectedIndex;

                //判断当前是否是列表最后一项
                if (i < listBox1.Items.Count - 1)
                {
                    axWindowsMediaPlayer1.URL = slist[i + 1];

                    //因为selectindex为多选模式，所以必须clear
                    listBox1.SelectedIndices.Clear();

                    listBox1.SelectedIndex = i + 1;

                }
                else   //重新开始
                {
                    listBox1.SelectedIndices.Clear();
                    listBox1.SelectedIndex = 0;
                    axWindowsMediaPlayer1.URL = slist[0];
                }

            }
            catch
            {
            }

        }
        /// <summary>
        /// 随机播放的功能函数
        /// </summary>
        public void RandomPlayer()
        {

            Random r = new Random();
            int i = r.Next(0, slist.Count);
            axWindowsMediaPlayer1.URL = slist[i];
            listBox1.SelectedIndices.Clear();
            listBox1.SelectedIndex = i;

        }
        /// <summary>
        /// 播放器状态改变事件，播放模式核心实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axWindowsMediaPlayer1_StatusChange(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                //在选中事件中实现，不在此事件
                return;
            }
            else if (radioButton1.Checked == true && axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                //随机模式
                RandomPlayer();

            }
            else
            {

                if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
                {
                    //顺序模式
                    ListPlayer();

                }
            }


        }

        /// <summary>
        /// 修复wmplayer的不自动播放bug
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            //播放结束时有bug，播放器状态进入准备状态，而不是播放，强制播放
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsReady)
            {
                try
                {
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                }
                catch
                { }
            }
        }

      
        /// <summary>
        /// 托盘右键 暂停和继续
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 暂停ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();
                contextMenuStrip2.Items[0].Text = "继续";
            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
                contextMenuStrip2.Items[0].Text = "暂停";
            }

        }
        /// <summary>
        /// 托盘图标双击暂停，继续
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();

            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();

            }
        }
        /// <summary>
        /// 右键退出功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要退出？", "question", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                // 1.this.Close(); 只是关闭当前窗口，若不是主窗体的话，是无法退出程序的，另外若有托管线程（非主线程），也无法干净地退出； 

                //2.Application.Exit(); 强制所有消息中止，退出所有的窗体，但是若有托管线程（非主线程），也无法干净地退出； 

                //3.Application.ExitThread(); 强制中止调用线程上的所有消息，同样面临其它线程无法正确退出的问题； 

                //4.System.Environment.Exit(0); 这是最彻底的退出方式，不管什么线程都被强制退出，把程序结束的很干净。


                Application.Exit();
                //System.Environment.Exit(0);
                //this.Close();
            }
            else
            {

            }
        }
        /// <summary>
        /// 托盘右键显示窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {


            if (contextMenuStrip2.Items[1].Text == "显示")
            {
                this.Show();
                //还原窗体显示    
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点
                this.Activate();
                //任务栏区显示图标
                this.ShowInTaskbar = true;
                //托盘区图标隐藏
                // notifyIcon1.Visible = false;
                contextMenuStrip2.Items[1].Text = "隐藏";
            }
            else
            {
                contextMenuStrip2.Items[1].Text = "显示";
                // Form1_Deactivate(this, e);
                this.notifyIcon1.Visible = true; //显示托盘图标
                this.Hide();//隐藏窗体
                this.ShowInTaskbar = false;//图标不显示在任务栏

            }

        }

      
        /// <summary>
        /// 最小化到托盘，注意deactive事件使用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            //当窗体为最小化状态时
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon1.Visible = true; //显示托盘图标
                this.Hide();//隐藏窗体
                this.ShowInTaskbar = false;//图标不显示在任务栏
            }
        }

        /// <summary>
        /// 关闭窗口提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("确定关闭？", "", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            {
                e.Cancel = true;

            }


            // 窗体的Closing事件，里面如果使用Application.Exit()，会弹出两次对话框询问:
            //这个是很正常的，当执行Application.Exit()时，就激活窗体的关闭事件，从而调用该事件的处理程序StartForm_FormClosing，你这样写相当于递归调用
            //修改如下：
            //private void StartForm_FormClosing(object sender, FormClosingEventArgs e)
            //        {
            //            if (DialogResult.Cancel == MessageBox.Show("确认退出？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information))
            //            {
            //                e.Cancel = true;
            //            }
            //        }
            //或者用System.Environment.Exit(0);

        }

/// <summary>
/// 右键打开音乐目录，没有实现自动选定文件
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
        private void 打开目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", Path.GetDirectoryName(slist[listBox1.SelectedIndex]));//打开D盘
        }

        /// <summary>
        /// 歌词实现，每隔一段时间自动for筛选出匹配歌词
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (File.Exists(Path.ChangeExtension(axWindowsMediaPlayer1.URL, ".lrc")))
            {
                double mctime = axWindowsMediaPlayer1.Ctlcontrols.currentPosition;

                //筛选功能，有次数限制，否则窗体假死
                for (int i = 0; i < dlrc.Count; i++)
                {
                    if (mctime > dlrc[i] && mctime < dlrc[i + 1])
                    {
                        label1.Text = slrc[i];
                    }

                }

            }

            else if(axWindowsMediaPlayer1.playState== WMPLib.WMPPlayState.wmppsPlaying)
            {
                label1.Text = "没有歌词文件！";
            }
            else
            {
                label1.Text = "等待中";

            }


            }
  
    }
    }


