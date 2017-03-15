using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Paloma;

namespace Project2
{
	class Model
	{
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
			public int frameOffset;
			public int tagOffset;
			public int meshOffset;
			public int fileSize;
		};

		public struct Frame
		{
			public Vector3 minimums;
			public Vector3 maximums;
			public Vector3 position;
			public float scale;
			public string creator;		// 16 bytes
		};

		public struct Tag
		{
			public string name;
			public Vector3 position;
			public Matrix rotation;
		};

		public struct Skin
		{
			public string name;
			public int index;
		};

		public struct Vertex
		{
			public Vector3 vertex;
			public byte[] normal;
		};

		public struct MeshHeader
		{
			public string ID;	// 4 bytes
			public string name;	// 64 bytes
			public int flags;
			public int frameCount;
			public int skinCount;
			public int vertexCount;
			public int triangleCount;
			public int triangleOffset;
			public int skinOffset;
			public int textureVectorOffset;
			public int vertexOffset;
			public int meshSize;
		};

		public struct Mesh
		{
			public MeshHeader header;
			public Skin[] skins;
			public int[] triangleVertices;
			public Vector2[] textureCoordinates;
			public Vertex[] vertices;
			public int texture;
		};

		MD3Header header;
		Frame[] frames;
		Tag[] tags;
		Mesh[] meshes;
		Model[] links;
		GraphicsDevice device;

		int startFrame;
		int endFrame;
		int nextFrame;
		float interpolation;
		int currentFrame;

		Texture2D[] textures;
		static Vector3[,] normals;

		public Model(GraphicsDevice device, string modelPath, string skinPath)
		{
			this.device = device;
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

				textures = new Texture2D[meshes[i].header.skinCount];

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
				meshes[i].textureCoordinates = new Vector2[meshes[i].header.vertexCount];
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

        public static void SetUp()
        {
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
