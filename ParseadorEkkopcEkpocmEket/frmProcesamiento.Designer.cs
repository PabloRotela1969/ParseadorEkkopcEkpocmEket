namespace ParseadorEkkopcEkpocmEket
{
    partial class frmProcesamiento
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnArchivoConfiguracion = new System.Windows.Forms.Button();
            this.btnAbrirCarpetaLectura = new System.Windows.Forms.Button();
            this.btnProcesar = new System.Windows.Forms.Button();
            this.btnLog = new System.Windows.Forms.Button();
            this.lblEstado = new System.Windows.Forms.Label();
            this.ttInformacion = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // btnArchivoConfiguracion
            // 
            this.btnArchivoConfiguracion.Location = new System.Drawing.Point(12, 12);
            this.btnArchivoConfiguracion.Name = "btnArchivoConfiguracion";
            this.btnArchivoConfiguracion.Size = new System.Drawing.Size(276, 30);
            this.btnArchivoConfiguracion.TabIndex = 0;
            this.btnArchivoConfiguracion.Text = "Cargar Archivo de Configuracion";
            this.btnArchivoConfiguracion.UseVisualStyleBackColor = true;
            this.btnArchivoConfiguracion.Click += new System.EventHandler(this.btnArchivoConfiguracion_Click);
            this.btnArchivoConfiguracion.MouseHover += new System.EventHandler(this.btnArchivoConfiguracion_MouseHover);

            // 
            // btnAbrirCarpetaLectura
            // 
            this.btnAbrirCarpetaLectura.Location = new System.Drawing.Point(12, 48);
            this.btnAbrirCarpetaLectura.Name = "btnAbrirCarpetaLectura";
            this.btnAbrirCarpetaLectura.Size = new System.Drawing.Size(276, 23);
            this.btnAbrirCarpetaLectura.TabIndex = 1;
            this.btnAbrirCarpetaLectura.Text = "Abrir carpeta de lectura de archivos";
            this.btnAbrirCarpetaLectura.UseVisualStyleBackColor = true;
            this.btnAbrirCarpetaLectura.Click += new System.EventHandler(this.btnAbrirCarpetaLectura_Click);
            this.btnAbrirCarpetaLectura.MouseHover += new System.EventHandler(this.btnAbrirCarpetaLectura_MouseHover);

            // 
            // btnProcesar
            // 
            this.btnProcesar.Location = new System.Drawing.Point(12, 111);
            this.btnProcesar.Name = "btnProcesar";
            this.btnProcesar.Size = new System.Drawing.Size(162, 39);
            this.btnProcesar.TabIndex = 2;
            this.btnProcesar.Text = "Procesar archivos";
            this.btnProcesar.UseVisualStyleBackColor = true;
            this.btnProcesar.Click += new System.EventHandler(this.btnProcesar_Click);
            this.btnProcesar.MouseHover += new System.EventHandler(this.btnProcesar_MouseHover);
            // 
            // btnLog
            // 
            this.btnLog.Location = new System.Drawing.Point(12, 77);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(276, 26);
            this.btnLog.TabIndex = 3;
            this.btnLog.Text = "Cargar archivo de log";
            this.btnLog.UseVisualStyleBackColor = true;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            this.btnLog.MouseHover += new System.EventHandler(this.btnLog_MouseHover);
            // 
            // lblEstado
            // 
            this.lblEstado.AutoSize = true;
            this.lblEstado.Location = new System.Drawing.Point(180, 111);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(0, 13);
            this.lblEstado.TabIndex = 4;
            // 
            // frmProcesamiento
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 166);
            this.Controls.Add(this.btnArchivoConfiguracion);
            this.Controls.Add(this.btnAbrirCarpetaLectura);
            this.Controls.Add(this.lblEstado);
            this.Controls.Add(this.btnLog);
            this.Controls.Add(this.btnProcesar);
            this.Name = "frmProcesamiento";
            this.Text = "Proceso de archivos";
            this.MouseHover += new System.EventHandler(this.frmProcesamiento_MouseHover);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnArchivoConfiguracion;
        private System.Windows.Forms.Button btnAbrirCarpetaLectura;
        private System.Windows.Forms.Button btnProcesar;
        private System.Windows.Forms.Button btnLog;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.ToolTip ttInformacion;
    }
}

