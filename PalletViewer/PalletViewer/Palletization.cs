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
		public BoxParam box;
		public Pallet(Layer layer1, Layer layer2, Layer layer3, bool differentLayer,
			BoxParam _boxm, int heightPal, int maxPalWeight)
		{

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
