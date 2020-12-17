using QLKS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace QLKS.ViewModel
{
    public class HoaDonLuuTruViewModel : BaseViewModel
    {
        private ThongTinPhong _ThongTinPhongChonThue;
        public ThongTinPhong ThongTinPhongChonThue { get => _ThongTinPhongChonThue; set { _ThongTinPhongChonThue = value; OnPropertyChanged(); } }
        private NHANVIEN _NhanVienLapHD;
        public NHANVIEN NhanVienLapHD { get => _NhanVienLapHD; set { _NhanVienLapHD = value; OnPropertyChanged(); } }
        private KHACHHANG _KhachHangThue;
        public KHACHHANG KhachHangThue { get => _KhachHangThue; set { _KhachHangThue = value; OnPropertyChanged(); } }
        private string _CMND_KH;
        public string CMND_KH { get => _CMND_KH; set { _CMND_KH = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }


        public HoaDonLuuTruViewModel()
        {
            KhachHangThue = new KHACHHANG();            

            CancelCommand = new RelayCommand<Window>((p) => { return p == null ? false : true; }, (p) => { p.Close(); });

            SaveCommand = new RelayCommand<Window>((p) => 
            {
                if (p == null || p.DataContext == null)
                    return false;

                var hoadonVM = p.DataContext as HoaDonViewModel;
                if (hoadonVM.MaPhong == 0)
                    return false;

                if (hoadonVM.MaHD == 0 && (string.IsNullOrEmpty(hoadonVM.CMND_KH) || string.IsNullOrEmpty(hoadonVM.KhachHangThue.HOTEN_KH)))
                {
                    return false;
                }

                if (hoadonVM.MaHD != 0)
                    return false;

                return true;
            }, (p) =>
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //lấy thông tin phòng chọn thuê, nhân viên làm hóa đơn và thời gian làm hóa đơn
                        var hoadonVM = p.DataContext as HoaDonViewModel;
                        ThongTinPhongChonThue = hoadonVM.ThongTinPhongChonThue;
                        NhanVienLapHD = hoadonVM.NhanVienLapHD;
                        KhachHangThue = hoadonVM.KhachHangThue;
                        CMND_KH = hoadonVM.CMND_KH;

                        //kiểm tra xem khách hàng đã có trong csdl của khách sạn hay chưa
                        var khachHang = DataProvider.Ins.model.KHACHHANG.Where(x => x.CMND_KH == CMND_KH).SingleOrDefault();
                        if (khachHang == null)
                        {
                            KHACHHANG newKhachHang = new KHACHHANG() { HOTEN_KH = KhachHangThue.HOTEN_KH, SODIENTHOAI_KH = KhachHangThue.SODIENTHOAI_KH, CMND_KH = CMND_KH };
                            DataProvider.Ins.model.KHACHHANG.Add(newKhachHang);
                            DataProvider.Ins.model.SaveChanges();
                            khachHang = newKhachHang;
                        }
                        //Tạo hóa đơn tổng
                        var hd = new HOADON() { MA_NV = NhanVienLapHD.MA_NV, MA_KH = khachHang.MA_KH, THOIGIANLAP_HD = DateTime.Now, TINHTRANG_HD = false };
                        DataProvider.Ins.model.HOADON.Add(hd);
                        DataProvider.Ins.model.SaveChanges();
                        //Tạo chi tiết hóa đơn lưu trú
                        var chitietHDLT = new CHITIET_HDLT() { MA_HD = hd.MA_HD, MA_PHONG = ThongTinPhongChonThue.Phong.MA_PHONG, THOIGIANNHAN_PHONG = DateTime.Now };
                        DataProvider.Ins.model.CHITIET_HDLT.Add(chitietHDLT);
                        DataProvider.Ins.model.SaveChanges();
                        //Đổi trạng thái của phòng
                        var phong = DataProvider.Ins.model.PHONG.Where(x => x.MA_PHONG == ThongTinPhongChonThue.Phong.MA_PHONG).SingleOrDefault();
                        phong.TINHTRANG_PHONG = "Đang thuê";
                        DataProvider.Ins.model.SaveChanges();
                       
                        ts.Complete();
                        MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e + "\n\tLưu không thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }                

                p.Close();
            });
        }
    }
}
