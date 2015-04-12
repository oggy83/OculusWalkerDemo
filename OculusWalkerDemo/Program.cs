using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using System.Diagnostics;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Oggy
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
            bool bStereoRendering = false;// change to 'false' due to non-stereo rendering for debug
			int multiThreadCount = 4;// 1 is single thread

            HmdDevice hmd = null;
            try
            {
                // init oculus rift hmd system
                HmdSystem.Initialize();
                var hmdSys = HmdSystem.GetInstance();
                hmd = hmdSys.DetectHmd();
            }
            catch (Exception)
            {
                // failed to detect hmd 
                hmd = null;
            }

#if !DEBUG
            var configForm = new ConfigForm(hmd);
            Application.Run(configForm);
            if (configForm.HasResult())
            {
                bStereoRendering = configForm.GetResult().UseHmd;
            }
            else
            {
                // cancel 
                return;
            }
#endif
            Size resolution = new Size();
            if (hmd == null)
            {
                //resolution.Width = 1920;// Full HD
                //resolution.Height = 1080;
                resolution.Width = 1280;
                resolution.Height = 720;
            }
            else
            {
                hmd.ResetPose();
                resolution = hmd.Resolution;
            }
			
			var form = new MainForm();
			form.ClientSize = resolution;

            bool isEnableWindowMode = false;
            if (hmd == null)
            {
                isEnableWindowMode = true;
            }
            else
            {
                isEnableWindowMode = hmd.IsEnableWindowMode;
            }

			// Create Device & SwapChain
			var desc = new SwapChainDescription()
			{
				BufferCount = 2,
				ModeDescription =
					new ModeDescription(resolution.Width, resolution.Height, new Rational(0, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = isEnableWindowMode,
				OutputHandle = form.GetRenderTarget().Handle,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Sequential,
				Usage = Usage.RenderTargetOutput,
				Flags = SwapChainFlags.AllowModeSwitch,
			};

			FeatureLevel[] levels = 
			{
				FeatureLevel.Level_11_0
			};

			Device device;
			SwapChain swapChain;
#if DEBUG
			Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, levels, desc, out device, out swapChain);
#else
			Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, levels, desc, out device, out swapChain);
#endif

			// Ignore all windows events 
			var factory = swapChain.GetParent<Factory>();
			factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

			DrawSystem.Initialize(form.GetRenderTarget().Handle, device, swapChain, hmd, bStereoRendering, multiThreadCount);

			var scene = new Scene(device, swapChain, form.GetRenderTarget(), hmd, bStereoRendering, multiThreadCount);
			RenderLoop.Run(form, () => { scene.RenderFrame(); });

			// Release
			DrawSystem.Dispose();
			scene.Dispose();
			device.Dispose();
			swapChain.Dispose();
			HmdSystem.Dispose();
		}

	}
}
