using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataAccessLayer;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Windows.Markup;
using BeLog.Logic;
using BeLogPrinting;
using System.IO;
using System.Collections.ObjectModel;
using System.Data;
using System.ComponentModel;
using BeLog.Logic.CustomEventArgs;

namespace BeLogForm
{
    public delegate void AddedCMREventHandler(object sender, CMRAddedEventArgs e);
    public delegate void EditedCMREventHandler(object sender, CMRAddedEventArgs e);
    /// <summary>
    /// Interaction logic for CMR.xaml
    /// </summary>
    public partial class CMRForm : Window
    {
        public event AddedCMREventHandler CMRAdded;
        public event EditedCMREventHandler CMREdited;
        public static RoutedCommand CreateCommand = null;
        string matrixPrinterName = String.Empty;
        string tagLabelPrinterName = String.Empty;
        User loggedUser = null;
        BaseLogic baseLogic = new BaseLogic();

        private Places DeliveryPlaces;
        private Places TakingPlaces;

        //public CMRForm(User loggedInUser) : this(loggedInUser, null, null) { }

        public CMRForm(User loggedInUser, Company consignor = null, Company consignee = null)
        {
            this.loggedUser = loggedInUser;
            InitializeComponent();
            try
            {
                InitializeCurrencies();
                InitializeCommands();
                InitializeConfigurationSettings();
                InitializeDatePickers();
                InitializeCompanyComboboxes();
                InitializeCountries();
                InitalizeDeparturePlace();
                InitializeComingPlace();

                if (mainGrid.DataContext == null)
                {
                    CMR cmr = generateNewCMR();
                    if (consignee != null) senderGrid.DataContext = consignor;
                    if (consignor != null) consigneeGrid.DataContext = consignee;
                    mainGrid.DataContext = cmr;
                    dataGrid1.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(dataGrid1_AutoGeneratingColumn);
                    dataGrid1.AutoGenerateColumns = true;
                    dataGrid1.CanUserAddRows = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("BeLog", ex.ToString());
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                Debug.WriteLine(ex.Source);
            }
        }

        private CMR generateNewCMR()
        {
            CMR cmr = new CMR();
            cmr.ADR = String.Empty;
            cmr.AttachedDocuments = String.Empty;
            cmr.Letter = String.Empty;
            cmr.Num = 0;
            cmr.PaymentInstruction = String.Empty;
            cmr.SendInstruction = String.Empty;
            cmr.SpecialAgreements = String.Empty;
            cmr.Transport = new Transport();
            cmr.Transport.Model = String.Empty;
            cmr.Transport.RegisterNumber = String.Empty;
            cmr.ConsigneePayment = new PaymentInfo();
            cmr.ConsigneePayment.Currency = String.Empty;
            cmr.ConsigneePayment.Saldo = 0;
            cmr.ConsigneePayment.Supplements = 0;
            cmr.ConsigneePayment.OtherCharges = 0;
            cmr.ConsigneePayment.Deductions = 0;
            cmr.ConsigneePayment.CarriageCharges = 0;
            cmr.SenderPayment = new PaymentInfo();
            cmr.SenderPayment.Currency = String.Empty;
            cmr.SenderPayment.Saldo = 0;
            cmr.SenderPayment.Supplements = 0;
            cmr.SenderPayment.OtherCharges = 0;
            cmr.SenderPayment.Deductions = 0;
            cmr.SenderPayment.CarriageCharges = 0;
            cmr.EstablieshedIn = "TALLINN";
            cmr.ClassValue = String.Empty;
            cmr.SenderID = 0;
            cmr.ConsigneeID = 0;
            //cmr.Sender = new Company();
            //cmr.Sender.Name = String.Empty;
            //cmr.Sender.Address = String.Empty;
            //cmr.Sender.CompanyType = (int)ComboType.Sender;
            //cmr.Sender.Country = String.Empty;
            //cmr.Consignee = new Company();
            //cmr.Consignee.Name = String.Empty;
            //cmr.Consignee.Address = String.Empty;
            //cmr.Consignee.CompanyType = (int)ComboType.Consignee;
            //cmr.Consignee.Country = String.Empty;
            //cmr.Carrier = new Company();
            //cmr.Carrier.CompanyType = (int)ComboType.Carrier;
            //cmr.Carrier.Name = String.Empty;
            //cmr.Carrier.Address = String.Empty;
            //cmr.Carrier.Country = String.Empty;
            //cmr.NextCarrier = new Company();
            //cmr.NextCarrier.CompanyType = (int)ComboType.SuccCarrier;
            //cmr.NextCarrier.Name = String.Empty;
            cmr.EstablieshedDate = DateTime.Now;
            cmr.TimeOfDep = DateTime.Now;
            //cmr.NextCarrier.Address = String.Empty;
            //cmr.NextCarrier.Country = String.Empty;
            OrderItem orderItem = new OrderItem();
            orderItem.MarkAndNum = String.Empty;
            orderItem.PackagingMethod = String.Empty;
            orderItem.GoodsNature = String.Empty;
            orderItem.StartNumber = String.Empty;
            cmr.UserCreated = loggedUser.Abreviation;
            cmr.OrderItems.Add(orderItem);

            cmr.OrderItems.ListChanged += new System.ComponentModel.ListChangedEventHandler(OrderItemsListChanged);
            return cmr;
        }

        private void OrderItemsListChanged(Object sender, System.ComponentModel.ListChangedEventArgs e)
        {

            ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid1.Items);
            IEditableCollectionView iecv = (IEditableCollectionView)view;

            System.Diagnostics.Debug.Print("isAdding = " + iecv.IsAddingNew.ToString());

            if (dataGrid1.Items.Count > 6)
            {
                MessageBox.Show("You can't add more that 6 rows!");
                System.Diagnostics.Debug.Print("Rows are already 6, no more!!!");
                iecv.CancelNew();
            }

        }

        private void InitializeCommands()
        {
            CreateCommand = new RoutedCommand();
            CommandBinding cb = new CommandBinding(CreateCommand,
            CreateCommandExecute, CreateCommandCanExecute);
            this.CommandBindings.Add(cb);


            KeyGesture kg = new KeyGesture(Key.N, ModifierKeys.Control);
            InputBinding ib = new InputBinding(CreateCommand, kg);
            this.InputBindings.Add(ib);

            var OpenCommand = new RoutedCommand();
            CommandBinding cbOpen = new CommandBinding(OpenCommand,
            OpenCommandExecute, OpenCommandCanExecute);
            this.CommandBindings.Add(cbOpen);

            KeyGesture kgOpen = new KeyGesture(Key.O, ModifierKeys.Control);
            InputBinding ibOpen = new InputBinding(OpenCommand, kgOpen);
            this.InputBindings.Add(ibOpen);

            var StoreCommand = new RoutedCommand();
            CommandBinding cbStore = new CommandBinding(StoreCommand,
            StoreCommandExecute, StoreCommandCanExecute);
            this.CommandBindings.Add(cbStore);

            KeyGesture kgStore = new KeyGesture(Key.S, ModifierKeys.Control);
            InputBinding ibStore = new InputBinding(StoreCommand, kgStore);
            this.InputBindings.Add(ibStore);
             
            var PrintCommand = new RoutedCommand();
            CommandBinding cbPrint = new CommandBinding(PrintCommand,
            PrintCommandExecute, PrintCommandCanExecute);
            this.CommandBindings.Add(cbPrint);

            KeyGesture kgPrint = new KeyGesture(Key.P, ModifierKeys.Control);
            InputBinding ibPrint = new InputBinding(PrintCommand, kgPrint);
            this.InputBindings.Add(ibPrint);
        }
        private void InitializeConfigurationSettings()
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains("MatrixPrinterName"))
            {
                matrixPrinterName = ConfigurationManager.AppSettings["MatrixPrinterName"];
            }
            if (ConfigurationManager.AppSettings.AllKeys.Contains("TagLabelPrinterName"))
            {
                tagLabelPrinterName = ConfigurationManager.AppSettings["TagLabelPrinterName"];
            }
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("et-EE");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("et-EE");

        }

        private void InitalizeDeparturePlace()
        {
            cbxTakingPlace.ItemsSource = baseLogic.db.CMRs.Select(c => c.TakingPlace).Distinct().OrderBy(c => c);
            //TakingPlaces = new Places(baseLogic.db.Companies.Where(c => c.CompanyType == (int)BeLog.Logic.ComboType.TakingPlace).OrderBy(c => c.Name));
            //cbxTakingPlace.ItemsSource = this.TakingPlaces;
            //cbxTakingPlace.DisplayMemberPath = "Address";
        }
        private void InitializeComingPlace()
        {
            cbxDeliveryPlace.ItemsSource = baseLogic.db.CMRs.Select(c => c.DeliveryPlace).Distinct().OrderBy(c => c);
            //DeliveryPlaces = new Places(baseLogic.db.Companies.Where(c => c.CompanyType == (int)BeLog.Logic.ComboType.Consignee).OrderBy(c => c.Name));
            //cbxDeliveryPlace.ItemsSource = this.DeliveryPlaces;
            //cbxDeliveryPlace.DisplayMemberPath = "Address";
        }

        void dataGrid1_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "CMRID" || e.PropertyName == "CMR" || e.PropertyName == "ID")
            {
                e.Column.Visibility = System.Windows.Visibility.Hidden;
            }
            if (e.PropertyName == "PackagingMethod")
            {
                e.Column.Header = "Pakkimisviis";
            }
            else if (e.PropertyName == "MarkAndNum")
            {
                e.Column.Header = "Mark ja Number";
            }
            else if (e.PropertyName == "NumOfPackages")
            {
                e.Column.Header = "Kohtade arv";
            }
            else if (e.PropertyName == "StartNumber")
            {
                e.Column.Header = "Mõõtühik";
            }
            else if (e.PropertyName == "GoodsNature")
            {
                e.Column.Header = "Kauba nimetus";
            }
            else if (e.PropertyName == "PackagingMethodName")
            {
                e.Column.Header = "Pakkimisviis";
            }
            else if (e.PropertyName == "Weight")
            {
                e.Column.Header = "Brutokaal, kg";
            }
            else if (e.PropertyName == "Volume")
            {
                e.Column.Header = "Maht, м³";
            }

        }

        private void datePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {

        }
        public class CountryInfo
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }
        public void InitializeCountries()
        {
            string[] countryList = BeLogForm.Properties.Resources.CountryList.Split('\n');
            string[] priorCountryList = BeLogForm.Properties.Resources.PriorCountries.Split('\n');
            List<CountryInfo> ciList = new List<CountryInfo>();

            foreach (var countryRow in countryList)
            {
                var countryName = countryRow.Substring(0, countryRow.Length - 3).Trim();
                var countryCode = countryRow.Substring(countryRow.Length - 3, 2);
                countryCode = countryCode.Trim();
                if (countryCode != String.Empty)
                {

                    CountryInfo ci = new CountryInfo();
                    ci.Name = countryName;
                    ci.Value = countryCode;
                    ciList.Add(ci);
                }
            }
            ciList = PrioritizeCountries(ciList, priorCountryList);
            cbxConsigneeCountry.ItemsSource = ciList;
            cbxSenderCountry.ItemsSource = ciList;
            cbxTakingCountry.ItemsSource = ciList;
            cbxDeliveryCountry.ItemsSource = ciList;
            //cbxCarrierCountry.ItemsSource = ciList;
            //cbxSuccCarrierCountry.ItemsSource = ciList;

            cbxConsigneeCountry.SelectedValue = ((List<CountryInfo>)cbxConsigneeCountry.Items.SourceCollection).Where(c => c.Value == "EE").Single();
            cbxSenderCountry.SelectedValue = ((List<CountryInfo>)cbxSenderCountry.Items.SourceCollection).Where(c => c.Value == "EE").Single();
            cbxTakingCountry.SelectedValue = ((List<CountryInfo>)cbxTakingCountry.Items.SourceCollection).Where(c => c.Value == "EE").Single();
            cbxDeliveryCountry.SelectedValue = ((List<CountryInfo>)cbxDeliveryCountry.Items.SourceCollection).Where(c => c.Value == "EE").Single();
            //cbxCarrierCountry.SelectedValue = "EE";
            //cbxSuccCarrierCountry.SelectedValue = "EE";
        }
        public void InitializeCurrencies()
        {

            string[] currList = BeLogForm.Properties.Resources.CurrencyList.Split('\n');
            foreach (var curr in currList)
            {
                var currVal = curr.Substring(0, 3);
                cbxCurrBalance.Items.Add(currVal);
                cbxCurrCarrChrge.Items.Add(currVal);
                cbxCurrDeductions.Items.Add(currVal);
                cbxCurrOthChrge.Items.Add(currVal);
                cbxCurrSupp.Items.Add(currVal);

            }
            cbxCurrBalance.SelectedValue = "EUR";
            cbxCurrCarrChrge.SelectedValue = "EUR";
            cbxCurrDeductions.SelectedValue = "EUR";
            cbxCurrOthChrge.SelectedValue = "EUR";
            cbxCurrSupp.SelectedValue = "EUR";

        }
        public void InitializeDatePickers()
        {
            datePicker2.Language = XmlLanguage.GetLanguage("et-EE");
            datePicker2.DisplayDate = DateTime.Now;
            datePicker3.Language = XmlLanguage.GetLanguage("et-EE");
            datePicker3.DisplayDate = DateTime.Now;
        }
        public void InitializeCompanyComboboxes()
        {
            //InitializeComboBoxItems(cbxSenderName, ComboType.Sender);
            //InitializeComboBoxItems(cbxConsigneeName, ComboType.Consignee);
            //InitializeComboBoxItems(cbxCarrierName, ComboType.Carrier);
            //InitializeComboBoxItems(cbxSuccCarrierName, ComboType.SuccCarrier);
            initSenderCBX();
            initConsigneeCBX();

        }

        private void InitializeComboBoxItems(ComboBox cbx, ComboType cbxType)
        {
            cbx.ItemsSource = baseLogic.getComboList(cbxType);
            cbx.SelectedValuePath = "ID";
            cbx.DisplayMemberPath = "Name";
        }

        private void initSenderCBX()
        {
            this.senderGrid.DataContext = new Company();
            this.cbxSenderName.ItemsSource = baseLogic.getComboList(ComboType.Sender);
            this.cbxSenderName.DisplayMemberPath = "Name";
            this.cbxSenderName.SelectedValuePath = "ID";
        }

        private void initConsigneeCBX()
        {
            this.consigneeGrid.DataContext = new Company();
            this.cbxConsigneeName.ItemsSource = baseLogic.getComboList(ComboType.Consignee);
            this.cbxConsigneeName.DisplayMemberPath = "Name";
            this.cbxConsigneeName.SelectedValuePath = "ID";
        }

        private int checkIfCompanyExist(Company company)
        {
            Company cco;

            try
            {

                if (company.ID == 0) // new company !
                {

                    if (baseLogic.db.Companies.Where(c => c.Name == company.Name).Count() > 0)
                    {
                        Exception ex = new Exception("There is already a company with that name");
                        throw ex;
                    }
                    else
                    {
                        baseLogic.db.Companies.InsertOnSubmit(company);
                        baseLogic.db.SubmitChanges();
                        cco = company;
                    }
                }
                else // existing company !
                {
                    cco = baseLogic.db.Companies.Where(c => c.ID == company.ID).First();
                }

            }
            catch (System.Data.SqlClient.SqlException sqlex)
            {
                throw sqlex;
            }
            catch (InvalidOperationException iex)
            {
                throw iex;
            }


            return cco.ID;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("et-EE");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("et-EE");
            string errorMessage = String.Empty;
            if (ValidateObligatoryFields(ref errorMessage))
            {
                try
                {

                    if (dataGrid1.Items != null && dataGrid1.Items.Count != 0)
                    {
                        CMR cmr = (CMR)mainGrid.DataContext;

                        try
                        {
                            Company t_sender = (Company)senderGrid.DataContext;
                            cmr.SenderID = checkIfCompanyExist(t_sender);

                            Company t_consignee = (Company)consigneeGrid.DataContext;
                            cmr.ConsigneeID = checkIfCompanyExist(t_consignee);

                            // Not used fields , so I'll just fill with a nonsence data
                            cmr.CarrierID = cmr.ConsigneeID; // empty
                            cmr.NextCarrierID = cmr.ConsigneeID; // empty name 

                            //cmr.DeliveryPlace = this.cbxDeliveryPlace.Text;
                            //CountryInfo ci = (CountryInfo)this.cbxDeliveryCountry.SelectedItem;
                            //cmr.DeliveryCountry = ci.Value;

                            //ci = (CountryInfo)this.cbxTakingCountry.SelectedItem;
                            //cmr.TakingCountry = ci.Value;
                            //cmr.TakingPlace = this.cbxTakingPlace.Text;

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }

                        setEmptyFields(ref cmr);
                        cmr.SenderPayment.Currency = cbxCurrCarrChrge.Text;
                        cmr.ConsigneePayment.Currency = cbxCurrCarrChrge.Text;
                        GatherTruckInfo(ref cmr, Truck.Second);
                        if (tbxCMRId.Text == String.Empty)
                        {
                            baseLogic.db.CMRs.InsertOnSubmit(cmr);
                        }

                        //baseLogic.submitChanges();
                        baseLogic.db.SubmitChanges();
                        //todo: remove cmr printing when manifest will be done to the end
                     //   ReportPrintDocument.PrintReport(cmr.ID, matrixPrinterName);
                        tbxCMRId.Text = cmr.ID.ToString();

                        //if(tbxCMRId.Text == String.Empty)
                        //{
                        AddedCMREventHandler cmrAdedd = CMRAdded;
                        if (cmrAdedd != null)
                        {
                            cmrAdedd(null, new CMRAddedEventArgs(cmr.ID));
                            this.Close();
                        }
                        //}
                        //else
                        //{
                        //     EditedCMREventHandler cmrEdited = CMREdited;
                        //    if (cmrEdited != null)
                        //    {
                        //        cmrEdited(null,new CMRAddedEventArgs(cmr.ID));
                        //    }
                        //}


                    }
           //         InitializeCompanyComboboxes();
           //         InitializeComingPlace();
            //        InitalizeDeparturePlace();
                }
                catch (System.Data.SqlClient.SqlException sex)
                {
                    MessageBox.Show(sex.Message, "SqlError", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Data.Linq.ChangeConflictException cex)
                {
                    //MessageBox.Show(cex.Message, "ChangeConflictError", MessageBoxButton.OK, MessageBoxImage.Error);
                    // try it once
                    System.Diagnostics.Debug.Print(cex.Message + " but I will try again");
                    baseLogic = new BaseLogic();
                    this.btnPrint_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(errorMessage);
            }
        }

        public void setEmptyFields(ref CMR cmr)
        {
            foreach (var property in cmr.GetType().GetProperties())
            {
                if (property.PropertyType.Name == "String")
                {
                    if (property.GetValue(cmr, null) == null)
                    {
                        property.SetValue(cmr, String.Empty, null);
                    }
                }
            }
        }
        public void setEmptyFieldsOrderItem(ref OrderItem orderItem)
        {
            foreach (var property in orderItem.GetType().GetProperties())
            {
                if (property.PropertyType.Name == "String")
                {
                    if (property.GetValue(orderItem, null) == null)
                    {
                        property.SetValue(orderItem, String.Empty, null);
                    }
                }
            }
        }

        public void GatherPaymentInfo(ref CMR cmr)
        {
            foreach (string paymentType in Enum.GetNames(typeof(PaymentType)))
            {
                PaymentInfo pmntInfo = new PaymentInfo();
                TextBox tbxCarrChrge = (TextBox)mainGrid.FindName("tbx" + paymentType + "CarrChrge");
                TextBox tbxDeductions = (TextBox)mainGrid.FindName("tbx" + paymentType + "Deductions");
                TextBox tbxBalance = (TextBox)mainGrid.FindName("tbx" + paymentType + "Balance");
                TextBox tbxSupplements = (TextBox)mainGrid.FindName("tbx" + paymentType + "Supplements");
                TextBox tbxOthChrge = (TextBox)mainGrid.FindName("tbx" + paymentType + "OthChrge");
                pmntInfo.CompanyID = 0;
                pmntInfo.Currency = ((string)((ComboBox)mainGrid.FindName("cbxCurrOthChrge")).SelectedValue).Replace("\t", "");
                pmntInfo.Deductions = setPmntInfoPrice(tbxDeductions);
                pmntInfo.CarriageCharges = setPmntInfoPrice(tbxCarrChrge);
                pmntInfo.Saldo = setPmntInfoPrice(tbxBalance);
                pmntInfo.Supplements = setPmntInfoPrice(tbxSupplements);
                pmntInfo.OtherCharges = setPmntInfoPrice(tbxOthChrge);
                if (paymentType == "Cons")
                {
                    if (cmr.SenderPayment == null) cmr.SenderPayment = pmntInfo;
                    else
                    {
                        cmr.SenderPayment.Currency = ((string)((ComboBox)mainGrid.FindName("cbxCurrOthChrge")).SelectedValue).Replace("\t", "");
                    }
                }
                else
                {
                    if (cmr.ConsigneePayment == null) cmr.ConsigneePayment = pmntInfo;
                    else
                    {
                        cmr.ConsigneePayment.Currency = ((string)((ComboBox)mainGrid.FindName("cbxCurrOthChrge")).SelectedValue).Replace("\t", "");
                    }
                }
            }
        }
        private decimal setPmntInfoPrice(TextBox tbx)
        {
            decimal result = 0.0M;
            if (tbx.Text != String.Empty)
            {
                Decimal.TryParse(tbx.Text, out result);
            }
            return result;
        }

        private void GatherCompaniesInfo(ref CMR cmr)
        {
            foreach (string comboType in Enum.GetNames(typeof(ComboType)))
            {
                Company cpn = new Company();
                int companyID = int.MinValue;
                ComboBox cbxName = (ComboBox)mainGrid.FindName("cbx" + comboType + "Name");
                if (cbxName.SelectedValue != null)
                {
                    companyID = (int)cbxName.SelectedValue;
                    if (comboType == "Sender")
                    {
                        cmr.SenderID = companyID;
                    }
                    else if (comboType == "Consignee")
                    {
                        cmr.ConsigneeID = companyID;
                    }
                    else if (comboType == "Carrier")
                    {
                        //if (cmr.Carrier != null) cmr.Carrier = null;
                        cmr.CarrierID = companyID;
                    }
                    else if (comboType == "SuccCarrier")
                    {
                        cmr.NextCarrierID = companyID;
                    }
                }
                else
                {
                    ComboBox cbxAdress = (ComboBox)mainGrid.FindName("cbx" + comboType + "Adress");
                    ComboBox cbxCountry = (ComboBox)mainGrid.FindName("cbx" + comboType + "Country");
                    cpn.CompanyType = (int)Enum.Parse(typeof(ComboType), comboType);

                    int id = int.MinValue;
                    var res = baseLogic.db.Companies.Where(c => c.Address.Trim() == cbxAdress.Text.Trim() && c.Country.Trim() == cbxCountry.Text.Trim()
                         && c.CompanyType == (int)Enum.Parse(typeof(ComboType), comboType));
                    if (res.Count() > 0)
                    {
                        id = res.First().ID;
                    }
                    cpn.Name = cbxName.Text.Trim();
                    cpn.Address = cbxAdress.Text.Trim();
                    if (cbxCountry.SelectedItem == null)
                    {
                        cpn.Country = String.Empty;
                    }
                    else
                    {
                        cpn.Country = ((CountryInfo)cbxCountry.SelectedItem).Value;
                    }
                    if (comboType == "Sender")
                    {
                        if (id != int.MinValue) cmr.SenderID = id;
                        else
                        {
                            //cmr.Sender = cpn;
                        }
                    }
                    else if (comboType == "Consignee")
                    {
                        if (id != int.MinValue) cmr.ConsigneeID = id;
                        else
                        {
                            //cmr.Consignee = cpn;
                        }
                    }
                    else if (comboType == "Carrier")
                    {
                        if (id != int.MinValue)
                        {
                            cmr.CarrierID = id;
                        }
                        else
                        {
                            //cmr.Carrier = cpn;
                        }
                    }
                    else if (comboType == "SuccCarrier")
                    {
                        if (id != int.MinValue)
                        {

                            cmr.NextCarrierID = id;
                        }
                        else
                        {
                            //cmr.NextCarrier = cpn;
                        }
                    }
                }
            }
        }
        public void GatherTruckInfo(ref CMR cmr, Truck truck)
        {
            switch (truck)
            {
                case Truck.Second:
                    {
                        //Transport transport = new Transport();
                        //transport.Model = tbxTruck2.Text;
                        //transport.RegisterNumber = tbxRegNum2.Text;
                        //var res = baseLogic.db.Transports.Where(c => c.Model == transport.Model && c.RegisterNumber == transport.RegisterNumber);
                        //if (res.Count() == 1)
                        //{
                        //    cmr.TransportID2 = res.Select(c => c.ID).First();
                        //}
                        //else if (transport.Model != String.Empty && transport.RegisterNumber != String.Empty)
                        //{
                        //    cmr.TransportID2 = baseLogic.insertTransportInfo(transport);
                        //}
                        //else
                        //{
                        //    cmr.TransportID2 = new Nullable<int>();
                        //}

                        break;
                    }
                case Truck.First:
                    {
                        if (cmr.Transport == null) cmr.Transport = new Transport();
                        cmr.Transport.Model = tbxTruck.Text;
                        cmr.Transport.RegisterNumber = tbxRegNum.Text;
                        break;
                    }
                default:
                    break;
            }
        }

        private void cbxCompanyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;
            cbx.IsEditable = true;
            string cbxName = cbx.Name.Replace("cbx", "").Replace("Name", "");

            if (cbx.SelectedValue != null)
            {
                setOtheCompanyInfo(cbx, cbxName, (int)cbx.SelectedValue);
            }
            else
            {
                setOtheCompanyInfo(cbx, cbxName, int.MinValue);
            }
        }
        public void setOtheCompanyInfo(ComboBox cbx, string containsName, int ID)
        {
            ComboBox cbxAdress = (ComboBox)this.FindName("cbx" + containsName + "Adress");
            ComboBox cbxCountry = (ComboBox)this.FindName("cbx" + containsName + "Country");
            Grid grd = (Grid)this.FindName(containsName.ToLower() + "Grid");
            cbxAdress.IsEditable = true;
            cbxCountry.IsEditable = false;
            if (ID != int.MinValue)
            {
                try
                {
                    Company co = baseLogic.db.Companies.Where(c => c.ID == ID).Single();
                    grd.DataContext = co;

                }
                catch (NullReferenceException x)
                {
                    throw x;
                }
            }
            else
            {
                //var tempCompany =   new Company();
                //tempCompany.CompanyType =(int)Enum.Parse(typeof(ComboType), containsName);
                //grd.DataContext = tempCompany;
                cbx.Text = cbx.Text;
                /*
                 * Commented address, country cleaning because of the last bug report
                 * 26.11.2010 Let's see if it will work
                 */
                //cbxAdress.Text = String.Empty;
                //cbxCountry.Text = String.Empty;


            }
        }

        private void tbxTotalCons_TextChanged(object sender, TextChangedEventArgs e)
        {
            float fl = 0;
            TextBox tbx = (TextBox)sender;
            if (!float.TryParse(tbx.Text, out fl) && fl == 0)
            {
                tbx.Text = "0";
            }
        }


        private void fileExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public string returnMessageInfo(Control ctrl, string extraInfo)
        {
            Label lbl = ctrl as Label;
            if (lbl != null)
            {
                if (extraInfo == "Consignee") extraInfo = "Saaja";
                else if (extraInfo == "Sender") extraInfo = "Saatja";
                return String.Format("Vaja täita {0} {1}", extraInfo, lbl.Content);
            }
            return String.Empty;
        }
        public bool ValidateObligatoryFields(ref string errorMessage)
        {
            StringBuilder strBldr = new StringBuilder();
            foreach (string comboType in Enum.GetNames(typeof(ComboType)))
            {
                if (comboType == "Consignee" || comboType == "Sender")
                {
                    ComboBox cbxName = (ComboBox)mainGrid.FindName("cbx" + comboType + "Name");
                    ComboBox cbxAdress = (ComboBox)mainGrid.FindName("cbx" + comboType + "Adress");
                    ComboBox cbxCountry = (ComboBox)mainGrid.FindName("cbx" + comboType + "Country");

                    if (!FormInfoGatherer.ValidateControl(cbxName))
                    {
                        strBldr.AppendLine(returnMessageInfo((Control)mainGrid.FindName("lbl" + comboType + "Name"), comboType));

                    }

                    if (!FormInfoGatherer.ValidateControl(cbxAdress))
                    {
                        strBldr.AppendLine(returnMessageInfo((Control)mainGrid.FindName("lbl" + comboType + "Adress"), comboType));
                    }
                    if (!FormInfoGatherer.ValidateControl(cbxCountry))
                    {
                        strBldr.AppendLine(returnMessageInfo((Control)mainGrid.FindName("lbl" + comboType + "Country"), comboType));
                    }
                }
            }
            if (!FormInfoGatherer.ValidateControl(cbxTakingPlace))
            {
                strBldr.AppendLine(returnMessageInfo(lblTakingPlace, "Pealelaadimiskoht"));
            }
            if (!FormInfoGatherer.ValidateControl(cbxTakingCountry))
            {
                strBldr.AppendLine(returnMessageInfo(lblTakingCountry, "Pealelaadimiskoht"));
            }
            if (!FormInfoGatherer.ValidateControl(cbxDeliveryPlace))
            {
                strBldr.AppendLine(returnMessageInfo(lblDeliveryPlace, "Mahalaadimiskoht"));
            }
            if (!FormInfoGatherer.ValidateControl(cbxDeliveryCountry))
            {
                strBldr.AppendLine(returnMessageInfo(lblDeliveryCountry, "Mahalaadimiskoht"));
            }

            if (!FormInfoGatherer.ValidateSenderInstructions(this.tbxSendInstructions))
            {
                strBldr.AppendLine("SendInstructions: too many lines / line too long");
            }

            FormInfoGatherer.ValidateDataGrid(dataGrid1, ref strBldr);

            if (strBldr.Length > 0)
            {
                errorMessage = strBldr.ToString();
                return false;
            }
            return true;
        }

        private void btn_CMRSearch_Click(object sender, RoutedEventArgs e)
        {
            CMRSearch cmrSearch = new CMRSearch();
            cmrSearch.ShowDialog();
            if (cmrSearch.Result != null && cmrSearch.Result != String.Empty)
            {

                //// just for checking
                //CMR cmr = generateNewCMR();
                //mainGrid.DataContext = cmr;
                //this.senderGrid.DataContext = new Company();
                //this.consigneeGrid.DataContext = new Company();
                //dataGrid1.ItemsSource = cmr.OrderItems;

                tbxCMRId.Text = cmrSearch.Result;
                CMR selectedCmr = baseLogic.db.CMRs.Where(c => c.ID == int.Parse(cmrSearch.Result)).First();
                mainGrid.DataContext = selectedCmr;

                // set sender & consignee
                if (baseLogic.db.Companies.Where(c => c.ID == selectedCmr.SenderID).Count() > 0)
                    this.senderGrid.DataContext = baseLogic.db.Companies.Where(c => c.ID == selectedCmr.SenderID).First();
                else
                    this.senderGrid.DataContext = new Company();

                if (baseLogic.db.Companies.Where(c => c.ID == selectedCmr.ConsigneeID).Count() > 0)
                    this.consigneeGrid.DataContext = baseLogic.db.Companies.Where(c => c.ID == selectedCmr.ConsigneeID).First();
                else
                    this.consigneeGrid.DataContext = new Company();

                dataGrid1.ItemsSource = ((CMR)mainGrid.DataContext).OrderItems;
                ((CMR)mainGrid.DataContext).OrderItems.ListChanged += new System.ComponentModel.ListChangedEventHandler(OrderItemsListChanged);
            }
        }

        private void btnNewCMR_Click(object sender, RoutedEventArgs e)
        {
            //cbxCarrierName.SelectionChanged -= cbxCompanyName_SelectionChanged;
            cbxConsigneeName.SelectionChanged -= cbxCompanyName_SelectionChanged;
            cbxSenderName.SelectionChanged -= cbxCompanyName_SelectionChanged;
            //cbxSuccCarrierName.SelectionChanged -= cbxCompanyName_SelectionChanged;
            //tbxRegNum2.Text = String.Empty;
            //tbxTruck2.Text = String.Empty;
            tbxCMRId.Text = String.Empty;

            CMR cmr = generateNewCMR();
            mainGrid.DataContext = cmr;

            this.cbxSenderName.Text = String.Empty;
            this.cbxConsigneeName.Text = String.Empty;

            this.senderGrid.DataContext = new Company();
            this.consigneeGrid.DataContext = new Company();
            dataGrid1.ItemsSource = cmr.OrderItems;
            //cbxCarrierName.SelectionChanged += cbxCompanyName_SelectionChanged;
            cbxConsigneeName.SelectionChanged += cbxCompanyName_SelectionChanged;
            cbxSenderName.SelectionChanged += cbxCompanyName_SelectionChanged;
            //cbxSuccCarrierName.SelectionChanged += cbxCompanyName_SelectionChanged;
        }

        private void CreateCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CreateCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            btnNewCMR_Click(null, null);
        }
        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            btn_CMRSearch_Click(null, null);
        }
        private void StoreCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void StoreCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            btnPrint_Click(null, null);
        }
        private void PrintCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void PrintCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            btnPrintCMR_Click(null, null);
        }

        private void cbxDeliveryPlace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    var foundObjects = baseLogic.db.CMRs.Where(c => c.DeliveryPlace == (string)e.AddedItems[0]);
                    if (foundObjects != null && foundObjects.Count() > 0)
                    {
                        var selectedItem = foundObjects.Select(c => c.DeliveryCountry);
                        if (selectedItem != null && selectedItem.Count() > 0)
                        {
                            cbxDeliveryCountry.SelectedValue = selectedItem.First();
                            return;
                        }

                    }
                }
                //Place DeliveryPlace = (Place)cbxDeliveryPlace.SelectedItem;
                //cbxDeliveryCountry.SelectedValue = DeliveryPlace.Country;
            }
            catch
            {
                cbxDeliveryCountry.SelectedIndex = 0;
            }
        }

        private void cbxTakingPlace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    var foundObjects = baseLogic.db.CMRs.Where(c => c.TakingPlace == (string)e.AddedItems[0]);
                    if (foundObjects != null && foundObjects.Count() > 0)
                    {
                        var selectedItem = foundObjects.Select(c => c.TakingCountry);
                        if (selectedItem != null && selectedItem.Count() > 0)
                        {
                            cbxTakingCountry.SelectedValue = selectedItem.First();
                            return;
                        }
                    }
                }

                //Place TakingPlace = (Place)cbxTakingPlace.SelectedItem;
                //cbxTakingCountry.SelectedValue = TakingPlace.Country;
            }
            catch
            {
                cbxTakingCountry.SelectedIndex = 0;
            }
        }

        private void btnPrintTags_Click(object sender, RoutedEventArgs e)
        {
            var CMR = mainGrid.DataContext as CMR;
            if (CMR != null && CMR.ID > 0)
            {
                ReportPrintDocument.PritnTagLabels(CMR.ID, tagLabelPrinterName);
            }
        }
        private List<CountryInfo> PrioritizeCountries(List<CountryInfo> filterList, string[] countriesToPrior)
        {
            foreach (var priorCountry in countriesToPrior)
            {
                var tempCountryIdx = filterList.IndexOf(filterList.Where(c => c.Value == priorCountry.Replace("\r", "")).Single());
                var tempCountry = filterList[tempCountryIdx];
                filterList.RemoveAt(tempCountryIdx);
                filterList.Insert(0, tempCountry);
            }
            return filterList;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CompaniesManagement cw = new CompaniesManagement(ref baseLogic);
            cw.ShowDialog();
            InitializeCompanyComboboxes();
            InitializeComingPlace();
            InitalizeDeparturePlace();
        }

        private void btnEdit1_Click(object sender, RoutedEventArgs e)
        {
            if ((Company)this.senderGrid.DataContext != null)
            {
                Company tco = (Company)this.senderGrid.DataContext;
                CompaniesManagement cm;

                if (tco.ID != 0)
                {
                    Company selCompany = baseLogic.db.Companies.Where(c => c.ID == tco.ID).First();
                    cm = new CompaniesManagement(selCompany, BeLog.Logic.ComboType.Sender, ref baseLogic);
                    cm.ShowDialog();
                    initSenderCBX();
                    //InitializeCompanyComboboxes();
                    this.senderGrid.DataContext = selCompany;
                }
                else
                {
                    cm = new CompaniesManagement(ref baseLogic);
                    cm.ShowDialog();
                    initSenderCBX();
                    //InitializeCompanyComboboxes();
                    //InitializeComingPlace();
                }

            }
        }

        private void btnEdit2_Click(object sender, RoutedEventArgs e)
        {
            if ((Company)this.consigneeGrid.DataContext != null)
            {
                Company tco = (Company)this.consigneeGrid.DataContext;
                CompaniesManagement cm;

                if (tco.ID != 0)
                {
                    Company selCompany = baseLogic.db.Companies.Where(c => c.ID == tco.ID).First();
                    cm = new CompaniesManagement(selCompany, BeLog.Logic.ComboType.Consignee, ref baseLogic);
                    cm.ShowDialog();
                    initConsigneeCBX();
                    //InitializeCompanyComboboxes();
                    this.consigneeGrid.DataContext = selCompany;
                }
                else
                {
                    cm = new CompaniesManagement(BeLog.Logic.ComboType.Consignee, ref baseLogic);
                    cm.ShowDialog();
                    initConsigneeCBX();
                    //InitializeCompanyComboboxes();
                }


            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CompaniesManagement cm;
            if ((Place)this.cbxTakingPlace.SelectedItem != null)
            {

                Place t_place = (Place)this.cbxTakingPlace.SelectedItem;
                Company tco = t_place.Company;

                if (tco.ID != 0)
                {
                    Company selCompany = baseLogic.db.Companies.Where(c => c.ID == tco.ID).First();
                    cm = new CompaniesManagement(selCompany, BeLog.Logic.ComboType.TakingPlace, ref baseLogic);
                    cm.ShowDialog();
                    InitalizeDeparturePlace();
                    this.cbxTakingPlace.SelectedItem = t_place;
                }
            }
            else
            {
                cm = new CompaniesManagement(BeLog.Logic.ComboType.TakingPlace, ref baseLogic);
                cm.ShowDialog();
                InitalizeDeparturePlace();
            }
        }

        private void senderGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            Grid sgrd = senderGrid;
            try
            {
                Company c = (Company)sgrd.DataContext;

                //System.Diagnostics.Debug.Print(c.ID.ToString());
                //System.Diagnostics.Debug.Print(c.Address.ToString());
                //System.Diagnostics.Debug.Print(c.Name.ToString());
                //System.Diagnostics.Debug.Print(c.Country.ToString());
            }
            catch (Exception)
            {

                System.Diagnostics.Debug.Print("DataContext is now Null");
            }



        }

        private void btnPrintCMR_Click(object sender, RoutedEventArgs e)
        {

            CMR cmr = mainGrid.DataContext as CMR;
            if (cmr != null && cmr.ID!=0)
            {
                ReportPrintDocument.PrintReport(cmr.ID, matrixPrinterName);
            }
            else 
            {
                MessageBox.Show("Please select CMR");
            }
        }

    }

    public class Places : ObservableCollection<Place>
    {
        private IOrderedQueryable<Company> iOrderedQueryable;

        public Places(System.Data.Linq.Table<Company> companies)
            : base()
        {
            foreach (Company co in companies)
            {
                string addr = co.Name + ", " + co.Address;
                Add(new Place(addr, co.Country, co));
            }
        }

        public Places(IOrderedQueryable<Company> iOrderedQueryable)
            : base()
        {
            // TODO: Complete member initialization
            this.iOrderedQueryable = iOrderedQueryable;

            foreach (Company co in iOrderedQueryable)
            {
                string addr = co.Name + ", " + co.Address;
                Add(new Place(addr, co.Country, co));
            }
        }
    }

    public class Place
    {
        private string _addr;
        private string _country;
        private Company _company;

        public Place(string addrArg, string countryArg, Company companyArg)
        {
            this.Address = addrArg;
            this.Country = countryArg;
            this.Company = companyArg;
        }

        public string Address
        {
            get { return _addr; }
            set { _addr = value; }
        }

        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }

        public Company Company
        {
            get { return _company; }
            set { _company = value; }
        }

    }

}
