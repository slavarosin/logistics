using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAccessLayer;
using System.Configuration;
using System.Data;

namespace BeLog.Logic
{
    public enum Truck
    {
        First,
        Second
    }
    public enum ComboType
    {
        Sender,
        Consignee,
        Carrier,
        SuccCarrier,
        OrderSender,
        OrderReceiver,
        TakingPlace
    }
    public enum ApplicationAccess
    {
        Admin,
        CMRForm,
        OrderForm
    }
  
    public class BaseLogic
    {
        public BeLogDB db {get;set;}
        public BaseLogic()
        {
            //db = new BeLogDB(ConfigurationManager.ConnectionStrings["BeLogForm.Properties.Settings.BeLogConnectionString"].ConnectionString);
            // What for the additional connectionString is provided?
            db = new BeLogDB();
        }
        public IQueryable<Company> GetCompanyByID(int id)
        {
           return db.Companies.Where(c => c.ID == id);
        }
        public List<Company> getComboList(ComboType comboType)
        {
            return db.Companies.Where(c => c.CompanyType == (int)comboType).OrderBy(c => c.Name).ToList().Distinct(((IEqualityComparer<Company>)new CompanyEqualityComparer())).ToList();
        }
        public bool submitChanges()
        {
            db.SubmitChanges();
            return true;
        }
        public bool InsertNewCMR(CMR cmr, List<OrderItem> orderItem)
        {
            db.Refresh(System.Data.Linq.RefreshMode.KeepCurrentValues, db.CMRs);
            db.Refresh(System.Data.Linq.RefreshMode.KeepCurrentValues, db.OrderItems);
            if(!db.CMRs.Contains(cmr))
            {
                db.CMRs.InsertOnSubmit(cmr);
            
                db.OrderItems.InsertAllOnSubmit(orderItem);
                    
            }
            db.SubmitChanges();
            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.CMRs);
            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.OrderItems);
            return true;
        }
        public void insertNewCompany(Company company)
        {
            db.Companies.InsertOnSubmit(company);
            db.SubmitChanges();
        }
        public int getLastCompanyID()
        {
            if (db.Companies.Count() != 0)
            {
                return db.Companies.Max(c => c.ID);
            }
            return 1;
        }
        public int alreadyContainsCompany(Company company)
        {
            var res = db.Companies.Where(c => c.Name == company.Name && c.CompanyType == company.CompanyType);
            if(res.Count()>0)
            {
               return res.Select(c=>c.ID).Single();
            }
            return int.MinValue;
        }
        public int insertPaymentInfo(PaymentInfo pmntInfo)
        {
            int paymentID = 1;
             if (db.PaymentInfos.Count() != 0)
            {
                paymentID = db.PaymentInfos.Max(c => c.ID) + 1;
            }

             db.PaymentInfos.InsertOnSubmit(pmntInfo);
             db.SubmitChanges();
             return db.PaymentInfos.Max(c => c.ID); ;
        }
        public int GetTruckId()
        {
            if (db.Transports.Count() != 0)
            {
                return db.Transports.Max(c => c.ID);
            }
            return 1;
        }
        public int insertTransportInfo(Transport transport)
        {
            db.Transports.InsertOnSubmit(transport);
            db.SubmitChanges();
            return transport.ID;

        }
        public void insertOrderItems(OrderItem orderItem)
        {
            if (orderItem.ID == 0)
            {
                db.OrderItems.InsertOnSubmit(orderItem);
            }
            else
            {
                db.OrderItems.Attach(orderItem); 
            }
                db.SubmitChanges();
        }
        public int GetNextOrderItemID()
        {
            
            if (db.OrderItems.Count() != 0)
            {
                return db.OrderItems.Max(c => c.ID) + 1;
            }
            return 1;
        }
        public int GetNextCMRID()
        {

            if (db.CMRs.Count() != 0)
            {
                return db.CMRs.Max(c => c.ID) + 1;
            }
            return 1000000;
        }
      
        public CMR GetCMRByID(int cmrId)
        {
            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues,db.CMRs);
            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.PaymentInfos);
            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.OrderItems);
           
            
            var foundRow =  db.CMRs.Where(c => c.ID == cmrId);
            if (foundRow.Count() > 1 || foundRow.Count() == 0)
            {
                return null;
            }
            return foundRow.Single();
        }

        public CMR GetCMRWithDetailsByID(int cmrId)
        {
            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.CMRs);
            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.PaymentInfos);
            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.OrderItems);


            var foundRow = db.CMRs.Where(c => c.ID == cmrId);
            if (foundRow.Count() > 1 || foundRow.Count() == 0)
            {
                return null;
            }

            var foundItmeRow = db.OrderItems.Where(c => c.CMRID == cmrId);

            return foundRow.Single();
        }
    }
}
