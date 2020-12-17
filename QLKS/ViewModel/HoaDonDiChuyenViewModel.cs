using QLKS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLKS.ViewModel
{
    class HoaDonDiChuyenViewModel : BaseViewModel
    {
        private int _MaHD;
        public int MaHD { get => _MaHD; set { _MaHD = value; OnPropertyChanged(); } }
        private CHUYENDI _ChuyenDi;
        public CHUYENDI ChuyenDi { get => _ChuyenDi; set { _ChuyenDi = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public HoaDonDiChuyenViewModel()
        {
            CancelCommand = new RelayCommand<Window>((p) => { return p == null ? false : true; }, (p) => { p.Close(); });

            SaveCommand = new RelayCommand<Window>((p) => 
            {
                if(p == null)
                    return false;

                var hoadonVM = p.DataContext as HoaDonViewModel;
                if (hoadonVM.ChuyenDi == null)
                    return false;

                return true;
            }, (p) =>
            {
                //lấy thông tin phòng chọn thuê, nhân viên làm hóa đơn và thời gian làm hóa đơn
                var hoadonVM = p.DataContext as HoaDonViewModel;
                MaHD = hoadonVM.MaHD;
                ChuyenDi = hoadonVM.ChuyenDi;
                //DateTime ThoiGianLapHD = new DateTime(hoadonVM.DateLapHD.Year, hoadonVM.DateLapHD.Month, hoadonVM.DateLapHD.Day,
                //                                      hoadonVM.TimeLapHD.Hour, hoadonVM.TimeLapHD.Minute, hoadonVM.TimeLapHD.Second);
                //Thêm chi tiết hóa đơn giặt ủi
                var chitietHDDC = new CHITIET_HDDC() { MA_HD = MaHD, MA_CD = ChuyenDi.MA_CD, TRIGIA_CTHDDC = ChuyenDi.DONGIA_CD, THOIGIANLAP_CTHDDC = DateTime.Now };
                DataProvider.Ins.model.CHITIET_HDDC.Add(chitietHDDC);
                DataProvider.Ins.model.SaveChanges();

                MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                p.Close();
            });
        }
    }
}
