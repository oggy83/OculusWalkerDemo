using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace Oggy
{
	public static class AnimUtil
	{
		/// <summary>
		/// Interpolate two vector3 values
		/// </summary>
		/// <param name="a">value1</param>
		/// <param name="b">value2</param>
		/// <param name="t">amount (0.0, 1.0)</param>
		/// <returns>result of interpolation</returns>
		/// <remarks>
		/// if amount == 0.0, then this function returns value1,
		/// if amount == 1.0, then this function returns value2.
		/// </remarks>
		static public Vector3 IpoVec3(Vector3 a, Vector3 b, float t)
		{
			return Vector3.Lerp(a, b, t);
		}

		public static Quaternion IpoQuat(Quaternion a, Quaternion b, float t)
		{
			return Quaternion.Slerp(a, b, t);
		}

		public static float IpoFloat(float a, float b, float t)
		{
			return a * (1 - t) + b * t;
		}
	}
}
