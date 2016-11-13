using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.ComponentModel;

namespace System
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LocalizedStringAttribute : Attribute
    {
        public readonly CultureInfo Culture;
        public string Value;

        public LocalizedStringAttribute(CultureInfo culture, string localizedValue)
        {
            Culture = culture;
            Value = localizedValue;
        }

        public LocalizedStringAttribute(string cultureName, string localizedValue)
            : this(new CultureInfo(cultureName), localizedValue)
        {
        }

        public LocalizedStringAttribute(string localizedValue)
            : this(CultureInfo.InvariantCulture, localizedValue)
        {
        }
    }

    public class LocalizedStringPlAttribute : LocalizedStringAttribute
    {
        private static CultureInfo culturePl = new CultureInfo("pl-PL");

        public LocalizedStringPlAttribute(string localizedValue)
            : base(culturePl, localizedValue)
        {
        }
    }

    // Nie jest to statyczny obiekt, żeby można było dziedziczyć w innych projektach
    // Dynamiczną klasę można też łatwiej bindować do kontrolek
    // Użyj SysUtils.Strings
    public class LocalizationStrings : INotifyPropertyChanged  
    {
        #region *** Engine ***

        static CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        static CultureInfo invariantCulture = CultureInfo.InvariantCulture;
        public event PropertyChangedEventHandler PropertyChanged;

        public LocalizationStrings()
        {
            UpdateLocalizedProperties();
        }

        public void UpdateLocalizedProperties()
        {
            // publiczny, żeby można było w przyszłości czytać wartośći ze źródeł zewnętrznych, np. pliku
            var allProps = this.GetType().GetProperties();
            foreach (var prop in allProps)
                try
                {
                    var attributes = prop.GetCustomAttributes(typeof(LocalizedStringAttribute), true);
                    var locAttr = GetLocalizedAttribute(attributes);

                    if (locAttr == null)
                        throw new Exception(string.Format("ERROR: {0}.{1} property does not contain {2}.", prop.DeclaringType.Name, prop.Name, typeof(LocalizedStringAttribute).Name));

                    prop.SetValue(this, locAttr.Value, null);
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs(prop.Name));
                }
                catch (Exception ex)
                {
                    var msg = ex.Message + string.Format("\r\n{0}.{1}", this.GetType().Name, prop.Name);
                    throw new Exception(msg, ex);
                }
        }


        public static string GetFromAttribute(object o)
        {
            // publiczny, żeby można było w przyszłości czytać wartośći ze źródeł zewnętrznych, np. pliku
            var attributes = o.GetType().GetCustomAttributes(typeof(LocalizedStringAttribute), true);
            var locAttr = GetLocalizedAttribute(attributes);
            if (locAttr == null)
                throw new Exception(string.Format("ERROR: Object of type {0} does not contain {1}.", o.GetType().Name, typeof(LocalizedStringAttribute).Name));
            return locAttr.Value;
        }

        private static LocalizedStringAttribute GetLocalizedAttribute(object[] attributes)
        {
            if (attributes == null)
                return null;
            LocalizedStringAttribute invaraintCultureAttr = null;

            foreach (LocalizedStringAttribute attr in attributes)
            {
                if (attr.Culture.Equals(uiCulture))
                    return attr; // Curent UI Culture zwracaj od razu
                else if (attr.Culture.Equals(invariantCulture))
                    invaraintCultureAttr = attr; //Invariant Culture zwracaj tylko wtedy gdy nie ma CurentUICulture
            }
            return invaraintCultureAttr;
        } 

        #endregion

        [LocalizedString("TestProperty Value")]
        [LocalizedStringPl("Wartość TestProperty")]
        public string TestProperty { get; set; }



        [LocalizedString("Instrument not initialized")]
        [LocalizedStringPl("Nie udało się zainicjować instrumentu")]
        public string InstrumentInitFailed { get; set; }

        [LocalizedString("Protocol error")]
        [LocalizedStringPl("Błąd protołu")]
        public string ProtocolError { get; set; }

        [LocalizedString("Cannot change instrument settings.")]
        [LocalizedStringPl("Nie można zmienić ustawień instrumentu.")]
        public string CantChangeInstrSettings { get; set; }

        [LocalizedString("Cannot read instrument configuration.")]
        [LocalizedStringPl("Nie można odczytać konfiguracji instrumentu.")]
        public string InvalidConfValue { get; set; }

        [LocalizedString("Ivalid data format.")]
        [LocalizedStringPl("Nieprawidłowy format danych.")]
        public string InvalidDataFormat { get; set; }

        [LocalizedString("Ivalid number value: '{0}'")]
        [LocalizedStringPl("Nieprawidłowy format liczby: '{0}'")]
        public string InvalidNumberFormatX { get; set; }

        [LocalizedString("Ivalid time format")]
        [LocalizedStringPl("Nieprawidłowy format czasu")]
        public string InvalidTimeFormat { get; set; }

        [LocalizedString("Communication error.")]
        [LocalizedStringPl("Błąd podczas komunikacji z instrumentem.")]
        public string CommunicationError { get; set; }

        [LocalizedString("Cannot change transmision format.")]
        [LocalizedStringPl("Nie można zmienić formatu transmisji w instrumencie.")]
        public string CantSetTransferFormat { get; set; }

        [LocalizedString("Data for '{0}' column not found")]
        [LocalizedStringPl("Nie znaleziono danych dla kolumny '{0}")]
        public string ColDataNotFound { get; set; }

        [LocalizedString("Column '{0}' not found")]
        [LocalizedStringPl("Nie znaleziono kolumny '{0}")]
        public string ColNotFound { get; set; }

        [LocalizedString("Row '{0}' not found")]
        [LocalizedStringPl("Nie znaleziono wiersza '{0}")]
        public string RowNotFound { get; set; }

        [LocalizedString("To many values. Additional data will be ignored.")]
        [LocalizedStringPl("Zbyt dużo wartości. Dodatkowe dane zostaną zignorowane.")]
        public string ColDataIgnored { get; set; }

        [LocalizedString("Wizard property is null")]
        [LocalizedStringPl("Wizard property is null")]
        public string NullWizardProperty { get; set; }

        [LocalizedString("Error")]
        [LocalizedStringPl("Błąd")]
        public string WizError { get; set; }

        [LocalizedString("Invalid file name")]
        [LocalizedStringPl("Nieprawidłowa nazwa pliku")]
        public string InvalidFileName { get; set; }

        [LocalizedString("File contains no data")]
        [LocalizedStringPl("Plik nie zawiera danych")]
        public string FileContainsNoData { get; set; }

        [LocalizedString("No data")]
        [LocalizedStringPl("Brak danych")]
        public string NoData { get; set; }


        [LocalizedString("File not exists")]
        [LocalizedStringPl("Plik nie istnieje")]
        public string FileNotExists { get; set; }

        [LocalizedString("File '{0} not existss")]
        [LocalizedStringPl("Plik '{0}' nie istnieje.")]
        public string FileNotExistsPath { get; set; }

        [LocalizedString("Phrase '{0} not found")]
        [LocalizedStringPl("Nie odnaleziono frazy '{0}'")]
        public string PhraseNotFound { get; set; }

        [LocalizedString("Word not found in '{0}'")]
        [LocalizedStringPl("Nie odnaleziono żadnego wyrazu we frazie '{0}'")]
        public string WordNotFound { get; set; }

        [LocalizedString("File not found: {0}")]
        [LocalizedStringPl("Nie znaleziono pliku: '{0}'")]
        public string FileNotFound { get; set; }

        [LocalizedString("Ivalid elements count")]
        [LocalizedStringPl("Nieprawidłowa ilość elementów")]
        public string InvalidFieldsCount { get; set; }

        [LocalizedString("Field '{0}' not found")]
        [LocalizedStringPl("Nie znaleziono pola '{0}'")]
        public string FieldNotFound { get; set; }

        [LocalizedString("Field no {0}")]
        [LocalizedStringPl("Pole nr {0}")]
        public string FieldNoX { get; set; }

        [LocalizedString("Field number")]
        [LocalizedStringPl("Numer pola")]
        public string FieldNumber { get; set; }


        [LocalizedString("Nothing found")]
        [LocalizedStringPl("Nic nie znaleziono")]
        public string NothingFound { get; set; }

        [LocalizedString("'{0} not found")]
        [LocalizedStringPl("Nie znaleziono '{0}'")]
        public string XNotFound { get; set; }

        [LocalizedString("Blocks not found")]
        [LocalizedStringPl("Nie znaleziono bloków")]
        public string BlocksNotFond { get; set; }



        [LocalizedString("Field '{0}' not found. Default value was used.")]
        [LocalizedStringPl("Nie znaleziono pola '{0}' - przypisano wartość domyślną.")]
        public string FieldNotFoundDefaltValueSetted { get; set; }

        [LocalizedString("Invalid value '{0}: {1}")]
        [LocalizedStringPl("Nieprawidłowa wartość '{0}': {1}")]
        public string InvalidFieldValue { get; set; }

        [LocalizedString("Drawing has no blocks definied")]
        [LocalizedStringPl("Rysunek nie ma zdefiniowanych bloków")]
        public string DwgHasNoBlocks { get; set; }
        

        [LocalizedString("Element '{0}' was not found.")]
        [LocalizedStringPl("Nie znaleziono elementu '{0}'")]
        public string XmlElementNotFound { get; set; }

        [LocalizedString("XML Element expected.")]
        [LocalizedStringPl("Oczekiwano elementu XML")]
        public string XmlElementExpected { get; set; }

        
        [LocalizedString("Trying to use planned unit as actual unit.")]
        [LocalizedStringPl("Próba użycia planowanej jednostki jako aktualnej jednostki.")]
        public string GwPlannedIsNull { get; set; }

        [LocalizedString("Selected elements can be edited individualy.")]
        [LocalizedStringPl("Wybrane elementy  można edytować tylko pojedyńczo.")]
        public string GwSelectedItemsAreEditableOnlyIndividually { get; set; }

        [LocalizedString("Items can be added only to work day")]
        [LocalizedStringPl("Kolejne elementy można dodawać tylko do dnia pracy.")]
        public string GwNextUnitsCanBeAddedToWorkDayOnly { get; set; }

        [LocalizedString("'{0}' has invalid value")]
        [LocalizedStringPl("'{0}' zawiera nieprawidłową wartość")]
        public string EditBoxValueIsInvalid { get; set; }

        [LocalizedString("'{0}' has invalid value")]
        [LocalizedStringPl("'{0}' zawiera nieprawidłową wartość")]
        public string FieldValueIsInvalid { get; set; }

        [LocalizedString("Item not selected")]
        [LocalizedStringPl("Nie zaznaczono żadnego elementu")]
        public string ItemNotSelected { get; set; }

        
        [LocalizedString("Select block name")]
        [LocalizedStringPl("Wskaż nazwę bloku")]
        public string AcSelectBlockName { get; set; }

        [LocalizedString("Ivalid serial number")]
        [LocalizedStringPl("Nieprawidłowy numer seryjny.")]
        public string InvalidSerial { get; set; }

        [LocalizedString("Invalid license key")]
        [LocalizedStringPl("Nieprawidłowy klucz licencji.")]
        public string InvalidLicKey { get; set; }

        [LocalizedString("Ivalid license")]
        [LocalizedStringPl("Nieprawidłowa licencja.")]
        public string InvalidLicence { get; set; }

        [LocalizedString("Imported license was generated for another product")]
        [LocalizedStringPl("Importowana licencja dotyczy innego produktu")]
        public string InvalidImpLicProduct { get; set; }

        [LocalizedString("Imported license was generated for another product version")]
        [LocalizedStringPl("Importowana licencja dotyczy innej wersji produktu")]
        public string InvalidImpLicVersion { get; set; }

        [LocalizedString("Imported license was generated for another computer")]
        [LocalizedStringPl("Importowana licencja dotyczy innego komputera")]
        public string InvalidImpLicMachine { get; set; }

        [LocalizedString("License expiried.")]
        [LocalizedStringPl("Licencja wygasła.")]
        public string LicenseExpired { get; set; }


        [LocalizedString("You have used depreaced software version. You have to download newest software from wendor website in order to obtain new licese.")]
        [LocalizedStringPl("Używasz nieakutalnej wersji oprogramowania. Aby wygenerować licencję odwiedź stronę producenta i pobierz najnowszą wersję oprogramowania")]
        public string LicDepracedProductVersion { get; set; }


        [LocalizedString("Unknown instrument")]
        [LocalizedStringPl("Nieznany instrument")]
        public string UnknownInstrument { get; set; }

        [LocalizedString("Conecting...")]
        [LocalizedStringPl("Trwa łączenie...")]
        public string Connecting { get; set; }

        [LocalizedString("Initialization...")]
        [LocalizedStringPl("Inicjalizacja instrumentu...")]
        public string InstrumentInitialization { get; set; }

        [LocalizedString("Downloading jobs...")]
        [LocalizedStringPl("Wczytywanie obiektów...")]
        public string DownloadingJobs { get; set; }

        [LocalizedString("Downloading observations...")]
        [LocalizedStringPl("Wczytywanie danych pomiarowych...")]
        public string DownloadingObs { get; set; }

        [LocalizedString("Downloading data...")]
        [LocalizedStringPl("Wczytywanie danych...")]
        public string DownloadingData { get; set; }

        [LocalizedString("Downloading coordinates...")]
        [LocalizedStringPl("Wczytywanie wszpółrzędnych...")]
        public string DownloadingCoord { get; set; }

        [LocalizedString("{0} data blocks downloaded...")]
        [LocalizedStringPl("Wczytano '{0}' bloków...")]
        public string DownloadedBlokcsCount { get; set; }

        [LocalizedString("Next >")]
        [LocalizedStringPl("Dalej >")]
        public string WizardNext { get; set; }

        [LocalizedString("< Prior")]
        [LocalizedStringPl("< Wstecz")]
        public string WizardPrior { get; set; }

        [LocalizedString("Close")]
        [LocalizedStringPl("Zamknij")]
        public string Close { get; set; }

        [LocalizedString("Cancel")]
        [LocalizedStringPl("Anuluj")]
        public string Cancel { get; set; }

        [LocalizedString("Errors count: {0}")]
        [LocalizedStringPl("Błędów: {0}")]
        public string ErrorsCount { get; set; }

        [LocalizedString("Warnings count: {0}")]
        [LocalizedStringPl("Ostrzeżeń: {0}")]
        public string WarningCount { get; set; }

        [LocalizedString("None")]
        [LocalizedStringPl("Brak")]
        public string None { get; set; }

        [LocalizedString("Stations")]
        [LocalizedStringPl("Stacje")]
        public string Stations { get; set; }

        [LocalizedString("Coordinate system: \"1965\", Zone {0}")]
        [LocalizedStringPl("Układ współrzędnych \"1965\", Strefa {0}")]
        public string CoordSystem1965 { get; set; }



        [LocalizedString("All files (*.*)")]
        [LocalizedStringPl("Wszystkie pliki (*.*)")]
        public string AllFiles { get; set; }

        [LocalizedString("Text files (*.txt)")]
        [LocalizedStringPl("Pliki tekstowe (*.txt)")]
        public string TxtFiles { get; set; }

        [LocalizedString("XML files (*.xml)")]
        [LocalizedStringPl("Pliki XML (*.xml)")]
        public string XmlFiles { get; set; }

        [LocalizedString("Licence files (*.lic)")]
        [LocalizedStringPl("Pliki licencji (*.lic)")]
        public string LicFiles { get; set; }
        
        [LocalizedString("GeoPracownik files")]
        [LocalizedStringPl("Pliki programu GeoPracownik (*.gwxml)")]
        public string GeoWorkerFiles { get; set; }

        [LocalizedString("All Leica files")]
        [LocalizedStringPl("Wszystkie pliki Leica")]
        public string AllLeica { get; set; }

        [LocalizedString("Leica IDEX files (*.idx)")]
        [LocalizedStringPl("Pliki Leica IDEX (*.idx)")]
        public string IdexFiles { get; set; }

        [LocalizedString("Leica GSI files (*.gsi)")]
        [LocalizedStringPl("Pliki Leica GSI (*.gsi)")]
        public string GsiFiles { get; set; }

        [LocalizedString("',' (Comma)")]
        [LocalizedStringPl("',' (Przecinek)")]
        public string SepComma { get; set; }

        [LocalizedString("'.' (Period)")]
        [LocalizedStringPl("'.' (Kropka)")]
        public string SepPeriod { get; set; }



        [LocalizedString("Enter matching elements, eg. 'abc*'")]
        [LocalizedStringPl("Wpisz pasujące elementy, np. 'abc*'")]
        public string EnterMatchingElements { get; set; }


        [LocalizedString("Activation successful")]
        [LocalizedStringPl("Aktywacja przebiegła pomyślnie")]
        public string ActivationSuccessful { get; set; }

        [LocalizedString("This product is licensed for:")]
        [LocalizedStringPl("Licencję na użytkowanie tego programu posiada:")]
        public string LicencedFor { get; set; }

        [LocalizedString("You are using demo version")]
        [LocalizedStringPl("Używasz wersji demonstracyjnej programu")]
        public string ThisIsDemoVersion { get; set; }

        [LocalizedString("{0} deays left")]
        [LocalizedStringPl("Pozostało {0} dni")]
        public string DemoDaysLeft { get; set; }

        [LocalizedString("Additional modules: ")]
        [LocalizedStringPl("Dodatkowe moduły: ")]
        public string AdditionalModules { get; set; }

        [LocalizedString("License key is valid")]
        [LocalizedStringPl("Klucz licencji jest prawidłowy")]
        public string LicenseIsValid { get; set; }

        [LocalizedString("License expiried")]
        [LocalizedStringPl("Licencja wygasła")]
        public string LicenseIsExpired { get; set; }

        [LocalizedString("License is invalid")]
        [LocalizedStringPl("Brak licencji")]
        public string LicenseIsInvalid { get; set; }

        [LocalizedString("The application has been updated. New version will not take effect until after you quit and launch the application again.")]
        [LocalizedStringPl("Nowa wersja programu jest dostępna! Uruchom ponownie program aby korzystać z najnowszej wersji.")]
        public string NewAppVersionNeedRestart { get; set; }

        [LocalizedString("New version avaiable!")]
        [LocalizedStringPl("Dostępna jest nowsza wersja aplikacji!")]
        public string NewAppVersionAvaiable { get; set; }

        [LocalizedString("Enter task name")]
        [LocalizedStringPl("Podaj nazwę zadania")]
        public string EnterTaskName { get; set; }

        [LocalizedString("Project ID")]
        [LocalizedStringPl("Identyfikator projektu")]
        public string ProjectId { get; set; }

        [LocalizedString("Project description")]
        [LocalizedStringPl("Opis projektu")]
        public string ProjectDsc { get; set; }

        [LocalizedString("Project")]
        [LocalizedStringPl("Projekt")]
        public string Project { get; set; }

        [LocalizedString("Projects count")]
        [LocalizedStringPl("Ilość projektów")]
        public string ProjectCount { get; set; }

        [LocalizedString("Projects list")]
        [LocalizedStringPl("Lista projektów")]
        public string ProjectList { get; set; }

        [LocalizedString("Task")]
        [LocalizedStringPl("Zadanie")]
        public string Task { get; set; }

        [LocalizedString("Tasks count")]
        [LocalizedStringPl("Ilość zadań")]
        public string TaskCount { get; set; }

        [LocalizedString("Tasks list")]
        [LocalizedStringPl("Lista zadań")]
        public string TaskList { get; set; }

        [LocalizedString("Workers list")]
        [LocalizedStringPl("Lista pracowników")]
        public string WorkerList { get; set; }

        [LocalizedString("Worker")]
        [LocalizedStringPl("Pracownik")]
        public string Worker { get; set; }

        [LocalizedString("{0} (Worked {1}, compensated {2})")]
        [LocalizedStringPl("{0} (Przepracowane {1}, Zrekompensowane {2})")]
        public string OvertimeWithDescription { get; set; }

        [LocalizedString("Sum")]
        [LocalizedStringPl("Razem")]
        public string Sum { get; set; }

        [LocalizedString("Other")]
        [LocalizedStringPl("Inne")]
        public string Other { get; set; }

        [LocalizedString("Work time")]
        [LocalizedStringPl("Czas pracy")]
        public string WorkTime { get; set; }

        [LocalizedString("Saving file...")]
        [LocalizedStringPl("Zapisywanie pliku...")]
        public string SavingFile { get; set; }

        [LocalizedString("Opening file...")]
        [LocalizedStringPl("Otwieranie pliku...")]
        public string OpeningFile { get; set; }

        [LocalizedString("Remove '{0}'")]
        [LocalizedStringPl("Usunąć '{0}'?")]
        public string RemoveElementQFmt { get; set; }

        [LocalizedString("Are You sure You want remove data?")]
        [LocalizedStringPl("Na pewno usunąć dane?")]
        public string RemoveDataQ { get; set; }

        [LocalizedString("Save changes?")]
        [LocalizedStringPl("Zapisać zmiany?")]
        public string SaveChangesQ { get; set; }

        [LocalizedString("Changes saved")]
        [LocalizedStringPl("Zmiany zostały zapisane")]
        public string ChangesSaved { get; set; }

        [LocalizedString("{0} items saved.")]
        [LocalizedStringPl("Zapisano {0} elementów.")]
        public string XItemsSaved { get; set; }

        [LocalizedString("{0} items loaded.")]
        [LocalizedStringPl("Wczytano {0} elementów.")]
        public string XItemsLoaded { get; set; }

        [LocalizedString("Update completed")]
        [LocalizedStringPl("Akutalizacja zakończona.")]
        public string UpdateCompleted { get; set; }

        [LocalizedString("It is recommended that you install certificates from GeoSoft. Do you want to do it now?")]
        [LocalizedStringPl("Zaleca się zainstalowanie certyfikatów firmy GeoSoft. Czy chcesz to zrobić teraz?")]
        public string InstallCertificateQ { get; set; }


        [LocalizedString("Yes")]
        [LocalizedStringPl("Tak")]
        public string Yes { get; set; }

        [LocalizedString("No")]
        [LocalizedStringPl("Nie")]
        public string No { get; set; }

        [LocalizedString("Save")]
        [LocalizedStringPl("Zapisz")]
        public string Save { get; set; }

        [LocalizedString("Add")]
        [LocalizedStringPl("Dodaj")]
        public string Add { get; set; }

        [LocalizedString("Edit")]
        [LocalizedStringPl("Edytuj")]
        public string Edit { get; set; }

        [LocalizedString("Delete")]
        [LocalizedStringPl("Usuń")]
        public string Delete { get; set; }

        [LocalizedString("Delete?")]
        [LocalizedStringPl("Usunąć?")]
        public string DeleteQ { get; set; }

        [LocalizedString("Delete selected item?")]
        [LocalizedStringPl("Usunąć wybrany element?")]
        public string DeleteSelectedItemQ { get; set; }

        [LocalizedString("Invalid password")]
        [LocalizedStringPl("Nieprawidłowe hasło")]
        public string InvalidPassword { get; set; }

        [LocalizedString("Field value out of range {0}: {1} - {2}")]
        [LocalizedStringPl("Nieprawidłowa wartość '{0}'. Proszę wprowadzić wartość z zakresu {1}-{2}.")]
        public string InvalidFieldValueRange { get; set; }

        [LocalizedString("No selected day")]
        [LocalizedStringPl("Należy zaznaczyć co najmniej jeden dzień")]
        public string NoSelectedDay { get; set; }

        [LocalizedString("Selected item: ")]
        [LocalizedStringPl("Wybrano: ")]
        public string SelectedItem { get; set; }

        [LocalizedString("Select free day")]
        [LocalizedStringPl("Należy wybrać dzień wolny od pracy gdy nie wpisano początku i końca pracy")]
        public string NotFreeDayAndNotWorkTime { get; set; }



        [LocalizedString("Cancelled")]
        [LocalizedStringPl("Anulowano")]
        public string Canceled { get; set; }



        [LocalizedString("Invalid email")]
        [LocalizedStringPl("Nieprawidłowy e-mail")]
        public string InvalidEmail{ get; set; }

        
        [LocalizedString("Invalid company name")]
        [LocalizedStringPl("Nieprawidłowa nazwa firmy")]
        public  string InvalidCompanyName{ get; set; }


        [LocalizedString("Invalid login data. Try again.")]
        [LocalizedStringPl("Podano nieprawidłowe dane. Spróbuj jeszcze raz.")]
        public  string InvalidLoginData{ get; set; }

        [LocalizedString("Invalid company and/or user name")]
        [LocalizedStringPl("Nieprawidłowa nazwa firmy i/lub pracownika")]
        public  string InvalidCompanyUserName{ get; set; } 

        [LocalizedString("Company name exists")]
        [LocalizedStringPl("Podana nazwa firmy już istnieje")]
        public  string CompanyNameExists{ get; set; }

        [LocalizedString("Invalid chars")]
        [LocalizedStringPl("Podano nieprawidłowe znaki")]
        public  string InvalidChars{ get; set; }

        [LocalizedString("Cannot select free day")]
        [LocalizedStringPl("Nie można wybrać dnia wolnego od pracy gdy wpisane są początek lub koniec pracy")]
        public  string FreeDayAndWorkTime{ get; set; }



        [LocalizedString("Loading...")]
        [LocalizedStringPl("Wczytywanie...")]
        public string Loading { get; set; }

        [LocalizedString("Loaded")]
        [LocalizedStringPl("Wczytano")]
        public string Loaded { get; set; }

        [LocalizedString("Saving...")]
        [LocalizedStringPl("Zapisywanie... ")]
        public string Saving { get; set; }

        [LocalizedString("Error")]
        [LocalizedStringPl("Błąd")]
        public string Error { get; set; }

        [LocalizedString("Warning")]
        [LocalizedStringPl("Ostrzeżenie")]
        public string Warning { get; set; }

        [LocalizedString("Information")]
        [LocalizedStringPl("Informacja")]
        public string Information { get; set; }

        

        [LocalizedString("Street Map")]
        [LocalizedStringPl("Mapa Ogólna")]
        public string StreetMap { get; set; }

        [LocalizedString("Topo Map")]
        [LocalizedStringPl("Mapa Topograficzna")]
        public string TopoMap { get; set; }

        [LocalizedString("Imagery Map")]
        [LocalizedStringPl("Mapa Satelitarna")]
        public string ImageryMap { get; set; }

        [LocalizedString("Imagery with labels")]
        [LocalizedStringPl("Mapa Satelitarna z opisami")]
        public string ImageryWithLabels { get; set; }

        [LocalizedString("Add Layer")]
        [LocalizedStringPl("Dodaj Warstwę")]
        public string AddLayer { get; set; }

        [LocalizedString("Add Objects")]
        [LocalizedStringPl("Dodaj Obiekty")]
        public string AddObjects { get; set; }

        [LocalizedString("Show Layers")]
        [LocalizedStringPl("Pokaż Warstwy")]
        public string ShowLayers { get; set; }

        [LocalizedString("Find Parcel")]
        [LocalizedStringPl("Znajdź działkę")]
        public string FindParcel { get; set; }

        [LocalizedString("Layers")]
        [LocalizedStringPl("Warstwy")]
        public string Layers { get; set; }



        [LocalizedString("Find Parcels")]
        [LocalizedStringPl("Znajdź działki")]
        public string FindParcels { get; set; }

        [LocalizedString("Objects")]
        [LocalizedStringPl("Obiekty")]
        public string ObjectsLayer { get; set; }

        [LocalizedString("Points")]
        [LocalizedStringPl("Punkty")]
        public string Points { get; set; }

        [LocalizedString("Line")]
        [LocalizedStringPl("Linia")]
        public string Line { get; set; }

        [LocalizedString("Area")]
        [LocalizedStringPl("Obszar")]
        public string Area { get; set; }


        [LocalizedString("Water")]
        [LocalizedStringPl("Woda")]
        public string Water { get; set; }

        [LocalizedString("Land")]
        [LocalizedStringPl("Ląd")]
        public string Land { get; set; }



        [LocalizedString("CSV - Comma separated values")]
        [LocalizedStringPl("CSV - tekst rozdzielany separatorem")]
        public string CsvFileDescription { get; set; }

        [LocalizedString("Text file")]
        [LocalizedStringPl("Plik tekstowy")]
        public string TxtFileDescription { get; set; }

        [LocalizedString("Tab delimited text")]
        [LocalizedStringPl("Tekst rozdzielany tabulatorem")]
        public string TabFileDescription { get; set; }


        [LocalizedString("Fixed width text")]
        [LocalizedStringPl("Tekst o stałej szerokości")]
        public string FixedWidthText { get; set; }

        [LocalizedString("Delimited text")]
        [LocalizedStringPl("Tekst rozdzielany separatorem")]
        public string DelimitedText { get; set; }

        [LocalizedString("Space")]
        [LocalizedStringPl("Spacja")]
        public string Space { get; set; }

        [LocalizedString("Tab")]
        [LocalizedStringPl("Tabulator")]
        public string Tab { get; set; }

        [LocalizedString("Semicolon")]
        [LocalizedStringPl("Średnik")]
        public string Semicolon { get; set; }

        [LocalizedString("Comma")]
        [LocalizedStringPl("Przecinek")]
        public string Comma { get; set; }

        public string DelimitedFileDescription(params string[] delimiters)
        {
            var delimTexts = delimiters.ToList();
            for (int i = 0; i < delimTexts.Count; i++)
            {
                delimTexts[i] = delimTexts[i]
                    .Replace(" ", SysUtils.Strings.Space)
                    .Replace("\t", SysUtils.Strings.Tab)
                    .Replace(";", SysUtils.Strings.Semicolon)
                    .Replace(",", SysUtils.Strings.Comma);
            }
            return SysUtils.Strings.DelimitedText + " (" + delimTexts.Join(", ") + ")";
        }

        
       

        [LocalizedString("Loading points...")]
        [LocalizedStringPl("Wczytywanie punktów... ")]
        public string LoadingPoints { get; set; }

        [LocalizedString("{0} points loaded.")]
        [LocalizedStringPl("Wczytano {0} punktów.")]
        public string XPointsLoaded { get; set; }

        [LocalizedString("{0} points saved. ")]
        [LocalizedStringPl("Zapisano {0} punktów. ")]
        public string XPointsSaved { get; set; }



        [LocalizedString("Enter insert point")]
        [LocalizedStringPl("Wskaż punkt wstawienia")]
        public string EnterInsertionPoint { get; set; }

        [LocalizedString("Enter start point")]
        [LocalizedStringPl("Wskaż punkt początkowy")]
        public string EnterStartPoint { get; set; }

        [LocalizedString("Enter end point")]
        [LocalizedStringPl("Wskaż punkt końcowy")]
        public string EnterEndPoint { get; set; }

        [LocalizedString("Enter point")]
        [LocalizedStringPl("Wskaż punkt")]
        public string EnterPoint { get; set; }

        [LocalizedString("Enter point ID")]
        [LocalizedStringPl("Podaj identyfikator punktu")]
        public string EnterPointId { get; set; }

        [LocalizedString("Enter block name")]
        [LocalizedStringPl("Podaj nazwę bloku")]
        public string EnterBlockName { get; set; }

        [LocalizedString("Enter text height")]
        [LocalizedStringPl("Podaj wysokość tekstu")]
        public string EnterTextHeight { get; set; }

        [LocalizedString("Enter text position")]
        [LocalizedStringPl("Wskaż pozycję testu")]
        public string EnterTextPosition { get; set; }

        [LocalizedString("Select block")]
        [LocalizedStringPl("Wskaż blok")]
        public string SelectBlock { get; set; }

        [LocalizedString("Select text")]
        [LocalizedStringPl("Wskaż tekst")]
        public string SelectText { get; set; }

        [LocalizedString("Select line")]
        [LocalizedStringPl("Wskaż linię")]
        public string SelectLine { get; set; }




        [LocalizedString("Tools")]
        [LocalizedStringPl("Narzędzia")]
        public string Tools { get; set; }

        [LocalizedString("Info")]
        [LocalizedStringPl("Info")]
        public string Info { get; set; }

        [LocalizedString("Rysowanie")]
        [LocalizedStringPl("Drawing")]
        public string Drawing { get; set; }

        [LocalizedString("Blocks")]
        [LocalizedStringPl("Bloki")]
        public string Blocks { get; set; }

        [LocalizedString("Configuration")]
        [LocalizedStringPl("Konfiguracja")]
        public string Configuration { get; set; }

        [LocalizedString("License")]
        [LocalizedStringPl("Licencja")]
        public string License { get; set; }

        [LocalizedString("Make suggestion")]
        [LocalizedStringPl("Zgłoś sugestię")]
        public string MakeSuggestion { get; set; }

        [LocalizedString("Show LOG file")]
        [LocalizedStringPl("Pokaż plik LOG")]
        public string ShowLogFile { get; set; }


        [LocalizedString("Loading lines...")]
        [LocalizedStringPl("Wczytywanie linii... ")]
        public string LoadingLines { get; set; }

        [LocalizedString("Creating polygons...")]
        [LocalizedStringPl("Tworzenie poligonów... ")]
        public string CreatingPolygons { get; set; }

        [LocalizedString("Deleting duplicates...")]
        [LocalizedStringPl("Usuwanie duplikatów... ")]
        public string DeletingDuplicates { get; set; }

        [LocalizedString("Processing data...")]
        [LocalizedStringPl("Przetwarzanie danych... ")]
        public string ProcessingData { get; set; }


        [LocalizedString("{0} entities updated")]
        [LocalizedStringPl("Zaktualizowano {0} obiektów. ")]
        public string XEntitesUpdated { get; set; }


        [LocalizedString("Ribbon with tools is not active - change worspace")]
        [LocalizedStringPl("Wstążka z narzędziami (Ribbon) nie jest aktywna - należy zmienić przestrzeń roboczą (Workspace)")]
        public string RibbonNotFound { get; set; }




        [LocalizedString("Control point")]
        [LocalizedStringPl("Punkt osnowy")]
        public string ControlPoint { get; set; }

        [LocalizedString("Boundary point")]
        [LocalizedStringPl("Punkt graniczny")]
        public string BoundaryPoint { get; set; }

        [LocalizedString("Survey point")]
        [LocalizedStringPl("Punkt pomiarowy")]
        public string SurveyPoint { get; set; }

        [LocalizedString("Height point")]
        [LocalizedStringPl("Punkt wysokościowy")]
        public string HeightPoint { get; set; }

        [LocalizedString("BenchMark")]
        [LocalizedStringPl("Reper")]
        public string BenchMark { get; set; }


        [LocalizedString("Blocks import")]
        [LocalizedStringPl("Import bloków")]
        public string ImportBlocks { get; set; }

        [LocalizedString("Blocks export")]
        [LocalizedStringPl("Eksport bloków")]
        public string ExportBlocks { get; set; }

        [LocalizedString("Texts export")]
        [LocalizedStringPl("Eksport tekstów")]
        public string ExportTexts { get; set; }

        [LocalizedString("Export text and coordinates of text position")]
        [LocalizedStringPl("Eksportuje teksty oraz współrzędne wstawienia tekstu")]
        public string ExportTextsDsc { get; set; }

        [LocalizedString("Block settings")]
        [LocalizedStringPl("Ustawienia bloków")]
        public string BlockSettings { get; set; }

        [LocalizedString("Update DGN")]
        [LocalizedStringPl("Aktualizuj DGN")]
        public string ConvertDgnTexts { get; set; }

        [LocalizedString("Change Polish characters generatedy by Microstation")]
        [LocalizedStringPl("Zamienia krzaczki z zaimportowanych plików DGN na litery z polskimi ogonkami")]
        public string ConvertDgnTextsDsc { get; set; }

        [LocalizedString("Slope")]
        [LocalizedStringPl("Spadek")]
        public string Slope { get; set; }

        [LocalizedString("Calculate slope between two points")]
        [LocalizedStringPl("Oblicza spadek wyrażony w % pomiędzy dwoma punktami")]
        public string SlopeDsc { get; set; }

        [LocalizedString("Ortho")]
        [LocalizedStringPl("Orto")]
        public string Orto { get; set; }

        [LocalizedString("Set Ortho mode and begin line drawing. Use F10 button in order to turn Ortho mode off.")]
        [LocalizedStringPl("Ustawia tryb rysowania 'Orto' i rozpoczyna rysowanie polilinii. Aby wyłączyć tryb 'Orto' użyj przycisku F10.")]
        public string OrtoDsc { get; set; }

        [LocalizedString("Find block")]
        [LocalizedStringPl("Znajdź blok")]
        public string FindBlock { get; set; }




        [LocalizedString("License loaded. Restart application again to apply changes.")]
        [LocalizedStringPl("Licencja została wczytana. Uruchom aplikację ponownie aby wprowadzić zmiany.")]
        public string LicLoaded { get; set; }


        [LocalizedString("Show Geo7 palette")]
        [LocalizedStringPl("Pokazuje paletę narzędzi Geo7 Tools")]
        public string ShowToolsPaletteDsc { get; set; }
    }
}
