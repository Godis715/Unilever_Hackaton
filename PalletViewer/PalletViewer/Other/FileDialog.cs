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

		public bool ImportFileDialog()
		{
			OpenFileDialog importFileDialog = new OpenFileDialog();
			importFileDialog.Title = "Import";
			importFileDialog.Filter = "(*.xlsx)|*.xlsx";
			if (importFileDialog.ShowDialog() == true)
			{
				FilePath = importFileDialog.FileName;
				return true;
			}
			return false;
		}

		public bool ExportFileDialog()

		{
			SaveFileDialog exportFileDialog = new SaveFileDialog();

			exportFileDialog.Title = "Export";

			exportFileDialog.Filter = "(*.xlsx)|*.xlsx";

			if (exportFileDialog.ShowDialog() == true)

			{
				FilePath = exportFileDialog.FileName;
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