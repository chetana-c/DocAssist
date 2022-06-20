using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1.DAL
{
    class DAL
    {
        public static string GetConnectionString()
        {
            
            String ConnectionString = null;
            try
            {
                ConnectionString = "Data Source=DBSEP5560;Initial Catalog=OCU_DATA_LAB;Integrated Security=SSPI;";
            }
            catch (Exception ex)
            {
                ConnectionString = null;
                MessageBox.Show("An error occured while creating the connection string for fetching data. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return ConnectionString;
        }

        public static string ExecuteCommand(String Query)
        {
            String str;
            String ConnectionString = GetConnectionString();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlCommand com = new SqlCommand(Query, connection);
                    str = (String)com.ExecuteScalar();
                    com.Dispose();
                    connection.Close();
                    ConnectionString = null;
                    return str;
                }

            }
            catch (Exception ex)
            {
                ConnectionString = null;
                ConnectionString = null;
                Application.UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
                //MessageBox.Show("An error occured while performing the required operation. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ex.Message;

            }
        }

        public static DataSet GetData(String Query)
        {
            String ConnectionString = GetConnectionString();
            DataSet Result = new DataSet();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(Query, connection))
                    {
                        adapter.SelectCommand.CommandTimeout = 360;
                        adapter.Fill(Result);
                    }
                    connection.Close();
                }
                ConnectionString = null;
            }
            catch (Exception ex)
            {
                ConnectionString = null;
                ConnectionString = null;
                Application.UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
                MessageBox.Show("An error occured while fetching data. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return Result;
        }

        public static void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
