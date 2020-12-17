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
    class BaoCaoKhachHangViewModel : BaseViewModel
    {
        private ObservableCollection<ThongTinBaoCao> _ListDoanhThuKhachHang;
        public ObservableCollection<ThongTinBaoCao> ListDoanhThuKhachHang { get => _ListDoanhThuKhachHang; set { _ListDoanhThuKhachHang = value; OnPropertyChanged(); } }
        private NHANVIEN _NhanVien;
        public NHANVIEN NhanVien { get => _NhanVien; set { _NhanVien = value; OnPropertyChanged(); } }
        private int _TongDoanhThu;
        public int TongDoanhThu { get => _TongDoanhThu; set { _TongDoanhThu = value; OnPropertyChanged(); } }
        private string _TieuDeBieuDo;
        public string TieuDeBieuDo { get => _TieuDeBieuDo; set { _TieuDeBieuDo = value; OnPropertyChanged(); } }
        private DateTime _NgayBatDau;
        public DateTime NgayBatDau { get => _NgayBatDau; set { _NgayBatDau = value; OnPropertyChanged(); } }
        private DateTime _NgayKetThuc;
        public DateTime NgayKetThuc { get => _NgayKetThuc; set { _NgayKetThuc = value; OnPropertyChanged(); } }
        private DateTime _NgayKetThucReal;
        public DateTime NgayKetThucReal { get => _NgayKetThucReal; set { _NgayKetThucReal = value; OnPropertyChanged(); } }
        

        public ICommand ShowCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand LoadReportCommand { get; set; }

        public BaoCaoKhachHangViewModel()
        {
            NgayBatDau = DateTime.Now;
            NgayKetThuc = DateTime.Now;

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
                ListDoanhThuKhachHang = new ObservableCollection<ThongTinBaoCao>();

                var tong = (from hd in DataProvider.Ins.model.HOADON
                            where (hd.THOIGIANLAP_HD >= NgayBatDau) && (hd.THOIGIANLAP_HD < NgayKetThucReal) && (hd.TINHTRANG_HD == true)
                            select hd.TRIGIA_HD).Sum();
                if (tong == null)
                {
                    ListDoanhThuKhachHang = null;
                    MessageBox.Show("Không có báo cáo trong khoảng thời gian đã chọn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                TongDoanhThu = (int)tong;
                TieuDeBieuDo = "Tổng doanh thu: " + TongDoanhThu.ToString("N0");
                
                foreach(var kh in DataProvider.Ins.model.KHACHHANG)
                {
                    var item = (from hd in DataProvider.Ins.model.HOADON                                
                                where (hd.THOIGIANLAP_HD >= NgayBatDau) && (hd.THOIGIANLAP_HD < NgayKetThucReal) && (hd.TINHTRANG_HD == true) && (hd.MA_KH==kh.MA_KH)
                                select hd.TRIGIA_HD).Sum();
                    if (item != null)
                    {
                        ListDoanhThuKhachHang.Add(new ThongTinBaoCao() { Item = kh.HOTEN_KH, DoanhThu = (int)item, TiLe = (double)item / TongDoanhThu });
                    }
                   
                }
            });

            LoadReportCommand = new RelayCommand<CrystalReportsViewer>((p) =>
            {
                if (p == null)
                    return false;
                return true;
            }, (p) =>
            {
                DoanhThuKhachHangReport rp = new DoanhThuKhachHangReport();
                rp.SetDataSource(ListDoanhThuKhachHang);
                rp.SetParameterValue("txtNgayBatDau", NgayBatDau);
                rp.SetParameterValue("txtNgayKetThuc", NgayKetThuc);
                rp.SetParameterValue("txtTongDoanhThu", TongDoanhThu);
                rp.SetParameterValue("txtNhanVien", NhanVien.HOTEN_NV);
                p.ViewerCore.ReportSource = rp;
            });

            PrintCommand = new RelayCommand<Window>((p) =>
            {
                if (p == null || p.DataContext == null)
                    return false;

                if (ListDoanhThuKhachHang != null)
                    return true;
                MessageBox.Show("Không có báo cáo, vui lòng xuất báo cáo trước khi in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                var MainVM = p.DataContext as MainViewModel;
                NhanVien = MainVM.NhanVien;
                KhachHangReport rp = new KhachHangReport();
                rp.ShowDialog();
            });
        }
    }
}
