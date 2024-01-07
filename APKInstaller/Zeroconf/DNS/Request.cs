using System.Collections.Generic;

namespace Zeroconf.DNS
{
    internal class Request
    {
        public Header header;
        private readonly List<Question> questions;

        public Request()
        {
            header = new Header
            {
                OPCODE = OPCode.Query,
                QDCOUNT = 0
            };

            questions = [];
        }

        public void AddQuestion(Question question)
        {
            questions.Add(question);
        }

        public byte[] Data
        {
            get
            {
                List<byte> data = [];
                header.QDCOUNT = (ushort)questions.Count;
                data.AddRange(header.Data);
                foreach (Question q in questions)
                {
                    data.AddRange(q.Data);
                }

                return data.ToArray();
            }
        }
    }
}
