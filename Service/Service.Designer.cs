using System;

namespace DC_SRV_VM_LINK.Service
{
    partial class Service
    {
        private static System.ComponentModel.IContainer components = null;

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

            this.ServiceName = Program.SRV_Name;
        }
    }
}