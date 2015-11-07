﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oggy
{
	public partial class GameEntityComponent
	{
		public enum UpdateLines
		{
			/// <summary>
			/// Not Update
			/// </summary>
			Invalid = 0,	

            /// <summary>
            /// process input phase
            /// </summary>
			Input,

            Behavior,
			PostBehavior,

			Posing,

			PreDraw,

			Draw,
		}
	}
}
