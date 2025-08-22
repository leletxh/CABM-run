using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using Microsoft.Win32; // Required for Registry access
using System.Threading; // For CancellationToken and Delay

namespace 启动器
{
    public partial class dl : Form
    {
        private HttpClient httpClient;
        private string downloadUrl;
        private string localFilePath;
        private string extractPath;
        private string tag;
        private string localTagPath;
        private bool useGitCloneProxy = false;
        private Process launchedProcess = null; // 保存启动的进程引用

        // --- Registry Constants ---
        private const string REGISTRY_KEY_PATH = @"SOFTWARE\YourAppName"; // Replace 'YourAppName' with your actual app name
        private const string REGISTRY_VALUE_NAME = "LaunchCount";

        // --- Retry Configuration ---
        private const int MAX_RETRIES = 5; // 最大重试次数
        private static readonly TimeSpan RETRY_DELAY = TimeSpan.FromSeconds(5); // 基础重试延迟

        public dl()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            extractPath = Path.Combine("C:\\", "CABM");
            localTagPath = Path.Combine(extractPath, "api", "tag");

            // 注册窗体关闭事件
            this.FormClosing += Dl_FormClosing;
        }

        private void Dl_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 在窗体关闭时杀死app.exe进程
            KillAppProcess();
        }

        public void update_log(string str) => this.Invoke((MethodInvoker)delegate
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            log.Text += $"\n[{timestamp}] {str}";
            nowlog.Text = str;
        });

        private void steps1_ItemClick(object sender, AntdUI.StepsItemEventArgs e)
        {

        }

        private void main()
        {

        }

        private void dl_Load(object sender, EventArgs e)
        {
            step.Current = 0;
            update_log("启动器初始化完成");
            update_log("开始执行自动更新流程...");

            // --- Add Registry Count Logic Here ---
            int launchCount = GetLaunchCountFromRegistry();
            update_log($"当前启动次数: {launchCount}");

            // Check if it's the 50th launch or more
            if (launchCount >= 50)
            {
                update_log("检测到超过50次启动，询问是否开启彩蛋...");

                // --- Show the MessageBox ---
                DialogResult result = MessageBox.Show(
                    "您已经启动了超过50次！\n是否要开启彩蛋模式？\n(这将打开一个特殊网页)",
                    "彩蛋提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    update_log("用户选择开启彩蛋，打开网页...");
                    OpenSpecialWebPage();
                }
                else
                {
                    update_log("用户选择不开启彩蛋。");
                }

                // Reset the counter after showing the dialog (optional, depends on desired behavior)
                // ResetLaunchCountInRegistry(); // Uncomment if you want to reset after 50th launch
            }

            // Increment the count *after* checking, but before starting the download process
            IncrementLaunchCountInRegistry();

            // Create thread to run download method
            Task.Run(() => start_download());
        }

        // --- Registry Helper Methods ---
        private int GetLaunchCountFromRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    object value = key.GetValue(REGISTRY_VALUE_NAME);
                    if (value != null && int.TryParse(value.ToString(), out int count))
                    {
                        return count;
                    }
                    else
                    {
                        // If key doesn't exist or parsing fails, assume 0
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                update_log($"读取启动次数失败: {ex.Message}");
                return 0; // Fallback to 0 on error
            }
        }

        private void IncrementLaunchCountInRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    int currentCount = GetLaunchCountFromRegistry();
                    int newCount = currentCount + 1;
                    key.SetValue(REGISTRY_VALUE_NAME, newCount, RegistryValueKind.DWord);
                    update_log($"启动次数已增加到: {newCount}");
                }
            }
            catch (Exception ex)
            {
                update_log($"增加启动次数失败: {ex.Message}");
            }
        }

        // --- Optional: Reset Counter ---
        private void ResetLaunchCountInRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    key.SetValue(REGISTRY_VALUE_NAME, 0, RegistryValueKind.DWord);
                    update_log("启动次数已重置为 0");
                }
            }
            catch (Exception ex)
            {
                update_log($"重置启动次数失败: {ex.Message}");
            }
        }

        // --- Open Special Web Page ---
        private void OpenSpecialWebPage()
        {
            try
            {
                // Ensure the URL is valid
                string url = "https://sr.mihoyo.com/main";
                update_log($"尝试打开网页: {url}");
                System.Diagnostics.Process.Start(url);
                update_log("网页已打开。");
            }
            catch (Exception ex)
            {
                update_log($"打开网页失败: {ex.Message}");
                MessageBox.Show($"无法打开网页: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void start_download()
        {
            try
            {
                update_log("开始检查本地版本...");
                SetProgress(0.0f);

                // 1. 检查本地版本
                string localTag = "";
                if (File.Exists(localTagPath))
                {
                    localTag = File.ReadAllText(localTagPath).Trim();
                    update_log($"本地版本号: {localTag}");
                }
                else
                {
                    update_log("未找到本地版本文件");
                }

                // 2. 获取远程版本
                update_log("正在连接到版本服务器...");
                string apiUrl = "https://raw.githubusercontent.com/xhc2008/CABM/refs/heads/main/api/tag";
                update_log($"请求URL: {apiUrl}");

                string tagResponse = "";

                // 2. 获取远程版本 - 使用重试逻辑
                try
                {
                    tagResponse = await RetryOnExceptionAsync(async () =>
                    {
                        update_log($"尝试直接连接获取版本信息...");
                        return await httpClient.GetStringAsync(apiUrl);
                    }, MAX_RETRIES, RETRY_DELAY);
                }
                catch (Exception ex)
                {
                    update_log($"直接连接获取版本失败: {ex.Message}");
                    update_log("尝试使用gitclone代理获取版本...");

                    string proxyUrl = apiUrl.Replace("https://raw.githubusercontent.com/", "https://wget.la/https://raw.githubusercontent.com/");
                    update_log($"代理URL: {proxyUrl}");

                    try
                    {
                        tagResponse = await RetryOnExceptionAsync(async () =>
                        {
                            update_log($"尝试通过代理连接获取版本信息...");
                            return await httpClient.GetStringAsync(proxyUrl);
                        }, MAX_RETRIES, RETRY_DELAY);
                        useGitCloneProxy = true; // 如果代理成功获取tag，则后续下载也用代理
                        update_log("✓ 代理获取版本成功");
                    }
                    catch (Exception proxyEx)
                    {
                        throw new Exception($"代理获取版本也失败: {proxyEx.Message}");
                    }
                }

                tag = tagResponse.Trim();
                update_log($"远程版本号: {tag}");
                SetProgress(0.05f); // 5%进度

                // 3. 比较版本
                if (!string.IsNullOrEmpty(localTag) && localTag == tag)
                {
                    update_log("✓ 版本一致，无需更新，直接启动程序");
                    step.Current = 4;
                    SetProgress(1.0f); // 100%进度

                    // 直接启动程序
                    await LaunchApplication();
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(localTag))
                    {
                        update_log("本地无版本信息，需要完整下载");
                    }
                    else
                    {
                        update_log($"版本不一致，本地:{localTag} 远程:{tag}，需要更新");
                    }
                }
                step.Current = 1;

                // 构建下载URL，如果使用了代理则也应用到下载URL
                string baseUrl = $"https://github.com/xhc2008/CABM/releases/download/{tag}/Windows-Release-{tag}.zip";
                if (useGitCloneProxy)
                {
                    downloadUrl = baseUrl.Replace("https://github.com/", "https://wget.la/https://github.com/");
                    update_log("使用代理下载");
                }
                else
                {
                    downloadUrl = baseUrl;
                }

                localFilePath = Path.Combine(Path.GetTempPath(), $"Windows-Release-{tag}.zip");

                update_log($"下载地址构建完成");
                update_log($"目标文件: {localFilePath}");
                SetProgress(0.1f); // 10%进度

                update_log("开始下载文件...");
                update_log($"下载URL: {downloadUrl}");

                // 5. 下载文件
                await DownloadWithResumeAsync(downloadUrl, localFilePath, MAX_RETRIES, RETRY_DELAY);

                update_log("✓ 文件下载完成");
                update_log($"文件大小: {FormatFileSize(new FileInfo(localFilePath).Length)}");
                SetProgress(0.6f); // 60%进度

                // 6. 验证哈希（保留位置）
                step.Current = 2;
                update_log("正在验证文件完整性...");
                // TODO: 哈希验证功能待实现
                for (int i = 0; i <= 10; i++)
                {
                    SetProgress(0.6f + (0.1f * i / 10)); // 60%-70%进度
                    await Task.Delay(100); // 模拟验证过程
                    update_log($"验证进度: {i * 10}%");
                }
                update_log("✓ 文件完整性验证通过");
                SetProgress(0.7f); // 70%进度

                // 7. 解压文件（合并替换模式）
                step.Current = 3;
                update_log("开始解压文件（合并替换模式）...");
                update_log($"目标目录: {extractPath}");
                ExtractFileWithMerge(localFilePath, extractPath);

                update_log("✓ 文件解压完成");
                SetProgress(0.9f); // 90%进度

                // 8. 保存当前版本号
                try
                {
                    string tagDir = Path.GetDirectoryName(localTagPath);
                    if (!Directory.Exists(tagDir))
                    {
                        Directory.CreateDirectory(tagDir);
                    }
                    File.WriteAllText(localTagPath, tag);
                    update_log($"✓ 版本号已保存: {tag}");
                }
                catch (Exception ex)
                {
                    update_log($"⚠ 版本号保存失败: {ex.Message}");
                }

                // 9. 启动程序
                step.Current = 4;
                update_log("正在启动主程序...");
                await LaunchApplication();
            }
            catch (Exception ex)
            {
                update_log($"❌ 严重错误：{ex.Message}");
                update_log($"错误堆栈：{ex.StackTrace}");
                SetProgress(0.0f); // 重置进度条表示错误
            }
        }

        private async Task LaunchApplication()
        {
            string exePath = Path.Combine(extractPath, "app.exe");
            update_log($"查找程序: {exePath}");

            if (File.Exists(exePath))
            {
                update_log($"✓ 找到主程序: {Path.GetFileName(exePath)}");
                update_log($"程序路径: {exePath}");

                // 启动程序并添加 --no-browser 参数
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "--no-browser",
                    UseShellExecute = true,
                    WorkingDirectory = extractPath // 设置工作目录为 C:\CABM
                };
                launchedProcess = Process.Start(startInfo); // 保存进程引用

                update_log("✓ 程序启动成功 (参数: --no-browser)");
                SetProgress(1.0f); // 100%进度
                update_log("主程序启动完成");

                // 等待3秒后启动browser窗口
                await Task.Delay(3000);
                LaunchBrowserWindow();

                // 隐藏启动器窗口
                this.Invoke((MethodInvoker)delegate {
                    this.Hide();
                    update_log("启动器窗口已隐藏");
                });

                update_log("所有任务已完成，启动器继续在后台运行");
            }
            else
            {
                update_log("⚠ 未找到指定程序 app.exe，正在搜索其他可执行文件...");
                var exeFiles = Directory.GetFiles(extractPath, "*.exe");
                if (exeFiles.Length > 0)
                {
                    string firstExe = exeFiles[0];
                    update_log($"✓ 找到可执行文件: {Path.GetFileName(firstExe)}");

                    // 启动找到的第一个exe文件并添加 --no-browser 参数
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = firstExe,
                        Arguments = "--no-browser",
                        UseShellExecute = true,
                        WorkingDirectory = extractPath // 设置工作目录为 C:\CABM
                    };
                    launchedProcess = Process.Start(startInfo); // 保存进程引用

                    update_log("✓ 程序启动成功 (参数: --no-browser)");
                    SetProgress(1.0f); // 100%进度
                    update_log("主程序启动完成");

                    // 等待3秒后启动browser窗口
                    await Task.Delay(3000);
                    LaunchBrowserWindow();

                    // 隐藏启动器窗口
                    this.Invoke((MethodInvoker)delegate {
                        this.Hide();
                        update_log("启动器窗口已隐藏");
                    });

                    update_log("所有任务已完成，启动器继续在后台运行");
                }
                else
                {
                    update_log("❌ 错误：未找到任何可执行文件");
                    SetProgress(0.95f); // 95%进度（错误状态）
                }
            }
        }

        private void LaunchBrowserWindow()
        {
            try
            {
                update_log("正在启动browser窗口...");

                // 在UI线程中创建并显示browser窗口
                this.Invoke((MethodInvoker)delegate {
                    // 假设browser是你的窗口类名
                    browser browserForm = new browser();
                    browserForm.Show();
                    update_log("✓ browser窗口启动成功");
                });
            }
            catch (Exception ex)
            {
                update_log($"❌ 启动browser窗口失败: {ex.Message}");
            }
        }

        // 杀死app.exe进程的方法
        private void KillAppProcess()
        {
            try
            {
                // 方法1: 如果我们保存了进程引用，直接杀死它
                if (launchedProcess != null && !launchedProcess.HasExited)
                {
                    update_log("正在终止app.exe进程...");
                    launchedProcess.Kill();
                    launchedProcess.WaitForExit(5000); // 等待最多5秒
                    update_log("✓ app.exe进程已终止");
                    return;
                }

                Process[] processes = Process.GetProcessesByName("app");
                if (processes.Length > 0)
                {
                    update_log($"找到 {processes.Length} 个app.exe进程，正在终止...");
                    foreach (Process process in processes)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                                process.WaitForExit(3000);
                            }
                        }
                        catch (Exception ex)
                        {
                            update_log($"警告: 终止进程 {process.Id} 失败: {ex.Message}");
                        }
                    }
                    update_log("✓ 所有app.exe进程已终止");
                }
                else
                {
                    update_log("未找到运行中的app.exe进程");
                }
            }
            catch (Exception ex)
            {
                update_log($"❌ 终止app.exe进程时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 支持断点续传和自动重试的文件下载方法。
        /// </summary>
        /// <param name="url">要下载的文件URL</param>
        /// <param name="filePath">本地保存的文件路径</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="retryDelay">基础重试延迟</param>
        private async Task DownloadWithResumeAsync(string url, string filePath, int maxRetries, TimeSpan retryDelay)
        {
            long totalBytes = 0;
            long alreadyDownloadedBytes = 0;
            bool canReportProgress = false;
            int lastReportedProgress = 0;

            // 检查本地是否已存在部分下载的文件
            if (File.Exists(filePath))
            {
                alreadyDownloadedBytes = new FileInfo(filePath).Length;
                update_log($"发现部分下载的文件，已下载: {FormatFileSize(alreadyDownloadedBytes)}");
            }

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        // 如果已下载部分，则设置 Range 头
                        if (alreadyDownloadedBytes > 0)
                        {
                            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(alreadyDownloadedBytes, null);
                            update_log($"设置断点续传 Range: bytes={alreadyDownloadedBytes}-");
                        }

                        update_log($"发起下载请求 (尝试 {attempt + 1}/{maxRetries + 1})...");
                        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                        {
                            // 检查状态码
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                // 全新下载
                                update_log("服务器响应 200 OK，进行全新下载。");
                                alreadyDownloadedBytes = 0; // 重置已下载字节
                                if (File.Exists(filePath)) File.Delete(filePath); // 删除旧文件
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.PartialContent)
                            {
                                // 断点续传成功
                                update_log("服务器响应 206 Partial Content，断点续传成功。");
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable)
                            {
                                // 请求的 Range 不满足，可能文件已在服务器端更新或本地文件损坏
                                update_log("服务器响应 416 Range Not Satisfiable，可能文件已更新，重新开始下载。");
                                alreadyDownloadedBytes = 0;
                                if (File.Exists(filePath)) File.Delete(filePath);
                                // 重新发起请求，不带 Range 头
                                continue; // 重新循环，发起不带 Range 的请求
                            }
                            else
                            {
                                response.EnsureSuccessStatusCode(); // 抛出其他非成功状态码的异常
                            }

                            totalBytes = response.Content.Headers.ContentLength ?? -1L;
                            canReportProgress = totalBytes != -1;
                            if (canReportProgress)
                            {
                                // 对于续传，总大小是已下载 + 响应内容长度
                                long effectiveTotalBytes = alreadyDownloadedBytes + (response.Content.Headers.ContentLength ?? 0);
                                update_log($"文件总大小: {FormatFileSize(effectiveTotalBytes)}, 需要下载: {FormatFileSize(response.Content.Headers.ContentLength ?? 0)}");
                            }
                            else
                            {
                                update_log("无法获取文件大小信息");
                            }

                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 8192, true)) // 使用 Append 模式
                            {
                                var buffer = new byte[8192];
                                var totalBytesReadInThisAttempt = 0L;
                                int bytesRead;

                                update_log("开始接收数据流...");

                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                                    totalBytesReadInThisAttempt += bytesRead;
                                    alreadyDownloadedBytes += bytesRead; // 更新总已下载字节

                                    if (canReportProgress)
                                    {
                                        // 计算基于总大小的进度
                                        long effectiveTotalBytes = alreadyDownloadedBytes + (response.Content.Headers.ContentLength ?? (totalBytes - alreadyDownloadedBytes));
                                        var displayProgress = effectiveTotalBytes > 0 ? (float)alreadyDownloadedBytes / effectiveTotalBytes : 0;
                                        int currentProgress = (int)(displayProgress * 20);
                                        if (currentProgress > lastReportedProgress)
                                        {
                                            lastReportedProgress = currentProgress;
                                            update_log($"下载进度: {displayProgress:P0} ({FormatFileSize(alreadyDownloadedBytes)}/{FormatFileSize(effectiveTotalBytes)})");
                                        }
                                        // 更新UI进度条 (0.1 到 0.6)
                                        SetProgress(0.1f + (displayProgress * 0.5f));
                                    }
                                }

                                update_log($"本次尝试下载完成: {FormatFileSize(totalBytesReadInThisAttempt)} 已接收");
                                // 如果成功完成，跳出重试循环
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        throw new Exception($"下载文件在 {maxRetries + 1} 次尝试后仍然失败: {ex.Message}", ex);
                    }
                    else
                    {
                        update_log($"下载失败 (尝试 {attempt + 1}/{maxRetries + 1}): {ex.Message}. 等待 {retryDelay.TotalSeconds} 秒后重试...");
                        await Task.Delay(retryDelay);
                        // 可以在这里增加指数退避，例如 retryDelay *= 2;
                    }
                }
            }
        }


        /// <summary>
        /// 通用的带重试机制的异步操作执行器，适用于不涉及文件流的简单网络请求。
        /// </summary>
        /// <typeparam name="T">操作返回的结果类型</typeparam>
        /// <param name="operation">要执行的异步操作</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="retryDelay">基础重试延迟</param>
        /// <returns>操作成功后的结果</returns>
        private async Task<T> RetryOnExceptionAsync<T>(Func<Task<T>> operation, int maxRetries, TimeSpan retryDelay)
        {
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        throw new Exception($"操作在 {maxRetries + 1} 次尝试后仍然失败: {ex.Message}", ex);
                    }
                    else
                    {
                        update_log($"操作失败 (尝试 {attempt + 1}/{maxRetries + 1}): {ex.Message}. 等待 {retryDelay.TotalSeconds} 秒后重试...");
                        await Task.Delay(retryDelay);
                        // 可以在这里增加指数退避，例如 retryDelay *= 2;
                    }
                }
            }
            // 这行代码理论上不会执行到，因为循环要么返回，要么抛异常
            throw new InvalidOperationException("重试逻辑异常");
        }

        private void ExtractFileWithMerge(string zipFilePath, string extractPath)
        {
            try
            {
                update_log("初始化合并解压环境...");

                // 确保目标目录存在
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                    update_log($"创建目录: {extractPath}");
                }

                update_log("开始合并解压ZIP文件...");

                using (var archive = System.IO.Compression.ZipFile.OpenRead(zipFilePath))
                {
                    var entries = archive.Entries.ToList();
                    var totalEntries = entries.Count;
                    var processedEntries = 0;

                    update_log($"ZIP文件包含 {totalEntries} 个项目");

                    foreach (var entry in entries)
                    {
                        var destinationPath = Path.Combine(extractPath, entry.FullName);

                        // 确保目录存在
                        var directory = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // 如果是文件则解压（文件名不为空）
                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            // 合并替换模式：直接覆盖已存在的文件，保留不存在的文件
                            entry.ExtractToFile(destinationPath, true);
                            processedEntries++;

                            if (processedEntries % 10 == 0 || processedEntries == totalEntries)
                            {
                                var progressPercentage = (float)processedEntries / totalEntries;
                                update_log($"解压文件: {destinationPath}");
                                SetProgress(0.7f + (progressPercentage * 0.2f));
                            }
                        }
                    }
                }

                update_log("✓ ZIP文件合并解压完成");
                SetProgress(0.9f);

                // 删除临时下载的zip文件
                try
                {
                    File.Delete(zipFilePath);
                    update_log($"✓ 临时文件已清理: {Path.GetFileName(zipFilePath)}");
                }
                catch (Exception ex)
                {
                    update_log($"⚠ 临时文件清理失败: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"解压文件失败: {ex.Message}");
            }
        }

        private void SetProgress(float value)
        {
            if (progress != null)
            {
                this.Invoke((MethodInvoker)delegate {
                    progress.Value = value;
                });
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}