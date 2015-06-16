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


namespace BeLogForm
{
    public class FormInfoGatherer
    {
        private CMR cmr = new CMR();
        private BaseLogic bl = new BaseLogic();
        Grid mainGrid;
        private List<OrderItem> orderItemList = new List<OrderItem>();
        public FormInfoGatherer(Grid mainGrid)
        {
            this.mainGrid = mainGrid;
        }
        public int ValidateAndStore()
        {
            if (ValidateForm(mainGrid))
            {
                return StoreInfo(mainGrid);
            }
            return int.MinValue;
        }
        public static bool ValidateForm(Grid mainGrid)
        {
            bool isValid = true;
            foreach (string comboType in Enum.GetNames(typeof(ComboType)))
            {
                ComboBox cbxName = (ComboBox)mainGrid.FindName("cbx" + comboType + "Name");
                ComboBox cbxAdress = (ComboBox)mainGrid.FindName("cbx" + comboType + "Adress");
                ComboBox cbxCountry = (ComboBox)mainGrid.FindName("cbx" + comboType + "Country");
                isValid = ValidateControl(cbxName);
                isValid &= ValidateControl(cbxAdress);
                isValid &= ValidateControl(cbxCountry);
            }
            TextBox tbxTimeOfDepDate = (TextBox)mainGrid.FindName("tbxTimeOfDepDate");
            TextBox tbxNum = (TextBox)mainGrid.FindName("tbxNum");
            TextBox tbxEstDate = (TextBox)mainGrid.FindName("tbxEstDate");
            isValid &= ValidateControl(tbxTimeOfDepDate, "Date");
            isValid &= ValidateControl(tbxNum, "Int");
            isValid &= ValidateControl(tbxEstDate, "Date");
            return isValid;
        }
        public static bool ValidateDataGrid(DataGrid dataGrid1, ref StringBuilder strBldr)
        {
            bool isValid = true;
            if (dataGrid1.Items.Count == 0)
            {
                strBldr.AppendLine("Palun täitke info ühest laadimis ainest");
               isValid = false;
            }
            else if (dataGrid1.Items.Count == 1)
            {
                foreach (object item in dataGrid1.Items)
                {
                    OrderItem orderItem = item as OrderItem;
                    if (orderItem == null)
                    {
                        strBldr.AppendLine("Palun täitke info ühest laadimis ainest");
                        isValid = false;
                    }

                }
            }
            if (!isValid) dataGrid1.Style = (Style)dataGrid1.FindResource("ControlErrorStyle");
            else dataGrid1.Style = (Style)dataGrid1.FindResource("ControlNormalStyle");
            return isValid;
        }

        public static bool ValidateControl(Control ctrl, string tbxType = "Text")
        {
            bool isValid = true;
            if (ctrl.GetType() == typeof(ComboBox))
            {
                ComboBox cbx = (ComboBox)ctrl;

                if (cbx.SelectedValue == null && String.IsNullOrEmpty(cbx.Text))
                {
                    isValid = false;
                }
                else
                {
                    isValid = true;
                }
            }
            else if (ctrl.GetType() == typeof(TextBox))
            {
                TextBox tbx = (TextBox)ctrl;
                if (tbxType == "Text")
                {
                    if (tbx.Text == String.Empty)
                    {
                        isValid = false;
                    }
                    else
                    {
                        isValid = true;
                    }
                }
                else if (tbxType == "Date")
                {
                    DateTime dt = new DateTime();
                    isValid = DateTime.TryParse(tbx.Text, out dt);
                }
                else if (tbxType == "Int")
                {
                    int integer = int.MinValue;
                    isValid = int.TryParse(tbx.Text, out integer);
                }

            }

            if (!isValid) ctrl.Style = (Style)ctrl.FindResource("ControlErrorStyle");
            else ctrl.Style = (Style)ctrl.FindResource("ControlNormalStyle");

            return isValid;
        }

        public static bool ValidateSenderInstructions(TextBox tbx, string tbxType = "Text")
        {

            if (tbx.LineCount > 8)
            {
                tbx.Style = (Style)tbx.FindResource("ControlErrorStyle");
                return false;
            }
            
            for (int i = 0; i < tbx.LineCount; i++)
            {
                if (tbx.GetLineLength(i) > 50)
                {
                    tbx.Style = (Style)tbx.FindResource("ControlErrorStyle");
                    return false;
                }
            }
            
            tbx.Style = (Style)tbx.FindResource("ControlNormalStyle");
            return true;
        }

        private void GatherCompaniesInfo(Grid mainGrid)
        {


            foreach (string comboType in Enum.GetNames(typeof(ComboType)))
            {
                Company cpn = new Company();
                int companyID = int.MinValue;
                ComboBox cbxName = (ComboBox)mainGrid.FindName("cbx" + comboType + "Name");
                if (cbxName.SelectedValue != null)
                {
                    companyID = (int)cbxName.SelectedValue;
                }
                else
                {

                    ComboBox cbxAdress = (ComboBox)mainGrid.FindName("cbx" + comboType + "Adress");
                    ComboBox cbxCountry = (ComboBox)mainGrid.FindName("cbx" + comboType + "Country");
                    cpn.CompanyType = (int)Enum.Parse(typeof(ComboType), comboType);
                    cpn.ID = bl.getLastCompanyID() + 1;
                    cpn.Name = cbxName.Text;
                    cpn.Address = cbxAdress.Text;
                    cpn.Country = cbxCountry.Text;
                    companyID = bl.alreadyContainsCompany(cpn);
                    if (companyID == int.MinValue)
                    {
                        bl.insertNewCompany(cpn);
                        companyID = cpn.ID;
                    }

                }
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
                    cmr.CarrierID = companyID;
                }
                else if (comboType == "SuccCarrier")
                {
                    cmr.NextCarrierID = companyID;
                }

            }


        }
        public List<OrderItem> gatherOrderItems(Grid mainGrid, int CMRID)
        {
            DataGrid grid = (DataGrid)mainGrid.FindName("dataGrid1");
            OrderItem item = null;
            List<OrderItem> items = new List<OrderItem>();
            try
            {
                foreach (OrderItem orderItem in grid.Items)
                {

                    item = orderItem;
                    item.CMRID = CMRID;
                    item.ID = bl.GetNextOrderItemID();
                    items.Add(item);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("BeLog", ex.ToString());
            }
            return items;
        }
        public int StoreInfo(Grid mainGrid)
        {
            GatherCompaniesInfo(mainGrid);
            ComboBox delPlace = (ComboBox)mainGrid.FindName("cbxDeliveryPlace");
            ComboBox delCountry = (ComboBox)mainGrid.FindName("cbxDeliveryCountry");
            ComboBox takingPlace = (ComboBox)mainGrid.FindName("cbxTakingPlace");
            ComboBox takingCountry = (ComboBox)mainGrid.FindName("cbxTakingCountry");

            TextBox tbxAttachedDocuments = (TextBox)mainGrid.FindName("tbxAttachedDocuments");
            TextBox tbxClass = (TextBox)mainGrid.FindName("tbxClass");
            TextBox tbxNum = (TextBox)mainGrid.FindName("tbxNum");
            TextBox tbxLetter = (TextBox)mainGrid.FindName("tbxLetter");
            TextBox tbxADR = (TextBox)mainGrid.FindName("tbxADR");
            CheckBox chxCMRAgreemntLegal = (CheckBox)mainGrid.FindName("chxCMRAgreemntLegal");
            CheckBox rbnFranko = (CheckBox)mainGrid.FindName("chxFranko");
            CheckBox rbnNoFranko = (CheckBox)mainGrid.FindName("chxNonFranko");
            TextBox tbxEstIn = (TextBox)mainGrid.FindName("tbxEstIn");
            TextBox tbxSendInstructions = (TextBox)mainGrid.FindName("tbxSendInstructions");
            TextBox tbxCMRId = (TextBox)mainGrid.FindName("tbxCMRId");

            TextBox tbxSpecAgr = (TextBox)mainGrid.FindName("tbxSpecAgr");
            TextBox tbxTimeOfDepDate = (TextBox)mainGrid.FindName("tbxTimeOfDepDate");




            TextBox tbxEstDate = (TextBox)mainGrid.FindName("tbxEstDate");

            cmr.DeliveryPlace = delPlace.Text;
            cmr.DeliveryCountry = delCountry.Text;
            cmr.TakingPlace = takingPlace.Text;
            cmr.TakingCountry = takingCountry.Text;

            cmr.AttachedDocuments = tbxAttachedDocuments.Text;
            cmr.ClassValue = tbxClass.Text;
            cmr.Num = int.Parse(tbxNum.Text);
            cmr.Letter = tbxLetter.Text;
            cmr.ADR = tbxADR.Text;
            cmr.CMRValid = chxCMRAgreemntLegal.IsChecked.Value;
            string paymentInstr = String.Empty;
            if (rbnFranko.IsChecked.Value) paymentInstr = (string)rbnFranko.Content;
            else if (rbnNoFranko.IsChecked.Value) paymentInstr = (string)rbnNoFranko.Content;
            cmr.PaymentInstruction = paymentInstr;
            cmr.EstablieshedIn = tbxEstIn.Text;
            cmr.EstablieshedDate = DateTime.Parse(tbxEstDate.Text);
            cmr.TimeOfDep = DateTime.Parse(tbxTimeOfDepDate.Text);

            cmr.SendInstruction = tbxSendInstructions.Text;
            cmr.SpecialAgreements = tbxSpecAgr.Text;
            cmr.SenderPaymentID = gatherPaymentInfo(mainGrid, PaymentType.Send);
            cmr.ConsigneePaymentID = gatherPaymentInfo(mainGrid, PaymentType.Cons);
            cmr.TransportID = gatherTruckInfo(mainGrid);
            cmr.ID = bl.GetNextCMRID();
            bl.InsertNewCMR(cmr, gatherOrderItems(mainGrid, cmr.ID));
            return cmr.ID;

        }
        public int gatherTruckInfo(Grid mainGrid)
        {

            TextBox tbxRegNum = (TextBox)mainGrid.FindName("tbxRegNum");
            TextBox tbxTruck = (TextBox)mainGrid.FindName("tbxTruck");
            TextBox tbxMark = (TextBox)mainGrid.FindName("tbxMark");

            Transport transport = new Transport();
            transport.ID = bl.GetTruckId();

            transport.Model = tbxMark.Text;
            transport.RegisterNumber = tbxRegNum.Text;
            bl.insertTransportInfo(transport);
            return transport.ID;
        }

        public int gatherPaymentInfo(Grid mainGrid, PaymentType type)
        {
            PaymentInfo pmntInfo = new PaymentInfo();
            string paymentType = Enum.GetName(typeof(PaymentType), type);
            TextBox tbxCarrChrge = (TextBox)mainGrid.FindName("tbx" + paymentType + "CarrChrge");
            TextBox tbxDeductions = (TextBox)mainGrid.FindName("tbx" + paymentType + "Deductions");
            TextBox tbxBalance = (TextBox)mainGrid.FindName("tbx" + paymentType + "Balance");
            TextBox tbxSupplements = (TextBox)mainGrid.FindName("tbx" + paymentType + "Supplements");
            TextBox tbxOthChrge = (TextBox)mainGrid.FindName("tbx" + paymentType + "OthChrge");
            pmntInfo.CompanyID = 0;
            pmntInfo.Currency = ((string)((ComboBox)mainGrid.FindName("cbxCurrOthChrge")).SelectedValue).Replace("\t", ""); ;
            pmntInfo.Deductions = Decimal.Parse(tbxDeductions.Text);
            pmntInfo.CarriageCharges = Decimal.Parse(tbxCarrChrge.Text);
            pmntInfo.Saldo = Decimal.Parse(tbxBalance.Text);
            pmntInfo.Supplements = Decimal.Parse(tbxSupplements.Text);
            pmntInfo.OtherCharges = Decimal.Parse(tbxOthChrge.Text);
            return bl.insertPaymentInfo(pmntInfo);
        }


    }

    public enum PaymentType
    {
        Cons,
        Send
    }
}
