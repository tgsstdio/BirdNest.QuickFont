using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Text;

namespace DynamicStreaming
{
	public class MapPersistent : ISolution
	{
		private Matrix4 mProj;
		private int mWidth;
		private int mHeight;
		private int mSizeOfVec2;

		private uint kParticleCountX;
		private uint kParticleCountY;
		//private uint kParticleCount;
		//private uint kVertexCount;
		//private long kParticleBufferSize;
		private int kVertsPerParticle;

		public MapPersistent (int vertsPerParticles, int width, int height)
		{
			mBufferLockManager = new BufferLockManager (true);
			mSizeOfVec2 = System.Runtime.InteropServices.Marshal.SizeOf (typeof(Vector2));

			mWidth = width;
			mHeight = height;

			kParticleCountX = 500;
			kParticleCountY = 320;
			kVertsPerParticle = vertsPerParticles;
			//kParticleCount = (kParticleCountX * kParticleCountY);
			//kVertexCount = (uint) (kParticleCount * kVertsPerParticle);
		}

		public string GetProblemName() { return "DynamicStreaming"; }

		public bool SupportsApi ()
		{
			throw new NotImplementedException ();
		}

		public void SetSize (int width, int height)
		{
			mWidth = width;
			mHeight = height;
			mProj = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians(45.0f), (float)width / (float)height, 0.1f, 10000.0f);
		}

		private int CreateProgram<T>(string _vsFilename, string _psFilename, string[] _uniforms, T[] _outUniformLocationStruct)
		{
			return CreateProgram<T>(_vsFilename, _psFilename, "", _uniforms, _outUniformLocationStruct);
		}

		private int CreateProgram<T>(string _vsFilename, string _psFilename, string _shaderPrefix, string[] _uniforms, T[] _outUniformLocationStruct)
		{
			int uniformCount = _uniforms.Length;

			// Ensure that the sizes match, otherwise there is a parameter mismatch.
//			assert(uniformCount == (sizeof(T) / sizeof(GLuint))
//				&& (sizeof(T) % sizeof(GLuint) == 0));

			return CreateProgram(_vsFilename, _psFilename, _shaderPrefix, _uniforms, _outUniformLocationStruct);
		}

		private int CreateProgram(string _vsFilename, string _psFilename)
		{
			return CreateProgram(_vsFilename, _psFilename, "");
		}

		// --------------------------------------------------------------------------------------------------------------------
		private int CreateProgram(string _vsFilename,string _psFilename, string _shaderPrefix)
		{
			int vs = CompileShaderFromFile(ShaderType.VertexShader, _vsFilename, _shaderPrefix),
			fs = CompileShaderFromFile(ShaderType.FragmentShader, _psFilename, _shaderPrefix);

			// If any are 0, dump out early.
			if ((vs * fs) == 0) {
				return 0;
			}

			int retProgram = LinkShaders(vs, fs);

			// Flag these now, they're either attached (linked in) and will be cleaned up with the link, or the
			// link failed and we're about to lose track of them anyways.
			GL.DeleteShader(fs);
			GL.DeleteShader(vs);

			return retProgram;
		}

		private int LinkShaders(int _vs, int _fs)
		{
			int retVal = GL.CreateProgram ();
		    GL.AttachShader(retVal, _vs);
		    GL.AttachShader(retVal, _fs);
		    GL.LinkProgram(retVal);

		    int linkStatus = 0;
			GL.GetProgram(retVal,GetProgramParameterName.LinkStatus, out linkStatus);

		    int glinfoLogLength = 0;
			GL.GetProgram(retVal, GetProgramParameterName.InfoLogLength, out glinfoLogLength);

		    if (glinfoLogLength > 1) {
		        string buffer = GL.GetProgramInfoLog(retVal);
				if (linkStatus != (int) All.True) {
					Console.WriteLine("Shader Linking failed with the following errors:");
		        }
		        else {
					Console.WriteLine("Shader Linking succeeded, with following warnings/messages:\n");
		        }

				Console.WriteLine(buffer);
		    }

			if (linkStatus != (int) All.True) {
//		        #ifndef POSIX
//		            assert(!"Shader failed linking, here's an assert to break you in the debugger.");
//		        #endif
		        GL.DeleteProgram(retVal);
		        retVal = 0;
		    }

		    return retVal;
		}

		private int CompileShaderFromFile(ShaderType _shaderType, string _shaderFilename, string _shaderPrefix)
		{
			string shaderFullPath = Path.Combine("Shaders/glsl/", _shaderFilename);

			int retVal = GL.CreateShader(_shaderType);
			string fileContents = File.ReadAllText(shaderFullPath);
			//string includePath = ".";

			// GLSL has this annoying feature that the #version directive must appear first. But we 
			// want to inject some #define shenanigans into the shader. 
			// So to do that, we need to split for the part of the shader up to the end of the #version line,
			// and everything after that. We can then inject our defines right there.
			var strTuple = versionSplit(fileContents);
			string versionStr = strTuple.Item1;
			string shaderContents = strTuple.Item2;

			var builder = new StringBuilder ();
			builder.AppendLine (versionStr);
			builder.AppendLine (_shaderPrefix);
			builder.Append (shaderContents);

			GL.ShaderSource(retVal, builder.ToString());
			GL.CompileShader(retVal);

			int compileStatus = 0;
			GL.GetShader(retVal, ShaderParameter.CompileStatus, out compileStatus);

			int glinfoLogLength = 0;
			GL.GetShader(retVal, ShaderParameter.InfoLogLength, out glinfoLogLength);
			if (glinfoLogLength > 1) {
				string buffer = GL.GetShaderInfoLog(retVal);
				if (compileStatus != (int) All.True) {
					Console.WriteLine("Shader Compilation failed for shader '{0}', with the following errors:", _shaderFilename);
				} else {
					Console.WriteLine("Shader Compilation succeeded for shader '{0}', with the following log:", _shaderFilename);
				}

				Console.WriteLine(buffer);
			}

			if (compileStatus != (int) All.True) 
			{
				GL.DeleteShader(retVal);
				retVal = 0;
			}

			return retVal;
		}

		private Tuple<string, string> versionSplit(string _srcString)
		{
			int StringLen = _srcString.Length;
			int substrStartPos = 0;
			int eolPos = 0;
			for (eolPos = substrStartPos; eolPos < StringLen; ++eolPos) {
				if (_srcString[eolPos] != '\n') {
					continue;
				}

				if (matchVersionLine(_srcString, substrStartPos, eolPos + 1)) {
					return strsplit(_srcString, eolPos + 1);
				}

				substrStartPos = eolPos + 1;
			}

			// Could be on the last line (not really--the shader will be invalid--but we'll do it anyways)
			if (matchVersionLine(_srcString, substrStartPos, StringLen)) {
				return strsplit(_srcString, eolPos + 1);
			}

			return new Tuple<string, string>("", _srcString);
		}

		private Tuple<string, string> strsplit(string _srcString, int _splitEndPos)
		{
			return new Tuple<string, string>(
				_srcString.Substring(0, _splitEndPos),
				_srcString.Substring(_splitEndPos));
		}

		private bool matchVersionLine(string _srcString, int _startPos, int _endPos)
		{
			int checkPos = _startPos;
			//Assert(_endPos <= _srcString.Length);

			// GCC doesn't support regexps yet, so we're doing a hand-coded look for 
			// ^\s*#\s*version\s+\d+\s*$
			// Annoying!

			// ^ was handled by the caller.

			// \s*
			while (checkPos < _endPos && (_srcString[checkPos] == ' ' || _srcString[checkPos] == '\t')) {
				++checkPos;
			}

			if (checkPos == _endPos) {
				return false;
			}

			// #
			if (_srcString[checkPos] == '#') {
				++checkPos;        
			} else {
				return false;
			}

			if (checkPos == _endPos) {
				return false;
			}

			// \s*
			while (checkPos < _endPos && (_srcString[checkPos] == ' ' || _srcString[checkPos] == '\t')) {
				++checkPos;
			}

			if (checkPos == _endPos) {
				return false;
			}

			// version
			const string kSearchString = "version";
			int kSearchStringLen = kSearchString.Length;

			if (checkPos + kSearchStringLen >= _endPos) {
				return false;
			}

			if (string.Compare(kSearchString, 0, _srcString, checkPos, kSearchStringLen) == 0) {
				checkPos += kSearchStringLen;
			} else {
				return false;
			}

			// \s+ (as \s\s*)
			if (_srcString[checkPos] == ' ' || _srcString[checkPos] == '\t') {
				++checkPos;
			} else {
				return false;
			}

			while (checkPos < _endPos && (_srcString[checkPos] == ' ' || _srcString[checkPos] == '\t')) {
				++checkPos;
			}

			if (checkPos == _endPos) {
				return false;
			}

			// \d+ (as \d\d*)
			if (_srcString[checkPos] >= '0' && _srcString[checkPos] <= '9') {
				++checkPos;
			} else {
				return false;
			}

			// Check the version number
			while (checkPos < _endPos && (_srcString[checkPos] >= '0' && _srcString[checkPos] <= '9')) {
				++checkPos;
			}

			while (checkPos < _endPos && (_srcString[checkPos] == ' ' || _srcString[checkPos] == '\t')) {
				++checkPos;
			}

			while (checkPos < _endPos && (_srcString[checkPos] == '\r' || _srcString[checkPos] == '\n')) {
				++checkPos;
			}

			// NOTE that if the string terminates here we're successful (unlike above)
			if (checkPos == _endPos) {
				return true;
			}

			return false;
		}

		// --------------------------------------------------------------------------------------------------------------------
		private int CreateProgram(string _vsFilename, string _psFilename, string[] _uniformNames, ref int[] _outUniformLocations)
		{
			return CreateProgram(_vsFilename, _psFilename, "", _uniformNames, _outUniformLocations);
		}

		// --------------------------------------------------------------------------------------------------------------------
		private int CreateProgram(string _vsFilename, string _psFilename, string _shaderPrefix, string[] _uniformNames, ref int[] _outUniformLocations)
		{
			int retProg = CreateProgram(_vsFilename, _psFilename, _shaderPrefix);
			if (retProg != 0) {
				for (int i = 0; i < _uniformNames.Length; ++i) {
					_outUniformLocations[i] = GL.GetUniformLocation(retProg, _uniformNames[i]);
				}
			}

			return retProg;
		}

		public bool Init(uint _maxVertexCount)
		{
			const uint K_TRIPLE_BUFFER = 3;

//			if (glBufferStorage == nullptr) {
//				console::warn("Unable to initialize solution '%s', glBufferStorage() unavailable.", GetName().c_str());
//				return false;
//			}

			// Uniform Buffer
			mUniformBuffer = GL.GenBuffer();

			// Program
			string[] kUniformNames = { "CB0"};

			mProgram = CreateProgram("streaming_vb_gl_vs.glsl",
				"streaming_vb_gl_fs.glsl",
				kUniformNames, 
				ref mUniformLocation);

			if (mProgram == 0) {
				Console.Write("Unable to initialize solution '%s', shader compilation/linking failed.", GetName());
				return false;
			}

			// Dynamic vertex buffer
			mVertexBuffer = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBuffer);

			mParticleBufferSize = (K_TRIPLE_BUFFER * System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector2)) * _maxVertexCount);
			IntPtr bufferSize = (IntPtr) mParticleBufferSize;
			BufferStorageFlags flags = BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit;
			GL.BufferStorage(BufferTarget.ArrayBuffer, bufferSize, (IntPtr) 0, flags);

			BufferAccessMask rangeFlags = BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit;
			mVertexDataPtr = GL.MapBufferRange(BufferTarget.ArrayBuffer, (IntPtr) 0, bufferSize, rangeFlags);

			mVAO = GL.GenVertexArray();
			GL.BindVertexArray(mVAO);

			return GL.GetError() == ErrorCode.NoError;
		}

		public void Render(Vector2[] vertices)
		{
			// Program
			GL.UseProgram(mProgram);

			// Uniforms
			Constants cb = new Constants ();
			cb.width = 2.0f / mWidth;
			cb.height = -2.0f / mHeight;

			GL.BindBuffer(BufferTarget.UniformBuffer, mUniformBuffer);
			GL.BufferData(BufferTarget.UniformBuffer, (IntPtr) System.Runtime.InteropServices.Marshal.SizeOf(typeof(Constants)), ref cb, BufferUsageHint.DynamicDraw);
			GL.BindBufferBase(BufferTarget.UniformBuffer, 0, mUniformBuffer);

			// Input Layout
			GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBuffer);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, mSizeOfVec2, System.Runtime.InteropServices.Marshal.OffsetOf(typeof(Vector2), "x"));
			GL.EnableVertexAttribArray(0);

			// Rasterizer State
			GL.Disable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Front);
			GL.Disable(EnableCap.ScissorTest);
			GL.Viewport(0, 0, mWidth, mHeight);

			// Blend State
			GL.Disable(EnableCap.Blend);
			GL.ColorMask(true, true, true, true);

			// Depth Stencil State
			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);

			int kParticleCount = (int) (vertices.Length / kVertsPerParticle);
			int kParticleSizeBytes = (int) (kVertsPerParticle * mSizeOfVec2);
			int kStartIndex = (int) (mStartDestOffset / mSizeOfVec2);

			// Need to wait for this area to become available. If we've sized things properly, it will always be 
			// available right away.
			mBufferLockManager.WaitForLockedRange((int) mStartDestOffset, (int) (vertices.Length * mSizeOfVec2));

			for (int i = 0; i < kParticleCount; ++i)
			{
				int vertexOffset = i * kVertsPerParticle;
				long srcOffset = vertexOffset;
				int dstOffset = mStartDestOffset + (i * kParticleSizeBytes);

				// TODO : copy structs
				IntPtr dst = IntPtr.Add (mVertexDataPtr, dstOffset);

				float [] source = new float[1];
				System.Runtime.InteropServices.Marshal.Copy (source, 0, dst, kParticleSizeBytes);
				//memcpy(dst, vertices[vertexOffset], kParticleSizeBytes);

				GL.DrawArrays(PrimitiveType.Triangles, kStartIndex + vertexOffset, kVertsPerParticle);
			}

			// Lock this area for the future.
			mBufferLockManager.LockRange(mStartDestOffset, (int) (vertices.Length * mSizeOfVec2));

			mStartDestOffset = (int)( (mStartDestOffset + (kParticleCount * kParticleSizeBytes)) % mParticleBufferSize);
		}

		public void ShutDown()
		{
			GL.DisableVertexAttribArray(0);
			GL.DeleteVertexArray(mVAO);

			GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBuffer);
			GL.UnmapBuffer(BufferTarget.ArrayBuffer);
			GL.DeleteBuffer(mVertexBuffer);

			GL.DeleteBuffer(mUniformBuffer);
			GL.DeleteProgram(mProgram);
		}

		public string GetName() 
		{ 
			return "GLMapPersistent"; 		
		}

		private struct Constants
		{
			public float width;
			public float height;
			public float pad0;
			public float pad1;
		}

		private int mUniformBuffer;
		private int mVertexBuffer;
		private int mProgram;
		private int mVAO;

		private int mStartDestOffset;
		private long mParticleBufferSize;

		private BufferLockManager mBufferLockManager;

		private int[] mUniformLocation;
		private IntPtr mVertexDataPtr;
	//	private BufferLock mBufferLock;
	}
}

