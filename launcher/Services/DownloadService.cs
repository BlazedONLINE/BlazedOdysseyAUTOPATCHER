using BlazedOdysseyLauncher.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazedOdysseyLauncher.Services;

public class DownloadService : IDownloadService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ISecurityService _securityService;

    public event EventHandler<UpdateProgress>? ProgressChanged;

    public DownloadService(ISecurityService securityService)
    {
        _securityService = securityService;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("BlazedOdysseyLauncher/1.0");
        _httpClient.Timeout = TimeSpan.FromMinutes(30); // Long timeout for large files
    }

    public async Task<bool> DownloadFileAsync(string url, string destination, string expectedHash, 
        long expectedSize = 0, CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;
        const int retryDelayMs = 1000;

        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                if (retry > 0)
                {
                    await Task.Delay(retryDelayMs * retry, cancellationToken);
                }

                return await DownloadFileInternal(url, destination, expectedHash, expectedSize, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (retry < maxRetries - 1)
            {
                // Log retry attempt
                Console.WriteLine($"Download retry {retry + 1}/{maxRetries} for {url}: {ex.Message}");
                
                // Clean up partial file
                if (File.Exists(destination))
                {
                    try { File.Delete(destination); } catch { }
                }
            }
        }

        return false;
    }

    private async Task<bool> DownloadFileInternal(string url, string destination, string expectedHash, 
        long expectedSize, CancellationToken cancellationToken)
    {
        var tempFile = destination + ".tmp";
        long existingBytes = 0;
        
        // Support resume if partial file exists
        if (File.Exists(tempFile))
        {
            existingBytes = new FileInfo(tempFile).Length;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Add range header for resume
        if (existingBytes > 0)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingBytes, null);
        }

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? expectedSize;
        if (response.StatusCode == System.Net.HttpStatusCode.PartialContent)
        {
            totalBytes += existingBytes;
        }

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(tempFile, existingBytes > 0 ? FileMode.Append : FileMode.Create, 
            FileAccess.Write, FileShare.None, bufferSize: 81920);

        var buffer = new byte[81920]; // 80KB buffer
        long totalBytesRead = existingBytes;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            totalBytesRead += bytesRead;

            // Report progress
            ProgressChanged?.Invoke(this, new UpdateProgress
            {
                CurrentFile = Path.GetFileName(destination),
                CurrentFileProgress = totalBytesRead,
                CurrentFileSize = totalBytes,
                Status = $"Downloading {Path.GetFileName(destination)}..."
            });
        }

        await fileStream.FlushAsync(cancellationToken);
        fileStream.Close();

        // Verify file hash
        if (!string.IsNullOrEmpty(expectedHash))
        {
            var actualHash = await _securityService.CalculateFileHashAsync(tempFile);
            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(tempFile);
                throw new InvalidOperationException($"Hash verification failed for {destination}. Expected: {expectedHash}, Actual: {actualHash}");
            }
        }

        // Verify file size
        if (expectedSize > 0)
        {
            var actualSize = new FileInfo(tempFile).Length;
            if (actualSize != expectedSize)
            {
                File.Delete(tempFile);
                throw new InvalidOperationException($"Size verification failed for {destination}. Expected: {expectedSize}, Actual: {actualSize}");
            }
        }

        // Atomic move to final destination
        if (File.Exists(destination))
        {
            File.Delete(destination);
        }
        File.Move(tempFile, destination);

        return true;
    }

    public async Task<bool> DownloadManifestAsync(string url, string destination, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            await File.WriteAllBytesAsync(destination, content, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<byte[]> DownloadBytesAsync(string url, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}