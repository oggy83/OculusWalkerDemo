using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oggy
{
	/// <summary>
	/// This class represents a skeleton.
	/// </summary>
	/// <remarks>animation object should have this compoment</remarks>
	public class SkeletonComponent : GameEntityComponent
	{
		/// <summary>
		/// get/set skeleton
		/// </summary>
		public Skeleton Skeleton { get; set; }

		public event EventHandler OnSkeletonChanged = (sender, e) => {};

		public SkeletonComponent(DrawSystem.BoneData[] boneArray)
		: base(GameEntityComponent.UpdateLines.Invalid)
		{
			Skeleton = new Skeleton(boneArray);
		}

		public void NotifyChangeSkeleton()
		{
			OnSkeletonChanged(this, new EventArgs());
		}
	}
}
