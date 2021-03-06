﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using SharpDX;
using System.Diagnostics;

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
			var mapSys = MapSystem.GetInstance();

			//var path = "Chr/c9000/test.blend";
            //var searchPath = "Chr/c9000";
			var path = "Chr/c9100/c9100.blend";
			var searchPath = "Chr/c9100";
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
				layoutC.Transform = Matrix.Identity;
				AddComponent(layoutC);
				
				//var markerC = new ModelMarkerComponent(scene);
				//AddComponent(markerC);

				var modelC = new ModelComponent(GameEntityComponent.UpdateLines.Draw);
				modelC.ModelContext.EnableCastShadow = true;
				modelC.ModelContext.DrawModel = drawModel;
				//modelC.ModelContext.DebugModel = debugModel;
				AddComponent(modelC);

				var animC = new AnimComponent(animRes);
				AddComponent(animC);

                var behaviorC = new ChrBehaviorComponent();
                AddComponent(behaviorC);

				var minimapC = new MinimapComponent();
				AddComponent(minimapC);

				MapLocation startLocation = mapSys.GetStartInfo();
				//var inputC = new GodViewInputComponent();
                var inputC = new FpsInputComponent(startLocation);
				AddComponent(inputC);

				
			}
		}
	}
}
