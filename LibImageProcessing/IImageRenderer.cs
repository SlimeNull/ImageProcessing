namespace LibImageProcessing
{
    public interface IImageRenderer : IDisposable
    {
        int OutputWidth { get; }
        int OutputHeight { get; }

        void Render(Span<byte> outputBgraData);
    }
}
