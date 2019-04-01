
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
			Canvas.Children.Add(new Line
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
		public enum DirectionFilling { Down, Right }

		public enum OrientationBox { Vertically, Horizontally }
		#endregion'

		#region Входные данные
		public double WidthPallet { get; set; }
		public double LengthPallet { get; set; }

		public double WidthBox { get; set; }
		public double LengthBox { get; set; }
		public double HeightBox { get; set; }

		private Canvas Canvas;
		#endregion
		public LayerOnPallet(double _WidthPallet, double _LengthPallet, Canvas _Canvas)
		{
			WidthPallet = _WidthPallet;
			LengthPallet = _LengthPallet;
			Canvas = _Canvas;
		}

		#region Выходные данные 
		public double ErrorLayer { get; set; }
		public int CountBoxes { get; set; }
		#endregion

		#region Для 3D генерации коробов
		private BoxFactory boxFactory;
		private bool isEnableGen = false;
		public void GenerateBoxs_On(BoxFactory _boxFactory)
		{
			Boxes = new List<Box>();
			boxFactory = _boxFactory;
			isEnableGen = true;
		}
		public void GenerateBoxs_Off()
		{
			isEnableGen = false;
		}
		public List<Box> Boxes { get; set; }
		#endregion

		private readonly double ScalingKoef = 10;
		private readonly double AllowEps = 0;

		public void CreateLayer(double _widthBox, double _lengthBox, double _heightBox,
			DirectionFilling directionFilling = DirectionFilling.Down, OrientationBox orientationBox = OrientationBox.Vertically)
		{
			WidthBox = _widthBox;
			LengthBox = _lengthBox;
			HeightBox = _heightBox;

			CountBoxes = 0;

			var startPoint = new Vector { X = 0, Y = 0 };
			
			//Убрать
			DrawArea(startPoint, new Vector { X = startPoint.X + WidthPallet, Y = startPoint.Y + LengthPallet } * ScalingKoef);

			FillArea(directionFilling, orientationBox, startPoint, WidthPallet, LengthPallet);
		}

		#region Для ориентации короба
		private double Length_belowWall;
		private double Length_sideWall;
		//Назначаем длину Низа и Бока для данной ориентации 
		private void InitLengths(OrientationBox orBox)
		{
			if (orBox == OrientationBox.Vertically)
			{
				Length_sideWall = LengthBox;
				Length_belowWall = WidthBox;
			}
			if (orBox == OrientationBox.Horizontally)
			{
				Length_sideWall = WidthBox;
				Length_belowWall = LengthBox;
			}
		}
		//Меняем ориентацию
		private OrientationBox InvertOrientation(OrientationBox orBox)
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

		private void FillArea(DirectionFilling dirFil, OrientationBox orBox, 
			Vector startPoint, double width, double length)
		{
			//Определение длины Низа и Бока для текущей ориентации 
			InitLengths(orBox);

			//Данные для следующей итерации
			DirectionFilling nextDir = dirFil;
			Vector nextStartPoint = startPoint;
			double nextWidth = 0;
			double nextLength = 0;

			int alpha = 0;
			int countBoxesOnColumn = 0;
			int countBoxesOnRow = 0;
			switch (dirFil)
			{
				case DirectionFilling.Down:
					//Нужно ещё обдумать
					if (width < Length_sideWall)
					{
						alpha = Convert.ToInt32(Math.Floor(length / Length_sideWall));
					}
					else
					{
						alpha = CountAlpha(length, Length_sideWall, Length_belowWall);
					}
					if (alpha == 0)
					{
						//Вар1
						//orBox = invertOrientation(orBox);
						//initLengths(orBox);
						//alpha = Convert.ToInt32(Math.Floor(length / Length_sideWall));

						//alpha = 1; вар2

						//Вар 3
						alpha = Convert.ToInt32(Math.Floor(length / Length_sideWall));
					}
					//...

					countBoxesOnColumn = alpha;
					countBoxesOnRow = Convert.ToInt32(Math.Floor(width / Length_belowWall));
					
					//Подсчёт общей ошибки
					ErrorLayer += (width - (countBoxesOnRow * Length_belowWall - startPoint.X)) * (Length_sideWall * alpha);

					//Инициализация данных для следующей итерации
					nextDir = DirectionFilling.Right;
					nextStartPoint = new Vector { X = startPoint.X, Y = startPoint.Y + Length_sideWall * alpha };
					nextWidth = width;
					nextLength = length - Length_sideWall * alpha;

					//Убрать
					DrawLine(new Vector { X = startPoint.X, Y = startPoint.Y + alpha * Length_sideWall } * ScalingKoef,
						new Vector { X = startPoint.X + width, Y = startPoint.Y + alpha * Length_sideWall } * ScalingKoef, Brushes.Blue);

					//Проверка на выход из рекурсии
					if ( nextLength < Length_belowWall &&  Math.Abs(nextLength - Length_belowWall) / Length_belowWall > 0)
					{
						return;
					}
					if ( nextWidth < Length_sideWall && Math.Abs(nextWidth - Length_sideWall) / Length_sideWall > 0)
					{
						return;
					}
					break;

				case DirectionFilling.Right:
					//Нужно ещё обдумать
					if (length < Length_belowWall)
					{
						alpha = Convert.ToInt32(Math.Floor(width / Length_belowWall));
					}
					else
					{
						alpha = CountAlpha(width, Length_belowWall, Length_sideWall);
					}	
					if (alpha == 0)
					{
						//Вар1
						//orBox = invertOrientation(orBox);
						//initLengths(orBox);
						//alpha = Convert.ToInt32(Math.Floor(width / Length_belowWall));

						//alpha = 1; вар2

						//Вар 3
						alpha = Convert.ToInt32(Math.Floor(width / Length_belowWall));
					}
					//...

					countBoxesOnColumn = Convert.ToInt32(Math.Floor(length / Length_sideWall));
					countBoxesOnRow = alpha;

					//Подсчёт общей ошибки
					ErrorLayer += (length - (countBoxesOnColumn * Length_sideWall - startPoint.Y)) * (Length_belowWall * alpha);

					//Инициализация данных для следующей итерации
					nextDir = DirectionFilling.Down;
					nextStartPoint = new Vector { X = startPoint.X + Length_belowWall * alpha, Y = startPoint.Y };
					nextWidth = width - Length_belowWall * alpha;
					nextLength = length;

					DrawLine(new Vector { X = startPoint.X + alpha * Length_belowWall, Y = startPoint.Y } * ScalingKoef,
						new Vector { X = startPoint.X + alpha * Length_belowWall, Y = startPoint.Y + length } * ScalingKoef, Brushes.Blue);

					//Проверка на выход из рекурсии
					if (nextWidth < Length_sideWall && Math.Abs(nextWidth - Length_sideWall) / Length_sideWall > 0)
					{
						return;
					}
					if (nextLength < Length_belowWall && Math.Abs(nextLength - Length_belowWall) / Length_belowWall > 0)
					{
						return;
					}
					break;
			}
			CountBoxes += countBoxesOnRow * countBoxesOnColumn;

			if (isEnableGen) GenerateBoxes(dirFil, orBox, startPoint, countBoxesOnRow, countBoxesOnColumn);

			//Смена ориентации короба
			var nextOr = InvertOrientation(orBox);
			FillArea(nextDir, nextOr, nextStartPoint, nextWidth, nextLength);
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

				if (tempError < lastError || (resAlpha == 0 && tempError - lastError < AllowEps))
				{
					lastError = tempError;
					resAlpha = alpha;
				}
			}
			return resAlpha;
		}

		private void GenerateBoxes(DirectionFilling dirFil, OrientationBox orBox, Vector startPoint,
			int countBoxesOnRow, int countBoxesOnColumn)
		{
			Vector pointBegin = startPoint;
			switch (dirFil)
			{
				case DirectionFilling.Down:
					for (int i = 0; i <= countBoxesOnRow; i++)
					{
						//Замощение по столбцу до разделяющей линии
						for (int j = 0; j < countBoxesOnColumn; j++)
						{
							var box = boxFactory.GenBox();
							box.Translate(new Vector3D(pointBegin.X, 0, pointBegin.Y));

							if (orBox == OrientationBox.Horizontally) box.RotUp();

							//Убрать
							var point1 = pointBegin * ScalingKoef;
							var point2 = new Vector { X = pointBegin.X + Length_belowWall, Y = pointBegin.Y + Length_sideWall } * ScalingKoef;
							DrawBox(point1, point2);

							pointBegin.Y += Length_sideWall;
						}
						//Переход на следующий столбец
						pointBegin.X += Length_belowWall;
						pointBegin.Y = startPoint.Y;
					}
					break;
				case DirectionFilling.Right:
					for (int i = 0; i < countBoxesOnColumn; i++)
					{
						//Замощение по строке до разделяющей линии 
						for (int j = 0; j < countBoxesOnRow; j++)
						{
							var box = boxFactory.GenBox();

							box.Translate(new Vector3D(pointBegin.X, 0, pointBegin.Y));

							if (orBox == OrientationBox.Horizontally) box.RotUp();

							//Убрать
							var point1 = pointBegin * ScalingKoef;
							var point2 = new Vector { X = pointBegin.X + Length_belowWall, Y = pointBegin.Y + Length_sideWall } * ScalingKoef;
							DrawBox(point1, point2);

							pointBegin.X += Length_belowWall;
						}
						//Переход на следующую строку
						pointBegin.X = startPoint.X;
						pointBegin.Y += Length_sideWall;
					}
					break;
			}
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

    public class Scene : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        private Point3D SceneCenter { get; set; }

        public MyCamera Camera { get; set; }

        public void RestoreMesh(MeshGeometry3D[] meshes)
        {
            UpMesh = meshes[0];
            FrontMesh = meshes[1];
            SideMesh = meshes[2];

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpMesh)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FrontMesh)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SideMesh)));
        }

        public Scene(Point3D _scenter)
        {
            SceneCenter = _scenter;
            Camera = new MyCamera(2.0, _scenter, new Point(Math.PI / 4, Math.PI / 4));
        }

        public Point StartMousePos { get; set; }

        public Point LastMousePos { get; set; }
    }

    public class MyCamera : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Point SavedUVPos { get; set; }

        private Point UVPos { get; set; }

        public Point3D CameraPos { get; set; }

        public Vector3D CameraOrient { get; set; }

        public double Rad { get; set; }

        public Point3D Center { get; set; }

        private readonly double MaxU = Math.PI / 2.0 - 0.01;

        public MyCamera(double _rad, Point3D _center, Point _uvpos)
        {
            Rad = _rad;
            Center = _center;
            SavedUVPos = _uvpos;
            UVPos = _uvpos;

            ShiftUVPosition(new Vector(0, 0));
        }

        public void ShiftUVPosition(Vector shift)
        {
            shift /= 100;

            UVPos = SavedUVPos - shift;

            if (UVPos.Y < 0.1)
            {
                UVPos = new Point(UVPos.X, 0.1);
            }
            else if (UVPos.Y > Math.PI / 2)
            {
                UVPos = new Point(UVPos.X, Math.PI / 2);
            }

            double RsinU = Rad * Math.Sin(UVPos.Y);
            double RcosU = Rad * Math.Cos(UVPos.Y);
            double cosV = Math.Cos(UVPos.X);
            double sinV = Math.Sin(UVPos.X);

            Vector3D sphereCoords = new Vector3D
            {
                Z = RsinU * cosV,
                X = RsinU * sinV,
                Y = RcosU
            };

            CameraPos = Center + sphereCoords;

            CameraOrient = Center - CameraPos;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CameraPos)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CameraOrient)));

        }

        public void RestorePosition()
        {
            SavedUVPos = UVPos;
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
            double widthPallet = 12;
            double lengthPallet = 8;
            var layer = new LayerOnPallet(widthPallet, lengthPallet, TestScene);

            double widthBox = 1.3;
            double lengthBox = 2.2;
            double heigthBox = 2.3;

			layer.GenerateBoxs_On(new BoxFactory(widthBox, heigthBox, lengthBox));
            layer.CreateLayer(widthBox, lengthBox, heigthBox,
                LayerOnPallet.DirectionFilling.Right, LayerOnPallet.OrientationBox.Vertically);

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
    }
}
