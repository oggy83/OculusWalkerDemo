using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Oggy
{
	public struct Aabb
	{
		public Vector3 Min;
		public Vector3 Max;

		public Aabb(Vector3 min, Vector3 max)
			: this()
		{
			Min = min;
			Max = max;
		}

		public bool IsInvalid()
		{
			return Min.IsZero && Max.IsZero;
		}

		/// <summary>
		/// update aabb so that aabb contains the given point
		/// </summary>
		/// <param name="point"></param>
		public void AddPoint(Vector3 point)
		{
			if (IsInvalid())
			{
				Min = point;
				Max = point;
				return;
			}

			if (Min.X > point.X) Min.X = point.X;
			if (Min.Y > point.Y) Min.Y = point.Y;
			if (Min.X > point.Z) Min.Z = point.Z;

			if (Max.X < point.X) Max.X = point.X;
			if (Max.Y < point.Y) Max.Y = point.Y;
			if (Max.X < point.Z) Max.Z = point.Z;
		}

		public static Aabb Invalid()
		{
			return new Aabb(Vector3.Zero, Vector3.Zero);
		}

	}
}
