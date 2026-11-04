namespace Crucible.Core.State
{
    /// <summary>
    /// Ghi giá trị nguyên thủy dạng nhị phân chuẩn hóa (little-endian, độ dài cố định) thẳng vào FNV-1a 64 —
    /// không cấp phát buffer.
    /// </summary>
    public sealed class CanonicalWriter
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        private ulong _hash = OffsetBasis;

        public ulong Hash => _hash;

        public void Write(bool value) => WriteByte(value ? (byte)1 : (byte)0);

        public void Write(int value) => WriteBytes(unchecked((uint)value), 4);

        public void Write(long value) => WriteBytes(unchecked((ulong)value), 8);

        public void Write(ulong value) => WriteBytes(value, 8);

        /// <summary>Độ dài (-1 nếu null) rồi từng ký tự UTF-16.</summary>
        public void Write(string value)
        {
            if (value == null)
            {
                Write(-1);
                return;
            }

            Write(value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteBytes(value[i], 2);
        }

        private void WriteBytes(ulong value, int byteCount)
        {
            for (var i = 0; i < byteCount; i++)
                WriteByte((byte)(value >> (8 * i)));
        }

        private void WriteByte(byte value)
        {
            unchecked
            {
                _hash ^= value;
                _hash *= Prime;
            }
        }
    }
}
