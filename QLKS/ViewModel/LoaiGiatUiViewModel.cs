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
    public class LoaiGiatUiViewModel : BaseViewModel
    {
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
        private string _TenLoaiGiatUi;
        public string TenLoaiGiatUi { get => _TenLoaiGiatUi; set { _TenLoaiGiatUi = value; OnPropertyChanged(); } }
        private int _DonGia;
        public int DonGia { get => _DonGia; set { _DonGia = value; OnPropertyChanged(); } }
        private string _SearchLoaiGiatUi;
        public string SearchLoaiGiatUi { get => _SearchLoaiGiatUi; set { _SearchLoaiGiatUi = value; OnPropertyChanged(); } }
        public bool sort;

        public ICommand SearchLoaiGiatUiCommand { get; set; }
        //public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SortLoaiGiatUiCommand { get; set; }

        public LoaiGiatUiViewModel()
        {
            ListLoaiGiatUi = new ObservableCollection<LOAIGIATUI>(DataProvider.Ins.model.LOAIGIATUI);

            SearchLoaiGiatUiCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                if (string.IsNullOrEmpty(SearchLoaiGiatUi))
                {
                    CollectionViewSource.GetDefaultView(ListLoaiGiatUi).Filter = (all) => { return true; };
                }
                else
                {
                    CollectionViewSource.GetDefaultView(ListLoaiGiatUi).Filter = (searchLoaiGiatUi) =>
                    {
                        return (searchLoaiGiatUi as LOAIGIATUI).TEN_LOAIGU.IndexOf(SearchLoaiGiatUi, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               (searchLoaiGiatUi as LOAIGIATUI).DONGIA_LOAIGU.ToString().IndexOf(SearchLoaiGiatUi, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }

            });

            //AddCommand = new RelayCommand<Object>((p) =>
            //{
            //    if (string.IsNullOrEmpty(TenLoaiGiatUi) || string.IsNullOrEmpty(DonGia.ToString()))
            //        return false;

            //    var listLoaiGiatUi = DataProvider.Ins.model.LOAIGIATUI.Where(x => x.TEN_LOAIGU == TenLoaiGiatUi);
            //    if (listLoaiGiatUi == null || listLoaiGiatUi.Count() != 0)
            //        return false;

            //    return true;
            //}, (p) =>
            //{
            //    var loaiGiatUi = new LOAIGIATUI() { TEN_LOAIGU = TenLoaiGiatUi, DONGIA_LOAIGU = DonGia };

            //    DataProvider.Ins.model.LOAIGIATUI.Add(loaiGiatUi);
            //    DataProvider.Ins.model.SaveChanges();

            //    ListLoaiGiatUi.Add(loaiGiatUi);
            //});

            EditCommand = new RelayCommand<Object>((p) =>
            {
                if (string.IsNullOrEmpty(TenLoaiGiatUi) || string.IsNullOrEmpty(DonGia.ToString())
                 || DonGia == 0 || SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn loại giặt ủi muốn sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var listLoaiGiatUi = DataProvider.Ins.model.LOAIGIATUI.Where(x => x.TEN_LOAIGU == TenLoaiGiatUi);
                if (listLoaiGiatUi != null && listLoaiGiatUi.Count() != 0)
                    return true;

                MessageBox.Show("Loại giặt ủi không tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }, (p) =>
            {
                var loaiGiatUi = DataProvider.Ins.model.LOAIGIATUI.Where(x => x.TEN_LOAIGU == TenLoaiGiatUi).SingleOrDefault();
                loaiGiatUi.TEN_LOAIGU = TenLoaiGiatUi;
                loaiGiatUi.DONGIA_LOAIGU = DonGia;
                DataProvider.Ins.model.SaveChanges();

                MessageBox.Show("Sửa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefershControls();
            });

            RefreshCommand = new RelayCommand<Object>((p) => { return true; }, (p) =>
            {
                RefershControls();
            });

            SortLoaiGiatUiCommand = new RelayCommand<GridViewColumnHeader>((p) => { return p == null ? false : true; }, (p) =>
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListLoaiGiatUi);
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

        void RefershControls()
        {
            TenLoaiGiatUi = null;
            DonGia = 0;
        }
    }
}
