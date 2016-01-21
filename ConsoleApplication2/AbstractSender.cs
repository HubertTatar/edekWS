using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleApplication2
{
    public abstract class AbstractSender
    {

        public abstract void send(Deklaracja deklaracja);

        public abstract string getUpo(Deklaracja deklaracja);

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private string SerializeObject(Object toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
            Utf8StringWriter textWriter = new Utf8StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }
    }
}

public sealed class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding { get { return Encoding.UTF8; } }
}

public sealed class StringWriterWithEncoding : StringWriter
{
    private readonly Encoding encoding;

    public StringWriterWithEncoding(Encoding encoding)
    {
        this.encoding = encoding;
    }

    public override Encoding Encoding
    {
        get { return encoding; }
    }
}
