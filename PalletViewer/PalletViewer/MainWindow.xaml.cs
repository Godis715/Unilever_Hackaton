﻿
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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
			coordBoxes = new List<coordBox>();
			scene = _scene;
		}

		#region Выходные данные 
		public double errorLayer { get; set; }

		public List<coordBox> coordBoxes { get; set; }
		#endregion

		public void createLayer(double _widthBox, double _lengthBox, double _heightBox,
			DirectionFilling directionFilling = DirectionFilling.Down, OrientationBox orientationBox = OrientationBox.Vertically)
		{
			widthBox = _widthBox;
			lengthBox = _lengthBox;
			heightBox = _heightBox;

			 var startPoint = new Vector { X = 10, Y = 10 };
			DrawArea(startPoint, new Vector { X = startPoint.X + widthPallet, Y = startPoint.Y + lengthPallet });
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
							DrawBox(tempCoordBox.point1, tempCoordBox.point2);
							//Добавление в список координат коробов
							coordBoxes.Add(tempCoordBox);
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

					DrawLine(new Vector { X = startPoint.X, Y = startPoint.Y + alpha * length_sideWall },
						new Vector { X = startPoint.X + width, Y = startPoint.Y + alpha * length_sideWall }, Brushes.Blue);

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
							DrawBox(tempCoordBox.point1, tempCoordBox.point2);
							//Добавление в список координат коробов
							coordBoxes.Add(tempCoordBox);
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

					DrawLine(new Vector { X = startPoint.X + alpha * length_belowWall, Y = startPoint.Y },
						new Vector { X = startPoint.X + alpha * length_belowWall, Y = startPoint.Y + length }, Brushes.Blue);

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

	class Box
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }

    public class Box
    {
        private Point3D S { get; set; }

        private Vector3D Dim { get; set; }

        private Point3D[] points;

        public Point3D[] Points
        {
            get
            {
                if (points == null)
                {
                    var E = S + Dim;

                    points = new Point3D[]
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
                }

                return points;
            }
            set
            {
                points = value;
            }
        }

        public int[] Orient { get; set; }

        public Box(Vector3D dim)
        {
            Dim = dim; S = new Point3D(0, 0, 0);
            Orient = new int[] { 0, 1, 2 };
        }

        public Box(Vector3D dim, Point3D start)
        {
            Dim = dim; S = start;
            Orient = new int[] { 0, 1, 2 };
        }

        public void Translate(Vector3D shift)
        {
            S += shift;
        }

        public void RotUp()
        {
            Dim = new Vector3D(Dim.Z, Dim.Y, Dim.X);
            Helper.Swap(ref Orient[1], ref Orient[2]);

            points = null;
        }

        public void RotFront()
        {
            Dim = new Vector3D(Dim.Y, Dim.X, Dim.Z);
            Helper.Swap(ref Orient[0], ref Orient[2]);

            points = null;
        }

        public void RotSide()
        {
            Dim = new Vector3D(Dim.X, Dim.Z, Dim.Y);
            Helper.Swap(ref Orient[0], ref Orient[1]);

            points = null;
        }
    }

    public class BoxFactory
    {
        private Vector3D Dimensions { get; set; }

        private Point3D Start { get; set; }

        // params of standart box
        public BoxFactory(double sWidth, double sHeight, double sLength)
        {
            Dimensions = new Vector3D(sWidth, sHeight, sLength);

            Start = new Point3D(0, 0, 0);
        }

        public Box GenBox()
        {
            return new Box(Dimensions, Start);
        }
    }

    public class Scene
    {
        #region brushes
        public SolidColorBrush UpBrush { get; set; }

        public SolidColorBrush FrontBrush { get; set; }

        public SolidColorBrush SideBrush { get; set; }
        #endregion

        #region meshes
        public MeshGeometry3D UpMesh { get; set; }

        public MeshGeometry3D FrontMesh { get; set; }

        public MeshGeometry3D SideMesh { get; set; }
        #endregion

        public Point3D SceneCenter { get; set; }

        public Point3D CameraPosition { get; set; }

        public Vector3D CameraOrient { get; set; }

        public void RestoreCamera()
        {
            CameraOrient = SceneCenter - CameraPosition;
        }

        private Vector LastMousePos { get; set; }
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

        void MouseDownEventHandler()
        {

        }

        /*class Camera
        {
            public Point UVPosition { get; set; }
            public double Rad { get; set; }
            public Point3D Center {  }
            pub
        }*/

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

            /* mesh.Positions.Add(new Point3D(0, 1, 0));
            mesh.Positions.Add(new Point3D(0, 0, 0));
            mesh.Positions.Add(new Point3D(-1, 0, 0));
            mesh.Positions.Add(new Point3D(-1, 1, 0));

<<<<<<<
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3); */
=======
            //mesh.TriangleIndices.Add(1);
            //mesh.TriangleIndices.Add(0);
            //mesh.TriangleIndices.Add(2);
            //mesh.TriangleIndices.Add(2);
            //mesh.TriangleIndices.Add(0);
            //mesh.TriangleIndices.Add(3);
            
            //DiffuseMaterial meshMat = new DiffuseMaterial((Brush)Application.Current.Resources["patternBrush"]);
>>>>>>>

            MyScene = new Scene {

                UpBrush = new SolidColorBrush { Color = Colors.Red, Opacity = 1.0 },
                FrontBrush = new SolidColorBrush { Color = Colors.Yellow, Opacity = 1.0 },
                SideBrush = new SolidColorBrush { Color = Colors.Green, Opacity = 1.0 },

                UpMesh = meshes[0],
                FrontMesh = meshes[1],
                SideMesh = meshes[2],

                CameraPosition = new Point3D(5, 5, 5)
            };

            MyScene.RestoreCamera();

            this.DataContext = MyScene;
        }
		private void TestScene_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			double widthPallet = 1200 / 2;
			double lengthPallet = 800 / 2;
			var layer = new LayerOnPallet(widthPallet, lengthPallet, TestScene);

			double widthBox = 206 / 2;
			double lengthBox = 331 / 2;
			double heigthBox = 10.6;
			layer.createLayer(widthBox, lengthBox, heigthBox,
				LayerOnPallet.DirectionFilling.Right, LayerOnPallet.OrientationBox.Vertically);
		}
	}
}
