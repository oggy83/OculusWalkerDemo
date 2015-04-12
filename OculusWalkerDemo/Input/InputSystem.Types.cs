using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Oggy
{
	public partial class InputSystem
	{
		/// <summary>
		/// specification of game pad stick input
		/// </summary>
		public struct ThumbSpec
		{
			public float MaxMagnitude;

			public float MinMagnitude;

			public static ThumbSpec Default()
			{
				return new ThumbSpec()
				{
					MaxMagnitude = 32000,
					MinMagnitude = 8000,
				};
			}
		}

		/// <summary>
		/// input data from game pad stick
		/// </summary>
		public struct ThumbInput
		{
			/// <summary>
			/// direction of stick inclination
			/// </summary>
			public Vector2 Direction;

			/// <summary>
			/// power of stick inclination
			/// </summary>
			public float RawMagnitude;

			/// <summary>
			/// magnitude applied ThumbSpec
			/// </summary>
			public float Magnitude;

			/// <summary>
			/// normalized magnitude [0, 1]
			/// </summary>
			public float NormalizedMagnitude;
		}

		
	}
}
