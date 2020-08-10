using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace ExceptionViewer
{
    public partial class Viewer : Form
    {
        public Viewer()
        {
            InitializeComponent();
        }

        private void Viewer_Load(object sender, EventArgs e)
        {
            LoadListItems();
            DataLoad();
            cmbPackingSlips.SelectedIndex = 0;
        }

        void LoadListItems()
        {
            List<ItemList> items = new List<ItemList>();
            items.Add(new ItemList() { Label = "Sales Service", Value = "SaleService" });
            items.Add(new ItemList() { Label = "Counting FG", Value = "FGCounting" });
            items.Add(new ItemList() { Label = "Counting Supply Chain", Value = "SCCounting" });
            items.Add(new ItemList() { Label = "EAM Services", Value = "EAMService" });

            cmbProject.DataSource = items;
            cmbProject.DisplayMember = "Label";
            cmbProject.ValueMember = "Value";
        }

        void DataLoad()
        {
            DBClass db = new DBClass(DBClass.DbName.DeviceMsg);
            var pings = db.GetPingData(dtpException.Value, cmbProject.SelectedValue.ToString(), tbSearch.Text.Trim());
            var items = from c in pings
                        select new
                        {
                            DeviceName = c.DeviceName,
                            UserName = c.Username,
                            DatePing = c.DateOccurString
                        };
            dgPing.DataSource = items.ToList();
            dgPing.Columns[2].Width = 125;
            lbPings.Text = "Count: " + pings.Count.ToString();

            var messages = db.GetDeviceMessages(dtpException.Value, cmbProject.SelectedValue.ToString(), tbSearch.Text.Trim());
            var msgs = from c in messages
                       select new
                       {
                           DeviceName = c.DeviceName,
                           UserName = c.Username,
                           MessageDate = c.DateOccurString,
                           Message = c.Message,
                           MethodName = c.MethodName,
                           Parameters = c.Parameters
                       };
            dgMessages.DataSource = msgs.ToList();
            dgMessages.Columns[2].Width = 125;
            dgMessages.Columns[3].Width = 200;
            dgMessages.Columns[4].Width = 120;
            dgMessages.Columns[5].Width = 350;
            lbMessages.Text = "Count: "+messages.Count.ToString();

            var errors = db.GetErrorMessages(dtpException.Value, cmbProject.SelectedValue.ToString(),tbSearch.Text.Trim());
            var ers = from c in errors
                       select new
                       {
                           DeviceName = c.DeviceName,
                           UserName = c.Username,
                           MessageDate = c.DateOccurString,                           
                           MethodName = c.MethodName,
                           Parameters = c.Parameters,
                           Message = c.Message,
                           StackTrace = c.StackTrace
                       };
            dgExceptions.DataSource = ers.ToList();
            dgExceptions.Columns[2].Width = 125;
            dgExceptions.Columns[4].Width = 180;
            dgExceptions.Columns[5].Width = 200;
            dgExceptions.Columns[6].Width = 350;
            lbExceptions.Text = "Count: " + errors.Count.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataLoad();

        }

        ArrayList packingsDuplicate = new ArrayList();
        private void btnSearchDelivery_Click(object sender, EventArgs e)
        {
            DBClass db = new DBClass(DBClass.DbName.DynamicsLive);
            
            ArrayList pickings = new ArrayList();
            ArrayList packings = new ArrayList();
            

            string strPackingSlips = cmbPackingSlips.Text;
            DataTable dt = db.GetFgDeliveries(dtpDelivery.Value, tbSearchDelivery.Text.Trim(), strPackingSlips,chkManual.CheckState);
            
            dgFGDelivery.DataSource = dt;
            lbCountDeliveries.Text = "Pallet Count: " + dt.Rows.Count.ToString();
            
            foreach (DataRow dr in dt.Rows)
            {
                string pickingId = dr["PickingIdNumSeq"].ToString();
                if (!pickings.Contains(pickingId) && !string.IsNullOrEmpty(pickingId))
                {
                    pickings.Add(pickingId);
                }

                string packingId = dr["PackingSlipId"].ToString();
                if (!packings.Contains(packingId))
                {
                    packings.Add(packingId);
                }

            }

            lbCountPick.Text = "Picking Count: " + pickings.Count.ToString();
            lbPacking.Text = "Delivery Count: " + packings.Count.ToString();
            packings.Sort();

            packings.Insert(0, "---All Deliveries---");
            if ( strPackingSlips.Equals("---All Deliveries---"))
                packingsDuplicate = packings;
            cmbPackingSlips.DataSource = packingsDuplicate;
            if (!string.IsNullOrEmpty(strPackingSlips))
                cmbPackingSlips.SelectedItem = strPackingSlips;
        }

        private void btnSearchTimeDiff_Click(object sender, EventArgs e)
        {
            DataTable dtTimeDiff = new DBClass(DBClass.DbName.DynamicsLive).GetTimeDifferences(dtpTimeDiff.Value);
            lbTimeDiff.Text = "Total Picking List: "+dtTimeDiff.Rows.Count.ToString();
            dgTimeDiff.DataSource = dtTimeDiff;
            dgTimeDiff.Columns[5].Width = dgTimeDiff.Columns[6].Width = dgTimeDiff.Columns[7].Width = 120;
               
            
        }

        private void dgTimeDiff_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewLinkColumn && e.RowIndex >= 0)
            {
                string cellText = senderGrid.CurrentCell.Value.ToString();

                if (!string.IsNullOrEmpty(cellText))
                {
                    string salesId = dgTimeDiff.Rows[e.RowIndex].Cells[1].Value.ToString();
                    string driverName = dgTimeDiff.Rows[e.RowIndex].Cells[2].Value.ToString();
                    string truckPlate = dgTimeDiff.Rows[e.RowIndex].Cells[3].Value.ToString();

                    PalletLog log = new PalletLog();
                    log.PickingListId = cellText;
                    log.SalesId = salesId;
                    log.DriverName = driverName;
                    log.TruckPlate = truckPlate;
                    log.ShowDialog();
                }
            }
        }
    }

    public class ItemList
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }
}
