// ===== MCSManager 動態開關功能示例代碼 =====
// 此文件包含可直接集成到 Form1 的代碼

namespace Mirle.Agvc.Simulator
{
    public partial class Form1 : Form
    {
        // ===== UI 控件聲明（在 Form1.cs 中添加這些字段）=====
        private System.Windows.Forms.Button btnEnableMCS;
        private System.Windows.Forms.Button btnDisableMCS;
        private System.Windows.Forms.Label lblMCSStatus;
        private System.Windows.Forms.CheckBox chbMCSManager;
        
        // ===== 初始化 UI 控件（在 InitializeComponent 中調用此方法）=====
        /// <summary>
        /// 初始化 MCSManager 動態開關 UI 控件
        /// </summary>
        private void InitializeMCSManagerControls()
        {
            // 選項 A：使用按鈕控制
            InitializeMCSManagerButtons();
            
            // 選項 B：使用複選框控制（可選）
            // InitializeMCSManagerCheckbox();
        }

        // ===== 方案 A：按鈕控制方案 =====
        private void InitializeMCSManagerButtons()
        {
            // 建立「啟用 MCS」按鈕
            btnEnableMCS = new System.Windows.Forms.Button();
            btnEnableMCS.Text = "啟用 MCS";
            btnEnableMCS.Location = new System.Drawing.Point(350, 500);
            btnEnableMCS.Size = new System.Drawing.Size(100, 30);
            btnEnableMCS.TabIndex = 999;
            btnEnableMCS.Click += BtnEnableMCS_Click;
            this.Controls.Add(btnEnableMCS);

            // 建立「禁用 MCS」按鈕
            btnDisableMCS = new System.Windows.Forms.Button();
            btnDisableMCS.Text = "禁用 MCS";
            btnDisableMCS.Location = new System.Drawing.Point(460, 500);
            btnDisableMCS.Size = new System.Drawing.Size(100, 30);
            btnDisableMCS.TabIndex = 1000;
            btnDisableMCS.Click += BtnDisableMCS_Click;
            this.Controls.Add(btnDisableMCS);

            // 建立狀態標籤
            lblMCSStatus = new System.Windows.Forms.Label();
            lblMCSStatus.Text = "MCS 狀態: 已啟用";
            lblMCSStatus.Location = new System.Drawing.Point(350, 470);
            lblMCSStatus.Size = new System.Drawing.Size(200, 25);
            lblMCSStatus.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            lblMCSStatus.ForeColor = System.Drawing.Color.Green;
            this.Controls.Add(lblMCSStatus);
        }

        // ===== 方案 B：複選框控制方案 =====
        private void InitializeMCSManagerCheckbox()
        {
            chbMCSManager = new System.Windows.Forms.CheckBox();
            chbMCSManager.Text = "啟用 MCSManager";
            chbMCSManager.Checked = true;
            chbMCSManager.Location = new System.Drawing.Point(350, 500);
            chbMCSManager.Size = new System.Drawing.Size(150, 25);
            chbMCSManager.TabIndex = 1001;
            chbMCSManager.CheckedChanged += ChbMCSManager_CheckedChanged;
            this.Controls.Add(chbMCSManager);
        }

        // ===== 事件處理：按鈕方式 =====
        private void BtnEnableMCS_Click(object sender, EventArgs e)
        {
            try
            {
                if (scApp == null)
                {
                    MessageBox.Show("應用未正確初始化");
                    return;
                }

                scApp.SetMCSManagerEnabled(true);
                lblMCSStatus.Text = "MCS 狀態: 已啟用";
                lblMCSStatus.ForeColor = System.Drawing.Color.Green;
                logger.Info("MCSManager 已成功啟用");
                MessageBox.Show("MCSManager 已成功啟用");
            }
            catch (Exception ex)
            {
                logger.Error($"啟用 MCSManager 失敗: {ex.Message}");
                MessageBox.Show($"啟用 MCSManager 失敗: {ex.Message}");
            }
        }

        private void BtnDisableMCS_Click(object sender, EventArgs e)
        {
            try
            {
                if (scApp == null)
                {
                    MessageBox.Show("應用未正確初始化");
                    return;
                }

                scApp.SetMCSManagerEnabled(false);
                lblMCSStatus.Text = "MCS 狀態: 已禁用";
                lblMCSStatus.ForeColor = System.Drawing.Color.Red;
                logger.Info("MCSManager 已成功禁用");
                MessageBox.Show("MCSManager 已成功禁用");
            }
            catch (Exception ex)
            {
                logger.Error($"禁用 MCSManager 失敗: {ex.Message}");
                MessageBox.Show($"禁用 MCSManager 失敗: {ex.Message}");
            }
        }

        // ===== 事件處理：複選框方式 =====
        private void ChbMCSManager_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (scApp == null)
                {
                    MessageBox.Show("應用未正確初始化");
                    chbMCSManager.CheckedChanged -= ChbMCSManager_CheckedChanged;
                    chbMCSManager.Checked = !chbMCSManager.Checked;
                    chbMCSManager.CheckedChanged += ChbMCSManager_CheckedChanged;
                    return;
                }

                bool isEnabled = chbMCSManager.Checked;
                scApp.SetMCSManagerEnabled(isEnabled);
                string status = isEnabled ? "已啟用" : "已禁用";
                logger.Info($"MCSManager {status}");
            }
            catch (Exception ex)
            {
                logger.Error($"切換 MCSManager 狀態失敗: {ex.Message}");
                MessageBox.Show($"切換 MCSManager 狀態失敗: {ex.Message}");
                
                // 恢復複選框狀態
                chbMCSManager.CheckedChanged -= ChbMCSManager_CheckedChanged;
                chbMCSManager.Checked = !chbMCSManager.Checked;
                chbMCSManager.CheckedChanged += ChbMCSManager_CheckedChanged;
            }
        }

        // ===== 查詢 MCSManager 狀態的便利方法 =====
        public bool IsMCSManagerEnabled()
        {
            if (scApp == null)
                return false;
            return scApp.GetMCSManagerEnabled();
        }

        // ===== 在 FormClosing 事件中添加清理代碼 =====
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 在程式關閉前禁用 MCSManager
                if (scApp != null && scApp.GetMCSManagerEnabled())
                {
                    scApp.SetMCSManagerEnabled(false);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"程式關閉時清理 MCSManager 失敗: {ex.Message}");
            }
        }
    }
}

/* 
===== 集成步驟 =====

1. 在 Form1.cs 中找到 Form1 的字段聲明部分（在其他 private 字段之後）
   複製以下字段聲明到該部分：
   
   private System.Windows.Forms.Button btnEnableMCS;
   private System.Windows.Forms.Button btnDisableMCS;
   private System.Windows.Forms.Label lblMCSStatus;
   private System.Windows.Forms.CheckBox chbMCSManager;

2. 在 InitializeComponent() 的末尾（在 ResumeLayout(false) 之後）
   添加以下調用：
   
   InitializeMCSManagerControls();

3. 複製上述所有事件處理方法到 Form1.cs 中

4. 如需要，在 Form1.cs 中找到 Form_Load 或 Form_Closing 事件，
   在 Form_Closing 事件中添加 FormClosing 代碼

5. 編譯並測試

===== 使用流程 =====

基本流程：
① 應用啟動時 MCSManager 自動啟用
② 用戶可通過按鈕或複選框動態啟用/禁用 MCSManager
③ 每次切換都會記錄到日誌
④ 應用關閉時自動清理資源

*/
