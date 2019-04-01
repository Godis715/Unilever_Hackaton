
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace PalletViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
	class LayerOnPallet
	{
		/* Сведения о слое
		 (0,0)_______
			 |       |
			 |       | l
			 |       |
			 |_______|
			     w    (w,l)
		 */
		#region Функции рисования
		private void DrawLine(Vector pointStart, Vector pointEnd, SolidColorBrush color)
		{
			scene.Children.Add(new Line
			{
				X1 = pointStart.X,
				Y1 = pointStart.Y,
				X2 = pointEnd.X,
				Y2 = pointEnd.Y,
				StrokeStartLineCap = PenLineCap.Round,
				StrokeEndLineCap = PenLineCap.Round,
				StrokeThickness = 1,
				Stroke = color
			});
		}
		private void DrawBox(Vector pointLD, Vector pointRU)
		{
			var color = Brushes.Green;
			var pointLU = new Vector { X = pointLD.X, Y = pointRU.Y };
			var pointRD = new Vector { X = pointRU.X, Y = pointLD.Y };
			DrawLine(pointLD, pointRD, color);
			DrawLine(pointLD, pointLU, color);
			DrawLine(pointLU, pointRU, color);
			DrawLine(pointRD, pointRU, color);

			DrawLine(pointLD, pointRU, color);
			DrawLine(pointLU, pointRD, color);
		}
		private void DrawArea(Vector pointLD, Vector pointRU)
		{
			var color = Brushes.Red;
			var pointLU = new Vector { X = pointLD.X, Y = pointRU.Y };
			var pointRD = new Vector { X = pointRU.X, Y = pointLD.Y };
			DrawLine(pointLD, pointRD, color);
			DrawLine(pointLD, pointLU, color);
			DrawLine(pointLU, pointRU, color);
			DrawLine(pointRD, pointRU, color);
		}
		#endregion

		#region Определения доп.типов данных
		public struct coordBox
		{
			public Vector point1 { get; set; }
			public Vector point2 { get; set; }
		}

		public enum DirectionFilling { Down, Right }

		public enum OrientationBox { Vertically, Horizontally }
		#endregion'

		#region Входные данные
		public double widthPallet { get; set; }
		public double lengthPallet { get; set; }

		public double widthBox { get; set; }
		public double lengthBox { get; set; }
		public double heightBox { get; set; }

		private Canvas scene;
		#endregion
		public LayerOnPallet(double _widthPallet, double _lengthPallet, Canvas _scene)
		{
			widthPallet = _widthPallet;
			lengthPallet = _lengthPallet;
			coordBoxes = new List<Box>();
			scene = _scene;
		}

		#region Выходные данные 
		public double errorLayer { get; set; }

		public List<Box> coordBoxes { get; set; }
        #endregion

        public BoxFactory boxFactory { get; set; }
		public void createLayer(double _widthBox, double _lengthBox, double _heightBox, BoxFactory _boxFactory,
			DirectionFilling directionFilling = DirectionFilling.Down, OrientationBox orientationBox = OrientationBox.Vertically)
		{
			widthBox = _widthBox;
			lengthBox = _lengthBox;
			heightBox = _heightBox;

            boxFactory = _boxFactory;

			 var startPoint = new Vector { X = 0, Y = 0 };
			DrawArea(startPoint, new Vector { X = startPoint.X + widthPallet, Y = startPoint.Y + lengthPallet } * koef);
			fillArea(directionFilling, orientationBox, startPoint, widthPallet, lengthPallet);
			int size = coordBoxes.Count;
		}

		#region Для ориентации короба
		private double length_belowWall;
		private double length_sideWall;
		//Назначаем длину Низа и Бока для данной ориентации 
		private void initLengths(OrientationBox orBox)
		{
			if (orBox == OrientationBox.Vertically)
			{
				length_sideWall = lengthBox;
				length_belowWall = widthBox;
			}
			if (orBox == OrientationBox.Horizontally)
			{
				length_sideWall = widthBox;
				length_belowWall = lengthBox;
			}
		}
		//Меняем ориентацию
		private OrientationBox invertOrientation(OrientationBox orBox)
		{
			if (orBox == OrientationBox.Vertically)
			{
				return OrientationBox.Horizontally;
			}
			else
			{
				return OrientationBox.Vertically;
			}
		}
		#endregion'
        private double koef = 10;
        private double allowEps = 0;
		private void fillArea(DirectionFilling dirFil, OrientationBox orBox, 
			Vector startPoint, double width, double length)
		{
			//Определение длины Низа и Бока
			initLengths(orBox);
			//Данные для следующей итерации
			DirectionFilling nextDir = dirFil;
			Vector nextStartPoint = startPoint;
			double nextWidth = 0;
			double nextLength = 0;

			int alpha;
			Vector pointBegin;
			switch (dirFil)
			{
				case DirectionFilling.Down:
					//Нужно ещё обдумать
					if (width < length_sideWall)
					{
						alpha = Convert.ToInt32(Math.Floor(length / length_sideWall));
					}
					else
					{
						alpha = CountAlpha(length, length_sideWall, length_belowWall);
					}
					if (alpha == 0)
					{
						//Вар1
						//orBox = invertOrientation(orBox);
						//initLengths(orBox);
						//alpha = Convert.ToInt32(Math.Floor(length / length_sideWall));

						//alpha = 1; вар2

						//Вар 3
						alpha = Convert.ToInt32(Math.Floor(length / length_sideWall));
					}
					//...
					pointBegin = startPoint;
					do
					{
						//Замощение по столбцу до разделяющей линии
						for (int i = 0; i < alpha; i++)
						{
							var tempCoordBox = new coordBox
							{
								point1 = pointBegin,
								point2 = new Vector { X = pointBegin.X + length_belowWall, Y = pointBegin.Y + length_sideWall}
							};

                            var box = boxFactory.GenBox();

                            box.Translate(new Vector3D(pointBegin.X, 0, pointBegin.Y));

                            if (orBox == OrientationBox.Horizontally)
                            {
                                box.RotUp();
                            }
                            
							DrawBox(tempCoordBox.point1 * koef, tempCoordBox.point2 * koef);
							//Добавление в список координат коробов
							coordBoxes.Add(box);

							pointBegin.Y += length_sideWall;
						}
						//Переход на следующий столбец
						pointBegin.X += length_belowWall;
						pointBegin.Y = startPoint.Y;
					} while ((pointBegin.X + length_belowWall) - startPoint.X <= width ||
					Math.Abs((pointBegin.X + length_belowWall) - startPoint.X - width) < allowEps);
					//Подсчёт общей ошибки
					errorLayer += (width - (pointBegin.X - startPoint.X)) * (length_sideWall * alpha);

					//Инициализация данных для следующей итерации
					nextDir = DirectionFilling.Right;
					nextStartPoint = new Vector { X = startPoint.X, Y = startPoint.Y + length_sideWall * alpha };
					nextWidth = width;
					nextLength = length - length_sideWall * alpha;

					DrawLine(new Vector { X = startPoint.X, Y = startPoint.Y + alpha * length_sideWall } * koef,
						new Vector { X = startPoint.X + width, Y = startPoint.Y + alpha * length_sideWall } * koef, Brushes.Blue);

					//Проверка на выход из рекурсии
					if ( nextLength < length_belowWall &&  Math.Abs(nextLength - length_belowWall) / length_belowWall > 0)
					{
						return;
					}
					if ( nextWidth < length_sideWall && Math.Abs(nextWidth - length_sideWall) / length_sideWall > 0)
					{
						return;
					}
					break;

				case DirectionFilling.Right:
					//Нужно ещё обдумать
					if (length < length_belowWall)
					{
						alpha = Convert.ToInt32(Math.Floor(width / length_belowWall));
					}
					else
					{
						alpha = CountAlpha(width, length_belowWall, length_sideWall);
					}	
					if (alpha == 0)
					{
						//Вар1
						//orBox = invertOrientation(orBox);
						//initLengths(orBox);
						//alpha = Convert.ToInt32(Math.Floor(width / length_belowWall));

						//alpha = 1; вар2

						//Вар 3
						alpha = Convert.ToInt32(Math.Floor(width / length_belowWall));
					}
					//...
					pointBegin = startPoint;
					do
					{
						//Замощение по строке до разделяющей линии 
						for (int i = 0; i < alpha; i++)
						{
							var tempCoordBox = new coordBox
							{
								point1 = pointBegin,
								point2 = new Vector { X = pointBegin.X + length_belowWall, Y = pointBegin.Y + length_sideWall }
							};

                            var box = boxFactory.GenBox();

                            box.Translate(new Vector3D(pointBegin.X, 0, pointBegin.Y));

                            if (orBox == OrientationBox.Horizontally)
                            {
                                box.RotUp();
                            }

                            DrawBox(tempCoordBox.point1 * koef, tempCoordBox.point2 * koef);
							//Добавление в список координат коробов
							coordBoxes.Add(box);
							pointBegin.X += length_belowWall;
						}
						//Переход на следующую строку
						pointBegin.X = startPoint.X;
						pointBegin.Y += length_sideWall;
					} while ((pointBegin.Y + length_sideWall) - startPoint.Y <= length ||
					Math.Abs((pointBegin.Y + length_sideWall) - startPoint.Y - length) < allowEps);
					//Подсчёт общей ошибки
					errorLayer += (length - (pointBegin.Y - startPoint.Y)) * (length_belowWall * alpha);

					//Инициализация данных для следующей итерации
					nextDir = DirectionFilling.Down;
					nextStartPoint = new Vector { X = startPoint.X + length_belowWall * alpha, Y = startPoint.Y };
					nextWidth = width - length_belowWall * alpha;
					nextLength = length;

					DrawLine(new Vector { X = startPoint.X + alpha * length_belowWall, Y = startPoint.Y } * koef,
						new Vector { X = startPoint.X + alpha * length_belowWall, Y = startPoint.Y + length } * koef, Brushes.Blue);

					//Проверка на выход из рекурсии
					if (nextWidth < length_sideWall && Math.Abs(nextWidth - length_sideWall) / length_sideWall > 0)
					{
						return;
					}
					if (nextLength < length_belowWall && Math.Abs(nextLength - length_belowWall) / length_belowWall > 0)
					{
						return;
					}
					break;
			}
			//Смена ориентации короба
			var nextOr = invertOrientation(orBox);
			fillArea(nextDir, nextOr, nextStartPoint, nextWidth, nextLength);
		}

		private int CountAlpha(double divisionLenth, double length_1, double length_2)
		{
			double lastError = double.PositiveInfinity;
			int resAlpha = 0;

			//Погрешность
			double eps = 0.0;

			//Вычисление максимальной альфы, с учётом погрешности 
			double doubleMaxAlpha = divisionLenth / length_1;
			int maxAlpha = Convert.ToInt32(Math.Floor(doubleMaxAlpha));
			if (Math.Ceiling(doubleMaxAlpha) - doubleMaxAlpha < eps)
			{
				maxAlpha = Convert.ToInt32(Math.Ceiling(divisionLenth / length_1));
			}

			//Вычисление альфы
			for (int alpha = 0; alpha <= maxAlpha; alpha++)
			{
				//Вычисление бетты для данной альфы с учётом погрешности
				double doubleBetta = (divisionLenth - length_1 * alpha) / length_2;
				int betta = Convert.ToInt32(Math.Floor(doubleBetta));
				if (Math.Ceiling(doubleBetta) - doubleBetta < eps)
				{
					betta = Convert.ToInt32(Math.Ceiling(doubleBetta));
				}
				//Вычисление текущей ошибки
				double tempError = Math.Abs(divisionLenth - (length_1 * alpha + length_2 * betta));

				if (tempError < lastError || (resAlpha == 0 && tempError - lastError < allowEps))
				{
					lastError = tempError;
					resAlpha = alpha;
				}
			}
			return resAlpha;
		}
	}

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

        public static readonly double BroderIndent = 0.001;

        public static readonly double CameraEPS = Math.PI / 50.0;

        public static readonly Vector CameraRotationCoefs = new Vector(1.0, 1.0);

        public static readonly SolidColorBrush UpBrush = new SolidColorBrush { Color = Colors.Red, Opacity = 1.0 };
        public static readonly SolidColorBrush FrontBush = new SolidColorBrush { Color = Colors.Yellow, Opacity = 1.0 };
        public static readonly SolidColorBrush SideBrush = new SolidColorBrush { Color = Colors.Green, Opacity = 1.0 };
        public static readonly SolidColorBrush BorderBrush = new SolidColorBrush { Color = Colors.Black, Opacity = 1.0 };

        public static readonly Material UpMaterial = new DiffuseMaterial(UpBrush);
        public static readonly Material FrontMaterial = new DiffuseMaterial(FrontBush);
        public static readonly Material SideMaterial = new DiffuseMaterial(SideBrush);
        public static readonly Material BorderMaterial = new DiffuseMaterial(BorderBrush);

    }

    public partial class MainWindow : Window
    {
        public MeshGeometry3D[] BoxToPolygons(Box[] boxes)
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

        public MeshGeometry3D CreatePolygonMesh(Point3D[] points)
        {
            MeshGeometry3D polyMesh = new MeshGeometry3D();

            for (int i = 0; i < points.Length; ++i)
            {
                polyMesh.Positions.Add(points[i]);
            }

            polyMesh.TriangleIndices.Add(0);
            polyMesh.TriangleIndices.Add(1);
            polyMesh.TriangleIndices.Add(2);
            polyMesh.TriangleIndices.Add(2);
            polyMesh.TriangleIndices.Add(3);
            polyMesh.TriangleIndices.Add(0);

            return polyMesh;
        }

        public GeometryModel3D[] CreateMarkedPolygon(Point3D[] points, Material mat, double thickness)
        {
            var markedPolygon = new GeometryModel3D[2];

            var polyMesh = CreatePolygonMesh(points);

            markedPolygon[0] = new GeometryModel3D(polyMesh, mat);

            var markMesh = new MeshGeometry3D();

            var indent = Vector3D.CrossProduct(points[1] - points[0], points[3] - points[0]);

            indent.Normalize();

            indent *= Params.BroderIndent;

            for(int i = 0; i < points.Length; ++i)
            {
                markMesh.Positions.Add(points[i] + indent);
            }

            for(int i = 0; i < 4; ++i)
            {
                int j = (i + 1) % 4;

                int k = (i + 2) % 4;


                Vector3D dir = points[k] - points[j];

                dir.Normalize();

                dir *= thickness;

                Point3D newPoint = points[j] + dir + indent;

                markMesh.Positions.Add(newPoint);

                markMesh.TriangleIndices.Add(i);
                markMesh.TriangleIndices.Add(j);
                markMesh.TriangleIndices.Add(4 + 2 * i);

                int l = (i + 4 - 1) % 4;

                dir = points[l] - points[i];

                dir.Normalize();

                dir *= thickness;

                newPoint = points[i] + dir + indent;

                markMesh.Positions.Add(newPoint);

                markMesh.TriangleIndices.Add(i);
                markMesh.TriangleIndices.Add(4 + 2 * i);
                markMesh.TriangleIndices.Add(4 + 2 * i + 1);
            }

            markedPolygon[1] = new GeometryModel3D(markMesh, Params.BorderMaterial);

            return markedPolygon;
        }

        public GeometryModel3D[] BoxToPolygons1(Box[] boxes)
        {
            List<GeometryModel3D> polygons = new List<GeometryModel3D>();

            var materials = new Material[] { Params.UpMaterial, Params.FrontMaterial, Params.SideMaterial };

            var ind = Params.BoxSidesIndecies;
            for (int i = 0; i < boxes.Length; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    int orient = boxes[i].Orient[j];
                    for (int k = 0; k < 2; ++k)
                    {

                        var markedPoly = CreateMarkedPolygon(
                            new Point3D[]
                            {
                                boxes[i].Points[ind[orient][k * 4 + 0]],
                                boxes[i].Points[ind[orient][k * 4 + 1]],
                                boxes[i].Points[ind[orient][k * 4 + 2]],
                                boxes[i].Points[ind[orient][k * 4 + 3]],
                            },
                            materials[orient],
                            0.5
                            );

                        polygons.Add(markedPoly[0]);
                        polygons.Add(markedPoly[1]);

                    }
                }
            }

            return polygons.ToArray();
        }

        public Scene MyScene { get; set; }


        public MainWindow()
        {
            InitializeComponent();
        }

        #region event handlers

        private void TestScene_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double widthPallet = 120;
            double lengthPallet = 80;
            var layer = new LayerOnPallet(widthPallet, lengthPallet, TestScene);

            double widthBox = 14;
            double lengthBox = 5;
            double heigthBox = 10;
            layer.createLayer(widthBox, lengthBox, heigthBox, new BoxFactory(widthBox, heigthBox, lengthBox),
                LayerOnPallet.DirectionFilling.Right, LayerOnPallet.OrientationBox.Vertically);

            var polygons = BoxToPolygons1(layer.coordBoxes.ToArray());

            for (int i = 0; i < polygons.Length; ++i)
            {
                polygons[i].Freeze();
                Models.Children.Add(polygons[i]);
            }

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
                if ((MyScene.LastMousePos - e.GetPosition(this)).Length > Params.MouseMoveEPS)
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

        void MouseLeave_ViewPort(object sender, MouseEventArgs e)
        {
            if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
            {
                MyScene.Camera.RestorePosition();
            }
        }
        #endregion

        private void MainWindow_Loaded_1(object sender, RoutedEventArgs e)
        {


            MyScene = new Scene(new Point3D(60, 0, 40), new Size(ViewportArea.ActualWidth, ViewportArea.ActualHeight))
            {

                UpBrush = Params.UpBrush,
                FrontBrush = Params.FrontBush,
                SideBrush = Params.SideBrush,
            };

            

            this.DataContext = MyScene;
        }
    }
}
