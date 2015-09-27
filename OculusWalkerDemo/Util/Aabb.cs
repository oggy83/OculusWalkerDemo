﻿using System;
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

		public bool IsInFrustum(Matrix wvpMatrix)
		{
			var vertices = new Vector3[8];
			vertices[0] = new Vector3(Min.X, Min.X, Min.Y);
			vertices[1] = new Vector3(Max.X, Min.X, Min.Y);
			vertices[2] = new Vector3(Max.X, Min.X, Max.Y);
			vertices[3] = new Vector3(Min.X, Min.X, Max.Y);
			vertices[4] = new Vector3(Min.X, Max.X, Min.Y);
			vertices[5] = new Vector3(Max.X, Max.X, Min.Y);
			vertices[6] = new Vector3(Max.X, Max.X, Max.Y);
			vertices[7] = new Vector3(Min.X, Max.X, Max.Y);

			foreach (var vertex in vertices)
			{
				var v = MathUtil.Transform3(vertex, wvpMatrix);
				if (-1.0 <= v.X && v.X <= 1.0 && -1.0 <= v.Y && v.Y <= 1.0 && 0.0 <= v.Z && v.Z <= 1.0)
				{
					// visible
					return true;
				}
			}

			return false;
		}

		public static Aabb Invalid()
		{
			return new Aabb(Vector3.Zero, Vector3.Zero);
		}

	}
}
