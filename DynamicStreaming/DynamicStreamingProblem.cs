using System;
using OpenTK;
using System.Collections.Generic;

namespace DynamicStreaming
{
	public class DynamicStreamingProblem : IProblem
	{
		private int kVertsPerParticle = 6;
		private int kParticleCountX;
		private int kParticleCountY;

		public DynamicStreamingProblem (ISolution solution, int vertsPerParticle, int x, int y)
		{
			mActiveSolution = solution;
			kVertsPerParticle = vertsPerParticle;
			kParticleCountX = x;
			kParticleCountY = y;
		}

		private uint mIteration;
		private Vector2[] mVertexData;
		#region IProblem implementation

		public void Init ()
		{
			int kParticleCount = (kParticleCountX * kParticleCountY);
			int kVertexCount = kParticleCount * kVertsPerParticle;

			mVertexData = new Vector2[kVertexCount];
		}

		public void Render ()
		{
			this.Update ();

			MapPersistent solution = mActiveSolution as MapPersistent;

			if (solution != null)
			{
				solution.Render (mVertexData);
			}
		}

		private void Update()
		{
			const float spacing = 1.0f;
			const float w = 1.0f;
			const float h = 1.0f;

			const int kMarchPixelsX = 24;
			const int kMarchPixelsY = 128;

			float offsetX = (mIteration % kMarchPixelsX) * w;
			float offsetY = ((mIteration / kMarchPixelsX) % kMarchPixelsY) * h;

			int address = 0;
			for (int yPos = 0; yPos < kParticleCountY; ++yPos) {
				float y = spacing + yPos * (spacing + h);

				for (int xPos = 0; xPos < kParticleCountX; ++xPos) {
					float x = spacing + xPos * (spacing + w);

					Vector2[] verts = {
						new Vector2{X = x + offsetX + 0, Y = y + offsetY + 0 },
						new Vector2{X = x + offsetX + w, Y = y + offsetY + 0 },
						new Vector2{X = x + offsetX + 0, Y = y + offsetY + h },
						new Vector2{X = x + offsetX + w, Y = y + offsetY + 0 },
						new Vector2{X = x + offsetX + 0, Y = y + offsetY + h },
						new Vector2{X = x + offsetX + w, Y = y + offsetY + h },
					};

					mVertexData[address + 0] = verts[0];
					mVertexData[address + 1] = verts[1];
					mVertexData[address + 2] = verts[2];
					mVertexData[address + 3] = verts[3];
					mVertexData[address + 4] = verts[4];
					mVertexData[address + 5] = verts[5];

					address += kVertsPerParticle;
				}
			}

			++mIteration;
		}

		public void Shutdown ()
		{
			if (mActiveSolution != null)
			{
				mActiveSolution.ShutDown ();
			}
		}

		public string GetName ()
		{
			return "DynamicStreaming";
		}

		public bool SetSolution (ISolution solution)
		{
			throw new NotImplementedException ();
		}

		public void GetClearValues (out OpenTK.Vector4 outCol, out float outDepth)
		{
			var clearCol = new Vector4(0.3f, 0.0f, 0.3f, 1.0f );
			outCol = clearCol;
			outDepth = 1.0f;
		}

		public ISolution mActiveSolution {
			get;
			set;
		}

		#endregion
	}
}

