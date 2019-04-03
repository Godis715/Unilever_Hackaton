using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletViewer
{
	class Layer
	{
		public int countPr;
		public int height;
	}

	class Pallet
	{
		public int countPr;
		public int height;
		public int weight;
		public int CountLayer1 { get; private set; }
		public int CountLayer2 { get; private set; }
		public int CountLayer3 { get; private set; }
		public Layer layer1;
		public Layer layer2;
		public Layer layer3;
		public BoxParam box;


		public Pallet(Layer _layer1, Layer _layer2, Layer _layer3, bool differentLayer,
			BoxParam _box, int maxHeightPal, int maxPalWeight)
		{
			layer1 = _layer1;
			layer2 = _layer2;
			layer3 = _layer3;
			box = _box;
			int weightPr = box.weight / box.count;
			if (differentLayer)
			{
				// temp counts layers
				int count1 = 0;
				int count2 = 0;
				int count3 = 0;
				// temp var
				int _countPr = 0;
				int _height = 0;
				// brute force - perebor
				while (weight <= maxPalWeight && _height <= maxHeightPal)
				{
					count2 = 0;
					while (weight <= maxPalWeight && _height <= maxHeightPal)
					{
						count3 = 0;
						while (weight <= maxPalWeight && _height <= maxHeightPal)
						{
							if (_countPr > countPr ||
								(_countPr == countPr && _height < height))
							{
								countPr = _countPr;
								height = _height;
								CountLayer1 = count1;
								CountLayer2 = count2;
								CountLayer3 = count3;
							}
							++count3;
							_countPr += layer3.countPr;
							_height += layer3.height;
							weight = _countPr * weightPr;
						}
						++count2;
						_countPr = layer1.countPr * count1 + layer2.countPr * count2;
						_height += layer1.height * count1 + layer2.height * count2;
						weight = _countPr * weightPr;
					}
					++count1;
					_countPr = layer1.countPr * count1;
					_height += layer1.height * count1
;
					weight = _countPr * weightPr;
				}
			}
			// if not different layer
			CountLayer1 = Math.Min(maxPalWeight / (weightPr * layer1.countPr),
				maxPalWeight / layer1.height);
			CountLayer2 = Math.Min(maxPalWeight / (weightPr * layer2.countPr),
				maxPalWeight / layer2.height);
			CountLayer3 = Math.Min(maxPalWeight / (weightPr * layer3.countPr),
				maxPalWeight / layer3.height);
			countPr = CountLayer1 * layer1.countPr;
			if (CountLayer1 * layer1.countPr < CountLayer2 * layer2.countPr)
			{
				CountLayer1 = 0;
				countPr = CountLayer2 * layer2.countPr;
				height = CountLayer2 * layer2.height;
				weight = weightPr * countPr;
			}
			else
			{
				CountLayer2 = 0;
				countPr = CountLayer1 * layer1.countPr;
				height = CountLayer1 * layer1.height;
				weight = weightPr * countPr;
			}

			if (countPr < CountLayer3 * layer3.countPr)
			{
				CountLayer1 = 0;
				CountLayer2 = 0;
				countPr = CountLayer3 * layer3.countPr;
				height = CountLayer3 * layer3.height;
				weight = weightPr * countPr;
			}
			else
			{
				CountLayer3 = 0;
			}
		}

		public Pallet(Layer _layer1, Layer _layer2, bool differentLayer,
	BoxParam _box, int maxHeightPal, int maxPalWeight)
		{
			layer1 = _layer1;
			layer2 = _layer2;
			layer3 = _layer2;
			CountLayer3 = 0;
			box = _box;
			int weightPr = box.weight / box.count;
			if (differentLayer)
			{
				// temp counts layers
				int count1 = 0;
				int count2 = 0;
				// temp var
				int _countPr = 0;
				int _height = 0;
				// brute force - perebor
				while (weight <= maxPalWeight && _height <= maxHeightPal)
				{
					count2 = 0;
					while (weight <= maxPalWeight && _height <= maxHeightPal)
					{
						if (_countPr > countPr ||
							(_countPr == countPr && _height < height))
						{
							countPr = _countPr;
							height = _height;
							CountLayer1 = count1;
							CountLayer2 = count2;
						}
						++count2;
						_countPr += layer3.countPr;
						_height += layer3.height;
						weight = _countPr * weightPr;
					}
					++count1;
					_countPr = layer1.countPr * count1;
					_height += layer1.height * count1
;
					weight = _countPr * weightPr;
				}
			}
			// if not different layer
			CountLayer1 = Math.Min(maxPalWeight / (weightPr * layer1.countPr),
				maxPalWeight / layer1.height);
			CountLayer2 = Math.Min(maxPalWeight / (weightPr * layer2.countPr),
				maxPalWeight / layer2.height);
			
			if (CountLayer1 * layer1.countPr < CountLayer2 * layer2.countPr)
			{
				CountLayer1 = 0;
				countPr = CountLayer2 * layer2.countPr;
				height = CountLayer2 * layer1.height;
				weight = weightPr * countPr;

			}
			else
			{
				CountLayer2 = 0;
				countPr = CountLayer1 * layer1.countPr;
				height = CountLayer1 * layer1.height;
				weight = weightPr * countPr;
			}
		}

		public Pallet(Layer _layer1, BoxParam _box, int maxHeightPal, int maxPalWeight)
		{
			layer1 = _layer1;
			layer2 = _layer1;
			layer3 = _layer1;
			CountLayer2 = 0;
			CountLayer3 = 0;
			box = _box;
			int weightPr = box.weight / box.count;
			// if not different layer
			CountLayer1 = Math.Min(maxPalWeight / (weightPr * layer1.countPr),
				maxPalWeight / layer1.height);
			countPr = CountLayer1 * layer1.countPr;
			height = CountLayer1 * layer1.height;
			weight = weightPr * countPr;
		}
	}

	class Palletization
	{
		private readonly int lengthPr;
		private readonly int widhtPr;
		private readonly int heightPr;
		private readonly int weightPr;

		private readonly int lengthPal;
		private readonly int widhtPal;
		private readonly int heightPal;
		private readonly int maxPalWeight;

		private readonly int minCount;
		private readonly int maxCount;
		private readonly int maxBoxWeight;
		private readonly int ratioBox;

		private readonly string sizeProduct;
		private readonly bool differentLayer;

		private readonly LayerOnPallet layerMaker;

		public Palletization(int _lengthPr, int _widhtPr, int _heightPr, int _weightPr,
			int _lengthPal, int _widhtPal, int _heightPal, int _maxPalWeight,
			int _minCount, int _maxCount, int _maxBoxWeight, int _ratioBox,
			string _sizeProduct, bool _differentLayer)
		{
			var arr = new int[] { _lengthPr, _widhtPr, _heightPr };
			Array.Sort(arr);
			//lengthPr = Math.Max(Math.Max(_lengthPr, _widhtPr), _heightPr);
			lengthPr = arr[2];
			widhtPr = arr[1];
			heightPr = arr[0];
			weightPr = _weightPr;

			lengthPal = _lengthPal;
			widhtPal = _widhtPal;
			heightPal = _heightPal;
			maxPalWeight = _maxPalWeight * 1000;
			layerMaker = new LayerOnPallet(widhtPal, lengthPal);

			minCount = Math.Min(_minCount, _maxCount);
			maxCount = Math.Max(_minCount, _maxCount);
			maxBoxWeight = _maxBoxWeight * 1000;
			ratioBox = _ratioBox;

			sizeProduct = _sizeProduct;
			differentLayer = _differentLayer;
		}

		public Pallet GetPallet()
		{
			var boxesGen = new BoxGenerator(lengthPr, widhtPr, heightPr, weightPr,
			minCount, maxCount, maxBoxWeight, ratioBox, sizeProduct);

			var boxes = boxesGen.GetBoxes();

			
			return GetPalletByBox(boxes[0]);
		}

		public Pallet[] GetBeterPallet(int n)
		{

			return new Pallet[2];
		}

		private Pallet GetPalletByBox(BoxParam box)
		{
			if (box.x == box.y || box.y == box.z)
			// if 2 sides in box is equal
			{
				layerMaker.CreateLayer((double)box.x, (double)box.y);
				var layer1 = new Layer
				{
					countPr = layerMaker.CountBoxes,
					height = box.z
				};
				layerMaker.CreateLayer((double)box.y, (double)box.z);
				var layer2 = new Layer
				{
					countPr = layerMaker.CountBoxes,
					height = box.x
				};

				return new Pallet(layer1, layer2, differentLayer,
					box, heightPal, maxPalWeight);
			}
			else if (box.x == box.y && box.y == box.z)
			// box is cube
			{
				layerMaker.CreateLayer((double)box.x, (double)box.y);
				var layer = new Layer
				{
					countPr = layerMaker.CountBoxes,
					height = box.z
				};

				return new Pallet(layer, box, heightPal, maxPalWeight);
			}
			else
			{
				layerMaker.CreateLayer((double)box.x, (double)box.y);
				var layer1 = new Layer
				{
					countPr = layerMaker.CountBoxes,
					height = box.z
				};
				layerMaker.CreateLayer((double)box.y, (double)box.z);
				var layer2 = new Layer
				{
					countPr = layerMaker.CountBoxes,
					height = box.x
				};
				layerMaker.CreateLayer((double)box.z, (double)box.x);
				var layer3 = new Layer
				{
					countPr = layerMaker.CountBoxes,
					height = box.y
				};

				return new Pallet(layer1, layer2, layer3, differentLayer,
					box, heightPal, maxPalWeight);
			}
			
		}
	}
}
