using System.Net.Mail;
using CxManager.Domain.Exceptions;

namespace CxManager.Domain.Validation;

public static class DomainValidation
{
    public static string ValidateRequiredString(
        string? value,
        string fieldName,
        int minLength,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value: value))
            throw new ValidationException(message: $"O campo '{fieldName}' é obrigatório.");

        string trimmed = value.Trim();

        if (trimmed.Length < minLength || trimmed.Length > maxLength)
            throw new ValidationException(
                message: $"O campo '{fieldName}' deve ter entre {minLength} e {maxLength} caracteres.");

        return trimmed;
    }

    public static string? ValidateOptionalString(
        string? value,
        string fieldName,
        int minLength,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value: value))
            return null;

        string trimmed = value.Trim();

        if (trimmed.Length < minLength || trimmed.Length > maxLength)
            throw new ValidationException(
                message: $"O campo '{fieldName}' deve ter entre {minLength} e {maxLength} caracteres.");

        return trimmed;
    }

    public static long ValidatePositiveId(long value, string fieldName)
    {
        if (value <= 0)
            throw new ValidationException(message: $"O campo '{fieldName}' deve conter um identificador positivo.");

        return value;
    }

    public static string ValidateEmail(
        string? value,
        int minLength = 5,
        int maxLength = 150)
    {
        string trimmed = ValidateRequiredString(
            value: value,
            fieldName: "e-mail",
            minLength: minLength,
            maxLength: maxLength);

        if (!MailAddress.TryCreate(address: trimmed, result: out MailAddress? mailAddress) ||
            mailAddress.Address != trimmed)
        {
            throw new ValidationException(message: "O e-mail informado possui formato inválido.");
        }

        return trimmed;
    }
}
