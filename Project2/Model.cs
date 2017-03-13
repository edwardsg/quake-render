using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;

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

		List textures;
		static Vector3[,] normals;

		// Load a MD3 model
		void LoadModel(string modelFileName)
		{
			// Open file
			BinaryReader reader = new BinaryReader(File.Open(modelFileName, FileMode.Open);

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
					frames[j].creator += (char) bytes[j];
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
	}
}
