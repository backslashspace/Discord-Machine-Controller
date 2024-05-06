using System;

namespace Link_Master
{
    partial class Service
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(Boolean disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.ServiceName = Program.ServiceName;
        }
    }
}