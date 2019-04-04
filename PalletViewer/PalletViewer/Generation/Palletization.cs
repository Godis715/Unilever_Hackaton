﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace PalletViewer
{
	class Layer
	{
		public int countBox;
		public int height;
		public List<Box> boxes;

		public void UpPallet(int valueUP)
		{
			foreach (var box in boxes)
			{
				box.Translate(new Vector3D { X = 0, Y = valueUP, Z = 0 });
			}
		}

		public Layer Copy()
		{
			var copyLayer = new Layer {
				countBox = countBox,
				height = height,
				boxes = new List<Box>()
			};


			copyLayer.countBox = countBox;
			copyLayer.height = height;
			copyLayer.boxes = new List<Box>();
			foreach (var item in boxes)
			{
				copyLayer.boxes.Add(item.Copy());
			}
			return copyLayer;
		}
	}

	class Pallet
	{
		public int CountPr { get; private set; }
		public int Height { get; private set; }
		public int Weight { get; private set; }
		private int CountLayer1 { get; set; }
		private int CountLayer2 { get; set; }
		private int CountLayer3 { get; set; }
		public BoxParam Box { get; private set; }
		public Layer[] Layers { get; private set; }


		public Pallet(Layer layer1, Layer layer2, Layer layer3, bool differentLayer,
			BoxParam _box, int maxHeightPal, int maxPalWeight)
		{
			Box = _box;
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
				while (Weight <= maxPalWeight && _height <= maxHeightPal)
				{
					count2 = 0;
					while (Weight <= maxPalWeight && _height <= maxHeightPal)
					{
						count3 = 0;
						while (Weight <= maxPalWeight && _height <= maxHeightPal)
						{
							if (countBox * Box.count > CountPr ||
								(countBox * Box.count == CountPr && _height < Height))
							{
								CountPr = countBox * Box.count;
								Height = _height;
								CountLayer1 = count1;
								CountLayer2 = count2;
								CountLayer3 = count3;
							}
							++count3;
							countBox += layer3.countBox;
							_height += layer3.height;
							Weight = countBox * Box.weight;
						}
						++count2;
						countBox = layer1.countBox * count1 + layer2.countBox * count2;
						_height = layer1.height * count1 + layer2.height * count2;
						Weight = countBox * Box.weight;
					}
					++count1;
					countBox = layer1.countBox * count1;
					_height = layer1.height * count1;
					Weight = countBox * Box.weight;
				}
				Weight = CountPr * (Box.weight / Box.count);
				Layers = new Layer[CountLayer1 + CountLayer2 + CountLayer3];
				for (int i = 0; i < CountLayer1; i++)
				{
					Layers[i] = layer1.Copy();
				}
				for (int i = 0; i < CountLayer2; i++)
				{
					Layers[i + CountLayer1] = layer2.Copy();
				}
				for (int i = 0; i < CountLayer3; i++)
				{
					Layers[i + CountLayer1 + CountLayer2] = layer3.Copy();
				}
				return;
			}
			// if not different layer
			CountLayer1 = Math.Min(maxPalWeight / (Box.weight * layer1.countBox),
				maxPalWeight / layer1.height);
			CountLayer2 = Math.Min(maxPalWeight / (Box.weight * layer2.countBox),
				maxPalWeight / layer2.height);
			CountLayer3 = Math.Min(maxPalWeight / (Box.weight * layer3.countBox),
				maxPalWeight / layer3.height);


			if (CountLayer1 * layer1.countBox < CountLayer2 * layer2.countBox)
			{
				CountLayer1 = 0;
				int countBox = CountLayer2 * layer2.countBox;
				Height = CountLayer2 * layer2.height;
				Weight = countBox * Box.weight;
				CountPr = countBox * Box.count;

				Layers = new Layer[CountLayer2];
				for (int i = 0; i < CountLayer2; i++)
				{
					Layers[i] = layer2.Copy();
				}
			}
			else
			{
				CountLayer2 = 0;
				int countBox = CountLayer1 * layer1.countBox;
				Height = CountLayer1 * layer1.height;
				Weight = countBox * Box.weight;
				CountPr = countBox * Box.count;

				Layers = new Layer[CountLayer1];
				for (int i = 0; i < CountLayer1; i++)
				{
					Layers[i] = layer1.Copy();
				}
			}

			if (CountPr < CountLayer3 * layer3.countBox)
			{
				CountLayer1 = 0;
				CountLayer2 = 0;
				int countBox = CountLayer3 * layer3.countBox;
				Height = CountLayer3 * layer3.height;
				Weight = countBox * Box.weight;
				CountPr = countBox * Box.count;

				Layers = new Layer[CountLayer3];
				for (int i = 0; i < CountLayer3; i++)
				{
					Layers[i] = layer3.Copy();
				}
			}
			else
			{
				CountLayer3 = 0;
			}
		}

		public Pallet(Layer layer1, Layer layer2, bool differentLayer,
	BoxParam _box, int maxHeightPal, int maxPalWeight)
		{
			CountLayer3 = 0;
			Box = _box;
			if (differentLayer)
			{
				// temp counts layers
				int count1 = 0;
				int count2 = 0;
				// temp var
				int countBox = 0;
				int _height = 0;
				// brute force - perebor
				while (Weight <= maxPalWeight && _height <= maxHeightPal)
				{
					count2 = 0;
					while (Weight <= maxPalWeight && _height <= maxHeightPal)
					{
						if (countBox * Box.weight > CountPr ||
							(countBox * Box.weight == CountPr && _height < Height))
						{
							CountPr = countBox * Box.weight;
							Height = _height;
							CountLayer1 = count1;
							CountLayer2 = count2;
						}
						++count2;
						countBox += layer2.countBox;
						_height += layer2.height;
						Weight = countBox * Box.weight;
					}
					++count1;
					countBox = layer1.countBox * count1;
					_height += layer1.height * count1;
					Weight = countBox * Box.weight;
				}
				Weight = CountPr * (Box.weight / Box.count);
				Layers = new Layer[CountLayer1 + CountLayer2];
				for (int i = 0; i < CountLayer1; i++)
				{
					Layers[i] = layer1.Copy();
				}
				for (int i = 0; i < CountLayer2; i++)
				{
					Layers[i + CountLayer1] = layer2.Copy();
				}
				return;
			}
			// if not different layer
			CountLayer1 = Math.Min(maxPalWeight / (Box.weight * layer1.countBox),
				maxPalWeight / layer1.height);
			CountLayer2 = Math.Min(maxPalWeight / (Box.weight * layer2.countBox),
				maxPalWeight / layer2.height);
			
			if (CountLayer1 * layer1.countBox < CountLayer2 * layer2.countBox)
			{
				CountLayer1 = 0;
				int countBox = CountLayer2 * layer2.countBox;
				Height = CountLayer2 * layer1.height;
				CountPr = countBox * Box.count;
				Weight = countBox * Box.weight;
				Layers = new Layer[CountLayer2];
				for (int i = 0; i < CountLayer2; i++)
				{
					Layers[i] = layer2.Copy();
				}
			}
			else
			{
				CountLayer2 = 0;
				int countBox = CountLayer1 * layer1.countBox;
				Height = CountLayer1 * layer1.height;
				CountPr = countBox * Box.count;
				Weight = Box.weight * countBox;
				Layers = new Layer[CountLayer1];
				for (int i = 0; i < CountLayer1; i++)
				{
					Layers[i] = layer1.Copy();
				}
			}
		}

		public Pallet(Layer layer1, BoxParam _box, int maxHeightPal, int maxPalWeight)
		{
			CountLayer2 = 0;
			CountLayer3 = 0;
			Box = _box;

			CountLayer1 = Math.Min(maxPalWeight / (Box.weight * layer1.countBox),
				maxPalWeight / layer1.height);
			int countBox = CountLayer1 * layer1.countBox;
			Height = CountLayer1 * layer1.height;
			Weight = Box.weight * countBox;
			CountPr = countBox * Box.count;

			Layers = new Layer[CountLayer1];
			for (int i = 0; i < CountLayer1; i++)
			{
				Layers[i] = layer1.Copy();
			}
		}

		public void ShiftLayers()
		{
			if (Layers.Length == 0)
			{
				throw new Exception("Invalid pallet");
			}
			int shift = Layers[0].height;
			for (int i = 1; i < Layers.Length; i++)
			{
				Layers[i].UpPallet(shift);
				shift += Layers[i].height;
			}
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

			int i = 0;
			Pallet pallet = null;
			while (i < boxes.Length && pallet == null)
			{
				pallet = GetPalletByBox(boxes[i]);

				
				++i;
			}
			

			for (; i < boxes.Length; i++)
			{
				var newPallet = GetPalletByBox(boxes[i]);
				if (newPallet != null && 
					(newPallet.CountPr > pallet.CountPr ||
					(newPallet.CountPr == pallet.CountPr && newPallet.Height < pallet.Height)))
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
			var layers = layerMaker.CreateLayers(box.x, box.y, box.z, 
				new BoxFactory(box.x, box.y, box.z));
		
			switch (layers.Length)
			{
				case 1:
					{
						var pallet = new Pallet(layers[0], box, heightPal, maxPalWeight);
						if (pallet.Layers.Length > 0)
						{
							return pallet;
						}
						else
						{
							return null;
						}
					}
				case 2:
					{
						var pallet = new Pallet(layers[0], layers[1], differentLayer,
							box, heightPal, maxPalWeight);
						if (pallet.Layers.Length > 0)
						{
							return pallet;
						}
						else
						{
							return null;
						}
					}
				case 3:
					{
						var pallet = new Pallet(layers[0], layers[1], layers[2], differentLayer,
							box, heightPal, maxPalWeight);
						if (pallet.Layers.Length > 0)
						{
							return pallet;
						}
						else
						{
							return null;
						}
					}
				default:
					return null;
			}
		}
	}
}
