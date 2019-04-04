using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PalletViewer
{
	class Model : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private List<Pallet> pallets;

		//Идёт бинд на его параметры
		public Pallet CurrentPallet { get; set; }

		public Model(StackPanel _ListLayers, MenuItem _ListOrders)
		{
			ListLayers = _ListLayers;
			pallets = new List<Pallet>();
		}

		private MenuItem ListOrders;

		private void AddOrderToList()
		{
			var order = new MenuItem();
			order.Header = "Order" + pallets.Count.ToString();
		}

		#region Список слоёв
		private void ChooseLayer(object sender, RoutedEventArgs e)
		{
			var index = Int32.Parse(((Button)sender).Content.ToString().Split('#')[1]);
			// вызвать отрисовку
		}

		private StackPanel ListLayers;

		private void CreateListLayer()
		{
			for (int i = 0; i < CurrentPallet.Layers.ToList().Count; i++)
			{
				var tempLayer = new Button();
				tempLayer.Content = "Layer" + '#' + i.ToString();
				tempLayer.Click += ChooseLayer;
				ListLayers.Children.Add(tempLayer);
			}
		}
		#endregion

		public void AddPallet(Pallet pallet)
		{
			pallets.Add(pallet);
			CurrentPallet = pallet;
			CreateListLayer();
			PropertyChanged(this, new PropertyChangedEventArgs(nameof(CurrentPallet)));
		}
	}
}