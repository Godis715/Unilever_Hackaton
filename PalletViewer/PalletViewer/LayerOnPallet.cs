using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace PalletViewer
{
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

		private readonly double ScalingKoef = 20;
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
			if (isEnableGen)
			{
				DrawArea(startPoint, new Vector { X = startPoint.X + WidthPallet, Y = startPoint.Y + LengthPallet } * ScalingKoef);
			}

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
					if (isEnableGen)
					{
						DrawLine(new Vector { X = startPoint.X, Y = startPoint.Y + alpha * Length_sideWall } * ScalingKoef,
						new Vector { X = startPoint.X + width, Y = startPoint.Y + alpha * Length_sideWall } * ScalingKoef, Brushes.Blue);
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

					if (isEnableGen)
					{
						DrawLine(new Vector { X = startPoint.X + alpha * Length_belowWall, Y = startPoint.Y } * ScalingKoef,
						new Vector { X = startPoint.X + alpha * Length_belowWall, Y = startPoint.Y + length } * ScalingKoef, Brushes.Blue);
					}
					break;
			}
			CountBoxes += countBoxesOnRow * countBoxesOnColumn;

			if (isEnableGen) GenerateBoxes(dirFil, orBox, startPoint, countBoxesOnRow, countBoxesOnColumn);

			//Проверка на выход из рекурсии
			if (nextLength < Length_belowWall && Math.Abs(nextLength - Length_belowWall) / Length_belowWall > 0)
			{
				return;
			}
			if (nextWidth < Length_sideWall && Math.Abs(nextWidth - Length_sideWall) / Length_sideWall > 0)
			{
				return;
			}

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
					for (int i = 1; i <= countBoxesOnRow; i++)
					{
						//Замощение по столбцу до разделяющей линии
						for (int j = 1; j <= countBoxesOnColumn; j++)
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
					for (int i = 1; i <= countBoxesOnColumn; i++)
					{
						//Замощение по строке до разделяющей линии 
						for (int j = 1; j <= countBoxesOnRow; j++)
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
}