using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace PalletViewer
{
	class Model : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private List<Pallet> pallets;

		public Pallet CurrentPallet { get; set; }

		public string Test { get; set; }

		public Model()
		{
			pallets = new List<Pallet>();
			Test = "Test";
		}



		public void AddPallet(Pallet pallet)
		{
			pallets.Add(pallet);
			CurrentPallet = pallet;
			PropertyChanged(this, new PropertyChangedEventArgs(nameof(CurrentPallet)));
		}
	}
}