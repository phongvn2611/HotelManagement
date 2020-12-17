using QLKS.Model;
using SAPBusinessObjects.WPF.Viewer;
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
    class BaoCaoDichVuViewModel : BaseViewModel
    {
        private ObservableCollection<ThongTinBaoCao> _ListDichVu;
        public ObservableCollection<ThongTinBaoCao> ListDichVu { get => _ListDichVu; set { _ListDichVu = value; OnPropertyChanged(); } }
        private NHANVIEN _NhanVien;
        public NHANVIEN NhanVien { get => _NhanVien; set { _NhanVien = value; OnPropertyChanged(); } }
        private int _TongDoanhThu;
        public int TongDoanhThu { get => _TongDoanhThu; set { _TongDoanhThu = value; OnPropertyChanged(); } }
        private string _TieuDeBieuDo;
        public string TieuDeBieuDo { get => _TieuDeBieuDo; set { _TieuDeBieuDo = value; OnPropertyChanged(); } }
        private int _LuuTru;
        public int LuuTru { get => _LuuTru; set { _LuuTru = value; OnPropertyChanged(); } }
        private int _AnUong;
        public int AnUong { get => _AnUong; set { _AnUong = value; OnPropertyChanged(); } }
        private int _GiatUi;
        public int GiatUi { get => _GiatUi; set { _GiatUi = value; OnPropertyChanged(); } }
        private int _DiChuyen;
        public int DiChuyen { get => _DiChuyen; set { _DiChuyen = value; OnPropertyChanged(); } }
        private DateTime _NgayBatDau;
        public DateTime NgayBatDau { get => _NgayBatDau; set { _NgayBatDau = value; OnPropertyChanged(); } }
        private DateTime _NgayKetThuc;
        public DateTime NgayKetThuc { get => _NgayKetThuc; set { _NgayKetThuc = value; OnPropertyChanged(); } }
        private DateTime _NgayKetThucReal;
        public DateTime NgayKetThucReal { get => _NgayKetThucReal; set { _NgayKetThucReal = value; OnPropertyChanged(); } }

        //Tra cứu báo cáo dịch vụ
        private ObservableCollection<BAOCAODICHVU> _ListBaoCaoDichVu;
        public ObservableCollection<BAOCAODICHVU> ListBaoCaoDichVu { get => _ListBaoCaoDichVu; set { _ListBaoCaoDichVu = value; OnPropertyChanged(); } }
        private string _SearchBaoCaoDichVu;
        public string SearchBaoCaoDichVu { get => _SearchBaoCaoDichVu; set { _SearchBaoCaoDichVu = value; OnPropertyChanged(); } }
        public bool sort;
        public ICommand SearchBaoCaoDichVuCommand { get; set; }
        public ICommand SortBaoCaoDichVuCommand { get; set; }

        public ICommand ShowCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand LoadReportCommand { get; set; }

        public BaoCaoDichVuViewModel()
        {
            NgayBatDau = DateTime.Now;
            NgayKetThuc = DateTime.Now;
            ListBaoCaoDichVu = new ObservableCollection<BAOCAODICHVU>(DataProvider.Ins.model.BAOCAODICHVU);

            SearchBaoCaoDichVuCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                if (string.IsNullOrEmpty(SearchBaoCaoDichVu))
                {
                    CollectionViewSource.GetDefaultView(ListBaoCaoDichVu).Filter = (all) => { return true; };
                }
                else
                {
                    CollectionViewSource.GetDefaultView(ListBaoCaoDichVu).Filter = (searchBaoCaoDichVu) =>
                    {
                        return (searchBaoCaoDichVu as BAOCAODICHVU).MA_BCDV.ToString().IndexOf(SearchBaoCaoDichVu, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchBaoCaoDichVu as BAOCAODICHVU).THOIGIANLAP_BCDV.Value.Year.ToString().IndexOf(SearchBaoCaoDichVu, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchBaoCaoDichVu as BAOCAODICHVU).THOIGIANLAP_BCDV.Value.Month.ToString().IndexOf(SearchBaoCaoDichVu, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchBaoCaoDichVu as BAOCAODICHVU).THOIGIANLAP_BCDV.Value.Day.ToString().IndexOf(SearchBaoCaoDichVu, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }

            });

            SortBaoCaoDichVuCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) =>
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListBaoCaoDichVu);
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

            ShowCommand = new RelayCommand<Object>((p) =>
            {
                if (NgayBatDau == null || NgayKetThuc == null)
                {
                    MessageBox.Show("Vui lòng chọn đầy đủ ngày bắt đầu và ngày kết thúc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (NgayBatDau > NgayKetThuc)
                {
                    MessageBox.Show("Ngày kết thúc phải sau ngày bắt đầu, vui lòng chọn lại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                return true;
            }, (p) =>
            {
                NgayKetThucReal = NgayKetThuc.AddDays(1);
                TongDoanhThu = 0;
                TieuDeBieuDo = string.Empty;
                ListDichVu = new ObservableCollection<ThongTinBaoCao>();

                var tong = (from hd in DataProvider.Ins.model.HOADON
                            where (hd.THOIGIANLAP_HD >= NgayBatDau) && (hd.THOIGIANLAP_HD < NgayKetThucReal) && (hd.TINHTRANG_HD == true)
                            select hd.TRIGIA_HD).Sum();
                if (tong == null)
                {
                    ListDichVu = null;
                    MessageBox.Show("Không có báo cáo trong khoảng thời gian đã chọn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                TongDoanhThu = (int)tong;
                TieuDeBieuDo = "Tổng doanh thu: " + TongDoanhThu.ToString("N0");

                #region Tính doanh thu từng loại dịch vụ
                var anuong = (from hd in DataProvider.Ins.model.HOADON
                              join au in DataProvider.Ins.model.CHITIET_HDAU
                              on hd.MA_HD equals au.MA_HD
                              where (hd.THOIGIANLAP_HD >= NgayBatDau) && (hd.THOIGIANLAP_HD < NgayKetThucReal) && (hd.TINHTRANG_HD == true)
                              select au.TRIGIA_CTHDAU).Sum();
                if (anuong == null)
                {
                    AnUong = 0;
                }
                else
                {
                    AnUong = (int)anuong;
                }

                var luutru = (from hd in DataProvider.Ins.model.HOADON
                              join lt in DataProvider.Ins.model.CHITIET_HDLT
                              on hd.MA_HD equals lt.MA_HD
                              where (hd.THOIGIANLAP_HD >= NgayBatDau) && (hd.THOIGIANLAP_HD < NgayKetThucReal) && (hd.TINHTRANG_HD == true)
                              select lt.TRIGIA_CTHDLT).Sum();
                if (luutru == null)
                {
                    LuuTru = 0;
                }
                else
                {
                    LuuTru = (int)luutru;
                }

                var dichuyen = (from hd in DataProvider.Ins.model.HOADON
                                join dc in DataProvider.Ins.model.CHITIET_HDDC
                                on hd.MA_HD equals dc.MA_HD
                                where (hd.THOIGIANLAP_HD >= NgayBatDau) && (hd.THOIGIANLAP_HD < NgayKetThucReal) && (hd.TINHTRANG_HD == true)
                                select dc.TRIGIA_CTHDDC).Sum();
                if (dichuyen == null)
                {
                    DiChuyen = 0;
                }
                else
                {
                    DiChuyen = (int)dichuyen;
                }

                var giatui = (from hd in DataProvider.Ins.model.HOADON
                              join gu in DataProvider.Ins.model.CHITIET_HDGU
                              on hd.MA_HD equals gu.MA_HD
                              where (hd.THOIGIANLAP_HD >= NgayBatDau) && (hd.THOIGIANLAP_HD < NgayKetThucReal) && (hd.TINHTRANG_HD == true)
                              select gu.TRIGIA_CTHDGU).Sum();
                if (giatui == null)
                {
                    GiatUi = 0;
                }
                else
                {
                    GiatUi = (int)giatui;
                }
                #endregion

                ListDichVu.Add(new ThongTinBaoCao() { Item = "Lưu trú", DoanhThu = LuuTru, TiLe = (double)LuuTru / TongDoanhThu });
                ListDichVu.Add(new ThongTinBaoCao() { Item = "Ăn uống", DoanhThu = AnUong, TiLe = (double)AnUong / TongDoanhThu });
                ListDichVu.Add(new ThongTinBaoCao() { Item = "Giặt ủi", DoanhThu = GiatUi, TiLe = (double)GiatUi / TongDoanhThu });
                ListDichVu.Add(new ThongTinBaoCao() { Item = "Di chuyển", DoanhThu = DiChuyen, TiLe = (double)DiChuyen / TongDoanhThu });
            });

            SaveCommand = new RelayCommand<Object>((p) =>
            {
                if (ListDichVu != null)
                    return true;
                MessageBox.Show("Không có báo cáo, vui lòng xuất báo cáo trước khi lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                var baocao = new BAOCAODICHVU();
                baocao.TONGDOANHTHU_BCDV = TongDoanhThu;
                baocao.DOANHTHULUUTRU_BCDV = LuuTru;
                baocao.DOANHTHUANUONG_BCDV = AnUong;
                baocao.DOANHTHUGIATUI_BCDV = GiatUi;
                baocao.DOANHTHUDICHUYEN_BCDV = DiChuyen;
                baocao.NGAYBATDAU_BCDV = NgayBatDau;
                baocao.NGAYKETTHUC_BCDV = NgayKetThuc;
                baocao.THOIGIANLAP_BCDV = DateTime.Now;
                DataProvider.Ins.model.BAOCAODICHVU.Add(baocao);
                DataProvider.Ins.model.SaveChanges();
                MessageBox.Show("Lưu báo cáo thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            PrintCommand = new RelayCommand<Window>((p) =>
              {
                  if (p == null || p.DataContext == null)
                      return false;

                  if (ListDichVu != null)
                      return true;
                  MessageBox.Show("Không có báo cáo, vui lòng xuất báo cáo trước khi in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                  return false;
              }, (p) =>
              {
                  var MainVM = p.DataContext as MainViewModel;
                  NhanVien = MainVM.NhanVien;
                  DichVuReport rp = new DichVuReport();
                  rp.ShowDialog();
              });

            LoadReportCommand = new RelayCommand<CrystalReportsViewer>((p) =>
            {
                if (p == null)
                    return false;
                return true;
            }, (p) =>
            {
                DoanhThuDichVuReport rp = new DoanhThuDichVuReport();
                rp.SetDataSource(ListDichVu);
                rp.SetParameterValue("txtNgayBatDau", NgayBatDau);
                rp.SetParameterValue("txtNgayKetThuc", NgayKetThuc);
                rp.SetParameterValue("txtTongDoanhThu", TongDoanhThu);
                rp.SetParameterValue("txtNhanVien", NhanVien.HOTEN_NV);
                p.ViewerCore.ReportSource = rp;
            });
        }
    }
}
