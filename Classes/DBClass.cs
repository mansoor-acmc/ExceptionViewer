using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ExceptionViewer
{
    public class DBClass
    {
        SqlConnection conn = null;

        public enum DbName
        {
            DeviceMsg,
            DynamicsLive
        }

        public DBClass(DbName msgDB)
        {
            string connString = string.Empty;
            if (msgDB == DbName.DeviceMsg)
                connString = ConfigurationManager.ConnectionStrings["ConnErrorDB"].ConnectionString;
            else if (msgDB == DbName.DynamicsLive)
                connString = ConfigurationManager.ConnectionStrings["dynamicsConString"].ConnectionString;

            conn = new SqlConnection(connString);
        }

        public List<DeviceMessage> GetPingData(DateTime date, string project, string searchText)
        {
            List<DeviceMessage> result = new List<DeviceMessage>();
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            string sqlString = "SELECT * from DevicePing WHERE Convert(Date,PingDate)=@p1 AND ProjectName=@p2 ORDER BY PingDate DESC";
            
            try
            {                
                cmd.Parameters.Add("@p1", SqlDbType.DateTime);
                cmd.Parameters["@p1"].Value = date.Date;

                cmd.Parameters.Add("@p2", SqlDbType.NVarChar);
                cmd.Parameters["@p2"].Value = project;                

                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    result.Add(new DeviceMessage()
                    {
                        DeviceName = dr["DeviceName"].ToString(),
                        Username = dr["UserName"].ToString(),
                        DateOccurString = dr["PingDate"].ToString()
                    });

                } dr.Close();
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return result;
        }

        public List<DeviceMessage> GetDeviceMessages(DateTime date, string project, string searchText)
        {
            List<DeviceMessage> result = new List<DeviceMessage>();
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            string sqlString = "SELECT * from DeviceMessage WHERE Convert(Date,MsgDate)=@p1 AND ProjectName=@p2 ";
            if (!string.IsNullOrEmpty(searchText))
            {
                sqlString += "AND Parameter like @p3 ";
            }
            sqlString += "ORDER BY MsgDate DESC;";

            try
            {
                cmd.Parameters.Add("@p1", SqlDbType.DateTime);
                cmd.Parameters["@p1"].Value = date.Date;

                cmd.Parameters.Add("@p2", SqlDbType.NVarChar);
                cmd.Parameters["@p2"].Value = project;
                
                if (!string.IsNullOrEmpty(searchText))
                {
                    cmd.Parameters.Add("@p3", SqlDbType.NVarChar);
                    cmd.Parameters["@p3"].Value = "%" + searchText + "%";
                }

                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    result.Add(new DeviceMessage()
                    {
                        DeviceName = dr["DeviceName"].ToString(),
                        Username = dr["UserName"].ToString(),
                        DateOccurString = dr["MsgDate"].ToString(),
                        Message = dr["Message"].ToString(),
                        MethodName = dr["MethodName"].ToString(),
                        Parameters = dr["Parameter"].ToString()
                    });

                } dr.Close();
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return result;
        }

        public List<DeviceMessage> GetErrorMessages(DateTime date, string project, string searchText)
        {
            List<DeviceMessage> result = new List<DeviceMessage>();
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            string sqlString = "SELECT * from ErrorInfo WHERE Convert(Date,DateOfError)=@p1 AND ProjectName=@p2 ";
            if (!string.IsNullOrEmpty(searchText))
            {
                sqlString += "AND Parameters like @p3 ";
            }
            sqlString += "ORDER BY DateOfError DESC";

            try
            {
                cmd.Parameters.Add("@p1", SqlDbType.DateTime);
                cmd.Parameters["@p1"].Value = date.Date;

                cmd.Parameters.Add("@p2", SqlDbType.NVarChar);
                cmd.Parameters["@p2"].Value = project;

                if (!string.IsNullOrEmpty(searchText))
                {
                    cmd.Parameters.Add("@p3", SqlDbType.NVarChar);
                    cmd.Parameters["@p3"].Value = "%" + searchText + "%";
                }

                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    result.Add(new DeviceMessage()
                    {
                        DeviceName = dr["DeviceName"].ToString(),
                        Username = dr["UserName"].ToString(),
                        DateOccurString = dr["DateOfError"].ToString(),
                        Message = dr["ErrorString"].ToString(),
                        StackTrace = dr["FullTrace"].ToString(),//.Replace("\r\n", "\t"),
                        MethodName = dr["MethodName"].ToString(),
                        Parameters = dr["Parameters"].ToString()
                    });

                } dr.Close();
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return result;
        }

        public DataTable GetFgDeliveries(DateTime date, string searchText, string packingSlipId, CheckState stateManual)
        {
            DataTable result = new DataTable();
            
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            cmd.Parameters.Add("@p1", SqlDbType.DateTime);
            cmd.Parameters["@p1"].Value = date.Date;

            string sqlString = "select trans.PICKINGIDNUMSEQ as PickingListId, trans.PackingSlipId as PackingSlipNo, origin.REFERENCEID as SalesId, dim.INVENTSERIALID PalletNo, dim.CONFIGID Grade,dim.INVENTCOLORID Shade,dim.INVENTSIZEID Caliber,trans.VOUCHER,trans.* "
                + "from InventTransOrigin origin inner join InventTrans trans on trans.InventTransOrigin = origin.RecId "
                + "inner join InventDim dim on dim.InventDimId = trans.InventDimId "
                //+"inner join InventTransPosting post on post.VOUCHER = trans.VOUCHER AND post.TRANSDATE = trans.DATEPHYSICAL and post.INVENTTRANSORIGIN = trans.INVENTTRANSORIGIN " 
                + "where origin.REFERENCECATEGORY=0 and trans.DATEPHYSICAL = @p1 "
                //+"and post.INVENTTRANSPOSTINGTYPE=1 "
                ;
            if (!string.IsNullOrEmpty(searchText))
            {
                sqlString += "AND (trans.PICKINGIDNUMSEQ like @p2 OR dim.INVENTSERIALID like @p2) ";
                cmd.Parameters.Add("@p2", SqlDbType.NVarChar);
                cmd.Parameters["@p2"].Value = "%" + searchText + "%";
            }
            if (!string.IsNullOrEmpty(packingSlipId) && !packingSlipId.Equals("---All Deliveries---"))
            {
                sqlString += "AND trans.PackingSlipId=@p3 ";
                cmd.Parameters.Add("@p3", SqlDbType.NVarChar);
                cmd.Parameters["@p3"].Value = packingSlipId;
            }
            if (stateManual != CheckState.Indeterminate)
            {
                if (stateManual == CheckState.Checked)
                    sqlString += "AND trans.PICKINGIDNUMSEQ = '' ";
                else if(stateManual == CheckState.Unchecked)
                    sqlString += "AND trans.PICKINGIDNUMSEQ != '' ";
            }
            sqlString += "order by trans.PICKINGIDNUMSEQ desc, dim.INVENTSERIALID;";

            try
            {                
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                da.Fill(result);                
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return result;
        }

        public DataTable CreateTimeDiffTable()
        {
            DataTable dt = new DataTable("TimeDiff");

            dt.Columns.Add("PickingListId");
            dt.Columns.Add("SalesId");
            dt.Columns.Add("DriverName");
            dt.Columns.Add("TruckNo");
            dt.Columns.Add("MobileNo");
            dt.Columns.Add("PickingListScan",typeof(DateTime));
            dt.Columns.Add("FirstPalletScan", typeof(DateTime));
            dt.Columns.Add("LastPalletScan", typeof(DateTime));
            dt.Columns.Add("TimeDifference", typeof(TimeSpan));

            return dt;
        }
        public DataTable GetTimeDifferences(DateTime date)
        {
            DataTable result = CreateTimeDiffTable();
            string sqlString = "select PICKINGIDNUMSEQ, SalesId, TruckPlate,DriverName,MobileNum, FirstPickingScan from SalesLinePick where convert(date, CreatedDateTime)=@p1";
            
            SqlCommand cmdA = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter(cmdA);
            cmdA.Parameters.Add("@p1", SqlDbType.DateTime);
            cmdA.Parameters["@p1"].Value = date.Date;
            try
            {
                cmdA.Connection = conn;
                cmdA.CommandType = CommandType.Text;
                cmdA.CommandText = sqlString;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                DataTable dtLocal = new DataTable();
                da.Fill(dtLocal);

                foreach(DataRow drLocal in dtLocal.Rows)
                {
                    DataRow drow = result.NewRow();
                    drow[0] = drLocal["PICKINGIDNUMSEQ"].ToString();
                    drow[1] = drLocal["SalesId"].ToString();
                    drow[2] = drLocal["DriverName"].ToString();
                    drow[3] = drLocal["TruckPlate"].ToString();
                    drow[4] = drLocal["MobileNum"].ToString();
                    drow[5] = DateTime.Parse(drLocal["FirstPickingScan"].ToString()).AddHours(3);


                    sqlString = "select * from PICKINGHISTORY where PICKINGIDNUMSEQ=@p2 AND PalletStatus != 4 AND UpdatedDate>=@p3 ORDER BY UpdatedDate";
                    SqlCommand cmd2 = new SqlCommand();
                    cmd2.Connection = conn;
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = sqlString;
                    cmd2.Parameters.Add("@p2", SqlDbType.NVarChar);
                    cmd2.Parameters["@p2"].Value = drLocal["PICKINGIDNUMSEQ"].ToString();
                    cmd2.Parameters.Add("@p3", SqlDbType.DateTime);
                    cmd2.Parameters["@p3"].Value = drLocal["FirstPickingScan"];
                    int iCount = 0;

                    SqlDataReader sdr = cmd2.ExecuteReader();
                    while(sdr.Read())
                    //if (dtLocal.Rows.Count > 0)
                    {
                        if (iCount.Equals(0))
                            drow[6] = DateTime.Parse(sdr["UpdatedDate"].ToString()).AddHours(3);
                        drow[7] = DateTime.Parse(sdr["UpdatedDate"].ToString()).AddHours(3);
                        iCount++;
                    }
                    if(iCount>0)
                    drow[8] = (DateTime)drow[7] - (DateTime)drow[6];
                    sdr.Close();

                    result.Rows.Add(drow);
                }
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return result;
        }

        public DataTable GetPalletHistory(string pickingId)
        {
            DataTable dtLocal = new DataTable();
            string sqlString = "select DeviceName, CONVERT(VARCHAR(19),DATEADD(Hour,3, UpdatedDate),120) as UpdatedDate,UpdatedBy,InventSerialId," +
            "case PalletStatus when 0 then 'Search' when 1 then 'Remove' when 2 then 'Reserve' when 3 then 'UnReserve' when 4 then 'PickingListSearch' END as Status"+
            ",Remarks,RecId from PICKINGHISTORY where PICKINGIDNUMSEQ=@p1 ORDER BY UpdatedDate";
            SqlCommand cmdA = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter(cmdA);

            cmdA.Parameters.Add("@p1", SqlDbType.NVarChar);
            cmdA.Parameters["@p1"].Value = pickingId;

            try
            {
                cmdA.Connection = conn;
                cmdA.CommandType = CommandType.Text;
                cmdA.CommandText = sqlString;

                if (conn.State != ConnectionState.Open)
                    conn.Open();
                                
                da.Fill(dtLocal);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return dtLocal;
        }
        
    }
}
