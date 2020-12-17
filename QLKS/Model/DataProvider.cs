using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLKS.Model
{
    public class DataProvider
    {
        private static DataProvider _ins;
        public static DataProvider Ins { get { if (_ins == null) _ins = new DataProvider(); return _ins; } set { _ins = value; } }
        public QLKS_IS201Entities model;
       
        public DataProvider()
        {
            model = new QLKS_IS201Entities();
        }
    }
}
