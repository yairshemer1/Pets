using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
//using Oracle.DataAccess.Client;

namespace Pets
{
    public class DAL
    {
        private static SqlConnection sqlconnection = new SqlConnection(Global.strConnectionString);
        public static string conststrTableSuffix;

        public static string Sanitize(string str)
        {
            return str.Replace("'", "' + CHAR(39) + '").Replace("\"", "");
        }

        public static int UpdateOrDelete(string strSql)
        {
            //This method can throw Exceptions
            if (strSql.Contains("DELETE "))
            {
                if (false == strSql.Contains(" WHERE "))
                {
                    throw new Exception("A DELETE statement without a 'WHERE'. Unacceptable.");
                }
            }
            if (strSql.Contains("UPDATE "))
            {
                if (false == strSql.Contains(" WHERE "))
                {
                    throw new Exception("An UPDATE statement without a 'WHERE'. Unacceptable.");
                }
            }
            int intRowsAffected = -1;
            //  string strConnectionString;
            //   strConnectionString = demo9.asr.GetValue("mama9", "".GetType()).ToString();
            //  using (SqlConnection sqlconnection = new SqlConnection(strConnectionString))
            //  {
            using (SqlCommand sqlcommand = new SqlCommand(strSql, sqlconnection))
            {
                sqlconnection.Open();
                try
                {
                    intRowsAffected = sqlcommand.ExecuteNonQuery();
                    sqlconnection.Close();
                }
                catch (Exception ex)
                {
                    try
                    {
                        sqlconnection.Close();
                    }
                    catch (Exception ex2)
                    {
                        //
                    }
                    sqlcommand.Dispose();
                    throw ex;
                }
            }
            //    sqlconnection.Dispose();
            //  }
            return intRowsAffected;
        }

        public static DataTable Select(string strSql)
        {
            // string strLocalDB;
            // strLocalDB = asr.GetValue("mama9", "".GetType()).ToString().Trim();
            //     bool boolResult;
            string strTableName = "data";
            if (strSql.StartsWith("SELECT * FROM [tbl") && strSql.Contains("]"))
            {
                strTableName = ((strSql.Substring(0, strSql.IndexOf(']')))).Replace("SELECT * FROM [tbl", "");
            }
            using (SqlDataAdapter sqldataadapter = new SqlDataAdapter(strSql, sqlconnection))
            {
                using (DataTable datatable = new DataTable(strTableName))
                {
                    try
                    {
                        sqldataadapter.Fill(datatable);
                        // sqlconnectionLocal.Dispose();
                        sqldataadapter.Dispose();
                        return datatable;
                        //if (int.Parse(datatable.Rows[0][0].ToString()) > 0)
                        //{
                        //    //    boolResult = true;
                        //}
                        //else
                        //{
                        //    //  boolResult = false;
                        //}
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            //    sqlconnectionLocal.Close();
                            //    sqlconnectionLocal.Dispose();
                        }
                        catch (Exception ex2)
                        {
                            try
                            {
                                //      sqlconnectionLocal.Dispose();
                            }
                            catch (Exception ex3)
                            {
                                //
                            }
                        }
                        sqldataadapter.Dispose();
                        datatable.Dispose();
                        throw ex;
                    }
                }
            }
        }

        public static DataTable SelectWithForeignKeys(string strSql)
        {
            // string strLocalDB;
            // strLocalDB = asr.GetValue("mama9", "".GetType()).ToString().Trim();
            //     bool boolResult;
            string strTableName = "data";
            if (strSql.StartsWith("SELECT * FROM [tbl") && strSql.Contains("]"))
            {
                strTableName = ((strSql.Substring(0, strSql.IndexOf(']')))).Replace("SELECT * FROM [tbl", "");
            }
            string strSecondQuerySql = "";
            using (SqlDataAdapter sqldataadapter = new SqlDataAdapter(strSql, sqlconnection))
            {
                using (DataTable datatable = new DataTable(strTableName))
                {
                    Fill(sqldataadapter, datatable);
                    // sqlconnectionLocal.Dispose();
                    bool boolHasForeignKeys = false;
                    strSecondQuerySql = "SELECT ";
                    foreach (DataColumn datacolumn in datatable.Columns)
                    {
                        if (datacolumn.ColumnName.EndsWith("_"))
                        {
                            boolHasForeignKeys = true;

                            strSecondQuerySql += "(SELECT [description] FROM [tbl" + Sanitize(datacolumn.ColumnName.Replace("_", ""));
                            if (false==datacolumn.ColumnName.EndsWith("s_"))
                            {
                                strSecondQuerySql += "s";
                            }
                            strSecondQuerySql += "] WHERE [id]=[" + Sanitize(datacolumn.ColumnName);
                            strSecondQuerySql+="]) AS '" +Sanitize(datacolumn.ColumnName.Replace("_",""))+"', ";
                        }
                        else
                        {
                            strSecondQuerySql += "[" + Sanitize(datacolumn.ColumnName) + "], ";
                        }
                    }
                    if (false == boolHasForeignKeys || datatable.Columns.Count == 0)
                    {
                        sqldataadapter.Dispose();
                        return datatable;
                        ///////////////////////////////////////////////////////////////////////
                    }
                    strSecondQuerySql = strSecondQuerySql.Substring(0, strSecondQuerySql.LastIndexOf(','));
                    strSecondQuerySql += " FROM [tbl"+Sanitize(strTableName)+"];";
                    datatable.Dispose();
                }
                sqldataadapter.Dispose();
            }
            using (SqlDataAdapter sqldataadapter = new SqlDataAdapter(strSecondQuerySql, sqlconnection))
            {
                using (DataTable datatable = new DataTable(strTableName))
                {
                    sqldataadapter.Fill(datatable);
                    sqldataadapter.Dispose();
                    return datatable;
                }
            }
        }

        private static void Fill(SqlDataAdapter sqldataadapter, DataTable datatable)
        {
            try
            {
                sqldataadapter.Fill(datatable);
            }
            catch (Exception ex)
            {
                datatable.Dispose();
                sqldataadapter.Dispose();
                throw ex;
            }
        }

        public static int Insert(string strSql)
        {
            //This method can throw Exceptions
            int intRowsInserted = -1;
            // string strConnectionString;
            //strConnectionString = demo9.asr.GetValue("mama9", "".GetType()).ToString();
            //using (SqlConnection sqlconnection = new SqlConnection(strConnectionString))
            //{
            using (SqlCommand sqlcommand = new SqlCommand(strSql, sqlconnection))
            {
                sqlconnection.Open();
                try
                {
                    intRowsInserted = sqlcommand.ExecuteNonQuery();
                    sqlconnection.Close();
                }
                catch (Exception ex)
                {
                    try
                    {
                        sqlconnection.Close();
                    }
                    catch (Exception ex2)
                    {
                        //
                    }
                    sqlcommand.Dispose();
                    throw ex;
                }
                //}
                //  sqlconnection.Dispose();
            }
            return intRowsInserted;
        }

        public static int InsertAndGetNewId(string strSql, bool ItPutsTheOutPutClauseItself = true)
        {
            //This method can throw Exceptions
            if (false == strSql.Contains("VALUES"))
            {
                throw new Exception();
            }
            if (strSql.IndexOf("VALUES") != strSql.LastIndexOf("VALUES"))
            {
                throw new Exception();
            }
            strSql = strSql.Replace("VALUES", "OUTPUT INSERTED.ID VALUES");
            //This method can throw Exceptions
            // int intRowsInserted = -1;
            // string strConnectionString;
            //strConnectionString = demo9.asr.GetValue("mama9", "".GetType()).ToString();
            //using (SqlConnection sqlconnection = new SqlConnection(strConnectionString))
            //{
            //using (SqlCommand sqlcommand = new SqlCommand(strSql, Global.sqlconnection))
            using (DataTable dt = Select(strSql))
            {
                string str = (dt.Rows[0][0]).ToString();
                dt.Dispose();
                return int.Parse(str);
            }
            //}
            //  sqlconnection.Dispose();

        }
    }
}