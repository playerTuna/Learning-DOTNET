using System;
using System.Runtime.Serialization;
namespace TodoApi.Exceptions;

public class BadRequestException : System.Exception {
    public BadRequestException() : base() { }
    public BadRequestException(string message) : base(message) { }
    public BadRequestException(string message, System.Exception inner) : base(message, inner) { }
    // protected BadRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}