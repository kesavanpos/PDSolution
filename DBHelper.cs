using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Pricing
{
    public static class DBHelper
    {
        static string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public static ObservableCollection<T> GetSPResult<T>(string spName, Dictionary<string, object> paramCollection)
        {
            var result = new ObservableCollection<T>();
            var dt = new DataTable();
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        if (paramCollection != null)
                        {
                            var paramList = paramCollection
                            .Select(m => new SqlParameter { ParameterName = m.Key, Value = m.Value }).ToList();
                            sqlCommand.Parameters.AddRange(paramList.ToArray());
                        }

                        using (var dataAdapter = new SqlDataAdapter(sqlCommand))
                        {
                            dataAdapter.Fill(dt);
                        }

                        result = DataTableMapToList<T>(dt);
                    }
                }
            }
            catch (Exception ex)
            {   
            }

            return result;
        }

        public static T ExecuteScalarSPResult<T>(string spName, Dictionary<string, object> paramCollection)
        {
            try
            {
                T result;
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        if (paramCollection != null)
                        {
                            var paramList = paramCollection
                            .Select(m => new SqlParameter { ParameterName = m.Key, Value = m.Value }).ToList();
                            sqlCommand.Parameters.AddRange(paramList.ToArray());
                        }

                        sqlConnection.Open();
                        var retVal = sqlCommand.ExecuteScalar();
                        if (retVal != null)
                            result = (T)retVal;
                        else
                            result = default(T);
                        sqlConnection.Close();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {  
                return default(T);
            }
        }

        public static int ExecuteInsertSP(string spName, Dictionary<string, object> paramCollection)
        {
            int result = 0;
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        if (paramCollection != null)
                        {
                            var paramList = paramCollection
                            .Select(m => new SqlParameter { ParameterName = m.Key, Value = m.Value ?? DBNull.Value }).ToList();
                            sqlCommand.Parameters.AddRange(paramList.ToArray());
                        }

                        sqlConnection.Open();
                        result = sqlCommand.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {   
            }

            return result;
        }

        public static bool ExecuteBulkInsertSP(string spName, DataTable inputTable, string inputParam, Dictionary<string, object> paramCollection)
        {
            bool result = false;
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandTimeout = 300;
                        var headersParam = sqlCommand.Parameters.AddWithValue(string.Format("@{0}", inputParam), inputTable);
                        headersParam.SqlDbType = SqlDbType.Structured;
                        var outputParam = new SqlParameter("@OutputStatus", SqlDbType.VarChar, 10);
                        outputParam.Direction = ParameterDirection.Output;

                        if (paramCollection != null)
                        {
                            var paramList = paramCollection
                            .Select(m => new SqlParameter { ParameterName = m.Key, Value = m.Value }).ToList();
                            sqlCommand.Parameters.AddRange(paramList.ToArray());
                        }
                        sqlCommand.Parameters.Add(outputParam);

                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                        result = Convert.ToBoolean(sqlCommand.Parameters["@OutputStatus"].Value);
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public static int ExecuteInsertStructuredSP(string spName, Dictionary<string, object> paramCollection)
        {
            int result = 0;
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        if (paramCollection != null)
                        {
                            foreach (var parm in paramCollection)
                            {
                                var sqlParameter = sqlCommand.Parameters.Add(parm.Key, SqlDbType.Structured);
                                sqlParameter.Value = parm.Value;
                            }
                        }

                        sqlConnection.Open();
                        result = sqlCommand.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public static T ExecuteInsertSPAndReturnOutParam<T>(string spName, Dictionary<string, object> paramCollection, SqlParameter outParameter)
        {
            T result;

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        if (paramCollection != null)
                        {
                            var paramList = paramCollection
                                .Select(m => new SqlParameter { ParameterName = m.Key, Value = m.Value ?? DBNull.Value }).ToList();
                            sqlCommand.Parameters.AddRange(paramList.ToArray());
                            sqlCommand.Parameters.Add(outParameter);
                        }

                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (outParameter.Value is DBNull)
            {
                result = default(T);
            }
            else
            {
                result = (T)outParameter.Value;
            }
            return result;
        }

        public static T ExecuteInsertSPAndReturnOutParamString<T>(string spName, Dictionary<string, object> paramCollection, string outParamName)
        {
            T result;
            var outParameter = new SqlParameter();
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        outParameter = new SqlParameter(outParamName, SqlDbType.NVarChar);
                        if (paramCollection != null)
                        {
                            var paramList = paramCollection
                            .Select(m => new SqlParameter { ParameterName = m.Key, Value = m.Value }).ToList();
                            sqlCommand.Parameters.AddRange(paramList.ToArray());
                            outParameter.Direction = ParameterDirection.Output;
                            outParameter.Size = 50;
                            sqlCommand.Parameters.Add(outParameter);
                        }

                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (outParameter.Value != null)
                result = (T)outParameter.Value;
            else
                result = default(T);

            return result;
        }


        public static List<string> ExecuteInsertSPAndReturnOutParam(string spName, Dictionary<string, object> paramCollection, string outParamName, string outParamName1)
        {
            var outParameterToken = new SqlParameter();
            var outParameterExpiretime = new SqlParameter();
            var outputs = new List<string>();
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        outParameterToken = new SqlParameter(outParamName, SqlDbType.NVarChar, 450);
                        outParameterExpiretime = new SqlParameter(outParamName1, SqlDbType.NVarChar, 40);
                        if (paramCollection != null)
                        {
                            var paramList = paramCollection
                            .Select(m => new SqlParameter { ParameterName = m.Key, Value = m.Value }).ToList();
                            sqlCommand.Parameters.AddRange(paramList.ToArray());
                            outParameterToken.Direction = ParameterDirection.Output;
                            sqlCommand.Parameters.Add(outParameterToken);
                            outParameterExpiretime.Direction = ParameterDirection.Output;
                            sqlCommand.Parameters.Add(outParameterExpiretime);
                        }

                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {   
            }
            outputs.Add(outParameterToken.Value.ToString());
            outputs.Add(outParameterExpiretime.Value.ToString());
            return outputs;
        }

        public static List<string> ExecuteInsertSPAndReturnOutParams(string spName, Dictionary<string, object> paramCollection, string outParamName, string outParamName1, string outParamName2, string outParamName3)
        {
            var outParameterOrderId = new SqlParameter();
            var outParameterOpenTime = new SqlParameter();
            var outParameterZoneTime = new SqlParameter();
            var outParameterSiteName = new SqlParameter();
            var outputs = new List<string>();
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        outParameterOrderId = new SqlParameter(outParamName, SqlDbType.BigInt);
                        outParameterOpenTime = new SqlParameter(outParamName1, SqlDbType.DateTime);
                        outParameterZoneTime = new SqlParameter(outParamName2, SqlDbType.VarChar, 100);
                        outParameterSiteName = new SqlParameter(outParamName3, SqlDbType.VarChar, 100);
                        if (paramCollection != null)
                        {
                            var paramList = paramCollection
                                .Select(m => new SqlParameter { ParameterName = m.Key, Value = m.Value ?? DBNull.Value }).ToList();
                            sqlCommand.Parameters.AddRange(paramList.ToArray());
                            outParameterOrderId.Direction = ParameterDirection.Output;
                            sqlCommand.Parameters.Add(outParameterOrderId);
                            outParameterOpenTime.Direction = ParameterDirection.Output;
                            sqlCommand.Parameters.Add(outParameterOpenTime);
                            outParameterZoneTime.Direction = ParameterDirection.Output;
                            sqlCommand.Parameters.Add(outParameterZoneTime);
                            outParameterSiteName.Direction = ParameterDirection.Output;
                            sqlCommand.Parameters.Add(outParameterSiteName);
                        }

                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {   
            }
            outputs.Add(outParameterOrderId.Value.ToString());
            outputs.Add(outParameterOpenTime.Value.ToString());
            outputs.Add(outParameterZoneTime.Value.ToString());
            outputs.Add(outParameterSiteName.Value.ToString());
            return outputs;
        }

        public static DataTable GetSPDataTableResult(string spName, Dictionary<string, object> paramCollection)
        {
            var dt = new DataTable();
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(spName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        if (paramCollection != null)
                        {
                            var paramList = paramCollection
                            .Select(m => new SqlParameter { ParameterName = m.Key, Value = m.Value }).ToList();
                            sqlCommand.Parameters.AddRange(paramList.ToArray());
                        }

                        using (var dataAdapter = new SqlDataAdapter(sqlCommand))
                        {
                            dataAdapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {   
            }
            return dt;
        }

        public static List<T> DataReaderMapToList<T>(IDataReader dr)
        {
            var list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                foreach (var prop in obj.GetType().GetProperties())
                {
                    if (!Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, dr[prop.Name], null);
                    }
                }

                list.Add(obj);
            }

            return list;
        }

        public static ObservableCollection<T> DataTableMapToList<T>(DataTable dt)
        {
            var list = new ObservableCollection<T>();
            T obj = default(T);
            foreach (DataRow rows in dt.Rows)
            {
                if (dt.Rows.Count != 0)
                {
                    obj = Activator.CreateInstance<T>();
                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        if (dt.Columns.Contains(prop.Name))
                        {
                            if (!object.Equals(rows[prop.Name], DBNull.Value))
                            {
                                prop.SetValue(obj, rows[prop.Name], null);
                            }
                        }
                    }

                    list.Add(obj);
                }
            }

            dt.Clear();
            return list;
        }
    }
}