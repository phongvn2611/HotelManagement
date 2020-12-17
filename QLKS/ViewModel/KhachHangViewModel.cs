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
    public class KhachHangViewModel : BaseViewModel
    {
        private ObservableCollection<KHACHHANG> _ListKhachHang;
        public ObservableCollection<KHACHHANG> ListKhachHang { get => _ListKhachHang; set { _ListKhachHang = value; OnPropertyChanged(); } }
        private KHACHHANG _SelectedItem;
        public KHACHHANG SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    TenKhachHang = SelectedItem.HOTEN_KH;
                    SoDienThoai = SelectedItem.SODIENTHOAI_KH;
                    CMND = SelectedItem.CMND_KH;
                }
            }
        }
        private string _TenKhachHang;
        public string TenKhachHang { get => _TenKhachHang; set { _TenKhachHang = value; OnPropertyChanged(); } }
        private string _SoDienThoai;
        public string SoDienThoai { get => _SoDienThoai; set { _SoDienThoai = value; OnPropertyChanged(); } }
        private string _CMND;
        public string CMND { get => _CMND; set { _CMND = value; OnPropertyChanged(); } }
        private string _SearchKhachHang;
        public string SearchKhachHang { get => _SearchKhachHang; set { _SearchKhachHang = value; OnPropertyChanged(); } }
        public bool sort;

        public ICommand SearchKhachHangCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SortKhachHangCommand { get; set; }

        public KhachHangViewModel()
        {
            ListKhachHang = new ObservableCollection<KHACHHANG>(DataProvider.Ins.model.KHACHHANG.ToList());
            
            SearchKhachHangCommand = new RelayCommand<Object>((p) => { return true; }, (p) => {
                if (string.IsNullOrEmpty(SearchKhachHang))
                {
                    CollectionViewSource.GetDefaultView(ListKhachHang).Filter = (all) => { return true; };                    
                }
                else
                {
                    CollectionViewSource.GetDefaultView(ListKhachHang).Filter = (searchKhachHang) =>
                    {
                        return (searchKhachHang as KHACHHANG).HOTEN_KH.IndexOf(SearchKhachHang, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchKhachHang as KHACHHANG).CMND_KH.IndexOf(SearchKhachHang, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }

            });

            AddCommand = new RelayCommand<Object>((p) =>
            {
                if (string.IsNullOrEmpty(TenKhachHang) || string.IsNullOrEmpty(CMND))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin khách hàng muốn thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }                    

                var listKhachHang = DataProvider.Ins.model.KHACHHANG.Where(x => x.CMND_KH == CMND);
                if (listKhachHang == null || listKhachHang.Count() != 0)
                {
                    MessageBox.Show("Khách hàng đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                } 
                

                return true;
            }, (p) =>
            {
                var khachHang = new KHACHHANG() { HOTEN_KH = TenKhachHang, SODIENTHOAI_KH = SoDienThoai, CMND_KH = CMND };
                DataProvider.Ins.model.KHACHHANG.Add(khachHang);
                DataProvider.Ins.model.SaveChanges();

                ListKhachHang.Add(khachHang);

                MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefershControls();
            });

            DeleteCommand = new RelayCommand<Object>((p) =>
            {
                if (string.IsNullOrEmpty(TenKhachHang) || string.IsNullOrEmpty(CMND) || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn khách hàng muốn xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }                    

                var listKhachHang = DataProvider.Ins.model.KHACHHANG.Where(x => x.MA_KH == SelectedItem.MA_KH);
                if (listKhachHang != null && listKhachHang.Count() != 0)
                    return true;

                return false;
            }, (p) =>
            {
                using (var transactions = DataProvider.Ins.model.Database.BeginTransaction())
                {
                    try
                    {
                        var khachHang = DataProvider.Ins.model.KHACHHANG.Where(x => x.MA_KH == SelectedItem.MA_KH).FirstOrDefault();
                        DataProvider.Ins.model.KHACHHANG.Remove(khachHang);
                        DataProvider.Ins.model.SaveChanges();

                        transactions.Commit();
                        RemoveKhachHang(khachHang.MA_KH);
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefershControls();
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
                if (string.IsNullOrEmpty(TenKhachHang) || string.IsNullOrEmpty(CMND) || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn khách hàng muốn sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }                    

                var listKhachHang = DataProvider.Ins.model.KHACHHANG.Where(x => x.MA_KH == SelectedItem.MA_KH);
                if (listKhachHang != null && listKhachHang.Count() != 0)
                    return true;

                return false;
            }, (p) =>
            {
                var khachHang = DataProvider.Ins.model.KHACHHANG.Where(x => x.MA_KH == SelectedItem.MA_KH).SingleOrDefault();
                khachHang.HOTEN_KH = TenKhachHang;
                khachHang.SODIENTHOAI_KH = SoDienThoai;
                khachHang.CMND_KH = CMND;
                DataProvider.Ins.model.SaveChanges();

                MessageBox.Show("Sửa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefershControls();
            });

            RefreshCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                RefershControls();
            });

            SortKhachHangCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) =>
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListKhachHang);
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

        void RemoveKhachHang(int makh)
        {
            if (ListKhachHang == null || ListKhachHang.Count() == 0)
                return;
            foreach (KHACHHANG item in ListKhachHang)
            {
                if (item.MA_KH == makh)
                {
                    ListKhachHang.Remove(item);
                    return;
                }
            }
        }

        void RefershControls()
        {
            TenKhachHang = null;
            SoDienThoai = null;
            CMND = null;
        }
    }
}
