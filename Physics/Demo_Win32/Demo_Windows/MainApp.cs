﻿using System;
using Reign.Core;
using Reign.Video;
using Reign.Video.API;
//using Reign.Physics;
using ShaderMaterials.Shaders;
using System.Collections.Generic;

using Jitter;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
using Jitter.Dynamics.Joints;
using System.Reflection;
using System.Diagnostics;
using Reign.Physics;

namespace Demo
{
	#if WIN32
	class MainApp : Window
	#elif METRO || XNA
	class MainApp : Application
	#endif
	{
		bool loaded;
		RootDisposable root;
		VideoI video;
		QuickDrawI qd;
		ViewPortI viewPort;
		Camera camera;
		QuickDraw3ColorMaterial material;
		Texture2DI texture;

		RasterizerStateI rasterizerState;
		SamplerStateI samplerState;
		BlendStateI blendState;
		DepthStencilStateI depthStencilState;

		Model sphereModel, capsuleModel, boxModel, monkeyModel;
		CollisionSystem collisionSystem;
		World world;
		RigidBody[] spheres;
		RigidBody floorBox, monkey;
		TriangleMesh triangleMesh;
		
		public MainApp()
		#if WIN32
		: base("QuickDraw Sample", 512, 512, WindowStartPositions.CenterCurrentScreen, WindowTypes.FrameSizable)
		#elif METRO
		: base(ApplicationOrientations.Landscape)
		#elif XNA
		: base(512, 512)
		#endif
		{
			
		}

		protected override void shown()
		{
			try
			{
				root = new RootDisposable();

				// video objects
				VideoTypes videoType;
				#if METRO
				VideoTypes createVideoTypes = VideoTypes.D3D11;
				#elif WIN32
				VideoTypes createVideoTypes = VideoTypes.D3D11 | VideoTypes.D3D9 | VideoTypes.OpenGL;
				#elif XNA
				VideoTypes createVideoTypes = VideoTypes.XNA;
				#endif

				#if WIN32 || METRO
				video = Video.Init(createVideoTypes, out videoType, root, this, true);
				#elif XNA
				video = Video.Create(createVideoTypes, out videoType, root, this);
				#endif

				// shaders
				DiffuseTextureMaterial.Init(video, "Data/", video.FileTag, ShaderVersions.Max, null);
				DiffuseTextureMaterial.ApplyInstanceConstantsCallback = applyDiffuseCallbackMethod;
				QuickDraw3ColorMaterial.Init(video, "Data/", video.FileTag, ShaderVersions.Max, null);
				material = new QuickDraw3ColorMaterial();
				texture = Texture2DAPI.New(video, "Data/Rocks.dds", null);
				qd = QuickDrawAPI.New(video, QuickDraw3ColorMaterial.BufferLayoutDesc);

				var frame = FrameSize;
				viewPort = ViewPortAPI.New(video, 0, 0, frame.Width, frame.Height);
				float camDis = 50;
				camera = new Camera(viewPort, new Vector3(0, 5, camDis), new Vector3(), new Vector3(0, 5+1, camDis));

				// states
				rasterizerState = RasterizerStateAPI.New(video, RasterizerStateDescAPI.New(RasterizerStateTypes.Solid_CullCW));
				samplerState = SamplerStateAPI.New(video, SamplerStateDescAPI.New(SamplerStateTypes.Linear_Wrap));
				blendState = BlendStateAPI.New(video, BlendStateDescAPI.New(BlendStateTypes.None));
				depthStencilState = DepthStencilStateAPI.New(video, DepthStencilStateDescAPI.New(DepthStencilStateTypes.ReadWrite_Less));

				// models
				var materialTypes = new Dictionary<string,Type>();
				materialTypes.Add("Material", typeof(DiffuseTextureMaterial));

				var materialFieldTypes = new List<MaterialFieldBinder>();
				materialFieldTypes.Add(new MaterialFieldBinder("Material", "Paint_dds", "Diffuse"));

				var extOverrides = new Dictionary<string,string>();
				var emptyBinders = new List<MaterialFieldBinder>();
				sphereModel = new Model(video, "Data/sphere.rm", "Data/", materialTypes, emptyBinders, emptyBinders, emptyBinders, emptyBinders, materialFieldTypes, extOverrides, 0, null);
				//capsuleModel = Model.Create(videoType, video, "Data/capsule.rm", "Data/", materialTypes, emptyBinders, emptyBinders, emptyBinders, emptyBinders, materialFieldTypes, extOverrides);
				boxModel = new Model(video, "Data/box.rm", "Data/", materialTypes, emptyBinders, emptyBinders, emptyBinders, emptyBinders, materialFieldTypes, extOverrides, 0, null);
				monkeyModel = new Model(video, "Data/monkeyFlat.rm", "Data/", materialTypes, emptyBinders, emptyBinders, emptyBinders, emptyBinders, materialFieldTypes, extOverrides, 0, null);
				
				// physics
				collisionSystem = new CollisionSystemPersistentSAP();
				world = new World(collisionSystem);
				world.Gravity = new JVector(0, -9.81f, 0);
				
				spheres = new RigidBody[35];
				for (int i = 0; i != spheres.Length; ++i)
				{
					spheres[i] = new RigidBody(new SphereShape(1));
					spheres[i].Position = new JVector(spheres.Length/(float)(i+1), (i*3)+5, 0);
					world.AddBody(spheres[i]);
				}
				floorBox = new RigidBody(new BoxShape(30, 1, 30));
				floorBox.Shape.TransformScale = new Vector3(30, 1, 30) * .5f;
				floorBox.Position = new JVector(0, -7, 0);
				floorBox.IsStatic = true;
				//floorBox.Orientation = JMatrix.CreateFromYawPitchRoll(0, -.25f, 0);
				world.AddBody(floorBox);
				triangleMesh = new TriangleMesh("Data/monkeyFlat.rtmm", loadTiangleMesh);

				loaded = true;
			}
			catch (Exception e)
			{
				Message.Show("Error", e.Message);
				dispose();
			}
		}
		
		private void loadTiangleMesh(object sender, bool succeeded)
		{
			var mesh = (TriangleMesh)sender;

		    mesh.Scale(5);
		    monkey = new RigidBody(new TriangleMeshShape(new Octree(mesh)));
			monkey.Shape.TransformScale = new Vector3(5);
		    //monkey.IsStatic = true;
		    world.AddBody(monkey);
		}

		private void dispose()
		{
			if (world != null)
			{
				world.Clear();
				world = null;
			}
			if (root != null)
			{
				root.Dispose();
				root = null;
			}
		}

		protected override void closing()
		{
			dispose();
		}

		protected override void update(Time time)
		{
			if (!loaded) return;

            world.Step(time.Delta, true);
			camera.RotateAroundLookLocationWorld(0, 1 * time.Delta, 0);
		}

		int nextSphere;
		int renderMode;
		private void applyDiffuseCallbackMethod(DiffuseTextureMaterial material, Mesh mesh)
		{
			if (renderMode == 0)
			{
				material.Transform = spheres[nextSphere].Transform;
				++nextSphere;
			}
			else if (renderMode == 1)
			{
				material.Transform = floorBox.Transform;
			}
			else if (renderMode == 2)
			{
				material.Transform = monkey.Transform;
			}
		}

		protected override void render(Time time)
		{
			if (!loaded) return;

			var e = Loader.UpdateLoad();
			if (e != null)
			{
				Message.Show("Error", e.Message);
				dispose();
				loaded = false;
			}
			if (Loader.ItemsRemainingToLoad != 0) return;

			video.Update();
			video.EnableRenderTarget();
			rasterizerState.Enable();
			samplerState.Enable(0);
			blendState.Enable();
			depthStencilState.Enable();
			video.ClearColorDepth(0, .3f, .3f, 1);

			viewPort.Size = FrameSize;
			viewPort.Apply();
			camera.Apply();

			DiffuseTextureMaterial.Camera = camera.TransformMatrix;
			DiffuseTextureMaterial.LightDirection = -camera.Position.Normalize();
			DiffuseTextureMaterial.LightColor = new Vector4(1);
			DiffuseTextureMaterial.BufferLayout.Enable();

			// spheres
			renderMode = 0;
			var mesh = sphereModel.Meshes[0];
			mesh.VertexBuffer.Enable(mesh.IndexBuffer);
			nextSphere = 0;
			foreach (var sphere in spheres)
			{
			    mesh.Material.Apply(mesh);
			    mesh.VertexBuffer.Draw();
			}

			// floor box
			renderMode = 1;
			mesh = boxModel.Meshes[0];
			mesh.VertexBuffer.Enable(mesh.IndexBuffer);
			mesh.Material.Apply(mesh);
			mesh.VertexBuffer.Draw();

			// monkey
			renderMode = 2;
			mesh = monkeyModel.Meshes[0];
			mesh.VertexBuffer.Enable(mesh.IndexBuffer);
			mesh.Material.Apply(mesh);
			mesh.VertexBuffer.Draw();

			// ray cast
			/*var rayOrg =  new JVector(0, 5, 0);
			var rayDir = new JVector(.3f, -1, 0);
			JVector normal;
			float fraction;
			collisionSystem.Raycast(floorBox, rayOrg, rayDir, out normal, out fraction);
			QuickDraw3ColorMaterial.Camera = camera.TransformMatrix;
			material.Enable();
			material.Apply();
			qd.StartLines();
			    qd.Color(0, 1, 0, 1);
			    qd.Pos(rayOrg.X, rayOrg.Y, rayOrg.Z);
			    var rayDirScale = rayDir * 50;
			    qd.Pos(rayOrg.X+rayDirScale.X, rayOrg.Y+rayDirScale.Y, rayOrg.Z+rayDirScale.Z);

			    qd.Color(1, 0, 0, 1);
			    rayOrg = rayOrg + (rayDir * fraction);
			    qd.Pos(rayOrg.X, rayOrg.Y, rayOrg.Z);
			    normal *= 10;
			    qd.Pos(rayOrg.X+normal.X, rayOrg.Y+normal.Y, rayOrg.Z+normal.Z);
			qd.End();*/

			video.Present();
		}
	}
}
