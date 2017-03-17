using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Paloma;

namespace Project2
{
	// Contains and renders an indivuidual model
	class Model
	{
		// Information for entire model
		struct MD3Header
		{
			public string ID;		// "IDP3"
			public int version;		// 15
			public string file;		// 64 bytes
			public int flags;
			public int frameCount;
			public int tagCount;
			public int meshCount;
			public int skinCount;
			public int frameOffset;	// Start of frame data relative to beginning of model
			public int tagOffset;	// Start of tag data
			public int meshOffset;	// Start of mesh data
			public int fileSize;
		};

		// Each mesh has multiple keyframes used for animations
		public struct Frame
		{
			public Vector3 minimums;	// Bounding box minimum corner
			public Vector3 maximums;	// Maximum corner
			public Vector3 position;	// Origin, usually (0, 0, 0)
			public float scale;			// Radius of bounding sphere
			public string creator;		// 16 bytes
		};

		// Used to align separate models
		public struct Tag
		{
			public string name;
			public Vector3 position;
			public Matrix rotation;
		};

		// Information for a specific mesh or surface within model
		public struct MeshHeader
		{
			public string ID;				// 4 bytes
			public string name;				// 64 bytes
			public int flags;
			public int frameCount;
			public int skinCount;
			public int vertexCount;
			public int triangleCount;
			public int triangleOffset;		// Start of triangle data relative to start of mesh
			public int skinOffset;			// Start of skin data
			public int textureVectorOffset;	// Start of texture coordinate data
			public int vertexOffset;		// Start of vertex data
			public int meshSize;
		};

		// Piece of model - also called surface
		public struct Mesh
		{
			public MeshHeader header;
			public Skin[] skins;				// Skins used for this mesh
			public int[] triangleVertices;		// Offsets into vertices - 3 * number of triangles
			public Vector2[] textureCoordinates;// Coordinates in texture for each vertex
			public Vertex[] vertices;			// All vertices in mesh with normals
			public int texture;					// Index of texture used
		};

		// Made up of textures - also called shader
		public struct Skin
		{
			public string name;
			public int index;
		};

		// Vertex within a mesh
		public struct Vertex
		{
			public Vector3 vertex;	// Position
			public byte[] normal;	// Vertex normal encoded as index into precomputed normals
		};

		private static GraphicsDevice device;

		// All model data
		private MD3Header header;
		private Frame[] frames;
		private Tag[] tags;
		private Mesh[] meshes;
		private Model[] links;
		Texture2D[] textures;		// All textures used by this model

		static Vector3[,] normals;	// 65,536 precomputed normals

		// Animation information
		private int startFrame;
		private int endFrame;
		private int nextFrame;
		private int currentFrame;
		float interpolation = 0;

		// Properties
		public int StartFrame
		{
			set { startFrame = value; }
		}

		public int EndFrame
		{
			set { endFrame = value; }
		}

		public int NextFrame
		{
			set { nextFrame = value; }
		}

		public int CurrentFrame
		{
			set { currentFrame = value; }
		}

		public Model(string modelPath, string skinPath)
		{
			LoadModel(modelPath);
			LoadSkin(skinPath);
		}

		// Load a MD3 model
		private void LoadModel(string modelFileName)
		{
			// Open file
			BinaryReader reader = new BinaryReader(File.Open(modelFileName, FileMode.Open));

			int currentOffset = 0;

			// Header
			byte[] bytes = reader.ReadBytes(4);
			for (int j = 0; j < bytes.Length && bytes[j] != '\0'; ++j)
				header.ID += (char) bytes[j];

			header.version = reader.ReadInt32();

			bytes = reader.ReadBytes(64);
			for (int j = 0; j < bytes.Length && bytes[j] != '\0'; ++j)
				header.file += (char) bytes[j];

			header.flags = reader.ReadInt32();
			header.frameCount = reader.ReadInt32();
			header.tagCount = reader.ReadInt32();
			header.meshCount = reader.ReadInt32();
			header.skinCount = reader.ReadInt32();
			header.frameOffset = reader.ReadInt32();
			header.tagOffset = reader.ReadInt32();
			header.meshOffset = reader.ReadInt32();
			header.fileSize = reader.ReadInt32();

			// Memory Allocation
			frames = new Frame[header.frameCount];
			tags = new Tag[header.frameCount * header.tagCount];
			meshes = new Mesh[header.meshCount];
			links = new Model[tags.Length];

			// Frames
			reader.BaseStream.Seek(header.frameOffset, SeekOrigin.Begin);
			for (int i = 0; i < frames.Length; ++i)
			{
				frames[i].minimums = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				frames[i].maximums = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				frames[i].position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				frames[i].scale = reader.ReadSingle();

				bytes = reader.ReadBytes(16);
				for (int j = 0; j < bytes.Length && bytes[j] != '\0'; ++j)
					frames[i].creator += (char) bytes[j];
			}

			// Tags
			reader.BaseStream.Seek(header.tagOffset, SeekOrigin.Begin);
			for (int i = 0; i < tags.Length; ++i)
			{
				bytes = reader.ReadBytes(64);
				for (int j = 0; j < bytes.Length && bytes[j] != '\0'; ++j)
					tags[i].name += (char) bytes[j];

				tags[i].position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				tags[i].rotation = new Matrix(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
											  reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
											  reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
											  0, 0, 0, 1);
			}

			textures = new Texture2D[header.meshCount];

			// Meshes
			currentOffset = header.meshOffset;
			for (int i = 0; i < meshes.Length; ++i)
			{
				// Seek to the beginning of each new mesh
				reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);

				// Header
				bytes = reader.ReadBytes(4);
				for (int j = 0; j < bytes.Length && bytes[j] != '\0'; ++j)
					meshes[i].header.ID += (char)bytes[j];

				bytes = reader.ReadBytes(64);
				for (int j = 0; j < bytes.Length && bytes[j] != '\0'; ++j)
					meshes[i].header.name += (char)bytes[j];

				meshes[i].header.flags = reader.ReadInt32();
				meshes[i].header.frameCount = reader.ReadInt32();
				meshes[i].header.skinCount = reader.ReadInt32();
				meshes[i].header.vertexCount = reader.ReadInt32();
				meshes[i].header.triangleCount = reader.ReadInt32();
				meshes[i].header.triangleOffset = reader.ReadInt32();
				meshes[i].header.skinOffset = reader.ReadInt32();
				meshes[i].header.textureVectorOffset = reader.ReadInt32();
				meshes[i].header.vertexOffset = reader.ReadInt32();
				meshes[i].header.meshSize = reader.ReadInt32();

				// Skins
				reader.BaseStream.Seek(currentOffset + meshes[i].header.skinOffset, SeekOrigin.Begin);
				meshes[i].skins = new Skin[meshes[i].header.skinCount];
				for (int k = 0; k < meshes[i].skins.Length; ++k)
				{
					bytes = reader.ReadBytes(64);
					for (int j = 0; j < bytes.Length && bytes[j] != '\0'; ++j)
						meshes[i].skins[k].name += (char) bytes[j];

					meshes[i].skins[k].index = reader.ReadInt32();
				}

				// Triangles
				reader.BaseStream.Seek(currentOffset + meshes[i].header.triangleOffset, SeekOrigin.Begin);
				meshes[i].triangleVertices = new int[meshes[i].header.triangleCount * 3];
				for (int k = 0; k < meshes[i].triangleVertices.Length; ++k)
					meshes[i].triangleVertices[k] = reader.ReadInt32();

				// Texture coordinates
				reader.BaseStream.Seek(currentOffset + meshes[i].header.textureVectorOffset, SeekOrigin.Begin);
				meshes[i].textureCoordinates = new Vector2[meshes[i].header.vertexCount * meshes[i].header.frameCount];
				for (int k = 0; k < meshes[i].textureCoordinates.Length; ++k)
					meshes[i].textureCoordinates[k] = new Vector2(reader.ReadSingle(), reader.ReadSingle());

				// Vertices
				reader.BaseStream.Seek(currentOffset + meshes[i].header.vertexOffset, SeekOrigin.Begin);
				meshes[i].vertices = new Vertex[meshes[i].header.vertexCount * meshes[i].header.frameCount];
				for (int k = 0; k < meshes[i].vertices.Length; ++k)
				{
					meshes[i].vertices[k].vertex = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
					meshes[i].vertices[k].normal = new byte[2] { reader.ReadByte(), reader.ReadByte() };
				}

				// Texture starts uninitialized
				meshes[i].texture = -1;

				// Increase offset for next mesh
				currentOffset += meshes[i].header.meshSize;
			}

			reader.Close();
		}

		// Loads a skin
		private void LoadSkin(string skinFileName)
		{
			var lines = File.ReadLines(skinFileName);

			// Loop through all lines in the file
			int currentTexture = 0;
			foreach (var line in lines)
			{
				if (line != "" && !line.StartsWith("tag_"))
				{
					string meshName = line.Substring(0, line.IndexOf(','));
					string texturePath = line.Substring(line.IndexOf(',') + 1);

					// Find mesh with corresponding name
					int i;
					for (i = 0; meshes[i].header.name != meshName && i < meshes.Length; ++i);

					// Load texture from file name and add it to texture list
					if (i != meshes.Length)
					{
						textures[currentTexture] = LoadTexture(device, texturePath);
						meshes[i].texture = currentTexture;
						++currentTexture;
					}
					else
					{
						Console.WriteLine("aaaahh");
					}
				}
			}
		}

		// Reads textures from jpg, png, and tga files
		public static Texture2D LoadTexture(GraphicsDevice device, string texturePath)
		{
			Texture2D texture;

			if (texturePath.ToLower().EndsWith(".tga"))
			{
				TargaImage image = new TargaImage(texturePath);
				texture = new Texture2D(device, image.Header.Width, image.Header.Height);
				Color[] data = new Color[image.Header.Height * image.Header.Width];
				for (int y = 0; y < image.Header.Height; y++)
					for (int x = 0; x < image.Header.Width; x++)
					{
						System.Drawing.Color color = image.Image.GetPixel(x, y);
						data[y * image.Header.Width + x] = new Color(color.R, color.G, color.B, color.A);
					}
				image.Dispose();
				texture.SetData(data);
			}
			else
			{
				FileStream stream = new FileStream(texturePath, FileMode.Open);
				texture = Texture2D.FromStream(device, stream);
				stream.Close();
			}

			return texture;
		}

		// Changes interpolation amount based on time between keyframes
		public void UpdateFrame(float frameFraction)
		{
			interpolation += frameFraction;

			// Increment current and next frames
			if (interpolation > 1)
			{
				interpolation = 0;
				currentFrame = nextFrame;
				++nextFrame;

				// Looping
				if (nextFrame >= endFrame)
					nextFrame = startFrame;
			}
		}

		// Creates references to other models linked to this one by tags
        public void Link(string tagName, Model model)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (tagName.Equals(tags[i].name) )
                {
                    links[i] = model; 
                }
            }
        }

		// Draws this model and all those linked to it
        public void DrawAllModels(Matrix current, Matrix next, BasicEffect effect)
        {
			DrawModel(current, next, effect);
			
			Matrix m;
			Matrix mNext;
			for (int i = 0; i < header.tagCount; ++i)
			{
				if (links[i] != null)
				{
					int currentTag = currentFrame * header.tagCount + i;
					int nextTag = nextFrame * header.tagCount + i;

					m = tags[currentTag].rotation;
					mNext = tags[nextTag].rotation;

					m *= current * Matrix.CreateTranslation(tags[currentTag].position);
					mNext *= next * Matrix.CreateTranslation(tags[nextTag].position);

					links[i].DrawAllModels(m, mNext, effect);
				}
			}
		}

		// Renders this model to the screen
		public void DrawModel(Matrix current, Matrix next, BasicEffect effect)
		{
			// Loop through each mesh
			for (int i = 0; i < header.meshCount; ++i)
			{
				// Set appropriate texture
				if (meshes[i].texture != -1)
					effect.Texture = textures[meshes[i].texture];

				// Vertex array for current mesh
				VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[meshes[i].header.vertexCount];

				// Index of first vertex for both current and next frame
				int currentOffset = currentFrame * meshes[i].header.vertexCount;
				int nextOffset = nextFrame * meshes[i].header.vertexCount;

				// Loop through all vertices in mesh
				for (int j = 0; j < meshes[i].header.vertexCount; ++j)
				{
					// Find current and next vertex based on offset
					Vertex currentVertex = meshes[i].vertices[j + currentOffset];
					Vertex nextVertex = meshes[i].vertices[j + nextOffset];

					// Positions of current and next vertices
					Vector3 currentPosition = currentVertex.vertex;
					Vector3 nextPosition = nextVertex.vertex;

					// Transform positions
					currentPosition = Vector3.Transform(currentPosition, current);
					nextPosition = Vector3.Transform(nextPosition, next);

					// Get precomputed normals based on the two bytes of encoded normals
					Vector3 currentNormal = normals[currentVertex.normal[0], currentVertex.normal[1]];
					Vector3 nextNormal = normals[nextVertex.normal[0], nextVertex.normal[1]];

					// Transform normals
					currentNormal = Vector3.TransformNormal(currentNormal, current);
					nextNormal = Vector3.TransformNormal(nextNormal, next);

					// Interpolate between current and next positions and normals
					Vector3 interpolatedPosition = Vector3.Lerp(currentPosition, nextPosition, interpolation);
					Vector3 interpolatedNormal = Vector3.Lerp(currentNormal, nextNormal, interpolation);

					// Get texture coordinates for this vertex
					Vector2 textureCoordinate = meshes[i].textureCoordinates[j];

					// Add new vertex object to list
					vertices[j] = new VertexPositionNormalTexture(interpolatedPosition, interpolatedNormal, textureCoordinate);
				}

				// Create vertex buffer for current mesh with new vertices
				VertexBuffer vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), meshes[i].header.vertexCount, BufferUsage.WriteOnly);
				vertexBuffer.SetData(vertices);

				IndexBuffer indexBuffer = new IndexBuffer(device, typeof(int), meshes[i].header.triangleCount * 3, BufferUsage.WriteOnly);
				indexBuffer.SetData<int>(meshes[i].triangleVertices);

				device.SetVertexBuffer(vertexBuffer);
				device.Indices = indexBuffer;
				
				// Draw current mesh
				foreach (EffectPass pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshes[i].header.triangleCount);
				}
			}
		}

		// Sets up static members - GraphicsDevice and precomputed normals
        public static void SetUp(GraphicsDevice device)
        {
			Model.device = device;

			normals = new Vector3[256, 256];

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    float alpha = (float)(2.0 * i * Math.PI / 255);
                    float beta = (float)(2.0 * j * Math.PI / 255);
                    normals[i, j].X = (float)(Math.Cos(beta) * Math.Sin(alpha));
                    normals[i, j].Y = (float)(Math.Sin(beta) * Math.Sin(alpha));
                    normals[i, j].Z = (float)(Math.Cos(alpha));
                }
            }
        }
    }
}
