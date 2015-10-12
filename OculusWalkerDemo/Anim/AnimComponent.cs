using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX;

namespace Oggy
{
	/// <summary>
	/// This class repersents an animation 
	/// </summary>
	public class AnimComponent : GameEntityComponent
	{
		#region properties

		/// <summary>
		/// special overlap transform for 'head' bone matrix
		/// </summary>
		public Matrix HeadBoneMatrix;

		#endregion // properties

		public AnimComponent(AnimResource resource)
		: base(GameEntityComponent.UpdateLines.Posing)
		{
			m_resource = resource;
			HeadBoneMatrix = Matrix.Identity;
		}

		/// <summary>
		/// Update component
		/// </summary>
		/// <param name="dT">spend time [sec]</param>
		public override void Update(double dT)
		{
			if (m_skeletonC == null)
			{
				// no skeleton
				return;
			}

			var skeleton = m_skeletonC.Skeleton;
			bool isChanged = false;

			if (m_player != null)
			{
				m_player.Update(dT);
				isChanged = true;
			}

			if (HeadBoneMatrix != Matrix.Identity)
			{
				// overlap head bone
				int headBoneIndex = m_skeletonC.Skeleton.FindBoneByName("head");
				m_skeletonC.Skeleton.SetBoneMatrix(headBoneIndex, HeadBoneMatrix);
				isChanged = true;
			}

			if (isChanged)
			{
				m_skeletonC.NotifyChangeSkeleton();
			}
		}

		public AnimHandle Play(string animId)
		{
			if (m_skeletonC == null)
			{
				// no skeleton
				return AnimHandle.Invalid();
			}

			if (m_resource == null)
			{
				// no resource
				return AnimHandle.Invalid();
			}

			foreach (var action in m_resource.Data.Value.Actions)
			{
				if (action.Name == animId)
				{
					return m_player.Play(action);
				}
			}

			// not found
			Debug.Fail("anim action not found : " + animId);
			return AnimHandle.Invalid();
		}

		public override void OnAddToEntity(GameEntity entity)
		{
			base.OnAddToEntity(entity);

			m_skeletonC = Owner.FindComponent<SkeletonComponent>();
			//Debug.Assert(m_skeletonC != null, "AnimCompoment depends on SkeletonCompoment");

			if (m_skeletonC != null && m_resource.Data != null)
			{
				m_player = new LayeredAnimPlayer(m_skeletonC.Skeleton);
			}
		}

		public override void OnRemoveFromEntity(GameEntity entity)
		{
			m_skeletonC = null;

			base.OnRemoveFromEntity(entity);
		}

		#region private members

		/// <summary>
		/// skeleton compoment (optional)
		/// </summary>
		private SkeletonComponent m_skeletonC;

		/// <summary>
		/// animation resource
		/// </summary>
		private AnimResource m_resource;

		/// <summary>
		/// animation player
		/// </summary>
		private LayeredAnimPlayer m_player;

		#endregion // private members
	}
}
