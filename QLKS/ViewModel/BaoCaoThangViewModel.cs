using QLKS.Model;
using SAPBusinessObjects.WPF.Viewer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLKS.ViewModel
{
    class BaoCaoThangViewModel : BaseViewModel
    {
        private ObservableCollection<ThongTinBaoCao> _ListDoanhThuNgay;
        public ObservableCollection<ThongTinBaoCao> ListDoanhThuNgay { get => _ListDoanhThuNgay; set { _ListDoanhThuNgay = value; OnPropertyChanged(); } }
        private NHANVIEN _NhanVien;
        public NHANVIEN NhanVien { get => _NhanVien; set { _NhanVien = value; OnPropertyChanged(); } }

        private int _Nam;
        public int Nam { get => _Nam; set { _Nam = value; OnPropertyChanged(); } }
        private int _SelectedThang;
        public int SelectedThang { get => _SelectedThang; set { _SelectedThang = value; OnPropertyChanged(); } }
        private int _TongDoanhThu;
        public int TongDoanhThu { get => _TongDoanhThu; set { _TongDoanhThu = value; OnPropertyChanged(); } }
        private string _TieuDeBieuDo;
        public string TieuDeBieuDo { get => _TieuDeBieuDo; set { _TieuDeBieuDo = value; OnPropertyChanged(); } }
        private ObservableCollection<int> _ListNgay;
        public ObservableCollection<int> ListNgay { get => _ListNgay; set { _ListNgay = value; OnPropertyChanged(); } }
        public int[] ListThang { get; set; }
        public ICommand ShowCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand LoadReportCommand { get; set; }
        public BaoCaoThangViewModel()
        {
            ListThang = new int[12] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            int[] ngays = new int[31];
            ListNgay = new ObservableCollection<int>(ngays);
            Nam = DateTime.Now.Year;
            SelectedThang = DateTime.Now.Month;

            ShowCommand = new RelayCommand<Object>((p) =>
              {
                  if (string.IsNullOrEmpty(Nam.ToString()) || string.IsNullOrEmpty(SelectedThang.ToString()))
                  {
                      MessageBox.Show("Vui lòng nhập năm và chọn tháng cần lập báo cáo!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                      return false;
                  }
                  return true;
              }, (p) =>
              {
                  TongDoanhThu = 0;
                  TieuDeBieuDo = string.Empty;
                  ListDoanhThuNgay = null;

                  var tong = (from hd in DataProvider.Ins.model.HOADON
                              where (hd.THOIGIANLAP_HD.Value.Year == Nam) && (hd.THOIGIANLAP_HD.Value.Month == SelectedThang) && (hd.TINHTRANG_HD == true)
                              select hd.TRIGIA_HD).Sum();
                  if (tong == null)
                  {
                      ListDoanhThuNgay = null;
                      MessageBox.Show("Không có báo cáo trong thời gian đã chọn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                      return;
                  }
                  TongDoanhThu = (int)tong;
                  TieuDeBieuDo = "Tổng doanh thu: " + TongDoanhThu.ToString("N0");
                  ListDoanhThuNgay = new ObservableCollection<ThongTinBaoCao>();
                  for (int i = 1; i <= ListNgay.Count(); i++)
                  {
                      var ngay = (from hd in DataProvider.Ins.model.HOADON
                                  where (hd.THOIGIANLAP_HD.Value.Year == Nam) && (hd.THOIGIANLAP_HD.Value.Month == SelectedThang) && (hd.THOIGIANLAP_HD.Value.Day == i) && (hd.TINHTRANG_HD == true)
                                  select hd.TRIGIA_HD).Sum();
                      if (ngay == null)
                      {
                          ListNgay[i - 1] = 0;
                      }
                      else
                      {
                          ListNgay[i - 1] = (int)ngay;
                      }
                      ListDoanhThuNgay.Add(new ThongTinBaoCao() { Item = "Ngày " + i, DoanhThu = ListNgay[i - 1], TiLe = (double)ListNgay[i - 1] / TongDoanhThu });
                  }
              });

            PrintCommand = new RelayCommand<Window>((p) =>
            {
                if (p == null || p.DataContext == null)
                    return false;

                if (ListDoanhThuNgay != null)
                    return true;
                MessageBox.Show("Không có báo cáo, vui lòng xuất báo cáo trước khi in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                var MainVM = p.DataContext as MainViewModel;
                NhanVien = MainVM.NhanVien;
                ThangReport rp = new ThangReport();
                rp.ShowDialog();
            });

            LoadReportCommand = new RelayCommand<CrystalReportsViewer>((p) =>
            {
                if (p == null)
                    return false;
                return true;
            }, (p) =>
            {
                DoanhThuThangReport rp = new DoanhThuThangReport();
                rp.SetDataSource(ListDoanhThuNgay);
                rp.SetParameterValue("txtNam", Nam);
                rp.SetParameterValue("txtThang", SelectedThang);
                rp.SetParameterValue("txtTongDoanhThu", TongDoanhThu);
                rp.SetParameterValue("txtNhanVien", NhanVien.HOTEN_NV);
                p.ViewerCore.ReportSource = rp;
            });
        }
    }
}
