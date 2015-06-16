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
using System.Data;
using BeLog.Logic.Containers;
using BeLog.Logic.CustomEventArgs;

namespace BeLogForm
{
    /// <summary>
    /// Interaction logic for InvoiceForm.xaml
    /// </summary>
    public partial class TransportManifest : Window
    {
        public const string cbxConsignorName = "cbConsignor";
        public const string cbxConsigneeName = "cbConsignee";
        User loggedUser = null;
        string normalPrinterName = String.Empty;
        string matrixPrinterName = String.Empty;
        BaseLogic baseLogic = new BaseLogic();
        int manifestId = -1;
        Button activeButton = null;

        public TransportManifest(User loggedInUser)
        {
            InitializeComponent();
            dataGrid1.CanUserAddRows = false;
            loggedUser = loggedInUser;
            if (ConfigurationManager.AppSettings.AllKeys.Contains("MatrixPrinterName"))
            {
                matrixPrinterName = ConfigurationManager.AppSettings["MatrixPrinterName"];
            }

            if (transportManifestGrid.DataContext == null)
            {
                Manifest transManifest = generateNewManifest();
            }
        }


        private Manifest generateNewManifest()
        {
            Manifest trMan = new Manifest();
            return trMan;
        }


        private void OrderItemsListChanged(Object sender, System.ComponentModel.ListChangedEventArgs e)
        {



        }


        private void btnNewTransManifest_Click(object sender, RoutedEventArgs e)
        {
            Manifest newManifest = new Manifest();
            newManifest.ManifestDate = DateTime.Now;
            transportManifestGrid.DataContext = newManifest;
            var manifestItems = GenerateManifestItems();
            dataGrid1.ItemsSource = manifestItems;


        }

        private void btnSaveTM_Click(object sender, RoutedEventArgs e)
        {
            if (transportManifestGrid.DataContext is Manifest)
            {
                var manifest = (Manifest)transportManifestGrid.DataContext;
                if (manifest.id == 0)
                {
                    manifest.CreatedBy = loggedUser.ID;
                    baseLogic.db.Manifests.InsertOnSubmit(manifest);
                }
                baseLogic.db.SubmitChanges();
                var manifestCMRList = (List<ManifestItem>)dataGrid1.ItemsSource;
                if (manifestCMRList.Count() > 0) manifestCMRList.RemoveAt(manifestCMRList.Count() - 1);
                foreach (var manifestCMR in manifestCMRList)
                {
                    baseLogic.db.CMRs.Where(c => c.ID == manifestCMR.CMRId).Single().ManifestId = manifest.id;
                    baseLogic.db.SubmitChanges();
                    //   ReportPrintDocument.PrintReport(manifestCMR.CMRId, matrixPrinterName);
                }
                // ReportPrintDocument.PritnManifest(manifest.id, "Microsoft XPS Document Writer");
            }
        }

        private void btnEditTM_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_TransManifestSearch_Click(object sender, RoutedEventArgs e)
        {
            TransportManifestSearch manifestSearch = new TransportManifestSearch();
            manifestSearch.ShowDialog();
            if (manifestSearch.SelectedManifestID != 0)
            {
                transportManifestGrid.DataContext = baseLogic.db.Manifests.Where(c => c.id == manifestSearch.SelectedManifestID).First();
                dataGrid1.ItemsSource = null;
                var preInitItems = GenerateManifestItems(
                     baseLogic.db.CMRs.Where(c => c.ManifestId == manifestSearch.SelectedManifestID).ToList());
                preInitItems.Add(GenerateManifestItem(null, preInitItems.Count() + 1));
                dataGrid1.ItemsSource = preInitItems;
            }
        }


        private void fileExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private List<ManifestItem> GenerateManifestItems(List<CMR> cmrList = null)
        {
            var manifestList = new List<ManifestItem>();
            if (cmrList != null)
            {
                int posNr = 1;
                cmrList.ForEach(c =>
                    {
                        manifestList.Add(GenerateManifestItem(c, posNr));
                        posNr++;
                    });
            }
            else
            {
                manifestList.Add(GenerateManifestItem(null, 1));
            }
            return manifestList;
        }
        private ManifestItem GenerateManifestItem(CMR cmr, int posNr)
        {
            if (cmr == null) return new ManifestItem()
            {
                PositionId = posNr,
                Consignor =null,// baseLogic.db.Companies.Where(c => c.CompanyType == (int)ComboType.Sender).FirstOrDefault(),
                Consignee =null//  baseLogic.db.Companies.Where(c => c.CompanyType == (int)ComboType.Consignee).FirstOrDefault()
            };
            return new ManifestItem()
                            {
                                PositionId = posNr,
                                //Consignor = GetCompanyInfoById(cmr.SenderID),
                                //Consignee = GetCompanyInfoById(cmr.ConsigneeID),
                                Consignor = baseLogic.db.Companies.Where(c => c.ID == cmr.SenderID).Single(),
                                Consignee = baseLogic.db.Companies.Where(c => c.ID == cmr.ConsigneeID).Single(),
                                Bruttoweight = GetBruttoWeightByCMRId(cmr.ID),
                                PackagingMode = GetPackagingModeByCMRId(cmr.ID),
                                PlaceNum = GetPlacesNumberByCMRId(cmr.ID),
                                CMRId = cmr.ID
                            };
        }

        private IQueryable<Company> GetConsignors()
        {
            var companyList = baseLogic.db.Companies.Where(c => c.CompanyType == (int)ComboType.Sender);
            //if (companyList.Count() == 0) return null;
            return companyList;
        }

        private IQueryable<Company> GetConsignees()
        {
            var companyList = baseLogic.db.Companies.Where(c => c.CompanyType == (int)ComboType.Consignee);
            //if (companyList.Count() == 0) return null;
            return companyList;//.ToList();
        }

        private string GetCompanyInfoById(int? companyId)
        {
            var companyList = baseLogic.db.Companies.Where(c => c.ID == companyId);
            if (companyList.Count() == 0) return String.Empty;
            return companyList.Select(c => c.Name).Single();
        }

        // TODO Check Amount Value
        private string GetPackagingModeByCMRId(int cmrId)
        {

            string sb = string.Empty;
            var orderItemList = baseLogic.db.OrderItems.Where(c => c.CMRID == new Nullable<int>(cmrId));
            if (orderItemList.Count() != 0)
            {
                foreach (OrderItem oa in orderItemList)
                {
                    sb += oa.PackagingMethod + " ";
                }
            }
            return sb;
        }

        private decimal GetBruttoWeightByCMRId(int cmrId)
        {
            decimal brtWeight = 0;
            var orderItemList = baseLogic.db.OrderItems.Where(c => c.CMRID == new Nullable<int>(cmrId));
            if (orderItemList.Count() != 0)
            {
                brtWeight = orderItemList.Sum(c => c.Weight);
            }
            return brtWeight;
        }

        private string GetAmountInfoByCMRId(int cmrId)
        {
            string strAmount = String.Empty;
            var orderItemList = baseLogic.db.OrderItems.Where(c => c.CMRID == new Nullable<int>(cmrId));
            if (orderItemList.Count() != 0)
            {
                int totalUnits = orderItemList.Sum(c => c.NumOfPackages);
                decimal totalWeight = orderItemList.Sum(c => c.Weight);
                strAmount = totalUnits + " pll " + totalWeight.ToString("#.##") + "kg";
            }
            return strAmount;
        }

        private int GetPlacesNumberByCMRId(int cmrId)
        {
            int kArv = 0;
            var orderItemList = baseLogic.db.OrderItems.Where(c => c.CMRID == new Nullable<int>(cmrId));
            if (orderItemList.Count() != 0)
            {
                kArv = orderItemList.Sum(c => c.NumOfPackages);
            }
            return kArv;
        }

        private void dataGrid1_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "PositionId")
            {
                e.Column.Header = "Pos. nr";
            }
            e.Column.IsReadOnly = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            var manifestItem = (ManifestItem)dataGrid1.SelectedItem;
            CMRForm cmrForm = new CMRForm(loggedUser, manifestItem.Consignor, manifestItem.Consignee);
            cmrForm.CMRAdded += new AddedCMREventHandler(cmrForm_CMRAdded);
            cmrForm.Show();


        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var manifestItem = (ManifestItem)dataGrid1.SelectedItem;
            CMRForm cmrForm = new CMRForm(loggedUser, manifestItem.Consignor, manifestItem.Consignee);
            cmrForm.CMREdited += new EditedCMREventHandler(cmrForm_CMREdited);
            cmrForm.Show();
        }

        void cmrForm_CMRAdded(object sender, CMRAddedEventArgs e)
        {
            this.Activate();
            if (e.CMRId != 0)
            {
                CMR cmr = baseLogic.db.CMRs.Where(c => c.ID == e.CMRId).Single();
                var currentManifestItems = (List<ManifestItem>)dataGrid1.ItemsSource;

                if (currentManifestItems.Count() > 0)
                {
                    currentManifestItems.RemoveAt(currentManifestItems.Count() - 1);
                }
                currentManifestItems.Add(GenerateManifestItem(cmr, currentManifestItems.Count() + 1));
                currentManifestItems.Add(GenerateManifestItem(null, currentManifestItems.Count() + 1));
                dataGrid1.ItemsSource = null;
                dataGrid1.ItemsSource = currentManifestItems;
            }
        }
        void cmrForm_CMREdited(object sender, CMRAddedEventArgs e)
        {
            //this.Activate();
            //if (e.CMRId != 0)
            //{
            //    CMR cmr = baseLogic.db.CMRs.Where(c => c.ID == e.CMRId).Single();
            //    var currentManifestItems = (List<ManifestItem>)dataGrid1.ItemsSource;

            //    if (currentManifestItems.Count() > 0)
            //    {
            //        currentManifestItems.RemoveAt(currentManifestItems.Count() - 1);
            //    }
            //    currentManifestItems.Add(GenerateManifestItem(cmr, currentManifestItems.Count() + 1));
            //    currentManifestItems.Add(GenerateManifestItem(null, currentManifestItems.Count() + 1));
            //    dataGrid1.ItemsSource = null;
            //    dataGrid1.ItemsSource = currentManifestItems;
            //}
        }

        private void cbxInfo_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            if (cbx != null)
            {
                if (cbx.Name == cbxConsignorName)
                {
                    cbx.ItemsSource = GetConsignors();
                }
                else if (cbx.Name == cbxConsigneeName)
                {
                    cbx.ItemsSource = GetConsignees();
                }
                cbx.DisplayMemberPath = "Name";
            }
        }

        private void dataGrid1_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        private void BtnPrintManifest_Click(object sender, RoutedEventArgs e)
        {
            PrintManifest(PrintRange.Manifest);
        }

        private void btnPrintAll_Click(object sender, RoutedEventArgs e)
        {
            PrintManifest(PrintRange.All);
        }


        private void PrintManifest(PrintRange printRange)
        {

            var manifest = transportManifestGrid.DataContext as Manifest;
            if (manifest != null && manifest.id != 0)
            {
                if (printRange == PrintRange.CMR || printRange == PrintRange.All)
                {
                    var manifestCMRList = (List<ManifestItem>)dataGrid1.ItemsSource;
                    if (manifestCMRList.Count() > 0) manifestCMRList.RemoveAt(manifestCMRList.Count() - 1);
                    foreach (var manifestCMR in manifestCMRList)
                    {
                        ReportPrintDocument.PrintReport(manifestCMR.CMRId, matrixPrinterName);
                    }
                }
                if (printRange == PrintRange.Manifest || printRange == PrintRange.All)
                {
                    ReportPrintDocument.PritnManifest(manifest.id, "Microsoft XPS Document Writer");
                }
            }
            else
            {
                MessageBox.Show("Please select manifest to print");
            }
        }
        [Flags]
        public enum PrintRange
        {
            Manifest,
            CMR,
            All
        }

        private void cbConsignor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
           if(sender is ComboBox &&  ((ComboBox)sender).Name=="cbConsignee")
           {
                var currentManifestItems = dataGrid1.ItemsSource as List<ManifestItem>;
                if(currentManifestItems!= null && currentManifestItems.Count!=0)
                {
                   var lastItem =  currentManifestItems.Last();
                   lastItem.Consignee = (Company)((ComboBox)e.Source).SelectedItem;
                   currentManifestItems.RemoveAt(currentManifestItems.Count()-1);
                    currentManifestItems.Add(lastItem);
                }
              
           }
           if(sender is ComboBox &&  ((ComboBox)sender).Name=="cbConsignor")
           {
                var currentManifestItems = dataGrid1.ItemsSource as List<ManifestItem>;
                if(currentManifestItems!= null && currentManifestItems.Count!=0)
                {
                   var lastItem =  currentManifestItems.Last();
                   lastItem.Consignor = (Company)((ComboBox)e.Source).SelectedItem;
                   currentManifestItems.RemoveAt(currentManifestItems.Count()-1);
                    currentManifestItems.Add(lastItem);
                }
              
           }
        
        }


    }
}
