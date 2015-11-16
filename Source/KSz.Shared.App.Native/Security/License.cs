using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace System
{
    public class License
    {
        private XmlDocument xmlDoc;
        public const string Demo = "Demo";
        public const string Beta = "Beta";
        public const string Commercial = "Commercial";
        public const string Geo7Tools = "Geo7";

        public License(string productName, string version, string machineId)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <Licence>
                <Signature></Signature>
                <Values>
                    <ProductName></ProductName>
                    <Version></Version>
                    <Edition></Edition>
                    <Type></Type>
                    <SerialNo></SerialNo>
                    <MachineId></MachineId>
                    <Modules></Modules>
                    <ExpirationDate>2999-01-01</ExpirationDate>
                    <Limit></Limit>
                </Values>
                <UserName></UserName>
            </Licence>");
            this.ProductName = productName;
            this.Version = version;
            this.MachineId = machineId;
            this.ExpirationDate = new DateTime(1900, 1, 1);
        }


        /// <summary>
        /// For internal use only
        /// </summary>
        /// <param name="fileName"></param>
        private License(string fileName)
        {
            xmlDoc = new XmlDocument();
            if (!File.Exists(fileName))
                throw new FileNotFoundException();
            xmlDoc.Load(fileName);
        }


        /// <summary>
        /// Inits all required License properties, without signing license
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="type"></param>
        /// <param name="edition"></param>
        /// <param name="limit"></param>
        /// <param name="modules"></param>
        /// <param name="expDate"></param>
        /// <param name="serialNo"></param>
        /// <param name="signature"></param>
        public void Init(string userName, string type, string edition, string limit, string modules, DateTime expDate, string serialNo)
        {
            this.Edition = edition;
            this.ExpirationDate = expDate;
            this.Limit = limit;
            this.Modules = modules;
            this.SerialNo = serialNo;
            this.Type = type;
            this.UserName = userName;
        }

        /// <summary>
        /// Inits all required License properties, without signing license
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="type"></param>
        /// <param name="edition"></param>
        /// <param name="limit"></param>
        /// <param name="modules"></param>
        public void InitDemo(string userName)
        {
            DateTime expDate = DateTime.Now.AddMonths(3);
            string serialNo = GenerateNewSerial();

            Init(userName, Demo, "", "", "", expDate, serialNo);
        }

        /// <summary>
        /// Inits all required License properties, without signing license
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="type"></param>
        /// <param name="edition"></param>
        /// <param name="limit"></param>
        /// <param name="modules"></param>
        public void InitBeta(string userName)
        {
            DateTime expDate = DateTime.Now.AddYears(2);
            string serialNo = GenerateNewSerial();

            Init(userName, Beta, "", "", "", expDate, serialNo);
        }

        public void Import(string fileName)
        {
            var fileLic = new License(fileName);

            if (this.ProductName != fileLic.ProductName)
                throw new Exception(this.FullProductName + ": " + SysUtils.Strings.InvalidImpLicProduct + " (" + fileLic.FullProductName + ")");

            if (this.Version != fileLic.Version)
                throw new Exception(this.FullProductName + ": " + SysUtils.Strings.InvalidImpLicVersion + " (" + fileLic.FullProductName + ")");

            if (this.MachineId != fileLic.MachineId)
                throw new Exception(this.FullProductName + " (" + this.MachineId + "): " + SysUtils.Strings.InvalidImpLicMachine + " (" + fileLic.MachineId + ")");

            if (!fileLic.SignatureValid)
                throw new Exception(this.FullProductName + ": " + SysUtils.Strings.InvalidLicKey);

            if (fileLic.Expired)
                throw new Exception(this.FullProductName + ": " + SysUtils.Strings.LicenseExpired);

            this.xmlDoc.LoadXml(fileLic.Xml);
        }


        public static License GetLicense(string machineId, string productName, string version)
        {
            License res = LicenseList.Get(machineId, productName, version);

            if (res == null)
            {
                res = new License(machineId, productName, version);
                res.ImportFromAppDataFolder();
            }
            return res;
        }


        public bool IsDeveloperLicense
        {
            get { return this.MachineId.IsIn("170567-292"); }
        }

        private XmlElement Values
        {
            get { return xmlDoc.DocumentElement["Values"]; }
        }

        public string this[string name]
        {
            get
            {
                if (Values[name] != null)
                    return Values[name].InnerXml;
                return "";
            }
            set { Values[name].InnerXml = value; }
        }

        public string ProductName
        {
            get { return this["ProductName"]; }
            private set { this["ProductName"] = value; }
        }

        public string Version
        {
            get { return this["Version"]; }
            private set { this["Version"] = value ?? ""; }
        }

        public string MachineId
        {
            get { return this["MachineId"]; }
            private set { this["MachineId"] = value.Trim(); }
        }

        public string SerialNo
        {
            get { return this["SerialNo"]; }
            set { this["SerialNo"] = value; }
        }

        public string Signature
        {
            get { return xmlDoc.DocumentElement["Signature"].InnerXml; }
            private set { xmlDoc.DocumentElement["Signature"].InnerXml = value; }
        }

        [Description("Np. GeoSoft, Warszawa")]
        ///GeoSoft, Warszawa
        public string UserName
        {
            get { return xmlDoc.DocumentElement["UserName"].InnerXml; }
            set { xmlDoc.DocumentElement["UserName"].InnerXml = value; }
        }

        [Description("Standart, Pro, Enterprice")]
        ///Standart, Pro, Enterprice
        public string Edition
        {
            get { return this["Edition"]; }
            set { this["Edition"] = value; }
        }

        [Description("Demo, Edu, Beta, Commercial")]
        /// Demo, Edu, Beta, Commercial
        public string Type
        {
            get { return this["Type"]; }
            set { this["Type"] = value; }
        }

        private const string dateFormat = "yyyy-MM-dd";
        public DateTime ExpirationDate
        {
            get { return DateTime.ParseExact(this["ExpirationDate"], dateFormat, CultureInfo.InvariantCulture); }
            set { this["ExpirationDate"] = value.ToString(dateFormat, CultureInfo.InvariantCulture); }
        }

        public string Limit
        {
            get { return this["Limit"]; }
            set { this["Limit"] = value; }
        }

        public string Modules
        {
            get { return this["Modules"]; }
            set { this["Modules"] = value; }
        }


        public bool IsDemo
        {
            get { return Type == Demo; }
        }

        public string GetSignatureData()
        {
            string res = "";
            foreach (XmlElement xEl in Values)
            {
                res = res + xEl.InnerXml;
            }
            return res;
        }

        /// <summary>
        /// Sprawdza tylko czy sama sygnatura jest OK. Nie sprawdza 'Edition' ani 'ExpirationDate'.
        /// </summary>
        /// <returns></returns>
        public bool SignatureValid
        {
            get
            {
                return Crypt.DsaVerify(GetSignatureData(), Signature);
            }
        }

        /// <summary>
        /// Licencja może być ok, ale może upłynąć termin - licencja wygasła.
        /// </summary>
        public bool Expired
        {
            get
            {
                DateTime now = DateTime.Now;
                //string counterFile = Path.ChangeExtension(UserLicFileName, ".dat");
                //try
                //{ 
                //    string lastDate = File.ReadAllText(counterFile);
                //    now = new DateTime(Convert.ToInt64(lastDate));
                //}
                //catch (Exception)
                //{
                //}
                //if (DateTime.Now >= now)
                //{
                //    File.WriteAllText(counterFile, DateTime.Now.Ticks.ToString());
                //    //File.SetAttributes(counterFile, FileAttributes.Hidden);
                //    now = DateTime.Now;
                //}
                return (now > ExpirationDate);
            }
        }

        public bool IsValid
        {
            get
            {
                // Sprawdź czy w ogóle nr seryjny jest ok. Po co?

                return SignatureValid && !Expired;
            }
        }

        public bool IsLicensed(string module)
        {
            return Modules.ToLower().Contains(module.ToLower()) || IsDemo;
        }

        public string StatusText
        {
            get
            {
                if (IsValid)
                    return SysUtils.Strings.LicenseIsValid;
                if (!SignatureValid)
                    return SysUtils.Strings.LicenseIsInvalid;
                if (Expired)
                    return SysUtils.Strings.LicenseIsExpired;

                return SysUtils.Strings.LicenseIsInvalid;
            }
        }

        /// <summary>
        /// Sign this licence
        /// </summary>
        /// <param name="dsa">Private DSA Key</param>
        //public void Sign(DSACryptoServiceProvider dsa)
        //{
        //    Signature = Crypt.DsaSign(GetSignatureData(), dsa);
        //}

        public void Sign(string signature)
        {
            Signature = signature;
        }

        public void SaveToFile(string fileName)
        {
            var dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            xmlDoc.Save(fileName);
        }

        public string Xml
        {
            get
            {
                return xmlDoc.OuterXml;
            }
        }

        private string CommonLicFileName()
        {
            return GetLicFileName(Environment.SpecialFolder.CommonApplicationData);
        }

        private string UserLicFileName()
        {
            return GetLicFileName(Environment.SpecialFolder.LocalApplicationData);
        }

        private string GetLicFileName(Environment.SpecialFolder folder)
        {
            var appDataPath = Environment.GetFolderPath(folder) + @"\GeoSoft\Licenses\";

            var file = appDataPath + ProductName;
            //if (!string.IsNullOrEmpty(Type))
            //    file = file + "-" + Type;
            if (!string.IsNullOrEmpty(Version))
                file = file + " " + Version;
            return file.Trim() + ".lic";
        }

        public void SaveToAppDataFolder()
        {
            try
            {
                SaveToFile(CommonLicFileName()); // Spróbuj zapisać w lokalizacji dostępnej dla wszystkich
            }
            catch { }
            try
            {
                SaveToFile(UserLicFileName()); // Na wszelki wypadek zapisz też w danych użytkownika
            }
            catch { }
        }

        public void ImportFromAppDataFolder()
        {
            try
            {
                Import(CommonLicFileName()); // Spróbuj wczytać z lokalizacji dostępnej dla wszystkich
            }
            catch
            {
                try
                {
                    Import(UserLicFileName()); // Jeżeli nie ma praw dostępu, wczytaj z danych użytkownika
                }
                catch { }
            }
        }


        public string GenerateNewSerial()
        {
            string id = Crypt.ChSumAlphaNum(DateTime.Now.Ticks.ToString(), 8);
            return id + "-" + ChSumSerial(id);
        }

        private string ChSumSerial(string idText)
        {
            return Crypt.ChSumAlphaNum(ProductName + "." + Version + "->" + idText, 3);
        }

        public bool SerialValid(string key)
        {
            string[] values = key.Split('-');
            if (values.Length != 2)
                return false;
            return ChSumSerial(values[0]) == values[1];
        }

        public string FullProductName
        {
            get { return ProductName + " " + Version + " " + Edition; }
        }

    }

    public class LicenseUrls
    {
        public const string ProductNameKey = "pn";
        public const string ProductVersionKey = "pv";
        public const string MachineIdKey = "mid";
        public const string MachineDscKey = "mdsc";
        public const string EmailKey = "email";
        public const string LicTypeKey = "lt";
        public const string EditionKey = "ed";
        public const string ModulesKey = "mod";

        public const string ProcIdKey = "cpuid";
        public const string BaseboardIdKey = "bbid";
        public const string MacKey = "mac";
        public const string AppVersionKey = "appv";




        //public const string LicGenUrl = "http://localhost:51234/get.aspx?";
        public readonly string GetLicUrl = "http://novotive.com/lic/get.aspx?";
        public readonly string BuyLicUrl = "http://novotive.com/lic/Buy.aspx?";

        private NameValueCollection Params = new NameValueCollection();


        public LicenseUrls(NameValueCollection urlParams)
        {
            Params = urlParams;
            CheckParams();
        }

        public LicenseUrls(License lic, string machineDsc, string email, string processorId, string baseBoardId, string macAddress, string appVersion, bool localRequest)
        {
            Params[MachineIdKey] = lic.MachineId;
            Params[MachineDscKey] = machineDsc;
            Params[EmailKey] = email;

            Params[ProductNameKey] = lic.ProductName;
            Params[ProductVersionKey] = lic.Version;
            Params[LicTypeKey] = lic.Type;
            Params[EditionKey] = lic.Edition;
            Params[ModulesKey] = lic.Modules;


            Params[ProcIdKey] = processorId;
            Params[BaseboardIdKey] = baseBoardId;
            Params[MacKey] = macAddress;
            Params[AppVersionKey] = appVersion;

            if (localRequest)
            {
                GetLicUrl = "http://localhost:51234/get.aspx?";
                BuyLicUrl = "http://localhost:51234/get.aspx?";
            }

            CheckParams();
        }

        private void CheckParams()
        {
            if (string.IsNullOrEmpty(Params[MachineIdKey]))
                throw new KeyNotFoundException("Machine Id not set");

            //Zawsze sprawdzaj MachineId z MachineDsc
            if (string.IsNullOrEmpty(Params[MachineDscKey]))
                throw new KeyNotFoundException("Machine description not set");

            if (string.IsNullOrEmpty(Params[EmailKey]))
                throw new KeyNotFoundException("Email not set");

            if (string.IsNullOrEmpty(Params[ProductNameKey]))
                throw new KeyNotFoundException("Product Name not set");

            //Version może być empty
            //if (string.IsNullOrEmpty(Params[ProductVersionKey]))
            //    throw new KeyNotFoundException("Product Version not set");
        }

        public string GetParam(string paramName, string errMsgIfEmpty)
        {
            var res = Params[paramName];
            if (string.IsNullOrEmpty(res) && !string.IsNullOrEmpty(errMsgIfEmpty))
                throw new Exception(errMsgIfEmpty);
            return res;
        }

        public string MachineId
        {
            get { return Params[MachineIdKey]; }
        }

        public string MachineDsc
        {
            get { return Params[MachineDscKey]; }
        }

        public string Email
        {
            get { return Params[EmailKey]; }
        }

        public string ProductName
        {
            get { return Params[ProductNameKey]; }
        }


        public string ProductVersion
        {
            get { return Params[ProductVersionKey] ?? ""; }
        }


        public string AppVersion
        {
            get { return Params[AppVersionKey ?? "0.0.0.0"]; }
        }


        public string ProcessorId
        {
            get { return Params[ProcIdKey]; }
        }

        public string BaseboardId
        {
            get { return Params[BaseboardIdKey]; }
        }

        public string MacAddress
        {
            get { return Params[MacKey]; }
        }

        public string LicType
        {
            get { return Params[LicTypeKey]; }
        }

        // ---  Urls  ---

        public string GetLicenseUrl
        {
            get { return GetGetLicenseUrl(null); }
        }

        public string GenBetaLicenseUrl
        {
            get { return GetGetLicenseUrl(License.Beta); }
        }

        public string GenDemoLicenseUrl
        {
            get { return GetGetLicenseUrl(License.Demo); }
        }

        private string GetGetLicenseUrl(string licType)
        {
            Params[LicTypeKey] = licType;
            return GetLicUrl + GetParamsUrlPart(ProductNameKey, ProductVersionKey, LicTypeKey,
                EmailKey, MachineIdKey, ProcIdKey, BaseboardIdKey, MacKey, AppVersionKey, MachineDscKey);
        }

        public string BuyLicenseUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private string GetParamsUrlPart(params string[] paramNames)
        {
            string res = "";
            foreach (var pn in paramNames)
            {
                var pv = Uri.EscapeDataString(Params[pn]); // HttpUtility.UrlEncode(Params[pn]);
                if (!string.IsNullOrEmpty(pv))
                    res += pn + "=" + pv + "&";
            }
            return res.TrimEnd('&');
        }
    }

    // IdMachnie jest podpisany 3cyfrową sumą kotrolną
    public class LicensedMachine
    {

        XmlDocument xmlDoc;

        public LicensedMachine()
        {
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <Machine>
                <MachineDsc></MachineDsc>
                <MachineId></MachineId>
            </Machine>");
        }

        public string this[string name]
        {
            get { return xmlDoc.DocumentElement[name].InnerXml; }
            set { xmlDoc.DocumentElement[name].InnerXml = value; }
        }

        public string MachineDsc
        {
            get { return this["MachineDsc"]; }
            set { this["MachineDsc"] = value; }
        }

        public string MachineId
        {
            get { return this["MachineId"]; }
            set { this["MachineId"] = value; }
        }


        private static string IdNumber(string procId, string baseBoardId, string macAddress)
        {
            return Crypt.ChSumAlphaNum(string.Format("<{0}+{1}+{2}>", procId, baseBoardId, macAddress));
        }

        private static string ChSumNumber(string idText)
        {
            return Crypt.ChSumAlphaNum(idText, 3);
        }

        public static string CalcMachineIdNumber(string procId, string baseBoardId, string macAddress)
        {
            string idText = IdNumber(procId, baseBoardId, macAddress);
            return idText + "-" + ChSumNumber(idText); ;
        }

        public static bool IdValid(string id)
        {
            string[] values = id.Split('-');
            if (values.Length != 2)
                return false;
            return ChSumNumber(values[0]) == values[1];
        }

        public void Export(string fileName)
        {
            xmlDoc.Save(fileName);
        }

        public void Import(string fileName)
        {
            xmlDoc.Load(fileName);
        }

    }



    public class LicenseList
    {
        private static Dictionary<string, License> Dict = new Dictionary<string, License>();

        static LicenseList()
        {
            Add("machineId", "GeoWorker", "1", "Pro", "serial", "100", "2999-01-01", "licKey");
        }

        private static string GetKey(string machineId, string productName, string productVersion)
        {
            return string.Format("{0}@{1} V.{2}", machineId, productName, productVersion);
        }

        private static void Add(string machineId, string productName, string productVersion, string edition,
            string serialNo, string limit, string expDate, string signature)
        {
            License lic = new License(productName, productVersion, machineId);
            string key = GetKey(machineId, productName, productVersion);

            lic.Edition = edition;
            lic.Type = License.Commercial;
            lic.SerialNo = serialNo;
            lic.ExpirationDate = Date.Parse(expDate);
            lic.Limit = limit;
            lic.Sign(signature);

            Dict[key] = lic;
        }

        public static License Get(string machineId, string productName, string productVersion)
        {
            string key = GetKey(machineId, productName, productVersion);
            if (Dict.ContainsKey(key))
                return Dict[key];
            return null;
        }
    }
}
