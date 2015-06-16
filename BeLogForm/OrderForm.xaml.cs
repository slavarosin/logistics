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
using System.Windows.Shapes;
using BeLog.Logic;
using DataAccessLayer;
using BeLogPrinting;
using System.Configuration;

using BeLog.Logic.Containers;

namespace BeLogForm
{
    /// <summary>
    /// Interaction logic for InvoiceForm.xaml
    /// </summary>
    public partial class OrderForm : Window
    {

        User loggedUser = null;
        string normalPrinterName = String.Empty;
        BaseLogic baseLogic = new BaseLogic();
        public OrderForm(User loggedInUser)
        {
            loggedUser = loggedInUser;
            InitializeComponent();
            InitializeCompanyComboboxes();
            InitializeDatePickers();
          
           
            //txtFromAbr.Text = loggedUser.Abreviation;
            //txtFName.Text = loggedUser.FirstName;
            //txtLName.Text = loggedUser.LastName;
            //txtPhone.Text = loggedUser.PhoneNumber;
            
            tbxUserNotes.Text = @"Hilinemise puhul, maha/pealelaadimisele hilinemiskulud katab ülevedaja summas 1000 eek/päevas Laadimine ja tolli kestus 48t. Mahalaadimine ja tolli kestus 24t. Autojuht  peab kontrollima  kauba kinnitamine, saatedokumendid, kaubanduslikku välimust; kauba kogust peale-ja mahalaadimisel.  Vedaja  kannab vastutust kauba eest  kogu transpordimise ajal.Veotellimust takistavatest asjaoludest teatada viivitamatult  tel:  +372-5133532.Märkused: Veohind sisaldab kõike veoga seotud kaasnevaid kulutusi.Leping jõustab tellija poolt allakirjutamisel. Lepingus loodud tingimuste rikkumise korral vahendame kokkulepehinda meie laekuvate pretensioone ulatuses.Tellimuse tühistamisest tuleb ette teatada teisele poolele 48 tundi enne kokkulepitud laadimise toimumist Käesoleva lepingu täitmisel pooled juhinduvad  Rahvusvahelisest Autokauba Veolepingu Konventsioonist ja EEA üldtingimustes.
                                  
Lugupidamisega,
Maksim Meleshko";
            if (ConfigurationManager.AppSettings.AllKeys.Contains("NormalPrinterName"))
            {
                normalPrinterName = ConfigurationManager.AppSettings["NormalPrinterName"];
            }
            var tempOrder =   createNewOrder();
            grdMainInvoice.DataContext =  tempOrder;

            OrderDispatch dispatch = new OrderDispatch();
            OrderDispatchItem dispathcItem = new OrderDispatchItem();

            var relatedDispatchItems = GetOrderDispatchItems();
            dataGridOrderDispatch.ItemsSource = new List<OrderDispatch>();
            dataGridOrderDispatch.CanUserAddRows = true;

            dataGridLoading.ItemsSource = new List<OrderDispatch>();
            dataGridLoading.CanUserAddRows = true;

           // tbxOrderNr.Text = tempOrder.OrderNr.ToString();
        }
        public void InitializeDatePickers()
        {
            datePicker1.Language = System.Windows.Markup.XmlLanguage.GetLanguage("et-EE");
            datePicker1.DisplayDate = DateTime.Now;
           
        }


        private List<OrderDispatchItem> GenerateOrderDispatchItems(List<OrderDispatch> orderDispatchList = null)
        {
            var dispatchObjectsList = new List<OrderDispatchItem>();
            if (orderDispatchList != null)
            {
                orderDispatchList.ForEach(c =>
                {
                    dispatchObjectsList.Add(GenerateOrderDispatchItem(c));
                });
            }
            return dispatchObjectsList;
        }

        private OrderDispatchItem GenerateOrderDispatchItem(OrderDispatch oneOrderDispatchItem)
        {
            return new OrderDispatchItem()
            {
                OrderId = oneOrderDispatchItem.Order_id,
                DispatchTime = oneOrderDispatchItem.DispatchTime,
                DispatchPlace = oneOrderDispatchItem.DispatchPlace,
                DispatchContact = oneOrderDispatchItem.DispatchContact
            };
        }

        private List<OrderDispatch> GetOrderDispatchItems(int? orderID = null, string dispatchType = null)
        {
            List<OrderDispatch> dispatchList = new List<OrderDispatch>();
            var orderRelatedItems =baseLogic.db.OrderDispatches.Where(c => c.Order_id == orderID);
            OrderDispatch ddispatch = new OrderDispatch();
            var foundItems = orderRelatedItems.Where(c => c.DispatchType == dispatchType);
            if(foundItems.Count()>0)
           {
               dispatchList = foundItems.ToList();
           }
            return dispatchList;

        }

        public void InitializeCompanyComboboxes()
        {
            InitializeComboBoxItems(cbxCompanyTo, ComboType.OrderReceiver);
        }
        private void InitializeComboBoxItems(ComboBox cbx, ComboType cbxType)
        {
            cbx.ItemsSource = baseLogic.getComboList(cbxType);
            cbx.SelectedValuePath = "ID";
            cbx.DisplayMemberPath = "Name";
        }

        private void cbxCompanyTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedValue is int)
            {
                baseLogic.db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, baseLogic.db.Companies);
                var foundResult = baseLogic.db.Companies.Where(c => c.ID == (int)((ComboBox)sender).SelectedValue);
                if (foundResult != null && foundResult.Count() == 1)
                {
                    grdCompanyTo.DataContext = foundResult.First();
                }
            }
            else
            {
                baseLogic.db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, baseLogic.db.Companies);
                grdCompanyTo.DataContext = GenerateNewCompany(ComboType.OrderReceiver);
            }

        }

        private void cbxCompanyFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedValue is int)
            {

                var foundResult = baseLogic.db.Companies.Where(c => c.ID == (int)((ComboBox)sender).SelectedValue);
                if (foundResult != null && foundResult.Count() == 1)
                {
                    grdCompanyFrom.DataContext = foundResult.First();
                }
            }
            else
            {
                baseLogic.db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, baseLogic.db.Companies);
                grdCompanyFrom.DataContext = GenerateNewCompany(ComboType.OrderSender);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            grdMainInvoice.DataContext = createNewOrder();
           
        }
        private Invoice createNewOrder()
        {
            Invoice inv = new Invoice();
            inv.GeneratedBy = loggedUser;
            inv.UserCreated = loggedUser.Abreviation;
            inv.InvDate = DateTime.Now;
            if(baseLogic.db.Invoices.Count()>0)
            {
               inv.OrderNr = baseLogic.db.Invoices.Max(c=>c.OrderNr)+1; 
            }
            else
            {
                inv.OrderNr = 1000000;
            }
                inv.ContactPhoneNumber = loggedUser.PhoneNumber;
            return inv;
        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            var tempUser = (User)loggedUser;
            {
                if (loggedUser.PhoneNumber != null)
                {
                    //var updatable = baseLogic.db.Users.Where(c =>c.ID == loggedUser.ID).First();
                    //updatable.PhoneNumber = loggedUser.PhoneNumber;
                   // baseLogic.db.SubmitChanges();
                }
            }

            var tempInv = (Invoice)grdMainInvoice.DataContext;
            var tempDispatch = dataGridOrderDispatch.DataContext;

            if (cbxCompanyTo.SelectedValue == null)
            {
                checkCompanyExistenceOrCreate(cbxCompanyTo, grdCompanyTo);
            }
            if (!(grdCompanyTo.DataContext is Company)) return;
            tempInv.CompanyTo = ((Company)grdCompanyTo.DataContext);
            //todo: refactroing needed
            tempInv.DispatchTime = "dummyTime";
            tempInv.DispatchContact = "dummyContact";
            tempInv.DispatchPlace = "dummyPlace";
            tempInv.LoadingTime = "dummyLoadingTime";
            tempInv.LoadingPlace = "dummyLoadingPlace";


           



            if (baseLogic.db.Invoices.Where(c => c.ID == tempInv.ID).Count() == 0)
            {

                //baseLogic.db.OrderDispatches.InsertOnSubmit(tempDispatch);
                baseLogic.db.Invoices.InsertOnSubmit(tempInv);
            }

            tempInv.UserNotes = tbxUserNotes.Text;
            try
            {
                baseLogic.db.SubmitChanges();
                List<OrderDispatch> dispItems = dataGridOrderDispatch.ItemsSource as List<OrderDispatch>;
                List<OrderDispatch> loadingItems = dataGridLoading.ItemsSource as List<OrderDispatch>;

                if (dispItems != null && loadingItems != null)
                {
                    foreach (var dispItem in dispItems)
                    {
                        if (!baseLogic.db.OrderDispatches.Contains(dispItem))
                        {
                            baseLogic.db.OrderDispatches.InsertOnSubmit(new OrderDispatch()
                            {
                                Order_id = tempInv.ID,
                                DispatchPlace = dispItem.DispatchPlace,
                                DispatchTime = dispItem.DispatchTime,
                                DispatchContact = dispItem.DispatchContact,
                                DispatchType = "M"
                            });
                        }
                    }

                    foreach (var loadingItem in loadingItems)
                    {
                        if (!baseLogic.db.OrderDispatches.Contains(loadingItem))
                        {
                            baseLogic.db.OrderDispatches.InsertOnSubmit(new OrderDispatch()
                            {
                                Order_id = tempInv.ID,
                                DispatchPlace = loadingItem.DispatchPlace,
                                DispatchTime = loadingItem.DispatchTime,
                                DispatchContact = loadingItem.DispatchContact,
                                DispatchType = "P"
                            });
                        }
                    }

                }
                baseLogic.db.SubmitChanges();
                
                System.Windows.Controls.PrintDialog dlg = new System.Windows.Controls.PrintDialog();
                dlg.PageRangeSelection = PageRangeSelection.AllPages;
                dlg.UserPageRangeEnabled = true;
                
                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    var selPrinterName = normalPrinterName;
                    if(result.HasValue)
                    {
                        selPrinterName = dlg.PrintQueue.FullName;
                    }
                    ReportPrintDocument.PrintInvoice(tempInv.ID, selPrinterName);
                }
                InitializeCompanyComboboxes();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Order Form Error", ex.Message + ex.StackTrace);
                System.Diagnostics.Debug.WriteLine("Order Form", String.Format("Error Message {0} \n  Stack Trace: {1}", ex.Message, ex.StackTrace));
                MessageBox.Show("Palun kontrollige kas kõik info on sissestatud");
            }
        }

        private void btnOpenInvoice_Click(object sender, RoutedEventArgs e)
        {
            OrderSearch invoiceSearch = new OrderSearch();
            invoiceSearch.ShowDialog();
            if (invoiceSearch.SelectedInvoiceID != 0)
            {
                grdMainInvoice.DataContext = baseLogic.db.Invoices.Where(c => c.ID == invoiceSearch.SelectedInvoiceID).First();
                dataGridOrderDispatch.ItemsSource = GetOrderDispatchItems(invoiceSearch.SelectedInvoiceID, "M");
                dataGridLoading.ItemsSource = GetOrderDispatchItems(invoiceSearch.SelectedInvoiceID, "P");
            }
        }
        private Company GenerateNewCompany(ComboType cpType)
        {
            Company cpn = new Company();
            cpn.Address = String.Empty;
            cpn.ContactPerson = String.Empty;
            cpn.Country = String.Empty;
            cpn.Fax = String.Empty;
            cpn.CompanyType = (int)cpType;
            cpn.Phone = String.Empty;
            return null;
        }
        private void checkCompanyExistenceOrCreate(ComboBox cbxItem, Grid dGrid)
        {
            var foundResult = baseLogic.db.Companies.Where(c => c.Name == cbxItem.Text);
            if (foundResult != null && foundResult.Count() > 0)
            {
                dGrid.DataContext = foundResult.First();
            }
            else
            {
                baseLogic.db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, baseLogic.db.Companies);
                if (dGrid.Name.Contains("To"))
                {
                    dGrid.DataContext = GenerateCompanyToInfo();
                }
            }
        }
        private Company GenerateCompanyToInfo()
        {
            Company cpn = new Company();
            cpn.Address = String.Empty;
            cpn.Name = cbxCompanyTo.Text;
           // cpn.ContactPerson = tbxCPerson.Text;
           // cpn.Country = String.Empty;
           // cpn.Fax = tbxFaxPhone.Text;
           // cpn.CompanyType = (int)ComboType.OrderReceiver;
           // cpn.Phone = tbxFaxPhone.Text;
            return cpn;
        }

        private Thickness SetNewMargin(Control realingnedControl)
        {
            Thickness currThikness = realingnedControl.Margin;
            Thickness newThickness = new Thickness(currThikness.Left, currThikness.Top + realingnedControl.Height + 10,
                currThikness.Right, currThikness.Bottom);
            return currThikness;
        }

        private void initorderReceiverGrid()
        {
            this.grdCompanyTo.DataContext = new Company();
        }
        
        private void orderConsegnee_Click(object sender, RoutedEventArgs e)
        {
            var tempCompany  = this.grdCompanyTo.DataContext as Company;
            if (tempCompany != null && tempCompany.ID != 0 )
            {
                Company tco = (Company)this.grdCompanyTo.DataContext;
                CompaniesManagement cm;
                if (tco.ID != 0)
                {
                    Company selCompany = baseLogic.db.Companies.Where(c => c.ID == tco.ID).First();
                    cm = new CompaniesManagement(selCompany, BeLog.Logic.ComboType.OrderReceiver, ref baseLogic);
                    cm.ShowDialog();

                    InitializeCompanyComboboxes();
                    //InitializeCompanyComboboxes();
                    this.grdCompanyTo.DataContext = selCompany;
                }
            }
            else
            {
               
                CompaniesManagement cm;
                cm = new CompaniesManagement(BeLog.Logic.ComboType.OrderReceiver, ref baseLogic);
                cm.ShowDialog();
                initorderReceiverGrid();
                InitializeCompanyComboboxes();
                //InitializeCompanyComboboxes();
            }
       
        }
    
    }
}
