using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System
{

    public abstract class TercRecord
    {
        internal TercRecord(string nazwa, string nazDod)
        {
            Nazwa = nazwa;
            NazDod = nazDod;
        }

        public string Nazwa { get; set; }
        public string NazDod { get; private set; }

        public abstract string Id { get; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(NazDod))
                return Nazwa;
            return Nazwa + ", " + NazDod;
        }

    }

    public class TercWoj : TercRecord
    {
        internal TercWoj(string woj, string nazwa, string nazDod)
            : base(nazwa, nazDod)
        {
            Woj = woj;
            PowList = new List<TercPow>();
            GmiList = new List<TercGmi>();
            ObrList = new List<TercObr>();
        }

        public string Woj { get; private set; }

        public List<TercPow> PowList { get; private set; }
        public List<TercGmi> GmiList { get; private set; }
        public List<TercObr> ObrList { get; private set; }

        public override string Id
        {
            get { return Woj; }
        }

    }

    public class TercPow : TercRecord
    {
        internal TercPow(TercWoj woj, string pow, string nazwa, string nazDod)
            : base(nazwa, nazDod)
        {
            Woj = woj;
            Pow = pow;
            GmiList = new List<TercGmi>();
            ObrList = new List<TercObr>();
        }

        public TercWoj Woj { get; private set; }
        public string Pow { get; private set; }
        public List<TercGmi> GmiList { get; private set; }
        public List<TercObr> ObrList { get; private set; }


        public override string Id
        {
            get { return Woj.Id + Pow; }
        }
    }

    public class TercGmi : TercRecord
    {
        internal TercGmi(TercWoj woj, TercPow pow, string gmi, string rodz, string nazwa, string nazDod)
            : base(nazwa, nazDod)
        {
            Woj = woj;
            Pow = pow;
            Gmi = gmi;
            Rodz = rodz;
            ObrList = new List<TercObr>();
        }

        public TercWoj Woj { get; private set; }
        public TercPow Pow { get; private set; }
        public string Gmi { get; private set; }
        public string Rodz { get; private set; }
        public List<TercObr> ObrList { get; private set; }

        public override string Id
        {
            get { return Pow.Id + Gmi + "_" + Rodz; }
        }


        public string RodzText
        {
            get
            {
                if (Rodz == "1")
                    return "gmina miejska";
                else if (Rodz == "2")
                    return "gmina wiejska";
                else if (Rodz == "3")
                    return "gmina miejsko-wiejska";
                else if (Rodz == "4")
                    return "miasto w gminie miejsko-wiejskiej";
                else if (Rodz == "5")
                    return "obszar wiejski w gminie miejsko-wiejskiej";
                else if (Rodz == "8")
                    return "dzielnice gminy Warszawa-Centrum";
                else if (Rodz == "9")
                    return "delegatury i dzielnice innych gmin miejskich";
                return null;
            }
        }

        public string NazwaRodz
        {
            get { return Nazwa + ", " + NazDod; }
        }
    }

    public class TercObr : TercRecord
    {
        internal TercObr(TercWoj woj, TercPow pow, TercGmi gmi, int obr, string nazwa, bool podzNaArk) : base(nazwa, null)
        {
            this.Woj = woj;
            this.Pow = pow;
            this.Gmi = gmi;
            this.Obr = obr;
            this.PodzNaAr = podzNaArk;
        }

        public bool PodzNaAr { get; internal set; }
        public int Obr { get; internal set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Nazwa, Obr.ToString("0000"));
        }

        public TercWoj Woj { get; private set; }
        public TercPow Pow { get; private set; }
        public TercGmi Gmi { get; private set; }

        public override string Id
        {
            //"000000_0.0000.0"
            get { return Gmi.Id + "." + Obr.ToString("0000"); }
        }

    }

    public class Terc
    {

        public static Terc Records = new Terc2011();

        public readonly List<TercWoj> WojList = new List<TercWoj>();

        public readonly Dictionary<string, TercWoj> WojDict = new Dictionary<string, TercWoj>();
        public readonly Dictionary<string, TercPow> PowDict = new Dictionary<string, TercPow>();
        public readonly Dictionary<string, TercGmi> GmiDict = new Dictionary<string, TercGmi>();
        public readonly Dictionary<string, TercObr> ObrDict = new Dictionary<string, TercObr>();
        public readonly Dictionary<string, TercRecord> Dict = new Dictionary<string, TercRecord>();

        private T GetDictValue<T>(Dictionary<string, T> dict, string key)
        {
            T res;
            if (dict.TryGetValue(key, out res))
                return res;
            else
                throw new Exception(typeof(T).Name + " with Id='" + key + "' not found.");
        }

        public TercWoj GetWoj(string id)
        {
            return GetDictValue<TercWoj>(WojDict, id);
        }

        public TercPow GetPow(string id)
        {
            return GetDictValue<TercPow>(PowDict, id);
        }

        public TercGmi GetGmi(string id)
        {
            return GetDictValue<TercGmi>(GmiDict, id);
        }

        public TercObr GetObr(string id)
        {
            return GetDictValue<TercObr>(ObrDict, id);
        }

        private T TryGetDictValue<T>(Dictionary<string, T> dict, string key)
        {
            T res;
            if (dict.TryGetValue(key, out res))
                return res;
            else
                return default(T);
        }

        public TercWoj TryGetWoj(string id)
        {
            return TryGetDictValue<TercWoj>(WojDict, id);
        }

        public TercPow TryGetPow(string id)
        {
            return TryGetDictValue<TercPow>(PowDict, id);
        }

        public TercGmi TryGetGmi(string id)
        {
            return TryGetDictValue<TercGmi>(GmiDict, id);
        }

        public TercObr TryGetObr(string id)
        {
            var res = TryGetDictValue<TercObr>(ObrDict, id);
            if (res == null)
            {
                var oId = new G5Id(id);
                if (oId.IdObr != null)
                {
                    return this.AddObr(oId.IdObr, "Obr. " + oId.NrObr, false);
                }
            }

            return res;
        }


        public bool ContainsNazwaGeogr(IG5IdProvider id, string nazwaGeogr)
        {
            if (nazwaGeogr != null)
                nazwaGeogr = nazwaGeogr.Trim();
            if (string.IsNullOrEmpty(nazwaGeogr))
                return true;

            var s = "";
            TercRecord tr;
            //if (this.TercRecords.Dict.TryGetValue(dz.Idd.IdWoj, out tr))
            //    s += tr.Nazwa+"|";
            if (this.Dict.TryGetValue(id.G5Id.IdPow, out tr))
                s += tr.Nazwa + "|";
            if (this.Dict.TryGetValue(id.G5Id.IdGmi, out tr))
                s += tr.Nazwa + "|";
            if (this.Dict.TryGetValue(id.G5Id.IdObr, out tr))
                s += tr.Nazwa + "|";

            return s.ToLower().Contains(nazwaGeogr.ToLower());
        }

        protected void AddTercRecord(string txtLineRecord)
        {
            //02;;;;DOLNOŚLĄSKIE;województwo;2011-01-01;
            //02;01;;;bolesławiecki;powiat;2011-01-01;
            //02;01;01;1;Bolesławiec;gmina miejska;2011-01-01;
            //02;01;02;2;Bolesławiec;gmina wiejska;2011-01-01;

            var recFields = txtLineRecord.Split(";".ToCharArray(), StringSplitOptions.None);
            var Woj = recFields[0];
            var Pow = recFields[1];
            var Gmi = recFields[2];
            var Rodz = recFields[3];
            var Nazwa = recFields[4];
            var NazDod = recFields[5];
            AddTercRecord(Woj, Pow, Gmi, Rodz, Nazwa, NazDod);


            //"000000_0.0000.0"
            //var idd = new G5IDD(obr + ".0");
            //if (!idd.IsValid)
            //    throw new ArgumentException("Invalid " + GetType().Name + ".IdObr value: " + obr, obr);
        }

        protected void AddTercRecord(string Woj, string Pow, string Gmi, string Rodz, string Nazwa, string NazDod)
        {
            if (string.IsNullOrEmpty(Pow))
                AddWoj(Woj, Nazwa, NazDod);
            else if (string.IsNullOrEmpty(Gmi))
                AddPow(Woj, Pow, Nazwa, NazDod);
            else
                AddGmi(Woj, Pow, Gmi, Rodz, Nazwa, NazDod);
        }

        private void AddGmi(string Woj, string Pow, string Gmi, string Rodz, string Nazwa, string NazDod)
        {
            var w = WojDict[Woj];
            var p = PowDict[Woj + Pow];
            var g = new TercGmi(w, p, Gmi, Rodz, Nazwa, NazDod);

            GmiDict.Add(g.Id, g);
            Dict.Add(g.Id, g);
            w.GmiList.Add(g);
            p.GmiList.Add(g);
        }

        private void AddPow(string Woj, string Pow, string Nazwa, string NazDod)
        {
            var w = WojDict[Woj];
            var p = new TercPow(w, Pow, Nazwa, NazDod);

            PowDict.Add(p.Id, p);
            Dict.Add(p.Id, p);
            w.PowList.Add(p);
        }

        private void AddWoj(string Woj, string Nazwa, string NazDod)
        {
            var w = new TercWoj(Woj, Nazwa, NazDod);
            WojDict.Add(w.Id, w);
            WojList.Add(w);
            Dict.Add(w.Id, w);
        }



        public TercObr AddObr(string idGmi, int nrObr, string nazwa, bool podzNaArk)
        {
            TercGmi g;
            if (!GmiDict.TryGetValue(idGmi, out g))
            {
                return null;
                //throw new Exception(GetType().Name + ".AddObr(): Nie znaleziono gminy o identyfikatorze " + Gmi + "\r\n" +"Nazwa obrebu: " + nazwa);
            }

            var o = new TercObr(g.Woj, g.Pow, g, nrObr, nazwa, podzNaArk);
            if (ObrDict.ContainsKey(o.Id))
            {
                o.Nazwa = nazwa;
                o.PodzNaAr = podzNaArk;
                return o;
            }

            ObrDict[o.Id] = o;
            Dict.Add(o.Id, o);
            g.ObrList.Add(o);
            g.Pow.ObrList.Add(o);
            g.Woj.ObrList.Add(o);
            return o;
        }

        public TercObr AddObr(string idObr, string nazwa, bool podzNaArk)
        {
            //000000_0.0000.0
            var g5i = new G5Id(idObr);
            return AddObr(g5i.IdGmi, Convert.ToInt32(g5i.NrObr), nazwa, podzNaArk);
        }

        public void AddObr(IEnumerable<G5Id> idList)
        {
            foreach (var id in idList)
            {
                if (id.IdObr != null)
                    AddObr(id.IdObr, id.NrObrInt.ToString(), false);
            }
        }

        public Terc Subset<TId>(IEnumerable<TId> idList) where TId : IG5IdProvider
        {
            HashSet<string> wojSet = new HashSet<string>();
            HashSet<string> powSet = new HashSet<string>();
            HashSet<string> gmiSet = new HashSet<string>();
            HashSet<string> obrSet = new HashSet<string>();

            foreach (var i in idList)
            {
                AddIfNotNull(wojSet, i.G5Id.IdWoj);
                AddIfNotNull(powSet, i.G5Id.IdPow);
                AddIfNotNull(gmiSet, i.G5Id.IdGmi);
                AddIfNotNull(obrSet, i.G5Id.IdObr);
            }

            Terc res = new Terc();

            foreach (var rec in WojDict.Values)
                if (wojSet.Contains(rec.Id))
                    res.AddTercRecord(rec.Woj, null, null, null, rec.Nazwa, rec.NazDod);

            foreach (var rec in PowDict.Values)
                if (powSet.Contains(rec.Id))
                    res.AddTercRecord(rec.Woj.Woj, rec.Pow, null, null, rec.Nazwa, rec.NazDod);

            foreach (var rec in GmiDict.Values)
                if (gmiSet.Contains(rec.Id))
                    res.AddTercRecord(rec.Woj.Woj, rec.Pow.Pow, rec.Gmi, rec.Rodz, rec.Nazwa, rec.NazDod);

            foreach (var rec in ObrDict.Values)
                if (obrSet.Contains(rec.Id))
                    res.AddObr(rec.Gmi.Id, rec.Obr, rec.Nazwa, rec.PodzNaAr);

            return res;
        }

        private void AddIfNotNull(HashSet<string> set, string value)
        {
            if (!string.IsNullOrEmpty(value))
                set.Add(value);
        }


        public void AddTerc(string tercId)
        {
            var wId = tercId.Substring(0, 2);
            if (!this.WojDict.ContainsKey(wId))
            {
                var w = Records.WojDict[wId];
                AddTercRecord(w.Woj, null, null, null, w.Nazwa, w.NazDod);
            }

            if (tercId.Length >= 4)
            {
                var pId = tercId.Substring(0, 4);
                if (!this.PowDict.ContainsKey(pId))
                {
                    var p = Records.PowDict[pId];
                    AddTercRecord(p.Woj.Woj, p.Pow, null, null, p.Nazwa, p.NazDod);

                }
            }

            //020102_2
            if (tercId.Length >= 8)
            {
                var gId = tercId.Substring(0, 8);
                if (!this.PowDict.ContainsKey(gId))
                {
                    var g = Records.GmiDict[gId];
                    AddTercRecord(g.Woj.Woj, g.Pow.Pow, g.Gmi, g.Rodz, g.Nazwa, g.NazDod);

                }
            }

        }
    }

}