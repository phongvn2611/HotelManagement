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
    public class LoaiPhongViewModel : BaseViewModel
    {
        private ObservableCollection<LOAIPHONG> _ListLoaiPhong;
        public ObservableCollection<LOAIPHONG> ListLoaiPhong { get => _ListLoaiPhong; set { _ListLoaiPhong = value; OnPropertyChanged(); } }
        private LOAIPHONG _SelectedItem;
        public LOAIPHONG SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    TenLoaiPhong = SelectedItem.TEN_LP;
                    DonGia = (int)SelectedItem.DONGIA_LP;
                }
            }
        }
        private string _TenLoaiPhong;
        public string TenLoaiPhong { get => _TenLoaiPhong; set { _TenLoaiPhong = value; OnPropertyChanged(); } }
        private int _DonGia;
        public int DonGia { get => _DonGia; set { _DonGia = value; OnPropertyChanged(); } }
        private string _SearchLoaiPhong;
        public string SearchLoaiPhong { get => _SearchLoaiPhong; set { _SearchLoaiPhong = value; OnPropertyChanged(); } }
        public bool sort;

        public ICommand SearchLoaiPhongCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SortLoaiPhongCommand { get; set; }

        public LoaiPhongViewModel()
        {
            ListLoaiPhong = new ObservableCollection<LOAIPHONG>(DataProvider.Ins.model.LOAIPHONG);

            SearchLoaiPhongCommand = new RelayCommand<Object>((p) => { return true; }, (p) => {
                if (string.IsNullOrEmpty(SearchLoaiPhong))
                {
                    CollectionViewSource.GetDefaultView(ListLoaiPhong).Filter = (all) => { return true; };                    
                }
                else
                {
                    CollectionViewSource.GetDefaultView(ListLoaiPhong).Filter = (searchLoaiPhong) =>
                    {
                        return (searchLoaiPhong as LOAIPHONG).TEN_LP.IndexOf(SearchLoaiPhong, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchLoaiPhong as LOAIPHONG).DONGIA_LP.ToString().IndexOf(SearchLoaiPhong, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }

            });

            AddCommand = new RelayCommand<Object>((p) => 
            {
                if (string.IsNullOrEmpty(TenLoaiPhong) || string.IsNullOrEmpty(DonGia.ToString()) || DonGia == 0)
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin loại phòng muốn thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }                    

                var listLoaiPhong = DataProvider.Ins.model.LOAIPHONG.Where(x => x.TEN_LP == TenLoaiPhong);
                if (listLoaiPhong == null || listLoaiPhong.Count() != 0)
                {
                    MessageBox.Show("Loại phòng đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }, (p) => {
                var loaiPhong = new LOAIPHONG() { TEN_LP = TenLoaiPhong, DONGIA_LP = DonGia };
                DataProvider.Ins.model.LOAIPHONG.Add(loaiPhong);
                DataProvider.Ins.model.SaveChanges();

                ListLoaiPhong.Add(loaiPhong);

                MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTTLoaiPhong();
                RefershControls();
            });

            DeleteCommand = new RelayCommand<Object>((p) =>
            {
                if (string.IsNullOrEmpty(TenLoaiPhong) || string.IsNullOrEmpty(DonGia.ToString())
                 || DonGia == 0 || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn loại phòng muốn xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var listLoaiPhong = DataProvider.Ins.model.LOAIPHONG.Where(x => x.MA_LP == SelectedItem.MA_LP);
                if (listLoaiPhong != null && listLoaiPhong.Count() != 0)
                    return true;

                return false;
            }, (p) =>
            {
                using (var transactions = DataProvider.Ins.model.Database.BeginTransaction())
                {
                    try
                    {
                        var loaiphong = DataProvider.Ins.model.LOAIPHONG.Where(x => x.MA_LP == SelectedItem.MA_LP).FirstOrDefault();
                        DataProvider.Ins.model.LOAIPHONG.Remove(loaiphong);
                        DataProvider.Ins.model.SaveChanges();

                        transactions.Commit();
                        RemoveLoaiPhong(loaiphong.MA_LP);
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
                if (string.IsNullOrEmpty(TenLoaiPhong) || string.IsNullOrEmpty(DonGia.ToString())
                 || DonGia == 0 || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn loại phòng muốn sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var listLoaiPhong = DataProvider.Ins.model.LOAIPHONG.Where(x => x.MA_LP == SelectedItem.MA_LP);
                if (listLoaiPhong != null && listLoaiPhong.Count() != 0)
                    return true;

                return false;
            }, (p) => {
                var loaiPhong = DataProvider.Ins.model.LOAIPHONG.Where(x => x.MA_LP == SelectedItem.MA_LP).SingleOrDefault();
                loaiPhong.TEN_LP = TenLoaiPhong;
                loaiPhong.DONGIA_LP = DonGia;
                DataProvider.Ins.model.SaveChanges();

                MessageBox.Show("Sửa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTTLoaiPhong();
                RefershControls();
            });

            RefreshCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                RefershControls();
            });

            SortLoaiPhongCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) =>
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListLoaiPhong);
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

        void RemoveLoaiPhong(int malp)
        {
            if (ListLoaiPhong == null || ListLoaiPhong.Count() == 0)
                return;
            foreach (LOAIPHONG item in ListLoaiPhong)
            {
                if (item.MA_LP == malp)
                {
                    ListLoaiPhong.Remove(item);
                    return;
                }
            }
        }

        void LoadTTLoaiPhong()
        {
            ListLoaiPhong = new ObservableCollection<LOAIPHONG>(DataProvider.Ins.model.LOAIPHONG);  
        }

        void RefershControls()
        {
            TenLoaiPhong = null;
            DonGia = 0;
        }
    }
}
