using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Serializer
{
    public class Program
    {

        public class StringStream : Stream
        {
            private readonly MemoryStream memStream;
            public StringStream(string text) =>
                memStream = new MemoryStream(Encoding.UTF8.GetBytes(text));

            public StringStream() => memStream = new MemoryStream();
            public StringStream(int capacity) => memStream = new MemoryStream(capacity);

            public override void Flush() => memStream.Flush();

            public override int Read(byte[] buffer, int offset, int count) =>
                memStream.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) =>
                 memStream.Seek(offset, origin);

            public override void SetLength(long value) => memStream.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => memStream.Write(buffer, offset, count);

            public override bool CanRead => memStream.CanRead;
            public override bool CanSeek => memStream.CanSeek;
            public override bool CanWrite => memStream.CanWrite;
            public override long Length => memStream.Length;
            public override long Position
            {
                get => memStream.Position;
                set => memStream.Position = value;
            }
            public override string ToString() => Encoding.UTF8.GetString(memStream.GetBuffer(), 0, (int)memStream.Length);
            public override int ReadByte() => memStream.ReadByte();
            public override void WriteByte(byte value) => memStream.WriteByte(value);
        }
        public class Text
        {

            public class Gay
            {
                public int V;

                public string T;


                public Gay()
                {

                }
                public Gay(string t, int v)
                {
                    T = t;
                    V = v;
                }
            }



            public Gay gay = new("3.14", 18);

            public Gay gay2 = new("6,13", 12);
            public Text(int I, int J, string K)
            {
                i = I;
                j = J;
                k = K;
            }
            public Text()
            {

            }
            public int i;
            public int j;
            public string k;


            public override string ToString()
            {
                return (i, j, k, (gay.V, gay.T)).ToString();
            }
        }


        static void Main(string[] args)
        {




            var orig = new Text[] { new(3, 3, "3"), new(5, 6, "7"), new(8, 3, "9") };
            var serialized = new JsonSerializer().Serialize(orig);
            var deserialized = new JsonSerializer().Deserialize<Text[]>(serialized);

            foreach (var el in deserialized)
            {
                Console.WriteLine(el);
            }
          



            
            /*

            DataContractJsonSerializer formatter3 = new DataContractJsonSerializer(typeof(Text[]));
            using var sw = new StringStream();
            formatter3.WriteObject(sw, new Text[] { new(3, 3, "3"), new(5, 6, "7"), new(8, 3, "9") });
            Console.WriteLine(sw.ToString());




            string res = new JsonSerializer().Serialize(new Text[] { new(3, 3, "3"), new(5, 6, "7"), new(8, 3, "9") });
            Console.WriteLine(new JsonSerializer().Deserialize(res, typeof(Text[])).ToString());

         */
        }
    }
}
