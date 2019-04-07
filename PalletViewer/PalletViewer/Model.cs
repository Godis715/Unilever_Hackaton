using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PalletViewer
{
	class Model : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private List<Pallet> pallets;

		//Идёт бинд на его параметры
		public Pallet CurrentPallet { get; set; }

		//Status bar биндиться на это поле
		public string tempOrderStr { get; set; }

		#region Поддон
		private List<Box> BasePallet;

		private void CreateBasePallet()
		{
			BasePallet = new List<Box>();
			double x = (double)CurrentPallet.Lenght / 20;
			double y = (double)CurrentPallet.BaseHeight / 5;
			double z = (double)CurrentPallet.Widht / 40;
			// upper balk
			var boxFactory = new BoxFactory(y, CurrentPallet.Widht, 2 * x);
			for (int i = 0; i < 7; i++)
			{
				var balk= boxFactory.GenBox();
				balk.Translate(new Vector3D { X = 0, Y = -y, Z = i * x * 3 });
				BasePallet.Add(balk);
			}
			// down balk
			boxFactory = new BoxFactory(y * 4, z * 2, CurrentPallet.Lenght);
			for (int i = 0; i < 3; i++)
			{
				var balk = boxFactory.GenBox();
				balk.Translate(new Vector3D { X = i * z * 19, Y = -CurrentPallet.BaseHeight, Z = 0 });
				BasePallet.Add(balk);
			}
		}
		#endregion

		public Model(StackPanel _ListLayers, MenuItem _ListOrders, Model3DGroup modelGroup, Size _vpsize)
		{
			ListLayers = _ListLayers;
			ListOrders = _ListOrders;
			pallets = new List<Pallet>();
			MyScene = new Scene(new Point3D(0, 0, 0), _vpsize);
			ModelGroup = modelGroup;
			tempOrderStr = "Current order: ";
		}

		public void ExportOrders(string pathFile)
		{
			using (ExcelPackage excel = new ExcelPackage())
			{
				//Создание листа
				excel.Workbook.Worksheets.Add("Result");
				var excelWorksheet = excel.Workbook.Worksheets["Result"];

				//Добавление заголовка
				List<string[]> headerRow = new List<string[]>()
				{
				  new string[] { "PalletWidth", "PalletLength", "PalletHeight", "WeightOnPallet", "CountProductOnPallet",
				  "BoxWidth", "BoxLength", "BoxHeight", "WeightInBox", "CountProductInBox"}
				};
				string headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";
				excelWorksheet.Cells[headerRange].LoadFromArrays(headerRow);

				//Добавление содержимого
				var cellData = new List<object[]>();
				for (int i = 0; i < pallets.Count; i++)
				{
					var pallet = pallets[i];
					cellData.Add(new object[] {
						pallet.Widht.ToString(), pallet.Lenght.ToString(),
						pallet.Height.ToString(), pallet.Weight.ToString(), pallet.CountPr.ToString(),
						pallet.BoxPallet.Widht.ToString(), pallet.BoxPallet.Lenght.ToString(),
						pallet.BoxPallet.Height.ToString(), pallet.BoxPallet.Weight.ToString(), pallet.BoxPallet.CountPr.ToString()
					});
				}
				excelWorksheet.Cells[2, 1].LoadFromArrays(cellData);

				//Сохранение
				FileInfo excelFile = new FileInfo(pathFile);
				excel.SaveAs(excelFile);
			}
		}

		#region Список заказов
		private void ChooseOrder(object sender, RoutedEventArgs e)
		{
			var nameOrder = ((MenuItem)sender).Header.ToString();
			var index = Int32.Parse(nameOrder.Split('#')[1]);

			CurrentPallet = pallets[index];

			CreateListLayer();
			PropertyChanged(this, new PropertyChangedEventArgs(nameof(CurrentPallet)));
			CreateBasePallet();
			DrawPallet(CurrentPallet);

			tempOrderStr = "Current order: " + nameOrder;
			PropertyChanged(this, new PropertyChangedEventArgs(nameof(tempOrderStr)));
		}

		private MenuItem ListOrders;

		private void AddOrderToList()
		{
			var nameOrder = "Order" + '#' + (pallets.Count - 1).ToString();

			var order = new MenuItem();
			order.Header = nameOrder;
			order.Click += ChooseOrder;
			ListOrders.Items.Add(order);

			tempOrderStr = "Current order: " + nameOrder;
			PropertyChanged(this, new PropertyChangedEventArgs(nameof(tempOrderStr)));
		}
		#endregion

		#region Список слоёв
		private StackPanel ListLayers;

		private Layer GetLayerByName(string name) => CurrentPallet.Layers[Int32.Parse(name.Split('#')[1])];

		private void FlipByX(object sender, RoutedEventArgs e)
		{
			var nameLayer = ((Button)((Grid)((Menu)((MenuItem)((MenuItem)sender).Parent).Parent).Parent).Children[0]).Content.ToString();
			var layer = GetLayerByName(nameLayer); 
			if (layer != null)
			{
				layer.FlipByX(CurrentPallet.Widht);
				//var layerPallet = new Pallet(layer, CurrentPallet.Box,
				//CurrentPallet.Lenght, CurrentPallet.Widht, layer.height, 0, CurrentPallet.Weight);

				DrawPallet(CurrentPallet);
			}
		}

		private void FlipByZ(object sender, RoutedEventArgs e)
		{
			var nameLayer = ((Button)((Grid)((Menu)((MenuItem)((MenuItem)sender).Parent).Parent).Parent).Children[0]).Content.ToString();
			var layer = GetLayerByName(nameLayer);
			if (layer != null)
			{
				layer.FlipByZ(CurrentPallet.Lenght);
				//var layerPallet = new Pallet(layer, CurrentPallet.Box,
				//CurrentPallet.Lenght, CurrentPallet.Widht, layer.height, 0, CurrentPallet.Weight);

				DrawPallet(CurrentPallet);
			}
		}

		private void SwapLayer(object sender, RoutedEventArgs e)
		{
			var nameLayer1 = ((Button)((Grid)((Menu)((MenuItem)((MenuItem)((MenuItem)sender).Parent).Parent).Parent).Parent).Children[0]).Content.ToString();
			var layer1 = GetLayerByName(nameLayer1);
			var index1 = CurrentPallet.Layers.ToList<Layer>().IndexOf(layer1);

			var nameLayer2 = ((MenuItem)sender).Header.ToString();
			var layer2 = GetLayerByName(nameLayer2);
			var index2 = CurrentPallet.Layers.ToList<Layer>().IndexOf(layer2);

			Helper.Swap(ref CurrentPallet.Layers[index1], ref CurrentPallet.Layers[index2]);

			// restore heights
			CurrentPallet.ShiftLayers();

			// redraw
			DrawPallet(CurrentPallet);
		}

		private void ChooseLayer(object sender, RoutedEventArgs e)
		{
			var nameLayer = ((Button)sender).Content.ToString();

			var layer = GetLayerByName(nameLayer);

			// drawing
			var layerPallet = new Pallet(layer, CurrentPallet.Box,
				CurrentPallet.Lenght, CurrentPallet.Widht, layer.height, 0, CurrentPallet.Weight);
			DrawPallet(layerPallet);
		}

		private void CreateListLayer()
		{
			ListLayers.Children.Clear();

			for (int i = 0; i < CurrentPallet.Layers.ToList().Count; i++)
			{
				var grid = new Grid();
				grid.ColumnDefinitions.Add(new ColumnDefinition ());
				grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

				var tempLayer = new Button();
				tempLayer.Content = "Layer" + '#' + i.ToString();
				tempLayer.Click += ChooseLayer;

				var redLayer_menu = new Menu();
				var redLayer_menuIt = new MenuItem { Header = "Redaction" };

				var flipX_item = new MenuItem { Header = "Flip X"};
				flipX_item.Click += FlipByX;
				var flipZ_item = new MenuItem { Header = "Flip Z" };
				flipZ_item.Click += FlipByZ;
				var swap_item = new MenuItem { Header = "Swap"};
				for (int j = 0; j < CurrentPallet.Layers.ToList().Count; j++)
				{
					if (j != i)
					{
						var nameLayer = "Layer" + '#' + j.ToString();
						var layerToSwap = new MenuItem();
						layerToSwap.Header = nameLayer;
						layerToSwap.Click += SwapLayer;
						swap_item.Items.Add(layerToSwap);
					}
				}

				redLayer_menuIt.Items.Add(flipX_item);
				redLayer_menuIt.Items.Add(flipZ_item);
				redLayer_menuIt.Items.Add(swap_item);

				redLayer_menu.Items.Add(redLayer_menuIt);

				Grid.SetColumn(tempLayer, 0);
				Grid.SetColumn(redLayer_menu, 1);

				grid.Children.Add(tempLayer);
				grid.Children.Add(redLayer_menu);

				ListLayers.Children.Add(grid);
			}
		}
		#endregion

		public void AddPallet(Pallet pallet)
		{

			pallets.Add(pallet);
			CurrentPallet = pallet;
			CreateListLayer();
			AddOrderToList();
			PropertyChanged(this, new PropertyChangedEventArgs(nameof(CurrentPallet)));
			CreateBasePallet();
			DrawPallet(CurrentPallet);
		}

		#region Функции сцены

		public Scene MyScene { get; set; }

		public Model3DGroup ModelGroup { get; set; }

		public void DrawPalletBase()
		{
			var ind = Params.BoxSidesIndecies;
			var borderMesh = MyScene.MyMesh.GetMesh("border");
			var palletBaseMesh = MyScene.MyMesh.GetMesh("basePallet");
			foreach (var balk in BasePallet)
			{
				var points = balk.Points;
				for (int i = 0; i < ind.Length; ++i) {
					for (int j = 0; j < 3; ++j)
					{
						for (int k = 0; k < 2; ++k)
						{
							CreateMarkedPolygon(
								new Point3D[]
								{
								points[ind[j][k * 4 + 0]],
								points[ind[j][k * 4 + 1]],
								points[ind[j][k * 4 + 2]],
								points[ind[j][k * 4 + 3]],
								},
								palletBaseMesh,
								borderMesh,
								5.0
								);
						}
					}
				}
			}
		}

		public void DrawPallet(Pallet pallet)
		{
			Vector3D palletDim = new Vector3D(pallet.Widht, pallet.Height, pallet.Lenght);

			double radParam = 1.8;

			double cameraRadius = palletDim.Length * radParam;

			Point3D center = new Point3D(pallet.Widht / 2, 0, pallet.Lenght / 2);

			//model.MyScene.Camera = new MyCamera(cameraRadius, center, new Point(Math.PI / 4, Math.PI / 4), model.MyScene.ViewportSize);

			MyCamera camera = MyScene.Camera;

			camera.Center = center;
			camera.Rad = cameraRadius;

			camera.ShiftUVPosition(new Vector(0, 0));

			camera.RestorePosition();

			var light = ModelGroup.Children[0];
			ModelGroup.Children.Clear();
			ModelGroup.Children.Add(light);

			MyScene.MyMesh = CreateMeshContainer();

			foreach (var layer in pallet.Layers)
			{
				BoxToPolygons(MyScene.MyMesh, layer.boxes.ToArray());
			}

			foreach (var model in MyScene.MyMesh.MeshDict)
			{
				ModelGroup.Children.Add(new GeometryModel3D(model.Value.MyMesh, model.Value.MyMat));
			}
			DrawPalletBase();
		}

		private MeshContainer CreateMeshContainer()
		{
			var meshCont = new MeshContainer();

			meshCont.AddMesh("up", Params.UpMaterial);
			meshCont.AddMesh("front", Params.FrontMaterial);
			meshCont.AddMesh("side", Params.SideMaterial);
			meshCont.AddMesh("border", Params.BorderMaterial);
			meshCont.AddMesh("basePallet", Params.BaseMaterial);

			return meshCont;
		}

		public void BoxBlockToPolygon(MeshContainer meshCont, BoxBlock block)
		{
			MeshWrapper[] geomMeshes = new MeshWrapper[] {
				meshCont.GetMesh("up"),
				meshCont.GetMesh("front"),
				meshCont.GetMesh("side")
			};

			MeshWrapper borderMesh = meshCont.GetMesh("border");

			var S = block.Start;
			var E = S + block.Dim;

			var points = new Point3D[]
			{
							new Point3D(S.X, S.Y, S.Z),
							new Point3D(S.X, S.Y, E.Z),
							new Point3D(S.X, E.Y, S.Z),
							new Point3D(S.X, E.Y, E.Z),

							new Point3D(E.X, S.Y, S.Z),
							new Point3D(E.X, S.Y, E.Z),
							new Point3D(E.X, E.Y, S.Z),
							new Point3D(E.X, E.Y, E.Z)
			};

			var ind = Params.BoxSidesWithoutBottomIndecies;

			for (int j = 0; j < 3; ++j)
			{
				int orient = block.Orient[j];
				for (int k = 0; k < ind[orient].Length; ++k)
				{

					CreateMarkedPolygon(
						new Point3D[]
						{
								points[ind[orient][k * 4 + 0]],
								points[ind[orient][k * 4 + 1]],
								points[ind[orient][k * 4 + 2]],
								points[ind[orient][k * 4 + 3]],
						},
						geomMeshes[orient],
						borderMesh,
						0.5
						);
				}
			}


		}

		public void BoxToPolygons(MeshContainer meshCont, Box[] boxes)
		{
			MeshWrapper[] geomMeshes = new MeshWrapper[] {
				meshCont.GetMesh("up"),
				meshCont.GetMesh("front"),
				meshCont.GetMesh("side")
			};

			MeshWrapper borderMesh = meshCont.GetMesh("border");

			var ind = Params.BoxSidesIndecies;
			for (int i = 0; i < boxes.Length; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					int orient = boxes[i].Orient[j];
					for (int k = 0; k < ind[j].Length / 4; ++k)
					{

						CreateMarkedPolygon(
							new Point3D[]
							{
								boxes[i].Points[ind[j][k * 4 + 0]],
								boxes[i].Points[ind[j][k * 4 + 1]],
								boxes[i].Points[ind[j][k * 4 + 2]],
								boxes[i].Points[ind[j][k * 4 + 3]],
							},
							geomMeshes[orient],
							borderMesh,
							5
							);
					}
				}
			}
		}

		public void CreateMarkedPolygon(Point3D[] points, MeshWrapper geomMesh, MeshWrapper borderMesh, double thickness)
		{
			CreatePolygonMesh(points, geomMesh);

			var indent = Vector3D.CrossProduct(points[1] - points[0], points[3] - points[0]);

			indent.Normalize();

			indent *= Params.BorderIndent;

			var borderPoints = new List<Point3D>();

			var borderIndecies = new List<int>();

			for (int i = 0; i < points.Length; ++i)
			{
				borderPoints.Add(points[i] + indent);
			}

			for (int i = 0; i < 4; ++i)
			{
				int j = (i + 1) % 4;

				int k = (i + 2) % 4;


				Vector3D dir = points[k] - points[j];

				dir.Normalize();

				dir *= thickness;

				Point3D newPoint = points[j] + dir + indent;

				borderPoints.Add(newPoint);

				borderIndecies.Add(i);
				borderIndecies.Add(j);
				borderIndecies.Add(4 + 2 * i);

				int l = (i + 4 - 1) % 4;

				dir = points[l] - points[i];

				dir.Normalize();

				dir *= thickness;

				newPoint = points[i] + dir + indent;

				borderPoints.Add(newPoint);

				borderIndecies.Add(i);
				borderIndecies.Add(4 + 2 * i);
				borderIndecies.Add(4 + 2 * i + 1);
			}

			borderMesh.AddMesh(borderPoints.ToArray(), borderIndecies.ToArray());
		}

		public void CreatePolygonMesh(Point3D[] points, MeshWrapper geomMesh)
		{
			geomMesh.AddMesh(points, new int[] { 0, 1, 2, 2, 3, 0 });
		}
		#endregion
	}
}