
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace System
{

    public struct Machine
    {

        public static string MachineId
        {
            get
            {
                return LicensedMachine.CalcMachineIdNumber(Machine.ProcessorId, Machine.BaseBoardId, Machine.MacAddress);
            }
        }

        public static string LicencedMachineDsc
        {
            get
            {
                return Machine.MachineDescription + "; " + Machine.ComputerName + "; CPU: " + Machine.ProcessorName;
            }
        }





        private static ManagementObject mProcessor = GetManagementObject("Win32_Processor"); //todo: check Win64_Processor?
        public static ManagementObject Processor
        {
            get { return mProcessor; }
        }

        private static ManagementObject mBaseBoard = GetManagementObject("Win32_BaseBoard");//todo: check Win64_Processor?
        public static ManagementObject BaseBoard
        {
            get { return mBaseBoard; }
        }

        private static ManagementObject mNetworkAdapter = GetManagementObject("Win32_NetworkAdapterConfiguration");
        public static ManagementObject NetworkAdapter
        {
            get { return mNetworkAdapter; }
        }

        private static ManagementObject mComputerSystem = GetManagementObject("Win32_ComputerSystem");
        public static ManagementObject ComputerSystem
        {
            get { return mComputerSystem; }
        }

        private static ManagementObject GetManagementObject(string path)
        {
            try
            {
                ManagementClass mngClass = new ManagementClass(path);
                ManagementObjectCollection objColl = mngClass.GetInstances();

                System.Collections.IEnumerator e = objColl.GetEnumerator();
                if (e.MoveNext())
                {
                    ManagementObject obj = (ManagementObject)e.Current;
                    obj.Get();
                    return obj;
                }
            }
            catch (Exception)
            {
                // return null;
            }
            return null;
        }

        private static string ValToStr(ManagementObject obj, string propName)
        {
            try
            {
                if (obj == null)
                    return "";
                if (obj[propName] == null)
                    return "";
                var res = obj[propName].ToString().Trim();
                res.Replace('\t', ' ');
                res = RemoveDoubleSpaces(res);
                return res;
            }
            catch (Exception)
            {
            }
            return "";
        }

        private static string RemoveDoubleSpaces(string s)
        {
            do
            {
                s = s.Replace("  ", " ");
            } while (s.Contains("  "));

            return s;
        }

        public static string ProcessorId
        {
            get
            {
                return
                    "uid-" + ValToStr(Processor, "UniqueId")
                    + "_pid-" + ValToStr(Processor, "ProcessorId")
                    + "_man-" + ValToStr(Processor, "Manufacturer")
                    + "_n-" + ValToStr(Processor, "Name")
                    + "_sp-" + ValToStr(Processor, "MaxClockSpeed");
            }
        }

        public static string ProcessorName
        {
            get
            {
                string s = ValToStr(Processor, "Name") + " - " + ValToStr(Processor, "MaxClockSpeed");
                return s;
            }
        }

        public static string ComputerName
        {
            get
            {
                //return Environment.MachineName;  //Uruchomione Przed Application.Run wywala błąd
                return ValToStr(Processor, "SystemName");
            }
        }

        public static string BaseBoardId
        {
            get
            {
                string res = ValToStr(BaseBoard, "SerialNumber");
                res = res.Trim().Trim('0').Trim(); // czasem SerialNumber = 00000000
                if (res.IsEmpty())
                {
                    res = ValToStr(BaseBoard, "Manufacturer") + "."
                            + ValToStr(BaseBoard, "Name");
                }
                var model = ValToStr(BaseBoard, "Model");
                if (!model.IsEmpty())
                    res = res + "." + model;
                return res;
            }
        }

        public static string MachineDescription
        {
            get
            {
                return ValToStr(ComputerSystem, "Manufacturer") + ", " + ValToStr(ComputerSystem, "Model");
            }
        }

        public static string MacAddress
        {
            get { return ValToStr(NetworkAdapter, "MacAddress"); }
        }

    }
}
