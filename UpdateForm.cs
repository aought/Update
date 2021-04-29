using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MyUpdate.Entity;
using MyUpdate.Utils;
using System.IO;
using System.Threading;
using System.Configuration;

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
            //List<FileENT> list = ConfigHelper.GetUpdateList();
            //if (list.Count > Convert.ToInt32(ConfigurationManager.AppSettings["counts"]))
            //{
            //    ConfigHelper.UpdateAppConfig("version", "0");
            //    MessageBox.Show("新添文件");
            //}
            //else
            //{
            //    ConfigHelper.UpdateAppConfig("counts", list.Count.ToString());
            //}
            UpdateApp();
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void UpdateApp()
        {
            // TODO：修改比对版本号逻辑


            int successCount = 0;
            int failCount = 0;
            int itemIndex = 0;
            // 获取更新文件的配置参数
            List<FileENT> list = ConfigHelper.GetUpdateList();

            if (list.Count >= Convert.ToInt32(ConfigurationManager.AppSettings["counts"]))
            {
                ConfigHelper.UpdateAppConfig("version", "0");
                // MessageBox.Show("新添文件");
            }
            else
            {
                ConfigHelper.UpdateAppConfig("counts", list.Count.ToString());
            }

            //if (list.Count == 0)
            //{
            //    MessageBox.Show("版本已是最新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    this.btnFinish.Enabled = true;
            //    this.btnStart.Enabled = false;
            //    isDelete = false;
            //    this.Close();
            //    return;
            //}

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
                                // TODO: 此处修改版本号逻辑，选取所以文件中最低的一个版本号作为总的版本号
                                // 即主程序只要有一个文件更新，那么整个程序更新，不做增量更新
                                // 这样也不用存储旧的配置文件清单了
                                // AppParameter.Version = list.Last().Version;
                                AppParameter.Version = ConfigHelper.GetVersion().ToString();
                                ConfigHelper.UpdateAppConfig("version", AppParameter.Version);
                                string times = (Convert.ToInt32(ConfigurationManager.AppSettings["times"]) + 1).ToString();
                                ConfigHelper.UpdateAppConfig("times", times);
                                // MessageBox.Show(times);
                                // finishMessage = "升级完成，程序已成功升级到" + AppParameter.Version;
                                finishMessage = "升级完成，程序已成功升级到" + times;
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

            // test:"C:\\Users\\Empty\\Documents\\GitHub\\Update\\bin\\"
            string test = ent.Src.Replace(ConfigurationManager.AppSettings["serverURL"], AppParameter.parentFolder).Replace("bin/VersionFolder", "").Replace("/", "\\");
            // MessageBox.Show(test);

            //string temp = ent.Src.Replace(ConfigurationManager.AppSettings["serverURL"], "").Replace("/", "\\");
            //string test = AppParameter.parentFolder + temp;
            //test = test.Replace("\\VersionFolder", "");

            if (!Directory.Exists(test))
            {
                Directory.CreateDirectory(test);
            }


            try
            {

                if (ent.Option == UpdateOption.del)
                    File.Delete(ent.FileFullName);
                else
                    // 下载更新文件到主程序目录
                    // 此处传入文件夹需修改
                    // ent.Src：ftp://localhost/bin/VersionFolder/sub
                    // ConfigurationManager.AppSettings["serverURL"]：ftp://localhost
                    // AppParameter.parentFolder：C:\\Users\\Empty\\Documents\\GitHub\\Update\\bin
                    // 目标文件夹：C:\Users\Empty\Documents\GitHub\Update\bin\sub
                    
                    FtpHelper.FTPDownLoadFile(ent.Src, test, ent.FileFullName);
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
            // 默认需要更新
            bool result = true;

            // 第一个参数：服务器地址；第二个参数：服务器上下载文件名；第三个参数：客户端下载保存文件名；第四个参数：客户端地址
            // AppParameter.ServerURL	"ftp://localhost/bin"
            // AppParameter.LocalPath	"C:\\Users\\Empty\\Documents\\GitHub\\Update\\bin\\Debug\\"
            FtpHelper.FTPDownLoadFile(AppParameter.ServerURL, "updateconfig.xml", "temp_config.xml", AppParameter.LocalPath);
            
            // 如果本地不存在更新配置文件返回true，即需要更新；
            if (!File.Exists(AppParameter.LocalUPdateConfig))
            {
                // 将拉取的服务器配置文件保存为本地的更新文件，并删除临时文件；
                File.Copy(AppParameter.LocalPath + "temp_config.xml", AppParameter.LocalUPdateConfig);
                // result = true;
            }
            // 本地如果存在更新文件，则需比对客户端和服务器端的文件；
            else
            {
                // 通过哈希值比对文件，两个文件相同result返回false，即不需要更新；
                // flag为true，两文件相同；flag为false，两文件不同，
                bool flag = FileCompareHelper.FileCompare(AppParameter.LocalUPdateConfig, AppParameter.LocalPath + "temp_config.xml");
                // 两文件不同，将本地配置清单拷贝成旧的配置清单，本地配置清单更新为服务器上的配置清单
                if (!flag)
                {
                    //if (File.Exists(AppParameter.oldConfig))
                    //    File.Delete(AppParameter.oldConfig);
                    //File.Copy(AppParameter.LocalUPdateConfig, AppParameter.oldConfig);

                    if (File.Exists(AppParameter.LocalUPdateConfig))
                        File.Delete(AppParameter.LocalUPdateConfig);
                    File.Copy(AppParameter.tempConfig, AppParameter.LocalUPdateConfig);
                }
                result = !flag;
            }

            //if (result)
            //{
            //    if (File.Exists(AppParameter.LocalUPdateConfig)) File.Delete(AppParameter.LocalUPdateConfig);
            //    File.Copy(AppParameter.LocalPath + "temp_config.xml", AppParameter.LocalUPdateConfig);
            //}
            //else
            //    result = false;

            File.Delete(AppParameter.LocalPath + "temp_config.xml");

            return result;
        }

        /// <summary>
        /// 备份
        /// </summary>
        /// <returns></returns>
        public static bool Backup()
        {
            // 判断文件夹是否存在，不存在则创建
            if (!Directory.Exists(AppParameter.BackupPath))
            {
                Directory.CreateDirectory(AppParameter.BackupPath);
            }

            // 以"yyyy-MM-dd HH_mm_ss_v_version.rar"方式命名，保存在指定备份目录；
            // string sourcePath = Path.Combine(AppParameter.BackupPath, DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss")+"_v_"+AppParameter.Version + ".rar");
            string sourcePath = Path.Combine(AppParameter.BackupPath, DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss") + "_v_" + ConfigurationManager.AppSettings["times"] + ".rar");
            // string temp = AppParameter.parentFolder.Trim();
            // AppParameter.MainPath	"C:\\Users\\Empty\\Documents\\GitHub\\Update\\bin\\Debug
            // AppParameter.parentFolder	"C:\\Users\\Empty\\Documents\\GitHub\\Update\\bin"
            return ZipHelper.Zip(AppParameter.parentFolder.Trim() , sourcePath);
            // return ZipHelper.Zip(AppParameter.parentFolder.Trim(), sourcePath);
        }
    }
}
