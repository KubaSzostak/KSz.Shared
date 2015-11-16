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

    /// <summary>
    /// Klasa bazowa do prezentacji danych Woj, Pow, Gmi, Obr
    /// </summary>
    public class TercPresenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            if (PropertyChanged != null)
            {
                if (propertyExpresssion == null)
                    throw new ArgumentNullException("propertyExpresssion");

                var lambda = (LambdaExpression)propertyExpresssion;
                var unaryExpr = lambda.Body as UnaryExpression;
                MemberExpression mbrExpr = null;

                if (unaryExpr != null)
                    mbrExpr = unaryExpr.Operand as MemberExpression;
                else
                    mbrExpr = lambda.Body as MemberExpression;
                if (mbrExpr == null)
                    throw new ArgumentException("The expression is not a member access expression.", "propertyExpresssion");

                var propInfo = mbrExpr.Member as PropertyInfo;
                if (propInfo == null)
                    throw new ArgumentException("The member access expression does not access a property.", "propertyExpresssion");

                var getMethod = propInfo.GetGetMethod(true);
                if (getMethod.IsStatic)
                    throw new ArgumentException("The referenced property is a static property.", "propertyExpresssion");

                PropertyChanged(this, new PropertyChangedEventArgs(mbrExpr.Member.Name));
            }
        }

        protected Terc TercRecords;

        public TercPresenter()
        {
            this.TercRecords = Terc.Records;
            SelectDefaultSubitem = false;
        }

        public virtual void Init(Terc records)
        {
            TercRecords = records;

            if (SelectDefaultSubitem)
                SelectedWoj = WojList.FirstOrDefault();
            else
                SelectedWoj = null; // Łańcuchowo wywołuje Pow,Gmi,Obr = null; 
        }

        private TercWoj mSelectedWoj;
        public TercWoj SelectedWoj
        {
            get { return mSelectedWoj; }
            set
            {
                mSelectedWoj = value;
                if (value != null)
                {
                    TercPresenter.LastSelectedWojId = value.Id;
                }
                SelectedPow = GetDefaultSubitem<TercPow>(PowList, SelectedPow);
                SelectedTercIdChanged();

                OnPropertyChanged(() => SelectedWoj);
                OnPropertyChanged(() => PowList);
                OnPropertyChanged(() => PowListEx);
            }
        }

        private TercPow mSelectedPow;
        public TercPow SelectedPow
        {
            get { return mSelectedPow; }
            set
            {
                mSelectedPow = value;
                if (value != null)
                    TercPresenter.LastSelectedPowId = value.Id;
                SelectedTercIdChanged();
                SelectedGmi = GetDefaultSubitem<TercGmi>(GmiList, SelectedGmi);

                OnPropertyChanged(() => SelectedPow);
                OnPropertyChanged(() => GmiList);
                OnPropertyChanged(() => GmiListEx);
            }
        }

        private TercGmi mSelectedGmi;
        public TercGmi SelectedGmi
        {
            get { return mSelectedGmi; }
            set
            {
                mSelectedGmi = value;
                if (value != null)
                    TercPresenter.LastSelectedGmiId = value.Id;
                SelectedTercIdChanged();
                SelectedObr = GetDefaultSubitem<TercObr>(ObrList, SelectedObr);

                OnPropertyChanged(() => SelectedGmi);
                OnPropertyChanged(() => ObrList);
                OnPropertyChanged(() => ObrListEx);
            }
        }

        private TercObr mSelectedObr;
        public TercObr SelectedObr
        {
            get { return mSelectedObr; }
            set
            {
                mSelectedObr = value;
                if (value != null)
                    TercPresenter.LastSelectedObrId = value.Id;
                SelectedTercIdChanged();

                SelectedObrChanged();
                OnPropertyChanged(() => SelectedObr);
            }
        }

        protected virtual void SelectedObrChanged()
        {
        }

        protected virtual void SelectedTercIdChanged()
        {
            OnPropertyChanged(() => SelectedTercId);
        }

        public virtual string SelectedTercId
        {
            get
            {
                if (SelectedObr != null)
                    return SelectedObr.Id;
                if (SelectedGmi != null)
                    return SelectedGmi.Id;
                if (SelectedPow != null)
                    return SelectedPow.Id;
                if (SelectedWoj != null)
                    return SelectedWoj.Id;
                return null;
            }
        }

        public static string LastSelectedWojId = "";
        public static string LastSelectedPowId = "";
        public static string LastSelectedGmiId = "";
        public static string LastSelectedObrId = "";
        public static string LastSelectedParcelId = G5Id.EmptyId;

        public List<TercWoj> WojList
        {
            get { return TercRecords.WojList; }
        }

        public List<TercPow> PowList
        {
            get
            {
                if (SelectedWoj == null)
                    return new List<TercPow>();
                return SelectedWoj.PowList;
            }
        }


        /// <summary>
        /// Gdy (SelectedWoj != null) - zwraca listę powiatów w wybranym województwie
        /// Gdy (SelectedWoj == null) - zwraca listę wszystkich powiatów
        /// </summary>
        public IEnumerable<TercPow> PowListEx
        {
            get
            {
                if (SelectedWoj != null)
                    return SelectedWoj.PowList;
                return TercRecords.PowDict.Values.OrderBy(p => p.Nazwa + p.NazDod);
            }
        }

        public List<TercGmi> GmiList
        {
            get
            {
                if (SelectedPow == null)
                    return new List<TercGmi>();
                return SelectedPow.GmiList;
            }
        }
        /// <summary>
        /// Gdy (SelectedPow != null) - zwraca listę gmin w wybranym powiecie
        /// Gdy (SelectedPow == null) - zwraca listę wszystkich powiatów lub powiató w wybranym województwie
        /// </summary>
        public IEnumerable<TercGmi> GmiListEx
        {
            get
            {
                if (SelectedPow != null)
                    return SelectedPow.GmiList;
                if (SelectedWoj != null)
                    return SelectedWoj.GmiList;
                return TercRecords.GmiDict.Values.OrderBy(p => p.Nazwa + p.NazDod);
            }
        }

        public List<TercObr> ObrList
        {
            get
            {
                if (SelectedGmi == null)
                    return new List<TercObr>();
                return SelectedGmi.ObrList;
            }
        }
        /// <summary>
        /// Gdy (SelectedPow != null) - zwraca listę gmin w wybranym powiecie
        /// Gdy (SelectedPow == null) - zwraca listę wszystkich powiatów lub powiató w wybranym województwie
        /// </summary>
        public IEnumerable<TercObr> ObrListEx
        {
            get
            {
                if (SelectedGmi != null)
                    return SelectedGmi.ObrList;
                if (SelectedPow != null)
                    return SelectedPow.ObrList;
                if (SelectedWoj != null)
                    return SelectedWoj.ObrList;
                return TercRecords.ObrDict.Values.OrderBy(p => p.Nazwa + p.NazDod);
            }
        }

        public bool SelectDefaultSubitem { get; set; }

        protected T GetDefaultSubitem<T>(IEnumerable<T> items, T selectedItem)
        {
            if (items == null)
                return default(T);

            if (SelectDefaultSubitem)
            {
                if (items.Contains(selectedItem))
                    return selectedItem;
                else
                    return items.FirstOrDefault();
            }
            else
                return default(T);
        }

        public virtual void Select(string id)
        {
            var selSubitem = SelectDefaultSubitem;
            SelectDefaultSubitem = false;

            var g5i = new G5Id(id);

            mSelectedWoj = Select<TercWoj>(WojList, g5i.IdWoj);
            OnPropertyChanged(() => SelectedWoj); // Wywołuj ręcznie, żeby za każdym razem nie aktualizować łańcuchowo SelectedPow,Gmi,Obr

            mSelectedPow = Select<TercPow>(PowList, g5i.IdPow);
            OnPropertyChanged(() => SelectedPow); // Wywołuj ręcznie, żeby za każdym razem nie aktualizować łańcuchowo SelectedGmi,Obr

            mSelectedGmi = Select<TercGmi>(GmiList, g5i.IdGmi);
            OnPropertyChanged(() => SelectedGmi); // Wywołuj ręcznie, żeby za każdym razem nie aktualizować łańcuchowo SelectedObr

            SelectedObr = Select<TercObr>(ObrList, g5i.IdObr); // A tu wywołuj standardowo, żeby właczyć obsługę zdarzeń pochodnych, np. SelectedSubitem

            SelectDefaultSubitem = selSubitem;
        }

        private T Select<T>(IEnumerable<T> items, string id) where T : TercRecord
        {
            if (string.IsNullOrEmpty(id))
                return default(T);
            else
                return items.FirstOrDefault(t => t.Id == id);
        }
    }



    /// <summary>
    /// Abstrakcyjna klasa do prezentacji danych wyszukiwanych na podstawie TERC
    /// </summary>
    /// <typeparam name="TSubitem"></typeparam>
    public abstract class TercSubitemsPresenterBase<TSubitem> : TercPresenter
    {

        // TercPresenter korzysta z danych wbuodwanych w bibliotekę DLL. Nie potrzebuje zewnętrznego źródła 
        // do informacji o Województwach, Powitach i Gminach. 
        // Ten Presenter będzie najprawdopodobniej pobierać informacje o obrębach z zewnętrznych
        // zródeł danych. Dlatego podejście do obsługi zdarzeń na listach jest zupełnie inne.


        private IList<TSubitem> mSubitemList = new List<TSubitem>();
        public IList<TSubitem> SubitemList
        {
            get
            {
                return mSubitemList;
            }
            set
            {
                if (mSubitemList == value)
                    return;
                
                mSubitemList = value;
                if (mSubitemList == null)
                    mSubitemList = new List<TSubitem>();

                SelectedSubitem = GetDefaultSubitem<TSubitem>(SubitemList, SelectedSubitem);
                OnPropertyChanged(() => SubitemList);
                SubitemListChanged();
            }
        }

        private TSubitem mSelectedSubitem;
        public TSubitem SelectedSubitem
        {
            get { return mSelectedSubitem; }
            set
            {
                mSelectedSubitem = value;
                var g5i = value as IG5IdProvider;
                if (g5i != null)
                    TercPresenter.LastSelectedParcelId = g5i.G5Id.IdDz;
                OnPropertyChanged(() => SelectedSubitem);
                SelectedSubitemChanged();
            }
        }

        protected virtual void SelectedSubitemChanged()
        {
        }

        protected virtual void SubitemListChanged()
        {

        }
    }

    /// <summary>
    /// Abstrakcyjna klasa do filtrowania danych na podstawie TERC, NazwyGeogr oraz kryteriów wyszukiwania
    /// </summary>
    /// <typeparam name="TSubitem"></typeparam>
    public abstract class TercSubitemsFilterPresenterBase<TSubitem> : TercSubitemsPresenterBase<TSubitem>
    {
        // Tworzy SubitemList dopiero po ręcznym wywołaniu metody Filter()

        protected abstract IEnumerable<TSubitem> GetSubitems(string idStart, string subitemFilter);

        public void Filter()
        {
            FilteringStatusChanged("Filtrowanie danych...");
            var res = new HashSet<TSubitem>();

            // Najpierw wyszukaj działki o podanych numerach
            string[] filterArr = new string[0];
            if (!string.IsNullOrEmpty(SubitemFilter))
                filterArr = SubitemFilter.Split(", ;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (filterArr.Length < 1)
                filterArr = new string[1] { "" };

            foreach (var filter in filterArr)
            {
                var subitems = GetSubitems(SelectedTercId, filter);
                foreach (var s in subitems)
                    res.Add(s);
            }

            // Potem, w razie potrzeby, przefiltruj wg nazwy gegoraficznej
            List<TSubitem> resList;
            if (string.IsNullOrEmpty(NazwaGeogr))
                resList = new List<TSubitem>(res);
            else
                resList = res.Where(r => ContainsNazwaGeogr(r)).ToList();

            if (SubitemComparison != null)
                resList.Sort(SubitemComparison);

            // Przefiltruj też zgodnie z wymogami klasy dziedziczącej
            var filteredList = FilterSubitems(resList);

            int pCount = filteredList.Count();
            if (pCount > 1000)
                filteredList = filteredList.Take(1000);

            FilteringStatusChanged("Wczytywanie danych...");
            SubitemList = new List<TSubitem>(filteredList);
            FilteringStatusChanged(null);


            if (pCount > 1000)
                FilteringFinished(string.Format("Wczytano 1000 pierwszych działek z {0} wyszukanych.", pCount));
            else
                FilteringFinished(string.Format("Znaleziono {0} działek.", pCount));
        }

        protected Comparison<TSubitem> SubitemComparison = null;

        protected int MaxSuitemsCount = int.MaxValue;

        protected virtual IEnumerable<TSubitem> FilterSubitems(IEnumerable<TSubitem> subitems)
        {
            return subitems;
        }

        protected virtual void FilteringStatusChanged(string filter)
        {
        }

        protected virtual void FilteringFinished(string msg)
        {
        }

        protected virtual bool ContainsNazwaGeogr(TSubitem subitem)
        {
            if (string.IsNullOrEmpty(NazwaGeogr))
                return true;
            var id = subitem as IG5IdProvider;
            if (id == null)
                return false; // Trzeba zaimplementować własną obsługę tej metody
            return TercRecords.ContainsNazwaGeogr(id, NazwaGeogr);
        }

        //public System.Windows.Input.ICommand FilterCommand
        //{
        //    get { return new DelegateCommand(Filter); }
        //}


        private string mSubitemFilter;
        public string SubitemFilter
        {
            get { return mSubitemFilter; }
            set
            {
                mSubitemFilter = value;
                OnPropertyChanged(() => SubitemFilter);
            }
        }

        private string mNazwaGeogr;
        public string NazwaGeogr
        {
            get { return mNazwaGeogr; }
            set
            {
                mNazwaGeogr = value;
                OnPropertyChanged(() => NazwaGeogr);
            }
        }

        public override void Select(string id)
        {
            base.Select(id);
        }
    }

    /// <summary>
    /// Klasa umożliwiająca filtrowanie Identyfikatorów działek (IDD) na podstawie danych TERC
    /// NazwyGeogr oraz podanego nr działki
    /// </summary>
    /// <typeparam name="TSubitem"></typeparam>
    public class G5IddFilterPresenter<TSubitem> : TercSubitemsFilterPresenterBase<TSubitem>
        where TSubitem : IG5IdProvider
    {
        protected readonly IEnumerable<TSubitem> AllSubitems;

        public G5IddFilterPresenter(IEnumerable<TSubitem> iddList)
        {
            SubitemComparison = (x, y) => { return G5Id.Comparer.Compare(x.G5Id, y.G5Id); };
            AllSubitems = iddList.Where(i => i.G5Id.IdDz != null);
            base.Init(Terc.Records.Subset(AllSubitems));
        }


        protected override IEnumerable<TSubitem> GetSubitems(string idStart, string subitemFilter)
        {
            IEnumerable<TSubitem> res = AllSubitems;

            if (!string.IsNullOrEmpty(idStart))
                res = res.Where(i => i.G5Id.IdDz.StartsWith(idStart));

            subitemFilter = subitemFilter.Trim();
            if (string.IsNullOrEmpty(subitemFilter))
                return res;

            var arDz = subitemFilter.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (arDz.Length > 1)
                return res.Where(i => i.G5Id.NrAr == arDz[0] && i.G5Id.ContainsNrDz(arDz[1]));
            else
                return res.Where(i => i.G5Id.ContainsNrDz(subitemFilter));
        }

        public override void Select(string id)
        {
            base.Select(id);
            SelectedSubitem = AllSubitems.FirstOrDefault(i => i.G5Id.IdDz == id);
        }
    }


    /// <summary>
    /// Abstrakcyjna klasa do prezentacji Identyfikatorów działek (IDD) zgodnych z TERC oraz NrArk
    /// </summary>
    /// <typeparam name="TSubitem"></typeparam>
    public abstract class G5IddPresenterBase<TSubitem> : TercSubitemsPresenterBase<TSubitem>
        where TSubitem : IG5IdProvider
    {
        // Tworzy SubitemList automatycznie po zmianie wartości SelectedObr

        protected abstract IEnumerable<TSubitem> GetSubitems(TercObr obr);

        public G5IddPresenterBase()
        {
            NrArList = null;
        }

        private string NrArEmpty = "<brak>";

        protected override void SelectedObrChanged()
        {
            base.SelectedObrChanged();
            if (SelectedObr == null)
            {
                mObrIddList = new List<TSubitem>();
                NrArList = null; // Brak podziału na arkusze
                //NrDzList = new ObservableCollection<string>();  // Is set in property SelectedNrAr
            }
            else
            {
                mObrIddList = GetSubitems(SelectedObr).ToList();
                mObrIddList.Sort();

                var arList = mObrIddList.Select(i => i.G5Id.NrAr).Distinct();
                var arListCount = arList.Count();
                if (arListCount < 1 || (arListCount == 1 && arList.First() == null))
                    NrArList = null;
                else
                {
                    NrArList = new List<string>(arList);
                    for (int i = 0; i < NrArList.Count; i++)
                    {
                        if (NrArList[i] == null)
                            NrArList[i] = NrArEmpty;
                    }
                }

                //NrDzList = new ObservableCollection<string>(mObrIddList.Select(i => i.NrDz)); // Is set in property SelectedNrAr
            }
            OnPropertyChanged(() => PodzNaAr);
            OnPropertyChanged(() => NrArList);
            //NotifyPropertyChanged(() => NrDzList); // Is set in property SelectedNrAr

            SelectedNrAr = GetDefaultSubitem<string>(NrArList, SelectedNrAr);
            //SelectedNrDz = GetDefaultSubitem<G5IDD>(IddList, SelectedNrDz); // Is set in property SelectedNrAr
            //SelectedNrDzList = new ObservableCollection<string>();  // Is set in property SelectedNrAr
        }

        private List<TSubitem> mObrIddList = new List<TSubitem>();
        public List<string> NrArList { get; private set; }

        public bool PodzNaAr
        {
            get
            {
                return NrArList != null;

                //if (SelectedObr != null)
                //    return SelectedObr.PodzNaAr;
                //return false;
            }
        }

        private string mSelectedNrAr;
        public string SelectedNrAr
        {
            get { return mSelectedNrAr; }
            set
            {
                mSelectedNrAr = value;

                var idd = mObrIddList.FirstOrDefault();
                if (SelectedObr == null || idd == null) // nie ma działek w obrębie
                {
                    //mSelectedNrAr = null; // nowy nr arkusza
                    SubitemList = null;
                    SelectedSubitem = default(TSubitem);
                }
                else
                {
                    if (PodzNaAr)
                    {
                        IEnumerable<TSubitem> itemsInAr;
                        if (mSelectedNrAr == NrArEmpty)
                            itemsInAr = mObrIddList.Where(i => i.G5Id.IdObr == SelectedObr.Id && i.G5Id.NrAr == null);
                        else
                        {
                            var idArk = SelectedObr.Id + ".AR_" + mSelectedNrAr;
                            itemsInAr = mObrIddList.Where(i => i.G5Id.IdAr == idArk);
                        }
                        SubitemList = new List<TSubitem>(itemsInAr);
                    }
                    else // Nie ma podziału na arkusze (dla całego obrębu)
                    {
                        //dzList = mObrIddList.Where(i => i.IdObr == idd.IdObr); //Zwróć wszystkie działki z wybranego obrębu
                        SubitemList = new List<TSubitem>(mObrIddList); // .ToList(); Jest sortowane raz przy przypisywaniu mObrIddList = XXX;  
                    }

                    SelectedSubitem = GetDefaultSubitem<TSubitem>(SubitemList, SelectedSubitem);
                }

                OnPropertyChanged(() => SelectedNrAr);
                OnPropertyChanged(() => SubitemList);
            }
        }

        public override void Select(string id)
        {
            base.Select(id);
            var idd = new G5Id(id);

            SelectedNrAr = idd.NrAr;

            var idDz = idd.IdDz;
            if (idDz != null)
            {
                TercPresenter.LastSelectedParcelId = idDz;
                SelectedSubitem = SubitemList.FirstOrDefault(s => s.G5Id.IdDz == idDz);
            }
            else
            {
                SelectedSubitem = default(TSubitem);
            }
        }
    }

    /// <summary>
    /// Klasa do prezentacji Identyfikatorów działek (IDD) zgodnych z TERC oraz NrArk
    /// </summary>
    /// <typeparam name="TSubitem"></typeparam>
    public class G5IddPresenter<TSubitem> : G5IddPresenterBase<TSubitem>
        where TSubitem : IG5IdProvider
    {
        List<TSubitem> allIddList;

        public G5IddPresenter(IEnumerable<TSubitem> iddList)
        {
            allIddList = iddList.Where(i => i.G5Id.IddIsValid).ToList();
            base.Init(Terc.Records.Subset(allIddList));
        }

        protected override IEnumerable<TSubitem> GetSubitems(TercObr obr)
        {
            var res = new List<TSubitem>();
            if (obr == null)
                return res;

            return allIddList.Where(i => i.G5Id.IdObr == obr.Id);
        }

        public void SubitemListAdd(TSubitem subitem)
        {
            allIddList.Add(subitem);
            //SubitemList.Add(subitem);

            // Może być dodany Subitem z poza aktualnie zaznaczonego obrębu, 
            // dlatego zamiast dodawać bezpośrednio do SubitemList po prostu
            // odśwież listy
            SelectedObrChanged();

            //SelectedSubitem = GetDefaultSubitem<TSubitem>(SubitemList, SelectedSubitem);
            //Gdy będzie spoza listy, zaznaczy pierwszy element mimo, że inny był już zaznaczony
            if (SubitemList.Contains(subitem))
                SelectedSubitem = subitem;
        }
    }

}
