namespace UserService.Domain.Exceptions;

public class InvalidTokenException(string message) : Exception(message);