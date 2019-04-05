//using Microsoft.Analytics.Interfaces;
//using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PalletViewer
{
	public class BoxParam
	{
		public int x;
		public int y;
		public int z;

		public int count;
		public int weight;

		// sort x, y, z
		public void NormalizedOriental()
		{
			var arr = new int[] { x, y, z };
			Array.Sort(arr);
			x = arr[0];
			y = arr[1];
			z = arr[2];
		}
	}

	public class Triple
	{
		public int x;
		public int y;
		public int z;
	}

	public class BoxGenerator
	{
		private readonly int lenght;
		private readonly int widht;
		private readonly int height;
		private readonly int weight;

		private readonly int minCount;
		private readonly int maxCount;
		private readonly int ratioBox;
		private readonly string sizeProduct;

		public BoxGenerator(int _lengthPr, int _widhtPr, int _heightPr, int _weightPr,
			int _minCount, int _maxCount, int _maxBoxWeight, int _ratioBox,
			string _sizeProduct)
		{
			lenght = _lengthPr;
			widht = _widhtPr;
			height = _heightPr;
			weight = _weightPr;
			minCount = _minCount;
			maxCount = (int)Math.Floor((double)_maxBoxWeight / _weightPr);
			maxCount = Math.Min(_maxCount, maxCount);
			ratioBox = _ratioBox;
			sizeProduct = _sizeProduct;
		}

		public BoxParam[] GetBoxes()
		{
			var result = new BoxParam[0];
			for (int i = minCount; i <= maxCount; ++i)
			{
				result = result.Concat(GetBoxesByCount(i)).ToArray();
			}

			return ValidationSize(result, ratioBox);
		}

		private BoxParam[] GetBoxesByCount(int count)
		{
			var result = new BoxParam[0];
			var divs = GetDividers(count);
			
			for (int i = 0; i < divs.Length &&
				divs[i] * divs[i] * divs[i] <= count; ++i)
			{
				for (int j = i; j < divs.Length &&
					divs[i] * divs[j] * divs[j] <= count; j++)
				{
					if (sizeProduct == "average" &&
						divs[j] == 1 &&
						divs[i] == 1)
					{
						continue;
					}
					if (sizeProduct == "small" &&
						(divs[j] == 1 ||
						divs[i] == 1))
					{
						continue;
					}
					if (count % (divs[i] * divs[j]) == 0)
					{
						var triple = new Triple
						{
							x = divs[i],
							y = divs[j],
							z = count / (divs[i] * divs[j])
						};
						result = result.Concat(GetBoxesByTriple(triple)).ToArray();
					}
				}
			}
			return Unique(result);
		}

		private BoxParam[] Unique(BoxParam[] boxes)
		{
			var result = new Stack<BoxParam>();
			for (int i = 0; i < boxes.Length; i++)
			{
				bool isUnique = true;
				for (int j = i + 1; j < boxes.Length; j++)
				{
					if (boxes[i].x == boxes[j].x &&
						boxes[i].y == boxes[j].y &&
						boxes[i].z == boxes[j].z)
					{
						isUnique = false;
						break;
					}
				}
				if (isUnique)
				{
					result.Push(boxes[i]);
				}
			}
			return result.ToArray();
		}

		private BoxParam[] GetBoxesByTriple(Triple triple)
		{
			int countProduct = triple.x * triple.y * triple.z;
			int weightBox = weight * countProduct;
			if (triple.x == triple.y && triple.y == triple.z)
			{
				var box = new BoxParam
				{
					x = triple.x * height,
					y = triple.x * widht,
					z = triple.x * lenght,
					count = countProduct,
					weight = weightBox
				};
				box.NormalizedOriental();
				var result = new BoxParam[1];
				result[0] = (box);
				return result;
			}
			else if (triple.x == triple.y)
			{
				var box1 = new BoxParam
				{
					x = triple.x * height,
					y = triple.x * widht,
					z = triple.z * lenght,
					count = countProduct,
					weight = weightBox
				};
				box1.NormalizedOriental();
				var box2 = new BoxParam
				{
					x = triple.x * lenght,
					y = triple.x * widht,
					z = triple.z * height,
					count = countProduct,
					weight = weightBox
				};
				box2.NormalizedOriental();
				var box3 = new BoxParam
				{
					x = triple.x * height,
					y = triple.x * lenght,
					z = triple.z * widht,
					count = countProduct,
					weight = weightBox
				};
				box3.NormalizedOriental();

				var result = new BoxParam[3];
				result[0] = (box1);
				result[1] = (box2);
				result[2] = (box3);
				return result;
			}
			else if (triple.z == triple.y)
			{
				var box1 = new BoxParam
				{
					x = triple.x * height,
					y = triple.z * widht,
					z = triple.z * lenght,
					count = countProduct,
					weight = weightBox
				};
				box1.NormalizedOriental();
				var box2 = new BoxParam
				{
					x = triple.x * lenght,
					y = triple.z * widht,
					z = triple.z * height,
					count = countProduct,
					weight = weightBox
				};
				box2.NormalizedOriental();
				var box3 = new BoxParam
				{
					x = triple.x * height,
					y = triple.z * lenght,
					z = triple.z * widht,
					count = countProduct,
					weight = weightBox
				};
				box3.NormalizedOriental();

				var result = new BoxParam[3];
				result[0] = (box1);
				result[1] = (box2);
				result[2] = (box3);
				return result;
			}
			else
			{
				var box1 = new BoxParam
				{
					x = triple.x * height,
					y = triple.y * widht,
					z = triple.z * lenght,
					count = countProduct,
					weight = weightBox
				};
				box1.NormalizedOriental();
				var box2 = new BoxParam
				{
					x = triple.x * widht,
					y = triple.y * height,
					z = triple.z * lenght,
					count = countProduct,
					weight = weightBox
				};
				box2.NormalizedOriental();
				var box3 = new BoxParam
				{
					x = triple.x * lenght,
					y = triple.y * widht,
					z = triple.z * height,
					count = countProduct,
					weight = weightBox
				};
				box3.NormalizedOriental();
				var box4 = new BoxParam
				{
					x = triple.x * widht,
					y = triple.y * lenght,
					z = triple.z * height,
					count = countProduct,
					weight = weightBox
				};
				box4.NormalizedOriental();
				var box5 = new BoxParam
				{
					x = triple.x * height,
					y = triple.y * lenght,
					z = triple.z * widht,
					count = countProduct,
					weight = weightBox
				};
				box5.NormalizedOriental();
				var box6 = new BoxParam
				{
					x = triple.x * lenght,
					y = triple.y * height,
					z = triple.z * widht,
					count = countProduct,
					weight = weightBox
				};
				box6.NormalizedOriental();

				var result = new BoxParam[6];
				result[0] = (box1);
				result[1] = (box2);
				result[2] = (box3);
				result[3] = (box4);
				result[4] = (box5);
				result[5] = (box6);
				return result;
			}
		}

		private int[] GetDividers(int number)
		{
			var result = new Queue<int>();
			for (int i = 1; i <= number; ++i)
			{
				if (number % i == 0)
				{
					result.Enqueue(i);
				}
			}
			return result.ToArray();
		}

		private BoxParam[] ValidationSize(BoxParam[] boxes, int ratio)
		{
			var result = new Stack<BoxParam>();
			foreach (var item in boxes)
			{
				int min = Math.Min(Math.Min(item.x, item.y), item.z);
				int max = Math.Max(Math.Max(item.x, item.y), item.z);
				if (max / min <= ratio)
				{
					result.Push(item);
				}
			}
			return result.ToArray();
		}
	}
}