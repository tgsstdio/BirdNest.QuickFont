namespace NextFont
{
	public interface INxFontLoader
	{
		NxFont Load (string path, float height, NxFontLoaderConfiguration loaderConfig);
		NxFont Load (string path, float height, float downSampleFactor, NxFontLoaderConfiguration loaderConfig);
	}
}

