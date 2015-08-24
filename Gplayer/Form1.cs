using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Gplayer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<string> _filePath = new List<string>(); //保存音乐文件的路径

        int currentNum = 0;                                     //保存当前播放的歌曲索引

        List<string> _initInfo = new List<string>();//歌曲初始化信息

        Random r = new Random();
        /// <summary>
        /// 加载时读取配置文件，加载歌曲列表，设置音量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            
            _initInfo.AddRange(File.ReadAllLines(@"init.ini"));
            string[] list = File.ReadAllLines(@"list.txt");
            if (_initInfo.Count==0)
            {
                _initInfo.Add(Path.GetDirectoryName(Path.GetFullPath(@"init.ini")));
                _initInfo.Add("2");
                _initInfo.Add("True");
                File.WriteAllLines(@"init.ini", _initInfo);
            }
            else
            {
                
                for (int j = 0; j < list.Length - 1; j++)
                {
                    listBox1.Items.Add(Path.GetFileNameWithoutExtension(list[j]));
                   
                    _filePath.Add(list[j]);
                }
                if (list.Length != 0)
                {
                    currentNum = int.Parse(list[list.Length - 1]);
                    playerPanel.URL = _filePath[currentNum];
                    listBox1.SelectedIndex = currentNum;
                   
                }
                playerPanel.settings.autoStart = false;

            }
            playerPanel.settings.volume = volumes.Value;
            thd = new Thread(titleMoving);
            thd.IsBackground = true;
            tBoxLrc.SelectionAlignment = HorizontalAlignment.Center;
            bmp.Add(new Bitmap(@"img\单曲.bmp"));
            bmp.Add(new Bitmap(@"img\单曲选中.bmp"));
            bmp.Add(new Bitmap(@"img\循环.bmp"));
            bmp.Add(new Bitmap(@"img\循环选中.bmp"));
            bmp.Add(new Bitmap(@"img\随机.bmp"));
            bmp.Add(new Bitmap(@"img\随机选中.bmp"));
            switch (_initInfo[1])
	        {
                    case "1":
                    pictureBox1.Image=bmp[1];
                     nowPBox =pictureBox1;
                     playingMode = 1;
                    break;
                    case "2":
                    pictureBox2.Image=bmp[3];
                     nowPBox =pictureBox2;
                     playingMode = 2;
                    break;
                    case "3":
                    pictureBox3.Image=bmp[5];
                     nowPBox =pictureBox3;
                     playingMode = 3;
                    break;
		        default:
                break;
	        }
            if (_initInfo[2] == "True")
            {
                pictureBox5.Image = new Bitmap(@"img\歌词选中.bmp");
                tBoxLrc.Visible = true;
                listBox1.Height = listBox1.Height - tBoxLrc.Height;
                DecodingLrc();
            }
            Control.CheckForIllegalCrossThreadCalls = false;
        
        }
        /// <summary>
        /// 双击播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (currentNum!=listBox1.SelectedIndex)
                {
                      currentNum = listBox1.SelectedIndex;
                      SongChanged();
                }
                else
                {
                     iKey = 0;
                }
                
    
                playerPanel.URL = _filePath[currentNum];
                
                Playing();
            }
            catch (Exception err)
            {
             MessageBox.Show(err.Message);
            }

        }
        private void 添加歌曲ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "选择音乐";
            ofd.Multiselect = true;
            ofd.Filter = "mp3|*.mp3|wav|*.wav|所有文件|*.*";
            ofd.InitialDirectory = _initInfo[0];
            ofd.ShowDialog();
            //获得文件的全路径
            string[] path = ofd.FileNames;
            /*将打开的文件和列表中存在的进行比较，如果不同就添
            加并将路径保存到——filePath字段中*/
            listBox1.ClearSelected();
            for (int j = 0; j < path.Length; j++)
            {
                if (!listBox1.Items.Contains(Path.GetFileName(path[j])))
                {
                    listBox1.Items.Add(Path.GetFileNameWithoutExtension(path[j]));
                    _filePath.Add(path[j]);
                }
            }
            if (playerPanel.URL == "")
            {
                playerPanel.URL = _filePath[0];
            }
            if (path.Length != 0)
            {
                _initInfo[0] = Path.GetDirectoryName(path[0]);
            }


        }


        private void 删除歌曲ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = listBox1.SelectedItems.Count;
            for (int i = 0; i < count; i++)
            {
                _filePath.RemoveAt(listBox1.SelectedIndex);
                if (currentNum==listBox1.SelectedIndex)
                {
                    playerPanel.Ctlcontrols.stop();
                    timer1.Enabled = false;
                    play.Text = "播放";
                }
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        
            if (currentNum>=_filePath.Count)
            {
                currentNum = _filePath.Count - 1;
            }
            playerPanel.URL = _filePath[currentNum];
            listBox1.SelectedIndex = currentNum;
        }
        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetForm setForm = new SetForm();
            setForm.Show();
        }
        /// <summary>
        /// 调节音量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void volumes_Scroll(object sender, ScrollEventArgs e)
        {
            playerPanel.settings.volume = volumes.Value;
        }

      
        /// <summary>
        /// 定时更新播放时间和播放状态条,根据播放模式设置歌曲的路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //显示当前播放时间和歌曲总时间************/
            label1.Text = playerPanel.currentMedia.durationString;
            playPosition.Maximum = Convert.ToInt32(playerPanel.currentMedia.duration);
            label2.Text = playerPanel.Ctlcontrols.currentPositionString;
            playPosition.Value = Convert.ToInt32(playerPanel.Ctlcontrols.currentPosition);
            //*************************************//
            if (tBoxLrc.Visible == true && key.Count != 0)
            {
                //   tBoxLrc.Text = "歌词开始显示"+iKey.ToString();
                DisplayLrc();//显示歌词
            }
            if (playerPanel.playState.ToString() == "wmppsStopped")
            {
                thd.Abort();
                switch (playingMode)
                {
                    case 1:
                        iKey = 0;
                        break;
                    case 2:
                        currentNum++;
                        SongChanged();

                        if (currentNum >= _filePath.Count)
                        {
                            currentNum = 0;
                        }                                  
                        break;
                    case 3:
                         int temp;
                        do
                        {
                           temp= r.Next(0, _filePath.Count);
                         }while(currentNum ==temp);
                        currentNum=temp;
                        SongChanged();
                        break;
                }
                playerPanel.URL = _filePath[currentNum];
                Playing();
            }
         
        }

        void titleMoving(object str)
        {
            int i;
            while (true)
            {
              
                  Thread.Sleep(1000);
                for (  i = 0;i <=moreLetterNum; i++)
                {
                    this.Text = ((string)str).Substring(i, 26);
                    Thread.Sleep(1000);
                }  
                Thread.Sleep(1000);
                for (i=i-1; i>=0;i--)
			    {
                    this.Text = ((string)str).Substring(i, 26);
                    Thread.Sleep(1000);                
			    }
            }
           
        }
         
     

        /// <summary>
        /// 播放暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void play_Click(object sender, EventArgs e)
        {
            if (play.Text == "播放")
            {
                if (playerPanel.status=="准备就绪")
                {
                      this.Text = listBox1.SelectedItem.ToString();
                      moreLetterNum = this.Text.Length > 26 ? this.Text.Length - 26 : 0;
                      if (moreLetterNum!=0)
                      {
                          thd.Start(listBox1.SelectedItem.ToString());
                      }
                      
                }
                playerPanel.Ctlcontrols.play();
                play.Text = "暂停";
                listBox1.ClearSelected();
                listBox1.SetSelected(currentNum, true);
                timer1.Enabled = true;
            }
            else
            {
                playerPanel.Ctlcontrols.pause();
                play.Text = "播放";
                timer1.Enabled = false;
            }
        }

        private void pre_Click(object sender, EventArgs e)
        {
            try
            {
                --currentNum;
                 SongChanged();
                if (currentNum < 0)
                {
                    currentNum = _filePath.Count - 1;
                }
                playerPanel.URL = _filePath[currentNum];
                Playing();
            }
            catch (Exception)
            {

                // MessageBox.Show(err.Message);
            }


        }

        private void next_Click(object sender, EventArgs e)
        {
            try
            {
                ++currentNum;
                 SongChanged();
                if (currentNum >= _filePath.Count)
                {
                    currentNum = 0;
                }
                playerPanel.URL = _filePath[currentNum];
                Playing();
            }
            catch (Exception)
            {

                //  MessageBox.Show(err.Message);
            }

        }
   
        /// <summary>
        /// 设置播放的信息，包括播放歌曲的url，将播放设置为暂停，列表当前歌曲设为选择
        /// </summary>
        int moreLetterNum;
        Thread thd;
        void Playing()
        {
            try
            {
              
                playerPanel.Ctlcontrols.play();
                play.Text = "暂停";
                listBox1.ClearSelected();
                listBox1.SetSelected(currentNum, true);
                timer1.Enabled = true;
                this.Text = listBox1.SelectedItem.ToString();
                moreLetterNum = this.Text.Length > 26? this.Text.Length - 26 : 0;
                if (moreLetterNum!=0)
                {
                    thd = new Thread(titleMoving);
                    thd.IsBackground = true;
                    thd.Start(listBox1.SelectedItem.ToString());
                }
            }
            catch (Exception)
            {
                play.Text = "播放";
                MessageBox.Show("请添加音乐");
            }

        }
        void SongChanged()
        {
            lrcSave.Clear();
            key.Clear();
            tBoxLrc.Clear();
            if (thd.IsAlive)
            {
                thd.Abort();
            }
            if (tBoxLrc.Visible == true)
            {
                DecodingLrc();
                 iKey = 0;
            }
        }
     

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_filePath.Count != 0)
            {
                _filePath.Add(currentNum.ToString());
            }
            _initInfo[1] = playingMode.ToString();
            _initInfo[2] = tBoxLrc.Visible.ToString();
            File.WriteAllLines(@"list.txt", _filePath);
            File.WriteAllLines(@"init.ini", _initInfo);
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            PlayingMode(pictureBox1);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            PlayingMode(pictureBox2);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            PlayingMode(pictureBox3);
        }
        List<Bitmap> bmp = new List<Bitmap>();
        PictureBox nowPBox = new PictureBox();
        int playingMode ;
        void PlayingMode(PictureBox pbox)
        {
            if (nowPBox != pbox)
            {
                pbox.Image = bmp[Convert.ToInt32(pbox.Tag)* 2 + 1];
                nowPBox.Image = bmp[2 * Convert.ToInt32(nowPBox.Tag)];
                nowPBox = pbox;
            }
            else
	        {
                 return ;
	        }
            playingMode=Convert.ToInt32(pbox.Tag)+1;
            
        }


        Dictionary<double, string> lrcSave = new Dictionary<double, string>();
        List<double> key=new List<double>();
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            if (tBoxLrc.Visible == false)
            {
                pictureBox5.Image = new Bitmap(@"img\歌词选中.bmp");
                tBoxLrc.Visible = true;
                listBox1.Height = listBox1.Height - tBoxLrc.Height;
                if (key.Count==0)
                {
                      DecodingLrc();
                }
              
            }
            else
            {
                pictureBox5.Image = new Bitmap(@"img\歌词.bmp");
                tBoxLrc.Visible =false;
                listBox1.Height = listBox1.Height + tBoxLrc.Height;
            }
                 
          }
     
        void DecodingLrc()
        {
            try
            {
               string[] lrc = File.ReadAllLines(@"lrc\" + listBox1.Items[currentNum] + ".lrc");
               key.Add(0);
                lrcSave.Add(key[0],( lrc[1].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries))[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                 for (int i = 7; i < lrc.Length; i++)
                {
                    string[] temp1 = lrc[i].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] temp2=temp1[0].Split(new char[]{':'},StringSplitOptions.RemoveEmptyEntries);
                    key.Add (Convert.ToDouble(temp2[0]) * 60+ Convert.ToDouble(temp2[1]));
                    lrcSave.Add(key[i-6], temp1[1]);
                }
              //  tBoxLrc.Text = "歌词加载完毕";
            }
            catch (Exception err)
            {
                switch (err.ToString().Split(new char[]{':'})[0])
                {
                    case "System.IO.FileNotFoundException":
                        tBoxLrc.Text = "没有对应歌曲的歌词文件";
                        break;
                    default:
                        tBoxLrc.Text = "程序出现其他异常";
                        break;
                }
              
            }
        
          
        }
            int  iKey=0;
        void DisplayLrc()
        {
                while ((iKey < key.Count-1) && (playerPanel.Ctlcontrols.currentPosition > key[iKey + 1]))
                {
                    iKey++;
                }
                string value;
                lrcSave.TryGetValue(key[iKey], out value);
                  tBoxLrc.Text = value;
                lrcSave.TryGetValue(key[iKey+1], out value);
                tBoxLrc.Text +="\r\n"+ value;
        }

   
       

   

    }
}
