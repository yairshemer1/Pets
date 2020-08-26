using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Policy;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

namespace Pets
{
     [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Pets : jla
    {
        //public string GetData(int value)
        //{
        //    return string.Format("You entered: {0}", value);
        //}

        public string GetToken(Stream stream)
        {
            try
            {
                HttpContext.Current.Request.Cookies.Remove("ap");
                HttpContext.Current.Request.Cookies.Remove("ap");
                HttpContext.Current.Response.Cookies.Remove("ap");
                HttpContext.Current.Response.Cookies.Remove("ap");
                //if (HttpContext.Current.Session.Count > 0)
                //{
                //    return;
                //}
                //This method will throw an Exception in case the credentials are invalid.
                Thread.Sleep(Global.random.Next(1, 500));
                Dictionary<string, string> dicInputFieldsLowerCase;
                //  string strJson;
                dicInputFieldsLowerCase = ReadInput(stream, true);//, out strJson);
                string strUserName = "";
                string strSubmittedPassword = "";
                // if (dicInputFieldsLowerCase.ContainsKey("username"))
                //  {
                //      strUserName = dicInputFieldsLowerCase["username"];
                //  }
                //  if (dicInputFieldsLowerCase.ContainsKey("password"))
                //  {
                //      strPassword = dicInputFieldsLowerCase["password"];
                //  }
                //    Credentials credentials = JsonConvert.DeserializeObject<Credentials>(strJson);
                strUserName = dicInputFieldsLowerCase["username"];
                strSubmittedPassword = dicInputFieldsLowerCase["password"];
                using (DataTable datatable = DAL.Select("SELECT TOP 1 [id], [role_], [postOfficeBox] FROM [tblPersons] WHERE [fax]='" + DAL.Sanitize(strUserName) + "';"))
                {
                    if (datatable.Rows.Count < 1)
                    {
                        datatable.Dispose();
                        throw new Exception();
                    }
                    //////////////////////////////////////////////////
                    if (false == DoesSubmittedPasswordMatchHash(strSubmittedPassword, datatable))
                    {
                        datatable.Dispose();
                        throw new Exception();
                    }
                    ///////////////////////////////////////////////////////////
                    string strUserId;
                    strUserId = datatable.Rows[0][0].ToString();
                    int intUserId;
                    string strUserRole;
                    strUserRole = datatable.Rows[0][1].ToString();
                    int intUserRole;
                    intUserId = int.Parse(strUserId);
                    intUserRole = int.Parse(strUserRole);
                    //HttpContext.Current.Session["usetId"] = intUserId;
                    //HttpContext.Current.Session["userRole"] = intUserRole;
                    ClearUserTokens(intUserId);
                    string strToken = CreateToken(intUserId);
                    // HttpContext.Current.Response.AddHeader("token", strToken);
                    datatable.Dispose();
                    //if (HttpContext.Current.Request.Cookies["ap"]==null||HttpContext.Current.Request.Cookies["ap"].Value== null|| HttpContext.Current.Request.Cookies["ap"].Value.Trim()=="")
                    HttpCookie httpCookie = new HttpCookie("ap", strToken);
                    httpCookie.Expires = DateTime.Now.AddDays(1);
                    httpCookie.HttpOnly = true;
                    HttpContext.Current.Response.Cookies.Add(httpCookie);
                    return "";//strToken;
                }
            }
            catch (Exception ex)
            {
                //HttpContext.Current.Session["usetId"] = null;
                //HttpContext.Current.Session["userRole"] = null;
                HttpContext.Current.Session.Clear(); //IMPORTANT
                HttpContext.Current.Session.Abandon();
                HttpContext.Current.Response.StatusCode = 403;
                HttpContext.Current.Response.End();
                return "";
            }
        }

        private static bool DoesSubmittedPasswordMatchHash(string strPassword, DataTable datatable)
        {
            Thread.Sleep(Global.random.Next(1, 500));
            byte[] hashBytes = Convert.FromBase64String((datatable.Rows[0][2]).ToString());
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(strPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static Dictionary<string, string> ReadInput(Stream stream, bool boolToLowerCase = false)
        {
            string strRaw;
            using (StreamReader streamreader = new StreamReader(stream))
            {
                strRaw = streamreader.ReadToEnd();
                streamreader.Close();
                streamreader.Dispose();
            }
            stream.Close();
            stream.Dispose();
            strRaw = HttpUtility.UrlDecode(strRaw, Encoding.UTF8);
            //strRaw = strRaw.Replace("wrapper", "");
            //strRaw = strRaw.Replace("=", "");
            Dictionary<string, string> dicInputFields = new Dictionary<string, string>();
            // object obj;
            //   obj=Newtonsoft.Json.JsonConvert.DeserializeObject(strRaw);
            string[] strarrFields;
            strarrFields = strRaw.Split(new char[] { ',', '&' });
            foreach (string strKeyAndValue in strarrFields)
            {
                if (strRaw.IndexOf('=') != -1 && strRaw.IndexOf('=') != strRaw.Length - 1)
                {
                    string strKey;
                    strKey = strKeyAndValue.Substring(0, strKeyAndValue.IndexOf('='));
                    strKey = strKey.Replace("{", "").Replace("}", "").Replace("\"", "").Trim();
                    string strValue;
                    strValue = strKeyAndValue.Substring(strKeyAndValue.IndexOf('=') + 1);
                    strValue = strValue.Replace("{", "").Replace("}", "").Replace("\"", "").Trim();
                    if (boolToLowerCase)
                    {
                        strKey = strKey.ToLower();
                        strValue = strValue.ToLower();
                    }
                    if (strKey.Trim() != "")
                    {
                        dicInputFields.Add(strKey, strValue);
                    }
                }
            }
            //  File.WriteAllText(@"C:\Users\Meir\desktop\json.json", JsonConvert.SerializeObject(dicInputFields), Encoding.UTF8);
            WebOperationContext.Current.OutgoingResponse.Headers.Add("ApiVersion", "v1.0");
            // strRaw = strRaw.Replace("&", "\",");
            //  strRaw = strRaw.Replace("=", ":\"");
            //  strRaw = '{' + strRaw + "\"}";
            //  str = strRaw;
            return dicInputFields;
        }

        //public string GetDat2a(int value)
        //{
        //    return "you entered " + value.ToString();
        //}

        public string InsertAnimal(Stream stream)
        {
            //string strJson; 
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                //, out strJson);//JsonSerializerSettings jsonserializersetting = new JsonSerializerSettings();//jsonserializersetting.Converters.Add(new JsonConvert1toTrue());// Animal animal=JsonConvert.DeserializeObject<Animal>(strJson,new JsonConvertTtoTrue());               
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting a new animal");
                }
                if ((dicInputFields["species_"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply a species_");
                }
                if ((dicInputFields["location_"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply a location_: where is the animal now");
                }
                if ((dicInputFields["size_"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply a size_");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Animal));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertPerson(Stream stream)
        {
            //string strJson; 
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting a new person");
                }
                if (false == dicInputFields.ContainsKey("firstName") || (dicInputFields["firstName"]).Trim() == "")
                {
                    if (false == dicInputFields.ContainsKey("lastName") || (dicInputFields["lastName"]).Trim() == "")
                    {
                        throw new Exception("Supply a name");
                    }
                }
                //This method can throw an Exception:
                if (IsNameTaken(dicInputFields))
                {
                    throw new Exception("Name already taken. Try something else");
                }
                if (dicInputFields.ContainsKey("role_"))
                {
                    int intRole_;
                    if (int.TryParse((dicInputFields["role_"]).ToString().Trim(), out intRole_))
                    {
                        if (intRole_ > 1)
                        {
                            if ((dicInputFields["postOfficeBox"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                            {
                                throw new Exception("Supply a post office box number");
                            }
                            if ((dicInputFields["fax"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                            {
                                throw new Exception("Supply a fax number");
                            }
                            //This method can throw an Exception:
                            if (IsUserNameTaken(dicInputFields["fax"]))
                            {
                                throw new Exception("Already taken. Try something else");
                            }
                        }
                    }
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Person));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        private bool IsNameTaken(Dictionary<string, string> dicInputFields)
        {
            //This method can throw an Exception
            string str;
            str = "SELECT [id] FROM [tblPersons] WHERE";
            str += " [firstName]='" + DAL.Sanitize((dicInputFields["firstName"]).Trim()) + "' AND [lastName]='" + DAL.Sanitize((dicInputFields["lastName"]).Trim()) + "';";
            using (DataTable datatable = DAL.Select(str))
            {
                if (datatable.Rows.Count > 0)
                {
                    datatable.Dispose();
                    return true;
                }
                else
                {
                    datatable.Dispose();
                    return false;
                }
            }
        }

        private bool IsUserNameTaken(string str)
        {
            //This method can throw an Exception
            string strSql;
            strSql = "SELECT [id] FROM [tblPersons] WHERE [fax]='" + DAL.Sanitize(str) + "';";
            using (DataTable datatable = DAL.Select(strSql))
            {
                if (datatable.Rows.Count > 0)
                {
                    datatable.Dispose();
                    return true;
                }
                datatable.Dispose();
            }
            return false;
        }

        public string InsertAdoption(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting a new adoption");
                }
                if ((dicInputFields["animal_"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply the animal_");
                }
                if ((dicInputFields["person_"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply the person_");
                }
                if ((dicInputFields["adoptedOn"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply the date");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Adoption));
                SetLocationTo2(dicInputFields);
                SetAvailableForAdoptionTo0(dicInputFields, typeof(Adoption));
                EndPossibleTemporaryAdoption(dicInputFields, typeof(Adoption));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        private void SetAvailableForAdoptionTo0(Dictionary<string, string> dicAnimalInputFields, Type type)
        {
            //This method can throw Exception
            try
            {
                string str;
                str = "UPDATE [tblAnimals] SET [availableForAdoption]='0' WHERE [id]=";
                if (type == typeof(Adoption))
                {
                    str += "'" + DAL.Sanitize((dicAnimalInputFields["animal_"]).Trim()) + "';";
                }
                else
                {
                    //animal
                    str += "'" + DAL.Sanitize(dicAnimalInputFields["id"]) + "';";
                }
                DAL.UpdateOrDelete(str);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in SetAvailableForAdoptionTo0() - adoption inserted", ex);
            }
        }

        private void SetLocationTo2(Dictionary<string, string> dicInputFields)
        {
            //This method can throw Exception
            try
            {
                string str;
                str = "UPADTE [tblAnimals] SET [location_]='2' WHERE id ='" + DAL.Sanitize(dicInputFields["animal_"]) + "';";
                DAL.UpdateOrDelete(str);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in SetLocationTo2() - adoption inserted", ex);
            }
        }

        public string InsertChip(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if ((dicInputFields["animal_"]).Trim() == "")
                {
                    throw new Exception("You must supply the animal_"); //This will throw an Exception in case this field was not supplied in the request
                }
                if ((dicInputFields["description"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply a description (usually the chip number)");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Chip));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertDoc(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if ((dicInputFields["nameAndPathIncludingExtension"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply a file name, including the path and the extension. This can also be a URL, including the extension");
                }
                if ((dicInputFields["animal_"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply the name of the animal_ that the document deals with");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Doc));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertDocumentType(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if ((dicInputFields["description"]).Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(DocumentType));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertLocation(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if ((dicInputFields["description"]).Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Location));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertPic(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if ((dicInputFields["animal_"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply the animal_");
                }
                if ((dicInputFields["nameAndPathIncludingExtension"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply the name of the file, including path and the extension. It can also be a URL, including the extension.");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Pic));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertSize(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if ((dicInputFields["description"]).Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Size));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertSpecies(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if ((dicInputFields["description"]).Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Species));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertTreatment(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if ((dicInputFields["animal_"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply the animal_");
                }
                if ((dicInputFields["treatmentTime"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply the time for the treatment");
                }
                if ((dicInputFields["person_"]).Trim() == "") //This will throw an Exception in case this field was not supplied in the request
                {
                    throw new Exception("You must supply the person_ who did the treatment");
                }
                if (false == dicInputFields.ContainsKey("treatmentType_") || (dicInputFields["treatmentType_"]).Trim() == "")
                {
                    if (false == dicInputFields.ContainsKey("medicineName") || (dicInputFields["medicineName"]).Trim() == "")
                    {
                        if (false == dicInputFields.ContainsKey("remarks") || (dicInputFields["remarks"]).Trim() == "")
                        {
                            throw new Exception("You must supply either the treatmentType_ or remarks or medicine name");
                        }
                    }
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(Treatment));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertTreatmentType(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                if (dicInputFields.ContainsKey("id"))
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if ((dicInputFields["description"]).Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                //This method can throw an Exception:
                PhraseSqlAndInsert(dicInputFields, typeof(TreatmentType));
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        private static void PhraseSqlAndInsert(Dictionary<string, string> dicInputFields, Type type)
        {
            //This method can throw an Exception
            FieldInfo[] fieldinfoarr = type.GetFields();
            Dictionary<string, Type> dicPropertyTypeByName = new Dictionary<string, Type>();
            foreach (FieldInfo fieldinfo in fieldinfoarr)
            {
                dicPropertyTypeByName.Add(fieldinfo.Name, fieldinfo.FieldType);
            }
            string strFieldNames;
            string strValues;
            strFieldNames = "INSERT INTO [tbl" + DAL.Sanitize(type.Name) + "s] ( ";
            strValues = "    ) VALUES ( ";
            foreach (string strKey in dicInputFields.Keys)
            {
                if (false == dicPropertyTypeByName.ContainsKey(strKey))
                {
                    throw new Exception();
                }
                if (dicPropertyTypeByName[strKey] == typeof(DateTime) || dicPropertyTypeByName[strKey] == typeof(DateTime?))
                {
                    //This method can throw Exceptions:
                    dicInputFields[strKey] = ReformatDate(dicInputFields[strKey]);
                }
                if ((dicInputFields[strKey]).Trim() != "")
                {
                    strFieldNames += " [" + DAL.Sanitize(strKey) + "],";
                    if (type != typeof(Person) || strKey.ToLower() != "postofficebox")
                    {
                        strValues += "'" + DAL.Sanitize(dicInputFields[strKey]) + "',";
                    }
                    else
                    {
                        strValues += "'" + DAL.Sanitize(HashIt(dicInputFields[strKey])) + "',";
                    }
                }
            }
            strFieldNames = strFieldNames.Remove(strFieldNames.LastIndexOf(','));
            strValues = strValues.Remove(strValues.LastIndexOf(','));
            strValues += ");";
            string strSql = strFieldNames + strValues;
            DAL.Insert(strSql);
        }

        private static string HashIt(string strInput)
        {
            byte[] salt = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(strInput, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            string savedPasswordHash = Convert.ToBase64String(hashBytes);
            return savedPasswordHash;
        }

        //private static void NoInUse(Dictionary<string, string> dicInputFields, ref string strFieldNames, ref string strValues)
        //{
        //    if (dicInputFields["arrivalDate"] != "")
        //    {
        //        strFieldNames += ",                    [arrivalDate]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["arrivalDate"]) + "'";
        //    }
        //    if (dicInputFields["birthDate"] != "")
        //    {
        //        strFieldNames += ",                    [birthDate]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["birthDate"]) + "'";
        //    }
        //    if (dicInputFields["fleaCollarPlacedOn"] != "")
        //    {
        //        strFieldNames += ",                  [fleaCollarPlacedOn]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["fleaCollarPlacedOn"]) + "'";
        //    }
        //    if (dicInputFields["inLocationSince"] != "")
        //    {
        //        strFieldNames += ",                     [inLocationSince]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["inLocationSince"]) + "'";
        //    }
        //    if (dicInputFields["inLocationUntil"] != "")
        //    {
        //        strFieldNames += ",                     [inLocationUntil]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["inLocationUntil"]) + "'";
        //    }
        //    if (dicInputFields["weightMeasuredOn"] != "")
        //    {
        //        strFieldNames += ",                    [weightMeasuredOn]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["weightMeasuredOn"]) + "'";
        //    }
        //    if (dicInputFields["breed"] != "")
        //    {
        //        strFieldNames += ",                    [breed]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["breed"]) + "'";
        //    }
        //    if (dicInputFields["cage"] != "")
        //    {
        //        strFieldNames += ",                    [cage]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["cage"]) + "'";
        //    }
        //    if (dicInputFields["color"] != "")
        //    {
        //        strFieldNames += ",                    [color]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["color"]) + "'";
        //    }
        //    if (dicInputFields["fleaCollarType"] != "")
        //    {
        //        strFieldNames += ",                    [fleaCollarType]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["fleaCollarType"]) + "'";
        //    }
        //    if (dicInputFields["medicalInfo"] != "")
        //    {
        //        strFieldNames += ",                  [medicalInfo]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["medicalInfo"]) + "'";
        //    }
        //    if (dicInputFields["name"] != "")
        //    {
        //        strFieldNames += ",                    [name]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["name"]) + "'";
        //    }
        //    if (dicInputFields["remarks"] != "")
        //    {
        //        strFieldNames += ",                    [remarks]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["remarks"]) + "'";
        //    }
        //    if (dicInputFields["weightInKilo"] != "")
        //    {
        //        strFieldNames += ",                    [weightInKilo]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["weightInKilo"]) + "'";
        //    }
        //    if (dicInputFields["fleaCollarGoodForMonths"] != "")
        //    {
        //        strFieldNames += ",                    [fleaCollarGoodForMonths]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["fleaCollarGoodForMonths"]) + "'";
        //    }
        //    if (dicInputFields["gender_"] != "")
        //    {
        //        strFieldNames += ",                    [gender_]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["gender_"]) + "'";
        //    }
        //    if (dicInputFields["size_"] != "")
        //    {
        //        strFieldNames += ",                    [size_]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["size_"]) + "'";
        //    }
        //    if (dicInputFields["availableForAdoption"] != "")
        //    {
        //        strFieldNames += ",                    [availableForAdoption]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["availableForAdoption"]) + "'";
        //    }
        //    if (dicInputFields["canBeChained"] != "")
        //    {
        //        strFieldNames += ",                    [canBeChained]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["canBeChained"]) + "'";
        //    }
        //    if (dicInputFields["canLiveOutdoors"] != "")
        //    {
        //        strFieldNames += ",                    [canLiveOutdoors]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["canLiveOutdoors"]) + "'";
        //    }
        //    if (dicInputFields["died"] != "")
        //    {
        //        strFieldNames += ",                    [died]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["died"]) + "'";
        //    }
        //    if (dicInputFields["fleaCollarAlsoForTicks"] != "")
        //    {
        //        strFieldNames += ",                    [fleaCollarAlsoForTicks]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["fleaCollarAlsoForTicks"]) + "'";
        //    }
        //    if (dicInputFields["goodWithChildren"] != "")
        //    {
        //        strFieldNames += ",                    [goodWithChildren]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["goodWithChildren"]) + "'";
        //    }
        //    if (dicInputFields["goodWithOtherAnimalsSameSpecies"] != "")
        //    {
        //        strFieldNames += ",                    [goodWithOtherAnimalsSameSpecies]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["goodWithOtherAnimalsSameSpecies"]) + "'";
        //    }
        //    if (dicInputFields["goodWithOtherAnimalsDifferentSpecies"] != "")
        //    {
        //        strFieldNames += ",                    [goodWithOtherAnimalsDifferentSpecies]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["goodWithOtherAnimalsDifferentSpecies"]) + "'";
        //    }
        //    if (dicInputFields["houseTrained"] != "")
        //    {
        //        strFieldNames += ",                    [houseTrained]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["houseTrained"]) + "'";
        //    }
        //    if (dicInputFields["isEnergetic"] != "")
        //    {
        //        strFieldNames += ",                    [isEnergetic]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["isEnergetic"]) + "'";
        //    }
        //    if (dicInputFields["sterilizationMarked"] != "")
        //    {
        //        strFieldNames += ",                    [sterilizationMarked]";
        //        strValues += ",'" + DAL.Sanitize(dicInputFields["sterilizationMarked"]) + "'";
        //    }
        //}

        //private void ReformatAllDates(ref Dictionary<string, string> dicInputFields, string[] strarrDateFieldNames)
        //{
        //    //This method can throw Exceptions
        //    try
        //    {
        //        foreach (string strDateFieldName in strarrDateFieldNames)
        //        {
        //            dicInputFields[strDateFieldName] = ReformatDate(dicInputFields[strDateFieldName]);//This method can throw Exceptions
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("There is a problem with the format of one of the dates you sent", ex);
        //    }
        //}

        private static string ReformatDate(string strOld)
        {
            //This method can throw Exceptions
            if (strOld == "")
            {
                return "";
            }
            string[] strarrSeparateDateFromTime;
            strarrSeparateDateFromTime = strOld.Split(new char[] { ' ' });
            if (strarrSeparateDateFromTime.Length > 1)
            {
                strOld = strarrSeparateDateFromTime[0];
            }
            string[] strarrDateParts = strOld.ToString().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (strarrDateParts.Length < 2)
            {
                strarrDateParts = strOld.ToString().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            }
            DateTime datetimeReformatted;
            if (strarrDateParts.Length == 2)
            {
                datetimeReformatted = new DateTime(DateTime.Now.Year, int.Parse(strarrDateParts[1]), int.Parse(strarrDateParts[0]));
            }
            else
            {
                if (int.Parse(strarrDateParts[0]) > 12)
                {
                    datetimeReformatted = new DateTime(int.Parse(strarrDateParts[0]), int.Parse(strarrDateParts[1]), int.Parse(strarrDateParts[2]));
                }
                else
                {
                    datetimeReformatted = new DateTime(int.Parse(strarrDateParts[2]), int.Parse(strarrDateParts[1]), int.Parse(strarrDateParts[0]));
                }
            }
            if (datetimeReformatted.Year < 1000)
            {
                datetimeReformatted = datetimeReformatted.AddYears(2000);
            }
            if (strarrSeparateDateFromTime.Length > 1)
            {
                datetimeReformatted += TimeSpan.Parse(strarrSeparateDateFromTime[1]);//This method can throw Exceptions
            }
            return datetimeReformatted.ToString("yyyy-MM-dd HH:mm:ss");
        }

        //public string UpdateAnimal(int id, int availableForAdoption_accepts_null = int.MinValue, int isEnergetic_accepts_null = int.MinValue, int sterilizationMarked = int.MinValue, int fleaCollarAlsoForTicks_accepts_null = int.MinValue, int houseTrained_accepts_null = int.MinValue, int goodWithChildren_accepts_null = int.MinValue, int goodWithOtherAnimalsSameSpecies_accepts_null = int.MinValue, int goodWithOtherAnimalsDifferentSpecies_accepts_null = int.MinValue, int canLiveOutdoors_accepts_null = int.MinValue, int canBeChained_accepts_null = int.MinValue, int died_accepts_null = int.MinValue, int species_ = int.MinValue, string name = "optional", string remarks = "optional", string medicalInfo = "optional", int currentLocation_ = int.MinValue, DateTime inLocationSince_accepts_null = default(DateTime), DateTime inLocationUntil_accepts_null = default(DateTime), DateTime birthDate_accepts_null = default(DateTime), DateTime arrivalDate_accepts_null = default(DateTime), int gender_ = int.MinValue, DateTime fleaCollarPlacedOn_accepts_null = default(DateTime), string fleaCollarType = "opotional", int fleaCollarGoodForMonths = int.MinValue, string cage = "optional", string color = "optional", string breed = "optional", int size_ = int.MinValue, int weightInKilo = int.MinValue, DateTime weightMeasuredOn_accepts_null = default(DateTime))
        //{
        //    {
        //        try
        //        {
        //            int intUserId = SearchUserIdBySubmittedToken();
        //        }
        //        catch (Exception ex)
        //        {
        //            return "";    //SearchUserIdBySubmittedToken already closed the response
        //        }
        //        try
        //        {
        //            DAL.Insert("INSERT INTO [tblAnimals] ([name], [species_], [location_]) VALUES ('" + DAL.Sanitize(name) + "', '" + DAL.Sanitize(species_.ToString()) + "', '" + DAL.Sanitize(currentLocation_.ToString()) + "');");
        //            return "Success";
        //        }
        //        catch (Exception ex)
        //        {
        //            string strErrorMessage;
        //            strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
        //            return strErrorMessage;
        //        }
        //    }
        //}


        public string GetTime()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        }

        private DataSet GetInner(string strWhat)
        {
            using (DataTable datatable = DAL.SelectWithForeignKeys("SELECT * FROM [tbl" + DAL.Sanitize(strWhat) + "];"))
            {
                HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                //  HttpContext.Current.Response.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine);
                //HttpContext.Current.Response.ContentType = "text/xml";
                // datatable.WriteXml(HttpContext.Current.Response.OutputStream);
                //    StringBuilder stringbuilder = new StringBuilder();
                //      using (XmlWriter xmlwriter = XmlWriter.Create(stringbuilder))
                //    {
                //      datatable.WriteXml(xmlwriter,XmlWriteMode.IgnoreSchema);
                //    xmlwriter.Close();
                //  xmlwriter.Dispose();
                //    }
                //    datatable.Dispose();
                //   return stringbuilder.ToString();
                //       return datatable;
                using (DataSet ds = new DataSet("dataset"))
                {
                    ds.Tables.Add(datatable);
                    datatable.Dispose();
                    return ds;
                }
            }
        }

        private string CreateToken(int intUserId)
        {
            string strToken = Global.random.NextDouble().ToString().Substring(2);
            strToken += Char.ConvertFromUtf32(Global.random.Next(63, 122));
            strToken += Global.random.NextDouble().ToString().Substring(2);
            strToken += Char.ConvertFromUtf32(Global.random.Next(63, 122));
            strToken += Global.random.NextDouble().ToString().Substring(2);
            strToken += Char.ConvertFromUtf32(Global.random.Next(63, 122));
            strToken += Global.random.NextDouble().ToString().Substring(2);
            strToken += Char.ConvertFromUtf32(Global.random.Next(63, 122));
            strToken += Global.random.NextDouble().ToString().Substring(2);
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytearr = hash.ComputeHash(Encoding.UTF8.GetBytes(strToken));
                strToken = "";
                for (int i = 0; i < bytearr.Length; i++)
                {
                    strToken += bytearr[i].ToString("X2");
                    double doublerandom = Global.random.NextDouble();
                    if (doublerandom > 0.7)
                    {
                        strToken += Char.ConvertFromUtf32(Global.random.Next(63, 122));
                    }
                    else
                    {
                        strToken += Global.random.Next(0, 999).ToString();
                    }
                }
            }
            Global.dicUserByToken.Add(strToken, intUserId);
            Global.dicCreationTimeByToken.Add(strToken, DateTime.Now);
            using (DataTable datatable = DAL.Select("SELECT TOP 1 [role_] FROM [tblPersons] WHERE [id]='" + DAL.Sanitize(intUserId.ToString()) + "';"))
            {
                if (datatable.Rows.Count > 0)
                {
                    int intRole;
                    if (int.TryParse((datatable.Rows[0][0]).ToString(), out intRole))
                    {
                        Global.dicRoleByUser[intUserId] = intRole;
                    }
                }
            }
            return strToken;
        }

        private static void ClearUserTokens(int intUser)
        {
            List<string> liststrTokensToRemove = new List<string>();
            foreach (string strToken in Global.dicUserByToken.Keys)
            {
                try
                {
                    if ((Global.dicUserByToken[strToken]) == intUser)
                    {
                        liststrTokensToRemove.Add(strToken);
                    }
                }
                catch (Exception ex)
                {
                    //
                }
            }
            foreach (string strTokenToRemove in liststrTokensToRemove)
            {
                try
                {
                    Global.dicCreationTimeByToken.Remove(strTokenToRemove);
                    Global.dicUserByToken.Remove(strTokenToRemove);
                }
                catch (Exception ex)
                {
                    //
                }
            }
        }

        private static string SendErrorSpecifyingForeignKeyFieldName(Exception ex)
        {
            if (ex.Message.ToLower().Contains("statement conflicted with the foreign key constraint"))
            {
                HttpContext.Current.Response.StatusCode = 547;
                string strProblemTable = "";
                string strToLookFor = ", table \"";
                if (ex.Message.Contains(strToLookFor))
                {
                    strProblemTable = ex.Message.Substring((ex.Message.IndexOf(strToLookFor)) + strToLookFor.Length);
                    strProblemTable = strProblemTable.Replace("dbo.", "");
                    if (strProblemTable.StartsWith("tbl"))
                    {
                        strProblemTable = strProblemTable.Substring(3);
                    }
                    strProblemTable = strProblemTable.Remove(strProblemTable.IndexOf('"'));
                }
                return "The " + strProblemTable + " table does not have the value you sent.";
            }
            HttpContext.Current.Response.StatusCode = 400;
            return "Bad Request";
        }

        private int SearchUserIdBySubmittedToken(int intMinimalRoleForThis, Stream stream)
        {
            //This method will throw an Exception in case the token is invalid
            try
            {
                return SearchUserByTokenInner(intMinimalRoleForThis);
            }
            catch (Exception ex)
            {
                stream.Close();
                stream.Dispose();
                HttpContext.Current.Response.End();
                throw new Exception();
            }
        }

        private int SearchUserIdBySubmittedToken(int intMinimalRoleForThis)
        {
            //This method will throw an Exception in case the token is invalid
            try
            {
                return SearchUserByTokenInner(intMinimalRoleForThis);
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.End();
                throw new Exception();
            }
        }

        private int SearchUserByTokenInner(int intMinimalRoleForThis)
        {
            //This method will throw an Exception in case the token is invalid
            Global.DeleteExpiredTokens();
            string strSubmittedToken = "";
            if (HttpContext.Current.Request.Cookies["ap"] == null || HttpContext.Current.Request.Cookies["ap"].Value == null || HttpContext.Current.Request.Cookies["ap"].Value.Trim() == "")
            {
                HttpContext.Current.Response.Cookies.Remove("ap");
                //httpCookie.HttpOnly = true;
            }
            else
            {
                strSubmittedToken = (HttpContext.Current.Request.Cookies["ap"]).Value;
            }
            //var varHeaders = HttpContext.Current.Request.Headers;
            //if (varHeaders["Autehntication"] != null)
            //{
            //    if ((varHeaders["Autehntication"]).StartsWith("Basic "))
            //    {
            //        string strSubmittedToken = (varHeaders["Autehntication"]).Trim();
            //        strSubmittedToken = strSubmittedToken.Substring("Basic ".Length);
            if (Global.dicUserByToken.ContainsKey(strSubmittedToken))
            {
                if (Global.dicCreationTimeByToken.ContainsKey(strSubmittedToken))
                {
                    int intUserId;
                    intUserId = Global.dicUserByToken[strSubmittedToken];
                    if (Global.dicRoleByUser.ContainsKey(intUserId))
                    {
                        //Is the user's role high enough to do what he/she asks?
                        if ((Global.dicRoleByUser[intUserId]) >= intMinimalRoleForThis)
                        {
                            return intUserId;
                        }
                    }
                }
            }
            //   Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            //   string usernamePassword = encoding.GetString(Convert.FromBase64String(strEncodedUsernamePassword));
            //   int seperatorIndex = usernamePassword.IndexOf(':');
            //   string username= usernamePassword.Substring(0, seperatorIndex);
            //   string password = usernamePassword.Substring(seperatorIndex + 1);
            //    }
            //   }
            HttpContext.Current.Response.Cookies.Remove("ap");
            HttpContext.Current.Response.Cookies.Remove("ap");
            HttpContext.Current.Session.Clear(); //IMPORTANT
            HttpContext.Current.Session.Abandon();
            HttpContext.Current.Response.StatusCode = 403;
            throw new Exception();
        }

        public static void HandleException(Exception ex)
        {
            try
            {
                string strException = Environment.NewLine + Environment.NewLine + DateTime.Now + Environment.NewLine + ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    strException += Environment.NewLine + "Inner: " + ex.Message;
                }
                strException += Environment.NewLine + "Stacktrace= " + ex.StackTrace;
                if (File.Exists(Global.strLogFileName))
                {
                    FileInfo fileinfo = new FileInfo(Global.strLogFileName);
                    if (fileinfo.Length > 100000000)
                    {
                        File.Delete(Global.strLogFileName);
                    }
                }
                File.AppendAllText(Global.strLogFileName, strException, Encoding.UTF8);
                //   HttpContext.Current.Response.Write("Forbidden");
                HttpContext.Current.Response.StatusCode = 403;
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.Close();
            }
            catch (Exception ex2)
            {
                //
            }
        }

        private int GetErrorCode(Exception ex)
        {
            int code;
            if (int.TryParse(ex.Message, out code))
            {
                return code; //TODO: log it with detail
            }
            else
            {
                return 10;
            }
        }

        public int GetDataUsingDataContract(Animal composite)
        {
            //WARNING: This method might have strings and ints and decimals equal to null. You don't want them to leak out of here and cause null reference Exceptions down the road
            bool? a = composite.availableForAdoption;
            bool aa;
            if (a == null)
            {
                //Do nothing, because the user left this property blank, e.g leave this property out of the SQL UPDATE query, so what is already in the DB won't change.
            }
            else
            {
                aa = (bool)a;//now use aa, e.g. take your sql query strSql="    UPDATE [tblAnimals] SET [isAggressive]='true'   " and add this property to the query:    strSql+="   ,   [availableForAdoption]='"+aa.ToString()+"'      "
            }
            decimal? c = composite.weightInKilo;
            if (c == null)
            {
                c = decimal.MinValue;//Later in the program I will check c, and if it's decimal.MinValue I know that it means that the user left the property blank
            }
            else
            {
                decimal cc;
                cc = (decimal)c;
                //now use cc
            }
            string d = composite.name;
            if (d == null)
            {
                //Do nothing, but don't let d leave this method because it's a string equal to null!
                //Or
                //assign a value that you can check afterwards and tell whether the user left the property blank, e.g.
                d = "the_user_left_this_property_blank";
                // or d= const_string_user_left_the_property_blank.
            }
            else
            {
                string dd;
                dd = d;
                //do something with dd, it's OK, it's not a null
                string ww = "esfwasd" + dd;
            }
            if (composite.color == null)
            {
                int j = 6;
                j++;
            }
            else
            {
                int asd = 765;
                asd++;
            }

            return 29;// ww;           
        }

        //public string UptateAnimal(Animal animal)
        //{
        //    try
        //    {
        //        int intUserId = SearchUserIdBySubmittedToken(5);
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";    //SearchUserIdBySubmittedToken already closed the response
        //    }
        //    try
        //    {
        //        string strSql;
        //        strSql = "UPDATE [tblAnimals] SET ";
        //        if (animal.species_ != null)
        //        {
        //            strSql += " [species_]='" + DAL.Sanitize(animal.species_.ToString()) + "', ";
        //        }
        //        if (animal.location_ != null)
        //        {
        //            strSql += " [location_]='" + DAL.Sanitize(animal.location_.ToString()) + "', ";
        //        }
        //        if (animal.arrivalDate != null)
        //        {
        //            strSql += " [arrivalDate]='" + DAL.Sanitize(animal.arrivalDate.ToString()) + "', ";
        //        }
        //        if (animal.birthDate != null)
        //        {
        //            strSql += " [birthDate]='" + DAL.Sanitize(animal.birthDate.ToString()) + "', ";
        //        }
        //        if (animal.fleaCollarPlacedOn != null)
        //        {
        //            strSql += " [fleaCollarPlacedOn]='" + DAL.Sanitize(animal.fleaCollarPlacedOn.ToString()) + "', ";
        //        }
        //        if (animal.inLocationSince != null)
        //        {
        //            strSql += "  [inLocationSince]='" + DAL.Sanitize(animal.inLocationSince.ToString()) + "', ";
        //        }
        //        if (animal.inLocationUntil != null)
        //        {
        //            strSql += " [inLocationUntil]='" + DAL.Sanitize(animal.inLocationUntil.ToString()) + "', ";
        //        }
        //        if (animal.weightMeasuredOn != null)
        //        {
        //            strSql += " [weightMeasuredOn]='" + DAL.Sanitize(animal.weightMeasuredOn.ToString()) + "', ";
        //        }
        //        if (animal.breed != null)
        //        {
        //            strSql += " [breed]='" + DAL.Sanitize(animal.breed) + "', ";
        //        }
        //        if (animal.cage != null)
        //        {
        //            strSql += " [cage]='" + DAL.Sanitize(animal.cage) + "', ";
        //        }
        //        if (animal.color != null)
        //        {
        //            strSql += " [color]='" + DAL.Sanitize(animal.color) + "', ";
        //        }
        //        if (animal.fleaCollarType != null)
        //        {
        //            strSql += " [fleaCollarType]='" + DAL.Sanitize(animal.fleaCollarType) + "', ";
        //        }
        //        if (animal.medicalInfo != null)
        //        {
        //            strSql += " [medicalInfo]='" + DAL.Sanitize(animal.medicalInfo) + "', ";
        //        }
        //        if (animal.name != null)
        //        {
        //            strSql += " [name]='" + DAL.Sanitize(animal.name) + "', ";
        //        }
        //        if (animal.remarks != null)
        //        {
        //            strSql += " [remarks]='" + DAL.Sanitize(animal.remarks) + "', ";
        //        }
        //        if (animal.weightInKilo != null)
        //        {
        //            strSql += " [weightInKilo]='" + DAL.Sanitize(animal.weightInKilo.ToString()) + "', ";
        //        }
        //        if (animal.fleaCollarGoodForMonths != null)
        //        {
        //            strSql += " [fleaCollarGoodForMonths]='" + DAL.Sanitize(animal.fleaCollarGoodForMonths.ToString()) + "', ";
        //        }
        //        if (animal.gender_ != null)
        //        {
        //            strSql += " [gender_]='" + DAL.Sanitize(animal.gender_.ToString()) + "', ";
        //        }
        //        if (animal.size_ != null)
        //        {
        //            strSql += " [size_]='" + DAL.Sanitize(animal.size_.ToString()) + "', ";
        //        }
        //        if (animal.availableForAdoption != null)
        //        {
        //            strSql += " [availableForAdoption]='" + DAL.Sanitize(animal.availableForAdoption.ToString()) + "', ";
        //        }
        //        if (animal.canBeChained != null)
        //        {
        //            strSql += " [canBeChained]='" + DAL.Sanitize(animal.canBeChained.ToString()) + "', ";
        //        }
        //        if (animal.canLiveOutdoors != null)
        //        {
        //            strSql += " [canLiveOutdoors]='" + DAL.Sanitize(animal.canLiveOutdoors.ToString()) + "', ";
        //        }
        //        if (animal.died != null)
        //        {
        //            strSql += " [died]='" + DAL.Sanitize(animal.died.ToString()) + "', ";
        //        }
        //        if (animal.fleaCollarAlsoForTicks != null)
        //        {
        //            strSql += " [fleaCollarAlsoForTicks]='" + DAL.Sanitize(animal.fleaCollarAlsoForTicks.ToString()) + "', ";
        //        }
        //        if (animal.goodWithChildren != null)
        //        {
        //            strSql += " [goodWithChildren]='" + DAL.Sanitize(animal.goodWithChildren.ToString()) + "', ";
        //        }
        //        if (animal.goodWithOtherAnimalsSameSpecies != null)
        //        {
        //            strSql += " [goodWithOtherAnimalsSameSpecies]='" + DAL.Sanitize(animal.goodWithOtherAnimalsSameSpecies.ToString()) + "', ";
        //        }
        //        if (animal.goodWithOtherAnimalsDifferentSpecies != null)
        //        {
        //            strSql += " [goodWithOtherAnimalsDifferentSpecies]='" + DAL.Sanitize(animal.goodWithOtherAnimalsDifferentSpecies.ToString()) + "', ";
        //        }
        //        if (animal.houseTrained != null)
        //        {
        //            strSql += " [houseTrained]='" + DAL.Sanitize(animal.houseTrained.ToString()) + "', ";
        //        }
        //        if (animal.isEnergetic != null)
        //        {
        //            strSql += " [isEnergetic]='" + DAL.Sanitize(animal.isEnergetic.ToString()) + "', ";
        //        }
        //        if (animal.sterilizationMarked != null)
        //        {
        //            strSql += " [sterilizationMarked]='" + DAL.Sanitize(animal.sterilizationMarked.ToString()) + "', ";
        //        }
        //        strSql = strSql.Remove(strSql.LastIndexOf(','));
        //        if (animal.id == null)
        //        {
        //            throw new Exception("You must supply an id");
        //        }

        //        //IMPORTANT                                      //IMPORTANT:
        //        strSql += " WHERE [id]='" + DAL.Sanitize(animal.id.ToString()) + "'";
        //        int intUpdatedRecordsCount;
        //        intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
        //        if (intUpdatedRecordsCount == 1)
        //        {
        //            return "Success";
        //        }
        //        else if (intUpdatedRecordsCount > 1)
        //        {
        //            return "Problem: more than one records were updated.";
        //        }
        //        else
        //        {
        //            return "Problem: no records were updated. Make sure you sent the correct animal id";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string strErrorMessage;
        //        strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
        //        return strErrorMessage;
        //    }
        //}

        //public string InsertPerson(Person person)
        //{
        //    try
        //    {
        //        int intUserId = SearchUserIdBySubmittedToken(9);
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";    //SearchUserIdBySubmittedToken already closed the response
        //    }
        //    try
        //    {
        //        if (person.id != null)
        //        {
        //            throw new Exception("Leave the id blank when inserting a new person");
        //        }
        //        if (person.firstName == null || person.lastName == null || person.firstName.Trim() == "" || person.lastName.Trim() == "")
        //        {
        //            throw new Exception("You must supply a first name and a last name");
        //        }
        //        string strFieldNames;
        //        string strValues;
        //        strFieldNames = "INSERT INTO [tblPersons] (    [firstName], [lastName]";
        //        strValues = "    )    VALUES ( '" + DAL.Sanitize(person.firstName) + "', '" + DAL.Sanitize(person.lastName) + "'";
        //        if (person.role_ != null)
        //        {
        //            strFieldNames += ",                    [role_]";
        //            strValues += ",'" + DAL.Sanitize(person.role_.ToString()) + "'";
        //        }
        //        if (person.gender_ != null)
        //        {
        //            strFieldNames += ",                    [gender_]";
        //            strValues += ",'" + DAL.Sanitize(person.gender_.ToString()) + "'";
        //        }
        //        if (person.idCard != null)
        //        {
        //            strFieldNames += ",                    [idCard]";
        //            strValues += ",'" + DAL.Sanitize(person.idCard.ToString()) + "'";
        //        }
        //        if (person.email != null)
        //        {
        //            strFieldNames += ",                    [email]";
        //            strValues += ",'" + DAL.Sanitize(person.email.ToString()) + "'";
        //        }
        //        if (person.fax != null)
        //        {
        //            strFieldNames += ",                    [fax]";
        //            strValues += ",'" + DAL.Sanitize(person.fax.ToString()) + "'";
        //        }
        //        if (person.postOfficeBox != null)
        //        {
        //            strFieldNames += ",                     [postOfficeBox]";
        //            strValues += ",'" + DAL.Sanitize(person.postOfficeBox.ToString()) + "'";
        //        }
        //        if (person.address != null)
        //        {
        //            strFieldNames += ",                    [address]";
        //            strValues += ",'" + DAL.Sanitize(person.address.ToString()) + "'";
        //        }
        //        if (person.phone1 != null)
        //        {
        //            strFieldNames += ",                    [phone1]";
        //            strValues += ",'" + DAL.Sanitize(person.phone1) + "'";
        //        }
        //        if (person.phone2 != null)
        //        {
        //            strFieldNames += ",                    [phone2]";
        //            strValues += ",'" + DAL.Sanitize(person.phone2) + "'";
        //        }
        //        if (person.workPlace != null)
        //        {
        //            strFieldNames += ",                    [workPlace]";
        //            strValues += ",'" + DAL.Sanitize(person.workPlace) + "'";
        //        }
        //        if (person.everReturnedAnAnimal != null)
        //        {
        //            strFieldNames += ",                    [everReturnedAnAnimal]";
        //            strValues += ",'" + DAL.Sanitize(person.everReturnedAnAnimal.ToString()) + "'";
        //        }
        //        if (person.remarks != null)
        //        {
        //            strFieldNames += ",                    [remarks]";
        //            strValues += ",'" + DAL.Sanitize(person.remarks) + "'";
        //        }
        //        if (person.doNotGiveHimAnimals != null)
        //        {
        //            strFieldNames += ",                    [doNotGiveHimAnimals]";
        //            strValues += ",'" + DAL.Sanitize(person.doNotGiveHimAnimals.ToString()) + "'";
        //        }
        //        strValues += ");";
        //        string strSql = strFieldNames + strValues;
        //        DAL.Insert(strSql);
        //        return "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        string strErrorMessage;
        //        strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
        //        return strErrorMessage;
        //    }
        //}

        //public string UpdatePerson(Person person)
        //{
        //    try
        //    {
        //        int intUserId = SearchUserIdBySubmittedToken(9);
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";    //SearchUserIdBySubmittedToken already closed the response
        //    }
        //    try
        //    {
        //        string strSql;
        //        strSql = "UPDATE [tblPersons] SET ";
        //        if (person.firstName != null)
        //        {
        //            strSql += " [firstName]='" + DAL.Sanitize(person.firstName.ToString()) + "', ";
        //        }
        //        if (person.lastName != null)
        //        {
        //            strSql += " [lastName]='" + DAL.Sanitize(person.lastName.ToString()) + "', ";
        //        }
        //        if (person.role_ != null)
        //        {
        //            strSql += " [role_]='" + DAL.Sanitize(person.role_.ToString()) + "', ";
        //        }
        //        if (person.gender_ != null)
        //        {
        //            strSql += " [gender_]='" + DAL.Sanitize(person.gender_.ToString()) + "', ";
        //        }
        //        if (person.idCard != null)
        //        {
        //            strSql += " [idCard]='" + DAL.Sanitize(person.idCard.ToString()) + "', ";
        //        }
        //        if (person.email != null)
        //        {
        //            strSql += " [email]='" + DAL.Sanitize(person.email.ToString()) + "', ";
        //        }
        //        if (person.fax != null)
        //        {
        //            strSql += " [fax]='" + DAL.Sanitize(person.fax.ToString()) + "', ";
        //        }
        //        if (person.postOfficeBox != null)
        //        {
        //            strSql += " [postOfficeBox]='" + DAL.Sanitize(person.postOfficeBox.ToString()) + "', ";
        //        }
        //        if (person.address != null)
        //        {
        //            strSql += "  [address]='" + DAL.Sanitize(person.address.ToString()) + "', ";
        //        }
        //        if (person.phone1 != null)
        //        {
        //            strSql += " [phone1]='" + DAL.Sanitize(person.phone1.ToString()) + "', ";
        //        }
        //        if (person.phone2 != null)
        //        {
        //            strSql += " [phone2]='" + DAL.Sanitize(person.phone2.ToString()) + "', ";
        //        }
        //        if (person.workPlace != null)
        //        {
        //            strSql += " [workPlace]='" + DAL.Sanitize(person.workPlace) + "', ";
        //        }
        //        if (person.everReturnedAnAnimal != null)
        //        {
        //            strSql += " [everReturnedAnAnimal]='" + DAL.Sanitize(person.everReturnedAnAnimal.ToString()) + "', ";
        //        }
        //        if (person.remarks != null)
        //        {
        //            strSql += " [remarks]='" + DAL.Sanitize(person.remarks) + "', ";
        //        }
        //        if (person.doNotGiveHimAnimals != null)
        //        {
        //            strSql += " [doNotGiveHimAnimals]='" + DAL.Sanitize(person.doNotGiveHimAnimals.ToString()) + "', ";
        //        }
        //        strSql = strSql.Remove(strSql.LastIndexOf(','));
        //        if (person.id == null)
        //        {
        //            throw new Exception("You must supply an id");
        //        }

        //        //IMPORTANT                                      //IMPORTANT:
        //        strSql += " WHERE [id]='" + DAL.Sanitize(person.id.ToString()) + "'";
        //        int intUpdatedRecordsCount;
        //        intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
        //        if (intUpdatedRecordsCount == 1)
        //        {
        //            return "Success";
        //        }
        //        else if (intUpdatedRecordsCount > 1)
        //        {
        //            return "Problem: more than one records were updated.";
        //        }
        //        else
        //        {
        //            return "Problem: no records were updated. Make sure you sent the correct person id";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string strErrorMessage;
        //        strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
        //        return strErrorMessage;
        //    }
        //}

        //public string UpdateAdoption(Adoption adoption)
        //{
        //    try
        //    {
        //        int intUserId = SearchUserIdBySubmittedToken(5);
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";    //SearchUserIdBySubmittedToken already closed the response
        //    }
        //    try
        //    {
        //        string strSql;
        //        strSql = "UPDATE [tblAdoptions] SET ";
        //        if (adoption.animal_ != null)
        //        {
        //            throw new Exception("You can not change the adopted animal");
        //        }
        //        if (adoption.person_ != null)
        //        {
        //            strSql += " [person_]='" + DAL.Sanitize(adoption.person_.ToString()) + "', ";
        //        }
        //        if (adoption.adoptedOn != null)
        //        {
        //            strSql += " [adoptedOn]='" + DAL.Sanitize(adoption.adoptedOn.ToString()) + "', ";
        //        }
        //        if (adoption.temporaryUntil != null)
        //        {
        //            strSql += " [temporaryUntil]='" + DAL.Sanitize(adoption.temporaryUntil.ToString()) + "', ";
        //        }
        //        if (adoption.staffMember != null)
        //        {
        //            strSql += " [staffMember]='" + DAL.Sanitize(adoption.staffMember) + "', ";
        //        }
        //        if (adoption.remarks != null)
        //        {
        //            strSql += " [remarks]='" + DAL.Sanitize(adoption.remarks) + "', ";
        //        }
        //        strSql = strSql.Remove(strSql.LastIndexOf(','));
        //        if (adoption.id == null)
        //        {
        //            throw new Exception("You must supply an id");
        //        }

        //        //IMPORTANT                                      //IMPORTANT:
        //        strSql += " WHERE [id]='" + DAL.Sanitize(adoption.id.ToString()) + "'";
        //        int intUpdatedRecordsCount;
        //        intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
        //        if (intUpdatedRecordsCount == 1)
        //        {
        //            return "Success";
        //        }
        //        else if (intUpdatedRecordsCount > 1)
        //        {
        //            return "Problem: more than one records were updated.";
        //        }
        //        else
        //        {
        //            return "Problem: no records were updated. Make sure you sent the correct person id";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string strErrorMessage;
        //        strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
        //        return strErrorMessage;
        //    }
        //}

        //public string InsertAdoption(Adoption adoption)
        //{
        //    try
        //    {
        //        int intUserId = SearchUserIdBySubmittedToken(5);
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";    //SearchUserIdBySubmittedToken already closed the response
        //    }
        //    try
        //    {
        //        if (adoption.id != null)
        //        {
        //            throw new Exception("Leave the id blank when inserting a new adoption");
        //        }
        //        if (adoption.animal_ == null || adoption.person_ == null)
        //        {
        //            throw new Exception("You must supply the animal and the person who adopts it");
        //        }
        //        string strFieldNames;
        //        string strValues;
        //        strFieldNames = "INSERT INTO [tbladoptions] (    [animal_], [person_]";
        //        strValues = "    )    VALUES ( '" + DAL.Sanitize(adoption.animal_.ToString()) + "', '" + DAL.Sanitize(adoption.person_.ToString()) + "'";
        //        if (adoption.adoptedOn != null)
        //        {
        //            strFieldNames += ",                      [adoptedOn]";
        //            strValues += ",'" + DAL.Sanitize(adoption.adoptedOn.ToString()) + "'";
        //        }
        //        if (adoption.temporaryUntil != null)
        //        {
        //            strFieldNames += ",                      [temporaryUntil]";
        //            strValues += ",'" + DAL.Sanitize(adoption.temporaryUntil.ToString()) + "'";
        //        }
        //        if (adoption.staffMember != null)
        //        {
        //            strFieldNames += ",                      [staffMember]";
        //            strValues += ",'" + DAL.Sanitize(adoption.staffMember.ToString()) + "'";
        //        }
        //        if (adoption.remarks != null)
        //        {
        //            strFieldNames += ",                    [remarks]";
        //            strValues += ",'" + DAL.Sanitize(adoption.remarks) + "'";
        //        }
        //        strValues += ");";
        //        string strSql = strFieldNames + strValues;
        //        DAL.Insert(strSql);
        //        return "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        string strErrorMessage;
        //        strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
        //        return strErrorMessage;
        //    }
        //}

        public string DeleteDoc(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(Doc));
            return str;
        }

        public string DeleteAnimal(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(Animal));
            return str;
        }

        public string DeleteTreatment(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(Treatment));
            return str;
        }

        public string DeleteTreatmentType(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(TreatmentType));
            return str;
        }

        public string DeletePic(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(Pic));
            return str;
        }

        public string DeletePerson(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(Person));
            return str;
        }

        public string DeleteLocation(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(Location));
            return str;
        }

        public string DeleteChip(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(Chip));
            return str;
        }

        public string DeleteAdoption(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(Adoption));
            return str;
        }

        public string DeleteDocumentType(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(DocumentType));
            return str;
        }

        public string DeleteSize(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner(stream, typeof(Size));
            return str;
        }

        private string DeleteInner(Stream stream, Type type)
        {
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                int intRecordsUpdated;
                //This method can throw an Exception:
                intRecordsUpdated = PhraseSqlAndDelete(dicInputFields, type);
                return "Success. Deleted " + intRecordsUpdated.ToString() + " record";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        private int PhraseSqlAndDelete(Dictionary<string, string> dicInputFields, Type type)
        {
            //This method can throw an Exception
            int.Parse((dicInputFields["id"]).Trim()); //This will throw an Exception in case the id is not a valid number (or no id was supplied)
            string strSql;
            strSql = "DELETE FROM [tbl" + DAL.Sanitize(type.Name) + "s] WHERE [id]='" + DAL.Sanitize((dicInputFields["id"]).Trim()) + "';";
            int intRecordsUpdated;
            intRecordsUpdated = DAL.UpdateOrDelete(strSql);
            return intRecordsUpdated;
        }

        public string UpdateTreatmentType(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(TreatmentType));
            return str;
        }

        public string UpdateChip(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(Chip));
            return str;
        }

        public string UpdateDoc(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(Doc));
            return str;
        }

        public string UpdatePic(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(Pic));
            return str;
        }

        public string UpdateTreatment(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(Treatment));
            return str;
        }

        public string UpdateLocation(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(Location));
            return str;
        }

        public string UpdateSize(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(Size));
            return str;
        }

        public string UpdatePerson(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(Person));
            return str;
        }

        public string UpdateAnimal(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(Animal));
            return str;
        }

        public string UpdateAdoption(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(Adoption));
            return str;
        }

        public string UpdateDocumentType(Stream stream)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5, stream);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str;
            str = UpdateInner(stream, typeof(DocumentType));
            return str;
        }

        private string UpdateInner(Stream stream, Type type)
        {
            try
            {
                Dictionary<string, string> dicInputFields;
                dicInputFields = ReadInput(stream);
                dicInputFields = RemoveRequiredFieldsThatTheUserTriedToClear(dicInputFields, type);
                int intRecordsUpdated;
                intRecordsUpdated = PhraseSqlAndUpdate(dicInputFields, type);  //This method can throw an Exception
                InCaseOfAnimalDeath(dicInputFields, type);
                return "Success. Updated " + intRecordsUpdated.ToString() + " record";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        private Dictionary<string, string> RemoveRequiredFieldsThatTheUserTriedToClear(Dictionary<string, string> dicInputFields, Type type)
        {
            if (type == typeof(Pic))
            {
                if (dicInputFields.ContainsKey("nameAndPathIncludingExtension") && (dicInputFields["nameAndPathIncludingExtension"]).Trim() == "")
                {
                    dicInputFields.Remove("nameAndPathIncludingExtension");
                }
                if (dicInputFields.ContainsKey("isArrivalPic") && (dicInputFields["isArrivalPic"]).Trim() == "")
                {
                    dicInputFields.Remove("isArrivalPic");
                }
                if (dicInputFields.ContainsKey("hiddenFromPublic") && (dicInputFields["hiddenFromPublic"]).Trim() == "")
                {
                    dicInputFields.Remove("hiddenFromPublic");
                }
                if (dicInputFields.ContainsKey("animal_") && (dicInputFields["animal_"]).Trim() == "")
                {
                    dicInputFields.Remove("animal_");
                }
            }
            else if (type == typeof(Adoption))
            {
                if (dicInputFields.ContainsKey("animal_") && (dicInputFields["animal_"]).Trim() == "")
                {
                    dicInputFields.Remove("animal_");
                }
                if (dicInputFields.ContainsKey("person_") && (dicInputFields["person_"]).Trim() == "")
                {
                    dicInputFields.Remove("person_");
                }
                if (dicInputFields.ContainsKey("adoptedOn") && (dicInputFields["adoptedOn"]).Trim() == "")
                {
                    dicInputFields.Remove("adoptedOn");
                }
            }
            else if (type == typeof(Animal))
            {
                if (dicInputFields.ContainsKey("location_") && (dicInputFields["location_"]).Trim() == "")
                {
                    dicInputFields.Remove("location_");
                }
                if (dicInputFields.ContainsKey("species_") && (dicInputFields["species_"]).Trim() == "")
                {
                    dicInputFields.Remove("species_");
                }
                if (dicInputFields.ContainsKey("size_") && (dicInputFields["size_"]).Trim() == "")
                {
                    dicInputFields.Remove("size_");
                }
                if (dicInputFields.ContainsKey("availableForAdoption") && (dicInputFields["availableForAdoption"]).Trim() == "")
                {
                    dicInputFields.Remove("availableForAdoption");
                }
                if (dicInputFields.ContainsKey("died") && (dicInputFields["died"]).Trim() == "")
                {
                    dicInputFields.Remove("died");
                }
                if (dicInputFields.ContainsKey("gender_") && (dicInputFields["gender_"]).Trim() == "")
                {
                    dicInputFields.Remove("gender_");
                }
                if (dicInputFields.ContainsKey("breed") && (dicInputFields["breed"]).Trim() == "")
                {
                    dicInputFields.Remove("breed");
                }
            }
            else if (type == typeof(Chip))
            {
                if (dicInputFields.ContainsKey("description") && (dicInputFields["description"]).Trim() == "")
                {
                    dicInputFields.Remove("description");
                }
                if (dicInputFields.ContainsKey("animal_") && (dicInputFields["animal_"]).Trim() == "")
                {
                    dicInputFields.Remove("animal_");
                }
            }
            else if (type == typeof(Doc))
            {
                if (dicInputFields.ContainsKey("animal_") && (dicInputFields["animal_"]).Trim() == "")
                {
                    dicInputFields.Remove("animal_");
                }
                if (dicInputFields.ContainsKey("nameAndPathIncludingExtension") && (dicInputFields["nameAndPathIncludingExtension"]).Trim() == "")
                {
                    dicInputFields.Remove("nameAndPathIncludingExtension");
                }
            }
            else if (type == typeof(Person))
            {
                if (dicInputFields.ContainsKey("role_") && (dicInputFields["role_"]).Trim() == "")
                {
                    dicInputFields.Remove("role_");
                }
                if (dicInputFields.ContainsKey("doNotGiveHimAnimals") && (dicInputFields["doNotGiveHimAnimals"]).Trim() == "")
                {
                    dicInputFields.Remove("doNotGiveHimAnimals");
                }
                if (dicInputFields.ContainsKey("fax") && (dicInputFields["fax"]).Trim() == "")
                {
                    dicInputFields.Remove("fax");
                }
                if (dicInputFields.ContainsKey("postOfficeBox") && (dicInputFields["postOfficeBox"]).Trim() == "")
                {
                    dicInputFields.Remove("postOfficeBox");
                }
                if (dicInputFields.ContainsKey("firstName") && (dicInputFields["firstName"]).Trim() == "")
                {
                    dicInputFields.Remove("firstName");
                }
                if (dicInputFields.ContainsKey("lastName") && (dicInputFields["lastName"]).Trim() == "")
                {
                    dicInputFields.Remove("lastName");
                }
            }
            else if (type == typeof(Treatment))
            {
                if (dicInputFields.ContainsKey("animal_") && (dicInputFields["animal_"]).Trim() == "")
                {
                    dicInputFields.Remove("animal_");
                }
                if (dicInputFields.ContainsKey("treatmentTime") && (dicInputFields["treatmentTime"]).Trim() == "")
                {
                    dicInputFields.Remove("treatmentTime");
                }
                if (dicInputFields.ContainsKey("person_") && (dicInputFields["person_"]).Trim() == "")
                {
                    dicInputFields.Remove("person_");
                }
                if (dicInputFields.ContainsKey("remarks") && (dicInputFields["remarks"]).Trim() == "")
                {
                    dicInputFields.Remove("remarks");
                }
                if (dicInputFields.ContainsKey("treatmentType_") && (dicInputFields["treatmentType_"]).Trim() == "")
                {
                    dicInputFields.Remove("treatmentType_");
                }
                if ( dicInputFields.ContainsKey("medicineName") && (dicInputFields["medicineName"]).Trim() == "")
                {
                    dicInputFields.Remove("medicineName");
                }
            }
            else if (type == typeof(TreatmentType))
            {
                if (dicInputFields.ContainsKey("numberOfRoundsNeeded") && (dicInputFields["numberOfRoundsNeeded"]).Trim() == "")
                {
                    dicInputFields.Remove("numberOfRoundsNeeded");
                }
                if (dicInputFields.ContainsKey("description") && (dicInputFields["description"]).Trim() == "")
                {
                    dicInputFields.Remove("description");
                }
              }
            else if (type == typeof(DocumentType))
            {
                if (dicInputFields.ContainsKey("description") && (dicInputFields["description"]).Trim() == "")
                {
                    dicInputFields.Remove("description");
                }
            }
            else if (type == typeof(Location))
            {
                if (dicInputFields.ContainsKey("description") && (dicInputFields["description"]).Trim() == "")
                {
                    dicInputFields.Remove("description");
                }
            }
            else if (type == typeof(Species))
            {
                if (dicInputFields.ContainsKey("description") && (dicInputFields["description"]).Trim() == "")
                {
                    dicInputFields.Remove("description");
                }
            }
            else if (type == typeof(Size))
            {
                if (dicInputFields.ContainsKey("description") && (dicInputFields["description"]).Trim() == "")
                {
                    dicInputFields.Remove("description");
                }
            }
            return dicInputFields;
        }

        private void InCaseOfAnimalDeath(Dictionary<string, string> dicInputFields, Type type)
        {
            if (type == typeof(Animal))
            {
                if (dicInputFields.ContainsKey("died"))
                {
                    if ((dicInputFields["died"]).Trim() != "")
                    {
                        if ((dicInputFields["died"]).Trim() == "1")
                        {
                            SetLocationTo3(dicInputFields);
                            SetAvailableForAdoptionTo0(dicInputFields, type);
                            EndPossibleTemporaryAdoption(dicInputFields, type);
                        }
                    }
                }
            }
        }

        private void EndPossibleTemporaryAdoption(Dictionary<string, string> dicInputFields, Type type)
        {
            try
            {
                string str;
                str = "UPDATE [tblAdoptions] SET [temporayUntil]=GETDATE() WHERE [animal_]='";
                if (type == typeof(Adoption))
                {
                    str += DAL.Sanitize(dicInputFields["animal_"]);
                }
                else if (type == typeof(Animal))
                {
                    str += DAL.Sanitize(dicInputFields["id"]);
                }
                str += "' AND [temporaryUntil] IS NOT NULL;";
            }
            catch (Exception ex)
            {
                throw new Exception("Error in EndPossibleTemporaryAdoption()", ex);
            }
        }

        private void SetLocationTo3(Dictionary<string, string> dicAnimalInputFields)
        {
            try
            {
                string str;
                str = "UPDATE [tblAnimals] SET [location_]='3' WHERE [id]='" + DAL.Sanitize(dicAnimalInputFields["id"]) + "';";
            }
            catch (Exception ex)
            {
                throw new Exception("Error in SetLocationTo3()", ex);
            }
        }

        private int PhraseSqlAndUpdate(Dictionary<string, string> dicInputFields, Type type)
        {
            //This method can throw an Exception
            FieldInfo[] fieldinfoarr = type.GetFields();
            Dictionary<string, Type> dicPropertyTypeByName = new Dictionary<string, Type>();
            foreach (FieldInfo fieldinfo in fieldinfoarr)
            {
                dicPropertyTypeByName.Add(fieldinfo.Name, fieldinfo.FieldType);
            }
            string strSql;
            strSql = "UPDATE [tbl" + DAL.Sanitize(type.Name) + "s] SET ";
            foreach (string strKey in dicInputFields.Keys)
            {
                if (false == dicPropertyTypeByName.ContainsKey(strKey))
                {
                    throw new Exception();
                }
                if (dicPropertyTypeByName[strKey] == typeof(DateTime) || dicPropertyTypeByName[strKey] == typeof(DateTime?))
                {
                    //This method can throw Exceptions:
                    dicInputFields[strKey] = ReformatDate(dicInputFields[strKey]);
                }
                if (strKey.Trim() != "id")
                {
                    if ((dicInputFields[strKey]).Trim() != "")
                    {
                        strSql += " [" + DAL.Sanitize(strKey) + "] = '" + DAL.Sanitize(dicInputFields[strKey]) + "' ,";
                    }
                }
            }
            strSql = strSql.Remove(strSql.LastIndexOf(','));
            int.Parse((dicInputFields["id"]).Trim()); //This will throw an Exception in case the id is not a valid number (or no id was supplied)
            strSql += " WHERE [id]='" + DAL.Sanitize((dicInputFields["id"]).Trim()) + "';";
            int intRecordsUpdated;
            intRecordsUpdated = DAL.UpdateOrDelete(strSql);
            return intRecordsUpdated;
        }

        public Message GetPersons()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            using (DataSet dataset = GetInner("Persons"))
            {
                //Do not send the passwords to the client:
                if (dataset.Tables.Count > 0)
                {
                    if (dataset.Tables[0].Columns.Contains("postOfficeBox"))
                    {
                        dataset.Tables[0].Columns.Remove("postOfficeBox");
                    }
                }
                string json = JsonConvert.SerializeObject(dataset);
                return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
            }
        }

        public Message GetChips()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Chips"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetAnimals()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(1);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Animals"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetPics()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Pics"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetLocations()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Locations"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetDocs()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Docs"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetDocumentTypes()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("DocumentTypes"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetTreatments()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Treatments"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetTreatmentTypes()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("TreatmentTypes"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetGenders()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(1);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Genders"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetSizes()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(1);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Sizes"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetSpecies()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(1);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Species"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public Message GetAdoptions()
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return Message.CreateMessage(MessageVersion.Soap11, "");    //SearchUserIdBySubmittedToken already closed the response
            }
            string json = JsonConvert.SerializeObject(GetInner("Adoptions"));
            return WebOperationContext.Current.CreateTextResponse(json, "application/json;charset=utf-8", System.Text.Encoding.UTF8);
        }

        public string DeleteAdoption(int intId)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner("Adoption", intId);
            return str;
        }

        private string DeleteInner(string strTableName, int intId)
        {
            try
            {
                string strSql = " DELETE FROM [tbl" + DAL.Sanitize(strTableName) + "s] WHERE [id]='" + DAL.Sanitize(intId.ToString()) + "'";
                int intDeletedRecordsCount;
                intDeletedRecordsCount = DAL.UpdateOrDelete(strSql);
                if (intDeletedRecordsCount == 1)
                {
                    return "Success";
                }
                else if (intDeletedRecordsCount > 1)
                {
                    return "Problem: more than one records were updated.";
                }
                else
                {
                    return "Problem: no records were updated. Make sure you sent the correct id";
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string DeleteDoc(int intId)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner("Doc", intId);
            return str;
        }

        public string UpdateDoc(Doc doc)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                string strSql;
                strSql = "UPDATE [tbldocs] SET ";
                if (doc.nameAndPathIncludingExtension != null)
                {
                    strSql += " [nameAndPathIncludingExtension]='" + DAL.Sanitize(doc.nameAndPathIncludingExtension.ToString()) + "', ";
                }
                if (doc.animal_ != null)
                {
                    strSql += " [animal_]='" + DAL.Sanitize(doc.animal_.ToString()) + "', ";
                }
                if (doc.documentType_ != null)
                {
                    strSql += " [documentType_]='" + DAL.Sanitize(doc.documentType_.ToString()) + "', ";
                }
                if (doc.adoption_ != null)
                {
                    strSql += " [adoption_]='" + DAL.Sanitize(doc.adoption_.ToString()) + "', ";
                }
                if (doc.treatment_ != null)
                {
                    strSql += " [treatment_]='" + DAL.Sanitize(doc.treatment_.ToString()) + "', ";
                }
                if (doc.remarks != null)
                {
                    strSql += " [remarks]='" + DAL.Sanitize(doc.remarks) + "', ";
                }
                strSql = strSql.Remove(strSql.LastIndexOf(','));
                if (doc.id == null)
                {
                    throw new Exception("You must supply an id");
                }

                //IMPORTANT                                      //IMPORTANT:
                strSql += " WHERE [id]='" + DAL.Sanitize(doc.id.ToString()) + "'";
                int intUpdatedRecordsCount;
                intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
                if (intUpdatedRecordsCount == 1)
                {
                    return "Success";
                }
                else if (intUpdatedRecordsCount > 1)
                {
                    return "Problem: more than one records were updated.";
                }
                else
                {
                    return "Problem: no records were updated. Make sure you sent the correct document id";
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertPic(Pic pic)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                string strFieldNames;
                string strValues;
                strFieldNames = "INSERT INTO [tblPics] (  ";
                strValues = "    )    VALUES (";
                if (pic.nameAndPathIncludingExtension != null)
                {
                    strFieldNames += "                      [nameAndPathIncludingExtension],";
                    strValues += "'" + DAL.Sanitize(pic.nameAndPathIncludingExtension) + "',";
                }
                if (pic.animal_ != null)
                {
                    strFieldNames += "                      [animal_],";
                    strValues += "'" + DAL.Sanitize(pic.animal_.ToString()) + "',";
                }
                if (pic.adoption_ != null)
                {
                    strFieldNames += "                    [adoption_],";
                    strValues += "'" + DAL.Sanitize(pic.adoption_.ToString()) + "',";
                }
                if (pic.treatment_ != null)
                {
                    strFieldNames += "                    [treatment_],";
                    strValues += "'" + DAL.Sanitize(pic.treatment_.ToString()) + "',";
                }
                if (pic.isArrivalPic != null)
                {
                    strFieldNames += "                    [isArrivalPic],";
                    strValues += "'" + DAL.Sanitize(pic.isArrivalPic.ToString()) + "',";
                }
                if (pic.takenOn != null)
                {
                    strFieldNames += "                    [takenOn],";
                    strValues += "'" + DAL.Sanitize(pic.takenOn.ToString()) + "',";
                }
                if (pic.hiddenFromPublic != null)
                {
                    strFieldNames += "                    [hiddenFromPublic],";
                    strValues += "'" + DAL.Sanitize(pic.hiddenFromPublic.ToString()) + "',";
                }
                if (pic.remarks != null)
                {
                    strFieldNames += "                    [remarks],";
                    strValues += "'" + DAL.Sanitize(pic.remarks) + "',";
                }
                strFieldNames = strFieldNames.Remove(strFieldNames.LastIndexOf(','));
                strValues = strValues.Remove(strValues.LastIndexOf(','));
                strValues += ");";
                string strSql = strFieldNames + strValues;
                DAL.Insert(strSql);
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string DeletePic(int intId)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner("Pic", intId);
            return str;
        }

        public string UpdatePic(Pic pic)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                string strSql;
                strSql = "UPDATE [tblpics] SET ";
                if (pic.nameAndPathIncludingExtension != null)
                {
                    strSql += " [nameAndPathIncludingExtension]='" + DAL.Sanitize(pic.nameAndPathIncludingExtension.ToString()) + "', ";
                }
                if (pic.animal_ != null)
                {
                    strSql += " [animal_]='" + DAL.Sanitize(pic.animal_.ToString()) + "', ";
                }
                if (pic.adoption_ != null)
                {
                    strSql += " [adoption_]='" + DAL.Sanitize(pic.adoption_.ToString()) + "', ";
                }
                if (pic.treatment_ != null)
                {
                    strSql += " [treatment_]='" + DAL.Sanitize(pic.treatment_.ToString()) + "', ";
                }
                if (pic.isArrivalPic != null)
                {
                    strSql += " [isArrivalPic]='" + DAL.Sanitize(pic.isArrivalPic.ToString()) + "', ";
                }
                if (pic.takenOn != null)
                {
                    strSql += " [takenOn]='" + DAL.Sanitize(pic.takenOn.ToString()) + "', ";
                }
                if (pic.hiddenFromPublic != null)
                {
                    strSql += " [hiddenFromPublic]='" + DAL.Sanitize(pic.hiddenFromPublic.ToString()) + "', ";
                }
                if (pic.remarks != null)
                {
                    strSql += " [remarks]='" + DAL.Sanitize(pic.remarks) + "', ";
                }
                strSql = strSql.Remove(strSql.LastIndexOf(','));
                if (pic.id == null)
                {
                    throw new Exception("You must supply an id");
                }

                //IMPORTANT                                      //IMPORTANT:
                strSql += " WHERE [id]='" + DAL.Sanitize(pic.id.ToString()) + "'";
                int intUpdatedRecordsCount;
                intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
                if (intUpdatedRecordsCount == 1)
                {
                    return "Success";
                }
                else if (intUpdatedRecordsCount > 1)
                {
                    return "Problem: more than one records were updated.";
                }
                else
                {
                    return "Problem: no records were updated. Make sure you sent the correct id";
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertTreatment(Treatment treatment)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (treatment.animal_ == null)
                {
                    throw new Exception("You must supply the animal");
                }
                string strFieldNames;
                string strValues;
                strFieldNames = "INSERT INTO [tblTreatments] (  ";
                strValues = "    )    VALUES (";
                if (treatment.animal_ != null)
                {
                    strFieldNames += "                      [animal_],";
                    strValues += "'" + DAL.Sanitize(treatment.animal_.ToString()) + "',";
                }
                if (treatment.treatmentType_ != null)
                {
                    strFieldNames += "                      [treatmentType_],";
                    strValues += "'" + DAL.Sanitize(treatment.treatmentType_.ToString()) + "',";
                }
                if (treatment.treatmentTime != null)
                {
                    strFieldNames += "                    [treatmentTime],";
                    strValues += "'" + DAL.Sanitize(treatment.treatmentTime.ToString()) + "',";
                }
                if (treatment.person_ != null)
                {
                    strFieldNames += "                    [person_],";
                    strValues += "'" + DAL.Sanitize(treatment.person_.ToString()) + "',";
                }
                if (treatment.medicineName != null)
                {
                    strFieldNames += "                    [medicineName],";
                    strValues += "'" + DAL.Sanitize(treatment.medicineName.ToString()) + "',";
                }
                if (treatment.medicineDosage != null)
                {
                    strFieldNames += "                    [medicineDosage],";
                    strValues += "'" + DAL.Sanitize(treatment.medicineDosage.ToString()) + "',";
                }
                if (treatment.thisIsRoundNumber != null)
                {
                    strFieldNames += "                    [thisIsRoundNumber],";
                    strValues += "'" + DAL.Sanitize(treatment.thisIsRoundNumber.ToString()) + "',";
                }
                if (treatment.nextRoundOn != null)
                {
                    strFieldNames += "                    [nextRoundOn],";
                    strValues += "'" + DAL.Sanitize(treatment.nextRoundOn.ToString()) + "',";
                }
                if (treatment.remarks != null)
                {
                    strFieldNames += "                    [remarks],";
                    strValues += "'" + DAL.Sanitize(treatment.remarks) + "',";
                }
                strFieldNames = strFieldNames.Remove(strFieldNames.LastIndexOf(','));
                strValues = strValues.Remove(strValues.LastIndexOf(','));
                strValues += ");";
                string strSql = strFieldNames + strValues;
                DAL.Insert(strSql);
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string DeleteTreatment(int intId)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner("Treatment", intId);
            return str;
        }

        public string UpdateTreatment(Treatment treatment)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                string strSql;
                strSql = "UPDATE [tbltreatments] SET ";
                if (treatment.animal_ != null)
                {
                    strSql += " [animal_]='" + DAL.Sanitize(treatment.animal_.ToString()) + "', ";
                }
                if (treatment.treatmentTime != null)
                    if (treatment.treatmentType_ != null)
                    {
                        strSql += " [treatmentType_]='" + DAL.Sanitize(treatment.treatmentType_.ToString()) + "', ";
                    }
                {
                    strSql += " [treatmentTime]='" + DAL.Sanitize(treatment.treatmentTime.ToString()) + "', ";
                }
                if (treatment.person_ != null)
                {
                    strSql += " [person_]='" + DAL.Sanitize(treatment.person_.ToString()) + "', ";
                }
                if (treatment.medicineName != null)
                {
                    strSql += " [medicineName]='" + DAL.Sanitize(treatment.medicineName) + "', ";
                }
                if (treatment.medicineDosage != null)
                {
                    strSql += " [medicineDosage]='" + DAL.Sanitize(treatment.medicineDosage.ToString()) + "', ";
                }
                if (treatment.thisIsRoundNumber != null)
                {
                    strSql += " [thisIsRoundNumber]='" + DAL.Sanitize(treatment.thisIsRoundNumber.ToString()) + "', ";
                }
                if (treatment.nextRoundOn != null)
                {
                    strSql += " [nextRoundOn]='" + DAL.Sanitize(treatment.nextRoundOn.ToString()) + "', ";
                }
                if (treatment.remarks != null)
                {
                    strSql += " [remarks]='" + DAL.Sanitize(treatment.remarks) + "', ";
                }
                strSql = strSql.Remove(strSql.LastIndexOf(','));
                if (treatment.id == null)
                {
                    throw new Exception("You must supply an id");
                }

                //IMPORTANT                                      //IMPORTANT:
                strSql += " WHERE [id]='" + DAL.Sanitize(treatment.id.ToString()) + "'";
                int intUpdatedRecordsCount;
                intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
                if (intUpdatedRecordsCount == 1)
                {
                    return "Success";
                }
                else if (intUpdatedRecordsCount > 1)
                {
                    return "Problem: more than one records were updated.";
                }
                else
                {
                    return "Problem: no records were updated. Make sure you sent the correct id";
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertTreatmentType(TreatmentType treatmentType)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                string strFieldNames;
                string strValues;
                strFieldNames = "INSERT INTO [tblTreatmentTypes] (  ";
                strValues = "    )    VALUES (";
                if (treatmentType.description != null)
                {
                    strFieldNames += "                      [description],";
                    strValues += "'" + DAL.Sanitize(treatmentType.description) + "',";
                }
                if (treatmentType.numberOfRoundsNeeded != null)
                {
                    strFieldNames += "                      [numberOfRoundsNeeded],";
                    strValues += "'" + DAL.Sanitize(treatmentType.numberOfRoundsNeeded.ToString()) + "',";
                }
                if (treatmentType.daysBetweenRounds != null)
                {
                    strFieldNames += "                    [daysBetweenRounds],";
                    strValues += "'" + DAL.Sanitize(treatmentType.daysBetweenRounds.ToString()) + "',";
                }
                strFieldNames = strFieldNames.Remove(strFieldNames.LastIndexOf(','));
                strValues = strValues.Remove(strValues.LastIndexOf(','));
                strValues += ");";
                string strSql = strFieldNames + strValues;
                DAL.Insert(strSql);
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string DeleteTreatmentType(int intId)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner("TreatmentType", intId);
            return str;
        }

        public string UpdateTreatmentType(TreatmentType treatmentType)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (treatmentType.description == null || treatmentType.description.Trim() == "")
                {
                    throw new Exception("You must supply a description (the name of the treatment type)");
                }
                string strSql;
                strSql = "UPDATE [tblTreatmentTypes] SET ";
                if (treatmentType.description != null)
                {
                    strSql += " [description]='" + DAL.Sanitize(treatmentType.description) + "', ";
                }
                if (treatmentType.numberOfRoundsNeeded != null)
                {
                    strSql += " [numberOfRoundsNeeded]='" + DAL.Sanitize(treatmentType.numberOfRoundsNeeded.ToString()) + "', ";
                }
                if (treatmentType.daysBetweenRounds != null)
                {
                    strSql += " [daysBetweenRounds]='" + DAL.Sanitize(treatmentType.daysBetweenRounds.ToString()) + "', ";
                }
                strSql = strSql.Remove(strSql.LastIndexOf(','));
                if (treatmentType.id == null)
                {
                    throw new Exception("You must supply an id");
                }

                //IMPORTANT                                      //IMPORTANT:
                strSql += " WHERE [id]='" + DAL.Sanitize(treatmentType.id.ToString()) + "'";
                int intUpdatedRecordsCount;
                intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
                if (intUpdatedRecordsCount == 1)
                {
                    return "Success";
                }
                else if (intUpdatedRecordsCount > 1)
                {
                    return "Problem: more than one records were updated.";
                }
                else
                {
                    return "Problem: no records were updated. Make sure you sent the correct id";
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertLocation(Location location)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (location.id != null)
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if (location.description == null || location.description.Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                string strSql;
                strSql = "INSERT INTO [tblLocations] ( [description] VALUES ('" + DAL.Sanitize(location.description) + "');";
                // strFieldNames = strFieldNames.Remove(strFieldNames.LastIndexOf(','));
                // strValues = strValues.Remove(strValues.LastIndexOf(','));
                DAL.Insert(strSql);
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string DeleteLocation(int intId)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner("Location", intId);
            return str;
        }

        public string UpdateLocation(Location location)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (location.id == null)
                {
                    throw new Exception("You must supply an id");
                }
                if (location.description == null || location.description.Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                string strSql;
                strSql = "UPDATE [tblLocations] SET ";
                if (location.description != null)
                {
                    strSql += " [description]='" + DAL.Sanitize(location.description) + "' ";
                }
                //IMPORTANT                                      //IMPORTANT:
                strSql += " WHERE [id]='" + DAL.Sanitize(location.id.ToString()) + "'";
                int intUpdatedRecordsCount;
                intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
                if (intUpdatedRecordsCount == 1)
                {
                    return "Success";
                }
                else if (intUpdatedRecordsCount > 1)
                {
                    return "Problem: more than one records were updated.";
                }
                else
                {
                    return "Problem: no records were updated. Make sure you sent the correct id";
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertRole(Role role)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (role.id != null)
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if (role.description == null || role.description.Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                string strSql;
                strSql = "INSERT INTO [tblRoles] ( [description] VALUES ('" + DAL.Sanitize(role.description) + "');";
                // strFieldNames = strFieldNames.Remove(strFieldNames.LastIndexOf(','));
                // strValues = strValues.Remove(strValues.LastIndexOf(','));
                DAL.Insert(strSql);
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string DeleteRole(int intId)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner("Role", intId);
            return str;
        }

        public string UpdateRole(Role role)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (role.id == null)
                {
                    throw new Exception("You must supply an id");
                }
                if (role.description == null || role.description.Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                string strSql;
                strSql = "UPDATE [tblRoles] SET ";
                if (role.description != null)
                {
                    strSql += " [description]='" + DAL.Sanitize(role.description) + "' ";
                }
                //IMPORTANT                                      //IMPORTANT:
                strSql += " WHERE [id]='" + DAL.Sanitize(role.id.ToString()) + "'";
                int intUpdatedRecordsCount;
                intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
                if (intUpdatedRecordsCount == 1)
                {
                    return "Success";
                }
                else if (intUpdatedRecordsCount > 1)
                {
                    return "Problem: more than one records were updated.";
                }
                else
                {
                    return "Problem: no records were updated. Make sure you sent the correct id";
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertSize(Size size)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (size.id != null)
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if (size.description == null || size.description.Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                string strSql;
                strSql = "INSERT INTO [tblSizes] ( [description] VALUES ('" + DAL.Sanitize(size.description) + "');";
                // strFieldNames = strFieldNames.Remove(strFieldNames.LastIndexOf(','));
                // strValues = strValues.Remove(strValues.LastIndexOf(','));
                DAL.Insert(strSql);
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string DeleteSize(int intId)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner("Size", intId);
            return str;
        }

        public string UpdateSize(Size size)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(5);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (size.id == null)
                {
                    throw new Exception("You must supply an id");
                }
                if (size.description == null || size.description.Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                string strSql;
                strSql = "UPDATE [tblSizes] SET ";
                if (size.description != null)
                {
                    strSql += " [description]='" + DAL.Sanitize(size.description) + "' ";
                }
                //IMPORTANT                                      //IMPORTANT:
                strSql += " WHERE [id]='" + DAL.Sanitize(size.id.ToString()) + "'";
                int intUpdatedRecordsCount;
                intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
                if (intUpdatedRecordsCount == 1)
                {
                    return "Success";
                }
                else if (intUpdatedRecordsCount > 1)
                {
                    return "Problem: more than one records were updated.";
                }
                else
                {
                    return "Problem: no records were updated. Make sure you sent the correct id";
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string InsertSpecies(Species species)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (species.id != null)
                {
                    throw new Exception("Leave the id blank when inserting");
                }
                if (species.description == null || species.description.Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                string strSql;
                strSql = "INSERT INTO [tblSpecies] ( [description] VALUES ('" + DAL.Sanitize(species.description) + "');";
                // strFieldNames = strFieldNames.Remove(strFieldNames.LastIndexOf(','));
                // strValues = strValues.Remove(strValues.LastIndexOf(','));
                DAL.Insert(strSql);
                return "Success";
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        public string DeleteSpecies(int intId)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            string str = DeleteInner("Specie", intId);
            return str;
        }

        public string UpdateSpecies(Species species)
        {
            try
            {
                int intUserId = SearchUserIdBySubmittedToken(9);
            }
            catch (Exception ex)
            {
                return "";    //SearchUserIdBySubmittedToken already closed the response
            }
            try
            {
                if (species.id == null)
                {
                    throw new Exception("You must supply an id");
                }
                if (species.description == null || species.description.Trim() == "")
                {
                    throw new Exception("You must supply a description");
                }
                string strSql;
                strSql = "UPDATE [tblSpecies] SET ";
                if (species.description != null)
                {
                    strSql += " [description]='" + DAL.Sanitize(species.description) + "' ";
                }
                //IMPORTANT                                      //IMPORTANT:
                strSql += " WHERE [id]='" + DAL.Sanitize(species.id.ToString()) + "'";
                int intUpdatedRecordsCount;
                intUpdatedRecordsCount = DAL.UpdateOrDelete(strSql);
                if (intUpdatedRecordsCount == 1)
                {
                    return "Success";
                }
                else if (intUpdatedRecordsCount > 1)
                {
                    return "Problem: more than one records were updated.";
                }
                else
                {
                    return "Problem: no records were updated. Make sure you sent the correct id";
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage;
                strErrorMessage = SendErrorSpecifyingForeignKeyFieldName(ex);
                return strErrorMessage;
            }
        }

        //public string DoPostOperation(Stream stream)
        //{
        //    string strResult;
        //    using (StreamReader streamreader = new StreamReader(stream))
        //    {
        //        strResult = streamreader.ReadToEnd();
        //        streamreader.Close();
        //        streamreader.Dispose();
        //    }
        //    stream.Close();
        //    stream.Dispose();
        //    return strResult;
        //    //
        //}

        //public string Ping(Stream s)
        //{
        //    s.Close();
        //    s.Dispose();
        //    return "Hello";
        //}

        //public string GetTEST()
        //{
        //    //byte[] buffer = new byte[1000];
        //    string f = "";
        //    foreach (string str in HttpContext.Current.Request.QueryString.AllKeys)
        //    {
        //        f += str + "=" + HttpContext.Current.Request.QueryString[str] + ";";
        //    }
        //    //SaveAs(@"C:\Users\Meir\destop\kjhgf.txt",false);
        //    //using (StreamReader s = new StreamReader(HttpContext.Current.Request.InputStream))
        //    //{
        //    //  f=s.ReadToEnd();
        //    //s.Close();
        //    //       s.Dispose();
        //    // }

        //    return f + "asd";
        //}

        //public CompositeType GetDataUsingDataContract(CompositeType composite)

        //{
        //    if (composite == null)
        //    {
        //        throw new ArgumentNullException("composite");
        //    }
        //    if (composite.BoolValue)
        //    {
        //        composite.StringValue += "Suffix";
        //    }
        //    return composite;
        //}
    }
}
