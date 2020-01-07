// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Globalization;
using System.IO;

namespace Buffalo.Main
{
	sealed class HexStream : Stream
	{
		public HexStream(TextWriter writer)
		{
			_writer = writer;
			_buffer = new char[65];

			for (var i = 0; i < _buffer.Length; i++)
			{
				_buffer[i] = ' ';
			}
		}

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;

		public override void Flush()
		{
			PushLine();
			_writer.Flush();
		}

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
			=> throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			for (var n = 0; n < count; n++)
			{
				WriteChar(buffer[offset + n]);

				if ((_position & LineMask) == 0)
				{
					PushLine();
				}
			}
		}

		const int PositionMask = unchecked((int)0xFFFFF0);
		const int LineMask = 0x0F;

		void WriteChar(byte b)
		{
			var linePos = _position & LineMask;
			var n = (linePos * 3) + 1;

			_buffer[n] = List[b >> 4];
			_buffer[n + 1] = List[0x0F & b];
			_buffer[49 + linePos] = b < 32 || b >= 127 ? '.' : (char)b;

			_position++;
			_lineDirty = true;
		}

		void PushLine()
		{
			if (_lineDirty)
			{
				_lineDirty = false;

				_writer.Write(((_position - 1) & PositionMask).ToString("X8", CultureInfo.InvariantCulture));
				_writer.Write(_buffer);
				_writer.WriteLine();

				for (var i = 0; i < _buffer.Length; i++)
				{
					_buffer[i] = ' ';
				}
			}
		}

		int _position;
		bool _lineDirty;
		readonly char[] _buffer;
		readonly TextWriter _writer;

		const string List = "0123456789ABCDEF";
	}
}
