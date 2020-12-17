using QLKS.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace QLKS.Model
{
    public class ThongTinPhong : BaseViewModel
    {
        public PHONG Phong { get; set; }
        private LOAIPHONG _LoaiPhong;
        public LOAIPHONG LoaiPhong { get => _LoaiPhong; set { _LoaiPhong = value; OnPropertyChanged(); } }
    }
}
