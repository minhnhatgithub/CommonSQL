using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Register_Facebook_V2
{
    public class CommonSQL
    {
		public static bool CheckExitsFile(string name)
		{
			return Connector.Instance.ExecuteScalar("SELECT COUNT(*) FROM files WHERE name='" + name + "' AND active=1;") > 0;
		}

		public static DataTable GetAllFilesFromDatabase(bool isShowAll = false)
		{
			DataTable result = new DataTable();
			try
			{
				string text = "";
				text = (isShowAll ? ("select id, name from files where active=1 UNION SELECT -1 AS id, '" + Language.GetValue("[Tất cả thư mục]") + "' AS name UNION SELECT 999999 AS id, '" + Language.GetValue("[Chọn nhiều thư mục]") + "' AS name ORDER BY id ASC") : "select id, name from files where active=1");
				result = Connector.Instance.ExecuteQuery(text);
			}
			catch
			{
			}
			return result;
		}
		public static DataTable GetAllFilesFromDatabaseForBin(bool isShowAll = false)
		{
			DataTable result = new DataTable();
			try
			{
				string text = "";
				text = (isShowAll ? ("select id, name from files WHERE id IN (SELECT DISTINCT idfile FROM accounts WHERE active=0) UNION SELECT -1 AS id, '" + Language.GetValue("[Tất cả thư mục]") + "' AS name UNION SELECT 999999 AS id, '" + Language.GetValue("[Chọn nhiều thư mục]") + "' AS name ORDER BY id ASC") : "select id, name from files WHERE id IN (SELECT DISTINCT idfile FROM accounts WHERE active=0)");
				result = Connector.Instance.ExecuteQuery(text);
			}
			catch
			{
			}
			return result;
		}

		public static bool InsertFileToDatabase(string namefile)
		{
			bool result = true;
			try
			{
				string query = "insert into files values(null,'" + namefile + "','" + DateTime.Now.ToString() + "',1)";
				Connector.Instance.ExecuteQuery(query);
				return result;
			}
			catch
			{
				return false;
			}
		}

		public static bool UpdateFileNameToDatabase(string idFile, string nameFile)
		{
			try
			{
				string query = "UPDATE files SET name='" + nameFile + "' where id=" + idFile;
				return Connector.Instance.ExecuteNonQuery(query) > 0;
			}
			catch
			{
			}
			return false;
		}

		public static bool DeleteFileToDatabase(string idFile)
		{
			bool result = false;
			try
			{
				if (Connector.Instance.ExecuteScalar("SELECT COUNT(idfile) FROM accounts WHERE idfile=" + idFile) == 0)
				{
					result = Connector.Instance.ExecuteNonQuery("delete from files where id=" + idFile) > 0;
					return result;
				}
				if (Connector.Instance.ExecuteNonQuery("UPDATE files SET active=0 where id=" + idFile) > 0)
				{
					result = DeleteAccountByIdFile(idFile);
					return result;
				}
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static bool UpdateMultiField(string field, List<string> lstId_FieldValue, string table = "accounts")
		{
			List<string> list = new List<string>();
			string text = "";
			string text2 = "";
			string text3 = "";
			for (int i = 0; i < lstId_FieldValue.Count; i++)
			{
				text = lstId_FieldValue[i].Split('|')[0];
				text2 = lstId_FieldValue[i].Split('|')[1];
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
					text3 = text3 + "WHEN '" + text + "' THEN '" + text2 + "' ";
				}
			}
			string query = "UPDATE " + table + " SET " + field + " = CASE id " + text3 + "END WHERE id IN('" + string.Join("','", list) + "'); ";
			return Connector.Instance.ExecuteNonQuery(query) > 0;
		}

		public static bool DeleteFileToDatabaseIfEmptyAccount()
		{
			bool result = false;
			try
			{
				result = Connector.Instance.ExecuteNonQuery("delete from files where id NOT IN (SELECT DISTINCT idfile FROM accounts)") > 0;
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static DataTable GetAllInfoFromAccount(List<string> lstIdFile, bool isGetActive = true)
		{
			DataTable result = new DataTable();
			try
			{
				string text = "";
				text = ((lstIdFile != null && lstIdFile.Count != 0) ? ("where idfile IN (" + string.Join(",", lstIdFile) + ") AND active=" + (isGetActive ? 1 : 0)) : ("where active=" + (isGetActive ? 1 : 0)));
				string query = "SELECT '-1' as id, '" + Language.GetValue("[Tất cả tình trạng]") + "' AS name UNION select DISTINCT '0' as id,info from accounts " + text + " ORDER BY id ASC";
				result = Connector.Instance.ExecuteQuery(query);
			}
			catch
			{
			}
			return result;
		}

		public static bool InsertAccountToDatabase(string uid, string pass, string token, string cookie, string email, string phone, string name, string friends, string groups, string birthday, string gender, string info, string backup, string fa2, string idFile, string emaiRecovery = "", string passMail = "", string useragent = "", string proxy = "")
		{
			bool result = true;
			try
			{
				string format = "INSERT INTO accounts(uid, pass,token,cookie1,email,name,friends,groups,birthday,gender,info,fa2,backup,idfile,passmail,useragent,proxy,dateImport,active) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}',1)";
				format = string.Format(format, uid, pass.Replace("'", "''"), token, cookie, email, name, friends, groups, birthday, gender, info, fa2, backup, idFile, passMail, useragent, proxy, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
				Connector.Instance.ExecuteQuery(format);
				return result;
			}
			catch
			{
				return false;
			}
		}

		public static List<string> ConvertToSqlInsertAccount(List<string> lstSqlStatement)
		{
			List<string> list = new List<string>();
			try
			{
				int num = 100;
				int num2 = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((double)lstSqlStatement.Count * 1.0 / 100.0)));
				string text = "";
				for (int i = 0; i < num2; i++)
				{
					text = "INSERT INTO accounts(uid, pass,token,cookie1,email,name,friends,groups,birthday,gender,info,fa2,idfile,passmail,useragent,proxy,dateImport,active) VALUES " + string.Join(",", lstSqlStatement.GetRange(num * i, (num * i + num <= lstSqlStatement.Count) ? num : (lstSqlStatement.Count % num)));
					list.Add(text);
				}
			}
			catch
			{
			}
			return list;
		}

		public static string ConvertToSqlInsertAccount(string uid, string pass, string token, string cookie, string email, string name, string friends, string groups, string birthday, string gender, string info, string fa2, string idFile, string passMail, string useragent, string proxy)
		{
			string text = "";
			try
			{
				text = "('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}',1)";
				text = string.Format(text, uid, pass.Replace("'", "''"), token, cookie, email, name.Replace("'", "''"), friends, groups, birthday, gender, info, fa2, idFile, passMail, useragent, proxy, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
			}
			catch
			{
			}
			return text;
		}

		public static bool UpdateFieldToAccount(string id, string fieldName, string fieldValue)
		{
			bool result = false;
			try
			{
				string text = "";
				if (fieldName == "pass")
				{
					text = ", pass_old=pass";
				}
				string query = "update accounts set " + fieldName + " = '" + fieldValue.Replace("'", "''") + "'" + text + " where id=" + id;
				if (Connector.Instance.ExecuteNonQuery(query) > 0)
				{
					result = true;
					return result;
				}
				result = false;
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static bool UpdateMultiFieldToAccount(string id, string lstFieldName, string lstFieldValue, bool isAllowEmptyValue = true)
		{
			bool result = false;
			try
			{
				if (lstFieldName.Split('|').Length == lstFieldValue.Split('|').Length)
				{
					int num = lstFieldName.Split('|').Length;
					string text = "";
					for (int i = 0; i < num; i++)
					{
						if (isAllowEmptyValue || !(lstFieldValue.Split('|')[i].Trim() == ""))
						{
							text = text + lstFieldName.Split('|')[i] + "='" + lstFieldValue.Split('|')[i].Replace("'", "''") + "',";
						}
					}
					text = text.TrimEnd(',');
					string query = "update accounts set " + text + " where id=" + id;
					result = Connector.Instance.ExecuteNonQuery(query) > 0;
					return result;
				}
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static bool UpdateMultiFieldToAccount(List<string> lstId, string lstFieldName, string lstFieldValue)
		{
			bool result = false;
			try
			{
				if (lstFieldName.Split('|').Length == lstFieldValue.Split('|').Length)
				{
					int num = lstFieldName.Split('|').Length;
					string text = "";
					for (int i = 0; i < num; i++)
					{
						text = text + lstFieldName.Split('|')[i] + "='" + lstFieldValue.Split('|')[i].Replace("'", "''") + "',";
					}
					text = text.TrimEnd(',');
					int num2 = 100;
					int num3 = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((double)lstId.Count * 1.0 / 100.0)));
					List<string> list = new List<string>();
					string text2 = "";
					for (int j = 0; j < num3; j++)
					{
						text2 = "update accounts set " + text + " where id IN (" + string.Join(",", lstId.GetRange(num2 * j, (num2 * j + num2 <= lstId.Count) ? num2 : (lstId.Count % num2))) + ")";
						list.Add(text2);
					}
					if (Connector.Instance.ExecuteNonQuery(list) > 0)
					{
						result = true;
						return result;
					}
					result = false;
					return result;
				}
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static bool UpdateFieldToFile(string idFile, string fieldName, string fieldValue)
		{
			bool result = false;
			try
			{
				string query = "update files set " + fieldName + " = '" + fieldValue.Replace("'", "''") + "' where id=" + idFile;
				if (Connector.Instance.ExecuteNonQuery(query) > 0)
				{
					result = true;
					return result;
				}
				result = false;
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static bool UpdateFieldToAccount(List<string> lstId, string fieldName, string fieldValue)
		{
			bool result = false;
			try
			{
				int num = 100;
				int num2 = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((double)lstId.Count * 1.0 / 100.0)));
				List<string> list = new List<string>();
				string text = "";
				string text2 = "";
				if (fieldName == "pass")
				{
					text2 = ", pass_old=pass";
				}
				for (int i = 0; i < num2; i++)
				{
					text = "update accounts set " + fieldName + " = '" + fieldValue.Replace("'", "''") + "'" + text2 + " where id IN (" + string.Join(",", lstId.GetRange(num * i, (num * i + num <= lstId.Count) ? num : (lstId.Count % num))) + ")";
					list.Add(text);
				}
				if (Connector.Instance.ExecuteNonQuery(list) > 0)
				{
					result = true;
					return result;
				}
				result = false;
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static bool UpdateFieldToFile(List<string> lstId, string fieldName, string fieldValue)
		{
			bool result = true;
			try
			{
				string query = "update files set " + fieldName + " = '" + fieldValue + "' where id IN (" + string.Join(",", lstId) + ")";
				if (Connector.Instance.ExecuteNonQuery(query) > 0)
				{
					result = true;
					return result;
				}
				result = false;
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static DataTable GetAccFromFile(List<string> lstIdFile = null, string info = "", bool isGetActive = true)
		{
			DataTable result = new DataTable();
			try
			{
				string text = "WHERE ";
				string text2 = ((lstIdFile == null || lstIdFile.Count <= 0) ? "" : ("t1.idFile IN (" + string.Join(",", lstIdFile) + ")"));
				if (text2 != "")
				{
					text = text + text2 + " AND ";
				}
				string text3 = ((info != "") ? ("t1.info = '" + info + "'") : "");
				if (text3 != "")
				{
					text = text + text3 + " AND ";
				}
				string text4 = $"t1.active = '{(isGetActive ? 1 : 0)}'";
				text += text4;
				string query = "SELECT t1.*, t2.name AS nameFile FROM accounts t1 JOIN files t2 ON t1.idfile=t2.id " + text + " ORDER BY t1.idfile";
				result = Connector.Instance.ExecuteQuery(query);
			}
			catch
			{
			}
			return result;
		}

		public static DataTable GetAccFromUid(List<string> lstUid)
		{
			DataTable result = new DataTable();
			try
			{
				int num = 100;
				int num2 = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((double)lstUid.Count * 1.0 / 100.0)));
				List<string> list = new List<string>();
				string text = "";
				for (int i = 0; i < num2; i++)
				{
					text = "SELECT t1.*, t2.name AS nameFile FROM accounts t1 JOIN files t2 ON t1.idfile=t2.id WHERE t1.uid IN ('" + string.Join("','", lstUid.GetRange(num * i, (num * i + num <= lstUid.Count) ? num : (lstUid.Count % num))) + "') and t1.active=1 ORDER BY t1.uid";
					list.Add(text);
				}
				result = Connector.Instance.ExecuteQuery(list);
			}
			catch (Exception ex)
			{
				ExportError(ex, "GetAccFromFile");
			}
			return result;
		}

		public static DataTable GetAllAccountFromDatabase(bool isGetActive = true)
		{
			DataTable result = new DataTable();
			try
			{
				string query = $"select uid from accounts where active={(isGetActive ? 1 : 0)};";
				result = Connector.Instance.ExecuteQuery(query);
			}
			catch
			{
			}
			return result;
		}

		public static bool DeleteAccountByIdFile(string idFile)
		{
			bool result = true;
			try
			{
				if (Connector.Instance.ExecuteNonQuery("UPDATE accounts SET active=0, dateDelete='" + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + "' where idfile=" + idFile) > 0)
				{
					result = true;
					return result;
				}
				result = false;
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static bool DeleteAccountToDatabase(string id)
		{
			try
			{
				return Connector.Instance.ExecuteNonQuery("UPDATE accounts SET active=0, dateDelete='" + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + "' where id=" + id) > 0;
			}
			catch
			{
			}
			return false;
		}

		public static DataTable GetAccFromId(List<string> lstId)
		{
			DataTable result = new DataTable();
			try
			{
				int num = 100;
				int num2 = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((double)lstId.Count * 1.0 / 100.0)));
				List<string> list = new List<string>();
				string text = "";
				for (int i = 0; i < num2; i++)
				{
					text = "SELECT uid, pass, token, cookie1,email, passmail, fa2 FROM accounts WHERE id IN ('" + string.Join("','", lstId.GetRange(num * i, (num * i + num <= lstId.Count) ? num : (lstId.Count % num))) + "')";
					list.Add(text);
				}
				result = Connector.Instance.ExecuteQuery(list);
			}
			catch (Exception ex)
			{
				ExportError(ex, "GetAccFromFile");
			}
			return result;
		}
		public static void ExportError(Exception ex, string error = "")
		{
			try
			{
				StreamWriter streamWriter = new StreamWriter("log\\log.txt", append: true);
				streamWriter.WriteLine("-----------------------------------------------------------------------------");
				streamWriter.WriteLine("Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
				if (error != "")
				{
					streamWriter.WriteLine("Error: " + error);
				}
				streamWriter.WriteLine();
				if (ex != null)
				{
					streamWriter.WriteLine("Type: " + ex.GetType().FullName);
					streamWriter.WriteLine("Message: " + ex.Message);
					streamWriter.WriteLine("StackTrace: " + ex.StackTrace);
					ex = ex.InnerException;
				}
			}
			catch
			{
			}
		}
		public static bool DeleteAccountToDatabase(List<string> lstId, bool isReallyDelete = false)
		{
			if (isReallyDelete)
			{
				List<string> list = new List<string>();
				DataTable accFromId = GetAccFromId(lstId);
				for (int i = 0; i < accFromId.Rows.Count; i++)
				{
					string text = "";
					for (int j = 0; j < accFromId.Columns.Count; j++)
					{
						text = text + accFromId.Rows[i][j].ToString() + "|";
					}
					text = text.Substring(0, text.Length - 1);
					list.Add(text);
				}
				File.AppendAllText("bin.txt", "======" + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + "======\r\n");
				File.AppendAllLines("bin.txt", list);
			}
			bool result = true;
			try
			{
				int num = 100;
				int num2 = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((double)lstId.Count * 1.0 / 100.0)));
				List<string> list2 = new List<string>();
				string text2 = "";
				for (int k = 0; k < num2; k++)
				{
					text2 = ((!isReallyDelete) ? ("UPDATE accounts SET active=0, dateDelete='" + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + "' where id IN (" + string.Join(",", lstId.GetRange(num * k, (num * k + num <= lstId.Count) ? num : (lstId.Count % num))) + ")") : ("delete from accounts where id IN (" + string.Join(",", lstId.GetRange(num * k, (num * k + num <= lstId.Count) ? num : (lstId.Count % num))) + ")"));
					list2.Add(text2);
				}
				for (int l = 0; l < list2.Count; l++)
				{
					result = Connector.Instance.ExecuteNonQuery(list2[l]) > 0;
				}
				return result;
			}
			catch (Exception ex)
			{
			
				return result;
			}
		}

	}
}