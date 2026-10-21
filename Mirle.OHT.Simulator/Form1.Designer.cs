namespace Mirle.Agvc.Simulator
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            dgvOht = new System.Windows.Forms.DataGridView();
            ip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            vh_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            btnAlarmSet = new System.Windows.Forms.Button();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            txtIp = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            btnConnect = new System.Windows.Forms.Button();
            btnDisConnect = new System.Windows.Forms.Button();
            timer1 = new System.Windows.Forms.Timer(components);
            btnCleanRichTextBox = new System.Windows.Forms.Button();
            txtError = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            radioButton_Reject41 = new System.Windows.Forms.RadioButton();
            radioButton_Reject31 = new System.Windows.Forms.RadioButton();
            radioButton_InterlockErrorUnload = new System.Windows.Forms.RadioButton();
            Scan_ReadFail = new System.Windows.Forms.RadioButton();
            Scan_Mismatche = new System.Windows.Forms.RadioButton();
            radioButton_MTL = new System.Windows.Forms.RadioButton();
            radioButton_InterlockError = new System.Windows.Forms.RadioButton();
            radioButton_Abort = new System.Windows.Forms.RadioButton();
            radioButton_Cancel = new System.Windows.Forms.RadioButton();
            radioButton_Abnormal_DoubleStorage = new System.Windows.Forms.RadioButton();
            radioButton_Abnormal_EmptyRetrieval = new System.Windows.Forms.RadioButton();
            radioButton_Abnormal_BcrDuplicate = new System.Windows.Forms.RadioButton();
            radioButton_Abnormal_BcrMisMatch = new System.Windows.Forms.RadioButton();
            radioButton_Abnormal_BcrReadFail = new System.Windows.Forms.RadioButton();
            radioButton_Normal = new System.Windows.Forms.RadioButton();
            btnSet = new System.Windows.Forms.Button();
            Mismatch_ID = new System.Windows.Forms.TextBox();
            set_button = new System.Windows.Forms.Button();
            label8 = new System.Windows.Forms.Label();
            tbx_init_add_id = new System.Windows.Forms.ComboBox();
            label9 = new System.Windows.Forms.Label();
            tbx_init_sec_id = new System.Windows.Forms.ComboBox();
            textBox3 = new System.Windows.Forms.TextBox();
            label10 = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            textBox4 = new System.Windows.Forms.TextBox();
            textBox5 = new System.Windows.Forms.TextBox();
            label12 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            stopSet_button = new System.Windows.Forms.Button();
            stopClear_button = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            txtAlarmCode = new System.Windows.Forms.TextBox();
            label26 = new System.Windows.Forms.Label();
            cmbAlarmLevel = new System.Windows.Forms.ComboBox();
            btnAlarmClear = new System.Windows.Forms.Button();
            txtCstID = new System.Windows.Forms.TextBox();
            label13 = new System.Windows.Forms.Label();
            btnCstSet = new System.Windows.Forms.Button();
            btnCstClear = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            txtPort = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            txtLocalPort = new System.Windows.Forms.TextBox();
            label14 = new System.Windows.Forms.Label();
            txtLocalIP = new System.Windows.Forms.TextBox();
            button1 = new System.Windows.Forms.Button();
            label15 = new System.Windows.Forms.Label();
            tbxMs = new System.Windows.Forms.TextBox();
            btnOn = new System.Windows.Forms.Button();
            btnOff = new System.Windows.Forms.Button();
            chbSetFlyToTarget = new System.Windows.Forms.CheckBox();
            btnSetJump = new System.Windows.Forms.Button();
            chbDisableLocChk = new System.Windows.Forms.CheckBox();
            btnDisableLocCheck = new System.Windows.Forms.Button();
            label16 = new System.Windows.Forms.Label();
            Button_BD_Car_Set = new System.Windows.Forms.Button();
            button3Button_BD_Car_ReSet = new System.Windows.Forms.Button();
            btnSecsCmdStart = new System.Windows.Forms.Button();
            btnSecsCmdStop = new System.Windows.Forms.Button();
            label17 = new System.Windows.Forms.Label();
            label18 = new System.Windows.Forms.Label();
            btn_DisCon = new System.Windows.Forms.Button();
            btn_disConStop = new System.Windows.Forms.Button();
            label19 = new System.Windows.Forms.Label();
            label20 = new System.Windows.Forms.Label();
            txBox_ConTime = new System.Windows.Forms.TextBox();
            txBox_DisConTime = new System.Windows.Forms.TextBox();
            label21 = new System.Windows.Forms.Label();
            tbxDelayTime = new System.Windows.Forms.TextBox();
            btSetDelayTime = new System.Windows.Forms.Button();
            txtIOThread = new System.Windows.Forms.TextBox();
            txtWorkThread = new System.Windows.Forms.TextBox();
            label22 = new System.Windows.Forms.Label();
            label23 = new System.Windows.Forms.Label();
            label24 = new System.Windows.Forms.Label();
            btThreadStart = new System.Windows.Forms.Button();
            txtVhID = new System.Windows.Forms.ComboBox();
            label25 = new System.Windows.Forms.Label();
            bt_sendTeachData = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            bt_setTeachReply = new System.Windows.Forms.Button();
            rb_teachNG = new System.Windows.Forms.RadioButton();
            rb_teachOk = new System.Windows.Forms.RadioButton();
            chbInSectionLoad = new System.Windows.Forms.CheckBox();
            grpConnection = new System.Windows.Forms.GroupBox();
            grpInitPos = new System.Windows.Forms.GroupBox();
            grpContinuousSend = new System.Windows.Forms.GroupBox();
            grpSimOptions = new System.Windows.Forms.GroupBox();
            grpCST = new System.Windows.Forms.GroupBox();
            grpAlarm = new System.Windows.Forms.GroupBox();
            grpStopBreakdown = new System.Windows.Forms.GroupBox();
            label5 = new System.Windows.Forms.Label();
            grpSECS = new System.Windows.Forms.GroupBox();
            grpDisconnect = new System.Windows.Forms.GroupBox();
            grpThread = new System.Windows.Forms.GroupBox();
            grpTeaching = new System.Windows.Forms.GroupBox();
            grpMismatchID = new System.Windows.Forms.GroupBox();
            grpDelay = new System.Windows.Forms.GroupBox();
            Send_136_Initial = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvOht).BeginInit();
            statusStrip1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            grpConnection.SuspendLayout();
            grpInitPos.SuspendLayout();
            grpContinuousSend.SuspendLayout();
            grpSimOptions.SuspendLayout();
            grpCST.SuspendLayout();
            grpAlarm.SuspendLayout();
            grpStopBreakdown.SuspendLayout();
            grpSECS.SuspendLayout();
            grpDisconnect.SuspendLayout();
            grpThread.SuspendLayout();
            grpTeaching.SuspendLayout();
            grpMismatchID.SuspendLayout();
            grpDelay.SuspendLayout();
            SuspendLayout();
            // 
            // dgvOht
            // 
            dgvOht.AllowUserToAddRows = false;
            dgvOht.AllowUserToDeleteRows = false;
            dgvOht.AllowUserToResizeColumns = false;
            dgvOht.AllowUserToResizeRows = false;
            dgvOht.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOht.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { ip, vh_id });
            dgvOht.Location = new System.Drawing.Point(14, 119);
            dgvOht.Margin = new System.Windows.Forms.Padding(4);
            dgvOht.Name = "dgvOht";
            dgvOht.ReadOnly = true;
            dgvOht.RowHeadersVisible = false;
            dgvOht.RowHeadersWidth = 51;
            dgvOht.RowTemplate.Height = 24;
            dgvOht.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvOht.Size = new System.Drawing.Size(327, 453);
            dgvOht.TabIndex = 3;
            // 
            // ip
            // 
            ip.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            ip.HeaderText = "IP";
            ip.MinimumWidth = 6;
            ip.Name = "ip";
            ip.ReadOnly = true;
            ip.Width = 130;
            // 
            // vh_id
            // 
            vh_id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            vh_id.HeaderText = "Vehicle ID";
            vh_id.MinimumWidth = 6;
            vh_id.Name = "vh_id";
            vh_id.ReadOnly = true;
            // 
            // btnAlarmSet
            // 
            btnAlarmSet.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnAlarmSet.Location = new System.Drawing.Point(9, 129);
            btnAlarmSet.Margin = new System.Windows.Forms.Padding(4);
            btnAlarmSet.Name = "btnAlarmSet";
            btnAlarmSet.Size = new System.Drawing.Size(82, 33);
            btnAlarmSet.TabIndex = 59;
            btnAlarmSet.Text = "Set";
            btnAlarmSet.UseVisualStyleBackColor = true;
            btnAlarmSet.Click += btnAlarmSet_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel1, toolStripStatusLabel2, toolStripStatusLabel3 });
            statusStrip1.Location = new System.Drawing.Point(0, 839);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            statusStrip1.Size = new System.Drawing.Size(1155, 24);
            statusStrip1.TabIndex = 6;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new System.Drawing.Size(76, 19);
            toolStripStatusLabel1.Text = "Dis-Connect";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new System.Drawing.Size(1052, 19);
            toolStripStatusLabel2.Spring = true;
            toolStripStatusLabel2.Text = " ";
            // 
            // toolStripStatusLabel3
            // 
            toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            toolStripStatusLabel3.Size = new System.Drawing.Size(10, 19);
            toolStripStatusLabel3.Text = " ";
            // 
            // txtIp
            // 
            txtIp.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txtIp.Location = new System.Drawing.Point(93, 22);
            txtIp.Margin = new System.Windows.Forms.Padding(4);
            txtIp.Name = "txtIp";
            txtIp.Size = new System.Drawing.Size(139, 25);
            txtIp.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.Location = new System.Drawing.Point(9, 27);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(70, 17);
            label2.TabIndex = 8;
            label2.Text = "Remote IP";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label3.Location = new System.Drawing.Point(438, 27);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(76, 18);
            label3.TabIndex = 10;
            label3.Text = "Vehicle ID";
            // 
            // btnConnect
            // 
            btnConnect.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnConnect.Location = new System.Drawing.Point(665, 20);
            btnConnect.Margin = new System.Windows.Forms.Padding(4);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(93, 33);
            btnConnect.TabIndex = 13;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnDisConnect
            // 
            btnDisConnect.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnDisConnect.Location = new System.Drawing.Point(665, 60);
            btnDisConnect.Margin = new System.Windows.Forms.Padding(4);
            btnDisConnect.Name = "btnDisConnect";
            btnDisConnect.Size = new System.Drawing.Size(93, 33);
            btnDisConnect.TabIndex = 14;
            btnDisConnect.Text = "Disconnect";
            btnDisConnect.UseVisualStyleBackColor = true;
            btnDisConnect.Click += btnDisConnect_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // btnCleanRichTextBox
            // 
            btnCleanRichTextBox.Location = new System.Drawing.Point(671, 246);
            btnCleanRichTextBox.Margin = new System.Windows.Forms.Padding(4);
            btnCleanRichTextBox.Name = "btnCleanRichTextBox";
            btnCleanRichTextBox.Size = new System.Drawing.Size(175, 36);
            btnCleanRichTextBox.TabIndex = 28;
            btnCleanRichTextBox.Text = "Clean RichTextBox";
            btnCleanRichTextBox.UseVisualStyleBackColor = true;
            btnCleanRichTextBox.Click += btnCleanRichTextBox_Click;
            // 
            // txtError
            // 
            txtError.AutoSize = true;
            txtError.Location = new System.Drawing.Point(-117, -133);
            txtError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            txtError.Name = "txtError";
            txtError.Size = new System.Drawing.Size(0, 16);
            txtError.TabIndex = 41;
            txtError.Visible = false;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(radioButton_Reject41);
            groupBox2.Controls.Add(radioButton_Reject31);
            groupBox2.Controls.Add(radioButton_InterlockErrorUnload);
            groupBox2.Controls.Add(Scan_ReadFail);
            groupBox2.Controls.Add(Scan_Mismatche);
            groupBox2.Controls.Add(radioButton_MTL);
            groupBox2.Controls.Add(radioButton_InterlockError);
            groupBox2.Controls.Add(radioButton_Abort);
            groupBox2.Controls.Add(radioButton_Cancel);
            groupBox2.Controls.Add(radioButton_Abnormal_DoubleStorage);
            groupBox2.Controls.Add(radioButton_Abnormal_EmptyRetrieval);
            groupBox2.Controls.Add(radioButton_Abnormal_BcrDuplicate);
            groupBox2.Controls.Add(radioButton_Abnormal_BcrMisMatch);
            groupBox2.Controls.Add(radioButton_Abnormal_BcrReadFail);
            groupBox2.Controls.Add(radioButton_Normal);
            groupBox2.Controls.Add(btnSet);
            groupBox2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            groupBox2.Location = new System.Drawing.Point(350, 119);
            groupBox2.Margin = new System.Windows.Forms.Padding(4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4);
            groupBox2.Size = new System.Drawing.Size(283, 453);
            groupBox2.TabIndex = 43;
            groupBox2.TabStop = false;
            groupBox2.Text = "Opera Set";
            // 
            // radioButton_Reject41
            // 
            radioButton_Reject41.AutoSize = true;
            radioButton_Reject41.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Reject41.Location = new System.Drawing.Point(128, 350);
            radioButton_Reject41.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Reject41.Name = "radioButton_Reject41";
            radioButton_Reject41.Size = new System.Drawing.Size(87, 22);
            radioButton_Reject41.TabIndex = 54;
            radioButton_Reject41.Tag = "EnumAddress.Client";
            radioButton_Reject41.Text = "Reject 41";
            radioButton_Reject41.UseVisualStyleBackColor = true;
            radioButton_Reject41.CheckedChanged += radioButton_Reject41_CheckedChanged;
            // 
            // radioButton_Reject31
            // 
            radioButton_Reject31.AutoSize = true;
            radioButton_Reject31.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Reject31.Location = new System.Drawing.Point(12, 350);
            radioButton_Reject31.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Reject31.Name = "radioButton_Reject31";
            radioButton_Reject31.Size = new System.Drawing.Size(87, 22);
            radioButton_Reject31.TabIndex = 53;
            radioButton_Reject31.Tag = "EnumAddress.Client";
            radioButton_Reject31.Text = "Reject 31";
            radioButton_Reject31.UseVisualStyleBackColor = true;
            // 
            // radioButton_InterlockErrorUnload
            // 
            radioButton_InterlockErrorUnload.AutoSize = true;
            radioButton_InterlockErrorUnload.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_InterlockErrorUnload.Location = new System.Drawing.Point(12, 290);
            radioButton_InterlockErrorUnload.Margin = new System.Windows.Forms.Padding(4);
            radioButton_InterlockErrorUnload.Name = "radioButton_InterlockErrorUnload";
            radioButton_InterlockErrorUnload.Size = new System.Drawing.Size(171, 22);
            radioButton_InterlockErrorUnload.TabIndex = 52;
            radioButton_InterlockErrorUnload.Tag = "EnumAddress.Client";
            radioButton_InterlockErrorUnload.Text = "InterlockErrorUnload";
            radioButton_InterlockErrorUnload.UseVisualStyleBackColor = true;
            // 
            // Scan_ReadFail
            // 
            Scan_ReadFail.AutoSize = true;
            Scan_ReadFail.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            Scan_ReadFail.Location = new System.Drawing.Point(175, 320);
            Scan_ReadFail.Margin = new System.Windows.Forms.Padding(4);
            Scan_ReadFail.Name = "Scan_ReadFail";
            Scan_ReadFail.Size = new System.Drawing.Size(72, 22);
            Scan_ReadFail.TabIndex = 51;
            Scan_ReadFail.Tag = "EnumAddress.Client";
            Scan_ReadFail.Text = "Scan-F";
            Scan_ReadFail.UseVisualStyleBackColor = true;
            // 
            // Scan_Mismatche
            // 
            Scan_Mismatche.AutoSize = true;
            Scan_Mismatche.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            Scan_Mismatche.Location = new System.Drawing.Point(82, 320);
            Scan_Mismatche.Margin = new System.Windows.Forms.Padding(4);
            Scan_Mismatche.Name = "Scan_Mismatche";
            Scan_Mismatche.Size = new System.Drawing.Size(78, 22);
            Scan_Mismatche.TabIndex = 50;
            Scan_Mismatche.Tag = "EnumAddress.Client";
            Scan_Mismatche.Text = "Scan-M";
            Scan_Mismatche.UseVisualStyleBackColor = true;
            // 
            // radioButton_MTL
            // 
            radioButton_MTL.AutoSize = true;
            radioButton_MTL.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_MTL.Location = new System.Drawing.Point(12, 320);
            radioButton_MTL.Margin = new System.Windows.Forms.Padding(4);
            radioButton_MTL.Name = "radioButton_MTL";
            radioButton_MTL.Size = new System.Drawing.Size(55, 22);
            radioButton_MTL.TabIndex = 49;
            radioButton_MTL.Tag = "EnumAddress.Client";
            radioButton_MTL.Text = "MTL";
            radioButton_MTL.UseVisualStyleBackColor = true;
            // 
            // radioButton_InterlockError
            // 
            radioButton_InterlockError.AutoSize = true;
            radioButton_InterlockError.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_InterlockError.Location = new System.Drawing.Point(12, 261);
            radioButton_InterlockError.Margin = new System.Windows.Forms.Padding(4);
            radioButton_InterlockError.Name = "radioButton_InterlockError";
            radioButton_InterlockError.Size = new System.Drawing.Size(154, 22);
            radioButton_InterlockError.TabIndex = 49;
            radioButton_InterlockError.Tag = "EnumAddress.Client";
            radioButton_InterlockError.Text = "InterlockErrorLoad";
            radioButton_InterlockError.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abort
            // 
            radioButton_Abort.AutoSize = true;
            radioButton_Abort.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Abort.Location = new System.Drawing.Point(12, 233);
            radioButton_Abort.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Abort.Name = "radioButton_Abort";
            radioButton_Abort.Size = new System.Drawing.Size(130, 22);
            radioButton_Abort.TabIndex = 48;
            radioButton_Abort.Tag = "EnumAddress.Server";
            radioButton_Abort.Text = "AbortComplete";
            radioButton_Abort.UseVisualStyleBackColor = true;
            // 
            // radioButton_Cancel
            // 
            radioButton_Cancel.AutoSize = true;
            radioButton_Cancel.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Cancel.Location = new System.Drawing.Point(12, 203);
            radioButton_Cancel.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Cancel.Name = "radioButton_Cancel";
            radioButton_Cancel.Size = new System.Drawing.Size(136, 22);
            radioButton_Cancel.TabIndex = 47;
            radioButton_Cancel.Tag = "EnumAddress.Server";
            radioButton_Cancel.Text = "CancelComplete";
            radioButton_Cancel.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_DoubleStorage
            // 
            radioButton_Abnormal_DoubleStorage.AutoSize = true;
            radioButton_Abnormal_DoubleStorage.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Abnormal_DoubleStorage.Location = new System.Drawing.Point(12, 144);
            radioButton_Abnormal_DoubleStorage.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Abnormal_DoubleStorage.Name = "radioButton_Abnormal_DoubleStorage";
            radioButton_Abnormal_DoubleStorage.Size = new System.Drawing.Size(203, 22);
            radioButton_Abnormal_DoubleStorage.TabIndex = 47;
            radioButton_Abnormal_DoubleStorage.Tag = "EnumAddress.Server";
            radioButton_Abnormal_DoubleStorage.Text = "Abnormal_DoubleStorage";
            radioButton_Abnormal_DoubleStorage.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_EmptyRetrieval
            // 
            radioButton_Abnormal_EmptyRetrieval.AutoSize = true;
            radioButton_Abnormal_EmptyRetrieval.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Abnormal_EmptyRetrieval.Location = new System.Drawing.Point(12, 173);
            radioButton_Abnormal_EmptyRetrieval.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Abnormal_EmptyRetrieval.Name = "radioButton_Abnormal_EmptyRetrieval";
            radioButton_Abnormal_EmptyRetrieval.Size = new System.Drawing.Size(203, 22);
            radioButton_Abnormal_EmptyRetrieval.TabIndex = 46;
            radioButton_Abnormal_EmptyRetrieval.Tag = "EnumAddress.Server";
            radioButton_Abnormal_EmptyRetrieval.Text = "Abnormal_EmptyRetrieval";
            radioButton_Abnormal_EmptyRetrieval.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_BcrDuplicate
            // 
            radioButton_Abnormal_BcrDuplicate.AutoSize = true;
            radioButton_Abnormal_BcrDuplicate.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Abnormal_BcrDuplicate.Location = new System.Drawing.Point(12, 115);
            radioButton_Abnormal_BcrDuplicate.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Abnormal_BcrDuplicate.Name = "radioButton_Abnormal_BcrDuplicate";
            radioButton_Abnormal_BcrDuplicate.Size = new System.Drawing.Size(187, 22);
            radioButton_Abnormal_BcrDuplicate.TabIndex = 45;
            radioButton_Abnormal_BcrDuplicate.Tag = "EnumAddress.Server";
            radioButton_Abnormal_BcrDuplicate.Text = "Abnormal_BcrDuplicate";
            radioButton_Abnormal_BcrDuplicate.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_BcrMisMatch
            // 
            radioButton_Abnormal_BcrMisMatch.AutoSize = true;
            radioButton_Abnormal_BcrMisMatch.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Abnormal_BcrMisMatch.Location = new System.Drawing.Point(12, 85);
            radioButton_Abnormal_BcrMisMatch.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Abnormal_BcrMisMatch.Name = "radioButton_Abnormal_BcrMisMatch";
            radioButton_Abnormal_BcrMisMatch.Size = new System.Drawing.Size(190, 22);
            radioButton_Abnormal_BcrMisMatch.TabIndex = 44;
            radioButton_Abnormal_BcrMisMatch.Tag = "EnumAddress.Server";
            radioButton_Abnormal_BcrMisMatch.Text = "Abnormal_BcrMisMatch";
            radioButton_Abnormal_BcrMisMatch.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_BcrReadFail
            // 
            radioButton_Abnormal_BcrReadFail.AutoSize = true;
            radioButton_Abnormal_BcrReadFail.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Abnormal_BcrReadFail.Location = new System.Drawing.Point(12, 55);
            radioButton_Abnormal_BcrReadFail.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Abnormal_BcrReadFail.Name = "radioButton_Abnormal_BcrReadFail";
            radioButton_Abnormal_BcrReadFail.Size = new System.Drawing.Size(180, 22);
            radioButton_Abnormal_BcrReadFail.TabIndex = 1;
            radioButton_Abnormal_BcrReadFail.Tag = "EnumAddress.Server";
            radioButton_Abnormal_BcrReadFail.Text = "Abnormal_BcrReadFail";
            radioButton_Abnormal_BcrReadFail.UseVisualStyleBackColor = true;
            // 
            // radioButton_Normal
            // 
            radioButton_Normal.AutoSize = true;
            radioButton_Normal.Checked = true;
            radioButton_Normal.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            radioButton_Normal.Location = new System.Drawing.Point(12, 27);
            radioButton_Normal.Margin = new System.Windows.Forms.Padding(4);
            radioButton_Normal.Name = "radioButton_Normal";
            radioButton_Normal.Size = new System.Drawing.Size(78, 22);
            radioButton_Normal.TabIndex = 0;
            radioButton_Normal.TabStop = true;
            radioButton_Normal.Tag = "EnumAddress.Client";
            radioButton_Normal.Text = "Normal";
            radioButton_Normal.UseVisualStyleBackColor = true;
            // 
            // btnSet
            // 
            btnSet.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnSet.Location = new System.Drawing.Point(95, 411);
            btnSet.Margin = new System.Windows.Forms.Padding(4);
            btnSet.Name = "btnSet";
            btnSet.Size = new System.Drawing.Size(88, 33);
            btnSet.TabIndex = 50;
            btnSet.Text = "Set";
            btnSet.UseVisualStyleBackColor = true;
            btnSet.Click += btnSet_Click;
            // 
            // Mismatch_ID
            // 
            Mismatch_ID.Font = new System.Drawing.Font("微軟正黑體", 10F);
            Mismatch_ID.Location = new System.Drawing.Point(8, 26);
            Mismatch_ID.Margin = new System.Windows.Forms.Padding(4);
            Mismatch_ID.Name = "Mismatch_ID";
            Mismatch_ID.Size = new System.Drawing.Size(190, 25);
            Mismatch_ID.TabIndex = 44;
            // 
            // set_button
            // 
            set_button.Font = new System.Drawing.Font("微軟正黑體", 10F);
            set_button.Location = new System.Drawing.Point(206, 20);
            set_button.Margin = new System.Windows.Forms.Padding(4);
            set_button.Name = "set_button";
            set_button.Size = new System.Drawing.Size(53, 33);
            set_button.TabIndex = 46;
            set_button.Text = "Set";
            set_button.UseVisualStyleBackColor = true;
            set_button.Click += set_button_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label8.Location = new System.Drawing.Point(9, 18);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(79, 18);
            label8.TabIndex = 48;
            label8.Text = "Init_AddID";
            // 
            // tbx_init_add_id
            // 
            tbx_init_add_id.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbx_init_add_id.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            tbx_init_add_id.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            tbx_init_add_id.Font = new System.Drawing.Font("微軟正黑體", 10F);
            tbx_init_add_id.FormattingEnabled = true;
            tbx_init_add_id.Location = new System.Drawing.Point(117, 14);
            tbx_init_add_id.Margin = new System.Windows.Forms.Padding(4);
            tbx_init_add_id.Name = "tbx_init_add_id";
            tbx_init_add_id.Size = new System.Drawing.Size(93, 25);
            tbx_init_add_id.TabIndex = 47;
            tbx_init_add_id.TextChanged += tbx_init_add_id_TextChanged;
            tbx_init_add_id.SelectedIndexChanged += tbx_init_add_id_SelectedIndexChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(-117, -133);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(0, 16);
            label9.TabIndex = 48;
            label9.Visible = false;
            // 
            // tbx_init_sec_id
            // 
            tbx_init_sec_id.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbx_init_sec_id.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            tbx_init_sec_id.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            tbx_init_sec_id.Font = new System.Drawing.Font("微軟正黑體", 10F);
            tbx_init_sec_id.FormattingEnabled = true;
            tbx_init_sec_id.Location = new System.Drawing.Point(117, 51);
            tbx_init_sec_id.Margin = new System.Windows.Forms.Padding(4);
            tbx_init_sec_id.Name = "tbx_init_sec_id";
            tbx_init_sec_id.Size = new System.Drawing.Size(93, 25);
            tbx_init_sec_id.TabIndex = 47;
            tbx_init_sec_id.TextChanged += tbx_init_sec_id_TextChanged;
            // 
            // textBox3
            // 
            textBox3.Location = new System.Drawing.Point(-117, -133);
            textBox3.Margin = new System.Windows.Forms.Padding(4);
            textBox3.Name = "textBox3";
            textBox3.Size = new System.Drawing.Size(11, 23);
            textBox3.TabIndex = 47;
            textBox3.Visible = false;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(-117, -133);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(0, 16);
            label10.TabIndex = 48;
            label10.Visible = false;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label11.Location = new System.Drawing.Point(9, 54);
            label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(72, 18);
            label11.TabIndex = 48;
            label11.Text = "Init_SecId";
            // 
            // textBox4
            // 
            textBox4.Location = new System.Drawing.Point(-117, -133);
            textBox4.Margin = new System.Windows.Forms.Padding(4);
            textBox4.Name = "textBox4";
            textBox4.Size = new System.Drawing.Size(11, 23);
            textBox4.TabIndex = 47;
            textBox4.Visible = false;
            // 
            // textBox5
            // 
            textBox5.Location = new System.Drawing.Point(-117, -133);
            textBox5.Margin = new System.Windows.Forms.Padding(4);
            textBox5.Name = "textBox5";
            textBox5.Size = new System.Drawing.Size(11, 23);
            textBox5.TabIndex = 47;
            textBox5.Visible = false;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(-117, -133);
            label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(0, 16);
            label12.TabIndex = 48;
            label12.Visible = false;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(-117, -133);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(0, 16);
            label7.TabIndex = 49;
            label7.Visible = false;
            // 
            // stopSet_button
            // 
            stopSet_button.Font = new System.Drawing.Font("微軟正黑體", 10F);
            stopSet_button.Location = new System.Drawing.Point(13, 49);
            stopSet_button.Margin = new System.Windows.Forms.Padding(4);
            stopSet_button.Name = "stopSet_button";
            stopSet_button.Size = new System.Drawing.Size(82, 33);
            stopSet_button.TabIndex = 51;
            stopSet_button.Text = "Set";
            stopSet_button.UseVisualStyleBackColor = true;
            stopSet_button.Click += stopSet_button_Click;
            // 
            // stopClear_button
            // 
            stopClear_button.Font = new System.Drawing.Font("微軟正黑體", 10F);
            stopClear_button.Location = new System.Drawing.Point(104, 49);
            stopClear_button.Margin = new System.Windows.Forms.Padding(4);
            stopClear_button.Name = "stopClear_button";
            stopClear_button.Size = new System.Drawing.Size(82, 33);
            stopClear_button.TabIndex = 52;
            stopClear_button.Text = "Reset";
            stopClear_button.UseVisualStyleBackColor = true;
            stopClear_button.Click += stopClear_button_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label1.Location = new System.Drawing.Point(9, 27);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(87, 18);
            label1.TabIndex = 53;
            label1.Text = "Alarm Code";
            // 
            // txtAlarmCode
            // 
            txtAlarmCode.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txtAlarmCode.Location = new System.Drawing.Point(9, 51);
            txtAlarmCode.Margin = new System.Windows.Forms.Padding(4);
            txtAlarmCode.Name = "txtAlarmCode";
            txtAlarmCode.Size = new System.Drawing.Size(180, 25);
            txtAlarmCode.TabIndex = 54;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label26.Location = new System.Drawing.Point(9, 80);
            label26.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label26.Name = "label26";
            label26.Size = new System.Drawing.Size(69, 18);
            label26.TabIndex = 56;
            label26.Text = "Alarm Lv.";
            // 
            // cmbAlarmLevel
            // 
            cmbAlarmLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbAlarmLevel.Font = new System.Drawing.Font("微軟正黑體", 10F);
            cmbAlarmLevel.FormattingEnabled = true;
            cmbAlarmLevel.Items.AddRange(new object[] { "Error", "Warn" });
            cmbAlarmLevel.Location = new System.Drawing.Point(9, 102);
            cmbAlarmLevel.Margin = new System.Windows.Forms.Padding(4);
            cmbAlarmLevel.Name = "cmbAlarmLevel";
            cmbAlarmLevel.Size = new System.Drawing.Size(180, 25);
            cmbAlarmLevel.TabIndex = 57;
            // 
            // btnAlarmClear
            // 
            btnAlarmClear.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnAlarmClear.Location = new System.Drawing.Point(103, 129);
            btnAlarmClear.Margin = new System.Windows.Forms.Padding(4);
            btnAlarmClear.Name = "btnAlarmClear";
            btnAlarmClear.Size = new System.Drawing.Size(86, 33);
            btnAlarmClear.TabIndex = 58;
            btnAlarmClear.Text = "Reset";
            btnAlarmClear.UseVisualStyleBackColor = true;
            btnAlarmClear.Click += btnAlarmClear_Click;
            // 
            // txtCstID
            // 
            txtCstID.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txtCstID.Location = new System.Drawing.Point(9, 51);
            txtCstID.Margin = new System.Windows.Forms.Padding(4);
            txtCstID.Name = "txtCstID";
            txtCstID.Size = new System.Drawing.Size(180, 25);
            txtCstID.TabIndex = 58;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label13.Location = new System.Drawing.Point(9, 27);
            label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(52, 18);
            label13.TabIndex = 57;
            label13.Text = "CST ID";
            // 
            // btnCstSet
            // 
            btnCstSet.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnCstSet.Location = new System.Drawing.Point(9, 91);
            btnCstSet.Margin = new System.Windows.Forms.Padding(4);
            btnCstSet.Name = "btnCstSet";
            btnCstSet.Size = new System.Drawing.Size(82, 33);
            btnCstSet.TabIndex = 56;
            btnCstSet.Text = "Set";
            btnCstSet.UseVisualStyleBackColor = true;
            btnCstSet.Click += btnCstSet_Click;
            // 
            // btnCstClear
            // 
            btnCstClear.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnCstClear.Location = new System.Drawing.Point(103, 91);
            btnCstClear.Margin = new System.Windows.Forms.Padding(4);
            btnCstClear.Name = "btnCstClear";
            btnCstClear.Size = new System.Drawing.Size(88, 33);
            btnCstClear.TabIndex = 59;
            btnCstClear.Text = "Reset";
            btnCstClear.UseVisualStyleBackColor = true;
            btnCstClear.Click += btnCstClear_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label6.Location = new System.Drawing.Point(239, 27);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(91, 18);
            label6.TabIndex = 62;
            label6.Text = "Remote Port";
            // 
            // txtPort
            // 
            txtPort.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txtPort.Location = new System.Drawing.Point(338, 22);
            txtPort.Margin = new System.Windows.Forms.Padding(4);
            txtPort.Name = "txtPort";
            txtPort.Size = new System.Drawing.Size(81, 25);
            txtPort.TabIndex = 61;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label4.Location = new System.Drawing.Point(239, 66);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(74, 18);
            label4.TabIndex = 63;
            label4.Text = "Local Port";
            // 
            // txtLocalPort
            // 
            txtLocalPort.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txtLocalPort.Location = new System.Drawing.Point(338, 63);
            txtLocalPort.Margin = new System.Windows.Forms.Padding(4);
            txtLocalPort.Name = "txtLocalPort";
            txtLocalPort.Size = new System.Drawing.Size(81, 25);
            txtLocalPort.TabIndex = 64;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label14.Location = new System.Drawing.Point(9, 66);
            label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(59, 18);
            label14.TabIndex = 65;
            label14.Text = "Local IP";
            // 
            // txtLocalIP
            // 
            txtLocalIP.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txtLocalIP.Location = new System.Drawing.Point(93, 63);
            txtLocalIP.Margin = new System.Windows.Forms.Padding(4);
            txtLocalIP.Name = "txtLocalIP";
            txtLocalIP.Size = new System.Drawing.Size(139, 25);
            txtLocalIP.TabIndex = 66;
            // 
            // button1
            // 
            button1.Font = new System.Drawing.Font("微軟正黑體", 9F);
            button1.Location = new System.Drawing.Point(776, 20);
            button1.Margin = new System.Windows.Forms.Padding(4);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(122, 74);
            button1.TabIndex = 67;
            button1.Text = "Auto Connect\r\nAll OHT";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label15.Location = new System.Drawing.Point(9, 30);
            label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(85, 18);
            label15.TabIndex = 70;
            label15.Text = "Interval(ms)";
            // 
            // tbxMs
            // 
            tbxMs.Font = new System.Drawing.Font("微軟正黑體", 10F);
            tbxMs.Location = new System.Drawing.Point(128, 26);
            tbxMs.Margin = new System.Windows.Forms.Padding(4);
            tbxMs.Name = "tbxMs";
            tbxMs.Size = new System.Drawing.Size(83, 25);
            tbxMs.TabIndex = 69;
            // 
            // btnOn
            // 
            btnOn.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnOn.Location = new System.Drawing.Point(23, 64);
            btnOn.Margin = new System.Windows.Forms.Padding(4);
            btnOn.Name = "btnOn";
            btnOn.Size = new System.Drawing.Size(82, 33);
            btnOn.TabIndex = 68;
            btnOn.Text = "On";
            btnOn.UseVisualStyleBackColor = true;
            btnOn.Click += btnOn_Click;
            // 
            // btnOff
            // 
            btnOff.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnOff.Location = new System.Drawing.Point(128, 64);
            btnOff.Margin = new System.Windows.Forms.Padding(4);
            btnOff.Name = "btnOff";
            btnOff.Size = new System.Drawing.Size(82, 33);
            btnOff.TabIndex = 71;
            btnOff.Text = "Off";
            btnOff.UseVisualStyleBackColor = true;
            btnOff.Click += btnOff_Click;
            // 
            // chbSetFlyToTarget
            // 
            chbSetFlyToTarget.AutoSize = true;
            chbSetFlyToTarget.Font = new System.Drawing.Font("微軟正黑體", 10F);
            chbSetFlyToTarget.Location = new System.Drawing.Point(12, 27);
            chbSetFlyToTarget.Margin = new System.Windows.Forms.Padding(4);
            chbSetFlyToTarget.Name = "chbSetFlyToTarget";
            chbSetFlyToTarget.Size = new System.Drawing.Size(156, 22);
            chbSetFlyToTarget.TabIndex = 72;
            chbSetFlyToTarget.Text = "Set Jump To Target";
            chbSetFlyToTarget.UseVisualStyleBackColor = true;
            // 
            // btnSetJump
            // 
            btnSetJump.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnSetJump.Location = new System.Drawing.Point(191, 22);
            btnSetJump.Margin = new System.Windows.Forms.Padding(4);
            btnSetJump.Name = "btnSetJump";
            btnSetJump.Size = new System.Drawing.Size(64, 31);
            btnSetJump.TabIndex = 73;
            btnSetJump.Text = "Set";
            btnSetJump.UseVisualStyleBackColor = true;
            btnSetJump.Click += btnSetJump_Click;
            // 
            // chbDisableLocChk
            // 
            chbDisableLocChk.AutoSize = true;
            chbDisableLocChk.Font = new System.Drawing.Font("微軟正黑體", 10F);
            chbDisableLocChk.Location = new System.Drawing.Point(12, 60);
            chbDisableLocChk.Margin = new System.Windows.Forms.Padding(4);
            chbDisableLocChk.Name = "chbDisableLocChk";
            chbDisableLocChk.Size = new System.Drawing.Size(170, 22);
            chbDisableLocChk.TabIndex = 74;
            chbDisableLocChk.Text = "Disable Vh Loc Check";
            chbDisableLocChk.UseVisualStyleBackColor = true;
            // 
            // btnDisableLocCheck
            // 
            btnDisableLocCheck.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnDisableLocCheck.Location = new System.Drawing.Point(191, 55);
            btnDisableLocCheck.Margin = new System.Windows.Forms.Padding(4);
            btnDisableLocCheck.Name = "btnDisableLocCheck";
            btnDisableLocCheck.Size = new System.Drawing.Size(64, 31);
            btnDisableLocCheck.TabIndex = 75;
            btnDisableLocCheck.Text = "Set";
            btnDisableLocCheck.UseVisualStyleBackColor = true;
            btnDisableLocCheck.Click += btnDisableLocCheck_Click;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label16.Location = new System.Drawing.Point(14, 102);
            label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(112, 18);
            label16.TabIndex = 76;
            label16.Text = "Breakdown Car:";
            // 
            // Button_BD_Car_Set
            // 
            Button_BD_Car_Set.Font = new System.Drawing.Font("微軟正黑體", 10F);
            Button_BD_Car_Set.Location = new System.Drawing.Point(14, 128);
            Button_BD_Car_Set.Margin = new System.Windows.Forms.Padding(4);
            Button_BD_Car_Set.Name = "Button_BD_Car_Set";
            Button_BD_Car_Set.Size = new System.Drawing.Size(82, 33);
            Button_BD_Car_Set.TabIndex = 77;
            Button_BD_Car_Set.Text = "Set";
            Button_BD_Car_Set.UseVisualStyleBackColor = true;
            Button_BD_Car_Set.Click += Button_BD_Car_Set_Click;
            // 
            // button3Button_BD_Car_ReSet
            // 
            button3Button_BD_Car_ReSet.Font = new System.Drawing.Font("微軟正黑體", 10F);
            button3Button_BD_Car_ReSet.Location = new System.Drawing.Point(104, 128);
            button3Button_BD_Car_ReSet.Margin = new System.Windows.Forms.Padding(4);
            button3Button_BD_Car_ReSet.Name = "button3Button_BD_Car_ReSet";
            button3Button_BD_Car_ReSet.Size = new System.Drawing.Size(82, 33);
            button3Button_BD_Car_ReSet.TabIndex = 78;
            button3Button_BD_Car_ReSet.Text = "Reset";
            button3Button_BD_Car_ReSet.UseVisualStyleBackColor = true;
            button3Button_BD_Car_ReSet.Click += button3Button_BD_Car_ReSet_Click;
            // 
            // btnSecsCmdStart
            // 
            btnSecsCmdStart.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnSecsCmdStart.Location = new System.Drawing.Point(9, 27);
            btnSecsCmdStart.Margin = new System.Windows.Forms.Padding(4);
            btnSecsCmdStart.Name = "btnSecsCmdStart";
            btnSecsCmdStart.Size = new System.Drawing.Size(82, 33);
            btnSecsCmdStart.TabIndex = 80;
            btnSecsCmdStart.Text = "Start";
            btnSecsCmdStart.UseVisualStyleBackColor = true;
            btnSecsCmdStart.Click += btnSecsCmdStart_Click;
            // 
            // btnSecsCmdStop
            // 
            btnSecsCmdStop.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btnSecsCmdStop.Location = new System.Drawing.Point(103, 27);
            btnSecsCmdStop.Margin = new System.Windows.Forms.Padding(4);
            btnSecsCmdStop.Name = "btnSecsCmdStop";
            btnSecsCmdStop.Size = new System.Drawing.Size(82, 33);
            btnSecsCmdStop.TabIndex = 81;
            btnSecsCmdStop.Text = "Stop";
            btnSecsCmdStop.UseVisualStyleBackColor = true;
            btnSecsCmdStop.Click += btnSecsCmdStop_Click;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new System.Drawing.Point(-117, -133);
            label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(0, 16);
            label17.TabIndex = 79;
            label17.Visible = false;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new System.Drawing.Point(-117, -133);
            label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(0, 16);
            label18.TabIndex = 82;
            label18.Visible = false;
            // 
            // btn_DisCon
            // 
            btn_DisCon.Font = new System.Drawing.Font("微軟正黑體", 9F);
            btn_DisCon.Location = new System.Drawing.Point(9, 27);
            btn_DisCon.Margin = new System.Windows.Forms.Padding(4);
            btn_DisCon.Name = "btn_DisCon";
            btn_DisCon.Size = new System.Drawing.Size(104, 33);
            btn_DisCon.TabIndex = 83;
            btn_DisCon.Text = "Disconect Test";
            btn_DisCon.UseVisualStyleBackColor = true;
            btn_DisCon.Click += btn_DisCon_Click;
            // 
            // btn_disConStop
            // 
            btn_disConStop.Font = new System.Drawing.Font("微軟正黑體", 9F);
            btn_disConStop.Location = new System.Drawing.Point(120, 27);
            btn_disConStop.Margin = new System.Windows.Forms.Padding(4);
            btn_disConStop.Name = "btn_disConStop";
            btn_disConStop.Size = new System.Drawing.Size(106, 33);
            btn_disConStop.TabIndex = 84;
            btn_disConStop.Text = "Disconect Stop";
            btn_disConStop.UseVisualStyleBackColor = true;
            btn_disConStop.Click += btn_disConStop_Click;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new System.Drawing.Font("微軟正黑體", 9F);
            label19.Location = new System.Drawing.Point(9, 74);
            label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(101, 16);
            label19.TabIndex = 85;
            label19.Text = "Connect Time(s):";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Font = new System.Drawing.Font("微軟正黑體", 9F);
            label20.Location = new System.Drawing.Point(9, 105);
            label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label20.Name = "label20";
            label20.Size = new System.Drawing.Size(116, 16);
            label20.TabIndex = 86;
            label20.Text = "Disconnect Time(s):";
            // 
            // txBox_ConTime
            // 
            txBox_ConTime.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txBox_ConTime.Location = new System.Drawing.Point(134, 68);
            txBox_ConTime.Margin = new System.Windows.Forms.Padding(4);
            txBox_ConTime.Name = "txBox_ConTime";
            txBox_ConTime.Size = new System.Drawing.Size(52, 25);
            txBox_ConTime.TabIndex = 87;
            txBox_ConTime.Text = "3";
            // 
            // txBox_DisConTime
            // 
            txBox_DisConTime.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txBox_DisConTime.Location = new System.Drawing.Point(134, 98);
            txBox_DisConTime.Margin = new System.Windows.Forms.Padding(4);
            txBox_DisConTime.Name = "txBox_DisConTime";
            txBox_DisConTime.Size = new System.Drawing.Size(52, 25);
            txBox_DisConTime.TabIndex = 88;
            txBox_DisConTime.Text = "15";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Font = new System.Drawing.Font("微軟正黑體", 9F);
            label21.Location = new System.Drawing.Point(9, 33);
            label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label21.Name = "label21";
            label21.Size = new System.Drawing.Size(66, 16);
            label21.TabIndex = 91;
            label21.Text = "Delay(ms):";
            // 
            // tbxDelayTime
            // 
            tbxDelayTime.Font = new System.Drawing.Font("微軟正黑體", 10F);
            tbxDelayTime.Location = new System.Drawing.Point(79, 28);
            tbxDelayTime.Margin = new System.Windows.Forms.Padding(4);
            tbxDelayTime.Name = "tbxDelayTime";
            tbxDelayTime.Size = new System.Drawing.Size(69, 25);
            tbxDelayTime.TabIndex = 90;
            tbxDelayTime.Text = "0";
            // 
            // btSetDelayTime
            // 
            btSetDelayTime.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btSetDelayTime.Location = new System.Drawing.Point(156, 23);
            btSetDelayTime.Margin = new System.Windows.Forms.Padding(4);
            btSetDelayTime.Name = "btSetDelayTime";
            btSetDelayTime.Size = new System.Drawing.Size(70, 33);
            btSetDelayTime.TabIndex = 89;
            btSetDelayTime.Text = "Set";
            btSetDelayTime.UseVisualStyleBackColor = true;
            btSetDelayTime.Click += btSetDelayTime_Click;
            // 
            // txtIOThread
            // 
            txtIOThread.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txtIOThread.Location = new System.Drawing.Point(89, 73);
            txtIOThread.Margin = new System.Windows.Forms.Padding(4);
            txtIOThread.Name = "txtIOThread";
            txtIOThread.Size = new System.Drawing.Size(169, 25);
            txtIOThread.TabIndex = 92;
            // 
            // txtWorkThread
            // 
            txtWorkThread.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txtWorkThread.Location = new System.Drawing.Point(89, 23);
            txtWorkThread.Margin = new System.Windows.Forms.Padding(4);
            txtWorkThread.Name = "txtWorkThread";
            txtWorkThread.Size = new System.Drawing.Size(169, 25);
            txtWorkThread.TabIndex = 93;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new System.Drawing.Point(-117, -133);
            label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label22.Name = "label22";
            label22.Size = new System.Drawing.Size(0, 16);
            label22.TabIndex = 94;
            label22.Visible = false;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Font = new System.Drawing.Font("微軟正黑體", 9F);
            label23.Location = new System.Drawing.Point(9, 27);
            label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label23.Name = "label23";
            label23.Size = new System.Drawing.Size(80, 16);
            label23.TabIndex = 95;
            label23.Text = "Work Thread";
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Font = new System.Drawing.Font("微軟正黑體", 9F);
            label24.Location = new System.Drawing.Point(12, 79);
            label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label24.Name = "label24";
            label24.Size = new System.Drawing.Size(63, 16);
            label24.TabIndex = 96;
            label24.Text = "IO Thread";
            // 
            // btThreadStart
            // 
            btThreadStart.Font = new System.Drawing.Font("微軟正黑體", 10F);
            btThreadStart.Location = new System.Drawing.Point(90, 114);
            btThreadStart.Margin = new System.Windows.Forms.Padding(4);
            btThreadStart.Name = "btThreadStart";
            btThreadStart.Size = new System.Drawing.Size(82, 30);
            btThreadStart.TabIndex = 97;
            btThreadStart.Text = "Start";
            btThreadStart.UseVisualStyleBackColor = true;
            btThreadStart.Click += btThreadStart_Click;
            // 
            // txtVhID
            // 
            txtVhID.Font = new System.Drawing.Font("微軟正黑體", 10F);
            txtVhID.Location = new System.Drawing.Point(525, 22);
            txtVhID.Margin = new System.Windows.Forms.Padding(4);
            txtVhID.Name = "txtVhID";
            txtVhID.Size = new System.Drawing.Size(116, 25);
            txtVhID.TabIndex = 98;
            txtVhID.SelectedIndexChanged += txtVhID_SelectedIndexChanged;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new System.Drawing.Point(-117, -133);
            label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label25.Name = "label25";
            label25.Size = new System.Drawing.Size(0, 16);
            label25.TabIndex = 100;
            label25.Visible = false;
            // 
            // bt_sendTeachData
            // 
            bt_sendTeachData.Font = new System.Drawing.Font("微軟正黑體", 10F);
            bt_sendTeachData.Location = new System.Drawing.Point(19, 39);
            bt_sendTeachData.Margin = new System.Windows.Forms.Padding(4);
            bt_sendTeachData.Name = "bt_sendTeachData";
            bt_sendTeachData.Size = new System.Drawing.Size(91, 33);
            bt_sendTeachData.TabIndex = 99;
            bt_sendTeachData.Text = "Send Data";
            bt_sendTeachData.UseVisualStyleBackColor = true;
            bt_sendTeachData.Click += bt_sendTeachData_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(bt_setTeachReply);
            groupBox1.Controls.Add(rb_teachNG);
            groupBox1.Controls.Add(rb_teachOk);
            groupBox1.Location = new System.Drawing.Point(126, 17);
            groupBox1.Margin = new System.Windows.Forms.Padding(4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4);
            groupBox1.Size = new System.Drawing.Size(126, 77);
            groupBox1.TabIndex = 102;
            groupBox1.TabStop = false;
            groupBox1.Text = "Teaching Reply";
            // 
            // bt_setTeachReply
            // 
            bt_setTeachReply.Font = new System.Drawing.Font("微軟正黑體", 10F);
            bt_setTeachReply.Location = new System.Drawing.Point(61, 25);
            bt_setTeachReply.Margin = new System.Windows.Forms.Padding(4);
            bt_setTeachReply.Name = "bt_setTeachReply";
            bt_setTeachReply.Size = new System.Drawing.Size(51, 33);
            bt_setTeachReply.TabIndex = 103;
            bt_setTeachReply.Text = "Set";
            bt_setTeachReply.UseVisualStyleBackColor = true;
            bt_setTeachReply.Click += bt_setTeachReply_Click;
            // 
            // rb_teachNG
            // 
            rb_teachNG.AutoSize = true;
            rb_teachNG.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            rb_teachNG.Location = new System.Drawing.Point(8, 48);
            rb_teachNG.Margin = new System.Windows.Forms.Padding(4);
            rb_teachNG.Name = "rb_teachNG";
            rb_teachNG.Size = new System.Drawing.Size(49, 22);
            rb_teachNG.TabIndex = 54;
            rb_teachNG.Tag = "EnumAddress.Client";
            rb_teachNG.Text = "NG";
            rb_teachNG.UseVisualStyleBackColor = true;
            // 
            // rb_teachOk
            // 
            rb_teachOk.AutoSize = true;
            rb_teachOk.Checked = true;
            rb_teachOk.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            rb_teachOk.Location = new System.Drawing.Point(8, 20);
            rb_teachOk.Margin = new System.Windows.Forms.Padding(4);
            rb_teachOk.Name = "rb_teachOk";
            rb_teachOk.Size = new System.Drawing.Size(46, 22);
            rb_teachOk.TabIndex = 54;
            rb_teachOk.TabStop = true;
            rb_teachOk.Tag = "EnumAddress.Client";
            rb_teachOk.Text = "OK";
            rb_teachOk.UseVisualStyleBackColor = true;
            // 
            // chbInSectionLoad
            // 
            chbInSectionLoad.AutoSize = true;
            chbInSectionLoad.Font = new System.Drawing.Font("微軟正黑體", 10F);
            chbInSectionLoad.Location = new System.Drawing.Point(12, 94);
            chbInSectionLoad.Margin = new System.Windows.Forms.Padding(4);
            chbInSectionLoad.Name = "chbInSectionLoad";
            chbInSectionLoad.Size = new System.Drawing.Size(210, 22);
            chbInSectionLoad.TabIndex = 103;
            chbInSectionLoad.Text = "InSection Load Assignment";
            chbInSectionLoad.UseVisualStyleBackColor = true;
            chbInSectionLoad.CheckedChanged += chbInSectionLoad_CheckedChanged;
            // 
            // grpConnection
            // 
            grpConnection.Controls.Add(Send_136_Initial);
            grpConnection.Controls.Add(label2);
            grpConnection.Controls.Add(txtIp);
            grpConnection.Controls.Add(label6);
            grpConnection.Controls.Add(txtPort);
            grpConnection.Controls.Add(label14);
            grpConnection.Controls.Add(grpInitPos);
            grpConnection.Controls.Add(txtLocalIP);
            grpConnection.Controls.Add(label4);
            grpConnection.Controls.Add(txtLocalPort);
            grpConnection.Controls.Add(label3);
            grpConnection.Controls.Add(txtVhID);
            grpConnection.Controls.Add(btnConnect);
            grpConnection.Controls.Add(btnDisConnect);
            grpConnection.Controls.Add(button1);
            grpConnection.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 136);
            grpConnection.Location = new System.Drawing.Point(14, 6);
            grpConnection.Margin = new System.Windows.Forms.Padding(4);
            grpConnection.Name = "grpConnection";
            grpConnection.Padding = new System.Windows.Forms.Padding(4);
            grpConnection.Size = new System.Drawing.Size(1132, 107);
            grpConnection.TabIndex = 200;
            grpConnection.TabStop = false;
            grpConnection.Text = "Connection Settings";
            // 
            // grpInitPos
            // 
            grpInitPos.Controls.Add(label8);
            grpInitPos.Controls.Add(tbx_init_add_id);
            grpInitPos.Controls.Add(label11);
            grpInitPos.Controls.Add(tbx_init_sec_id);
            grpInitPos.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpInitPos.Location = new System.Drawing.Point(910, 14);
            grpInitPos.Margin = new System.Windows.Forms.Padding(4);
            grpInitPos.Name = "grpInitPos";
            grpInitPos.Padding = new System.Windows.Forms.Padding(4);
            grpInitPos.Size = new System.Drawing.Size(222, 84);
            grpInitPos.TabIndex = 203;
            grpInitPos.TabStop = false;
            grpInitPos.Text = "Init Position";
            // 
            // grpContinuousSend
            // 
            grpContinuousSend.Controls.Add(label15);
            grpContinuousSend.Controls.Add(tbxMs);
            grpContinuousSend.Controls.Add(btnOn);
            grpContinuousSend.Controls.Add(btnOff);
            grpContinuousSend.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpContinuousSend.Location = new System.Drawing.Point(642, 119);
            grpContinuousSend.Margin = new System.Windows.Forms.Padding(4);
            grpContinuousSend.Name = "grpContinuousSend";
            grpContinuousSend.Padding = new System.Windows.Forms.Padding(4);
            grpContinuousSend.Size = new System.Drawing.Size(233, 107);
            grpContinuousSend.TabIndex = 201;
            grpContinuousSend.TabStop = false;
            grpContinuousSend.Text = "Continuous Send 134";
            // 
            // grpSimOptions
            // 
            grpSimOptions.Controls.Add(chbSetFlyToTarget);
            grpSimOptions.Controls.Add(btnSetJump);
            grpSimOptions.Controls.Add(chbDisableLocChk);
            grpSimOptions.Controls.Add(btnDisableLocCheck);
            grpSimOptions.Controls.Add(chbInSectionLoad);
            grpSimOptions.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpSimOptions.Location = new System.Drawing.Point(883, 119);
            grpSimOptions.Margin = new System.Windows.Forms.Padding(4);
            grpSimOptions.Name = "grpSimOptions";
            grpSimOptions.Padding = new System.Windows.Forms.Padding(4);
            grpSimOptions.Size = new System.Drawing.Size(263, 137);
            grpSimOptions.TabIndex = 202;
            grpSimOptions.TabStop = false;
            grpSimOptions.Text = "Simulation Options";
            // 
            // grpCST
            // 
            grpCST.Controls.Add(label13);
            grpCST.Controls.Add(txtCstID);
            grpCST.Controls.Add(btnCstSet);
            grpCST.Controls.Add(btnCstClear);
            grpCST.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpCST.Location = new System.Drawing.Point(218, 581);
            grpCST.Margin = new System.Windows.Forms.Padding(4);
            grpCST.Name = "grpCST";
            grpCST.Padding = new System.Windows.Forms.Padding(4);
            grpCST.Size = new System.Drawing.Size(204, 154);
            grpCST.TabIndex = 204;
            grpCST.TabStop = false;
            grpCST.Text = "CST ID";
            // 
            // grpAlarm
            // 
            grpAlarm.Controls.Add(label26);
            grpAlarm.Controls.Add(cmbAlarmLevel);
            grpAlarm.Controls.Add(label1);
            grpAlarm.Controls.Add(txtAlarmCode);
            grpAlarm.Controls.Add(btnAlarmSet);
            grpAlarm.Controls.Add(btnAlarmClear);
            grpAlarm.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpAlarm.Location = new System.Drawing.Point(430, 581);
            grpAlarm.Margin = new System.Windows.Forms.Padding(4);
            grpAlarm.Name = "grpAlarm";
            grpAlarm.Padding = new System.Windows.Forms.Padding(4);
            grpAlarm.Size = new System.Drawing.Size(204, 171);
            grpAlarm.TabIndex = 205;
            grpAlarm.TabStop = false;
            grpAlarm.Text = "Alarm";
            // 
            // grpStopBreakdown
            // 
            grpStopBreakdown.Controls.Add(label5);
            grpStopBreakdown.Controls.Add(stopSet_button);
            grpStopBreakdown.Controls.Add(stopClear_button);
            grpStopBreakdown.Controls.Add(label16);
            grpStopBreakdown.Controls.Add(Button_BD_Car_Set);
            grpStopBreakdown.Controls.Add(button3Button_BD_Car_ReSet);
            grpStopBreakdown.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpStopBreakdown.Location = new System.Drawing.Point(642, 530);
            grpStopBreakdown.Margin = new System.Windows.Forms.Padding(4);
            grpStopBreakdown.Name = "grpStopBreakdown";
            grpStopBreakdown.Padding = new System.Windows.Forms.Padding(4);
            grpStopBreakdown.Size = new System.Drawing.Size(233, 175);
            grpStopBreakdown.TabIndex = 206;
            grpStopBreakdown.TabStop = false;
            grpStopBreakdown.Text = "Stop / Breakdown";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("微軟正黑體", 10F);
            label5.Location = new System.Drawing.Point(14, 24);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(68, 18);
            label5.TabIndex = 79;
            label5.Text = "Stop Car:";
            // 
            // grpSECS
            // 
            grpSECS.Controls.Add(btnSecsCmdStart);
            grpSECS.Controls.Add(btnSecsCmdStop);
            grpSECS.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpSECS.Location = new System.Drawing.Point(14, 581);
            grpSECS.Margin = new System.Windows.Forms.Padding(4);
            grpSECS.Name = "grpSECS";
            grpSECS.Padding = new System.Windows.Forms.Padding(4);
            grpSECS.Size = new System.Drawing.Size(196, 74);
            grpSECS.TabIndex = 207;
            grpSECS.TabStop = false;
            grpSECS.Text = "SECS Command Test";
            // 
            // grpDisconnect
            // 
            grpDisconnect.Controls.Add(btn_DisCon);
            grpDisconnect.Controls.Add(btn_disConStop);
            grpDisconnect.Controls.Add(label19);
            grpDisconnect.Controls.Add(txBox_ConTime);
            grpDisconnect.Controls.Add(label20);
            grpDisconnect.Controls.Add(txBox_DisConTime);
            grpDisconnect.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpDisconnect.Location = new System.Drawing.Point(642, 388);
            grpDisconnect.Margin = new System.Windows.Forms.Padding(4);
            grpDisconnect.Name = "grpDisconnect";
            grpDisconnect.Padding = new System.Windows.Forms.Padding(4);
            grpDisconnect.Size = new System.Drawing.Size(233, 133);
            grpDisconnect.TabIndex = 208;
            grpDisconnect.TabStop = false;
            grpDisconnect.Text = "Disconnect Test";
            // 
            // grpThread
            // 
            grpThread.Controls.Add(label23);
            grpThread.Controls.Add(txtWorkThread);
            grpThread.Controls.Add(label24);
            grpThread.Controls.Add(txtIOThread);
            grpThread.Controls.Add(btThreadStart);
            grpThread.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpThread.Location = new System.Drawing.Point(883, 467);
            grpThread.Margin = new System.Windows.Forms.Padding(4);
            grpThread.Name = "grpThread";
            grpThread.Padding = new System.Windows.Forms.Padding(4);
            grpThread.Size = new System.Drawing.Size(263, 156);
            grpThread.TabIndex = 209;
            grpThread.TabStop = false;
            grpThread.Text = "Thread Watch";
            // 
            // grpTeaching
            // 
            grpTeaching.Controls.Add(bt_sendTeachData);
            grpTeaching.Controls.Add(groupBox1);
            grpTeaching.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpTeaching.Location = new System.Drawing.Point(883, 275);
            grpTeaching.Margin = new System.Windows.Forms.Padding(4);
            grpTeaching.Name = "grpTeaching";
            grpTeaching.Padding = new System.Windows.Forms.Padding(4);
            grpTeaching.Size = new System.Drawing.Size(263, 105);
            grpTeaching.TabIndex = 211;
            grpTeaching.TabStop = false;
            grpTeaching.Text = "Teaching Data";
            // 
            // grpMismatchID
            // 
            grpMismatchID.Controls.Add(Mismatch_ID);
            grpMismatchID.Controls.Add(set_button);
            grpMismatchID.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpMismatchID.Location = new System.Drawing.Point(883, 389);
            grpMismatchID.Margin = new System.Windows.Forms.Padding(4);
            grpMismatchID.Name = "grpMismatchID";
            grpMismatchID.Padding = new System.Windows.Forms.Padding(4);
            grpMismatchID.Size = new System.Drawing.Size(263, 69);
            grpMismatchID.TabIndex = 210;
            grpMismatchID.TabStop = false;
            grpMismatchID.Text = "Mismatch ID";
            // 
            // grpDelay
            // 
            grpDelay.Controls.Add(label21);
            grpDelay.Controls.Add(tbxDelayTime);
            grpDelay.Controls.Add(btSetDelayTime);
            grpDelay.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            grpDelay.Location = new System.Drawing.Point(642, 299);
            grpDelay.Margin = new System.Windows.Forms.Padding(4);
            grpDelay.Name = "grpDelay";
            grpDelay.Padding = new System.Windows.Forms.Padding(4);
            grpDelay.Size = new System.Drawing.Size(233, 74);
            grpDelay.TabIndex = 212;
            grpDelay.TabStop = false;
            grpDelay.Text = "Reply OHTC Delay";
            // 
            // Send_136_Initial
            // 
            Send_136_Initial.Font = new System.Drawing.Font("微軟正黑體", 10F);
            Send_136_Initial.Location = new System.Drawing.Point(525, 63);
            Send_136_Initial.Margin = new System.Windows.Forms.Padding(4);
            Send_136_Initial.Name = "Send_136_Initial";
            Send_136_Initial.Size = new System.Drawing.Size(116, 33);
            Send_136_Initial.TabIndex = 204;
            Send_136_Initial.Text = "Initial";
            Send_136_Initial.UseVisualStyleBackColor = true;
            Send_136_Initial.Click += Send_136_Initial_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1155, 863);
            Controls.Add(grpConnection);
            Controls.Add(dgvOht);
            Controls.Add(groupBox2);
            Controls.Add(grpContinuousSend);
            Controls.Add(grpSimOptions);
            Controls.Add(grpDelay);
            Controls.Add(grpMismatchID);
            Controls.Add(btnCleanRichTextBox);
            Controls.Add(grpCST);
            Controls.Add(grpAlarm);
            Controls.Add(grpStopBreakdown);
            Controls.Add(grpSECS);
            Controls.Add(grpDisconnect);
            Controls.Add(grpThread);
            Controls.Add(grpTeaching);
            Controls.Add(txtError);
            Controls.Add(label7);
            Controls.Add(label9);
            Controls.Add(label10);
            Controls.Add(label12);
            Controls.Add(label17);
            Controls.Add(label18);
            Controls.Add(label22);
            Controls.Add(label25);
            Controls.Add(textBox3);
            Controls.Add(textBox4);
            Controls.Add(textBox5);
            Controls.Add(statusStrip1);
            Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            Name = "Form1";
            Text = "Mirle300Commucator";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvOht).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            grpConnection.ResumeLayout(false);
            grpConnection.PerformLayout();
            grpInitPos.ResumeLayout(false);
            grpInitPos.PerformLayout();
            grpContinuousSend.ResumeLayout(false);
            grpContinuousSend.PerformLayout();
            grpSimOptions.ResumeLayout(false);
            grpSimOptions.PerformLayout();
            grpCST.ResumeLayout(false);
            grpCST.PerformLayout();
            grpAlarm.ResumeLayout(false);
            grpAlarm.PerformLayout();
            grpStopBreakdown.ResumeLayout(false);
            grpStopBreakdown.PerformLayout();
            grpSECS.ResumeLayout(false);
            grpDisconnect.ResumeLayout(false);
            grpDisconnect.PerformLayout();
            grpThread.ResumeLayout(false);
            grpThread.PerformLayout();
            grpTeaching.ResumeLayout(false);
            grpMismatchID.ResumeLayout(false);
            grpMismatchID.PerformLayout();
            grpDelay.ResumeLayout(false);
            grpDelay.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.DataGridView dgvOht;
        private System.Windows.Forms.Button btnAlarmSet;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisConnect;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnCleanRichTextBox;
        private System.Windows.Forms.Label txtError;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton_Abnormal_DoubleStorage;
        private System.Windows.Forms.RadioButton radioButton_Abnormal_EmptyRetrieval;
        private System.Windows.Forms.RadioButton radioButton_Abnormal_BcrDuplicate;
        private System.Windows.Forms.RadioButton radioButton_Abnormal_BcrMisMatch;
        private System.Windows.Forms.RadioButton radioButton_Abnormal_BcrReadFail;
        private System.Windows.Forms.RadioButton radioButton_Normal;
        private System.Windows.Forms.RadioButton radioButton_Abort;
        private System.Windows.Forms.RadioButton radioButton_Cancel;
        private System.Windows.Forms.RadioButton radioButton_InterlockError;
        private System.Windows.Forms.TextBox Mismatch_ID;
        private System.Windows.Forms.Button set_button;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox tbx_init_add_id;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox tbx_init_sec_id;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.RadioButton radioButton_MTL;
        private System.Windows.Forms.RadioButton Scan_ReadFail;
        private System.Windows.Forms.RadioButton Scan_Mismatche;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.Button stopSet_button;
        private System.Windows.Forms.Button stopClear_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAlarmCode;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.ComboBox cmbAlarmLevel;
        private System.Windows.Forms.Button btnAlarmClear;
        private System.Windows.Forms.TextBox txtCstID;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnCstSet;
        private System.Windows.Forms.Button btnCstClear;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.RadioButton radioButton_InterlockErrorUnload;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtLocalPort;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtLocalIP;
        private System.Windows.Forms.RadioButton radioButton_Reject31;
        private System.Windows.Forms.RadioButton radioButton_Reject41;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tbxMs;
        private System.Windows.Forms.Button btnOn;
        private System.Windows.Forms.Button btnOff;
        private System.Windows.Forms.CheckBox chbSetFlyToTarget;
        private System.Windows.Forms.Button btnSetJump;
        private System.Windows.Forms.CheckBox chbDisableLocChk;
        private System.Windows.Forms.Button btnDisableLocCheck;
        private System.Windows.Forms.CheckBox chbInSectionLoad;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button Button_BD_Car_Set;
        private System.Windows.Forms.Button button3Button_BD_Car_ReSet;
        private System.Windows.Forms.Button btnSecsCmdStart;
        private System.Windows.Forms.Button btnSecsCmdStop;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Button btn_DisCon;
        private System.Windows.Forms.Button btn_disConStop;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox txBox_ConTime;
        private System.Windows.Forms.TextBox txBox_DisConTime;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox tbxDelayTime;
        private System.Windows.Forms.Button btSetDelayTime;
        private System.Windows.Forms.TextBox txtIOThread;
        private System.Windows.Forms.TextBox txtWorkThread;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Button btThreadStart;
        private System.Windows.Forms.DataGridViewTextBoxColumn ip;
        private System.Windows.Forms.DataGridViewTextBoxColumn vh_id;
        private System.Windows.Forms.ComboBox txtVhID;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Button bt_sendTeachData;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bt_setTeachReply;
        private System.Windows.Forms.RadioButton rb_teachNG;
        private System.Windows.Forms.RadioButton rb_teachOk;
        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.GroupBox grpContinuousSend;
        private System.Windows.Forms.GroupBox grpSimOptions;
        private System.Windows.Forms.GroupBox grpCST;
        private System.Windows.Forms.GroupBox grpAlarm;
        private System.Windows.Forms.GroupBox grpStopBreakdown;
        private System.Windows.Forms.GroupBox grpSECS;
        private System.Windows.Forms.GroupBox grpDisconnect;
        private System.Windows.Forms.GroupBox grpThread;
        private System.Windows.Forms.GroupBox grpTeaching;
        private System.Windows.Forms.GroupBox grpMismatchID;
        private System.Windows.Forms.GroupBox grpInitPos;
        private System.Windows.Forms.GroupBox grpDelay;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button Send_136_Initial;
    }
}
