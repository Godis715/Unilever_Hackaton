using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

    public partial class MainWindow : Window
    {
        public MeshGeometry3D[] BoxToPolygons(Box[] boxes)
        {
            var PolyIndecies = new int[][]
            {
            new int[] { 4, 1, 0,   5, 1, 4,   2, 3, 6,   3, 7, 6 }, // up indecies
            new int[] { 6, 0, 2,   4, 0, 6,   1, 7, 3,   1, 5, 7 }, // front
            new int[] { 2, 1, 3,   0, 1, 2,   5, 6, 7,   5, 4, 6 }, // side

            };

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

        public Scene MyScene { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            BoxFactory boxFactory = new BoxFactory(0.23, 0.11, 0.13);

            Box box = boxFactory.GenBox();

            Box box1 = boxFactory.GenBox();

            Box box2 = boxFactory.GenBox();

            box.RotUp();

            box1.RotFront();

            MeshGeometry3D[] meshes = BoxToPolygons(new Box[] { box, box1, box2 });

            MyScene = new Scene(new Point3D(0, 0, 0))
            {

                UpBrush = new SolidColorBrush { Color = Colors.Red, Opacity = 1.0 },
                FrontBrush = new SolidColorBrush { Color = Colors.Yellow, Opacity = 1.0 },
                SideBrush = new SolidColorBrush { Color = Colors.Green, Opacity = 1.0 },

                UpMesh = meshes[0],
                FrontMesh = meshes[1],
                SideMesh = meshes[2],
            };

            this.DataContext = MyScene;
        }

        #region event handlers

        private void TestScene_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double widthPallet = 6;
            double lengthPallet = 8;
            var layer = new LayerOnPallet(widthPallet, lengthPallet, TestScene);

            double widthBox = 0.4;
            double lengthBox = 2.2;
            double heigthBox = 2.3;

			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Start();
			layer.GenerateBoxs_On(new BoxFactory(widthBox, heigthBox, lengthBox));
			layer.CreateLayer(widthBox, lengthBox, heigthBox,
				LayerOnPallet.DirectionFilling.Right, LayerOnPallet.OrientationBox.Vertically);
			stopwatch.Stop();

			var elapsedTime = stopwatch.Elapsed;

            var meshes = BoxToPolygons(layer.Boxes.ToArray());

            MyScene.RestoreMesh(meshes);

        }

        void MouseDown_ViewPort(object sender, MouseButtonEventArgs e)
        {
            MyScene.StartMousePos = e.GetPosition(this);

            MyScene.LastMousePos = MyScene.StartMousePos;
        }

        void MouseMove_ViewPort(object sender, MouseEventArgs e)
        {
            if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if ((MyScene.LastMousePos - e.GetPosition(this)).Length > 10)
                {
                    Vector shift = e.GetPosition(this) - MyScene.StartMousePos;

                    MyScene.Camera.ShiftUVPosition(shift);

                    MyScene.LastMousePos = e.GetPosition(this);
                }
            }
        }

        void MouseUp_ViewPort(object sender, MouseButtonEventArgs e)
        {
            MyScene.Camera.RestorePosition();
        }
		#endregion

		private void AddOrder(object sender, RoutedEventArgs e)
		{
			try
			{
				//#region Считывание параметров
				//var _WidthProduct = (int)UInt32.Parse(WidthProduct.Text);
				//var _LengthProduct = (int)UInt32.Parse(LengthProduct.Text);
				//var _HeightProduct = (int)UInt32.Parse(HeightProduct.Text);
				//var _WeightProduct = (int)UInt32.Parse(WeightProduct.Text);

				//var _SizeProduct = "";
				//foreach (RadioButton SizeProduct_rb in SizesProduct.Children)
				//{
				//	_SizeProduct = SizeProduct_rb.IsChecked.Value ? SizeProduct_rb.Content.ToString() : _SizeProduct;
				//}

				//var _WidthPallet = (int)UInt32.Parse(WidthPallet.Text);
				//var _LengthPallet = (int)UInt32.Parse(LengthPallet.Text);
				//var _HeigthPallet = (int)UInt32.Parse(HeigthPallet.Text);
				//var _MaxWeightOnPallet = (int)UInt32.Parse(MaxWeightOnPallet.Text);

				//var _MaxWeightInBox = (int)UInt32.Parse(MaxWeightInBox.Text);
				//var _MinCountInBox = (int)UInt32.Parse(MinCountInBox.Text);
				//var _MaxCountInBox = (int)UInt32.Parse(MaxCountInBox.Text);
				//var _RatioSideBox = (int)UInt32.Parse(RatioSideBox.Text);

				//var _isDifferentLayer = false;
				//foreach (RadioButton isDifferentLayer_Answer_rb in isDifferentLayer.Children)
				//{
				//	if (isDifferentLayer_Answer_rb.IsChecked.Value && isDifferentLayer_Answer_rb.Content.ToString() == "Yes")
				//	{
				//		_isDifferentLayer = true;
				//	}
				//}
				//#endregion
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
					_LengthPallet, _WidthPallet, _HeigthPallet, _MaxWeightOnPallet,
					_MinCountInBox, _MaxCountInBox, _MaxWeightInBox, _RatioSideBox,
					_SizeProduct, _isDifferentLayer);
				var pallet = paletization.GetPallet();

				//var paletization = new Palletization(9, 10, 11, 1,
				//	1200, 800, 1560, 650,
				//	2, 60, 10, 200,
				//	"big", false);
				//Stopwatch stopwatch = new Stopwatch();

				//stopwatch.Start();
				//var pallet = paletization.GetPallet();
				//stopwatch.Stop();

				//var elapsedTime = stopwatch.Elapsed;
				//var x = elapsedTime.Milliseconds;
				//int h = 0;
				//var boxesGen = new BoxGenerator((int)_WeightProduct, (int)_LengthPallet, (int)_HeightProduct, (int)_MinCountInBox, (int)_MaxCountInBox, (int)_WeightProduct);
				//var boxes = boxesGen.GetBoxes();
				//boxes = boxesGen.ValidationSize(boxes, (int)_RatioSideBox);

				//var file = new StreamWriter("test.txt");
				//foreach (var item in boxes)
				//{
				//	file.WriteLine("Box: " + item.x.ToString() + "; "
				//		+ item.y.ToString() + "; " + item.z.ToString());
				//}
				//file.WriteLine(boxes.Length);
				//file.Close();
				//TestScene.Children.Clear();

				//double widthPallet = 200;
				//double lengthPallet = 150;
				//var layer = new LayerOnPallet(widthPallet, lengthPallet, TestScene);

				//layer.GenerateBoxs_On(new BoxFactory(_WeightProduct, _HeightProduct, _LengthPallet));
				//layer.CreateLayer(_WeightProduct, _LengthPallet, _HeightProduct,
				//	LayerOnPallet.DirectionFilling.Right, LayerOnPallet.OrientationBox.Vertically);
			}
			catch
			{
				ErrorInput.Content = "Message: Error! Input not correct.";
				ErrorInput.Foreground = Brushes.Red;
				return;
			}
		}

		private void AddFile(object sender, RoutedEventArgs e)
		{
			var fileDialog = FileDialog.GetInstance();
			try
			{
				if (fileDialog.OpenFileDialog() == true)
				{
					PathFile.Text = fileDialog.FilePath;
				}
			}
			catch (Exception ex)
			{
				fileDialog.ShowMessage(ex.Message);
			}
		}

		private void AddOrders(object sender, RoutedEventArgs e)
		{
			//
		}
	}
}
