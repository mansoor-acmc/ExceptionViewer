using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ExceptionViewer
{
    public partial class PalletLog : Form
    {
        public PalletLog()
        {
            InitializeComponent();
        }

        public string PickingListId { get; set; }
        public string SalesId { get; set; }
        public string DriverName { get; set; }
        public string TruckPlate { get; set; }

        private void PalletLog_Load(object sender, EventArgs e)
        {
            DataTable dtHistory = new DBClass(DBClass.DbName.DynamicsLive).GetPalletHistory(PickingListId);
            dgPallets.DataSource = dtHistory;
            dgPallets.Columns[1].Width = 120;

            lbSalesId.Text = "Sales No. "+SalesId;
            lbDriverName.Text = "Driver Name: "+DriverName;
            lbPickingId.Text = "Picking ID: "+PickingListId;
            lbTruckPlate.Text = "Truck No. "+TruckPlate;
        }
    }
}
