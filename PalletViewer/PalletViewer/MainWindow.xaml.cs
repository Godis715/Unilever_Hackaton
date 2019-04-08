using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace PalletViewer
{
	class Helper
	{
		public static void Swap<T>(ref T a, ref T b)
		{
			T temp = a;
			a = b;
			b = temp;
		}
	}

	public static class Params
	{
		public static readonly double MouseMoveEPS = 10.0;

		public static readonly int[][] BoxPolyIndecies = {
			new int[] { 4, 1, 0,   5, 1, 4,   2, 3, 6,   3, 7, 6 }, // up indecies
            new int[] { 6, 0, 2,   4, 0, 6,   1, 7, 3,   1, 5, 7 }, // front
            new int[] { 2, 1, 3,   0, 1, 2,   5, 6, 7,   5, 4, 6 }, // side
        };

		public static readonly int[][] BoxSidesIndecies = {
			new int[] { 4, 5, 1, 0,    3, 7, 6, 2 },
			new int[] { 2, 6, 4, 0,    5, 7, 3, 1 },
			new int[] { 3, 2, 0, 1,    6, 7, 5, 4 },
		};

		public static readonly int[][] BoxSidesWithoutBottomIndecies = {
			new int[] {     3, 7, 6, 2 },
			new int[] { 2, 6, 4, 0,    5, 7, 3, 1 },
			new int[] { 3, 2, 0, 1,    6, 7, 5, 4 },
		};

		public static readonly double BorderIndent = 0.01;

		public static readonly double CameraEPS = Math.PI / 50.0;

		public static readonly Vector CameraRotationCoefs = new Vector(1.0, 1.0);

		public static readonly SolidColorBrush UpBrush = new SolidColorBrush { Color = Colors.Aqua, Opacity = 1.0 };
		public static readonly SolidColorBrush FrontBush = new SolidColorBrush { Color = Colors.BlueViolet, Opacity = 1.0 };
		public static readonly SolidColorBrush SideBrush = new SolidColorBrush { Color = Colors.Magenta, Opacity = 1.0 };
		public static readonly SolidColorBrush BorderBrush = new SolidColorBrush { Color = Colors.Black, Opacity = 1.0 };
		//public static readonly ImageBrush test = new ImageBrush((BitmapImage)Application.Current.TryFindResource("test"));

		public static readonly Material UpMaterial = new DiffuseMaterial(UpBrush);
		public static readonly Material FrontMaterial = new DiffuseMaterial(FrontBush);
		public static readonly Material SideMaterial = new DiffuseMaterial(SideBrush);
		public static readonly Material BorderMaterial = new DiffuseMaterial(BorderBrush);
		public static readonly Material BaseMaterial = new DiffuseMaterial(Brushes.SandyBrown);
	}

	public class BoxBlock
	{
		public Vector3D Dim { get; set; }

		public Point3D Start { get; set; }

		public int[] BoxCount { get; set; }

		public int[] Orient { get; set; }
	}

	public class MeshWrapper
	{
		public MeshGeometry3D MyMesh { get; set; }

		public Material MyMat { get; set; }

		public MeshWrapper(Material _mat)
		{
			MyMesh = new MeshGeometry3D();

			MyMat = _mat;
		}

		public void AddMesh(Point3D[] points, int[] indecies)
		{
			int startIndex = MyMesh.Positions.Count;

			for (int i = 0; i < points.Length; ++i)
			{
				MyMesh.Positions.Add(points[i]);
			}

			for (int i = 0; i < indecies.Length; ++i)
			{
				MyMesh.TriangleIndices.Add(indecies[i] + startIndex);
			}
		}
	}

	public class MeshContainer
	{
		public Dictionary<string, MeshWrapper> MeshDict { get; set; }

		public MeshContainer()
		{
			MeshDict = new Dictionary<string, MeshWrapper>();
		}

		public void AddMesh(string meshType, Material _mat)
		{
			MeshDict.Add(meshType, new MeshWrapper(_mat));
		}

		public MeshWrapper GetMesh(string type)
		{
			MeshDict.TryGetValue(type, out MeshWrapper meshWrapper);

			return meshWrapper;
		}
	}

	

	public partial class MainWindow : Window
	{
		public MeshGeometry3D[] BoxToPolygons(MeshContainer meshCont, Box[] boxes)
		{
			var PolyIndecies = Params.BoxPolyIndecies;

			var meshes = new MeshGeometry3D[3];

			for (int i = 0; i < 3; ++i)
			{
				meshes[i] = new MeshGeometry3D();
			}

			for (int i = 0; i < boxes.Length; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					for (int k = 0; k < 8; ++k)
					{
						meshes[j].Positions.Add(boxes[i].Points[k]);
					}
				}

				for (int j = 0; j < 3; ++j)
				{
					int orient = boxes[i].Orient[j];
					for (int k = 0; k < PolyIndecies[j].Length; ++k)
					{
						meshes[j].TriangleIndices.Add(PolyIndecies[orient][k] + i * 8);
					}
				}
			}

			return meshes;
		}





		//public void SplitBlock(Point3D[] points, int w, int h, MeshWrapper meshGeom, MeshWrapper border)

		private Model model;

		public MainWindow()
		{
			InitializeComponent();
		}

		#region event handlers

		private void TestScene_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			/*
            double widthPallet = 6;
            double lengthPallet = 8;
            var layer = new LayerOnPallet(widthPallet, lengthPallet, TestScene);

            double widthBox = 0.4;
            double lengthBox = 2.2;
            double heigthBox = 2.3;

			Stopwatch stopwatch = new Stopwatch();
            BoxToPolygons1(MyScene.MyMesh, layer.Boxes.ToArray());

			stopwatch.Start();
			layer.GenerateBoxs_On(new BoxFactory(widthBox, heigthBox, lengthBox));
			layer.CreateLayer(widthBox, lengthBox, heigthBox,
				LayerOnPallet.DirectionFilling.Right, LayerOnPallet.OrientationBox.Vertically);
			stopwatch.Stop();

			var elapsedTime = stopwatch.Elapsed;

            var meshes = BoxToPolygons(layer.Boxes.ToArray());

            MyScene.RestoreMesh(meshes);
			foreach (var mesh in MyScene.MyMesh.MeshDict)
			{
				var model = new GeometryModel3D(mesh.Value.MyMesh, mesh.Value.MyMat);
				Models.Children.Add(model);
			}
            */
		}

		void MouseDown_ViewPort(object sender, MouseButtonEventArgs e)
		{
			model.MyScene.StartMousePos = e.GetPosition(this);

			model.MyScene.LastMousePos = model.MyScene.StartMousePos;
		}

		void MouseMove_ViewPort(object sender, MouseEventArgs e)
		{
			if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
			{
				if ((model.MyScene.LastMousePos - e.GetPosition(this)).Length > Params.MouseMoveEPS)
				{
					Vector shift = e.GetPosition(this) - model.MyScene.StartMousePos;

					model.MyScene.Camera.ShiftUVPosition(shift);

					model.MyScene.LastMousePos = e.GetPosition(this);
				}
			}
		}

		void MouseUp_ViewPort(object sender, MouseButtonEventArgs e)
		{
			model.MyScene.Camera.RestorePosition();
		}
		#endregion

		private void AddOrder(object sender, RoutedEventArgs e)
		{
			try
			{
				#region Считывание параметров
				var _WidthProduct = (int)UInt32.Parse(WidthProduct.Text);
				var _LengthProduct = (int)UInt32.Parse(LengthProduct.Text);
				var _HeightProduct = (int)UInt32.Parse(HeightProduct.Text);
				var _WeightProduct = (int)UInt32.Parse(WeightProduct.Text);

				var _SizeProduct = "";
				foreach (RadioButton SizeProduct_rb in SizesProduct.Children)
				{
					_SizeProduct = SizeProduct_rb.IsChecked.Value ? SizeProduct_rb.Content.ToString() : _SizeProduct;
				}

				var _WidthPallet = (int)UInt32.Parse(WidthPallet.Text);
				var _LengthPallet = (int)UInt32.Parse(LengthPallet.Text);
				var _HeigthPallet = (int)UInt32.Parse(HeigthPallet.Text);
				var _MaxWeightOnPallet = (int)UInt32.Parse(MaxWeightOnPallet.Text);
				var _HeigthBasePallet = (int)UInt32.Parse(HeigthBasePallet.Text);

				var _MaxWeightInBox = (int)UInt32.Parse(MaxWeightInBox.Text);
				var _MinCountInBox = (int)UInt32.Parse(MinCountInBox.Text);
				var _MaxCountInBox = (int)UInt32.Parse(MaxCountInBox.Text);
				var _RatioSideBox = (int)UInt32.Parse(RatioSideBox.Text);

				var _isDifferentLayer = false;
				foreach (RadioButton isDifferentLayer_Answer_rb in isDifferentLayer.Children)
				{
					if (isDifferentLayer_Answer_rb.IsChecked.Value && isDifferentLayer_Answer_rb.Content.ToString() == "Yes")
					{
						_isDifferentLayer = true;
					}
				}

				if (_WidthProduct == 0 || _LengthPallet == 0 || _HeightProduct == 0 || _WeightProduct == 0 || _HeigthBasePallet == 0
					|| _WidthPallet == 0 || _LengthPallet == 0 || _HeigthPallet == 0 || _MaxWeightOnPallet == 0
					|| _MaxWeightInBox == 0 || _MinCountInBox == 0 || _MaxCountInBox == 0 || _RatioSideBox == 0)
				{
					throw new FormatException();
				}
				#endregion
				#region Сбрасывание парамтров
				WidthProduct.Clear();
				LengthProduct.Clear();
				HeightProduct.Clear();
				WeightProduct.Clear();
				foreach (RadioButton SizeProduct_rb in SizesProduct.Children)
				{
					SizeProduct_rb.IsChecked = (SizeProduct_rb.Content.ToString() == "big") ? true : false;
				}
				WidthPallet.Clear();
				LengthPallet.Clear();
				HeigthPallet.Text = "1800";
				MaxWeightOnPallet.Text = "800";
				MaxWeightInBox.Text = "15";
				MinCountInBox.Text = "10";
				MaxCountInBox.Text = "20";
				RatioSideBox.Text = "4";
				foreach (RadioButton isDifferentLayer_Answer_rb in isDifferentLayer.Children)
				{
					isDifferentLayer_Answer_rb.IsChecked = (isDifferentLayer_Answer_rb.Content.ToString() == "No") ? true : false;
				}
				ErrorInput.Content = "Message: ";
				ErrorInput.Foreground = Brushes.Black;
				#endregion

				var paletization = new Palletization(_LengthProduct, _WidthProduct, _HeightProduct, _WeightProduct,
					_LengthPallet, _WidthPallet, _HeigthPallet, _HeigthBasePallet, _MaxWeightOnPallet,
					_MinCountInBox, _MaxCountInBox, _MaxWeightInBox, _RatioSideBox,
					_SizeProduct, _isDifferentLayer);
				//var paletization = new Palletization(159, 35, 68, 229,
				//	1200, 800, 1710, 145, 800,
				//	10, 10, 5, 3,
				//	"average", true);


				var pallet = paletization.GetPallet();
				if (pallet != null)
				{
					model.AddPallet(pallet);
				}
			}
			catch (FormatException)
			{
				ErrorInput.Content = "Message: Error! Input is not correct.";
				ErrorInput.Foreground = Brushes.Red;
				return;
			}
			//catch(Exception ex)
			//{
			//    ErrorInput.Content = "Message: Application error! " + ex.Message + " " + ex.Source;
			//    ErrorInput.Foreground = Brushes.Red;
			//    return;
			//}
		}

		void MouseLeave_ViewPort(object sender, MouseEventArgs e)
		{
			if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
			{
				model.MyScene.Camera.RestorePosition();
			}
		}

		#region Import orders
		private void ImportFile(object sender, RoutedEventArgs e)
		{
			var fileDialog = FileDialog.GetInstance();
			try
			{
				if (fileDialog.ImportFileDialog() == true)
				{
					PathImportFile.Text = fileDialog.FilePath;
				}
			}
			catch (Exception ex)
			{
				fileDialog.ShowMessage(ex.Message);
			}
		}

		private void ImportOrders(object sender, RoutedEventArgs e)
		{
			try
			{
				using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(PathImportFile.Text.ToString())))
				{
					var myWorksheet = xlPackage.Workbook.Worksheets.First(); //select sheet here
					var totalRows = myWorksheet.Dimension.End.Row;
					var totalColumns = myWorksheet.Dimension.End.Column;

					if (totalColumns != 15)
					{
						throw new ArgumentException();
					}

					for (int rowNum = 2; rowNum <= totalRows; rowNum++) //selet starting row here
					{
						var valueInColumns = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToArray<string>();
						#region Считывание параметров
						var _WidthProduct = (int)UInt32.Parse(valueInColumns[0]);
						var _LengthProduct = (int)UInt32.Parse(valueInColumns[1]);
						var _HeightProduct = (int)UInt32.Parse(valueInColumns[2]);
						var _WeightProduct = (int)UInt32.Parse(valueInColumns[3]);
						var _SizeProduct = valueInColumns[4];

						var _WidthPallet = (int)UInt32.Parse(valueInColumns[5]);
						var _LengthPallet = (int)UInt32.Parse(valueInColumns[6]);
						var _HeigthPallet = (int)UInt32.Parse(valueInColumns[7]);
						var _MaxWeightOnPallet = (int)UInt32.Parse(valueInColumns[8]);
						var _HeigthBasePallet = (int)UInt32.Parse(valueInColumns[9]);

						var _MaxWeightInBox = (int)UInt32.Parse(valueInColumns[10]);
						var _MinCountInBox = (int)UInt32.Parse(valueInColumns[11]);
						var _MaxCountInBox = (int)UInt32.Parse(valueInColumns[12]);
						var _RatioSideBox = (int)UInt32.Parse(valueInColumns[13]);

						var _isDifferentLayer = (valueInColumns[14] == "Yes") ? true : false;

						if (_WidthProduct == 0 || _LengthPallet == 0 || _HeightProduct == 0 || _WeightProduct == 0 || _HeigthBasePallet == 0
							|| _WidthPallet == 0 || _LengthPallet == 0 || _HeigthPallet == 0 || _MaxWeightOnPallet == 0
							|| _MaxWeightInBox == 0 || _MinCountInBox == 0 || _MaxCountInBox == 0 || _RatioSideBox == 0)
						{
							throw new FormatException();
						}
						#endregion

						var paletization = new Palletization(_LengthProduct, _WidthProduct, _HeightProduct, _WeightProduct,
							_LengthPallet, _WidthPallet, _HeigthPallet, _HeigthBasePallet, _MaxWeightOnPallet,
							_MinCountInBox, _MaxCountInBox, _MaxWeightInBox, _RatioSideBox,
							_SizeProduct, _isDifferentLayer);

						var pallet = paletization.GetPallet();
						if (pallet != null)
						{
							model.AddPallet(pallet);
						}
						PathImportFile.Text = "";
						ErrorInput.Foreground = Brushes.Black;
						ErrorInput.Content = "Message:";
					}
				}
			}
			catch (ArgumentException)
			{
				ErrorInput.Content = "Message: Error! File is not correct.";
				ErrorInput.Foreground = Brushes.Red;
				return;
			}
			catch (FormatException)
			{
				ErrorInput.Content = "Message: Error! Input is not correct.";
				ErrorInput.Foreground = Brushes.Red;
				return;
			}
			catch (InvalidOperationException)
			{
				ErrorInput.Content = "Message: Error! File is not correct.";
				ErrorInput.Foreground = Brushes.Red;
				return;
			}
		}
		#endregion
		#region Export orders
		private void ExportFile(object sender, RoutedEventArgs e)
		{
			var fileDialog = FileDialog.GetInstance();
			try
			{
				if (fileDialog.ExportFileDialog() == true)
				{
					PathExportFile.Text = fileDialog.FilePath;
				}
			}
			catch (Exception ex)
			{
				fileDialog.ShowMessage(ex.Message);
			}
		}

		private void ExportOrders(object sender, RoutedEventArgs e)
		{
			model.ExportOrders(PathExportFile.Text.ToString());
		}
		#endregion


		private void MainWindow_Loaded_1(object sender, RoutedEventArgs e)
		{
			model = new Model(ListLayers,ListOrders, Models, new Size(ViewportArea.ActualWidth, ViewportArea.ActualHeight));

			model.MyScene = new Scene(new Point3D(500, 500, 500), new Size(ViewportArea.ActualWidth, ViewportArea.ActualHeight))
			{

				UpBrush = Params.UpBrush,
				FrontBrush = Params.FrontBush,
				SideBrush = Params.SideBrush,
			};

			Main.DataContext = model;
		}

		private void BoxView_cl(object sender, RoutedEventArgs e)
		{
			if (model.CurrentPallet != null)
			{
				model.DrawPallet(model.CurrentPallet.BoxPallet);
			}
		}

		private void MainView_cl(object sender, RoutedEventArgs e)
		{
			if (model.CurrentPallet != null)
			{
				model.DrawPallet(model.CurrentPallet);
			}
		}
	}
}