using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAccessLayer;
using System.ComponentModel;

namespace BeLog.Logic.Containers
{
    public class ManifestItem:INotifyPropertyChanged
    {
        private Company consignor;
        private Company consignee;
        public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }

        public int PositionId { get; set; }
        //public string Consignor { get; set; }
        //public string Consignee { get; set; }
        public Company Consignor
        {
            get { return this.consignor; }
            set {
                if(value != this.consignor)
                {
                 this.consignor = value;
                  NotifyPropertyChanged("Consignor");
                }
            }
        }
        public Company Consignee
        {
            get { return this.consignee; }
            set { this.consignee = value; }
        }
        public int CMRId { get; set; }
        public int PlaceNum { get; set; }
        public string PackagingMode { get; set; }
        public decimal Bruttoweight { get; set; }
        public bool IsNewItem
        {
            get { return (this.CMRId == 0); }
        }
        public bool AllowEdit
        {
            get { return false; } //return (this.CMRId !=0); }
        }
    }
}
