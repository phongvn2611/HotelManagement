using QLKS.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLKS.ViewModel
{
    public class HoaDonAnUongViewModel : BaseViewModel
    {
        private int _MaHD;
        public int MaHD { get => _MaHD; set { _MaHD = value; OnPropertyChanged(); } }
        private string _LoaiPhucVu;
        public string LoaiPhucVu { get => _LoaiPhucVu; set { _LoaiPhucVu = value; OnPropertyChanged(); } }
        private long _TongTien;
        public long TongTien { get => _TongTien; set { _TongTien = value; OnPropertyChanged(); } }
        private ObservableCollection<ThongTinOrder> _ListOrder;
        public ObservableCollection<ThongTinOrder> ListOrder { get => _ListOrder; set { _ListOrder = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public HoaDonAnUongViewModel()
        {
            CancelCommand = new RelayCommand<Window>((p) => { return p == null ? false : true; }, (p) => { p.Close(); });

            SaveCommand = new RelayCommand<Window>((p) => 
            {
                if (p == null)
                    return false;

                var hoadonVM = p.DataContext as HoaDonViewModel;
                if (hoadonVM.ListOrder == null || hoadonVM.ListOrder.Count() == 0)
                    return false;

                return true;
            }, (p) =>
            {
                //lấy thông tin phòng chọn thuê và nhân viên làm hóa đơn
                var hoadonVM = p.DataContext as HoaDonViewModel;
                MaHD = hoadonVM.MaHD;
                ListOrder = hoadonVM.ListOrder;
                //DateTime ThoiGianLapHD = new DateTime(hoadonVM.DateLapHD.Year, hoadonVM.DateLapHD.Month, hoadonVM.DateLapHD.Day,
                //                                      hoadonVM.TimeLapHD.Hour, hoadonVM.TimeLapHD.Minute, hoadonVM.TimeLapHD.Second);
                //Tạo chi tiết hóa đơn lưu trú
                foreach (ThongTinOrder item in ListOrder)
                {
                    var chitietHDAU = new CHITIET_HDAU() { MA_HD = MaHD, MA_MH = item.MatHang.MA_MH, SOLUONG_MH = item.SoLuong, TRIGIA_CTHDAU = item.ThanhTien, THOIGIANLAP_CTHDAU = DateTime.Now };
                    DataProvider.Ins.model.CHITIET_HDAU.Add(chitietHDAU);
                }
                DataProvider.Ins.model.SaveChanges();

                MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                p.Close();
            });
        }
    }
}
