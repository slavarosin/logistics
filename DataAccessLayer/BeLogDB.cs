using System;
using System.Linq;
using System.Collections.Generic;

namespace DataAccessLayer
{
    public class CompanyEqualityComparer : IEqualityComparer<Company>
    {
        public bool Equals(Company x, Company y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(Company obj)
        {
            int byteInfo = 0;
            try
            {
                if (obj.Name != String.Empty)
                {
                    obj.Name.ToList().ForEach(c => byteInfo += Convert.ToInt32(c));
                }
            }
            catch
            {

            }
            return obj.Name.Length + byteInfo;
        }
    }

    public partial class User
    {

        public bool IsAdmin
        {
            get { return this.ApplicationRights.Contains("Admin"); }
        }

        public List<string> RightsList
        {
            get
            {
                if (this.ApplicationRights == null) return new List<string>();
                return new List<string>(this.ApplicationRights.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            }
            set
            {
                string userRights = String.Empty;
                foreach (var rights in value)
                {
                    userRights += rights + ";";
                }
                this.ApplicationRights = userRights;
            }
        }
    }
  
}
