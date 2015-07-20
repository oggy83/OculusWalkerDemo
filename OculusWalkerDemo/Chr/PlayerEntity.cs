using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Oggy
{
	/// <summary>
	/// player entitiy
	/// </summary>
	public class PlayerEntity : GameEntity
	{
		public PlayerEntity()
			: base("player")
		{
			var entitySys = EntitySystem.GetInstance();
			var path = "Chr/c9000/test.blend";
            var searchPath = "Chr/c9000";
			var scene = BlenderScene.FromFile(path);
			if (scene != null)
			{
				var drawModel = DrawModel.FromScene(path + "/draw", scene, searchPath);
                var animRes = AnimResource.FromBlenderScene(path + "/anim", scene);
				//var debugModel = DrawModel.CreateTangentFrame(path + "/debug", scene);

                if (drawModel.BoneArray.Length != 0)
                {
                    var skeletonC = new SkeletonComponent(drawModel.BoneArray);
                    AddComponent(skeletonC);
                }

				var layoutC = new LayoutComponent();
				layoutC.Transform = Matrix.Translation(0, 0, 0);
				AddComponent(layoutC);
				
				//var markerC = new ModelMarkerComponent(scene);
				//AddComponent(markerC);

				var animC = new AnimComponent(animRes);
				AddComponent(animC);
                animC.Play("Pause");

                var behaviorC = new ChrBehaviorComponent();
                AddComponent(behaviorC);

				var inputC = new PlayerInputComponent();
				AddComponent(inputC);

				var modelC = new ModelComponent();
				modelC.ModelContext.EnableCastShadow = true;
				modelC.ModelContext.DrawModel = drawModel;
				//modelC.ModelContext.DebugModel = debugModel;
				AddComponent(modelC);
			}
		}
	}
}
