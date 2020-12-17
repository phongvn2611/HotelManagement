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
    public class MatHangViewModel : BaseViewModel
    {
        private ObservableCollection<ThongTinPhong> _ListTTPhongDangThue;
        public ObservableCollection<ThongTinPhong> ListTTPhongDangThue { get => _ListTTPhongDangThue; set { _ListTTPhongDangThue = value; OnPropertyChanged(); } }
        //Show tất cả mặt hàng
        private ObservableCollection<MATHANG> _ListMatHang;
        public ObservableCollection<MATHANG> ListMatHang { get => _ListMatHang; set { _ListMatHang = value; OnPropertyChanged(); } }
        //Show những mặt hàng mà khách chọn
        private ObservableCollection<ThongTinOrder> _ListOrder;
        public ObservableCollection<ThongTinOrder> ListOrder { get => _ListOrder; set { _ListOrder = value; OnPropertyChanged(); } }
        //Lấy thông tin mặt hàng khi nhân viên muốn quản lý
        private MATHANG _SelectedItemMH;
        public MATHANG SelectedItemMH { get => _SelectedItemMH; set { _SelectedItemMH = value; OnPropertyChanged(); } }
        private MATHANG _SelectedItem;
        public MATHANG SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    TenMatHang = SelectedItem.TEN_MH;
                    DonGia = (int)SelectedItem.DONGIA_MH;
                    NgayNhap = SelectedItem.NGAYNHAP_MH;
                }
            }
        }
        //Lấy thông tin mặt hàng mà khách muốn mua
        private ThongTinOrder _Order;
        public ThongTinOrder Order { get => _Order; set { _Order = value; OnPropertyChanged(); } }
        private ThongTinOrder _SelectedItemOrder;
        public ThongTinOrder SelectedItemOrder
        {
            get => _SelectedItemOrder;
            set
            {
                _SelectedItemOrder = value;
                OnPropertyChanged();
                if (SelectedItemOrder != null)
                {
                    if (Order == null)
                    {
                        Order = new ThongTinOrder();
                    }
                    Order.MatHang = SelectedItemMH;
                    Order.SoLuong = 1;
                    Order.ThanhTien = Order.SoLuong * (int)Order.MatHang.DONGIA_MH;
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
        //
        private long _TongTien;
        public long TongTien { get => _TongTien; set { _TongTien = value; OnPropertyChanged(); } }
        private int _TongSoLuongMHDC;
        public int TongSoLuongMHDC { get => _TongSoLuongMHDC; set { _TongSoLuongMHDC = value; OnPropertyChanged(); } }

        private string _TenMatHang;
        public string TenMatHang { get => _TenMatHang; set { _TenMatHang = value; OnPropertyChanged(); } }
        private int _DonGia;
        public int DonGia { get => _DonGia; set { _DonGia = value; OnPropertyChanged(); } }
        private DateTime? _NgayNhap;
        public DateTime? NgayNhap { get => _NgayNhap; set { _NgayNhap = value; OnPropertyChanged(); } }

        private string _SearchMatHang;
        public string SearchMatHang { get => _SearchMatHang; set { _SearchMatHang = value; OnPropertyChanged(); } }
        public bool sort;

        //Dịch vụ ăn uống
        public ICommand AddOrderCommand { get; set; }
        public ICommand DeleteOrderCommand { get; set; }
        public ICommand ThemSLCommand { get; set; }
        public ICommand BotSLCommand { get; set; }
        public ICommand SortMatHangDVAUCommand { get; set; }
        public ICommand SortMatHangOrderCommand { get; set; }
        public ICommand ShowHDAnUongCommand { get; set; }
        //Tra cứu và quản lý
        public ICommand SearchMatHangCommand { get; set; }
        public ICommand AddMatHangCommand { get; set; }
        public ICommand DeleteMatHangCommand { get; set; }
        public ICommand EditMatHangCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SortMatHangTCQLCommand { get; set; }

        public MatHangViewModel()
        {
            GetTTPhongDangThue();
            ListMatHang = new ObservableCollection<MATHANG>(DataProvider.Ins.model.MATHANG);
            ListOrder = new ObservableCollection<ThongTinOrder>();
            TongTien = 0;
            TongSoLuongMHDC = 0; //tính ra tổng số lượng mặt hàng đã chọn khi order
            sort = false; //sắp xếp column item

            #region Dịch vụ ăn uống
            AddOrderCommand = new RelayCommand<Object>((p) =>
            {
                if (SelectedItemMH == null)
                {
                    MessageBox.Show("Vui lòng chọn mặt hàng muốn mua!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (ListOrder.Count != 0)
                {
                    foreach (ThongTinOrder item in ListOrder)
                    {
                        if (SelectedItemMH.MA_MH == item.MatHang.MA_MH)
                        {
                            MessageBox.Show("Mặt hàng đã được chọn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                }

                return true;
            }, (p) =>
            {
                ThongTinOrder orderMatHang = new ThongTinOrder() { MatHang = SelectedItemMH, SoLuong = 1, ThanhTien = (int)SelectedItemMH.DONGIA_MH };
                ListOrder.Add(orderMatHang);
                TongTien += (int)orderMatHang.MatHang.DONGIA_MH;
                TongSoLuongMHDC++;
            });

            DeleteOrderCommand = new RelayCommand<Object>((p) =>
            {
                if (SelectedItemOrder == null)
                {
                    MessageBox.Show("Vui lòng chọn mặt hàng đã chọn muốn xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                
                return true;
            }, (p) =>
            {
                int i = 0;
                foreach (ThongTinOrder item in ListOrder)
                {
                    if (item.MatHang.MA_MH == SelectedItemOrder.MatHang.MA_MH)
                    {
                        ListOrder.Remove(item);
                        TongTien -= item.ThanhTien;
                        TongSoLuongMHDC -= item.SoLuong;
                        break;
                    }
                    i++;
                }
            });

            ThemSLCommand = new RelayCommand<Object>((p) =>
            {
                if (SelectedItemOrder == null)
                    return false;

                return true;
            }, (p) =>
            {
                foreach (ThongTinOrder item in ListOrder)
                {
                    if (item.MatHang.MA_MH == SelectedItemOrder.MatHang.MA_MH)
                    {
                        item.SoLuong++;
                        item.ThanhTien = item.SoLuong * (int)item.MatHang.DONGIA_MH;
                        TongTien += (int)item.MatHang.DONGIA_MH;
                        TongSoLuongMHDC++;
                        break;
                    }
                }
            });

            BotSLCommand = new RelayCommand<Object>((p) =>
            {
                if (SelectedItemOrder == null)
                    return false;

                foreach (ThongTinOrder item in ListOrder)
                {
                    if (item.MatHang.MA_MH == SelectedItemOrder.MatHang.MA_MH)
                    {
                        if (item.SoLuong == 1)
                            return false;
                    }
                }

                return true;
            }, (p) =>
            {
                foreach (ThongTinOrder item in ListOrder)
                {
                    if (item.MatHang.MA_MH == SelectedItemOrder.MatHang.MA_MH)
                    {
                        item.SoLuong--;
                        item.ThanhTien = item.SoLuong * (int)item.MatHang.DONGIA_MH;
                        TongTien -= (int)item.MatHang.DONGIA_MH;
                        TongSoLuongMHDC--;
                        break;
                    }
                }
            });

            SortMatHangDVAUCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) => {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListMatHang);
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

            SortMatHangOrderCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) => {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListOrder);
                if (sort)
                {
                    view.SortDescriptions.Clear();
                    if(p.Name == "")
                    view.SortDescriptions.Add(new SortDescription(p.Tag.ToString(), ListSortDirection.Ascending));
                }
                else
                {
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription(p.Tag.ToString(), ListSortDirection.Descending));
                }
                sort = !sort;
            });

            ShowHDAnUongCommand = new RelayCommand<Window>((p) =>
            {
                if (p == null || p.DataContext == null)
                    return false;

                if (SelectedPhong == null || ListOrder == null || ListOrder.Count() == 0)
                {
                    MessageBox.Show("Vui lòng chọn phòng và mặt hàng muốn mua!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }                    

                return true;
            }, (p) =>
            {
                HoaDon wd = new HoaDon();
                if (wd.DataContext == null)
                    return;
                var hoadonVM = wd.DataContext as HoaDonViewModel;
                hoadonVM.LoaiHD = (int)HoaDonViewModel.LoaiHoaDon.HoaDonAnUong;
                hoadonVM.HoaDon = hoadonVM.GetHoaDon(MaPhong);
                hoadonVM.NhanVienLapHD = (p.DataContext as MainViewModel).NhanVien;
                hoadonVM.KhachHangThue = hoadonVM.GetKhachHang(hoadonVM.HoaDon);
                hoadonVM.CMND_KH = hoadonVM.KhachHangThue.CMND_KH;
                hoadonVM.GetThongTinPhongThue(MaPhong);

                hoadonVM.TongTienHDAU = TongTien;
                //hoadonVM.LoaiPhucVu = SelectedLoaiPhucVu;
                hoadonVM.ListOrder = ListOrder;                
                wd.ShowDialog();
                RefershControlsDVAU();
            });
            #endregion

            #region Tra cứu & quản lý
            SearchMatHangCommand = new RelayCommand<Object>((p) => { return true; }, (p) => {
                if (string.IsNullOrEmpty(SearchMatHang))
                {
                    CollectionViewSource.GetDefaultView(ListMatHang).Filter = (all) => { return true; };                    
                }
                else
                {
                    CollectionViewSource.GetDefaultView(ListMatHang).Filter = (searchMatHang) =>
                    {
                        return (searchMatHang as MATHANG).TEN_MH.IndexOf(SearchMatHang, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchMatHang as MATHANG).DONGIA_MH.ToString().IndexOf(SearchMatHang, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }

            });

            AddMatHangCommand = new RelayCommand<Object>((p) =>
            {
                if (string.IsNullOrEmpty(TenMatHang) || string.IsNullOrEmpty(DonGia.ToString()) || DonGia == 0)
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin mặt hàng muốn thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }                    

                var listMatHang = DataProvider.Ins.model.MATHANG.Where(x => x.TEN_MH == TenMatHang);
                if (listMatHang == null || listMatHang.Count() != 0)
                {
                    MessageBox.Show("Mặt hàng đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }, (p) =>
            {
                MATHANG matHang = new MATHANG() { TEN_MH = TenMatHang, DONGIA_MH = DonGia, NGAYNHAP_MH = NgayNhap };
                DataProvider.Ins.model.MATHANG.Add(matHang);
                DataProvider.Ins.model.SaveChanges();

                ListMatHang.Add(matHang);

                MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTTMatHang();
                RefershControlsTCQL();
            });

            DeleteMatHangCommand = new RelayCommand<Object>((p) =>
            {
                if (string.IsNullOrEmpty(TenMatHang) || string.IsNullOrEmpty(DonGia.ToString())
                 || DonGia == 0 || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn mặt hàng muốn xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var listMatHang = DataProvider.Ins.model.MATHANG.Where(x => x.TEN_MH == TenMatHang);
                if (listMatHang != null && listMatHang.Count() != 0)
                    return true;

                MessageBox.Show("Mặt hàng không tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                using (var transactions = DataProvider.Ins.model.Database.BeginTransaction())
                {
                    try
                    {
                        var mathang = DataProvider.Ins.model.MATHANG.Where(x => x.MA_MH == SelectedItem.MA_MH).FirstOrDefault();
                        DataProvider.Ins.model.MATHANG.Remove(mathang);
                        DataProvider.Ins.model.SaveChanges();

                        transactions.Commit();
                        RemoveMatHang(mathang.MA_MH);
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTTMatHang();
                        RefershControlsTCQL();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Xóa không thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        transactions.Rollback();
                    }
                }
            });

            EditMatHangCommand = new RelayCommand<Object>((p) =>
            {
                if (string.IsNullOrEmpty(TenMatHang) || string.IsNullOrEmpty(DonGia.ToString())
                 || DonGia == 0 || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn mặt hàng muốn sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var listMatHang = DataProvider.Ins.model.MATHANG.Where(x => x.TEN_MH == TenMatHang);
                if (listMatHang != null && listMatHang.Count() != 0)
                    return true;

                MessageBox.Show("Mặt hàng không tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                var matHang = DataProvider.Ins.model.MATHANG.Where(x => x.MA_MH == SelectedItem.MA_MH).SingleOrDefault();
                matHang.TEN_MH = TenMatHang;
                matHang.DONGIA_MH = DonGia;
                matHang.NGAYNHAP_MH = NgayNhap;
                DataProvider.Ins.model.SaveChanges();

                MessageBox.Show("Sửa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefershControlsTCQL();
            });

            RefreshCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                RefershControlsTCQL();
            });

            SortMatHangTCQLCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) => {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListMatHang);
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
            #endregion
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

        void RemoveMatHang(int mamh)
        {
            if (ListMatHang == null || ListMatHang.Count() == 0)
                return;
            foreach (MATHANG item in ListMatHang)
            {
                if (item.MA_MH == mamh)
                {
                    ListMatHang.Remove(item);
                    return;
                }
            }
        }

        void LoadTTMatHang()
        {
            ListMatHang = new ObservableCollection<MATHANG>(DataProvider.Ins.model.MATHANG);
        }

        void RefershControlsDVAU()
        {
            SelectedPhong = null;
            //SelectedLoaiPhucVu = null;
            ListOrder.Clear();
            SelectedItemOrder = null;
            TongTien = 0;
            TongSoLuongMHDC = 0;

            SelectedItem = null;
        }

        void RefershControlsTCQL()
        {
            DonGia = 0;
            TenMatHang = null;
            NgayNhap = null;
        }
    }
}
