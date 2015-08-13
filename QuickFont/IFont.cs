namespace QuickFont
{
	public interface IFont<TData> where TData : class ,new() 
	{
		void SetData(QFontData<TData> data);
	}
}

