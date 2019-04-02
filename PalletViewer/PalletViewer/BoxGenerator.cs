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
	}

	public class Triple
	{
		public int x;
		public int y;
		public int z;
	}

	public class BoxGenerator
	{
		private int lenght;
		private int widht;
		private int height;
		private int minCount;
		private int maxCount;
		private int weight;

		public BoxGenerator(int l, int w, int h, int min, int max,
			int wgt, int minW, int maxW)
		{
			lenght = l;
			widht = w;
			height = h;
			minCount = (int)Math.Ceiling((double)minW / wgt);
			minCount = Math.Max(min, minCount);
			maxCount = (int)Math.Floor((double)maxW / wgt);
			maxCount = Math.Min(max, maxCount);
		}

		public BoxGenerator(int l, int w, int h, int min, int max, int wgt)
		{
			lenght = l;
			widht = w;
			height = h;
			minCount = min;
			maxCount = max;
			weight = wgt;
		}

		public BoxParam[] GetBoxes()
		{
			var result = new BoxParam[0];
			for (int i = minCount; i <= maxCount; ++i)
			{

				result = result.Concat(GetBoxesByCount(i)).ToArray();
			}
			return result;
		}

		private BoxParam[] GetBoxesByCount(int count)
		{
			var result = new BoxParam[0];
			var divs = GetDividers(count);
			//Console.Write("dividers: ");
			//foreach (var item in divs)
			//{
			//	Console.Write(item.ToString() + "; ");
			//}
			//Console.WriteLine();
			int len = divs.Length;
			for (int i = 0; divs[i] * divs[i] * divs[i] <= count; ++i)
			{
				for (int j = i; divs[i] * divs[j] * divs[j] <= count; j++)
				{
					if (count % (divs[i] * divs[j]) == 0)
					{
						var triple = new Triple
						{
							x = divs[i],
							y = divs[j],
							z = count / (divs[i] * divs[j])
						};
						var arr = GetBoxesByTriple(triple);
						//Console.WriteLine("Triple: " + triple.x.ToString() + "; "
						//	+ triple.y.ToString() + "; " + triple.z.ToString());
						//foreach (var item in arr)
						//{
						//	Console.WriteLine("Box: " + item.x.ToString() + "; "
						//		+ item.y.ToString() + "; " + item.z.ToString());
						//}
						result = result.Concat(arr).ToArray();
					}
				}
			}
			return result;
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
				var box2 = new BoxParam
				{
					x = triple.x * lenght,
					y = triple.x * widht,
					z = triple.z * height,
					count = countProduct,
					weight = weightBox
				};
				var box3 = new BoxParam
				{
					x = triple.x * height,
					y = triple.x * lenght,
					z = triple.z * widht,
					count = countProduct,
					weight = weightBox
				};
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
				var box2 = new BoxParam
				{
					x = triple.x * lenght,
					y = triple.z * widht,
					z = triple.z * height,
					count = countProduct,
					weight = weightBox
				};
				var box3 = new BoxParam
				{
					x = triple.x * height,
					y = triple.z * lenght,
					z = triple.z * widht,
					count = countProduct,
					weight = weightBox
				};
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
				var box2 = new BoxParam
				{
					x = triple.x * widht,
					y = triple.y * height,
					z = triple.z * lenght,
					count = countProduct,
					weight = weightBox
				};
				var box3 = new BoxParam
				{
					x = triple.x * lenght,
					y = triple.y * widht,
					z = triple.z * height,
					count = countProduct,
					weight = weightBox
				};
				var box4 = new BoxParam
				{
					x = triple.x * widht,
					y = triple.y * lenght,
					z = triple.z * height,
					count = countProduct,
					weight = weightBox
				};
				var box5 = new BoxParam
				{
					x = triple.x * height,
					y = triple.y * lenght,
					z = triple.z * widht,
					count = countProduct,
					weight = weightBox
				};
				var box6 = new BoxParam
				{
					x = triple.x * lenght,
					y = triple.y * height,
					z = triple.z * widht,
					count = countProduct,
					weight = weightBox
				};

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

		public BoxParam[] ValidationSize(BoxParam[] triples, int ratio)
		{
			var result = new Stack<BoxParam>();
			foreach (var item in triples)
			{
				var arr = new SortedSet<int>();
				arr.Add(item.x);
				arr.Add(item.y);
				arr.Add(item.z);

				if (arr.Max / arr.Min <= ratio)
				{
					result.Push(item);
				}
			}
			return result.ToArray();
		}
	}
}