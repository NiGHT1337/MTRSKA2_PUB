namespace Matroschka2.UI;

internal interface IProgressReporter
{
    void WriteProgressWithEta(long bytesProcessed, long totalBytes, TimeSpan elapsed);
}
