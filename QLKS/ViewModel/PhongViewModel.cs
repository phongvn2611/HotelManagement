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
using QLKS.Model;

namespace QLKS.ViewModel
{
    public class PhongViewModel : BaseViewModel
    {
        //Datacontext
        private ObservableCollection<ThongTinPhong> _ListTTPhong;
        public ObservableCollection<ThongTinPhong> ListTTPhong { get => _ListTTPhong; set { _ListTTPhong = value; OnPropertyChanged(); } }
        //ItemsSource combobox
        private ObservableCollection<LOAIPHONG> _ListLoaiPhong;
        public ObservableCollection<LOAIPHONG> ListLoaiPhong { get => _ListLoaiPhong; set { _ListLoaiPhong = value; OnPropertyChanged(); } }
        private ObservableCollection<string> _ListTinhTrangPhong;
        public ObservableCollection<string> ListTinhTrangPhong { get => _ListTinhTrangPhong; set { _ListTinhTrangPhong = value; OnPropertyChanged(); } }

        private ThongTinPhong _SelectedItem;
        public ThongTinPhong SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    MaPhong = SelectedItem.Phong.MA_PHONG;
                    SelectedLoaiPhong = SelectedItem.LoaiPhong;
                    SelectedTinhTrangPhong = SelectedItem.Phong.TINHTRANG_PHONG;
                }
            }
        }
        private int _MaPhong;
        public int MaPhong { get => _MaPhong; set { _MaPhong = value; OnPropertyChanged(); } }
        private LOAIPHONG _SelectedLoaiPhong;
        public LOAIPHONG SelectedLoaiPhong { get => _SelectedLoaiPhong; set { _SelectedLoaiPhong = value; OnPropertyChanged(); } }
        private string _SelectedTinhTrangPhong;
        public string SelectedTinhTrangPhong { get => _SelectedTinhTrangPhong; set { _SelectedTinhTrangPhong = value; OnPropertyChanged(); } }
        private string _SearchPhong;
        public string SearchPhong { get => _SearchPhong; set { _SearchPhong = value; OnPropertyChanged(); } }
        public bool sort;

        public ICommand SearchPhongCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SortPhongCommand { get; set; }

        public PhongViewModel()
        {
            LoadTTPhong();
            ListLoaiPhong = new ObservableCollection<LOAIPHONG>(DataProvider.Ins.model.LOAIPHONG);
            string[] tinhtrangphongs = new string[] { "Trống", "Đang thuê", "Đang sửa chữa" };
            ListTinhTrangPhong = new ObservableCollection<string>(tinhtrangphongs);

            SearchPhongCommand = new RelayCommand<Object>((p) => { return true; }, (p) => {
                if (string.IsNullOrEmpty(SearchPhong))
                {
                    CollectionViewSource.GetDefaultView(ListTTPhong).Filter = (all) => { return true; };                    
                }
                else
                {
                    CollectionViewSource.GetDefaultView(ListTTPhong).Filter = (searchPhong) =>
                    {
                        return (searchPhong as ThongTinPhong).LoaiPhong.TEN_LP.IndexOf(SearchPhong, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchPhong as ThongTinPhong).Phong.MA_PHONG.ToString().IndexOf(SearchPhong, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchPhong as ThongTinPhong).Phong.TINHTRANG_PHONG.IndexOf(SearchPhong, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }
            });

            AddCommand = new RelayCommand<Object>((p) => 
            {
                if (string.IsNullOrEmpty(MaPhong.ToString()) || SelectedLoaiPhong == null || SelectedTinhTrangPhong == null)
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin phòng muốn thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }                    

                var listPhong = DataProvider.Ins.model.PHONG.Where(x => x.MA_PHONG == MaPhong);
                if (listPhong == null || listPhong.Count() != 0)
                {
                    MessageBox.Show("Phòng đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }                    

                return true;
            }, (p) => {                
                var phong = new PHONG() { MA_PHONG = MaPhong, MA_LP = SelectedLoaiPhong.MA_LP, TINHTRANG_PHONG = SelectedTinhTrangPhong };
                DataProvider.Ins.model.PHONG.Add(phong);
                DataProvider.Ins.model.SaveChanges();
                //lấy loại phòng từ phòng vừa thêm vào và tạo ra thongtinphong sau đó thêm vào list
                var loaiPhong = DataProvider.Ins.model.LOAIPHONG.Where(x => x.MA_LP == phong.MA_LP).SingleOrDefault();
                ListTTPhong.Add(new ThongTinPhong() { Phong = phong, LoaiPhong = loaiPhong });

                MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTTPhong();
                RefershControls();
            });

            DeleteCommand = new RelayCommand<Object>((p) =>
            {
                if (string.IsNullOrEmpty(MaPhong.ToString()) || SelectedItem == null ||
                    SelectedLoaiPhong == null || SelectedTinhTrangPhong == null)
                {
                    MessageBox.Show("Vui lòng chọn phòng muốn xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var listPhong = DataProvider.Ins.model.PHONG.Where(x => x.MA_PHONG == MaPhong);
                if (listPhong != null && listPhong.Count() != 0)
                    return true;

                MessageBox.Show("Phòng không tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                using (var transactions = DataProvider.Ins.model.Database.BeginTransaction())
                {
                    try
                    {
                        var phong = DataProvider.Ins.model.PHONG.Where(x => x.MA_PHONG == SelectedItem.Phong.MA_PHONG).FirstOrDefault();
                        DataProvider.Ins.model.PHONG.Remove(phong);
                        DataProvider.Ins.model.SaveChanges();

                        transactions.Commit();
                        RemovePhong(phong.MA_PHONG);
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
                if (string.IsNullOrEmpty(MaPhong.ToString()) || SelectedItem == null || 
                    SelectedLoaiPhong == null || SelectedTinhTrangPhong == null)
                {
                    MessageBox.Show("Vui lòng chọn phòng muốn sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }                    

                var listPhong = DataProvider.Ins.model.PHONG.Where(x => x.MA_PHONG == MaPhong);
                if (listPhong != null && listPhong.Count() != 0)
                    return true;

                MessageBox.Show("Phòng không tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                var phong = DataProvider.Ins.model.PHONG.Where(x => x.MA_PHONG == SelectedItem.Phong.MA_PHONG).SingleOrDefault();
                phong.MA_LP = SelectedLoaiPhong.MA_LP;
                phong.TINHTRANG_PHONG = SelectedTinhTrangPhong;
                SelectedItem.LoaiPhong = DataProvider.Ins.model.LOAIPHONG.Where(x => x.MA_LP == SelectedLoaiPhong.MA_LP).SingleOrDefault();
                DataProvider.Ins.model.SaveChanges();

                MessageBox.Show("Sửa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTTPhong();
                RefershControls();
            });

            RefreshCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                RefershControls();
            });

            SortPhongCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) => {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListTTPhong);
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

        public void LoadTTPhong()
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
        }

        void RemovePhong(int map)
        {
            if (ListTTPhong == null || ListTTPhong.Count() == 0)
                return;
            foreach (ThongTinPhong item in ListTTPhong)
            {
                if (item.Phong.MA_PHONG == map)
                {
                    ListTTPhong.Remove(item);
                    return;
                }
            }
        }

        void RefershControls()
        {
            MaPhong = 0;
            SelectedLoaiPhong = null;
            SelectedTinhTrangPhong = null;
        }
    }
}
