using Blazored.LocalStorage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Todododo.Data
{
    public class DbStream : Stream
    {
        private readonly ILocalStorageService _localStorage;
        private readonly string _storageKey;
        private readonly MemoryStream _ms;

        public DbStream(ILocalStorageService localStorage, string storageKey, byte[] bytes = null)
        {
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _storageKey = !string.IsNullOrEmpty(storageKey) ? storageKey : throw new ArgumentNullException(nameof(storageKey));

            bytes ??= new byte[0];
            _ms = new MemoryStream();
            _ms.Write(bytes);
        }

        public override bool CanRead => _ms.CanRead;

        public override bool CanSeek => _ms.CanSeek;

        public override bool CanWrite => _ms.CanWrite;

        public override long Length => _ms.Length;

        public override long Position { get => _ms.Position; set => _ms.Position = value; }

        public override void Flush() => Task.Run(SaveOnStorage);

        public override async Task FlushAsync(System.Threading.CancellationToken cancellationToken) => await SaveOnStorage();

        private async Task SaveOnStorage() => await _localStorage.SetItemAsync(_storageKey, _ms.ToArray());

        public override int Read(byte[] buffer, int offset, int count) => _ms.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _ms.Seek(offset, origin);

        public override void SetLength(long value) => _ms.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _ms.Write(buffer, offset, count);

        public override void Close()
        {
            Flush();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
        }
    }

    public static class DbStreamExtensions
    {
        public const string Storagekey = "todododolitedb";

        public static async Task<DbStream> DbStream(this ILocalStorageService localStorage)
        {
            var data = await localStorage.GetItemAsync<byte[]>(Storagekey);

            return new DbStream(localStorage, Storagekey, data);
        }
    }
}
