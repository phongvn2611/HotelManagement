using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QLKS.Model
{
    class ThongTinBaoCao : INotifyPropertyChanged
    {
        private string _Item = string.Empty;
        private int _DoanhThu = 0;
        private double _TiLe = 0;

        public string Item
        {
            get { return _Item; }
            set
            {
                _Item = value;
                NotifyPropertyChanged("Item");
            }
        }

        public int DoanhThu
        {
            get { return _DoanhThu; }
            set
            {
                _DoanhThu = value;
                NotifyPropertyChanged("DoanhThu");
            }

        }

        public double TiLe
        {
            get { return _TiLe; }
            set
            {
                _TiLe = value;
                NotifyPropertyChanged("TiLe");
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
