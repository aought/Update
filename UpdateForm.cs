using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MyUpdate.Entity;
using MyUpdate.Utils;
using System.IO;
using System.Threading;

namespace MyUpdate
{
    public partial class UpdateForm : MyBaseForm
    {

        private bool isDelete=true;
        private bool runningLock = false;
        private Thread thread;

        public UpdateForm()
        {
            InitializeComponent();
        }

        private void UpdateForm_Load(object sender, EventArgs e)
        {
            CloseApp();

            if (CheckUpdate())
            {
                if (!Backup())
                {
                    MessageBox.Show("备份失败！");
                    btnStart.Enabled = false;
                    isDelete = true;
                    return;
                }
                else
                {
                    MessageBox.Show("备份成功");
                }

            }
            else
            {
                MessageBox.Show("暂时无更新");
                this.btnFinish.Enabled = true;
                this.btnStart.Enabled = false;
                isDelete = false;
                this.Close();
            }
        }

        private void UpdateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (runningLock )
            {
                if (MessageBox.Show("升级还在进行中，中断升级会导致程序不可用，是否中断",
                          "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                {
                    if (thread != null) thread.Abort();
                    isDelete = true;
                    AppParameter.IsRunning = false;
                }
                else 
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (isDelete) File.Delete(AppParameter.LocalUPdateConfig);

            StartApp();
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        // 点击更新按钮事件
        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            UpdateApp();
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void UpdateApp()
        {
            int successCount = 0;
            int failCount = 0;
            int itemIndex = 0;
            // 获取更新文件的配置参数
            List<FileENT> list = ConfigHelper.GetUpdateList();
            if (list.Count == 0)
            {
                MessageBox.Show("版本已是最新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.btnFinish.Enabled = true;
                this.btnStart.Enabled = false;
                isDelete = false;
                this.Close();
                return;
            }


            // 单个线程
            thread = new Thread(new ThreadStart(delegate
            {
                #region thread method

                FileENT ent = null;

                while (true)
                {
                    lock (this)
                    {
                        if (itemIndex >= list.Count)
                            break;
                        ent = list[itemIndex];

                        string msg = string.Empty;
                        if (ExecUpdateItem(ent))
                        {
                            msg = ent.FileFullName + "更新成功";
                            successCount++;
                        }
                        else
                        {
                            msg = ent.FileFullName + "更新失败";
                            failCount++;
                        }

                        if (this.InvokeRequired)
                        {
                            this.Invoke((Action)delegate()
                            {
                                listBox1.Items.Add(msg);
                                int val = (int)Math.Ceiling(1f / list.Count * 100);
                                progressBar1.Value = progressBar1.Value + val > 100 ? 100 : progressBar1.Value + val;
                            });
                        }


                        itemIndex++;
                        if (successCount + failCount == list.Count && this.InvokeRequired)
                        {
                            string finishMessage = string.Empty;
                            if (this.InvokeRequired)
                            {
                                this.Invoke((Action)delegate()
                                {
                                    btnFinish.Enabled = true;
                                });
                            }
                            isDelete = failCount != 0;
                            if (!isDelete)
                            {
                                AppParameter.Version = list.Last().Version;
                                ConfigHelper.UpdateAppConfig("version", AppParameter.Version);
                                finishMessage = "升级完成，程序已成功升级到" + AppParameter.Version;
                            }
                            else
                                finishMessage = "升级完成，但不成功";
                            MessageBox.Show(finishMessage, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            runningLock = false;
                        }
                    }
                }
                #endregion
            }));
            runningLock = true;
            thread.Start();

        }

        /// <summary>
        /// 执行单个更新
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public bool ExecUpdateItem(FileENT ent)
        {
            bool result = true;

            try
            {

                if (ent.Option == UpdateOption.del)
                    File.Delete(ent.FileFullName);
                else
                    // 下载更新文件到主程序目录
                    // ent.Src："ftp://192.168.2.113/"
                    // AppParameter.MainPath："C:\\Users\\Empty\\Desktop\\Debug"
                    // ent.FileFullName："Smart.FormDesigner.Demo.exe"
                    FtpHelper.FTPDownLoadFile(ent.Src, AppParameter.MainPath, ent.FileFullName);
            }
            catch { result = false; }
            return result;
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <returns></returns>
        public static bool CheckUpdate()
        {
            // result：是否更新标志；
            bool result = false;

            // 第一个参数：服务器地址；第二个参数：服务器上下载文件名；第三个参数：客户端下载保存文件名；第四个参数：客户端地址
            // AppParameter.ServerURL参数示例："ftp://192.168.2.113/"
            // AppParameter.LocalPath参数示例："C:\\Users\\Empty\\Documents\\GitHub\\Update\\bin\\Debug\\"
            FtpHelper.FTPDownLoadFile(AppParameter.ServerURL, "updateconfig.xml", "temp_config.xml", AppParameter.LocalPath);
            
            // 如果本地不存在更新配置文件返回true，即需要更新；
            if (!File.Exists(AppParameter.LocalUPdateConfig))
            {
                result = true;
            }
            // 本地如果存在更新文件，则需比对客户端和服务器端的文件；
            else
            {
                // 通过哈希值比对文件
                result = !FileCompareHelper.FileCompare(AppParameter.LocalUPdateConfig, AppParameter.LocalPath + "temp_config.xml");
            }

            // 将拉取的服务器配置文件保存为本地的更新文件，并删除临时文件；
            if (result)
            {
                if (File.Exists(AppParameter.LocalUPdateConfig)) File.Delete(AppParameter.LocalUPdateConfig);
                File.Copy(AppParameter.LocalPath + "temp_config.xml", AppParameter.LocalUPdateConfig);
            }
            else
                result = false;
            File.Delete(AppParameter.LocalPath + "temp_config.xml");
            return result;
        }

        /// <summary>
        /// 备份
        /// </summary>
        /// <returns></returns>
        public static bool Backup()
        {
            // 以"yyyy-MM-dd HH_mm_ss_v_version.rar"方式命名，保存在指定备份目录；
            string sourcePath = Path.Combine(AppParameter.BackupPath, DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss")+"_v_"+AppParameter.Version + ".rar");
            return ZipHelper.Zip(AppParameter.MainPath.Trim() , sourcePath);
        }
    }
}
