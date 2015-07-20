using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;


namespace Oggy
{
	public static class BlenderUtil
	{
		/// <summary>
		/// change coords system from blender to sharpDX
		/// </summary>
		/// <param name="v">blender vector</param>
		/// <returns>sharpDX vector</returns>
		public static Vector3 ChangeCoordsSystem(Vector3 v)
		{
			return new Vector3(v.X, v.Z, v.Y);
		}

		/// <summary>
		/// change coords system from blender to sharpDX
		/// </summary>
		/// <param name="v">blender vector</param>
		/// <returns>sharpDX vector</returns>
		public static Vector4 ChangeCoordsSystem(Vector4 v)
		{
			return new Vector4(v.X, v.Z, v.Y, v.W);
		}

		/// <summary>
		/// change coords system from blender to sharpDX
		/// </summary>
		/// <param name="q">blender vector</param>
		/// <returns>sharpDX vector</returns>
		public static Quaternion ChangeCoordsSystem(Quaternion q)
		{
            return q;
		}

		/// <summary>
		/// change coords system from blender to sharpDX
		/// </summary>
		/// <param name="v">blender matrix</param>
		/// <returns>sharpDX matrix</returns>
		public static Matrix ChangeCoordsSystem(Matrix m)
		{
			return new Matrix()
			{
				M11 = m.M11,
				M12 = m.M13,
				M13 = m.M12,
				M14 = m.M14,
				M21 = m.M21,
				M22 = m.M23,
				M23 = m.M22,
				M24 = m.M24,
				M31 = m.M31,
				M32 = m.M33,
				M33 = m.M32,
				M34 = m.M34,
				M41 = m.M41,
				M42 = m.M43,
				M43 = m.M42,
				M44 = m.M44,
			};
		}

		/// <summary>
		/// get length of array safely
		/// </summary>
		/// <typeparam name="Type">type of elment</typeparam>
		/// <param name="a">array</param>
		/// <returns>length</returns>
		public static int GetLengthOf<Type>(Type[] a)
		{
			return a == null ? 0 : a.Length;
		}

		private static Regex RnaRegex = new Regex(@"^pose\.bones\[""(\w+)""\]\.(\w+)$");

		/// <summary>
		/// parse rnaPath to bone name and property name
		/// </summary>
		/// <param name="rnaPath">rna path string</param>
		/// <param name="boneName">bone name</param>
		/// <param name="propertyName">property name</param>
		/// <returns>
		/// true : succeeded
		/// false : failed
		/// </returns>
		/// <remarks>
		/// Example
		/// input : pose.bones["Top"].rotation_quaternion
		/// output : Top, rotation_quaternion
		/// </remarks>
		public static bool ParseRnaPath(string rnaPath, ref string boneName, ref string propertyName)
		{
			var match = RnaRegex.Match(rnaPath);
			if (match.Success)
			{
				boneName = match.Groups[1].Value;
				propertyName = match.Groups[2].Value;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
