using QLKS.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace QLKS.ViewModel
{
    public class DiChuyenViewModel : BaseViewModel
    {
        private ObservableCollection<ThongTinPhong> _ListTTPhongDangThue;
        public ObservableCollection<ThongTinPhong> ListTTPhongDangThue { get => _ListTTPhongDangThue; set { _ListTTPhongDangThue = value; OnPropertyChanged(); } }

        private ObservableCollection<CHUYENDI> _ListChuyenDi;
        public ObservableCollection<CHUYENDI> ListChuyenDi { get => _ListChuyenDi; set { _ListChuyenDi = value; OnPropertyChanged(); } }
        private CHUYENDI _SelectedItem;
        public CHUYENDI SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    DonGia = (int)SelectedItem.DONGIA_CD;
                    DiemDen = SelectedItem.DIEMDEN_CD;
                }
            }
        }
        private string _DiemDen;
        public string DiemDen { get => _DiemDen; set { _DiemDen = value; OnPropertyChanged(); } }
        private int _DonGia;
        public int DonGia { get => _DonGia; set { _DonGia = value; OnPropertyChanged(); } }
        private string _SearchChuyenDi;
        public string SearchChuyenDi { get => _SearchChuyenDi; set { _SearchChuyenDi = value; OnPropertyChanged(); } }
        public bool sort;
        //Chọn phòng thực hiện sử dụng dịch vụ ăn uống
        private int _MaPhong;
        public int MaPhong { get => _MaPhong; set { _MaPhong = value; OnPropertyChanged(); } }
        private ThongTinPhong _SelectedPhong;
        public ThongTinPhong SelectedPhong { get => _SelectedPhong; set { _SelectedPhong = value; OnPropertyChanged(); if (_SelectedPhong != null) MaPhong = SelectedPhong.Phong.MA_PHONG; } }
        private KHACHHANG _KhachHangThue;
        public KHACHHANG KhachHangThue { get => _KhachHangThue; set { _KhachHangThue = value; OnPropertyChanged(); } }
        private NHANVIEN _NhanVienLapHD;
        public NHANVIEN NhanVienLapHD { get => _NhanVienLapHD; set { _NhanVienLapHD = value; OnPropertyChanged(); } }

        public ICommand ShowHDDiChuyenCommand { get; set; }
        public ICommand SearchChuyenDiCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SortChuyenDiCommand { get; set; }

        public DiChuyenViewModel()
        {
            ListChuyenDi = new ObservableCollection<CHUYENDI>(DataProvider.Ins.model.CHUYENDI);
            DonGia = 0;

            ShowHDDiChuyenCommand = new RelayCommand<Window>((p) =>
            {
                if (p == null || p.DataContext == null)
                    return false;

                if (DonGia == 0 || SelectedPhong == null || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn phòng và chuyến đi!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }, (p) =>
            {              
                HoaDon wd = new HoaDon();
                if (wd.DataContext == null)
                    return;
                var hoadonVM = wd.DataContext as HoaDonViewModel;
                hoadonVM.LoaiHD = (int)HoaDonViewModel.LoaiHoaDon.HoaDonDiChuyen;
                hoadonVM.HoaDon = hoadonVM.GetHoaDon(MaPhong);
                hoadonVM.NhanVienLapHD = (p.DataContext as MainViewModel).NhanVien;
                hoadonVM.KhachHangThue = hoadonVM.GetKhachHang(hoadonVM.HoaDon);
                hoadonVM.CMND_KH = hoadonVM.KhachHangThue.CMND_KH;
                hoadonVM.GetThongTinPhongThue(MaPhong);

                hoadonVM.TongTienHDDC = (long)SelectedItem.DONGIA_CD;
                hoadonVM.ChuyenDi = SelectedItem;
                wd.ShowDialog();
                RefershControlsDVDC();
            });

            SearchChuyenDiCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                if (string.IsNullOrEmpty(SearchChuyenDi))
                {
                    CollectionViewSource.GetDefaultView(ListChuyenDi).Filter = (all) => { return true; };
                }
                else
                {
                    CollectionViewSource.GetDefaultView(ListChuyenDi).Filter = (searchChuyenDi) =>
                    {
                        return (searchChuyenDi as CHUYENDI).DIEMDEN_CD.IndexOf(SearchChuyenDi, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchChuyenDi as CHUYENDI).DONGIA_CD.ToString().IndexOf(SearchChuyenDi, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }
            });

            AddCommand = new RelayCommand<Object>((p) =>
            {
                if (String.IsNullOrEmpty(DiemDen) || String.IsNullOrEmpty(DonGia.ToString()) || DonGia == 0)
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin chuyến đi muốn thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var cd = DataProvider.Ins.model.CHUYENDI.Where(x => x.DIEMDEN_CD == DiemDen);
                if (cd == null || cd.Count() != 0)
                {
                    MessageBox.Show("Chuyến đi đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }, (p) =>
            {
                var cd = new CHUYENDI() { DIEMDEN_CD = DiemDen, DONGIA_CD = DonGia };
                DataProvider.Ins.model.CHUYENDI.Add(cd);
                DataProvider.Ins.model.SaveChanges();

                ListChuyenDi.Add(cd);

                MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefershControlsTCQL();
            });

            DeleteCommand = new RelayCommand<Object>((p) =>
            {
                if (String.IsNullOrEmpty(DiemDen) || String.IsNullOrEmpty(DonGia.ToString())
                 || DonGia == 0 || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn chuyến đi muốn xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var cd = DataProvider.Ins.model.CHUYENDI.Where(x => x.DIEMDEN_CD == DiemDen);
                if (cd != null && cd.Count() != 0)
                    return true;

                MessageBox.Show("Chuyến đi không tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                using (var transactions = DataProvider.Ins.model.Database.BeginTransaction())
                {
                    try
                    {
                        var chuyendi = DataProvider.Ins.model.CHUYENDI.Where(x => x.MA_CD == SelectedItem.MA_CD).FirstOrDefault();
                        DataProvider.Ins.model.CHUYENDI.Remove(chuyendi);
                        DataProvider.Ins.model.SaveChanges();

                        transactions.Commit();
                        RemoveChuyenDi(chuyendi.MA_CD);
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefershControlsTCQL();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Xóa không thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        transactions.Rollback();
                    }
                }
            });

            EditCommand = new RelayCommand<Object>((p) =>
            {
                if (String.IsNullOrEmpty(DiemDen) || String.IsNullOrEmpty(DonGia.ToString())
                 || DonGia == 0 || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn chuyến đi muốn sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var cd = DataProvider.Ins.model.CHUYENDI.Where(x => x.DIEMDEN_CD == DiemDen);
                if (cd != null && cd.Count() != 0)
                    return true;

                MessageBox.Show("Chuyến đi không tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                var cd = DataProvider.Ins.model.CHUYENDI.Where(x => x.DIEMDEN_CD == DiemDen).SingleOrDefault();
                cd.DIEMDEN_CD = DiemDen;
                cd.DONGIA_CD = DonGia;
                DataProvider.Ins.model.SaveChanges();

                MessageBox.Show("Sửa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefershControlsTCQL();
            });

            RefreshCommand = new RelayCommand<Object>((p) =>{ return true; }, (p) => 
            {
                RefershControlsTCQL();
            });

            SortChuyenDiCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) =>
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListChuyenDi);
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

        void RemoveChuyenDi(int mach)
        {
            if (ListChuyenDi == null || ListChuyenDi.Count() == 0)
                return;
            foreach (CHUYENDI item in ListChuyenDi)
            {
                if (item.MA_CD == mach)
                {
                    ListChuyenDi.Remove(item);
                    return;
                }
            }
        }

        void RefershControlsDVDC()
        {
            SelectedPhong = null;
            SelectedItem = null;
            DonGia = 0;
            DiemDen = null;
        }

        void RefershControlsTCQL()
        {
            DiemDen = null;
            DonGia = 0;
        }
    }
}
