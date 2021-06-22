using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using BlazorProgressiveUpload.Client;
using BlazorProgressiveUpload.Client.Shared;
using System.IO;
using System.Net;
using System.Threading;

namespace BlazorProgressiveUpload.Client.Pages
{
    public partial class Index
    {

        [Inject]
        public HttpClient Client { get; set; }

        private Stream _fileStream = null;
        private string _selectedFileName = null;
        private long _uploaded = 0;
        private double _percentage = 0;
        public void OnChooseFile(InputFileChangeEventArgs e)
        {
            // Get the selected file   
            var file = e.File;

            // Check if the file is null then return from the method   
            if (file == null)
                return;

            // Validate the extension if requried (Client-Side)  

            // Set the value of the stream by calling OpenReadStream and pass the maximum number of bytes allowed because by default it only allows 512KB  
            // I used the value 5000000 which is about 50MB  
            using (var stream = file.OpenReadStream(50000000))
            {
                _fileStream = stream;
                _selectedFileName = file.Name;
            }
        }

        public async Task SubmitFormAsycn()
        {
            var content = new MultipartFormDataContent();
            var streamContent = new ProgressiveStreamContent(_fileStream, 100024, (u, p) =>
            {
                _uploaded = u;
                _percentage = p;
                StateHasChanged();
            });
            content.Add(streamContent, "File");

            var response = await Client.PostAsync("/weatherforecast", streamContent);

            if (response.IsSuccessStatusCode)
            {

            }
            else
            {

            }
        }


    }

    public class ProgressiveStreamContent : StreamContent
    {

        private readonly Stream _stream;
        private readonly int _maxBuffer = 1024 * 4; 

        public ProgressiveStreamContent(Stream stream, int maxBuffer, Action<int, double> onProgress) : base(stream)
        {
            _stream = stream;
            _maxBuffer = maxBuffer;
            OnProgress += onProgress;
        }

        public event Action<int, double> OnProgress;


        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = new byte[_maxBuffer];
            var totalLength = _stream.Length;
            long uploaded = 0;

            while (true)
            {
                using (_stream)
                {
                    var length = await _stream.ReadAsync(buffer, 0, _maxBuffer);
                    if (length <= 0)
                    {
                        break;
                    }

                    uploaded += length;
                    var perentage = Convert.ToDouble(uploaded * 100 / _stream.Length);
                    OnProgress?.Invoke((int)uploaded, perentage);
                    await Task.Delay(500);
                    await stream.WriteAsync(buffer);
                }
            }
        }

    }
}