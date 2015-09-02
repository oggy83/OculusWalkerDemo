using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;

namespace Oggy
{
    public class Scene : IDisposable
    {
		private const float EntityRange = 10.0f;
		
        public Scene(Device device, SwapChain swapChain, Panel renderTarget, HmdDevice hmd, bool bStereoRendering, int multiThreadCount)
        {
            var drawSys = DrawSystem.GetInstance();

            // load textures
            var textures = new List<TextureView>(new[]
			{
				TextureView.FromFile("dot", drawSys.D3D, "Image/dot.png"),
				TextureView.FromFile("floor", drawSys.D3D, "Image/floor.jpg"),
			});
            var numTextures = new DrawSystem.TextureData[10];
            for (int i = 0; i < 10; ++i)
            {
                var name = String.Format("number_{0}", i);
                numTextures[i] = new DrawSystem.TextureData
                {
                    Resource = TextureView.FromFile(name, drawSys.D3D, String.Format("Image/{0}.png", name)),
                    UvScale = Vector2.One,
                };
            }
            textures.AddRange(numTextures.Select(item => item.Resource));
            foreach (var tex in textures)
            {
                drawSys.ResourceRepository.AddResource(tex);
            }

            // light setting
            drawSys.SetDirectionalLight(new DrawSystem.DirectionalLightData()
            {
                Direction = new Vector3(0.3f, -0.5f, 0),
                Color = new Color3(0.6f, 0.6f, 0.5f),
            });
            drawSys.AmbientColor = new Color3(0.4f, 0.45f, 0.55f);
			drawSys.FogColor = new Color3(0.3f, 0.5f, 0.8f);
            drawSys.NearClip = 0.01f;
            drawSys.FarClip = 10000.0f;

            // create number entity
            m_fps = new FpsCounter();
            m_numberEntity = new NumberEntity(new NumberEntity.InitParam()
            {
                Dot = new DrawSystem.TextureData
                {
                    Resource = drawSys.ResourceRepository.FindResource<TextureView>("dot"),
                    UvScale = Vector2.One,
                },
                Numbers = numTextures,
                Layout = Matrix.RotationYawPitchRoll(1.0f, -1.5f, 0.0f) * Matrix.Translation(1.5f, 2.5f, 4.5f)
            });

            // create player
            m_player = new PlayerEntity();
            ChrSystem.GetInstance().Player = m_player;

            // create map block
            m_mapEntities = new List<GameEntity>();
            {
                var path = "Map/m9000/m9000.blend";
                var searchPath = "Map/m9000";
                var scene = BlenderScene.FromFile(path);
				var drawModel = DrawModel.FromScene(path + "/draw", scene, searchPath);

				// load map from image file
				var bitmap = (Bitmap)Image.FromFile("Image/testmap.png");
				int mapBlockWidth = bitmap.Width;
				int mapBlockHeight = bitmap.Height;

				for (int i = 0; i < mapBlockHeight; ++i)
				{
					for (int j = 0; j < mapBlockWidth; ++j)
					{
						System.Drawing.Color color = bitmap.GetPixel(j, i);
						if (color.R == 0 && color.G == 0 && color.B == 0)	// black is wall
						{
							var v = new Vector3((j - (float)mapBlockWidth * 0.5f) * 10, 0, (i - (float)mapBlockHeight * 0.5f) * 10);
							var entity = new GameEntity("wall");

							var layoutC = new LayoutComponent();
							layoutC.Transform = Matrix.Translation(v);
							entity.AddComponent(layoutC);

							var modelC = new ModelComponent();
							modelC.ModelContext.EnableCastShadow = true;
							modelC.ModelContext.DrawModel = drawModel;
							entity.AddComponent(modelC);

							m_mapEntities.Add(entity);
						}
					}
				}

				// create floor entity
				var floorModel = DrawModel.CreateFloor(mapBlockHeight * 5, 60.0f, Vector4.Zero);
				m_floor = new ModelEntity(new ModelEntity.InitParam()
				{
					Model = floorModel,
					Texture = new DrawSystem.TextureData
					{
						Resource = drawSys.ResourceRepository.FindResource<TextureView>("floor"),
						UvScale = Vector2.One
					},
					Layout = Matrix.Identity,
					Delay = 0.0f,
					Forward = Vector3.Zero,
					Color = Color4.White,
					Speed = 1,
				});
				m_drawModelList.Add(floorModel);
				m_multiThreadCount = multiThreadCount;
				m_taskList = new List<Task>(m_multiThreadCount);
				m_taskResultList = new List<CommandList>(m_multiThreadCount);

				bitmap.Dispose();
            }

            // other settings
#if DEBUG
            CameraSystem.GetInstance().ActivateCamera(CameraSystem.FollowEntityCameraName);
            //CameraSystem.GetInstance().ActivateCamera(CameraSystem.FixedCameraName);
            //CameraSystem.GetInstance().ActivateCamera(CameraSystem.FreeCameraName);
#else
            CameraSystem.GetInstance().ActivateCamera(CameraSystem.FollowEntityCameraName);
#endif // DEBUG
        }

        public void RenderFrame()
		{
			double dt = m_fps.GetDeltaTime();

			var drawSys = DrawSystem.GetInstance();
            var cameraSys = CameraSystem.GetInstance();
            var inputSys = InputSystem.GetInstance();
            var entitySys = EntitySystem.GetInstance();

			// update fps
			{
				double avgDT = m_fps.GetAverageDeltaTime();
				string text = String.Format("FPS:{0:f2}, DeltaTime:{1:f2}ms", 1.0 / avgDT, avgDT * 1000.0f);
				m_numberEntity.SetNumber(1.0f / (float)avgDT);
			}

			if (m_multiThreadCount > 1)
			{
				Task.WaitAll(m_taskList.ToArray());
				var tmpTaskResult = new List<CommandList>(m_taskResultList);

				drawSys.BeginScene();
                var context = drawSys.GetDrawContext();

                // camera setting
                inputSys.Update(dt);
                
                drawSys.Camera = cameraSys.GetCameraData().GetViewMatrix();

                entitySys.UpdateComponents(GameEntityComponent.UpdateLines.Input, dt);
                entitySys.UpdateComponents(GameEntityComponent.UpdateLines.Behavior, dt);
                cameraSys.Update(dt);
                entitySys.UpdateComponents(GameEntityComponent.UpdateLines.Posing, dt);
                m_floor.Draw(context);
                entitySys.UpdateComponents(GameEntityComponent.UpdateLines.PreDraw, dt);
                
				// start command list generation for the next frame
				m_taskList.Clear();
				m_taskResultList.Clear();
				m_taskResultList.AddRange(Enumerable.Repeat<CommandList>(null, m_multiThreadCount));
				m_accTime += dt;
				for (int threadIndex = 0; threadIndex < m_multiThreadCount; ++threadIndex)
				{
					int resultIndex = threadIndex;

					var subThreadContext = drawSys.GetSubThreadContext(threadIndex);
					m_taskList.Add(Task.Run(() =>
					{
                        // todo : do sub-thread task
						m_taskResultList[resultIndex] = subThreadContext.FinishCommandList();
					}));
				}
               
				// draw floor
				//m_floor.Draw(context);

				foreach (var result in tmpTaskResult)
				{
					context.ExecuteCommandList(result);
				}

				m_numberEntity.Draw(context);
				drawSys.EndScene();
			}
			else
			{
                // not supported
			}

			m_fps.EndFrame();
			m_fps.BeginFrame();
		}

        /// <summary>
        /// Release all com resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var entity in m_mapEntities)
            {
                entity.Dispose();
            }
            m_player.Dispose();
			Task.WaitAll(m_taskList.ToArray());
			m_numberEntity.Dispose();
			m_floor.Dispose();
			foreach (var model in m_drawModelList)
			{
				model.Dispose();
			}
		}

		#region private members

		private FpsCounter m_fps;
		private List<DrawModel> m_drawModelList = new List<DrawModel>();
		private ModelEntity m_floor;
		private NumberEntity m_numberEntity = null;
		private double m_accTime = 0;
		private int m_multiThreadCount;
		private List<Task> m_taskList = null;
		private List<CommandList> m_taskResultList = null;
        private PlayerEntity m_player = null;
        private List<GameEntity> m_mapEntities = null;

		#endregion // private members
	}
}
