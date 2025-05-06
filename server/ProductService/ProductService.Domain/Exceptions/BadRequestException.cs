namespace ProductService.Domain.Exceptions;

public class BadRequestException(string message): Exception(message) { }