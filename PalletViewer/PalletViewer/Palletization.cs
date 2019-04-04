using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletViewer
{
	class Layer
	{
		public int countBox;
		public int height;
		public List<Box> boxes;
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
			if (differentLayer)
			{
				// temp counts layers
				int count1 = 0;
				int count2 = 0;
				int count3 = 0;
				// temp var
				int countBox = 0;
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
							if (countBox * box.count > countPr ||
								(countBox * box.count == countPr && _height < height))
							{
								countPr = countBox * box.count;
								height = _height;
								CountLayer1 = count1;
								CountLayer2 = count2;
								CountLayer3 = count3;
							}
							++count3;
							countBox += layer3.countBox;
							_height += layer3.height;
							weight = countBox * box.weight;
						}
						++count2;
						countBox = layer1.countBox * count1 + layer2.countBox * count2;
						_height += layer1.height * count1 + layer2.height * count2;
						weight = countBox * box.weight;
					}
					++count1;
					countBox = layer1.countBox * count1;
					_height += layer1.height * count1;
					weight = countBox * box.weight;
				}
				weight = countPr * (box.weight / box.count);
				return;
			}
			// if not different layer
			CountLayer1 = Math.Min(maxPalWeight / (box.weight * layer1.countBox),
				maxPalWeight / layer1.height);
			CountLayer2 = Math.Min(maxPalWeight / (box.weight * layer2.countBox),
				maxPalWeight / layer2.height);
			CountLayer3 = Math.Min(maxPalWeight / (box.weight * layer3.countBox),
				maxPalWeight / layer3.height);


			if (CountLayer1 * layer1.countBox < CountLayer2 * layer2.countBox)
			{
				CountLayer1 = 0;
				int countBox = CountLayer2 * layer2.countBox;
				height = CountLayer2 * layer2.height;
				weight = countBox * box.weight;
				countPr = countBox * box.count;
			}
			else
			{
				CountLayer2 = 0;
				int countBox = CountLayer1 * layer1.countBox;
				height = CountLayer1 * layer1.height;
				weight = countBox * box.weight;
				countPr = countBox * box.count;
			}

			if (countPr < CountLayer3 * layer3.countBox)
			{
				CountLayer1 = 0;
				CountLayer2 = 0;
				int countBox = CountLayer3 * layer3.countBox;
				height = CountLayer3 * layer3.height;
				weight = countBox * box.weight;
				countPr = countBox * box.count;
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
			if (differentLayer)
			{
				// temp counts layers
				int count1 = 0;
				int count2 = 0;
				// temp var
				int countBox = 0;
				int _height = 0;
				// brute force - perebor
				while (weight <= maxPalWeight && _height <= maxHeightPal)
				{
					count2 = 0;
					while (weight <= maxPalWeight && _height <= maxHeightPal)
					{
						if (countBox * box.weight > countPr ||
							(countBox * box.weight == countPr && _height < height))
						{
							countPr = countBox * box.weight;
							height = _height;
							CountLayer1 = count1;
							CountLayer2 = count2;
						}
						++count2;
						countBox += layer3.countBox;
						_height += layer3.height;
						weight = countBox * box.weight;
					}
					++count1;
					countBox = layer1.countBox * count1;
					_height += layer1.height * count1;
					weight = countBox * box.weight;
				}
				weight = countPr * (box.weight / box.count);
				return;
			}
			// if not different layer
			CountLayer1 = Math.Min(maxPalWeight / (box.weight * layer1.countBox),
				maxPalWeight / layer1.height);
			CountLayer2 = Math.Min(maxPalWeight / (box.weight * layer2.countBox),
				maxPalWeight / layer2.height);
			
			if (CountLayer1 * layer1.countBox < CountLayer2 * layer2.countBox)
			{
				CountLayer1 = 0;
				int countBox = CountLayer2 * layer2.countBox;
				height = CountLayer2 * layer1.height;
				countPr = countBox * box.count;
				weight = countBox * box.weight;
				
			}
			else
			{
				CountLayer2 = 0;
				int countBox = CountLayer1 * layer1.countBox;
				height = CountLayer1 * layer1.height;
				countPr = countBox * box.count;
				weight = box.weight * countBox;
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

			CountLayer1 = Math.Min(maxPalWeight / (box.weight * layer1.countBox),
				maxPalWeight / layer1.height);
			int countBox = CountLayer1 * layer1.countBox;
			height = CountLayer1 * layer1.height;
			weight = box.weight * countBox;
			countPr = countBox * box.count;
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
			lengthPr = arr[2];
			widhtPr = arr[1];
			heightPr = arr[0];
			weightPr = _weightPr;

			lengthPal = Math.Max(_lengthPal, _widhtPal);
			widhtPal = Math.Min(_lengthPal, _widhtPal);
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
			if (boxes.Length == 0)
			{
				return null;
			}
			var pallet = GetPalletByBox(boxes[0]);

			for (int i = 1; i < boxes.Length; i++)
			{
				var newPallet = GetPalletByBox(boxes[i]);
				if (newPallet != null && newPallet.countPr > pallet.countPr)
				{
					pallet = newPallet;
				}
			}

			return pallet;
		}

		public Pallet[] GetBeterPallet(int n)
		{

			return new Pallet[2];
		}

		private Pallet GetPalletByBox(BoxParam box)
		{
            layerMaker.GenerateBoxs_On(new BoxFactory(box.z, box.y, box.x));

			var layers = layerMaker.CreateLayers((double)box.x, (double)box.y, (double)box.z);
            
			switch (layers.Length)
			{
				case 1:
					{
						return new Pallet(layers[0], box, heightPal, maxPalWeight);
					}
				case 2:
					{
						return new Pallet(layers[0], layers[1], differentLayer,
					box, heightPal, maxPalWeight);
					}
				case 3:
					{
						return new Pallet(layers[0], layers[1], layers[2], differentLayer,
					box, heightPal, maxPalWeight);
					}
				default:
					return null;
			}
		}
	}
}
