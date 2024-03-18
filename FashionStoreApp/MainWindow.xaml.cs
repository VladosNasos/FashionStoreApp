using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FashionStoreApp
{
    public partial class MainWindow : Window
    {
        private ClothingManager clothingManager = new ClothingManager();

        public MainWindow()
        {
    InitializeComponent();
    ItemsListView.ItemsSource = clothingManager.Items;
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var newItem = CreateClothingItemFromInput();
            clothingManager.AddItem(newItem);
            UpdateListView();
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var idToDelete = IdTextBox.Text;
            clothingManager.DeleteItem(idToDelete);
            UpdateListView();
        }

        private void ModifyItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var modifiedItem = CreateClothingItemFromInput();
            clothingManager.ModifyItem(modifiedItem);
            UpdateListView();
        }

        private void SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                clothingManager.SaveToFile(saveFileDialog.FileName);
            }
        }

        private void LoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                clothingManager.LoadFromFile(openFileDialog.FileName);
                UpdateListView();
            }
        }

        private void UpdateListView()
        {
            ItemsListView.Items.Refresh();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(IdTextBox.Text) ||
                string.IsNullOrWhiteSpace(TypeTextBox.Text) ||
                string.IsNullOrWhiteSpace(CutTextBox.Text) ||
                string.IsNullOrWhiteSpace(ColorTextBox.Text) ||
                string.IsNullOrWhiteSpace(FabricTextBox.Text) ||
                string.IsNullOrWhiteSpace(BrandTextBox.Text) ||
                string.IsNullOrWhiteSpace(BasePriceTextBox.Text))
            {
                MessageBox.Show("All fields must be filled out.");
                return false;
            }

            if (!decimal.TryParse(BasePriceTextBox.Text, out _))
            {
                MessageBox.Show("Base price must be a valid number.");
                return false;
            }

            return true;
        }

        private ClothingItem CreateClothingItemFromInput()
        {
            return new ClothingItem
            {
                Id = IdTextBox.Text,
                Type = TypeTextBox.Text,
                Cut = CutTextBox.Text,
                Color = ColorTextBox.Text,
                Fabric = FabricTextBox.Text,
                Brand = BrandTextBox.Text,
                BasePrice = decimal.Parse(BasePriceTextBox.Text),
                SizePriceModifiers = new Dictionary<string, decimal>() // Initialize without size modifiers
            };
        }
    }

    public class ClothingManager : INotifyPropertyChanged
    {
        private ObservableCollection<ClothingItem> _items = new ObservableCollection<ClothingItem>();
        public ObservableCollection<ClothingItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddItem(ClothingItem item)
        {
            Items.Add(item);
            OnPropertyChanged(nameof(Items));
        }

        public void DeleteItem(string id)
        {
            var item = Items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                Items.Remove(item);
                OnPropertyChanged(nameof(Items));
            }
        }

        public void ModifyItem(ClothingItem modifiedItem)
        {
            var item = Items.FirstOrDefault(i => i.Id == modifiedItem.Id);
            if (item != null)
            {
                item.Type = modifiedItem.Type;
                item.Cut = modifiedItem.Cut;
                item.Color = modifiedItem.Color;
                item.Fabric = modifiedItem.Fabric;
                item.Brand = modifiedItem.Brand;
                item.BasePrice = modifiedItem.BasePrice;
                item.SizePriceModifiers = modifiedItem.SizePriceModifiers;
                OnPropertyChanged(nameof(Items));
            }
        }

        public void SaveToFile(string filename)
        {
            File.WriteAllLines(filename, Items.Select(item => item.ToString()));
        }

        public void LoadFromFile(string filename)
        {
            var newItems = File.ReadAllLines(filename).Select(ClothingItem.Parse);
            Items.Clear();
            foreach (var item in newItems)
            {
                Items.Add(item);
            }
        }
    }

    public class ClothingItem : INotifyPropertyChanged
    {
        public Dictionary<string, decimal> SizePriceModifiers { get; set; } = new Dictionary<string, decimal>();

        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }
        private string _cut;
        public string Cut
        {
            get => _cut;
            set
            {
                _cut = value;
                OnPropertyChanged();
            }
        }
        private string _color;
        public string Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }
        private string _fabric;
        public string Fabric
        {
            get => _fabric;
            set
            {
                _fabric = value;
                OnPropertyChanged();
            }
        }
        private string _brand;
        public string Brand
        {
            get => _brand;
            set
            {
                _brand = value;
                OnPropertyChanged();
            }
        }
        private decimal _baseprice;
        public decimal BasePrice
        {
            get => _baseprice;
            set
            {
                _baseprice = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public override string ToString()
        {
            return $"{Id}|{Type}|{Cut}|{Color}|{Fabric}|{Brand}|{BasePrice}|{string.Join(",", SizePriceModifiers.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}";
        }

        public static ClothingItem Parse(string data)
        {
            var parts = data.Split('|');
            if (parts.Length < 8) throw new FormatException("The data format is incorrect.");

            var sizeModifiers = new Dictionary<string, decimal>();
            var sizePriceParts = parts[7].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in sizePriceParts)
            {
                var sizePrice = part.Split(':');
                if (sizePrice.Length != 2) throw new FormatException("Size price modifier format is incorrect.");

                var size = sizePrice[0];
                if (!decimal.TryParse(sizePrice[1], out var priceModifier))
                {
                    throw new FormatException("Price modifier is not a valid decimal.");
                }

                sizeModifiers[size] = priceModifier;
            }

            return new ClothingItem
            {
                Id = parts[0],
                Type = parts[1],
                Cut = parts[2],
                Color = parts[3],
                Fabric = parts[4],
                Brand = parts[5],
                BasePrice = decimal.Parse(parts[6]),
                SizePriceModifiers = sizeModifiers
            };
        }
    }
}
