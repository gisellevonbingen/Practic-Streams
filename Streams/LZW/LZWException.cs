using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Streams.LZW
{
    [Serializable]
    public class LZWException : Exception
    {
        public LZWException() { }
        public LZWException(string message) : base(message) { }
        public LZWException(string message, Exception inner) : base(message, inner) { }
        protected LZWException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }

}
