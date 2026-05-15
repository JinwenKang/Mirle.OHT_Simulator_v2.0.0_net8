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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.dgvOht = new System.Windows.Forms.DataGridView();
            this.ip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.vh_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAlarmSet = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisConnect = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnCleanRichTextBox = new System.Windows.Forms.Button();
            this.txtError = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButton_Reject41 = new System.Windows.Forms.RadioButton();
            this.radioButton_Reject31 = new System.Windows.Forms.RadioButton();
            this.radioButton_InterlockErrorUnload = new System.Windows.Forms.RadioButton();
            this.Scan_ReadFail = new System.Windows.Forms.RadioButton();
            this.Scan_Mismatche = new System.Windows.Forms.RadioButton();
            this.radioButton_MTL = new System.Windows.Forms.RadioButton();
            this.radioButton_InterlockError = new System.Windows.Forms.RadioButton();
            this.radioButton_Abort = new System.Windows.Forms.RadioButton();
            this.radioButton_Cancel = new System.Windows.Forms.RadioButton();
            this.radioButton_Abnormal_DoubleStorage = new System.Windows.Forms.RadioButton();
            this.radioButton_Abnormal_EmptyRetrieval = new System.Windows.Forms.RadioButton();
            this.radioButton_Abnormal_BcrDuplicate = new System.Windows.Forms.RadioButton();
            this.radioButton_Abnormal_BcrMisMatch = new System.Windows.Forms.RadioButton();
            this.radioButton_Abnormal_BcrReadFail = new System.Windows.Forms.RadioButton();
            this.radioButton_Normal = new System.Windows.Forms.RadioButton();
            this.Mismatch_ID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.set_button = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.tbx_init_add_id = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbx_init_sec_id = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnSet = new System.Windows.Forms.Button();
            this.stopSet_button = new System.Windows.Forms.Button();
            this.stopClear_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAlarmCode = new System.Windows.Forms.TextBox();
            this.btnAlarmClear = new System.Windows.Forms.Button();
            this.txtCstID = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.btnCstSet = new System.Windows.Forms.Button();
            this.btnCstClear = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtLocalPort = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtLocalIP = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.tbxMs = new System.Windows.Forms.TextBox();
            this.btnOn = new System.Windows.Forms.Button();
            this.btnOff = new System.Windows.Forms.Button();
            this.chbSetFlyToTarget = new System.Windows.Forms.CheckBox();
            this.btnSetJump = new System.Windows.Forms.Button();
            this.chbDisableLocChk = new System.Windows.Forms.CheckBox();
            this.btnDisableLocCheck = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.Button_BD_Car_Set = new System.Windows.Forms.Button();
            this.button3Button_BD_Car_ReSet = new System.Windows.Forms.Button();
            this.btnSecsCmdStart = new System.Windows.Forms.Button();
            this.btnSecsCmdStop = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.btn_DisCon = new System.Windows.Forms.Button();
            this.btn_disConStop = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.txBox_ConTime = new System.Windows.Forms.TextBox();
            this.txBox_DisConTime = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.tbxDelayTime = new System.Windows.Forms.TextBox();
            this.btSetDelayTime = new System.Windows.Forms.Button();
            this.txtIOThread = new System.Windows.Forms.TextBox();
            this.txtWorkThread = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.btThreadStart = new System.Windows.Forms.Button();
            this.txtVhID = new System.Windows.Forms.TextBox();
            this.label25 = new System.Windows.Forms.Label();
            this.bt_sendTeachData = new System.Windows.Forms.Button();
            this.label26 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb_teachOk = new System.Windows.Forms.RadioButton();
            this.rb_teachNG = new System.Windows.Forms.RadioButton();
            this.bt_setTeachReply = new System.Windows.Forms.Button();
            this.chbInSectionLoad = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOht)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvOht
            // 
            this.dgvOht.AllowUserToAddRows = false;
            this.dgvOht.AllowUserToDeleteRows = false;
            this.dgvOht.AllowUserToResizeColumns = false;
            this.dgvOht.AllowUserToResizeRows = false;
            this.dgvOht.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOht.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ip,
            this.vh_id});
            this.dgvOht.Location = new System.Drawing.Point(12, 77);
            this.dgvOht.Name = "dgvOht";
            this.dgvOht.ReadOnly = true;
            this.dgvOht.RowHeadersVisible = false;
            this.dgvOht.RowHeadersWidth = 51;
            this.dgvOht.RowTemplate.Height = 24;
            this.dgvOht.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOht.Size = new System.Drawing.Size(399, 354);
            this.dgvOht.TabIndex = 3;
            // 
            // ip
            // 
            this.ip.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ip.HeaderText = "IP";
            this.ip.MinimumWidth = 6;
            this.ip.Name = "ip";
            this.ip.ReadOnly = true;
            this.ip.Width = 150;
            // 
            // vh_id
            // 
            this.vh_id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.vh_id.HeaderText = "Vehicle ID";
            this.vh_id.MinimumWidth = 6;
            this.vh_id.Name = "vh_id";
            this.vh_id.ReadOnly = true;
            // 
            // btnAlarmSet
            // 
            this.btnAlarmSet.Location = new System.Drawing.Point(404, 548);
            this.btnAlarmSet.Name = "btnAlarmSet";
            this.btnAlarmSet.Size = new System.Drawing.Size(156, 23);
            this.btnAlarmSet.TabIndex = 5;
            this.btnAlarmSet.Text = "Alarm Set";
            this.btnAlarmSet.UseVisualStyleBackColor = true;
            this.btnAlarmSet.Click += new System.EventHandler(this.btnAlarmSet_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3});
            this.statusStrip1.Location = new System.Drawing.Point(0, 820);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(990, 24);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(76, 19);
            this.toolStripStatusLabel1.Text = "Dis-Connect";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(889, 19);
            this.toolStripStatusLabel2.Spring = true;
            this.toolStripStatusLabel2.Text = " ";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(10, 19);
            this.toolStripStatusLabel3.Text = " ";
            // 
            // txtIp
            // 
            this.txtIp.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtIp.Location = new System.Drawing.Point(85, 12);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(141, 27);
            this.txtIp.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(7, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "Remote IP";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(485, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 16);
            this.label3.TabIndex = 10;
            this.label3.Text = "Vehicle ID";
            // 
            // btnConnect
            // 
            this.btnConnect.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnConnect.Location = new System.Drawing.Point(699, 11);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 13;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnDisConnect.Location = new System.Drawing.Point(692, 39);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(105, 23);
            this.btnDisConnect.TabIndex = 14;
            this.btnDisConnect.Text = "DisConnect";
            this.btnDisConnect.UseVisualStyleBackColor = true;
            this.btnDisConnect.Click += new System.EventHandler(this.btnDisConnect_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnCleanRichTextBox
            // 
            this.btnCleanRichTextBox.Location = new System.Drawing.Point(771, 168);
            this.btnCleanRichTextBox.Name = "btnCleanRichTextBox";
            this.btnCleanRichTextBox.Size = new System.Drawing.Size(190, 27);
            this.btnCleanRichTextBox.TabIndex = 28;
            this.btnCleanRichTextBox.Text = "Clean RichTextBox";
            this.btnCleanRichTextBox.UseVisualStyleBackColor = true;
            this.btnCleanRichTextBox.Click += new System.EventHandler(this.btnCleanRichTextBox_Click);
            // 
            // txtError
            // 
            this.txtError.AutoSize = true;
            this.txtError.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtError.Location = new System.Drawing.Point(829, 198);
            this.txtError.Name = "txtError";
            this.txtError.Size = new System.Drawing.Size(68, 16);
            this.txtError.TabIndex = 41;
            this.txtError.Text = "Opera Set";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButton_Reject41);
            this.groupBox2.Controls.Add(this.radioButton_Reject31);
            this.groupBox2.Controls.Add(this.radioButton_InterlockErrorUnload);
            this.groupBox2.Controls.Add(this.Scan_ReadFail);
            this.groupBox2.Controls.Add(this.Scan_Mismatche);
            this.groupBox2.Controls.Add(this.radioButton_MTL);
            this.groupBox2.Controls.Add(this.radioButton_InterlockError);
            this.groupBox2.Controls.Add(this.radioButton_Abort);
            this.groupBox2.Controls.Add(this.radioButton_Cancel);
            this.groupBox2.Controls.Add(this.radioButton_Abnormal_DoubleStorage);
            this.groupBox2.Controls.Add(this.radioButton_Abnormal_EmptyRetrieval);
            this.groupBox2.Controls.Add(this.radioButton_Abnormal_BcrDuplicate);
            this.groupBox2.Controls.Add(this.radioButton_Abnormal_BcrMisMatch);
            this.groupBox2.Controls.Add(this.radioButton_Abnormal_BcrReadFail);
            this.groupBox2.Controls.Add(this.radioButton_Normal);
            this.groupBox2.Location = new System.Drawing.Point(439, 96);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(278, 334);
            this.groupBox2.TabIndex = 43;
            this.groupBox2.TabStop = false;
            // 
            // radioButton_Reject31
            // 
            this.radioButton_Reject31.AutoSize = true;
            this.radioButton_Reject31.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Reject31.Location = new System.Drawing.Point(6, 298);
            this.radioButton_Reject31.Name = "radioButton_Reject31";
            this.radioButton_Reject31.Size = new System.Drawing.Size(93, 20);
            this.radioButton_Reject31.TabIndex = 53;
            this.radioButton_Reject31.Tag = "EnumAddress.Client";
            this.radioButton_Reject31.Text = "Reject 31";
            this.radioButton_Reject31.UseVisualStyleBackColor = true;
            // 
            // radioButton_Reject41
            // 
            this.radioButton_Reject41.AutoSize = true;
            this.radioButton_Reject41.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Reject41.Location = new System.Drawing.Point(120, 298);
            this.radioButton_Reject41.Name = "radioButton_Reject41";
            this.radioButton_Reject41.Size = new System.Drawing.Size(93, 20);
            this.radioButton_Reject41.TabIndex = 54;
            this.radioButton_Reject41.Tag = "EnumAddress.Client";
            this.radioButton_Reject41.Text = "Reject 41";
            this.radioButton_Reject41.UseVisualStyleBackColor = true;
            this.radioButton_Reject41.CheckedChanged += new System.EventHandler(this.radioButton_Reject41_CheckedChanged);
            // 
            // radioButton_InterlockErrorUnload
            // 
            this.radioButton_InterlockErrorUnload.AutoSize = true;
            this.radioButton_InterlockErrorUnload.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_InterlockErrorUnload.Location = new System.Drawing.Point(6, 246);
            this.radioButton_InterlockErrorUnload.Name = "radioButton_InterlockErrorUnload";
            this.radioButton_InterlockErrorUnload.Size = new System.Drawing.Size(179, 20);
            this.radioButton_InterlockErrorUnload.TabIndex = 52;
            this.radioButton_InterlockErrorUnload.Tag = "EnumAddress.Client";
            this.radioButton_InterlockErrorUnload.Text = "InterlockErrorUnload";
            this.radioButton_InterlockErrorUnload.UseVisualStyleBackColor = true;
            // 
            // Scan_ReadFail
            // 
            this.Scan_ReadFail.AutoSize = true;
            this.Scan_ReadFail.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Scan_ReadFail.Location = new System.Drawing.Point(153, 272);
            this.Scan_ReadFail.Name = "Scan_ReadFail";
            this.Scan_ReadFail.Size = new System.Drawing.Size(74, 20);
            this.Scan_ReadFail.TabIndex = 51;
            this.Scan_ReadFail.Tag = "EnumAddress.Client";
            this.Scan_ReadFail.Text = "Scan-F";
            this.Scan_ReadFail.UseVisualStyleBackColor = true;
            // 
            // Scan_Mismatche
            // 
            this.Scan_Mismatche.AutoSize = true;
            this.Scan_Mismatche.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Scan_Mismatche.Location = new System.Drawing.Point(68, 272);
            this.Scan_Mismatche.Name = "Scan_Mismatche";
            this.Scan_Mismatche.Size = new System.Drawing.Size(79, 20);
            this.Scan_Mismatche.TabIndex = 50;
            this.Scan_Mismatche.Tag = "EnumAddress.Client";
            this.Scan_Mismatche.Text = "Scan-M";
            this.Scan_Mismatche.UseVisualStyleBackColor = true;
            // 
            // radioButton_MTL
            // 
            this.radioButton_MTL.AutoSize = true;
            this.radioButton_MTL.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_MTL.Location = new System.Drawing.Point(6, 272);
            this.radioButton_MTL.Name = "radioButton_MTL";
            this.radioButton_MTL.Size = new System.Drawing.Size(59, 20);
            this.radioButton_MTL.TabIndex = 49;
            this.radioButton_MTL.Tag = "EnumAddress.Client";
            this.radioButton_MTL.Text = "MTL";
            this.radioButton_MTL.UseVisualStyleBackColor = true;
            // 
            // radioButton_InterlockError
            // 
            this.radioButton_InterlockError.AutoSize = true;
            this.radioButton_InterlockError.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_InterlockError.Location = new System.Drawing.Point(5, 220);
            this.radioButton_InterlockError.Name = "radioButton_InterlockError";
            this.radioButton_InterlockError.Size = new System.Drawing.Size(163, 20);
            this.radioButton_InterlockError.TabIndex = 49;
            this.radioButton_InterlockError.Tag = "EnumAddress.Client";
            this.radioButton_InterlockError.Text = "InterlockErrorLoad";
            this.radioButton_InterlockError.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abort
            // 
            this.radioButton_Abort.AutoSize = true;
            this.radioButton_Abort.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Abort.Location = new System.Drawing.Point(5, 194);
            this.radioButton_Abort.Name = "radioButton_Abort";
            this.radioButton_Abort.Size = new System.Drawing.Size(134, 20);
            this.radioButton_Abort.TabIndex = 48;
            this.radioButton_Abort.Tag = "EnumAddress.Server";
            this.radioButton_Abort.Text = "AbortComplete";
            this.radioButton_Abort.UseVisualStyleBackColor = true;
            // 
            // radioButton_Cancel
            // 
            this.radioButton_Cancel.AutoSize = true;
            this.radioButton_Cancel.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Cancel.Location = new System.Drawing.Point(5, 168);
            this.radioButton_Cancel.Name = "radioButton_Cancel";
            this.radioButton_Cancel.Size = new System.Drawing.Size(142, 20);
            this.radioButton_Cancel.TabIndex = 47;
            this.radioButton_Cancel.Tag = "EnumAddress.Server";
            this.radioButton_Cancel.Text = "CancelComplete";
            this.radioButton_Cancel.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_DoubleStorage
            // 
            this.radioButton_Abnormal_DoubleStorage.AutoSize = true;
            this.radioButton_Abnormal_DoubleStorage.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Abnormal_DoubleStorage.Location = new System.Drawing.Point(5, 116);
            this.radioButton_Abnormal_DoubleStorage.Name = "radioButton_Abnormal_DoubleStorage";
            this.radioButton_Abnormal_DoubleStorage.Size = new System.Drawing.Size(211, 20);
            this.radioButton_Abnormal_DoubleStorage.TabIndex = 47;
            this.radioButton_Abnormal_DoubleStorage.Tag = "EnumAddress.Server";
            this.radioButton_Abnormal_DoubleStorage.Text = "Abnormal_DoubleStorage";
            this.radioButton_Abnormal_DoubleStorage.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_EmptyRetrieval
            // 
            this.radioButton_Abnormal_EmptyRetrieval.AutoSize = true;
            this.radioButton_Abnormal_EmptyRetrieval.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Abnormal_EmptyRetrieval.Location = new System.Drawing.Point(5, 142);
            this.radioButton_Abnormal_EmptyRetrieval.Name = "radioButton_Abnormal_EmptyRetrieval";
            this.radioButton_Abnormal_EmptyRetrieval.Size = new System.Drawing.Size(216, 20);
            this.radioButton_Abnormal_EmptyRetrieval.TabIndex = 46;
            this.radioButton_Abnormal_EmptyRetrieval.Tag = "EnumAddress.Server";
            this.radioButton_Abnormal_EmptyRetrieval.Text = "Abnormal_EmptyRetrieval";
            this.radioButton_Abnormal_EmptyRetrieval.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_BcrDuplicate
            // 
            this.radioButton_Abnormal_BcrDuplicate.AutoSize = true;
            this.radioButton_Abnormal_BcrDuplicate.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Abnormal_BcrDuplicate.Location = new System.Drawing.Point(5, 90);
            this.radioButton_Abnormal_BcrDuplicate.Name = "radioButton_Abnormal_BcrDuplicate";
            this.radioButton_Abnormal_BcrDuplicate.Size = new System.Drawing.Size(199, 20);
            this.radioButton_Abnormal_BcrDuplicate.TabIndex = 45;
            this.radioButton_Abnormal_BcrDuplicate.Tag = "EnumAddress.Server";
            this.radioButton_Abnormal_BcrDuplicate.Text = "Abnormal_BcrDuplicate";
            this.radioButton_Abnormal_BcrDuplicate.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_BcrMisMatch
            // 
            this.radioButton_Abnormal_BcrMisMatch.AutoSize = true;
            this.radioButton_Abnormal_BcrMisMatch.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Abnormal_BcrMisMatch.Location = new System.Drawing.Point(5, 64);
            this.radioButton_Abnormal_BcrMisMatch.Name = "radioButton_Abnormal_BcrMisMatch";
            this.radioButton_Abnormal_BcrMisMatch.Size = new System.Drawing.Size(200, 20);
            this.radioButton_Abnormal_BcrMisMatch.TabIndex = 44;
            this.radioButton_Abnormal_BcrMisMatch.Tag = "EnumAddress.Server";
            this.radioButton_Abnormal_BcrMisMatch.Text = "Abnormal_BcrMisMatch";
            this.radioButton_Abnormal_BcrMisMatch.UseVisualStyleBackColor = true;
            // 
            // radioButton_Abnormal_BcrReadFail
            // 
            this.radioButton_Abnormal_BcrReadFail.AutoSize = true;
            this.radioButton_Abnormal_BcrReadFail.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Abnormal_BcrReadFail.Location = new System.Drawing.Point(5, 38);
            this.radioButton_Abnormal_BcrReadFail.Name = "radioButton_Abnormal_BcrReadFail";
            this.radioButton_Abnormal_BcrReadFail.Size = new System.Drawing.Size(193, 20);
            this.radioButton_Abnormal_BcrReadFail.TabIndex = 1;
            this.radioButton_Abnormal_BcrReadFail.Tag = "EnumAddress.Server";
            this.radioButton_Abnormal_BcrReadFail.Text = "Abnormal_BcrReadFail";
            this.radioButton_Abnormal_BcrReadFail.UseVisualStyleBackColor = true;
            // 
            // radioButton_Normal
            // 
            this.radioButton_Normal.AutoSize = true;
            this.radioButton_Normal.Checked = true;
            this.radioButton_Normal.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_Normal.Location = new System.Drawing.Point(5, 12);
            this.radioButton_Normal.Name = "radioButton_Normal";
            this.radioButton_Normal.Size = new System.Drawing.Size(78, 20);
            this.radioButton_Normal.TabIndex = 0;
            this.radioButton_Normal.TabStop = true;
            this.radioButton_Normal.Tag = "EnumAddress.Client";
            this.radioButton_Normal.Text = "Normal";
            this.radioButton_Normal.UseVisualStyleBackColor = true;
            // 
            // Mismatch_ID
            // 
            this.Mismatch_ID.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Mismatch_ID.Location = new System.Drawing.Point(791, 505);
            this.Mismatch_ID.Name = "Mismatch_ID";
            this.Mismatch_ID.Size = new System.Drawing.Size(156, 27);
            this.Mismatch_ID.TabIndex = 44;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(820, 486);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 16);
            this.label5.TabIndex = 45;
            this.label5.Text = "Mismatch ID";
            // 
            // set_button
            // 
            this.set_button.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.set_button.Location = new System.Drawing.Point(823, 538);
            this.set_button.Name = "set_button";
            this.set_button.Size = new System.Drawing.Size(75, 23);
            this.set_button.TabIndex = 46;
            this.set_button.Text = "Set";
            this.set_button.UseVisualStyleBackColor = true;
            this.set_button.Click += new System.EventHandler(this.set_button_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label8.Location = new System.Drawing.Point(802, 18);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(79, 16);
            this.label8.TabIndex = 48;
            this.label8.Text = "Init_AddID";
            // 
            // tbx_init_add_id
            // 
            this.tbx_init_add_id.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbx_init_add_id.Location = new System.Drawing.Point(889, 14);
            this.tbx_init_add_id.Name = "tbx_init_add_id";
            this.tbx_init_add_id.Size = new System.Drawing.Size(62, 27);
            this.tbx_init_add_id.TabIndex = 47;
            this.tbx_init_add_id.Text = "10701";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label9.Location = new System.Drawing.Point(1281, 12);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(71, 16);
            this.label9.TabIndex = 48;
            this.label9.Text = "Local Port";
            // 
            // tbx_init_sec_id
            // 
            this.tbx_init_sec_id.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbx_init_sec_id.Location = new System.Drawing.Point(889, 43);
            this.tbx_init_sec_id.Name = "tbx_init_sec_id";
            this.tbx_init_sec_id.Size = new System.Drawing.Size(62, 27);
            this.tbx_init_sec_id.TabIndex = 47;
            this.tbx_init_sec_id.Text = "34601";
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox3.Location = new System.Drawing.Point(1352, 37);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(62, 27);
            this.textBox3.TabIndex = 47;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label10.Location = new System.Drawing.Point(1774, 39);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 16);
            this.label10.TabIndex = 48;
            this.label10.Text = "Local Port";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label11.Location = new System.Drawing.Point(803, 46);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(71, 16);
            this.label11.TabIndex = 48;
            this.label11.Text = "Init_SecId";
            // 
            // textBox4
            // 
            this.textBox4.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox4.Location = new System.Drawing.Point(1360, 63);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(62, 27);
            this.textBox4.TabIndex = 47;
            // 
            // textBox5
            // 
            this.textBox5.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox5.Location = new System.Drawing.Point(1852, 61);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(62, 27);
            this.textBox5.TabIndex = 47;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label12.Location = new System.Drawing.Point(2275, 63);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(71, 16);
            this.label12.TabIndex = 48;
            this.label12.Text = "Local Port";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label7.Location = new System.Drawing.Point(436, 77);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 16);
            this.label7.TabIndex = 49;
            this.label7.Text = "Opera Set";
            // 
            // btnSet
            // 
            this.btnSet.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSet.Location = new System.Drawing.Point(439, 436);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(75, 23);
            this.btnSet.TabIndex = 50;
            this.btnSet.Text = "Set";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // stopSet_button
            // 
            this.stopSet_button.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.stopSet_button.Location = new System.Drawing.Point(608, 509);
            this.stopSet_button.Name = "stopSet_button";
            this.stopSet_button.Size = new System.Drawing.Size(89, 23);
            this.stopSet_button.TabIndex = 51;
            this.stopSet_button.Text = "Stop Set";
            this.stopSet_button.UseVisualStyleBackColor = true;
            this.stopSet_button.Click += new System.EventHandler(this.stopSet_button_Click);
            // 
            // stopClear_button
            // 
            this.stopClear_button.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.stopClear_button.Location = new System.Drawing.Point(608, 547);
            this.stopClear_button.Name = "stopClear_button";
            this.stopClear_button.Size = new System.Drawing.Size(89, 23);
            this.stopClear_button.TabIndex = 52;
            this.stopClear_button.Text = "Stop Clear";
            this.stopClear_button.UseVisualStyleBackColor = true;
            this.stopClear_button.Click += new System.EventHandler(this.stopClear_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(441, 486);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 16);
            this.label1.TabIndex = 53;
            this.label1.Text = "Alarm Code";
            // 
            // txtAlarmCode
            // 
            this.txtAlarmCode.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAlarmCode.Location = new System.Drawing.Point(404, 509);
            this.txtAlarmCode.Name = "txtAlarmCode";
            this.txtAlarmCode.Size = new System.Drawing.Size(156, 27);
            this.txtAlarmCode.TabIndex = 54;
            // 
            // btnAlarmClear
            // 
            this.btnAlarmClear.Location = new System.Drawing.Point(404, 577);
            this.btnAlarmClear.Name = "btnAlarmClear";
            this.btnAlarmClear.Size = new System.Drawing.Size(156, 23);
            this.btnAlarmClear.TabIndex = 55;
            this.btnAlarmClear.Text = "Alarm Clear";
            this.btnAlarmClear.UseVisualStyleBackColor = true;
            this.btnAlarmClear.Click += new System.EventHandler(this.btnAlarmClear_Click);
            // 
            // txtCstID
            // 
            this.txtCstID.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtCstID.Location = new System.Drawing.Point(236, 509);
            this.txtCstID.Name = "txtCstID";
            this.txtCstID.Size = new System.Drawing.Size(156, 27);
            this.txtCstID.TabIndex = 58;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label13.Location = new System.Drawing.Point(266, 486);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(54, 16);
            this.label13.TabIndex = 57;
            this.label13.Text = "CST ID";
            // 
            // btnCstSet
            // 
            this.btnCstSet.Location = new System.Drawing.Point(236, 546);
            this.btnCstSet.Name = "btnCstSet";
            this.btnCstSet.Size = new System.Drawing.Size(156, 23);
            this.btnCstSet.TabIndex = 56;
            this.btnCstSet.Text = "Cst Set";
            this.btnCstSet.UseVisualStyleBackColor = true;
            this.btnCstSet.Click += new System.EventHandler(this.btnCstSet_Click);
            // 
            // btnCstClear
            // 
            this.btnCstClear.Location = new System.Drawing.Point(236, 577);
            this.btnCstClear.Name = "btnCstClear";
            this.btnCstClear.Size = new System.Drawing.Size(156, 23);
            this.btnCstClear.TabIndex = 59;
            this.btnCstClear.Text = "Cst Clear";
            this.btnCstClear.UseVisualStyleBackColor = true;
            this.btnCstClear.Click += new System.EventHandler(this.btnCstClear_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label6.Location = new System.Drawing.Point(232, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 16);
            this.label6.TabIndex = 62;
            this.label6.Text = "Remote Port";
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtPort.Location = new System.Drawing.Point(322, 13);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(141, 27);
            this.txtPort.TabIndex = 61;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(232, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 16);
            this.label4.TabIndex = 63;
            this.label4.Text = "Local Port";
            // 
            // txtLocalPort
            // 
            this.txtLocalPort.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtLocalPort.Location = new System.Drawing.Point(322, 47);
            this.txtLocalPort.Name = "txtLocalPort";
            this.txtLocalPort.Size = new System.Drawing.Size(141, 27);
            this.txtLocalPort.TabIndex = 64;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label14.Location = new System.Drawing.Point(9, 50);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(59, 16);
            this.label14.TabIndex = 65;
            this.label14.Text = "Local IP";
            // 
            // txtLocalIP
            // 
            this.txtLocalIP.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtLocalIP.Location = new System.Drawing.Point(85, 46);
            this.txtLocalIP.Name = "txtLocalIP";
            this.txtLocalIP.Size = new System.Drawing.Size(141, 27);
            this.txtLocalIP.TabIndex = 66;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button1.Location = new System.Drawing.Point(565, 43);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 58);
            this.button1.TabIndex = 67;
            this.button1.Text = "Auto Connect All OHT";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label15.Location = new System.Drawing.Point(788, 264);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(170, 16);
            this.label15.TabIndex = 70;
            this.label15.Text = "Continuous Send 134(ms)";
            // 
            // tbxMs
            // 
            this.tbxMs.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxMs.Location = new System.Drawing.Point(832, 290);
            this.tbxMs.Name = "tbxMs";
            this.tbxMs.Size = new System.Drawing.Size(72, 27);
            this.tbxMs.TabIndex = 69;
            // 
            // btnOn
            // 
            this.btnOn.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOn.Location = new System.Drawing.Point(780, 323);
            this.btnOn.Name = "btnOn";
            this.btnOn.Size = new System.Drawing.Size(75, 23);
            this.btnOn.TabIndex = 68;
            this.btnOn.Text = "On";
            this.btnOn.UseVisualStyleBackColor = true;
            this.btnOn.Click += new System.EventHandler(this.btnOn_Click);
            // 
            // btnOff
            // 
            this.btnOff.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOff.Location = new System.Drawing.Point(872, 323);
            this.btnOff.Name = "btnOff";
            this.btnOff.Size = new System.Drawing.Size(75, 23);
            this.btnOff.TabIndex = 71;
            this.btnOff.Text = "Off";
            this.btnOff.UseVisualStyleBackColor = true;
            this.btnOff.Click += new System.EventHandler(this.btnOff_Click);
            // 
            // chbSetFlyToTarget
            // 
            this.chbSetFlyToTarget.AutoSize = true;
            this.chbSetFlyToTarget.Font = new System.Drawing.Font("新細明體", 12F);
            this.chbSetFlyToTarget.Location = new System.Drawing.Point(733, 395);
            this.chbSetFlyToTarget.Name = "chbSetFlyToTarget";
            this.chbSetFlyToTarget.Size = new System.Drawing.Size(148, 20);
            this.chbSetFlyToTarget.TabIndex = 72;
            this.chbSetFlyToTarget.Text = "Set Jump To Target";
            this.chbSetFlyToTarget.UseVisualStyleBackColor = true;
            // 
            // btnSetJump
            // 
            this.btnSetJump.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSetJump.Location = new System.Drawing.Point(903, 393);
            this.btnSetJump.Name = "btnSetJump";
            this.btnSetJump.Size = new System.Drawing.Size(75, 23);
            this.btnSetJump.TabIndex = 73;
            this.btnSetJump.Text = "Set";
            this.btnSetJump.UseVisualStyleBackColor = true;
            this.btnSetJump.Click += new System.EventHandler(this.btnSetJump_Click);
            // 
            // chbDisableLocChk
            // 
            this.chbDisableLocChk.AutoSize = true;
            this.chbDisableLocChk.Font = new System.Drawing.Font("新細明體", 12F);
            this.chbDisableLocChk.Location = new System.Drawing.Point(733, 436);
            this.chbDisableLocChk.Name = "chbDisableLocChk";
            this.chbDisableLocChk.Size = new System.Drawing.Size(168, 20);
            this.chbDisableLocChk.TabIndex = 74;
            this.chbDisableLocChk.Text = "Disable Vh Loc Check";
            this.chbDisableLocChk.UseVisualStyleBackColor = true;
            // 
            // btnDisableLocCheck
            // 
            this.btnDisableLocCheck.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnDisableLocCheck.Location = new System.Drawing.Point(903, 433);
            this.btnDisableLocCheck.Name = "btnDisableLocCheck";
            this.btnDisableLocCheck.Size = new System.Drawing.Size(75, 23);
            this.btnDisableLocCheck.TabIndex = 75;
            this.btnDisableLocCheck.Text = "Set";
            this.btnDisableLocCheck.UseVisualStyleBackColor = true;
            this.btnDisableLocCheck.Click += new System.EventHandler(this.btnDisableLocCheck_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label16.Location = new System.Drawing.Point(802, 579);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(128, 16);
            this.label16.TabIndex = 76;
            this.label16.Text = "Breakdown Car Set";
            // 
            // Button_BD_Car_Set
            // 
            this.Button_BD_Car_Set.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Button_BD_Car_Set.Location = new System.Drawing.Point(780, 608);
            this.Button_BD_Car_Set.Name = "Button_BD_Car_Set";
            this.Button_BD_Car_Set.Size = new System.Drawing.Size(75, 23);
            this.Button_BD_Car_Set.TabIndex = 77;
            this.Button_BD_Car_Set.Text = "Set";
            this.Button_BD_Car_Set.UseVisualStyleBackColor = true;
            this.Button_BD_Car_Set.Click += new System.EventHandler(this.Button_BD_Car_Set_Click);
            // 
            // button3Button_BD_Car_ReSet
            // 
            this.button3Button_BD_Car_ReSet.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button3Button_BD_Car_ReSet.Location = new System.Drawing.Point(872, 608);
            this.button3Button_BD_Car_ReSet.Name = "button3Button_BD_Car_ReSet";
            this.button3Button_BD_Car_ReSet.Size = new System.Drawing.Size(75, 23);
            this.button3Button_BD_Car_ReSet.TabIndex = 78;
            this.button3Button_BD_Car_ReSet.Text = "Reset";
            this.button3Button_BD_Car_ReSet.UseVisualStyleBackColor = true;
            this.button3Button_BD_Car_ReSet.Click += new System.EventHandler(this.button3Button_BD_Car_ReSet_Click);
            // 
            // btnSecsCmdStart
            // 
            this.btnSecsCmdStart.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSecsCmdStart.Location = new System.Drawing.Point(22, 515);
            this.btnSecsCmdStart.Name = "btnSecsCmdStart";
            this.btnSecsCmdStart.Size = new System.Drawing.Size(75, 23);
            this.btnSecsCmdStart.TabIndex = 80;
            this.btnSecsCmdStart.Text = "Start";
            this.btnSecsCmdStart.UseVisualStyleBackColor = true;
            this.btnSecsCmdStart.Click += new System.EventHandler(this.btnSecsCmdStart_Click);
            // 
            // btnSecsCmdStop
            // 
            this.btnSecsCmdStop.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSecsCmdStop.Location = new System.Drawing.Point(114, 515);
            this.btnSecsCmdStop.Name = "btnSecsCmdStop";
            this.btnSecsCmdStop.Size = new System.Drawing.Size(75, 23);
            this.btnSecsCmdStop.TabIndex = 81;
            this.btnSecsCmdStop.Text = "Stop";
            this.btnSecsCmdStop.UseVisualStyleBackColor = true;
            this.btnSecsCmdStop.Click += new System.EventHandler(this.btnSecsCmdStop_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label17.Location = new System.Drawing.Point(17, 486);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(141, 16);
            this.label17.TabIndex = 79;
            this.label17.Text = "SECS Command Test";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label18.Location = new System.Drawing.Point(431, 654);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(107, 16);
            this.label18.TabIndex = 82;
            this.label18.Text = "Disconnect Test";
            // 
            // btn_DisCon
            // 
            this.btn_DisCon.Location = new System.Drawing.Point(404, 684);
            this.btn_DisCon.Name = "btn_DisCon";
            this.btn_DisCon.Size = new System.Drawing.Size(156, 23);
            this.btn_DisCon.TabIndex = 83;
            this.btn_DisCon.Text = "Disconect Test";
            this.btn_DisCon.UseVisualStyleBackColor = true;
            this.btn_DisCon.Click += new System.EventHandler(this.btn_DisCon_Click);
            // 
            // btn_disConStop
            // 
            this.btn_disConStop.Location = new System.Drawing.Point(404, 727);
            this.btn_disConStop.Name = "btn_disConStop";
            this.btn_disConStop.Size = new System.Drawing.Size(156, 23);
            this.btn_disConStop.TabIndex = 84;
            this.btn_disConStop.Text = "Disconect Test Stop";
            this.btn_disConStop.UseVisualStyleBackColor = true;
            this.btn_disConStop.Click += new System.EventHandler(this.btn_disConStop_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label19.Location = new System.Drawing.Point(566, 686);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(115, 16);
            this.label19.TabIndex = 85;
            this.label19.Text = "Connect Time(s):";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label20.Location = new System.Drawing.Point(566, 729);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(133, 16);
            this.label20.TabIndex = 86;
            this.label20.Text = "Disconnect Time(s):";
            // 
            // txBox_ConTime
            // 
            this.txBox_ConTime.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txBox_ConTime.Location = new System.Drawing.Point(699, 683);
            this.txBox_ConTime.Name = "txBox_ConTime";
            this.txBox_ConTime.Size = new System.Drawing.Size(52, 27);
            this.txBox_ConTime.TabIndex = 87;
            this.txBox_ConTime.Text = "3";
            // 
            // txBox_DisConTime
            // 
            this.txBox_DisConTime.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txBox_DisConTime.Location = new System.Drawing.Point(699, 723);
            this.txBox_DisConTime.Name = "txBox_DisConTime";
            this.txBox_DisConTime.Size = new System.Drawing.Size(52, 27);
            this.txBox_DisConTime.TabIndex = 88;
            this.txBox_DisConTime.Text = "15";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label21.Location = new System.Drawing.Point(401, 789);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(217, 16);
            this.label21.TabIndex = 91;
            this.label21.Text = "Set Reply OHTC Delay Time(ms)";
            // 
            // tbxDelayTime
            // 
            this.tbxDelayTime.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxDelayTime.Location = new System.Drawing.Point(625, 786);
            this.tbxDelayTime.Name = "tbxDelayTime";
            this.tbxDelayTime.Size = new System.Drawing.Size(72, 27);
            this.tbxDelayTime.TabIndex = 90;
            this.tbxDelayTime.Text = "0";
            // 
            // btSetDelayTime
            // 
            this.btSetDelayTime.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btSetDelayTime.Location = new System.Drawing.Point(706, 789);
            this.btSetDelayTime.Name = "btSetDelayTime";
            this.btSetDelayTime.Size = new System.Drawing.Size(75, 23);
            this.btSetDelayTime.TabIndex = 89;
            this.btSetDelayTime.Text = "Set";
            this.btSetDelayTime.UseVisualStyleBackColor = true;
            this.btSetDelayTime.Click += new System.EventHandler(this.btSetDelayTime_Click);
            // 
            // txtIOThread
            // 
            this.txtIOThread.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtIOThread.Location = new System.Drawing.Point(27, 738);
            this.txtIOThread.Name = "txtIOThread";
            this.txtIOThread.Size = new System.Drawing.Size(156, 27);
            this.txtIOThread.TabIndex = 92;
            // 
            // txtWorkThread
            // 
            this.txtWorkThread.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtWorkThread.Location = new System.Drawing.Point(27, 668);
            this.txtWorkThread.Name = "txtWorkThread";
            this.txtWorkThread.Size = new System.Drawing.Size(156, 27);
            this.txtWorkThread.TabIndex = 93;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label22.Location = new System.Drawing.Point(47, 611);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(95, 16);
            this.label22.TabIndex = 94;
            this.label22.Text = "Thread Watch";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label23.Location = new System.Drawing.Point(47, 643);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(90, 16);
            this.label23.TabIndex = 95;
            this.label23.Text = "Work Thread";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label24.Location = new System.Drawing.Point(47, 715);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(71, 16);
            this.label24.TabIndex = 96;
            this.label24.Text = "IO Thread";
            // 
            // btThreadStart
            // 
            this.btThreadStart.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btThreadStart.Location = new System.Drawing.Point(51, 775);
            this.btThreadStart.Name = "btThreadStart";
            this.btThreadStart.Size = new System.Drawing.Size(75, 23);
            this.btThreadStart.TabIndex = 97;
            this.btThreadStart.Text = "Start";
            this.btThreadStart.UseVisualStyleBackColor = true;
            this.btThreadStart.Click += new System.EventHandler(this.btThreadStart_Click);
            // 
            // txtVhID
            // 
            this.txtVhID.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtVhID.Location = new System.Drawing.Point(565, 12);
            this.txtVhID.Name = "txtVhID";
            this.txtVhID.Size = new System.Drawing.Size(121, 27);
            this.txtVhID.TabIndex = 98;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label25.Location = new System.Drawing.Point(803, 654);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(133, 16);
            this.label25.TabIndex = 100;
            this.label25.Text = "Send Teaching Data";
            // 
            // bt_sendTeachData
            // 
            this.bt_sendTeachData.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.bt_sendTeachData.Location = new System.Drawing.Point(823, 683);
            this.bt_sendTeachData.Name = "bt_sendTeachData";
            this.bt_sendTeachData.Size = new System.Drawing.Size(89, 23);
            this.bt_sendTeachData.TabIndex = 99;
            this.bt_sendTeachData.Text = "Send";
            this.bt_sendTeachData.UseVisualStyleBackColor = true;
            this.bt_sendTeachData.Click += new System.EventHandler(this.bt_sendTeachData_Click);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label26.Location = new System.Drawing.Point(806, 723);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(106, 16);
            this.label26.TabIndex = 101;
            this.label26.Text = "Teaching Reply";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bt_setTeachReply);
            this.groupBox1.Controls.Add(this.rb_teachNG);
            this.groupBox1.Controls.Add(this.rb_teachOk);
            this.groupBox1.Location = new System.Drawing.Point(787, 742);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(174, 71);
            this.groupBox1.TabIndex = 102;
            this.groupBox1.TabStop = false;
            // 
            // rb_teachOk
            // 
            this.rb_teachOk.AutoSize = true;
            this.rb_teachOk.Checked = true;
            this.rb_teachOk.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rb_teachOk.Location = new System.Drawing.Point(9, 21);
            this.rb_teachOk.Name = "rb_teachOk";
            this.rb_teachOk.Size = new System.Drawing.Size(49, 20);
            this.rb_teachOk.TabIndex = 54;
            this.rb_teachOk.TabStop = true;
            this.rb_teachOk.Tag = "EnumAddress.Client";
            this.rb_teachOk.Text = "OK";
            this.rb_teachOk.UseVisualStyleBackColor = true;
            // 
            // rb_teachNG
            // 
            this.rb_teachNG.AutoSize = true;
            this.rb_teachNG.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rb_teachNG.Location = new System.Drawing.Point(85, 21);
            this.rb_teachNG.Name = "rb_teachNG";
            this.rb_teachNG.Size = new System.Drawing.Size(49, 20);
            this.rb_teachNG.TabIndex = 54;
            this.rb_teachNG.Tag = "EnumAddress.Client";
            this.rb_teachNG.Text = "NG";
            this.rb_teachNG.UseVisualStyleBackColor = true;
            // 
            // bt_setTeachReply
            // 
            this.bt_setTeachReply.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.bt_setTeachReply.Location = new System.Drawing.Point(36, 42);
            this.bt_setTeachReply.Name = "bt_setTeachReply";
            this.bt_setTeachReply.Size = new System.Drawing.Size(89, 23);
            this.bt_setTeachReply.TabIndex = 103;
            this.bt_setTeachReply.Text = "Set";
            this.bt_setTeachReply.UseVisualStyleBackColor = true;
            this.bt_setTeachReply.Click += new System.EventHandler(this.bt_setTeachReply_Click);
            // 
            // chbInSectionLoad
            // 
            this.chbInSectionLoad.AutoSize = true;
            this.chbInSectionLoad.Font = new System.Drawing.Font("新細明體", 12F);
            this.chbInSectionLoad.Location = new System.Drawing.Point(733, 475);
            this.chbInSectionLoad.Name = "chbInSectionLoad";
            this.chbInSectionLoad.Size = new System.Drawing.Size(215, 20);
            this.chbInSectionLoad.TabIndex = 103;
            this.chbInSectionLoad.Text = "InSection Load Assignment";
            this.chbInSectionLoad.UseVisualStyleBackColor = true;
            this.chbInSectionLoad.CheckedChanged += new System.EventHandler(this.chbInSectionLoad_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(990, 844);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.bt_sendTeachData);
            this.Controls.Add(this.txtVhID);
            this.Controls.Add(this.btThreadStart);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.txtWorkThread);
            this.Controls.Add(this.txtIOThread);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.tbxDelayTime);
            this.Controls.Add(this.btSetDelayTime);
            this.Controls.Add(this.txBox_DisConTime);
            this.Controls.Add(this.txBox_ConTime);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.btn_disConStop);
            this.Controls.Add(this.btn_DisCon);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.btnSecsCmdStart);
            this.Controls.Add(this.btnSecsCmdStop);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.Button_BD_Car_Set);
            this.Controls.Add(this.button3Button_BD_Car_ReSet);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.chbInSectionLoad);
            this.Controls.Add(this.btnDisableLocCheck);
            this.Controls.Add(this.chbDisableLocChk);
            this.Controls.Add(this.btnSetJump);
            this.Controls.Add(this.chbSetFlyToTarget);
            this.Controls.Add(this.btnOff);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.tbxMs);
            this.Controls.Add(this.btnOn);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtLocalIP);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtLocalPort);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.btnCstClear);
            this.Controls.Add(this.txtCstID);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.btnCstSet);
            this.Controls.Add(this.btnAlarmClear);
            this.Controls.Add(this.txtAlarmCode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.stopClear_button);
            this.Controls.Add(this.stopSet_button);
            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbx_init_add_id);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.tbx_init_sec_id);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.set_button);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.Mismatch_ID);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.txtError);
            this.Controls.Add(this.btnCleanRichTextBox);
            this.Controls.Add(this.btnDisConnect);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtIp);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnAlarmSet);
            this.Controls.Add(this.dgvOht);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Mirle300Commucator";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOht)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button set_button;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbx_init_add_id;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbx_init_sec_id;
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
        private System.Windows.Forms.TextBox txtVhID;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Button bt_sendTeachData;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bt_setTeachReply;
        private System.Windows.Forms.RadioButton rb_teachNG;
        private System.Windows.Forms.RadioButton rb_teachOk;
    }
}

