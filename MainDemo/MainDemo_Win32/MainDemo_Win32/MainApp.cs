using System;
using System.Collections.Generic;
using Reign.Core;
using Reign.Video;
using Reign.Video.API;

using ShaderMaterials.Shaders;

namespace MainDemo_Win32
{
	class MainApp :
	#if WIN32
	WinFormApplication
	#endif
	{
		bool inited, loaded;
		RootDisposable root;
		VideoI video;
		ViewPortI viewPort;
		Camera camera;

		RasterizerStateI rasterizerState;
		DepthStencilStateI depthState;
		BlendStateI blendState;
		SamplerStateI samplerState;

		Model model;
		InstanceModel instanceModel;
		QuickDrawI qd;
		QuickDraw3ColorMaterial qdMaterial;

		public MainApp()
		{
			var desc = new ApplicationDesc()
			{
				Name = "Main Demo"
			};
			Init(desc);
		}

		public override void Shown()
		{
			try
			{
				root = new RootDisposable();

				VideoTypes videoType;
				video = Video.Init(VideoTypes.D3D11, out videoType, root, this, DepthStencilFormats.Defualt, true);
				viewPort = ViewPortAPI.New(video, new Point2(), video.BackBufferSize);
				float cameraDis = 10;
				camera = new Camera(viewPort, new Vector3(cameraDis, cameraDis, cameraDis), new Vector3(), new Vector3(cameraDis, cameraDis+1, cameraDis));

				//DiffuseSolidColorMaterial.Init(video, "Data/", video.FileTag, ShaderVersions.Max, null);
				//DiffuseSolidColorMaterial.ApplyInstanceConstantsCallback = applyTransform;
				DiffuseTextureMaterial.Init(video, "Data/", video.FileTag, ShaderVersions.Max, null);
				DiffuseTextureMaterial.ApplyObjectMeshCallback = applyTransform;
				DiffuseTextureMaterial.ApplyInstanceObjectMeshCallback = applyInstanceTransform;

				var materialTypes = new Dictionary<string,Type>();
				materialTypes.Add("Material", typeof(DiffuseTextureMaterial));
				var value3Types = new List<MaterialFieldBinder>();
				var textureTypes = new List<MaterialFieldBinder>();
				textureTypes.Add(new MaterialFieldBinder("Material", "Diffuse", "Diffuse"));
				model = new Model(video, "Data/untitled2.rm", "Data/", materialTypes, null, null, value3Types, null, textureTypes, null, 0, modelLoaded);

				QuickDraw3ColorMaterial.Init(video, "Data/", video.FileTag, ShaderVersions.Max, quickDrawShaderLoaded);
				qdMaterial = new QuickDraw3ColorMaterial();

				rasterizerState = RasterizerStateAPI.New(video, RasterizerStateDescAPI.New(RasterizerStateTypes.Solid_CullCW));
				depthState = DepthStencilStateAPI.New(video, DepthStencilStateDescAPI.New(DepthStencilStateTypes.ReadWrite_Less));
				blendState = BlendStateAPI.New(video, BlendStateDescAPI.New(BlendStateTypes.None));
				samplerState = SamplerStateAPI.New(video, SamplerStateDescAPI.New(SamplerStateTypes.Linear_Wrap));

				inited = true;
			}
			catch (Exception e)
			{
				Message.Show("Error", e.Message);
				dispose();
			}
		}

		private void modelLoaded(object sender, bool succeeded)
		{
			if (succeeded) instanceModel = new InstanceModel((Model)sender);
		}

		private void quickDrawShaderLoaded(object sender, bool succeeded)
		{
			if (succeeded) qd = QuickDrawAPI.New(video, QuickDraw3ColorMaterial.BufferLayoutDesc);
		}

		private void dispose()
		{
			inited = false;
			if (root != null)
			{
				root.Dispose();
				root = null;
			}
		}

		public override void Closing()
		{
			dispose();
		}

		public override void Update(Time time)
		{
			if (!loaded) return;

			//camera.RotateAroundLookPositionWorld(0, time.Delta, 0);
			//rot += time.Delta;

			instanceModel.Play(time);
		}

		float rot;
		private void applyTransform(DiffuseTextureMaterial material, ObjectMesh mesh)
		{
			material.Transform = Matrix4.FromAffineTransform(mesh.RotationMatrix, mesh.Scale, mesh.Position);//.RotateAroundAxisY(rot)
		}

		private void applyInstanceTransform(DiffuseTextureMaterial material, InstanceObjectMesh mesh)
		{
			material.Transform = Matrix4.FromAffineTransform(mesh.RotationMatrix, mesh.Scale, mesh.Position);//.RotateAroundAxisY(rot)
		}

		public override void Render(Time time)
		{
			if (!inited) return;

			video.Update();
			var e = Loader.UpdateLoad();
			if (e != null)
			{
				Message.Show("Loading Error", e.Message);
				dispose();
				return;
			}
			if (Loader.ItemsRemainingToLoad != 0) return;
			if (!loaded)
			{
				loaded = true;
				return;
			}

			rasterizerState.Enable();
			depthState.Enable();
			blendState.Enable();
			samplerState.Enable(0);

			video.EnableRenderTarget();
			video.ClearColorDepth(0, .5f, .5f, 1);

			viewPort.Apply();
			camera.Apply();
			//DiffuseSolidColorMaterial.Camera = camera.TransformMatrix;
			//DiffuseSolidColorMaterial.LightColor = new Vector4(1);
			//DiffuseSolidColorMaterial.LightDirection = -new Vector3(1, .5f, .3f).Normalize();
			DiffuseTextureMaterial.Camera = camera.TransformMatrix;
			DiffuseTextureMaterial.LightColor = new Vector4(1);
			DiffuseTextureMaterial.LightDirection = -new Vector3(1, .5f, .3f).Normalize();
			model.Render();
			instanceModel.PlaySpeed = 5;
			instanceModel.Render();

			qdMaterial.Enable();
			QuickDraw3ColorMaterial.Camera = camera.TransformMatrix;
			qdMaterial.Apply();
			qd.StartLines();
				foreach (var o in instanceModel.Objects)
				{
					var armObj = o as InstanceObjectArmature;
					if (armObj == null) continue;

					//var arm = armObj.Armature;
					foreach (var bone in armObj.Bones)
					{
						//if (bone.Parent == null) continue;

						qd.Color(1, 0, 0, 1);
						qd.Pos(bone.Position);
						qd.Pos(bone.Position + bone.RotationMatrix.X);

						qd.Color(0, 1, 0, 1);
						qd.Pos(bone.Position);
						qd.Pos(bone.Position + bone.RotationMatrix.Y);

						qd.Color(0, 0, 1, 1);
						qd.Pos(bone.Position);
						qd.Pos(bone.Position + bone.RotationMatrix.Z);
					}
				}
			qd.End();

			/*qdMaterial.Enable();
			QuickDraw3ColorMaterial.Camera = camera.TransformMatrix;
			qdMaterial.Apply();
			qd.StartLines();
				foreach (var o in model.Objects)
				{
					var armObj = o as ObjectArmature;
					if (armObj == null) continue;

					var arm = armObj.Armature;
					foreach (var bone in arm.Bones)
					{
						//if (bone.Parent == null) continue;

						qd.Color(1, 0, 0, 1);
						qd.Pos(bone.Position);
						qd.Pos(bone.Position + bone.Rotation.X);

						qd.Color(0, 1, 0, 1);
						qd.Pos(bone.Position);
						qd.Pos(bone.Position + bone.Rotation.Y);

						qd.Color(0, 0, 1, 1);
						qd.Pos(bone.Position);
						qd.Pos(bone.Position + bone.Rotation.Z);
					}
				}
			qd.End();*/

			video.Present();
		}
	}
}
