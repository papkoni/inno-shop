namespace ProductService.Domain.Exceptions;

public class NotFoundException(string message): Exception(message) { }