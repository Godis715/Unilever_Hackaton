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
		
		#region Определения доп.типов данных
		private enum DirectionFilling { Down, Right }

		private enum OrientationBox { Vertically, Horizontally }
		#endregion'

		#region Входные данные
		private double WidthPallet;
		private double LengthPallet;

		private double Length_belowWall;
		private double Length_sideWall;

		#endregion
		
		public LayerOnPallet(double _WidthPallet, double _LengthPallet)
		{
			WidthPallet = _WidthPallet;
			LengthPallet = _LengthPallet;
		}

		#region Для 3D генерации коробов
		private BoxFactory boxFactory;
		private Box templateBox;
		private List<Box> Boxes;
		#endregion

		private readonly double AllowEps = 0;

		#region Данные о сгенерированном слое
		private double ErrorLayer { get; set; }
		private int CountBoxes { get; set; }
		#endregion

		#region Генерация слоя
		private void ClearResult()
		{
			CountBoxes = 0;
			ErrorLayer = 0;
			Boxes = new List<Box>();
		}

		private void CheckValue(ref double tempErrorLayer, ref int tempMaxCountBox, ref List<Box> tempBoxes)
		{
			if (tempMaxCountBox < CountBoxes || (tempMaxCountBox == CountBoxes && ErrorLayer < tempErrorLayer))
			{
				tempMaxCountBox = CountBoxes;
				tempErrorLayer = ErrorLayer;
				tempBoxes = Boxes;
			}
		}

		private void FillInfoAboutLayer(ref Layer layer)
		{
			var tempBoxes = new List<Box>();
			var tempErrorLayer = double.PositiveInfinity;
			var tempMaxCountBox = 0;

			var startPoint = new Vector { X = 0, Y = 0 };
			ClearResult();
			FillArea(DirectionFilling.Down, OrientationBox.Horizontally, startPoint, WidthPallet, LengthPallet);
			CheckValue(ref tempErrorLayer, ref tempMaxCountBox, ref tempBoxes);

			ClearResult();
			FillArea(DirectionFilling.Down, OrientationBox.Vertically, startPoint, WidthPallet, LengthPallet);
			CheckValue(ref tempErrorLayer, ref tempMaxCountBox, ref tempBoxes);

			ClearResult();
			FillArea(DirectionFilling.Right, OrientationBox.Horizontally, startPoint, WidthPallet, LengthPallet);
			CheckValue(ref tempErrorLayer, ref tempMaxCountBox, ref tempBoxes);

			ClearResult();
			FillArea(DirectionFilling.Right, OrientationBox.Vertically, startPoint, WidthPallet, LengthPallet);
			CheckValue(ref tempErrorLayer, ref tempMaxCountBox, ref tempBoxes);

			layer.countBox = tempMaxCountBox;
			layer.boxes = tempBoxes;
		}
		#endregion

		public Layer[] CreateLayers(int _heightBox, int _widthBox, int _lengthBox, BoxFactory _boxFactory)
		{
			boxFactory = _boxFactory;

			templateBox = boxFactory.GenBox();

			var layers = new Stack<Layer>();

			Length_belowWall = _widthBox;
			Length_sideWall = _lengthBox;
			var layer1 = new Layer();
			layer1.height = _heightBox;
			FillInfoAboutLayer(ref layer1);
			if (layer1.countBox != 0)
			{
				layers.Push(layer1);
			}

			// two param is equal and one is different
			if ((_widthBox == _lengthBox && _heightBox != _widthBox) ||
				(_widthBox != _lengthBox && _heightBox == _widthBox))
			{
				Length_belowWall = _heightBox;
				Length_sideWall = _widthBox;
				var layer2 = new Layer();
				layer2.height = _lengthBox;

				//boxFactory.SetOrient(new int[] { 2, 1, 0 });

				templateBox = boxFactory.GenBox();

				templateBox.RotSide();

				FillInfoAboutLayer(ref layer2);
				if (layer2.countBox != 0)
				{
					layers.Push(layer2);
				}
			}

			// trhee params is equal
			else if (_widthBox != _lengthBox && _heightBox != _widthBox)
			{
				Length_belowWall = _widthBox;
				Length_sideWall = _heightBox;
				var layer2 = new Layer();
				layer2.height = _lengthBox;
				//boxFactory.SetOrient(new int[] { 1, 2, 0 });
				templateBox = boxFactory.GenBox();

				templateBox.RotSide();

				FillInfoAboutLayer(ref layer2);
				if (layer2.countBox != 0)
				{
					layers.Push(layer2);
				}

				Length_belowWall = _heightBox;
				Length_sideWall = _lengthBox;
				var layer3 = new Layer();
				layer3.height = (int)_widthBox;

				//boxFactory.SetOrient(new int[] { 1, 0,2 });

				templateBox = boxFactory.GenBox();

				templateBox.RotFront();

				FillInfoAboutLayer(ref layer3);
                if (layer3.countBox != 0)
				{
					layers.Push(layer3);
				}
			}
			return layers.ToArray();
		}

		#region Для ориентации короба
		private double LengthTemp_belowWall;
		private double LengthTemp_sideWall;
		//Назначаем длину Низа и Бока для данной ориентации 
		private void InitLengths(OrientationBox orBox)
		{
			if (orBox == OrientationBox.Vertically)
			{
				LengthTemp_sideWall = Length_sideWall;
				LengthTemp_belowWall = Length_belowWall;
			}
			if (orBox == OrientationBox.Horizontally)
			{
				LengthTemp_sideWall = Length_belowWall;
				LengthTemp_belowWall = Length_sideWall;
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

		#region Main функции
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
					if (width < LengthTemp_sideWall)
					{
						alpha = Convert.ToInt32(Math.Floor(length / LengthTemp_sideWall));
					}
					else
					{
						alpha = CountAlpha(length, LengthTemp_sideWall, LengthTemp_belowWall);
					}
					if (alpha == 0)
					{
						//Вар1
						//orBox = invertOrientation(orBox);
						//initLengths(orBox);
						//alpha = Convert.ToInt32(Math.Floor(length / LengthTemp_sideWall));

						//alpha = 1; вар2

						//Вар 3
						alpha = Convert.ToInt32(Math.Floor(length / LengthTemp_sideWall));
					}
					//...

					countBoxesOnColumn = alpha;
					countBoxesOnRow = Convert.ToInt32(Math.Floor(width / LengthTemp_belowWall));

					//Подсчёт общей ошибки
					ErrorLayer += (width - (countBoxesOnRow * LengthTemp_belowWall - startPoint.X)) * (LengthTemp_sideWall * alpha);

					//Инициализация данных для следующей итерации
					nextDir = DirectionFilling.Right;
					nextStartPoint = new Vector { X = startPoint.X, Y = startPoint.Y + LengthTemp_sideWall * alpha };
					nextWidth = width;
					nextLength = length - LengthTemp_sideWall * alpha;
                    
					break;

				case DirectionFilling.Right:
					//Нужно ещё обдумать
					if (length < LengthTemp_belowWall)
					{
						alpha = Convert.ToInt32(Math.Floor(width / LengthTemp_belowWall));
					}
					else
					{
						alpha = CountAlpha(width, LengthTemp_belowWall, LengthTemp_sideWall);
					}
					if (alpha == 0)
					{
						//Вар1
						//orBox = invertOrientation(orBox);
						//initLengths(orBox);
						//alpha = Convert.ToInt32(Math.Floor(width / LengthTemp_belowWall));

						//alpha = 1; вар2

						//Вар 3
						alpha = Convert.ToInt32(Math.Floor(width / LengthTemp_belowWall));
					}
					//...

					countBoxesOnColumn = Convert.ToInt32(Math.Floor(length / LengthTemp_sideWall));
					countBoxesOnRow = alpha;

					//Подсчёт общей ошибки
					ErrorLayer += (length - (countBoxesOnColumn * LengthTemp_sideWall - startPoint.Y)) * (LengthTemp_belowWall * alpha);

					//Инициализация данных для следующей итерации
					nextDir = DirectionFilling.Down;
					nextStartPoint = new Vector { X = startPoint.X + LengthTemp_belowWall * alpha, Y = startPoint.Y };
					nextWidth = width - LengthTemp_belowWall * alpha;
					nextLength = length;
                    
					break;
			}
			CountBoxes += countBoxesOnRow * countBoxesOnColumn;

			GenerateBoxes(dirFil, orBox, startPoint, countBoxesOnRow, countBoxesOnColumn);

			//Проверка на выход из рекурсии
			if (nextLength < LengthTemp_belowWall && Math.Abs(nextLength - LengthTemp_belowWall) / LengthTemp_belowWall > 0)
			{
				return;
			}
			if (nextWidth < LengthTemp_sideWall && Math.Abs(nextWidth - LengthTemp_sideWall) / LengthTemp_sideWall > 0)
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
							//var box = boxFactory.GenBox();
							var box = templateBox.Copy();
							box.Translate(new Vector3D(pointBegin.X, 0, pointBegin.Y));

							if (orBox == OrientationBox.Horizontally) box.RotUp();

                            Boxes.Add(box);
                            
							pointBegin.Y += LengthTemp_sideWall;
						}
						//Переход на следующий столбец
						pointBegin.X += LengthTemp_belowWall;
						pointBegin.Y = startPoint.Y;
					}
					break;
				case DirectionFilling.Right:
					for (int i = 1; i <= countBoxesOnColumn; i++)
					{
						//Замощение по строке до разделяющей линии 
						for (int j = 1; j <= countBoxesOnRow; j++)
						{
							//var box = boxFactory.GenBox();
							var box = templateBox.Copy();

							box.Translate(new Vector3D(pointBegin.X, 0, pointBegin.Y));

							if (orBox == OrientationBox.Horizontally) box.RotUp();
                            
                            Boxes.Add(box);

                            pointBegin.X += LengthTemp_belowWall;
						}
						//Переход на следующую строку
						pointBegin.X = startPoint.X;
						pointBegin.Y += LengthTemp_sideWall;
					}
					break;
			}
		}
		#endregion
	}
}