using QLKS.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLKS.ViewModel
{
    public class LuotGiatUiViewModel : BaseViewModel
    {
        private ThongTinGiatUi _TTGiatUi;
        public ThongTinGiatUi TTGiatUi { get => _TTGiatUi; set { _TTGiatUi = value; OnPropertyChanged(); } }

        private ObservableCollection<ThongTinPhong> _ListTTPhongDangThue;
        public ObservableCollection<ThongTinPhong> ListTTPhongDangThue { get => _ListTTPhongDangThue; set { _ListTTPhongDangThue = value; OnPropertyChanged(); } }

        private ObservableCollection<LOAIGIATUI> _ListLoaiGiatUi;
        public ObservableCollection<LOAIGIATUI> ListLoaiGiatUi { get => _ListLoaiGiatUi; set { _ListLoaiGiatUi = value; OnPropertyChanged(); } }

        private LOAIGIATUI _SelectedItem;
        public LOAIGIATUI SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    TenLoaiGiatUi = SelectedItem.TEN_LOAIGU;
                    DonGia = (int)SelectedItem.DONGIA_LOAIGU;                   
                }
            }
        }
        //Chọn phòng thực hiện sử dụng dịch vụ ăn uống
        private int _MaPhong;
        public int MaPhong { get => _MaPhong; set { _MaPhong = value; OnPropertyChanged(); } }
        private ThongTinPhong _SelectedPhong;
        public ThongTinPhong SelectedPhong { get => _SelectedPhong; set { _SelectedPhong = value; OnPropertyChanged(); if (_SelectedPhong != null) MaPhong = SelectedPhong.Phong.MA_PHONG; } }
        private KHACHHANG _KhachHangThue;
        public KHACHHANG KhachHangThue { get => _KhachHangThue; set { _KhachHangThue = value; OnPropertyChanged(); } }
        private NHANVIEN _NhanVienLapHD;
        public NHANVIEN NhanVienLapHD { get => _NhanVienLapHD; set { _NhanVienLapHD = value; OnPropertyChanged(); } }

        private string _TenLoaiGiatUi;
        public string TenLoaiGiatUi { get => _TenLoaiGiatUi; set { _TenLoaiGiatUi = value; OnPropertyChanged(); } }
        private int _DonGia;
        public int DonGia { get => _DonGia; set { _DonGia = value; OnPropertyChanged(); } }
        private int _CanNang;
        public int CanNang { get => _CanNang; set { _CanNang = value; OnPropertyChanged(); } }
        private DateTime _NgayBatDau;
        public DateTime NgayBatDau { get => _NgayBatDau; set { _NgayBatDau = value; OnPropertyChanged(); } }
        private DateTime _NgayKetThuc;
        public DateTime NgayKetThuc { get => _NgayKetThuc; set { _NgayKetThuc = value; OnPropertyChanged(); } }
        private int _ThanhTien;
        public int ThanhTien { get => _ThanhTien; set { _ThanhTien = value; OnPropertyChanged(); } }

        public ICommand ThanhTienCommand { get; set; }
        public ICommand ThanhTienKgCommand { get; set; }
        public ICommand ThanhTienNgayCommand { get; set; }
        public ICommand ShowHDGiatUiCommand { get; set; }

        public LuotGiatUiViewModel()
        {
            ListLoaiGiatUi=new ObservableCollection<LOAIGIATUI>(DataProvider.Ins.model.LOAIGIATUI);
            NgayBatDau = DateTime.Now;
            NgayKetThuc = DateTime.Now;
            ThanhTien = 0;

            ThanhTienCommand = new RelayCommand<TextBox>((p) => { return true; }, (p) =>
            {
                bool loaiThanhTien = p.IsEnabled;
                if (loaiThanhTien)
                {
                    ThanhTien = DonGia * CanNang;
                }
                else
                {
                    TimeSpan time = NgayKetThuc.Subtract(NgayBatDau);
                    if (time.TotalDays < 0)
                        ThanhTien = 0;
                    else
                        ThanhTien = DonGia * ((int)time.TotalDays + 1);
                }
            });

            ThanhTienKgCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                ThanhTien = DonGia * CanNang;
            });

            ThanhTienNgayCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                TimeSpan time = NgayKetThuc.Subtract(NgayBatDau);
                if(time.TotalDays < 0)
                    ThanhTien = 0;
                else
                    ThanhTien = DonGia * ((int)time.TotalDays + 1);
            });

            ShowHDGiatUiCommand = new RelayCommand<Window>((p) =>
            {
                if (p == null || p.DataContext == null)
                    return false;

                if (ThanhTien == 0 || SelectedPhong == null || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }, (p) =>
            {
                GetThongTinGiatUi();

                HoaDon wd = new HoaDon();
                if (wd.DataContext == null)
                    return;
                var hoadonVM = wd.DataContext as HoaDonViewModel;
                hoadonVM.LoaiHD = (int)HoaDonViewModel.LoaiHoaDon.HoaDonGiatUi;
                hoadonVM.HoaDon = hoadonVM.GetHoaDon(MaPhong);
                hoadonVM.NhanVienLapHD = (p.DataContext as MainViewModel).NhanVien;
                hoadonVM.KhachHangThue = hoadonVM.GetKhachHang(hoadonVM.HoaDon);
                hoadonVM.CMND_KH = hoadonVM.KhachHangThue.CMND_KH;
                hoadonVM.GetThongTinPhongThue(MaPhong);

                hoadonVM.TongTienHDGU = ThanhTien;
                hoadonVM.TTGiatUi = TTGiatUi;
                wd.ShowDialog();
                RefershControlsDVGU();
            });
        }

        public void GetTTPhongDangThue()
        {
            ListTTPhongDangThue = new ObservableCollection<ThongTinPhong>();
            var listTTPhongdangthue = from ph in DataProvider.Ins.model.PHONG
                                      join lp in DataProvider.Ins.model.LOAIPHONG
                                      on ph.MA_LP equals lp.MA_LP
                                      where ph.TINHTRANG_PHONG == "Đang thuê"
                                      select new ThongTinPhong()
                                      {
                                          Phong = ph,
                                          LoaiPhong = lp
                                      };
            foreach (ThongTinPhong item in listTTPhongdangthue)
            {
                ListTTPhongDangThue.Add(item);
            }
        }

        void GetThongTinGiatUi()
        {
            LUOTGIATUI luotGiatUi = new LUOTGIATUI();
            LOAIGIATUI loaiGiatUi = new LOAIGIATUI();
            if (SelectedItem.MA_LOAIGU == 1)
            {
                luotGiatUi.MA_LOAIGU = 1;
                luotGiatUi.SOKILOGRAM_LUOTGU = CanNang;
                luotGiatUi.NGAYBATDAU_LUOTGU = null;
                luotGiatUi.NGAYKETTHUC_LUOTGU = null;
            }
            else if (SelectedItem.MA_LOAIGU == 2)
            {
                luotGiatUi.MA_LOAIGU = 2;
                luotGiatUi.SOKILOGRAM_LUOTGU = 0;
                luotGiatUi.NGAYBATDAU_LUOTGU = NgayBatDau;
                luotGiatUi.NGAYKETTHUC_LUOTGU = NgayKetThuc;
            }
            var loaigu = DataProvider.Ins.model.LOAIGIATUI.Where(x => x.MA_LOAIGU == luotGiatUi.MA_LOAIGU).SingleOrDefault();
            TTGiatUi = new ThongTinGiatUi() { LuotGiatUi = luotGiatUi, LoaiGiatUi = loaigu};
        }

        void RefershControlsDVGU()
        {
            SelectedPhong = null;
            SelectedItem = null;
            NgayBatDau = DateTime.Now;
            NgayKetThuc = DateTime.Now;
            ThanhTien = 0;
            DonGia = 0;
            CanNang = 0;
        }
    }
}
