using QLKS.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QLKS.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public enum ChucNangKhachSan
        {
            TrangChu, DichVuAnUong, DichVuGiatUi, DichVuDiChuyen, TraCuu, BaoCao, TroGiup
        };
        private int _ChucNangKS;
        public int ChucNangKS { get => _ChucNangKS; set { _ChucNangKS = value; OnPropertyChanged(); } }

        private ObservableCollection<ThongTinPhong> _ListTTPhong;
        public ObservableCollection<ThongTinPhong> ListTTPhong { get => _ListTTPhong; set { _ListTTPhong = value; OnPropertyChanged(); } }
        private int _TongSoPhong;
        public int TongSoPhong { get => _TongSoPhong; set { _TongSoPhong = value; OnPropertyChanged(); } }
        private int _SoPhongTrong;
        public int SoPhongTrong { get => _SoPhongTrong; set { _SoPhongTrong = value;  OnPropertyChanged(); } }
        private int _SoPhongDangThue;
        public int SoPhongDangThue { get => _SoPhongDangThue; set { _SoPhongDangThue = value;  OnPropertyChanged(); } }
        private int _SoPhongDangSuaChua;
        public int SoPhongDangSuaChua { get => _SoPhongDangSuaChua; set { _SoPhongDangSuaChua = value; OnPropertyChanged(); } }

        private NHANVIEN _NhanVien;
        public NHANVIEN NhanVien { get => _NhanVien; set { _NhanVien = value; OnPropertyChanged(); } }
        private KHACHHANG _KhachHangThue;
        public KHACHHANG KhachHangThue { get => _KhachHangThue; set { _KhachHangThue = value; OnPropertyChanged(); } }
        private int _MaPhongChonThue;
        public int MaPhongChonThue { get => _MaPhongChonThue; set { _MaPhongChonThue = value; OnPropertyChanged(); } }

        public ICommand btnTrangChuCommand { get; set; }
        public ICommand btnDVAnUongCommand { get; set; }
        public ICommand btnDVGiatUiCommand { get; set; }
        public ICommand btnDVDiChuyenCommand { get; set; }
        public ICommand btnTraCuuCommand { get; set; }
        public ICommand btnBaoCaoCommand { get; set; }
        public ICommand btnTroGiupCommand { get; set; }

        public ICommand LoadedWindowCommand { get; set; }
        public ICommand LoadTatCaPhongCommand { get; set; }
        public ICommand LoadPhongTrongCommand { get; set; }
        public ICommand LoadPhongDangThueCommand { get; set; }
        public ICommand LoadPhongDangSuaChuaCommand { get; set; }

        public ICommand DangXuatCommand { get; set; }

        public ICommand ChonPhongCommand { get; set; }
        public ICommand ThuePhongCommand { get; set; }
        public ICommand ShowHDTongCommand { get; set; }

        public MainViewModel()
        {
            #region Xử lý ản hiện view
            btnTrangChuCommand = new RelayCommand<Object>((p) => { return true; }, (p) => 
            {
                ChucNangKS = (int)ChucNangKhachSan.TrangChu;
                MaPhongChonThue = 0;
                LoadTongSoPhong();
                LoadSoPhongDangThue();
                LoadSoPhongTrong();
                LoadSoPhongDangSuaChua();
            });
            btnDVAnUongCommand = new RelayCommand<Grid>((p) =>
            {
                if (p == null || p.DataContext == null)
                    return false;

                return true;
            }, (p) =>
            {
                ChucNangKS = (int)ChucNangKhachSan.DichVuAnUong;
                var mathangVM = p.DataContext as MatHangViewModel;
                mathangVM.GetTTPhongDangThue();
                MaPhongChonThue = 0;
            });
            btnDVGiatUiCommand = new RelayCommand<Grid>((p) =>
            {
                if (p == null || p.DataContext == null)
                    return false;

                return true;
            }, (p) =>
            {
                ChucNangKS = (int)ChucNangKhachSan.DichVuGiatUi;
                var giatuiVM = p.DataContext as LuotGiatUiViewModel;
                giatuiVM.GetTTPhongDangThue();
                MaPhongChonThue = 0;
            });
            btnDVDiChuyenCommand = new RelayCommand<Grid>((p) =>
            {
                if (p == null || p.DataContext == null)
                    return false;

                return true;
            }, (p) =>
            {
                ChucNangKS = (int)ChucNangKhachSan.DichVuDiChuyen;
                var dichuyenVM = p.DataContext as DiChuyenViewModel;
                dichuyenVM.GetTTPhongDangThue();
                MaPhongChonThue = 0;
            });
            btnTraCuuCommand = new RelayCommand<Object>((p) => 
            {
                return true;
            }, (p) => 
            { 
                ChucNangKS = (int)ChucNangKhachSan.TraCuu; MaPhongChonThue = 0; 
            });
            btnBaoCaoCommand = new RelayCommand<Object>((p) => { return true; }, (p) => { ChucNangKS = (int)ChucNangKhachSan.BaoCao; MaPhongChonThue = 0; });
            btnTroGiupCommand = new RelayCommand<Object>((p) => { return true; }, (p) => { ChucNangKS = (int)ChucNangKhachSan.TroGiup; MaPhongChonThue = 0; });
            #endregion

            #region Xử lý Load trang chủ
            LoadedWindowCommand = new RelayCommand<Window>((p) => { return p == null ? false : true; }, (p) =>
            {
                  p.Show();
                  LoadTTPhong();

                  TongSoPhong = ListTTPhong.Count();
                  SoPhongTrong = ListTTPhong.Where(x => x.Phong.TINHTRANG_PHONG == "Trống").Count();
                  SoPhongDangThue = ListTTPhong.Where(x => x.Phong.TINHTRANG_PHONG == "Đang thuê").Count();
                  SoPhongDangSuaChua = ListTTPhong.Where(x => x.Phong.TINHTRANG_PHONG == "Đang sửa chữa").Count();
            });

            DangXuatCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                  MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn đăng xuất khỏi hệ thống không?", "Đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);
                  if (result == MessageBoxResult.Yes)
                  {
                      p.Hide();
                      DangNhap dangnhap = new DangNhap();
                      dangnhap.ShowDialog();
                      p.Close();
                  }
            });

            LoadTatCaPhongCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                ListTTPhong = LoadTTPhong();

            });

            LoadPhongTrongCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                ListTTPhong = LoadTTPhong("Trống");
                

            });

            LoadPhongDangThueCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                ListTTPhong = LoadTTPhong("Đang thuê");

            });

            LoadPhongDangSuaChuaCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                ListTTPhong = LoadTTPhong("Đang sửa chữa");

            });
            #endregion

            MaPhongChonThue = 0;

            ChonPhongCommand = new RelayCommand<Button>((p) => { return p == null ? false : true; }, (p) =>
            {
                MaPhongChonThue = Int32.Parse(p.Tag.ToString());
            });

            ThuePhongCommand = new RelayCommand<Button>((p) =>
            {
                if (MaPhongChonThue == 0)
                {
                    MessageBox.Show("Vui lòng chọn phòng cần thuê!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var phong = DataProvider.Ins.model.PHONG.Where(x => x.MA_PHONG == MaPhongChonThue).SingleOrDefault();
                if (phong == null || phong.TINHTRANG_PHONG != "Trống")
                {
                    MessageBox.Show("Vui lòng chọn phòng trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }, (p) =>
            {
                HoaDon hd = new HoaDon();
                if (hd.DataContext == null)
                    return;
                var hoadonVM = hd.DataContext as HoaDonViewModel;
                hoadonVM.LoaiHD = (int)HoaDonViewModel.LoaiHoaDon.HoaDonLuuTru;
                hoadonVM.NhanVienLapHD = NhanVien;
                hoadonVM.CMND_KH = "";

                hoadonVM.MaPhong = MaPhongChonThue;
                hoadonVM.GetThongTinPhongThue(MaPhongChonThue);
                hd.ShowDialog();
                LoadTongSoPhong();
                LoadSoPhongDangThue();
                LoadSoPhongTrong();
            });

            ShowHDTongCommand = new RelayCommand<Object>((p) =>
            {
                if (MaPhongChonThue == 0)
                {
                    MessageBox.Show("Vui lòng chọn phòng muốn thanh toán!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var phong = DataProvider.Ins.model.PHONG.Where(x => x.MA_PHONG == MaPhongChonThue).SingleOrDefault();
                if (phong != null && phong.TINHTRANG_PHONG == "Đang thuê")
                    return true;

                MessageBox.Show("Vui lòng chọn phòng đang cho thuê!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                HoaDon wd = new HoaDon();
                if (wd.DataContext == null)
                    return;
                var hoadonVM = wd.DataContext as HoaDonViewModel;
                hoadonVM.LoaiHD = (int)HoaDonViewModel.LoaiHoaDon.HoaDonTong;
                hoadonVM.HoaDon = hoadonVM.GetHoaDon(MaPhongChonThue);
                hoadonVM.NhanVienLapHD = NhanVien;
                hoadonVM.KhachHangThue = hoadonVM.GetKhachHang(hoadonVM.HoaDon);
                hoadonVM.CMND_KH = hoadonVM.KhachHangThue.CMND_KH;
                hoadonVM.GetThongTinPhongThue(MaPhongChonThue);
                wd.ShowDialog();
                LoadTongSoPhong();
                LoadSoPhongDangThue();
                LoadSoPhongTrong();
            });
        }

        public ObservableCollection<ThongTinPhong> LoadTTPhong()
        {
            ListTTPhong = new ObservableCollection<ThongTinPhong>();
            var listTTPhong = from p in DataProvider.Ins.model.PHONG
                              join lp in DataProvider.Ins.model.LOAIPHONG
                              on p.MA_LP equals lp.MA_LP

                              select new ThongTinPhong()
                              {
                                  Phong = p,
                                  LoaiPhong = lp
                              };
            foreach (ThongTinPhong item in listTTPhong)
            {
                ListTTPhong.Add(item);
            }
            return ListTTPhong;

        }

        public ObservableCollection<ThongTinPhong> LoadTTPhong(string TinhTrangPhong)
        {
            ListTTPhong = new ObservableCollection<ThongTinPhong>();
            var listTTPhong = from ph in DataProvider.Ins.model.PHONG
                              join lp in DataProvider.Ins.model.LOAIPHONG
                              on ph.MA_LP equals lp.MA_LP
                              where ph.TINHTRANG_PHONG == TinhTrangPhong
                              select new ThongTinPhong()
                              {
                                  Phong = ph,
                                  LoaiPhong = lp
                              };
            foreach (ThongTinPhong item in listTTPhong)
            {
                ListTTPhong.Add(item);
            }
            return ListTTPhong;
        }

        public void LoadTongSoPhong()
        {
            TongSoPhong = DataProvider.Ins.model.PHONG.Count();
        }
        public void LoadSoPhongTrong()
        {
            SoPhongTrong = DataProvider.Ins.model.PHONG.Where(x => x.TINHTRANG_PHONG == "Trống").Count();
        }
        public void LoadSoPhongDangThue()
        {
            SoPhongDangThue = DataProvider.Ins.model.PHONG.Where(x => x.TINHTRANG_PHONG == "Đang thuê").Count();
        }
        public void LoadSoPhongDangSuaChua()
        {
            SoPhongDangSuaChua = DataProvider.Ins.model.PHONG.Where(x => x.TINHTRANG_PHONG == "Đang sửa chữa").Count();
        }
    }
}
