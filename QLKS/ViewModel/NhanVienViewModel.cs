using QLKS.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace QLKS.ViewModel
{
    public class NhanVienViewModel : BaseViewModel
    {
        //Datacontext
        private ObservableCollection<ThongTinNhanVien> _ListTTNhanVien;
        public ObservableCollection<ThongTinNhanVien> ListTTNhanVien { get => _ListTTNhanVien; set { _ListTTNhanVien = value; OnPropertyChanged(); } }
        //ItemsSource combobox
        private ObservableCollection<string> _ListChucVu;
        public ObservableCollection<string> ListChucVu { get => _ListChucVu; set { _ListChucVu = value; OnPropertyChanged(); } }
        private ObservableCollection<string> _ListGioiTinh;
        public ObservableCollection<string> ListGioiTinh { get => _ListGioiTinh; set { _ListGioiTinh = value; OnPropertyChanged(); } }
        private ThongTinNhanVien _SelectedItem;
        public ThongTinNhanVien SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    TenDangNhap = SelectedItem.TaiKhoan.TENDANGNHAP_TK;
                    MatKhau = null;
                    TenNhanVien = SelectedItem.NhanVien.HOTEN_NV;
                    SelectedGioiTinh = (bool)SelectedItem.NhanVien.GIOITINH_NV? "Nam" : "Nữ";
                    NgaySinh = SelectedItem.NhanVien.NGAYSINH_NV;
                    SoDienThoai = SelectedItem.NhanVien.SODIENTHOAI_NV;
                    SelectedChucVu = SelectedItem.NhanVien.CHUCVU_NV;
                    DiaChi = SelectedItem.NhanVien.DIACHI_NV;
                    NgayVaoLam = SelectedItem.NhanVien.NGAYVAOLAM_NV;
                }
            }
        }

        #region Thuộc tính Binding
        private string _TenDangNhap;
        public string TenDangNhap { get => _TenDangNhap; set { _TenDangNhap = value; OnPropertyChanged(); } }
        private string _MatKhau;
        public string MatKhau { get => _MatKhau; set { _MatKhau = value; OnPropertyChanged(); } }
        private string _TenNhanVien;
        public string TenNhanVien { get => _TenNhanVien; set { _TenNhanVien = value; OnPropertyChanged(); } }
        private string _SelectedGioiTinh;
        public string SelectedGioiTinh { get => _SelectedGioiTinh; set { _SelectedGioiTinh = value; OnPropertyChanged(); } }
        private DateTime? _NgaySinh;
        public DateTime? NgaySinh { get => _NgaySinh; set { _NgaySinh = value; OnPropertyChanged(); } }
        private string _SoDienThoai;
        public string SoDienThoai { get => _SoDienThoai; set { _SoDienThoai = value; OnPropertyChanged(); } }
        private string _SelectedChucVu;
        public string SelectedChucVu { get => _SelectedChucVu; set { _SelectedChucVu = value; OnPropertyChanged(); } }
        private string _DiaChi;
        public string DiaChi { get => _DiaChi; set { _DiaChi = value; OnPropertyChanged(); } }
        private DateTime? _NgayVaoLam;
        public DateTime? NgayVaoLam { get => _NgayVaoLam; set { _NgayVaoLam = value; OnPropertyChanged(); } }

        private string _SearchNhanVien;
        public string SearchNhanVien { get => _SearchNhanVien; set { _SearchNhanVien = value; OnPropertyChanged(); } }
        #endregion
        public bool sort;

        public ICommand PasswordChangedCommand { get; set; }
        public ICommand SearchNhanVienCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SortNhanVienCommand { get; set; }

        public NhanVienViewModel()
        {
            #region Load dữ liệu nhân viên
            LoadTTNhanVien();
            string[] chucvus = new string[] { "Quản lí", "Nhân viên" };
            ListChucVu = new ObservableCollection<string>(chucvus);
            string[] gioitinhs = new string[] { "Nam", "Nữ" };
            ListGioiTinh = new ObservableCollection<string>(gioitinhs);
            #endregion

            PasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return p == null ? false : true; }, (p) => { MatKhau = p.Password; });

            SearchNhanVienCommand = new RelayCommand<Object>((p) => { return true; }, (p) => {
                if (string.IsNullOrEmpty(SearchNhanVien))
                {
                    CollectionViewSource.GetDefaultView(ListTTNhanVien).Filter = (all) => { return true; };                    
                }
                else
                {
                    CollectionViewSource.GetDefaultView(ListTTNhanVien).Filter = (searchNhanVien) =>
                    {
                        return (searchNhanVien as ThongTinNhanVien).NhanVien.HOTEN_NV.IndexOf(SearchNhanVien, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchNhanVien as ThongTinNhanVien).NhanVien.CHUCVU_NV.IndexOf(SearchNhanVien, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }
                
            });

            AddCommand = new RelayCommand<PasswordBox>((p) =>
            {
                if (p == null)
                    return false;

                if (string.IsNullOrEmpty(TenDangNhap) || string.IsNullOrEmpty(MatKhau) || string.IsNullOrEmpty(TenNhanVien) ||
                SelectedChucVu == null || SelectedGioiTinh == null)
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin nhân viên muốn thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var listTenDangNhap = DataProvider.Ins.model.TAIKHOAN.Where(x => x.TENDANGNHAP_TK == TenDangNhap);
                if (listTenDangNhap == null || listTenDangNhap.Count() != 0)
                {
                    MessageBox.Show("Tài khoản đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }, (p) =>
            {
                var taiKhoan = new TAIKHOAN() { TENDANGNHAP_TK = TenDangNhap, MATKHAU_TK = MatKhau};
                DataProvider.Ins.model.TAIKHOAN.Add(taiKhoan);
                DataProvider.Ins.model.SaveChanges();

                var taiKhoanMoiTao = DataProvider.Ins.model.TAIKHOAN.Where(x => x.TENDANGNHAP_TK == TenDangNhap).SingleOrDefault();
                var nhanVien = new NHANVIEN()
                {
                    MA_TK = taiKhoanMoiTao.MA_TK,
                    HOTEN_NV = TenNhanVien,
                    GIOITINH_NV = ConvertGioiTinh(SelectedGioiTinh),
                    NGAYSINH_NV = NgaySinh,
                    SODIENTHOAI_NV = SoDienThoai,
                    CHUCVU_NV = SelectedChucVu,
                    DIACHI_NV = DiaChi,
                    NGAYVAOLAM_NV = NgayVaoLam
                };
                DataProvider.Ins.model.NHANVIEN.Add(nhanVien);
                DataProvider.Ins.model.SaveChanges();
                MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                p.Password = string.Empty;
                LoadTTNhanVien();
                RefershControls();
            });

            EditCommand = new RelayCommand<PasswordBox>((p) =>
            {
                if (p == null)
                    return false;

                if (string.IsNullOrEmpty(TenDangNhap) || string.IsNullOrEmpty(TenNhanVien) ||
                    SelectedChucVu == null || SelectedGioiTinh == null || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên muốn sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var listTTNV = DataProvider.Ins.model.TAIKHOAN.Where(x => x.TENDANGNHAP_TK == TenDangNhap);
                if (listTTNV != null && listTTNV.Count() != 0)
                    return true;

                MessageBox.Show("Tài khoản không tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                if (!string.IsNullOrEmpty(MatKhau))
                {
                    var taiKhoan = DataProvider.Ins.model.TAIKHOAN.Where(x => x.MA_TK == SelectedItem.NhanVien.MA_TK).SingleOrDefault();
                    taiKhoan.MATKHAU_TK = MatKhau;
                    DataProvider.Ins.model.SaveChanges();
                }                

                var nhanVien = DataProvider.Ins.model.NHANVIEN.Where(x => x.MA_NV == SelectedItem.NhanVien.MA_NV).SingleOrDefault();
                nhanVien.HOTEN_NV = TenNhanVien;
                nhanVien.GIOITINH_NV = ConvertGioiTinh(SelectedGioiTinh);
                nhanVien.NGAYSINH_NV = NgaySinh;
                nhanVien.SODIENTHOAI_NV = SoDienThoai;
                nhanVien.CHUCVU_NV = SelectedChucVu;
                nhanVien.DIACHI_NV = DiaChi;
                nhanVien.NGAYVAOLAM_NV = NgayVaoLam;
                DataProvider.Ins.model.SaveChanges();

                MessageBox.Show("Sửa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                p.Password = string.Empty;
                LoadTTNhanVien();
                RefershControls();
            });

            RefreshCommand = new RelayCommand<PasswordBox>((p) => { return p == null ? false : true; }, (p) =>
              {
                  p.Password = string.Empty;
                  RefershControls();
              });

            SortNhanVienCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) =>
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListTTNhanVien);
                if (sort)
                {
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription(p.Tag.ToString(), ListSortDirection.Ascending));
                }
                else
                {
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription(p.Tag.ToString(), ListSortDirection.Descending));
                }
                sort = !sort;
            });

        }

        void LoadTTNhanVien()
        {
            ListTTNhanVien = new ObservableCollection<ThongTinNhanVien>();

            var listTTNhanVien = from tk in DataProvider.Ins.model.TAIKHOAN
                                 join nv in DataProvider.Ins.model.NHANVIEN
                                 on tk.MA_TK equals nv.MA_TK
                                 select new ThongTinNhanVien()
                                 {
                                     TaiKhoan = tk,
                                     NhanVien = nv
                                 };
            foreach(ThongTinNhanVien item in listTTNhanVien)
            {
                ListTTNhanVien.Add(item);
            }
        }

        public bool ConvertGioiTinh(string gt)
        {
            if (gt == "Nam") return true;
            else return false;
        }

        void RefershControls()
        {
            TenDangNhap = null;
            MatKhau = null;
            TenNhanVien = null;
            NgaySinh = null;
            SoDienThoai = null;
            SelectedGioiTinh = null;
            SelectedChucVu = null;
            NgayVaoLam = null;
            DiaChi = null;
        }
    }
}
