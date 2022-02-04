using System;
using System.Runtime.Serialization;

namespace HP.Palette.Security.Exceptions {
    public class StorageException : ApplicationException {
        public StorageException()
            : base() { }
        public StorageException(string message)
            : base(message) { }
        public StorageException(string message, Exception innerException)
            : base(message, innerException) { }
        public StorageException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
