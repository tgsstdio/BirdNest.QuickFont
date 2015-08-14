namespace QuickFont
{
	public interface IQFontLoader
	{
		QFont Load (string path);
		QFont Load (string path, QFontLoaderConfiguration loaderConfig);
		QFont Load (string path, float downSampleFactor);
		QFont Load (string path, float downSampleFactor, QFontLoaderConfiguration loaderConfig);		
	}
}

