using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace PalletViewer
{
	public class FileDialog
	{
		public string FilePath { get; set; }

		public bool OpenFileDialog()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "(*.xlsx)|*.xlsx";
			if (openFileDialog.ShowDialog() == true)
			{
				FilePath = openFileDialog.FileName;
				return true;
			}
			return false;
		}

		private static FileDialog instance = null;

		public static FileDialog GetInstance()
		{
			if (instance == null)
			{
				instance = new FileDialog();
			}
			return instance;
		}

		public void ShowMessage(string message)
		{
			MessageBox.Show(message);
		}
	}
}