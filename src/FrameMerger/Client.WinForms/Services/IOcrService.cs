using System.Collections.Generic;
using System.Drawing;

namespace Client.WinForms.Services
{
    public interface IOcrService
    {
        string? RunOcrAndBuildText(IEnumerable<Bitmap?>? images = null, string? tessdataPath = null);
    }
}

