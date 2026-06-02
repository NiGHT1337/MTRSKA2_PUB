namespace Matroschka2.UI;

internal sealed class ConsoleProgressReporter : IProgressReporter
{
    public void WriteProgressWithEta(long bytesProcessed, long totalBytes, TimeSpan elapsed)
    {
        if (totalBytes <= 0)
        {
            return;
        }

        int progress = (int)((double)bytesProcessed / totalBytes * 100);
        progress = Math.Clamp(progress, 0, 100);

        double speed = bytesProcessed / Math.Max(1, elapsed.TotalSeconds);
        double etaSeconds = (totalBytes - bytesProcessed) / Math.Max(1, speed);

        Console.CursorLeft = 0;
        Console.Write($"[{new string('=', progress / 2)}>{new string(' ', 50 - progress / 2)}] {progress}% | ETA: {TimeSpan.FromSeconds(etaSeconds):hh\\:mm\\:ss}");
    }
}
